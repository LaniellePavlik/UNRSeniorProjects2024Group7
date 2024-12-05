using System;
using QuanticBrains.MotionMatching.Scripts.Components.PoseFinders.Models;
using QuanticBrains.MotionMatching.Scripts.Components.QueryFlows;
using QuanticBrains.MotionMatching.Scripts.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Components.PoseFinders
{
    [Serializable]
    public abstract class PoseFinder
    {
        protected float Counter;
        public abstract int Find(
            ref NativeArray<float3> previousPositions,
            ref float timeOnLastPositions,
            out float currentMinDistance,
            float4x4 rtsModelInverse,
            PoseFinderGenericVariables poseFinderGenericVariables,
            NativeArray<float3> currentBonesPosition,
            int bonesCount,
            QueryComputedFlow queryComputedFlow,
            NativeArray<float3> futureOffsets, 
            NativeArray<float3> futureDirections,
            PastTrajectory pastTrajectory,
            NativeArray<DistanceResult> poseResult,
            bool wantDebugDistance,
            bool forceContinuousPose);

        public static void UpdatePreviousRootBasedBonePosition(
            ref NativeArray<float3> previousRootBasedPositions,
            int index,
            float4x4 modelInverse,
            NativeArray<float3> currentPositions)
        {
            if (math.isnan(currentPositions[index].x))
            {
                return;
            }
            previousRootBasedPositions[index] = MathUtils.TranslateToLocal(modelInverse, currentPositions[index]);
        }
        
        public virtual void ResetCounter(int searchRate)
        {
            Counter = searchRate;
        }
        
        protected void CreateCurrentFeatures(
            ref NativeArray<float3> previousPositions, 
            ref NativeArray<float3> currentFeatures,
            float4x4 rtsModelInverse,
            NativeArray<float3> currentBonesPosition,
            int bonesCount,
            NativeArray<float3> currentPositions,
            float timeOnLastPositions, 
            NativeArray<float3> meanFeatureVelocity, 
            NativeArray<float3> stdFeatureVelocity, 
            NativeArray<float3> meanFeaturePosition, 
            NativeArray<float3> stdFeaturePosition)
        {
            float time = (Time.time - timeOnLastPositions);
            var batchCount = Mathf.Max(1, JobsUtility.JobWorkerCount / bonesCount);
            new CreateCurrentFeaturesJob()
            {
                time = time,
                currentBonesPosition = currentBonesPosition,
                modelInverse = rtsModelInverse,
                currentPositions = currentPositions,
                previousPositions = previousPositions,
                meanFeatureVelocity = meanFeatureVelocity,
                stdFeatureVelocity = stdFeatureVelocity,
                meanFeaturePosition = meanFeaturePosition,
                stdFeaturePosition = stdFeaturePosition,
                currentFeatures = currentFeatures
            }.Schedule(currentPositions.Length, batchCount).Complete();
            currentPositions.CopyTo(previousPositions);
        }

        [BurstCompile]
        private struct CreateCurrentFeaturesJob: IJobParallelFor
        {
            [ReadOnly] public float time;
            [ReadOnly] public float4x4 modelInverse;
            [ReadOnly] public NativeArray<float3> currentBonesPosition;
            [ReadOnly] public NativeArray<float3> meanFeatureVelocity;
            [ReadOnly] public NativeArray<float3> stdFeatureVelocity;
            [ReadOnly] public NativeArray<float3> meanFeaturePosition;
            [ReadOnly] public NativeArray<float3> stdFeaturePosition;
            
            public NativeArray<float3> currentPositions;
            public NativeArray<float3> previousPositions;
            [NativeDisableParallelForRestriction]
            public NativeArray<float3> currentFeatures;
            public void Execute(int index)
            {
                int featureIndex = index * 2;
                GetBoneBasedPosition(ref currentPositions, index, modelInverse, currentBonesPosition);
                var currentVelocity = (currentPositions[index] - previousPositions[index]) / time;
                
                currentFeatures[featureIndex + 1] =
                    (currentVelocity - meanFeatureVelocity[index]) / stdFeatureVelocity[index];
                currentFeatures[featureIndex] = (currentPositions[index] - meanFeaturePosition[index]) / stdFeaturePosition[index];
            }
        }
        
        public static void GetRootBasedPositions(
            ref NativeArray<float3> rootBasedPositions, 
            NativeArray<float3> currentBonesPosition, 
            int bonesCount,
            float4x4 rtsModelInverse)
        {
            GetRootBasedPositionsJob getNormalizedPositionsJob = new GetRootBasedPositionsJob
            {
                modelInverse = rtsModelInverse,
                bonePositions = currentBonesPosition,
                rootBasedPositions = rootBasedPositions
            };
            var batchCount = Mathf.Max(1, JobsUtility.JobWorkerCount / bonesCount);
            getNormalizedPositionsJob.Schedule(bonesCount, batchCount)
                .Complete();
        }

        [BurstCompile]
        private struct GetRootBasedPositionsJob: IJobParallelFor
        {
            [ReadOnly] public NativeArray<float3> bonePositions;
            [ReadOnly] public float4x4 modelInverse;
            
            [NativeDisableParallelForRestriction]
            [WriteOnly] public NativeArray<float3> rootBasedPositions;
            
            public void Execute(int index)
            {
                GetBoneBasedPosition(ref rootBasedPositions, index, modelInverse, bonePositions);
            }
        }

        private static void GetBoneBasedPosition(
            ref NativeArray<float3> rootBasedPositions,
            int index, 
            float4x4 modelInverse,
            NativeArray<float3> bonePositions)
        {
            if (math.isnan(bonePositions[index].x))
            {
                return;
            }
            rootBasedPositions[index] = MathUtils.TranslateToLocal(modelInverse, bonePositions[index]);
        }
    }
}
