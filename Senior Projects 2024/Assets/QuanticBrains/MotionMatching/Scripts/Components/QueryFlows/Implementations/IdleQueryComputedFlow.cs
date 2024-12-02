using QuanticBrains.MotionMatching.Scripts.Components.PoseFinders.Implementations;
using QuanticBrains.MotionMatching.Scripts.Components.PoseFinders.Models;
using QuanticBrains.MotionMatching.Scripts.Components.PoseSetters.Implementations;
using QuanticBrains.MotionMatching.Scripts.Components.Queries;
using QuanticBrains.MotionMatching.Scripts.Components.Queries.Implementations;
using QuanticBrains.MotionMatching.Scripts.Components.Queries.Models;
using QuanticBrains.MotionMatching.Scripts.Containers;
using QuanticBrains.MotionMatching.Scripts.Tags;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace QuanticBrains.MotionMatching.Scripts.Components.QueryFlows.Implementations
{
    public class IdleQueryComputedFlow : QueryComputedFlow
    {
        private IdleQueryComputed _idleQueryComputed;
        // private bool _isDone;
        
        public int currentAnimationPoseID;
        
        public ActionTagState currentState; //Loopable Tag is like an action tag but it doesn't use recovery state
        public NativeArray<QueryRange> currentRanges;
        public NativeArray<DistanceResult> idleDistanceResults;
        public NativeArray<DistanceResult> initDistanceResults;

        public IdleQueryComputedFlow(
            Dataset dataset, 
            Transform root, 
            CurrentBoneTransformsValues currentBoneTransformsValues,
            TransformAccessArray characterTransformsNative,
            GlobalWeights globalWeights,
            int searchRate) : base(dataset, root, currentBoneTransformsValues, characterTransformsNative, globalWeights, searchRate)
        {
            PoseFinder          = new IdlePoseFinder();
            PoseSetter          = new MotionPoseSetter();
        }
        
        protected void SetDefaultRangesAndStateOnInit()
        {
            if (_idleQueryComputed.loopTag.HasInitState())
            {
                currentState = ActionTagState.Init;
                currentRanges = _idleQueryComputed.GetInitRanges();
                distanceResults = initDistanceResults;
                return;
            }
        
            currentState = ActionTagState.InProgress;
            currentRanges = _idleQueryComputed.GetIdleRanges();
            distanceResults = idleDistanceResults;
        }
        
        #region Init
        //Init currentFrame and InitFrame
        public void InitAnimationBaseIndexes()
        {
            SetDefaultRangesAndStateOnInit();
            ((IdlePoseFinder)PoseFinder).ResetCounter(0);
        }
        #endregion
        
        public void EndIdleQuery()
        {
            // _isDone = true;
            SetDefaultRangesAndStateOnInit();
        }

        public override void Build(QueryComputed queryComputed, int length)
        {
            SetQueryComputed(queryComputed);
            SetDefaultRangesAndStateOnInit();
            ManageWeights(length);
            InitializeDistanceResults();
        }
        
        protected override void InitializeDistanceResults()
        {
            idleDistanceResults = new NativeArray<DistanceResult>(_idleQueryComputed.idleRanges.Count, Allocator.Persistent);
            initDistanceResults = new NativeArray<DistanceResult>(_idleQueryComputed.initRanges.Count, Allocator.Persistent);
        }

        /*protected override void InitializeTransition(
            ref float delta, 
            ref MotionData motionData, 
            ref BlendBoundaries blendBoundaries,
            NativeArray<float3> rootPositionBoundariesResults,
            NativeArray<quaternion> rootRotationBoundariesResults,
            NativeArray<BoneData> currentBonesDatas,
            NativeArray<BoneData> nextBonesDatas,
            FuturePrediction nextPrediction, 
            float responsivenessDirections, 
            bool isInertialized)
        {
            if (isInertialized && isSearch)
            {
                Inertialization.InitTransition(ref motionData, dataset.GetAnimationDataFromFeature(currentFeatureID));
            }

            delta -= dataset.poseStep;

            PoseBlender.CalculateBoundaries(
                ref blendBoundaries,
                rootPositionBoundariesResults,
                rootRotationBoundariesResults,
                currentBonesDatas,
                nextBonesDatas,
                dataset.GetAnimationPoses(currentFeatureID), 
                dataset.featuresData[currentFeatureID].animFrame,
                delta,
                root,
                nextPrediction,
                responsivenessDirections,
                delta,
                dataset.poseStep);

            ElapsedTime = delta;
            LerpDuration = dataset.poseStep;
        }*/

        protected override void SetQueryComputed(QueryComputed queryComputed)
        {
            base.SetQueryComputed(queryComputed);
            _idleQueryComputed = (IdleQueryComputed)queryComputed;
        }

        public IdleQueryComputed GetLoopableQueryComputed()
        {
            return _idleQueryComputed;
        }

        public override NativeArray<QueryRange> GetRangesNative()
        {
            return currentRanges;
        }

        public override void Destroy()
        {
            bonesWeights.weights.Dispose();
            QueryComputed.Destroy();
            if (initDistanceResults.IsCreated)
                initDistanceResults.Dispose();
            if (idleDistanceResults.IsCreated)
                idleDistanceResults.Dispose();
        }
    }
}
