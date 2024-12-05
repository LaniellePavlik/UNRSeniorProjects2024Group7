using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QuanticBrains.MotionMatching.Scripts.Components;
using QuanticBrains.MotionMatching.Scripts.Components.PoseBlenders.Models;
using QuanticBrains.MotionMatching.Scripts.Components.PoseFinders;
using QuanticBrains.MotionMatching.Scripts.Components.PoseFinders.Models;
using QuanticBrains.MotionMatching.Scripts.Components.Queries;
using QuanticBrains.MotionMatching.Scripts.Components.Queries.Implementations;
using QuanticBrains.MotionMatching.Scripts.Components.QueryFlows;
using QuanticBrains.MotionMatching.Scripts.Components.QueryFlows.Implementations;
using QuanticBrains.MotionMatching.Scripts.Containers;
using QuanticBrains.MotionMatching.Scripts.Containers.CustomAvatars;
using QuanticBrains.MotionMatching.Scripts.Containers.ExclusionMasks;
using QuanticBrains.MotionMatching.Scripts.CustomAttributes;
using QuanticBrains.MotionMatching.Scripts.Extensions;
using QuanticBrains.MotionMatching.Scripts.Helpers;
using QuanticBrains.MotionMatching.Scripts.Input.CharacterController;
using QuanticBrains.MotionMatching.Scripts.Tags;
using Sirenix.OdinInspector;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using MathUtils = QuanticBrains.MotionMatching.Scripts.Extensions.MathUtils;

namespace QuanticBrains.MotionMatching.Scripts
{
    public enum BlendingTypes
    {
        Lerp,
        Slerp
    }
    
    [HelpURL("https://docs.google.com/document/d/1Z5nf8H7vYYVHvkle0CqVThX3sSzWVuN12gjpDws-y18/edit?usp=drive_link")]
    public class MotionMatching : MonoBehaviour
    {
        #region Variables

        private const string Idle = "Idle";

        [ImageInScript("Assets/QuanticBrains/MotionMatching/Gizmos/vaizz-logo.png")]
        public string img;
        public CustomAvatar avatar;
        public Dataset dataset;
        public int searchRate = 5;
        public bool isRunning = true;
        
        [Range(0.05f, 0.2f)] public float halfLife = 0.05f;
        public bool wantApplyPositions;
        public bool wantApplyScales;
        
        [HideInInspector]
        public int currentAnimationID;
        [HideInInspector]
        public int currentAnimationFrame;
        
        [HideInInspector]
        public int animationName;
        
        private float _timeFromLastSearch;
        
        private bool _isInertialized = true;

        [Header("Estimation properties")]
        [Tooltip("Responsiveness for direction control")]
        public float responsivenessDirections = 0.75f;

        [Tooltip("Responsiveness for position control")]
        public float responsivenessPositions = 0.75f;

        [Header("Character Controller")]
        [Tooltip("ScriptableObject with your custom character controller implementation")]
        [SerializeField]
        protected CharacterControllerBase characterControllerBase;

        protected CharacterControllerBase CharacterControllerBaseInstantiated;
        
        [SerializeField]
        private ExclusionMaskBase exclusionMask;
        [Header("Weight Bones")]
        [BoxGroup("Weight Bones")] [Range(0f, 1f)]
        public float weightBonesPosition = 1f;

        [BoxGroup("Weight Bones")] [Range(0f, 1f)]
        public float weightBonesVelocity = 1f;

        [Header("Weight Futures")]
        [BoxGroup("Weight Futures")] [Range(0f, 1f)]
        public float weightFutureRootPosition = 1f;

        [BoxGroup("Weight Futures")] [Range(0f, 1f)]
        public float weightFutureRootDirection = 1f;

        [Header("Weight Pasts")]
        [BoxGroup("Weight Pasts")] [Range(0f, 1f)]
        public float weightPastRootPosition = 1f;

        [BoxGroup("Weight Pasts")] [Range(0f, 1f)]
        public float weightPastRootDirection = 1f;
        
        [Header("Weight Global")]
        [BoxGroup("Weight Global")] [Range(0f, 1f)]
        public float weightBones = 1f;

        [BoxGroup("Weight Global")] [Range(0f, 1f)]
        public float weightFutures = 1f;
        
        [BoxGroup("Weight Global")] [Range(0f, 1f)]
        public float weightPasts = 1f;
        [HideInInspector]
        
        [Tooltip("It's only informative")]
        public string[] currentPlayedQuery;
        public string[] _currentQuery;
        public string[] startingQuery = {"Idle"};
        
        [Header("Blending Configuration")]
        public BlendingTypes blendType = BlendingTypes.Slerp;
        [HideInInspector]
        public bool isBlendActivated = true;
        
        [Header("Debug")] [Tooltip("Debug trajectory visually")]
        public bool debugTrajectory = true;
        
