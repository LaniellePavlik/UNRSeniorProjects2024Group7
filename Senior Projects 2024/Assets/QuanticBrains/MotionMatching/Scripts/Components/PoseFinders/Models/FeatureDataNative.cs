using Unity.Collections;
using Unity.Mathematics;

namespace QuanticBrains.MotionMatching.Scripts.Components.PoseFinders.Models
{
    public struct FeatureDataNative
    {
        public int animFrame;
        [NativeDisableParallelForRestriction]
        public NativeArray<float3> futureDirections;
        [NativeDisableParallelForRestriction]
        public NativeArray<float3> futureOffsets;
        [NativeDisableParallelForRestriction]
        public NativeArray<float3> pastDirections;
        [NativeDisableParallelForRestriction]
        public NativeArray<float3> pastOffsets;
        [NativeDisableParallelForRestriction]
        public NativeArray<float3> positionsAndVelocities;
    }
}
