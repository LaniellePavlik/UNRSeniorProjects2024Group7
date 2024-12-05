using System;
using System.Collections.Generic;
using System.Linq;
using QuanticBrains.MotionMatching.Scripts.Components.PoseBlenders;
using QuanticBrains.MotionMatching.Scripts.Components.PoseBlenders.Models;
using QuanticBrains.MotionMatching.Scripts.Components.PoseFinders;
using QuanticBrains.MotionMatching.Scripts.Components.PoseFinders.Models;
using QuanticBrains.MotionMatching.Scripts.Components.PoseSetters;
using QuanticBrains.MotionMatching.Scripts.Components.Queries;
using QuanticBrains.MotionMatching.Scripts.Components.Queries.Models;
using QuanticBrains.MotionMatching.Scripts.Components.QueryFlows.Implementations;
using QuanticBrains.MotionMatching.Scripts.Components.QueryFlows.Models;
using QuanticBrains.MotionMatching.Scripts.Containers;
using QuanticBrains.MotionMatching.Scripts.Input.CharacterController;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace QuanticBrains.MotionMatching.Scripts.Components.QueryFlows
{
    public abstract class QueryComputedFlow
    {
        public float currentMinDistance;
        public int currentFeatureID;
        public QueryRange currentRange;

        public bool isQueryDone;
        protected PoseFinder PoseFinder;
        protected PoseSetter PoseSetter;

        public Dataset dataset;
        public TransformAccessArray characterTransformsNative;
        public CurrentBoneTransformsValues currentBoneTransformsValues;
        public Transform root;
        public int searchRate;
        public GlobalWeights globalWeights;
        
        //protected BlendBoundaries BlendBoundaries;
        //protected BlendingResults BlendingResults;
        public bool isSearch;
        
        public BonesWeights bonesWeights;
        public NativeArray<DistanceResult> distanceResults;

        protected QueryComputed QueryComputed;
        
        public float elapsedTime;
        protected float LerpDuration;

        protected QueryComputedFlow(
            Dataset dataset, 
            Transform root,
            CurrentBoneTransformsValues currentBoneTransformsValues,
            TransformAccessArray characterTransformsNative,
            GlobalWeights globalWeights,
            int searchRate)
        {
            this.globalWeights = globalWeights;
            this.dataset = dataset;
            this.root = root;
            this.characterTransformsNative = characterTransformsNative;
            this.currentBoneTransformsValues = currentBoneTransformsValues;
            this.searchRate = searchRate;

            currentFeatureID = 0;
            LerpDuration = dataset.poseStep;
        }

        public abstract void Build(QueryComputed queryComputed, int length);
        
        public virtual void Reset()
        {
            currentFeatureID = 0;
            PoseFinder.ResetCounter(searchRate);
        }
        
        public virtual void GetNewPose(
            ref float timeOnLastPositions,
            ref MotionData motionData,
            ref BlendBoundaries blendBoundaries,
            ref float delta,
            float4x4 rtsModelInverse,
            PoseFinderGenericVariables poseFinderGenericVariables,
            NativeArray<float3> rootPositionBoundariesResults,
            NativeArray<quaternion> rootRotationBoundariesResults,
            NativeArray<DistanceResult> poseResult,
            NativeArray<BoneData> currentBonesDatas,
            NativeArray<BoneData> nextBonesDatas,
            NativeArray<float3> currentBonesPosition,
            NativeArray<quaternion> originalDiffs,
            int bonesCount,
            FuturePrediction nextPrediction,
            PastTrajectory pastTrajectory,
            int rootNode,
            float responsivenessDirections,
            bool disabledRoot,
            bool isInertialized,
            bool wantApplyPositions,
            bool wantApplyScales,
            bool wantDebugDistances,
            bool forceContinuousPose)
        {
            Inertialization.UpdateMotionData(
                nextBonesDatas,
                dataset.GetAnimationDataFromFeature(currentFeatureID, QueryComputed),
                currentBoneTransformsValues.localPositions,
                currentBoneTransformsValues.localScales,
                currentBoneTransformsValues.rotations,
                originalDiffs,
                root.rotation,
                currentBoneTransformsValues.bonesCounter,
                delta, 
                dataset.poseStep, 
                ref motionData
                );
            
            currentFeatureID = PoseFinder.Find(
                ref motionData.PreviousPositions,
                ref timeOnLastPositions,
                out currentMinDistance,
                rtsModelInverse, 
                poseFinderGenericVariables,
                currentBonesPosition,
                bonesCount,
                this, 
                nextPrediction.futureOffsets,
                nextPrediction.futureOffsetDirections,
                pastTrajectory,
                poseResult,
                wantDebugDistances,
                forceContinuousPose);
            
            InitializeTransition(
                ref delta, 
                ref motionData, 
                ref blendBoundaries, 
                rtsModelInverse,
                rootPositionBoundariesResults,
                rootRotationBoundariesResults,
                currentBonesDatas,
                nextBonesDatas,
                nextPrediction, 
                rootNode,
                responsivenessDirections, 
                disabledRoot,
                isInertialized,
                wantApplyPositions,
                wantApplyScales,
                forceContinuousPose);
        }

        protected virtual void InitializeDistanceResults()
        {
            distanceResults = new NativeArray<DistanceResult>(QueryComputed.ranges.Count, Allocator.Persistent);
        }

        protected virtual void InitializeTransition(
            ref float delta, 
            ref MotionData motionData, 
            ref BlendBoundaries blendBoundaries,
            float4x4 rtsModelInverse,
            NativeArray<float3> rootPositionBoundariesResults,
            NativeArray<quaternion> rootRotationBoundariesResults,
            NativeArray<BoneData> currentBonesDatas,
            NativeArray<BoneData> nextBonesDatas,
            FuturePrediction nextPrediction,
            int rootNode,
            float responsivenessDirections, 
            bool disabledRoot,
            bool isInertialized,
            bool wantApplyPositions,
            bool wantApplyScales,
            bool isForcedContinuous = false)
        {
            delta -= dataset.poseStep;
            
            var animPoses = GetAnimationPoses();
            if (!disabledRoot)
            {
                PoseBlender.CalculateRootBoundariesMotion(
                    ref blendBoundaries,
                    rootPositionBoundariesResults,
                    rootRotationBoundariesResults,
                    animPoses,
                    QueryComputed.featuresData[currentFeatureID].animFrame,
                    Time.fixedDeltaTime,
                    root,
                    nextPrediction,
                    responsivenessDirections,
                    this is ActionQueryComputedFlow or IdleQueryComputedFlow,
                    isForcedContinuous
                );
            }

            var animPoseID = QueryComputed.featuresData[currentFeatureID].animFrame;
            
            nextBonesDatas.CopyFrom(animPoses.Count > animPoseID + 1
                ? animPoses[animPoseID + 1].bonesData
                : animPoses[animPoseID - 1].bonesData);
            currentBonesDatas.CopyFrom(animPoses[animPoseID].bonesData);
            
            var isLastPose = animPoses.Count <= animPoseID + 1;
            var batchCount = Math.Max(1, JobsUtility.JobWorkerCount / currentBoneTransformsValues.bonesCounter);
            new InitializeBoneTransitionsJob
            {
                bonesData = dataset.GetAnimationDataNativeFromFeature(currentFeatureID, QueryComputed).bonesData,
                isInertialized = isInertialized,
                isSearch = isSearch,
                isLastPose = isLastPose,
                currentBonesDatas = currentBonesDatas,
                nextBonesDatas = nextBonesDatas,
                currentPositions = currentBoneTransformsValues.positions,
                rootNode = rootNode,
                modelInverse = rtsModelInverse,
                motionData = motionData,
                boundaries = blendBoundaries,
                wantApplyPositions = wantApplyPositions,
                wantApplyScales = wantApplyScales
            }.Schedule(currentBoneTransformsValues.bonesCounter, batchCount).Complete();
        }

        protected void ManageWeights(int length)
        {
            bonesWeights = new BonesWeights
            {
                //This is necessary because of serialization things..
                weightFutureOffset = 0,
                weightFutureDirection = 0,
                weightPastOffset = 0,
                weightPastDirection = 0,
                weights = new NativeArray<float>(length * 2, Allocator.Persistent)
            };
            
            foreach (var tagID in QueryComputed.query)
            {
                CharacteristicsByTag mc = dataset.characteristics.characteristicsByTags.Find(mc => mc.id == tagID);
                List<BoneCharacteristic> characteristics = mc.characteristics;
                foreach (var characteristic in characteristics)
                {
                    bonesWeights.weights[characteristic.bone.id * 2] += characteristic.weightPosition; //Mean or higher?
                    bonesWeights.weights[characteristic.bone.id * 2 + 1] += characteristic.weightVelocity; //Mean or higher?
                }

                bonesWeights.weightFutureDirection  += mc.weightFutureDirection; //Mean or higher?
                bonesWeights.weightFutureOffset     += mc.weightFutureOffset; //Mean or higher? 
                bonesWeights.weightPastDirection    += mc.weightPastDirection;
                bonesWeights.weightPastOffset       += mc.weightPastOffset;
            }

            var totalTags = QueryComputed.query.Length;
            bonesWeights.weights.CopyFrom(bonesWeights.weights.Select(weight => weight / totalTags).ToArray()); //Mean or higher?
            bonesWeights.weightFutureOffset /= totalTags; //Mean or higher?
            bonesWeights.weightFutureDirection /= totalTags; //Mean or higher?
            bonesWeights.weightPastOffset /= totalTags; //Mean or higher?
            bonesWeights.weightPastDirection /= totalTags; //Mean or higher?
            bonesWeights.totalWeightPositions = bonesWeights.weights.Where((_, index) => index % 2 == 0).Sum();
            bonesWeights.totalWeightVelocities = bonesWeights.weights.Where((_, index) => index % 2 != 0).Sum();
        }

        public void GenerateNewPoseValues(
            ref NativeArray<OffsetBone> offsetsNative,
            ref NativeArray<float3> bonePositionResults,
            ref NativeArray<float3> boneScaleResults,
            ref NativeArray<quaternion> boneRotationResults,
            ref BlendingResults blendingResults,
            out float timeOnLastPositions,
            float4x4 rtsModelInverse,
            NativeArray<float3> currentPositions,
            NativeArray<float3> previousRootBasedPositions,
            NativeArray<quaternion> originalDiffRotations,
            quaternion rootRotation,
            BlendBoundaries blendBoundaries, 
            BlendingTypes blendingType,
            int bonesCount,
            int rootNode,
            float halfLife,
            bool disabledRoot,
            bool isInertialized,
            bool isBlendingActivated,
            bool wantApplyPositions,
            bool wantApplyScales)
        {
            if (!disabledRoot)
            {
                PoseBlender.BlendRoot(ref blendingResults, blendBoundaries, blendingType, elapsedTime, LerpDuration, isBlendingActivated);    
            }
            
            var batchCount = Math.Max(1, JobsUtility.JobWorkerCount / bonesCount);
            new CalculateBonesValuesJob()
            {
                blendBoundaries = blendBoundaries,
                isInertialized = isInertialized,
                isBlendingActivated = isBlendingActivated,
                isQueryDone = isQueryDone,
                wantApplyPositions = wantApplyPositions,
                wantApplyScales = wantApplyScales,
                currentPositions = currentPositions,
                originalDiffRotations = originalDiffRotations,
                rootRotation = rootRotation,
                modelInverse = rtsModelInverse,
                fixedDeltaTime = Time.fixedDeltaTime,
                halfLife = halfLife,
                blendingType = blendingType,
                elapsedTime = elapsedTime,
                lerpDuration = LerpDuration,
                previousRootBasedPositions = previousRootBasedPositions,
                blendingResults = blendingResults,
                offsetsNative = offsetsNative,
                positionResults = bonePositionResults,
                scaleResults = boneScaleResults,
                rotationResults = boneRotationResults,
                rootNode = rootNode
            }.Schedule(bonesCount, batchCount).Complete();
            
            timeOnLastPositions = Time.time;
        }

        public void SynchronizeTransforms(
            NativeArray<float3> bonePositionResults, 
            NativeArray<float3> boneScaleResults, 
            NativeArray<quaternion> boneRotationResults, 
            BlendingResults blendingResults, 
            CharacterControllerBase controller, 
            int rootNode,
            bool disabledRoot,
            bool wantApplyPositions,
            bool wantApplyScales
            )
        {
            if (!disabledRoot)
            {
                PoseSetter.SetRootPose(blendingResults, controller);    
            }

            new SetBonesJob
            {
                finalPositionsNative = bonePositionResults,
                finalScalesNative = boneScaleResults,
                finalRotationsNative = boneRotationResults,
                rootNode = rootNode,
                wantApplyPositions = wantApplyPositions,
                wantApplyScales = wantApplyScales
            }.Schedule(characterTransformsNative).Complete();
        }
        
        [BurstCompile]
        private struct CalculateBonesValuesJob : IJobParallelFor
        {
            [ReadOnly] public BlendBoundaries blendBoundaries;
            [ReadOnly] public bool isInertialized;
            [ReadOnly] public bool isBlendingActivated;
            [ReadOnly] public bool isQueryDone;
            [ReadOnly] public bool wantApplyPositions;
            [ReadOnly] public bool wantApplyScales;
            [ReadOnly] public NativeArray<float3> currentPositions;
            [ReadOnly] public float fixedDeltaTime;
            [ReadOnly] public float halfLife;
            [ReadOnly] public BlendingTypes blendingType;
            [ReadOnly] public float elapsedTime;
            [ReadOnly] public float lerpDuration;
            [ReadOnly] public float4x4 modelInverse;
            [ReadOnly] public NativeArray<quaternion> originalDiffRotations;
            [ReadOnly] public quaternion rootRotation;
            [ReadOnly] public int rootNode;
            
            public NativeArray<float3> previousRootBasedPositions;
            public BlendingResults blendingResults;
            public NativeArray<OffsetBone> offsetsNative;
            
            [WriteOnly] public NativeArray<float3> positionResults;
            [WriteOnly] public NativeArray<float3> scaleResults;
            [WriteOnly] public NativeArray<quaternion> rotationResults;

            public void Execute(int index)
            {
                PoseBlender.BlendBone(
                    ref blendingResults, 
                    index, 
                    rootNode,
                    blendBoundaries, 
                    blendingType, 
                    elapsedTime, 
                    lerpDuration,
                    wantApplyPositions,
                    wantApplyScales);

                if (isInertialized || isBlendingActivated)
                {
                    PoseFinder.UpdatePreviousRootBasedBonePosition(
                        ref previousRootBasedPositions, 
                        index, 
                        modelInverse,
                        currentPositions);
                }

                if (isQueryDone)
                {
                    return;
                }

                PoseSetter.GetNewBoneValues(
                    ref offsetsNative, 
                    index, blendingResults, 
                    positionResults, 
                    scaleResults,
                    rotationResults,
                    originalDiffRotations, 
                    rootRotation, 
                    rootNode, 
                    fixedDeltaTime, 
                    halfLife,
                    wantApplyPositions,
                    wantApplyScales);
            }
        }
        
        [BurstCompile]
        protected struct InitializeBoneTransitionsJob: IJobParallelFor
        {
            [ReadOnly] public NativeArray<BoneData> bonesData;
            [ReadOnly] public bool isInertialized;
            [ReadOnly] public bool isSearch;
            [ReadOnly] public bool isLastPose;
            [ReadOnly] public bool wantApplyPositions;
            [ReadOnly] public bool wantApplyScales;
            [ReadOnly] public NativeArray<BoneData> currentBonesDatas;
            [ReadOnly] public NativeArray<BoneData> nextBonesDatas;
            [ReadOnly] public NativeArray<float3> currentPositions;
            [ReadOnly] public float4x4 modelInverse;
            [ReadOnly] public int rootNode;
            
            public MotionData motionData;
            public BlendBoundaries boundaries;

            public void Execute(int index)
            {
                if (!currentBonesDatas[index].isValid)
                {
                    return;
                }
                if (isInertialized && isSearch)
                {
                    Inertialization.InitBoneTransition(ref motionData, index, bonesData, rootNode, wantApplyPositions, wantApplyScales);
                }

                PoseBlender.CalculateNextBonePositionJob(
                    ref boundaries, 
                    index, 
                    currentBonesDatas, 
                    nextBonesDatas,
                    rootNode,
                    wantApplyPositions,
                    wantApplyScales,
                    isLastPose);
                
                if (!isSearch)
                {
                    return;
                }
                
                PoseFinder.UpdatePreviousRootBasedBonePosition(
                    ref motionData.PreviousPositions, 
                    index, 
                    modelInverse,
                    currentPositions);
            }
        }

        protected virtual List<AnimationData> GetAnimationPoses()
        {
            return dataset.GetAnimationPoses(currentFeatureID, QueryComputed);
        }
        
        public virtual void Destroy()
        {
            bonesWeights.weights.Dispose();
            QueryComputed.Destroy();
            distanceResults.Dispose();
        }
        
        protected virtual void SetQueryComputed(QueryComputed queryComputed)
        {
            QueryComputed = queryComputed;
        }

        public List<FeatureData> GetFeatures()
        {
            return QueryComputed.featuresData;
        }
        
        public List<QueryRange> GetRanges()
        {
            return QueryComputed.GetRanges();
        }
        
        public virtual NativeArray<QueryRange> GetRangesNative()
        {
            return QueryComputed.GetRangesNative();
        }
        
        public QueryComputed GetQueryComputed()
        {
            return QueryComputed;
        }
    }
}