        private FuturePrediction _nextPrediction;
        //Components
        protected TrajectoryEstimation TrajectoryEstimation;

        private float _delta;
        private bool _isSearch;
        //public Action onTransition;

        //private int _counter;

        private List<QueryComputedFlow> _flows;
        public QueryComputedFlow currentQueryFlow;
        private MotionData _motionData;

        //Transforms
        private Transform[] _characterTransforms;
        private TransformAccessArray _characterTransformsNative;

        //Generalize avatar
        private NativeArray<quaternion> _originalDiffRotations;
        
        private float _weightBonesPositionTemp;
        private float _weightBonesVelocityTemp;
        private float _weightFutureRootPositionTemp;
        private float _weightFutureRootDirectionTemp;
        private float _weightBonesTemp;
        private float _weightFuturesTemp;
        
        private bool _wantStop;
        private int _bonesLength;

        private bool _isTestRangesActive;
        private int _rangeToTest;

        private float _timeOnLastPositions;
        private GlobalWeights _globalWeights;
        
        private BlendingResults _blendingResults;
        private BlendBoundaries _blendBoundaries;
        
        private NativeArray<BoneData> _currentBoneDatas;
        private NativeArray<BoneData> _nextBoneDatas;
        private NativeArray<float3> _rootPositionBoundariesResults;
        private NativeArray<quaternion> _rootRotationBoundariesResults;
        private NativeArray<float3> _trajectoryFloat3Results;
        private NativeArray<float3> _bonePositionResults;
        private NativeArray<float3> _boneScaleResults;
        private NativeArray<quaternion> _boneRotationResults;
        private PoseFinderGenericVariables _poseFinderGenericVariables;
        private CurrentBoneTransformsValues _currentBoneTransformsValues;
        private NativeArray<DistanceResult> _poseResult;
        protected PastTrajectory PastTrajectory;
        private Vector3 _lastPos;
        [HideInInspector] public float3 currentVelocity;

        private float4x4 _rtsModel;
        private float4x4 _rtsModelInverse;
        private Coroutine _synchronizeCoroutine;

        private bool _hasExcludedMaskChanged;

        private bool _wantDebugDistances;
        
        #endregion

        #region TestButtons
        #endregion

        #region MotionMatching
        public virtual void Awake()
        {
            
            dataset.LoadData();

            if (!avatar)
            {
                avatar = dataset.avatar;
            }

            if (!ExclusionMaskMatch(exclusionMask))
            {
                exclusionMask = null;
            }
            
            //Create components and init
            _bonesLength = avatar.Length;
            _characterTransforms = avatar.GetCharacterTransforms(transform, exclusionMask);
            _characterTransformsNative = new TransformAccessArray(_characterTransforms);
            _currentBoneTransformsValues = new CurrentBoneTransformsValues()
            {
                positions = new NativeArray<float3>(_characterTransforms.Length, Allocator.Persistent),
                rotations = new NativeArray<quaternion>(_characterTransforms.Length, Allocator.Persistent),
                localPositions = new NativeArray<float3>(_characterTransforms.Length, Allocator.Persistent),
                localRotations = new NativeArray<quaternion>(_characterTransforms.Length, Allocator.Persistent),
                localScales = new NativeArray<float3>(_characterTransforms.Length, Allocator.Persistent),
                bonesCounter = _characterTransforms.Length
            };
            
            //Get T-Pose values
            InitializeInitialRotations();

            UpdateTransformsValues();
            UpdateModelValues();

            InitializeMotionData(_bonesLength);
            _poseFinderGenericVariables = new PoseFinderGenericVariables();
            _poseFinderGenericVariables.Create(dataset.futureEstimates, dataset.pastEstimates, _bonesLength);
            
            InitializeResults(_characterTransforms.Length);
            InitializeBoundaries(_characterTransforms.Length);
            
            PoseFinder.GetRootBasedPositions(
                ref _motionData.PreviousPositions,
                _currentBoneTransformsValues.positions, 
                _currentBoneTransformsValues.bonesCounter,
                _rtsModelInverse);
            
            Inertialization.FirstFillInMotionData(ref _motionData, _originalDiffRotations, 
                transform.rotation, _characterTransforms, _bonesLength);

            //Character Controller
            CharacterControllerBaseInstantiated = Instantiate(characterControllerBase)
                .Also(cc => cc.Initialize(this));

            //Trajectory
            TrajectoryEstimation =
                new TrajectoryEstimation(dataset.futureEstimates, dataset.pastEstimates, responsivenessDirections, responsivenessPositions, dataset, transform.position, transform.forward);

            _globalWeights = new GlobalWeights()
            {
                weightBones = weightBones,
                weightFutures = weightFutures,
                weightBonesPosition = weightBonesPosition,
                weightBonesVelocity = weightBonesVelocity,
                weightFutureRootDirection = weightFutureRootDirection,
                weightFutureRootPosition = weightFutureRootPosition,
                weightPastRootPosition = weightPastRootPosition,
                weightPastRootDirection = weightPastRootDirection,
                weightpasts = weightPasts
            };
            
            //Initial pose Update from poseLookUp
            ManageQueries();
            currentAnimationFrame = 0;
            if (startingQuery[0].Equals(Idle))
            {
                SendIdleQuery();
            }
            else
            {
                SetQuery(startingQuery);
            }
            _synchronizeCoroutine = StartCoroutine(SynchronizeTransforms());

            _lastPos = transform.position;
        }

