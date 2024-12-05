using Unity.Collections;
using Unity.Mathematics;

namespace QuanticBrains.MotionMatching.Scripts.Components.PoseBlenders.Models
{
    public struct BlendBoundaries
    {
        public NativeArray<quaternion> startRotationValues;
        public NativeArray<quaternion> endRotationValues;
        public NativeArray<float3> startPositionValues;
        public NativeArray<float3> endPositionValues;
        public NativeArray<float3> startScaleValues;
        public NativeArray<float3> endScaleValues;
        public float3 startRootPositionToBlend;
        public float3 endRootPositionToBlend;
        public quaternion startRootRotationToBlend;
        public quaternion endRootRotationToBlend;
    }
}
