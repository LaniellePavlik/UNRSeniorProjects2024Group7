using System;
using System.Collections.Generic;
using System.Linq;
using QuanticBrains.MotionMatching.Scripts.Components.Queries.Models;
using QuanticBrains.MotionMatching.Scripts.Containers;
using QuanticBrains.MotionMatching.Scripts.CustomAttributes;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Components.Queries
{
    [Serializable]
    public abstract class QueryComputed
    {
        [ShowOnly]
        public string[] query;
        public List<QueryRange> ranges;

        public float forwardSpeed;
        public float backwardSpeed;
        public float sideSpeed;
        
        [HideInInspector] public List<FeatureData> featuresData;
        [HideInInspector] public float3[] meanFeaturePosition;
        [HideInInspector] public float3[] stdFeaturePosition;
        [HideInInspector] public float3[] meanFeatureVelocity;
        [HideInInspector] public float3[] stdFeatureVelocity;
        [HideInInspector] public float3[] meanFutureOffset;
        [HideInInspector] public float3[] stdFutureOffset;
        [HideInInspector] public float3[] meanFutureDirection;
        [HideInInspector] public float3[] stdFutureDirection;
        [HideInInspector] public float3[] meanPastOffset;
        [HideInInspector] public float3[] stdPastOffset;
        [HideInInspector] public float3[] meanPastDirection;
        [HideInInspector] public float3[] stdPastDirection;
        
        private FeaturesComputedNative _featuresComputedNative;
        private NativeArray<QueryRange> _ranges;

        protected QueryComputed(int fEstimates, int pEstimates, int nBones)
        {
            meanFeaturePosition = new float3[nBones];
            stdFeaturePosition = new float3[nBones];
            meanFeatureVelocity = new float3[nBones];
            stdFeatureVelocity = new float3[nBones];
            
            meanFutureOffset    = new float3[fEstimates];
            meanFutureDirection = new float3[fEstimates];
            stdFutureOffset     = new float3[fEstimates];
            stdFutureDirection  = new float3[fEstimates];
            
            meanPastOffset      = new float3[pEstimates];
            meanPastDirection   = new float3[pEstimates];
            stdPastOffset       = new float3[pEstimates];
            stdPastDirection    = new float3[pEstimates];
            featuresData = new List<FeatureData>();
        }

        public virtual List<QueryRange> GetRanges()
        {
            return ranges;
        }
        
        public virtual FeaturesComputedNative GetFeaturesQueryComputedNative()
        {
            return _featuresComputedNative.GetQueryComputedNative(this);
        }
        
        public NativeArray<QueryRange> GetRangesNative()
        {
            if (_ranges.IsCreated)
            {
                return _ranges;
            }

            _ranges = new NativeArray<QueryRange>(ranges.ToArray(), Allocator.Persistent);
            return _ranges;
        }

        public virtual void Destroy()
        {
            _featuresComputedNative.Destroy();

            if (!_ranges.IsCreated)
            {
                return;
            }

            _ranges.Dispose();
        }
    }
}