        private void FixedUpdate()
        {
            ManageMotionMatching();
        }
        
        private void ManageMotionMatching()
        {
            if (!isRunning) return;
            
            //Get current trajectory estimation based on input
            UpdateInput();
            UpdateModelValues();
            
            _nextPrediction = TrajectoryEstimation.GetFutureEstimations(
                ref _trajectoryFloat3Results,
                currentQueryFlow.GetQueryComputed(),
                _rtsModelInverse,
                transform,
                CharacterControllerBaseInstantiated.currentMoveInput,
                CharacterControllerBaseInstantiated.currentForward,
                CharacterControllerBaseInstantiated.IsStrafing(),
                Time.fixedDeltaTime,
                Time.fixedDeltaTime);

            ManageCharacterVelocity();
            
            PastTrajectory = TrajectoryEstimation.ManagePastTrajectory(transform);
            //Update visual frame every poseStep
            _delta += Time.fixedDeltaTime;
            if (_delta >= dataset.poseStep)
            {
                currentQueryFlow.GetNewPose(
                    ref _timeOnLastPositions, 
                    ref _motionData, 
                    ref _blendBoundaries,
                    ref _delta, 
                    _rtsModelInverse,
                    _poseFinderGenericVariables,
                    _rootPositionBoundariesResults,
                    _rootRotationBoundariesResults,
                    _poseResult,
                    _currentBoneDatas,
                    _nextBoneDatas,
                    _currentBoneTransformsValues.positions,
                    _originalDiffRotations,
                    _currentBoneTransformsValues.bonesCounter,
                    _nextPrediction, 
                    PastTrajectory,
                    avatar.GetRootBone(),
                    responsivenessDirections, 
                    IsDisabledRoot(),
                    _isInertialized,
                    wantApplyPositions,
                    wantApplyScales,
                    _wantDebugDistances,
                    _isTestRangesActive);
                currentQueryFlow.elapsedTime = _delta;
            }
            else
            {
                currentQueryFlow.elapsedTime += Time.fixedDeltaTime;
            }

            currentQueryFlow.GenerateNewPoseValues(
                ref _motionData.Offsets,
                ref _bonePositionResults,
                ref _boneScaleResults,
                ref _boneRotationResults,
                ref _blendingResults,
                out _timeOnLastPositions,
                _rtsModelInverse,
                _currentBoneTransformsValues.positions,
                _motionData.PreviousPositions,
                _originalDiffRotations,
                transform.rotation,
                _blendBoundaries,
                blendType,
                _currentBoneTransformsValues.bonesCounter,
                avatar.GetRootBone(),
                halfLife,
                IsDisabledRoot(),
                _isInertialized,
                isBlendActivated,
                wantApplyPositions,
                wantApplyScales);
            
            if (currentQueryFlow.isQueryDone)
            {
                ResetQueryAfterAction();
            }
            
            currentAnimationFrame = currentQueryFlow.GetFeatures()[currentQueryFlow.currentFeatureID].animFrame;
            currentAnimationID =  currentQueryFlow.GetFeatures()[currentQueryFlow.currentFeatureID].animationID;
        }

        private IEnumerator SynchronizeTransforms() // This is because of Final IK
        {
            yield return new WaitForFixedUpdate();
            if (isRunning)
            {
                currentQueryFlow
                    .SynchronizeTransforms(
                        _bonePositionResults, 
                        _boneScaleResults,
                        _boneRotationResults, 
                        _blendingResults,
                        CharacterControllerBaseInstantiated,
                        avatar.GetRootBone(),
                        IsDisabledRoot(),
                        wantApplyPositions,
                        wantApplyScales);
                UpdateTransformsValues();
            }
            
            _synchronizeCoroutine = StartCoroutine(SynchronizeTransforms());
        }

        private void UpdateTransformsValues()
        {
            new GetCurrentTransformValues()
            {
                boneTransformsValues = _currentBoneTransformsValues
            }.Schedule(_characterTransformsNative).Complete();
        }
        
        private void UpdateModelValues()
        {
            _rtsModelInverse = MathUtils.CreateInverseModel(transform.position, transform.rotation);
        }
        
