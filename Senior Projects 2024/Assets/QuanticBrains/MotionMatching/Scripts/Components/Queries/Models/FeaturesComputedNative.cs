using System.Collections.Generic;
using QuanticBrains.MotionMatching.Scripts.Containers;
using Unity.Collections;
using Unity.Mathematics;

namespace QuanticBrains.MotionMatching.Scripts.Components.Queries.Models
{
    public struct FeaturesComputedNative
    {
        //public NativeArray<QueryRange> ranges;
        public NativeArray<float3> meanFeaturePosition;
        public NativeArray<float3> stdFeaturePosition;
        public NativeArray<float3> meanFeatureVelocity;
        public NativeArray<float3> stdFeatureVelocity;
        
        public NativeArray<float3> meanFutureOffsets;
        public NativeArray<float3> stdFutureOffsets;
        public NativeArray<float3> meanFutureDirections;
        public NativeArray<float3> stdFutureDirections;
        
        public NativeArray<float3> meanPastOffsets;
        public NativeArray<float3> stdPastOffsets;
        public NativeArray<float3> meanPastDirections;
        public NativeArray<float3> stdPastDirections;
        
        public NativeArray<float3> featuresPositionsAndVelocities;
        public NativeArray<float3> featuresFutureOffsets;
        public NativeArray<float3> featuresFutureDirections;
        public NativeArray<float3> featuresPastOffsets;
        public NativeArray<float3> featuresPastDirections;
        public NativeArray<FeatureDataAnim> featureDataAnims;
        
        public FeaturesComputedNative GetQueryComputedNative(QueryComputed queryComputed)
        {
            if (meanFeaturePosition.IsCreated)
            {
                return this;
            }

            meanFeaturePosition =
                new NativeArray<float3>(queryComputed.meanFeaturePosition, Allocator.Persistent);
            stdFeaturePosition =
                new NativeArray<float3>(queryComputed.stdFeaturePosition, Allocator.Persistent);
            meanFeatureVelocity =
                new NativeArray<float3>(queryComputed.meanFeatureVelocity, Allocator.Persistent);
            stdFeatureVelocity =
                new NativeArray<float3>(queryComputed.stdFeatureVelocity, Allocator.Persistent);
            
            meanFutureOffsets =
                new NativeArray<float3>(queryComputed.meanFutureOffset, Allocator.Persistent);
            stdFutureOffsets =
                new NativeArray<float3>(queryComputed.stdFutureOffset, Allocator.Persistent);
            meanFutureDirections =
                new NativeArray<float3>(queryComputed.meanFutureDirection, Allocator.Persistent);
            stdFutureDirections =
                new NativeArray<float3>(queryComputed.stdFutureDirection, Allocator.Persistent);
            
            meanPastOffsets =
                new NativeArray<float3>(queryComputed.meanPastOffset, Allocator.Persistent);
            stdPastOffsets =
                new NativeArray<float3>(queryComputed.stdPastOffset, Allocator.Persistent);
            meanPastDirections =
                new NativeArray<float3>(queryComputed.meanPastDirection, Allocator.Persistent);
            stdPastDirections =
                new NativeArray<float3>(queryComputed.stdPastDirection, Allocator.Persistent);

            LoadFeatureDataAnims(queryComputed.featuresData);
            LoadFeaturesPositionsAndVelocities(queryComputed.featuresData);
            LoadFeaturesFutureOffsets(queryComputed.featuresData);
            LoadFeaturesFutureDirections(queryComputed.featuresData);
            LoadFeaturesPastOffsets(queryComputed.featuresData);
            LoadFeaturesPastDirections(queryComputed.featuresData);
            return this;
        }
        
        private void LoadFeatureDataAnims(List<FeatureData> featuresData)
        {
            featureDataAnims = new NativeArray<FeatureDataAnim>(featuresData.Count, Allocator.Persistent);
            for (int i = 0; i < featuresData.Count; i++)
            {
                featureDataAnims[i] = new FeatureDataAnim
                {
                    animFrame   = featuresData[i].animFrame,
                    animationID = featuresData[i].animationID,
                };
            }
        }

        private void LoadFeaturesPositionsAndVelocities(List<FeatureData> featuresData)
        {
            featuresPositionsAndVelocities = new NativeArray<float3>(featuresData[0].positionsAndVelocities.Length * featuresData.Count, Allocator.Persistent);
            var index = 0;
            for (int i = 0; i < featuresData.Count; i++)
            {
                foreach (var value in featuresData[i].positionsAndVelocities)
                {
                    featuresPositionsAndVelocities[index] = value;
                    index++;
                }
            }
        }
        
        private void LoadFeaturesFutureOffsets(List<FeatureData> featuresData)
        {
            featuresFutureOffsets = new NativeArray<float3>(featuresData[0].futureOffsets.Length * featuresData.Count, Allocator.Persistent);
            var index = 0;
            for (int i = 0; i < featuresData.Count; i++)
            {
                foreach (var value in featuresData[i].futureOffsets)
                {
                    featuresFutureOffsets[index] = value;
                    index++;
                }
            }
        }
        private void LoadFeaturesFutureDirections(List<FeatureData> featuresData)
        {
            featuresFutureDirections = new NativeArray<float3>(featuresData[0].futureDirections.Length * featuresData.Count, Allocator.Persistent);
            var index = 0;
            for (int i = 0; i < featuresData.Count; i++)
            {
                foreach (var value in featuresData[i].futureDirections)
                {
                    featuresFutureDirections[index] = value;
                    index++;
                }
            }
        }
        
        private void LoadFeaturesPastDirections(List<FeatureData> featuresData)
        {
            featuresPastDirections = new NativeArray<float3>(featuresData[0].pastDirections.Length * featuresData.Count, Allocator.Persistent);
            var index = 0;
            for (int i = 0; i < featuresData.Count; i++)
            {
                foreach (var value in featuresData[i].pastDirections)
                {
                    featuresPastDirections[index] = value;
                    index++;
                }
            }
        }

        private void LoadFeaturesPastOffsets(List<FeatureData> featuresData)
        {
            featuresPastOffsets = new NativeArray<float3>(featuresData[0].pastOffsets.Length * featuresData.Count, Allocator.Persistent);
            var index = 0;
            for (int i = 0; i < featuresData.Count; i++)
            {
                foreach (var value in featuresData[i].pastOffsets)
                {
                    featuresPastOffsets[index] = value;
                    index++;
                }
            }
        }

        public void Destroy()
        {
            if (!meanFeaturePosition.IsCreated)
            {
                return;
            }

            //ranges.Dispose();
            meanFeaturePosition.Dispose();
            stdFeaturePosition.Dispose();
            meanFeatureVelocity.Dispose();
            stdFeatureVelocity.Dispose();
            
            meanFutureOffsets.Dispose();
            stdFutureOffsets.Dispose();
            meanFutureDirections.Dispose();
            stdFutureDirections.Dispose();
            
            meanPastOffsets.Dispose();
            stdPastOffsets.Dispose();
            meanPastDirections.Dispose();
            stdPastDirections.Dispose();
            
            featuresPositionsAndVelocities.Dispose();
            featuresFutureOffsets.Dispose();
            featuresFutureDirections.Dispose();
            featuresPastOffsets.Dispose();
            featuresPastDirections.Dispose();
            featureDataAnims.Dispose();
        }
    }
}
