using System.Collections.Generic;
using System.Linq;
using QuanticBrains.MotionMatching.Scripts.Components.PoseFinders.Implementations;
using QuanticBrains.MotionMatching.Scripts.Components.PoseSetters.Implementations;
using QuanticBrains.MotionMatching.Scripts.Components.Queries;
using QuanticBrains.MotionMatching.Scripts.Components.Queries.Implementations;
using QuanticBrains.MotionMatching.Scripts.Containers;
using QuanticBrains.MotionMatching.Scripts.Containers.CustomAvatars;
using QuanticBrains.MotionMatching.Scripts.Extensions;
using QuanticBrains.MotionMatching.Scripts.Helpers;
using QuanticBrains.MotionMatching.Scripts.Input.CharacterController;
using QuanticBrains.MotionMatching.Scripts.Tags;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace QuanticBrains.MotionMatching.Scripts.Components.QueryFlows.Implementations
{
    public class ActionQueryComputedFlow : QueryComputedFlow
    {
        protected ActionQueryComputed ActionQueryComputed;
        public ActionTagState CurrentState;

        public int CurrentAnimationPoseID;

        private float _timePerFrame;

        //Warp Vectors if dynamic
        private bool _hasPos;
        private bool _hasRot;

        protected List<AnimationData> WarpData; //Root positions+rotations, bones data
        private TargetProperties? _targetToWarp;
        
        public bool FirstFrame;
        
        //Physics and collisions
        private CharacterControllerBase _characterController;
        private bool[] _physicsEnabled;
        private bool[] _collisionsEnabled;
        
        private const ActionTagState WarpState = ActionTagState.InProgress; 

        public ActionQueryComputedFlow(
            Dataset dataset, 
            Transform root, 
            CurrentBoneTransformsValues currentBoneTransformsValues,
            TransformAccessArray characterTransformsNative,
            GlobalWeights globalWeights,
            int searchRate) : base(dataset, root, currentBoneTransformsValues, characterTransformsNative, globalWeights, searchRate)
        {
            PoseFinder  = new ActionPoseFinder();
            PoseSetter  = new MotionPoseSetter();
        }
        
        public override void Build(QueryComputed queryComputed, int length)
        {
            SetQueryComputed(queryComputed);
            InitializeFirstState();
            ManageWeights(length);
            InitializeDistanceResults();
        }

        protected override List<AnimationData> GetAnimationPoses()
        {
            if (!isQueryDone)
            {
                return CurrentState == WarpState && WarpData != null
                    ? WarpData : dataset.GetAnimationPoses(currentFeatureID, QueryComputed);
            } 
            return dataset.GetAnimationPoses(currentFeatureID, QueryComputed);
        }
        
        public override void Reset()
        {
            isQueryDone = false;
            CurrentAnimationPoseID = -1;
            isSearch = true;
        }
        
        //Initialize action
        public virtual void Initialize(TargetProperties? warpTarget, List<FeatureData> featuresData,
            List<List<AnimationData>> animationsData, CharacterControllerBase controller,
            CollisionsPhysicsSetup initSetup = CollisionsPhysicsSetup.Disabled,
            CollisionsPhysicsSetup inProgressSetup = CollisionsPhysicsSetup.Disabled,
            CollisionsPhysicsSetup recoverySetup = CollisionsPhysicsSetup.Disabled)
        {
            InitAnimationBaseIndexes();
            InitWarpingData(warpTarget, featuresData, animationsData, WarpState);
            FirstFrame = false;

            //Initialize collisions
            SetCollisionsAndPhysics(controller, new[] {initSetup, inProgressSetup, recoverySetup});
        }
        
        public virtual bool InitializeBy(ActionTagState state, float time, float loopTime,
            TargetProperties? warpTarget, List<FeatureData> featuresData,
            List<List<AnimationData>> animationsData, CharacterControllerBase controller,
            CollisionsPhysicsSetup initSetup = CollisionsPhysicsSetup.Disabled,
            CollisionsPhysicsSetup inProgressSetup = CollisionsPhysicsSetup.Disabled,
            CollisionsPhysicsSetup recoverySetup = CollisionsPhysicsSetup.Disabled)
        {
            switch (state)
            {
                //Check if state exists
                case ActionTagState.Init when !ActionQueryComputed.actionTag.HasInitState():
                    return false;
                case ActionTagState.Recovery when !ActionQueryComputed.actionTag.HasRecoveryState():
                    return false;
            }

            time = Mathf.Clamp01(time);
            
            // 1. Init animation indexes by current time ->
            InitAnimationIndexesByTime(state, time);
            
            // 2. Init warping data
            InitWarpingData(warpTarget, featuresData, animationsData, WarpState);
            FirstFrame = false;
            
            // 3. Initialize collisions
            SetCollisionsAndPhysics(controller, new[] {initSetup, inProgressSetup, recoverySetup});
            
            // 4. Compute movement
            // we don't need to blend nor inertialize, just apply displacement directly + localPos + scale + rotation from last frame
            InitializeFromPose(controller, state, time, 0, featuresData, animationsData);
            
            return true;
        }
        
        protected virtual void InitializeFromPose(CharacterControllerBase controller, ActionTagState state, float time, float loopTime,
            List<FeatureData> featuresData, List<List<AnimationData>> animationsData)
        {
            // - Init first state
            InitializeFirstState();

            bool hasFinished = false;
            // - Loop over action states until currently selected state
            while (!hasFinished)
            {
                var animID = featuresData[ActionQueryComputed.GetRanges()[(int)CurrentState].featureIDStart]
                    .animationID;
                var maxFrames = animationsData[animID].Count;

                ToggleStatePhysicsAndCollisions();

                // - If warping state, init it
                var isWarpState = CurrentState == WarpState && WarpData != null;
                if (isWarpState)
                {
                    InitWarpingProperties(featuresData, animationsData, WarpState);
                    TransformWarpingToLocal();
                }

                // - Get the maximum pose by time if last state
                if (state == CurrentState)
                    maxFrames = (int)(time * maxFrames) + 1; //Since time is [0,1], get the nearest frame


                // - Get Poses depending on warp or not
                var poseList = isWarpState ? WarpData : animationsData[animID];

                //By default, 1 iteration on each state, but In Progress can be loopable
                for (int currentFrame = 0; currentFrame < maxFrames; currentFrame++)
                {
                    // - Apply each frame displacement hh
                    AnimationData nextAnimationData = poseList[currentFrame];
                    
                    var nextPos =
                        MathUtils.TranslateToGlobal(
                            root.position,
                            root.rotation,
                            nextAnimationData.rootPosition
                        );
                    
                    var nextRot = ((quaternion)root.rotation).Add(nextAnimationData.rootRotation);
                    
                    controller.Move(nextPos, nextRot, Time.fixedDeltaTime);
                }

                if (CurrentState == state)
                    hasFinished = true;
                else
                    ((ActionPoseFinder)PoseFinder).HandleNextState(this);
                
            }
        }
        
        //Init currentFrame and InitFrame
        protected virtual void InitAnimationBaseIndexes()
        {
            InitializeFirstState();
            ((ActionPoseFinder)PoseFinder).UpdateAnimationIndexes(this);
        }

        protected void InitializeFirstState()
        {
            CurrentState = ActionQueryComputed.actionTag.HasInitState() ? 
                ActionTagState.Init : 
                ActionTagState.InProgress;
        }

        //Init currentFrame and InitFrame by time reference
        protected virtual void InitAnimationIndexesByTime(ActionTagState state, float time)
        {
            CurrentState = state;
            
            // Get current frames from UpdateAnimationIndexes
            ((ActionPoseFinder)PoseFinder).UpdateAnimationIndexesByTime(this, time);
        }
        
        //Set collisions and physics properties and initial setup
        protected void SetCollisionsAndPhysics(CharacterControllerBase characterController,
            CollisionsPhysicsSetup[] physicsCollisionsSetup)
        {
            _characterController = characterController;

            _physicsEnabled = new bool[3];
            _collisionsEnabled = new bool[3];
                
            //Set up each state setup
            for (int i = 0; i < 3; i++)
            {
                _physicsEnabled[i] = (int)physicsCollisionsSetup[i] == ((int)physicsCollisionsSetup[i] | (int)CollisionsPhysicsSetup.PhysicsEnabled);
                _collisionsEnabled[i] = (int)physicsCollisionsSetup[i] == ((int)physicsCollisionsSetup[i] | (int)CollisionsPhysicsSetup.CollisionsEnabled);
            }
            
            //Initialize first state
            ToggleStatePhysicsAndCollisions();
        }
        
        //Modify collisions and physics based on current state
        protected void ToggleStatePhysicsAndCollisions() => 
            _characterController.ToggleCollisionsAndPhysics(_physicsEnabled[(int)CurrentState], _collisionsEnabled[(int)CurrentState]);
        
        protected void InitWarpingData(TargetProperties? targetToWarp, List<FeatureData> featuresData,
            List<List<AnimationData>> animationsData, ActionTagState warpState)
        {
            _hasPos = (int)ActionQueryComputed.actionTag.warpingType == ((int)ActionQueryComputed.actionTag.warpingType | (int)WarpingType.Position);
            _hasRot = (int)ActionQueryComputed.actionTag.warpingType == ((int)ActionQueryComputed.actionTag.warpingType | (int)WarpingType.Rotation);

            if (targetToWarp == null)
            {
                _targetToWarp = null;
                return;
            }

            int stateID = (int)warpState;
            //Init distance between target and current
            var framesToWarp = ActionQueryComputed.actionTag.ranges[stateID].poseStop -
                               ActionQueryComputed.actionTag.ranges[stateID].poseStart;
            _timePerFrame = 1.0f / framesToWarp;
            _targetToWarp = targetToWarp;

            //Init warping data array
            var warpAnimID = featuresData[ActionQueryComputed.GetRanges()[stateID].featureIDStart].animationID;
            WarpData = new List<AnimationData>(new AnimationData[animationsData[warpAnimID].Count]);

            if (CurrentState == warpState)
            {
                //if warp is start state
                InitWarpingProperties(featuresData, animationsData, warpState);
                TransformWarpingToLocal();
            }
        }

        public virtual void CheckInitWarpingPropertiesAndPhysicsSetup()
        {
            if (CurrentState == WarpState && !isQueryDone)
            {
                //Init Dynamic Warping
                InitWarpingProperties(ActionQueryComputed.featuresData, dataset.animationsData, WarpState);
                TransformWarpingToLocal();
            }

            if (!isQueryDone)
                ToggleStatePhysicsAndCollisions();
        }

        protected void TransformWarpingToLocal()
        {
            if (_targetToWarp == null) return;
            
            //We will use a reverse approach
            for (int currentFrame = WarpData.Count - 1; currentFrame >= 0; currentFrame--)
            {
                AnimationData finalAnimationData = WarpData[currentFrame];
                AnimationData baseAnimationData;

                if (currentFrame == 0)
                {
                    baseAnimationData.rootPosition = root.position;
                    baseAnimationData.rootRotation = root.rotation;
                }
                else
                    baseAnimationData = WarpData[currentFrame - 1];

                var model4X4 =
                    float4x4.TRS(baseAnimationData.rootPosition,
                        baseAnimationData.rootRotation, new float3(1));
                
                var finalPosInLocalSpace = math.transform(
                    math.inverse(model4X4),
                    finalAnimationData.rootPosition);

                var finalRotInLocalSpace = 
                    math.mul(finalAnimationData.rootRotation, math.inverse(baseAnimationData.rootRotation));
                
                //Assign new warping data to list
                AnimationData newWarpData = finalAnimationData;
                newWarpData.bonesData = finalAnimationData.bonesData;
                
                newWarpData.rootPosition = finalPosInLocalSpace;
                newWarpData.rootRotation = finalRotInLocalSpace;

                WarpData[currentFrame] = newWarpData;
            }
        }

        protected void InitWarpingProperties(List<FeatureData> featuresData,
            List<List<AnimationData>> animationsData, ActionTagState animState)
        {
            if (_targetToWarp == null) return;
            
            var animID = featuresData[ActionQueryComputed.GetRanges()[(int)animState].featureIDStart].animationID;
            
            //Check if they are the same            
            if (_hasPos && _hasRot && ActionQueryComputed.actionTag.rotWarpingMode == ActionQueryComputed.actionTag.posWarpingMode)
            {
                //If they are both the same, init using a same call
                if (ActionQueryComputed.actionTag.rotWarpingMode == WarpingMode.None) return;

                if (ActionQueryComputed.actionTag.rotWarpingMode == WarpingMode.Dynamic)
                {
                    InitDynamicWarpingTransforms(featuresData, animationsData, animState);
                    return;
                }

                InitCurveWarpingTransforms(ActionQueryComputed.actionTag.rotWarpingMode, WarpingType.PositionRotation, animationsData[animID]);
                return;
            }

            //If some of them is set to dynamic, then do it first
            if (_hasPos && ActionQueryComputed.actionTag.posWarpingMode is WarpingMode.Dynamic ||
                _hasRot && ActionQueryComputed.actionTag.rotWarpingMode is WarpingMode.Dynamic)
            {
                InitDynamicWarpingTransforms(featuresData, animationsData, animState);
            }

            //Then do the others based on curves
            if (_hasPos)
            {
                var posMode = ActionQueryComputed.actionTag.posWarpingMode switch
                {
                    WarpingMode.Linear => WarpingMode.Linear,
                    WarpingMode.Quadratic => WarpingMode.Quadratic,
                    WarpingMode.Exponential => WarpingMode.Exponential,
                    WarpingMode.DecayLogarithmic => WarpingMode.DecayLogarithmic,
                    WarpingMode.Custom => WarpingMode.Custom,
                    _ => WarpingMode.None //This won't be called unless we add new types
                };
                if(posMode != WarpingMode.None)
                    InitCurveWarpingTransforms(posMode, WarpingType.Position, animationsData[animID]);
            }

            if (!_hasRot) return;

            //Rotation init
            var rotMode = ActionQueryComputed.actionTag.rotWarpingMode switch
            {
                WarpingMode.Linear => WarpingMode.Linear,
                WarpingMode.Quadratic => WarpingMode.Quadratic,
                WarpingMode.Exponential => WarpingMode.Exponential,
                WarpingMode.DecayLogarithmic => WarpingMode.DecayLogarithmic,
                WarpingMode.Custom => WarpingMode.Custom,
                _ => WarpingMode.None
            };
            if(rotMode != WarpingMode.None)
                InitCurveWarpingTransforms(rotMode, WarpingType.Rotation, animationsData[animID]);
        }

        private void InitDynamicWarpingTransforms(List<FeatureData> featuresData, List<List<AnimationData>> animationsData, ActionTagState animState)
        {
            //Get In progress animation frames
            var animID = featuresData[ActionQueryComputed.GetRanges()[(int)animState].featureIDStart].animationID;

            //Transform each one to world relative transform transform
            //We will apply it later as offset from initial position
            float3 currentPos = root.position;
            quaternion currentRot = root.rotation;

            for (int currentFrame = 0; currentFrame < WarpData.Count; currentFrame++)
            {
                AnimationData nextAnimationData = animationsData[animID][currentFrame];
                AnimationData newWarpData = WarpData[currentFrame];

                var model4X4 =
                    float4x4.TRS(currentPos,
                        currentRot, new float3(1));

                var posInGlobalSpace = math.transform(
                    model4X4, 
                    nextAnimationData.rootPosition);

                currentPos = posInGlobalSpace;
                currentRot = math.mul(currentRot, nextAnimationData.rootRotation);

                //Assign new warping data to list
                newWarpData.bonesData = nextAnimationData.bonesData;
                newWarpData.rootPosition = posInGlobalSpace;
                newWarpData.rootRotation = currentRot;

                WarpData[currentFrame] = newWarpData;
            }

            InitDynamicWarpingDeformation();

            //While Update Targets, we will scale this movement based on the final position vs final target
        }

        private void InitDynamicWarpingDeformation()
        {
            if (_targetToWarp == null) return;
            var currentTarget = ((TargetProperties)_targetToWarp);
            
            //Get positions
            var animFinalPos = WarpData[^1].rootPosition;
            var targetFinalPos = currentTarget.Position;
            
            //Check if contact warping
            if (ActionQueryComputed.actionTag.contactWarping)
                targetFinalPos = GetContactWarpingTarget(targetFinalPos, ActionQueryComputed.actionTag.warpContactBones, WarpData[^1].bonesData);
            
            //Position deformation
            var dynamicPosDeformation = (Vector3)(targetFinalPos - animFinalPos);
            var dynamicPosMagnitude = dynamicPosDeformation.magnitude;
            dynamicPosDeformation = dynamicPosDeformation.normalized;
            
            //Get rotations
            var animFinalRot = WarpData[^1].rootRotation;
            var targetFinalRot = currentTarget.Rotation;
            
            //Rotation deformation
            var dynamicRotDeformation = math.mul(targetFinalRot, math.inverse(animFinalRot));
            
            for (int i = 0; i < WarpData.Count; i++)
            {
                float currentTime = Mathf.Clamp(i * _timePerFrame, 0.0f, 1.0f - 1e-10f);
            
                var newWarpData = WarpData[i];
            
                if (_hasPos)
                {
                    //Positions
                    var currentPosMagnitude = currentTime * dynamicPosMagnitude;
                    var deformedPos =
                        newWarpData.rootPosition + (float3)dynamicPosDeformation * currentPosMagnitude;
            
                    newWarpData.rootPosition = ActionQueryComputed.actionTag.positionWarpWeight * deformedPos;
                }
            
                if (_hasRot)
                {
                    //Rotations
                    var currentDeformation = Quaternion.Slerp(Quaternion.identity, dynamicRotDeformation, currentTime);
                    var deformationRot =
                        math.mul(newWarpData.rootRotation, currentDeformation);
            
                    //Apply weight
                    newWarpData.rootRotation = Quaternion.Slerp(Quaternion.identity, deformationRot,
                        ActionQueryComputed.actionTag.rotationWarpWeight);
                }
                WarpData[i] = newWarpData;
            }
        }

        private float3 GetContactWarpingTarget(float3 originalTargetPos, List<AvatarBone> warpContactBones, BoneData[] finalFrameBonesData)
        {
            //1. Get transform matrix from final root space to world space
            var currentTarget = ((TargetProperties)_targetToWarp);
            var rootFinalPos = currentTarget.Position;
            var rootFinalRot = currentTarget.Rotation;

            var model4X4 =                      //Local to global
                float4x4.TRS(rootFinalPos,
                    rootFinalRot, new float3(1));
                
            // 2. Compute position foreach bone
            float3 bonesPosInWorldSpace = float3.zero;
            foreach (var contactBone in warpContactBones)
            {
                // 2.1 Get Bone Data
                BoneData boneData = finalFrameBonesData[contactBone.id];
                    
                var bonePos = boneData.position;
                //var boneRot = boneData.rotation;
                    
                // 2.2 Transform pos to world space
                var boneInWorldSpace = math.transform(
                    model4X4, 
                    bonePos);

                // 2.3 Add to sum
                bonesPosInWorldSpace += boneInWorldSpace;
            }
            bonesPosInWorldSpace /= warpContactBones.Count;
                
            //4. Get Target final Position based on new bones
            //Assume target is Bone Position and compute Root Position
            var worldBonesToRoot = rootFinalPos - bonesPosInWorldSpace;
            var targetFinalPos = originalTargetPos + worldBonesToRoot;

            return targetFinalPos;
        }

        private void InitCurveWarpingTransforms(WarpingMode mode, WarpingType type, List<AnimationData> animData)
        {
            if (_targetToWarp == null) return;
            var currentTarget = ((TargetProperties)_targetToWarp);
            
            var targetFinalPos = currentTarget.Position;
            
            //Check if contact warping
            if (ActionQueryComputed.actionTag.contactWarping && type is WarpingType.Position or WarpingType.PositionRotation)
                targetFinalPos = GetContactWarpingTarget(targetFinalPos, ActionQueryComputed.actionTag.warpContactBones, animData[^1].bonesData);
            
            
            //Position and rotation offset init
            var positionOffset = (Vector3)targetFinalPos - root.position;
            var rotationOffset = currentTarget.Rotation * Quaternion.Inverse(root.rotation);
            
            for (int i = 0; i < WarpData.Count; i++)
            {
                float currentTime = Mathf.Clamp(i * _timePerFrame, 0.0f, 1.0f - 1e-10f);
                var newWarpData = WarpData[i];
            
                //Get curve value
                var curveValue = EvaluateCurve(mode, type, currentTime);
            
                //Check position
                if (type is WarpingType.Position or WarpingType.PositionRotation)
                {
                    //Positions
                    float3 curvePosIncrease =
                        curveValue * positionOffset; //Current displacement based on time % time is [0, 1]
                    newWarpData.rootPosition =
                        (float3)root.position + curvePosIncrease * ActionQueryComputed.actionTag.positionWarpWeight;
                }
            
                //Check rotation
                if (type is WarpingType.Rotation or WarpingType.PositionRotation)
                {
                    //Rotations
                    var rotation = root.rotation;
                    quaternion curveRotIncrease = Quaternion.Slerp(quaternion.identity, rotationOffset, currentTime);
                    newWarpData.rootRotation = rotation * Quaternion.Slerp(quaternion.identity, curveRotIncrease,
                        ActionQueryComputed.actionTag.rotationWarpWeight);
                }
            
                newWarpData.bonesData = animData[i].bonesData;
                WarpData[i] = newWarpData;
            }
        }
        
        private float EvaluateCurve(WarpingMode mode, WarpingType type, float currentTime)
        {
            return mode switch
            {
                WarpingMode.Linear =>
                    //y = x
                    currentTime,

                WarpingMode.Quadratic =>
                    //y = x * x
                    currentTime * currentTime,

                WarpingMode.Exponential =>
                    //y = e^(1-1/x^2)
                    math.exp(1 - 1 / (currentTime * currentTime)),

                WarpingMode.DecayLogarithmic =>
                    //y = sqrt(ln(x+1)/ln2)
                    math.sqrt(math.log(currentTime + 1) / math.LN2),

                WarpingMode.Custom => type == WarpingType.Position
                    ? ActionQueryComputed.actionTag.customWarpPositionCurve.Evaluate(currentTime)
                    : ActionQueryComputed.actionTag.customWarpRotationCurve.Evaluate(currentTime),

                _ => 0.0f
            };
        }
        
        protected override void SetQueryComputed(QueryComputed queryComputed)
        {
            base.SetQueryComputed(queryComputed);
            ActionQueryComputed = (ActionQueryComputed)queryComputed;
        }
        
        public ActionQueryComputed GetActionQueryComputed()
        {
            return ActionQueryComputed;
        }

        //Check whether this action can be interrupted and reset if so
        public virtual bool TryInterrupt(string[] query, bool isMotionQuery)
        {
            var currentStateInterruptible = ActionQueryComputed.actionTag.isInterruptibleByState[(int)CurrentState];
            if (!currentStateInterruptible) return false;
            
            var interruptibleType = ActionQueryComputed.actionTag.interruptibleType;
            switch (interruptibleType)
            {
                case InterruptibleBy.None: return false;
                case InterruptibleBy.Actions when isMotionQuery: return false;
                case InterruptibleBy.Motions when !isMotionQuery: return false;
                case InterruptibleBy.NameList:
                {
                    var names = query;
                    bool found = names.Any(name => ActionQueryComputed.actionTag.allowedInterruptionNames.Contains(name));

                    if (!found) return false;
                    break;
                }
                case InterruptibleBy.All:
                    break;
            }
            
            //Else, reset and interrupt current action
            Reset();
            _characterController.ToggleCollisionsAndPhysics(true, true);
            return true;
        }

        public ActionAnimationTimes GetAnimationTimes()
        {
            var ranges = GetActionQueryComputed().ranges;
            var init = (ranges[0].featureIDStop - ranges[0].featureIDStart) * dataset.poseStep;
            var action = (ranges[1].featureIDStop - ranges[1].featureIDStart) * dataset.poseStep;
            var recovery = (ranges[2].featureIDStop - ranges[2].featureIDStart) * dataset.poseStep;

            return new ActionAnimationTimes()
            {
                Init = init,
                Action = action,
                Recovery = recovery,
                Total = init + action + recovery
            };
        }
        
    }
    
    public enum CollisionsPhysicsSetup
    {
        Disabled = 0,
        CollisionsEnabled = 1,
        PhysicsEnabled = 2,
        BothEnabled = 3,
    }
    
    public struct ActionAnimationTimes
    {
        public float Init;
        public float Action;
        public float Recovery;
        public float Total;

        public override string ToString()
        {
            return "{ Init => " + Init + 
                   " ; Action => " + Action + 
                   " ; Recovery => " + Recovery + 
                   " ; Total => " + Total + " }";
        }
    }
}