        [BurstCompile]
        private struct GetCurrentTransformValues: IJobParallelForTransform
        {
            public CurrentBoneTransformsValues boneTransformsValues;         
            public void Execute(int index, TransformAccess transform)
            {
                boneTransformsValues.positions[index] = transform.position;
                boneTransformsValues.rotations[index] = transform.rotation;
                boneTransformsValues.localPositions[index] = transform.localPosition;
                boneTransformsValues.localRotations[index] = transform.localRotation;
                boneTransformsValues.localScales[index] = transform.localScale;
            }
        }

        private void UpdateInput()
        {
            //Custom user input
            if (!CharacterControllerBaseInstantiated) return;
            CharacterControllerBaseInstantiated.UpdateMotion(Time.fixedDeltaTime);
        }

        private void ManageCharacterVelocity()
        {
            var currentPos = transform.position;
            currentVelocity = (currentPos - _lastPos) / Time.fixedDeltaTime;
            _lastPos = currentPos;
        }

        private void InitializeInitialRotations()
        {
            avatar.GetOriginalAvatarRotations(out var originalCharacterRotations, out var defaultRotations, _characterTransforms, transform);
            ResetBoneRotations(_characterTransforms, defaultRotations);
            _originalDiffRotations = new NativeArray<quaternion>(originalCharacterRotations.Length, Allocator.Persistent);

            bool avatarIsHuman = avatar.avatar.isHuman;
            for (int i = 0; i < originalCharacterRotations.Length; i++)
            {
                if(avatarIsHuman)
                    _originalDiffRotations[i] = originalCharacterRotations[i] 
                                                * Quaternion.Inverse(dataset.originalBonesRotation[i]);
                else
                    _originalDiffRotations[i] = quaternion.identity;
            }
        }

        private void ResetBoneRotations(Transform[] characterTransforms, quaternion[] rotations)
        {
            for(int i = 0; i < characterTransforms.Length; i++)
            {
                if (!characterTransforms[i]) continue;
                
                characterTransforms[i].localRotation = rotations[i];
            }
        }
        
        /// <summary>
        /// Call this method to set a new query on this Motion Matching instance.
        /// If the current query is ActionQuery and it can't be interrupted by the new one,
        /// this method shall not execute any action
        /// </summary>
        /// <param name="query"> The main query name itself</param>
        /// <param name="values"> Additional query names for intersection </param>
        public void SendQuery(string query, params string[] values)
        {
            var finalQuery = values.Concat(new[] { query }).ToArray();
            _currentQuery = finalQuery;

            if (currentQueryFlow is ActionQueryComputedFlow actionQueryFlow && 
                !actionQueryFlow.TryInterrupt(finalQuery, true)) return;

            SetQuery(finalQuery);
        }

        /// <summary>
        /// Call this method to set this Motion Matching instance in Idle query.
        /// This won't be executed if there is an action currently in progress.
        /// </summary>
        public bool SendIdleQuery(bool forceUpdate = false)
        {
            _currentQuery = new[]{Idle};
            if (currentQueryFlow is ActionQueryComputedFlow) return false;
            
            SetIdleQuery();

            if (!forceUpdate) return true;
            
            var prevRunning = isRunning;
            _isInertialized = false;
            isRunning = true;
            
            ManageMotionMatching();
            
            isRunning = prevRunning;
            _isInertialized = true;
            
            currentQueryFlow
                .SynchronizeTransforms(
                    _bonePositionResults, 
                    _boneScaleResults,
                    _boneRotationResults, 
                    _blendingResults,
                    CharacterControllerBaseInstantiated,
                    avatar.GetRootBone(),
                    IsDisabledRoot(),
                    wantApplyPositions,
                    wantApplyScales);
            UpdateTransformsValues();

            return true;
        }
        
        protected void SetQuery(string[] query)
        {
            _currentQuery = query;
            ChangeQueryComputedFlow(query);
        }

        /// <summary>
        /// Call this method to execute a new Action Query on this Motion Matching instance.
        /// If another ActionQuery is currently being executed, and it can't be interrupted by the new one,
        /// this method will return false and will have no effect.
        /// </summary>
        /// <param name="query"> The action query name itself </param>
        /// <param name="targetTransform"> The world target position and rotation when using Warping. Null by default</param>
        /// <param name="initSetup"> The collisions and physics setup for the Init State (if exists). BothEnabled by default</param>
        /// <param name="actionSetup"> The collisions and physics setup for the InProgress State (if exists). BothEnabled by default</param>
        /// <param name="recoverySetup"> The collisions and physics setup for the Recovery State (if exists). BothEnabled by default</param>
        /// <returns></returns>
        public bool SendActionQuery(string query, TargetProperties? targetTransform = null,
            CollisionsPhysicsSetup initSetup = CollisionsPhysicsSetup.BothEnabled,
            CollisionsPhysicsSetup actionSetup = CollisionsPhysicsSetup.BothEnabled,
            CollisionsPhysicsSetup recoverySetup = CollisionsPhysicsSetup.BothEnabled)
        {
            
            if (currentQueryFlow is ActionQueryComputedFlow actionQueryFlow
                && !actionQueryFlow.TryInterrupt(new [] { query }, false)) 
                return false;
            
            ChangeQueryComputedFlow(new[] { query });

            ((ActionQueryComputedFlow)currentQueryFlow).Initialize(
                targetTransform, 
                currentQueryFlow.GetFeatures(), 
                dataset.animationsData,
                CharacterControllerBaseInstantiated, 
                initSetup, 
                actionSetup, 
                recoverySetup);
            
            return true;
        }

