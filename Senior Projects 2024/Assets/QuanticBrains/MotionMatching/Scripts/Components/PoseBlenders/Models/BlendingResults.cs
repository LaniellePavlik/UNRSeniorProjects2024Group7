using Unity.Collections;
using Unity.Mathematics;

namespace QuanticBrains.MotionMatching.Scripts.Components.PoseBlenders.Models
{
    public struct BlendingResults
    {
        public NativeArray<float3> bonesPosition;
        public NativeArray<float3> bonesScale;
        public NativeArray<quaternion> bonesRotation;
        public quaternion rootRotation;
        public float3 rootPosition;
    }
}
