using Unity.Collections;
using Unity.Mathematics;

namespace QuanticBrains.MotionMatching.Scripts.Components.PoseFinders.Models
{
    public struct PoseFinderGenericVariables
    {
        public NativeArray<float3> normalizedFutureDirections;
        public NativeArray<float3> normalizedFutureOffsets;
        public NativeArray<float3> normalizedPastDirections;
        public NativeArray<float3> normalizedPastOffsets;
        public NativeArray<float3> currentPositions;
        public NativeArray<float3> currentFeatures;

        public void Create(int futureEstimates, int pastEstimates, int bonesLength)
        {
            normalizedFutureDirections = new NativeArray<float3>(futureEstimates, Allocator.Persistent);
            normalizedFutureOffsets = new NativeArray<float3>(futureEstimates, Allocator.Persistent);
            normalizedPastDirections = new NativeArray<float3>(pastEstimates, Allocator.Persistent);
            normalizedPastOffsets = new NativeArray<float3>(pastEstimates, Allocator.Persistent);
            currentPositions = new NativeArray<float3>(bonesLength, Allocator.Persistent);
            currentFeatures = new NativeArray<float3>(bonesLength * 2, Allocator.Persistent);
        }
        
        public void Destroy()
        {
            normalizedFutureDirections.Dispose();
            normalizedFutureOffsets.Dispose();
            normalizedPastDirections.Dispose();
            normalizedPastOffsets.Dispose();
            currentPositions.Dispose();
            currentFeatures.Dispose();
        }
    }
}