        /// <summary>
        /// Call this method to execute a new Action Query on this Motion Matching instance starting from a selected point
        /// If another ActionQuery is currently being executed, and it can't be interrupted by the new one,
        /// this method will return false and will have no effect.
        /// </summary>
        /// <param name="query"> The action query name itself </param>
        /// <param name="state"> The state you want the action to be init by </param>
        /// <param name="time"> The specific time of the state where you want the action to be init. This time is normalized,
        /// so it must be [0,1], unless you are using a loop action, where each unit will represent a complete loop </param>
        /// <param name="loopTime"> It represents how many times a loop can be performed to achieve you init pose. It should
        /// only be used when sending a loop action query and deciding to init by recovery state</param>
        /// <param name="blendWithTransition"> Whether inertialization or snap to pose should be used </param>
        /// <param name="targetTransform"> The world target position and rotation when using Warping. Null by default</param>
        /// <param name="initSetup"> The collisions and physics setup for the Init State (if exists). BothEnabled by default</param>
        /// <param name="actionSetup"> The collisions and physics setup for the InProgress State (if exists). BothEnabled by default</param>
        /// <param name="recoverySetup"> The collisions and physics setup for the Recovery State (if exists). BothEnabled by default</param>
        /// <returns></returns>
        public bool SendActionQuery(string query, ActionTagState state, 
            float time, float loopTime = 1, bool blendWithTransition = false,
            TargetProperties? targetTransform = null,
            CollisionsPhysicsSetup initSetup = CollisionsPhysicsSetup.BothEnabled,
            CollisionsPhysicsSetup actionSetup = CollisionsPhysicsSetup.BothEnabled,
            CollisionsPhysicsSetup recoverySetup = CollisionsPhysicsSetup.BothEnabled)
        {
            
            if (currentQueryFlow is ActionQueryComputedFlow actionQueryFlow
                && !actionQueryFlow.TryInterrupt(new [] { query }, false)) 
                return false;
            
            ChangeQueryComputedFlow(new[] { query });

            if (currentQueryFlow is not LoopActionQueryComputedFlow) loopTime = 0;
            
            bool isInitBy = ((ActionQueryComputedFlow)currentQueryFlow).InitializeBy(
                state,
                time,
                loopTime,
                targetTransform, 
                currentQueryFlow.GetFeatures(), 
                dataset.animationsData,
                CharacterControllerBaseInstantiated, 
                initSetup, 
                actionSetup, 
                recoverySetup);
            
            if (isInitBy)
            { 
                ResetRootEndBoundaries(transform.position, transform.rotation);
                SetPoseByFeature(currentQueryFlow.currentFeatureID);
                return true;
            }
            

            //If error occurs, send query from start + error
            ((ActionQueryComputedFlow)currentQueryFlow).Initialize(
                targetTransform,
                currentQueryFlow.GetFeatures(),
                dataset.animationsData,
                CharacterControllerBaseInstantiated,
                initSetup,
                actionSetup,
                recoverySetup);
            
            Debug.LogError("Query Error.- State: " + state + " does not exist for query: " + query);
            return false;
        }
        
        protected void SetPoseByFeature(int featureId, bool blendWithTransition = false)
        {
            ResetBonesEndBoundaries(featureId);
            
            //FillInByPose with new feature
            var bonesData = dataset
                .GetAnimationDataFromFeature(featureId, currentQueryFlow.GetQueryComputed())
                .bonesData;

            if (blendWithTransition) return;
            
            Inertialization.FillInMotionDataByPose(ref _motionData, _originalDiffRotations,
                _characterTransforms, _bonesLength, bonesData);

            UpdateTransformsByPose(bonesData);
            UpdateTransformsValues();
        }
        
