using System.Collections.Generic;
using QuanticBrains.MotionMatching.Scripts.Components.PoseFinders.Implementations;
using QuanticBrains.MotionMatching.Scripts.Components.PoseSetters.Implementations;
using QuanticBrains.MotionMatching.Scripts.Components.Queries;
using QuanticBrains.MotionMatching.Scripts.Containers;
using QuanticBrains.MotionMatching.Scripts.Extensions;
using QuanticBrains.MotionMatching.Scripts.Helpers;
using QuanticBrains.MotionMatching.Scripts.Input.CharacterController;
using QuanticBrains.MotionMatching.Scripts.Tags;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace QuanticBrains.MotionMatching.Scripts.Components.QueryFlows.Implementations
{
    public class LoopActionQueryComputedFlow : ActionQueryComputedFlow
    {
        private List<AnimationData> _simData; //Root positions+rotations, bones data
        private bool _simulateRootMotion;
        private readonly float _poseStep;
        private readonly MotionMatching _mm; //We get velocity from MM script

        public bool InterruptedLoop;

        private const ActionTagState WarpState = ActionTagState.Init;

        public LoopActionQueryComputedFlow(
            Dataset dataset,
            MotionMatching motionMatching,
            CurrentBoneTransformsValues currentBoneTransformsValues,
            TransformAccessArray characterTransformsNative,
            GlobalWeights globalWeights,
            int searchRate) : base(dataset, motionMatching.transform, currentBoneTransformsValues,
            characterTransformsNative, globalWeights,
            searchRate)
        {
            PoseFinder = new LoopActionPoseFinder();
            PoseSetter = new MotionPoseSetter();

            _mm = motionMatching;
            _poseStep = dataset.poseStep;
        }

        public override void Build(QueryComputed queryComputed, int length)
        {
            base.Build(queryComputed, length);
            _simulateRootMotion = ActionQueryComputed.actionTag.simulateRootMotion;
        }

        protected override List<AnimationData> GetAnimationPoses()
        {
            if (isQueryDone) return dataset.GetAnimationPoses(currentFeatureID, QueryComputed);

            if (_simulateRootMotion && CurrentState == ActionTagState.InProgress) //Simulated root
            {
                return _simData ?? dataset.GetAnimationPoses(currentFeatureID, QueryComputed);
            }

            return CurrentState == WarpState && WarpData != null //Warping on init
                ? WarpData
                : dataset.GetAnimationPoses(currentFeatureID, QueryComputed);
        }

        //Initialize action
        public override void Initialize(TargetProperties? targetToWarp, List<FeatureData> featuresData,
            List<List<AnimationData>> animationsData, CharacterControllerBase controller,
            CollisionsPhysicsSetup initSetup = CollisionsPhysicsSetup.Disabled,
            CollisionsPhysicsSetup inProgressSetup = CollisionsPhysicsSetup.Disabled,
            CollisionsPhysicsSetup recoverySetup = CollisionsPhysicsSetup.Disabled)
        {
            InitAnimationBaseIndexes();
            InitWarpingData(targetToWarp, featuresData, animationsData, WarpState);
            FirstFrame = false;

            //Initialize collisions & warping
            SetCollisionsAndPhysics(controller, new[] { initSetup, inProgressSetup, recoverySetup });
            CheckInitWarpingPropertiesAndPhysicsSetup();

            InterruptedLoop = false;
        }

        public override bool InitializeBy(ActionTagState state, float time, float loopTime,
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
                case ActionTagState.Recovery when !ActionQueryComputed.actionTag.HasRecoveryState():
                    return false;
            }
            
            if(state != ActionTagState.InProgress) time = Mathf.Clamp01(time);
            
            // 1. Init animation indexes by current time
            InitAnimationIndexesByTime(state, time % 1);

            // 2. Init warping data
            InitWarpingData(warpTarget, featuresData, animationsData, WarpState);
            FirstFrame = false;

            // 3. Initialize collisions
            SetCollisionsAndPhysics(controller, new[] { initSetup, inProgressSetup, recoverySetup });

            // 4. Compute movement
            // we don't need to blend nor inertialize, just apply displacement directly + localPos + scale + rotation from last frame
            InitializeFromPose(controller, state, time, loopTime, featuresData, animationsData);
            
            InterruptedLoop = false;

            return true;
        }

        protected override void InitializeFromPose(CharacterControllerBase controller,
            ActionTagState state, float time, float loopTime,
            List<FeatureData> featuresData, List<List<AnimationData>> animationsData)
        {
            // - Init first state
            InitializeFirstState();
            
            // - Set time correctly
            if (state == ActionTagState.InProgress) loopTime = time;

            bool hasFinished = false;
            while (!hasFinished)
            {
                var animID = featuresData[ActionQueryComputed.GetRanges()[(int)CurrentState].featureIDStart].animationID;
                var maxFrames = animationsData[animID].Count;
                int nLoops = 1;
                
                ToggleStatePhysicsAndCollisions();
                
                // - If warping state, init it
                var isWarpState = CurrentState == WarpState && WarpData != null;
                if (isWarpState)
                {
                    InitWarpingProperties(featuresData, animationsData, WarpState);
                    TransformWarpingToLocal();
                }

                //Root motion
                var isRootSimulated = _simulateRootMotion && CurrentState == ActionTagState.InProgress;
                SimulateTransforms();
                
                // - Get the maximum pose by time
                if (state == CurrentState && state != ActionTagState.InProgress)
                {
                    maxFrames = (int)(time * maxFrames) + 1;
                }
                else if (CurrentState == ActionTagState.InProgress)
                {
                    nLoops = Mathf.FloorToInt(loopTime) + 1;
                    time = loopTime % 1; //Time always stored in loopTime

                    maxFrames = (int)(time * maxFrames) + 1;
                }
                
                var lastPosition = root.position;
                
                // - Loop from anim poses (nLoops referring to InProgress repeat times)
                for (int loop = 1; loop <= nLoops; loop++)
                {
                    // Warp and sim root
                    var poseList = isWarpState ? WarpData : animationsData[animID];
                    if (isRootSimulated)
                        poseList = _simData;
                    
                    var nFrames = loop == nLoops ? maxFrames : poseList.Count;
                    
                    for (int currentFrame = 0; currentFrame < nFrames; currentFrame++)
                    {
                        AnimationData nextAnimationData = poseList[currentFrame];
                        var nextPos =
                            MathUtils.TranslateToGlobal(
                                root.position,
                                root.rotation,
                                nextAnimationData.rootPosition
                            );
                    
                        var nextRot = ((quaternion)root.rotation).Add(nextAnimationData.rootRotation);
                        controller.Move(nextPos, nextRot, Time.fixedDeltaTime);
                        
                        var currentPos = root.position;
                        _mm.currentVelocity =  (currentPos - lastPosition) / Time.fixedDeltaTime;
                        lastPosition = currentPos;
                    }
                }
                
                if (CurrentState == state)
                    hasFinished = true;
                else
                    ((ActionPoseFinder)PoseFinder).HandleNextState(this);
            }
        }

        //Init currentFrame and InitFrame
        protected override void InitAnimationBaseIndexes()
        {
            InitializeFirstState();
            ((LoopActionPoseFinder)PoseFinder).UpdateAnimationIndexes(this);
        }

        public override void CheckInitWarpingPropertiesAndPhysicsSetup()
        {
            switch (CurrentState)
            {
                case ActionTagState.InProgress
                    when _simulateRootMotion: //Let's consider Simulation as a type of warping
                    SimulateTransforms();
                    break;

                case WarpState when !isQueryDone: //WarpState is Init
                    //Init Dynamic Warping
                    InitWarpingProperties(QueryComputed.featuresData, dataset.animationsData, WarpState);
                    TransformWarpingToLocal();
                    break;
            }

            if (!isQueryDone)
                ToggleStatePhysicsAndCollisions();
        }

        private void SimulateTransforms()
        {
            if (!_simulateRootMotion) return;
            if (CurrentState != ActionTagState.InProgress) return;

            InitSimulationTransforms(QueryComputed.featuresData, dataset.animationsData);
            TransformSimulationToLocal();
        }

        private void InitSimulationTransforms(List<FeatureData> featuresData, List<List<AnimationData>> animationsData)
        {
            // Get In Progress frames for simulation
            var simAnimID =
                featuresData[ActionQueryComputed.GetRanges()[(int)ActionTagState.InProgress].featureIDStart]
                    .animationID;

            //Init simData
            _simData = new List<AnimationData>(new AnimationData[animationsData[simAnimID].Count]);

            //Transform each one to world relative transform
            float3 currentPos = root.position;
            quaternion currentRot = root.rotation;

            for (int currentFrame = 0; currentFrame < _simData.Count; currentFrame++)
            {
                AnimationData nextAnimationData = animationsData[simAnimID][currentFrame];
                AnimationData newSimData = _simData[currentFrame];

                var model4X4 =
                    float4x4.TRS(currentPos,
                        currentRot, new float3(1));

                var posInGlobalSpace = math.transform(
                    model4X4,
                    nextAnimationData.rootPosition);

                currentPos = posInGlobalSpace;
                currentRot = math.mul(currentRot, nextAnimationData.rootRotation);

                //Assign new warping data to list
                newSimData.bonesData = nextAnimationData.bonesData;
                newSimData.rootPosition = posInGlobalSpace;
                newSimData.rootRotation = currentRot;

                _simData[currentFrame] = newSimData;
            }

            SimulateNewTransforms(_mm.currentVelocity);

            //While Update Targets, we will scale this movement based on the final position vs final target
        }

        private void SimulateNewTransforms(float3 velocity)
        {
            //Modify transforms and add simulated velocity

            //Init simulation variables (linear velocity)
            float3 lastPosition = _simData[0].rootPosition;

            for (int i = 0; i < _simData.Count; i++)
            {
                var newSimData = _simData[i];
                newSimData.rootPosition = lastPosition + velocity * _poseStep;

                lastPosition = newSimData.rootPosition;

                _simData[i] = newSimData;
            }
        }

        private void TransformSimulationToLocal()
        {
            //Transform data to local again
            for (int currentFrame = _simData.Count - 1; currentFrame >= 0; currentFrame--)
            {
                AnimationData finalAnimationData = _simData[currentFrame];
                AnimationData baseAnimationData;

                if (currentFrame == 0)
                {
                    baseAnimationData.rootPosition = root.position;
                    baseAnimationData.rootRotation = root.rotation;
                }
                else
                    baseAnimationData = _simData[currentFrame - 1];

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

                _simData[currentFrame] = newWarpData;
            }
        }

        public override bool TryInterrupt(string[] query, bool isMotionQuery)
        {
            var currentStateInterruptible = ActionQueryComputed.actionTag.isInterruptibleByState[(int)CurrentState];
            if (!currentStateInterruptible) return false;

            if (CurrentState != ActionTagState.InProgress) return base.TryInterrupt(query, isMotionQuery);

            return EndLoopAction();
        }

        /// <summary>
        /// Ends current loop action by setting last state (recovery) or ending on next find call
        /// </summary>
        public bool EndLoopAction()
        {
            if (CurrentState != ActionTagState.InProgress) return false;

            InterruptedLoop = true; //This makes that next in progress find makes action go to next state and end
            return !ActionQueryComputed.actionTag.HasRecoveryState();
            //((LoopActionPoseFinder)PoseFinder).SetNextStateProperties(this);
        }
    }
}