        /// <summary>
        /// Moves the character directly, bypassing the default MM movement
        /// </summary>
        /// <param name="position"> The target position in world space </param>
        /// <param name="rotation"> The target world rotation </param>
        /// <param name="disableCollisionsAndPhysics"> Whether to use collisions and physics to compute this movement or not </param>
        public void MoveCharacter(Vector3 position, Quaternion rotation, bool disableCollisionsAndPhysics = true)
        {
            var previousPhysics = CharacterControllerBaseInstantiated.physicsEnabled;
            var previousCollisions = CharacterControllerBaseInstantiated.collisionsEnabled;
            CharacterControllerBaseInstantiated.ToggleCollisionsAndPhysics(!disableCollisionsAndPhysics, !disableCollisionsAndPhysics);
            
            CharacterControllerBaseInstantiated.Move(position, rotation, Time.fixedDeltaTime);
            ResetRootStartBoundaries(transform.position, transform.rotation);
            ResetRootEndBoundaries(transform.position, transform.rotation);
            
            CharacterControllerBaseInstantiated.ToggleCollisionsAndPhysics(previousPhysics, previousCollisions);
        }

        private void UpdateTransformsByPose(BoneData[] boneData)
        {
            var rootNode = avatar.GetRootBone();
            for (int i = 0; i < _characterTransforms.Length; i++)
            {
                var characterTransform = _characterTransforms[i];
                
                if (characterTransform == null) return;
                
                if (i == rootNode || wantApplyPositions)
                {
                    characterTransform.localPosition = boneData[i].localPosition;
                }

                if (wantApplyScales)
                {
                    characterTransform.localScale = boneData[i].scale;
                }

                var rotation = 
                    math.mul(math.mul(transform.rotation, boneData[i].rotation), _originalDiffRotations[i]);
                characterTransform.rotation = rotation;
            }
        }

        private void ResetRootEndBoundaries(Vector3 position, Quaternion rotation)
        {
            _blendBoundaries.endRootPositionToBlend = position;
            _blendBoundaries.endRootRotationToBlend = rotation;
        }
        
        private void ResetRootStartBoundaries(Vector3 position, Quaternion rotation)
        {
            _blendBoundaries.startRootPositionToBlend = position;
            _blendBoundaries.startRootRotationToBlend = rotation;
        }
        
        private void ResetBonesEndBoundaries(int featureID)
        {
            //Apply pose
            var animData = 
                dataset.GetAnimationDataFromFeature(featureID, currentQueryFlow.GetQueryComputed());
            int rootBone = avatar.GetRootBone();

            for (int i = 0; i < animData.bonesData.Length; i++)
            {
                var boneData = animData.bonesData[i];

                //Rotation
                _blendBoundaries.endRotationValues[i] = boneData.rotation;
                
                //Position
                if (i == rootBone)
                    _blendBoundaries.endPositionValues[i] = boneData.position;
                else
                    _blendBoundaries.endPositionValues[i] = wantApplyPositions ? boneData.localPosition : float3.zero;

                //Scale
                _blendBoundaries.endScaleValues[i] = wantApplyScales ? boneData.scale : float3.zero;
            }
            
            //Get bones data -> localPosition, rotation, scale  (root is position)
        }
        
        //Call this method when idle is needed
        private void SetIdleQuery()
        {
            if (currentQueryFlow is IdleQueryComputedFlow) return;  //As we only have idle queries as loop, add this check
            
            ChangeQueryComputedFlow(new []{"Idle"});
            ((IdleQueryComputedFlow)currentQueryFlow).InitAnimationBaseIndexes();
        }

        /// <summary>
        /// Call this method for ending the current Loop Action Query.
        /// This will have no effect if the current played query is not a LoopActionQuery.
        /// </summary>
        public void EndLoopQuery()
        {
            if (currentQueryFlow is not LoopActionQueryComputedFlow) return;
            
            ((LoopActionQueryComputedFlow)currentQueryFlow).EndLoopAction();
        }

        /// <summary>
        /// Set a new current exclusion mask into to this Motion Matching instance.
        /// </summary>
        /// <param name="exclusionMask"></param>
        public void SetExclusionMask(ExclusionMaskBase exclusionMask)
        {
            if (!ExclusionMaskMatch(exclusionMask))
            {
                return;
            }
            
            this.exclusionMask = exclusionMask;
            avatar.GetCharacterTransforms(transform, this.exclusionMask);
            _characterTransformsNative.SetTransforms(_characterTransforms);
        }

        public ActionAnimationTimes GetActionAnimationTime()
        {
            return currentQueryFlow is not ActionQueryComputedFlow action ? default : action.GetAnimationTimes();
        }
        
        public ActionAnimationTimes GetActionAnimationTime(string query)
        {
            var queryComputed = _flows.First(qc => qc.GetQueryComputed().query.Contains(query));
            return queryComputed is not ActionQueryComputedFlow action ? default : action.GetAnimationTimes();
        }

        private bool ExclusionMaskMatch(ExclusionMaskBase exclusionMaskToCHeck)
        {
            switch (exclusionMaskToCHeck)
            {
                case HumanoidExclusionMask when avatar is not HumanoidAvatar:
                    Debug.LogError("Exclusion mask is not same type as avatar");
                    return false;
                case GenericExclusionMask when avatar is not GenericAvatar:
                    Debug.LogError("Exclusion mask is not same type as avatar");
                    return false;
                case GenericExclusionMask genericExclusionMask when
                    avatar != genericExclusionMask.genericAvatar:
                    Debug.LogError("Generic Exclusion mask avatar and generic avatar don't match");
                    return false;
                default:
                    return true;
            }
        }
        
        //Call this method when action has finished
        private void ResetQueryAfterAction()
        {
            //Set regular query instead of action
            if (_currentQuery[0].Equals(Idle))
            {
                SetIdleQuery();
            }
            else
            {
                SetQuery(_currentQuery);
            }
            
            //Reset physics and collisions
            CharacterControllerBaseInstantiated.ToggleCollisionsAndPhysics(true, true);
            
        }

        private bool IsDisabledRoot()
        {
            return exclusionMask && exclusionMask.disableRootMotion;
        }

        private void ChangeQueryComputedFlow(string[] query)
        {
            currentQueryFlow = GetCurrentQueryComputedFlow(query);
            ResetRootEndBoundaries(transform.position, transform.rotation);
            currentQueryFlow.Reset();
            _delta = dataset.poseStep;
        }

        private QueryComputedFlow GetCurrentQueryComputedFlow(string[] query)
        {
            currentPlayedQuery = query;
            return FindQueryComputedFlowByName(query);
        }

        protected QueryComputedFlow FindQueryComputedFlowByName(string[] query)
        {
            return _flows.First(qc =>
            {
                return query.All(t => qc.GetQueryComputed().query.Contains(t)) && query.Length == qc.GetQueryComputed().query.Length;
            });
        }

        private void ManageQueries()
        {
            _flows = new List<QueryComputedFlow>();
            dataset.queriesComputed.queries.ForEach(q =>
                _flows.Add(new MotionQueryComputedFlow(dataset, transform, _currentBoneTransformsValues, _characterTransformsNative, _globalWeights, searchRate)
                    .Also(m => m.Build(q, _bonesLength))));
            
            dataset.queriesComputed.actionQueries.ForEach(q =>
                _flows.Add(new ActionQueryComputedFlow(dataset, transform, _currentBoneTransformsValues, _characterTransformsNative, _globalWeights, searchRate)
                    .Also(m => m.Build(q, _bonesLength))));
            
            dataset.queriesComputed.loopActionQueries.ForEach(q =>
                _flows.Add(new LoopActionQueryComputedFlow(dataset, this, _currentBoneTransformsValues, _characterTransformsNative, _globalWeights, searchRate)
                    .Also(m => m.Build(q, _bonesLength))));

            dataset.queriesComputed.idleQueries.ForEach(q =>
                _flows.Add(new IdleQueryComputedFlow(dataset, transform, _currentBoneTransformsValues, _characterTransformsNative, _globalWeights, searchRate)
                    .Also(l => l.Build(q, _bonesLength))));
        }
        #endregion
        
        #region Collisions

        private void OnCollisionEnter(Collision collision)
        {
            CharacterControllerBaseInstantiated.OnCollisionEnter(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            CharacterControllerBaseInstantiated.OnCollisionExit(collision);
        }
        
        private void OnCollisionStay(Collision collision)
        {
            CharacterControllerBaseInstantiated.OnCollisionStay(collision);
        }

        private void OnTriggerEnter(Collider other)
        {
            CharacterControllerBaseInstantiated.OnTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            CharacterControllerBaseInstantiated.OnTriggerExit(other);
        }
        
        private void OnTriggerStay(Collider other)
        {
            CharacterControllerBaseInstantiated.OnTriggerStay(other);
        }

        #endregion

        #region Gizmos

        protected virtual void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            if (!debugTrajectory) return;
            if (math.isnan(_nextPrediction.futurePositions[0].x)) return;

            TrajectoryEstimation.DrawGizmos(transform, _characterTransforms, _nextPrediction, PastTrajectory, currentQueryFlow);
        }

        #endregion

        private void InitializeResults(int length)
        {
            _blendingResults = new BlendingResults()
            {
                bonesPosition = new NativeArray<float3>(length, Allocator.Persistent),
                bonesRotation = new NativeArray<quaternion>(length, Allocator.Persistent),
                bonesScale = new NativeArray<float3>(length, Allocator.Persistent),
                rootPosition = float3.zero,
                rootRotation = quaternion.identity
            };
            
            _bonePositionResults = new NativeArray<float3>(length, Allocator.Persistent);
            _boneScaleResults = new NativeArray<float3>(length, Allocator.Persistent);
            _boneRotationResults = new NativeArray<quaternion>(length, Allocator.Persistent);
            
            _rootPositionBoundariesResults = new NativeArray<float3>(2, Allocator.Persistent);  //0 start 1 end
            _rootRotationBoundariesResults = new NativeArray<quaternion>(2, Allocator.Persistent);  //0 start 1 end
            _trajectoryFloat3Results = new NativeArray<float3>(4, Allocator.Persistent); // Velocity, angular velocity, acceleration, next position
            _poseResult = new NativeArray<DistanceResult>(1, Allocator.Persistent);
        }

        private void InitializeMotionData(int bonesLength)
        {
            //Init Motion Data
            _motionData.Offsets = new NativeArray<OffsetBone>(bonesLength, Allocator.Persistent);
            _motionData.LastRotations = new NativeArray<quaternion>(bonesLength, Allocator.Persistent);
            _motionData.LastPositions = new NativeArray<float3>(bonesLength, Allocator.Persistent);
            _motionData.LastVelocities = new NativeArray<float3>(bonesLength, Allocator.Persistent);
            _motionData.LastScales = new NativeArray<float3>(bonesLength, Allocator.Persistent);
            _motionData.LastVelocityScales = new NativeArray<float3>(bonesLength, Allocator.Persistent);
            _motionData.AngularVelocities = new NativeArray<float3>(bonesLength, Allocator.Persistent);
            _motionData.PreviousPositions = new NativeArray<float3>(bonesLength, Allocator.Persistent);
        }
        
        private void InitializeBoundaries(int length)
        {
            _currentBoneDatas = new NativeArray<BoneData>(length, Allocator.Persistent);
            _nextBoneDatas = new NativeArray<BoneData>(length, Allocator.Persistent);
            _blendBoundaries = new BlendBoundaries()
            {
                startRotationValues = new NativeArray<quaternion>(length, Allocator.Persistent),
                endRotationValues = new NativeArray<quaternion>(length, Allocator.Persistent),
                startPositionValues = new NativeArray<float3>(length, Allocator.Persistent),
                endPositionValues = new NativeArray<float3>(length, Allocator.Persistent),
                startScaleValues = new NativeArray<float3>(length, Allocator.Persistent),
                endScaleValues = new NativeArray<float3>(length, Allocator.Persistent),
                startRootPositionToBlend = float3.zero,
                endRootPositionToBlend = float3.zero,
                startRootRotationToBlend = quaternion.identity,
                endRootRotationToBlend = quaternion.identity,
            };
        }
        
        private void OnDestroy()
        {
            _characterTransformsNative.Dispose();
            _blendingResults.bonesPosition.Dispose();
            _blendingResults.bonesRotation.Dispose();
            _blendingResults.bonesScale.Dispose();
            _rootPositionBoundariesResults.Dispose();
            _rootRotationBoundariesResults.Dispose();
            _bonePositionResults.Dispose();
            _boneScaleResults.Dispose();
            _boneRotationResults.Dispose();
            _trajectoryFloat3Results.Dispose();
            _poseResult.Dispose();
            
            _motionData.Offsets.Dispose();
            _motionData.LastRotations.Dispose();
            _motionData.LastPositions.Dispose();
            _motionData.LastVelocities.Dispose();
            _motionData.LastScales.Dispose();
            _motionData.LastVelocityScales.Dispose();
            _motionData.AngularVelocities.Dispose();
            _motionData.PreviousPositions.Dispose();

            _currentBoneDatas.Dispose();
            _nextBoneDatas.Dispose();
            _blendBoundaries.startRotationValues.Dispose();
            _blendBoundaries.endRotationValues.Dispose();
            _blendBoundaries.startPositionValues.Dispose();
            _blendBoundaries.endPositionValues.Dispose();
            _blendBoundaries.startScaleValues.Dispose();
            _blendBoundaries.endScaleValues.Dispose();

            _poseFinderGenericVariables.Destroy();

            _originalDiffRotations.Dispose();

            _currentBoneTransformsValues.UnLoad();
            TrajectoryEstimation.Destroy();
            _flows.ForEach(flow => flow.Destroy());
            dataset.Unload();
            
            if (_synchronizeCoroutine != null)
            {
                StopCoroutine(_synchronizeCoroutine);
            }

            Destroy(CharacterControllerBaseInstantiated);
        }
    }

    public struct CurrentBoneTransformsValues
    {
        public NativeArray<float3> positions;
        public NativeArray<quaternion> rotations;
        public NativeArray<float3> localPositions;
        public NativeArray<float3> localScales;
        public NativeArray<quaternion> localRotations;
        public int bonesCounter;

        public void UnLoad()
        {
            positions.Dispose();
            rotations.Dispose();
            localScales.Dispose();
            localPositions.Dispose();
            localRotations.Dispose();
        }
    }
    
    public struct GlobalWeights
    {
        public float weightBonesPosition;
        public float weightBonesVelocity;
        public float weightFutureRootPosition;
        public float weightFutureRootDirection;
        public float weightPastRootPosition;
        public float weightPastRootDirection;
        public float weightBones;
        public float weightFutures;
        public float weightpasts;
    }
}
