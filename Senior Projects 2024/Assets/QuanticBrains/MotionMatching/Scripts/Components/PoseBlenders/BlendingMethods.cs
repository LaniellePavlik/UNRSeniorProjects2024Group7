using Unity.Mathematics;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Components.PoseBlenders
{
    public static class BlendingMethods
    {
        public static quaternion Lerp(quaternion startValue, quaternion endValue, float elapsedTime, float lerpDuration)
        {
            return math.nlerp(startValue, endValue, elapsedTime / lerpDuration);
        }

        public static float3 Lerp(float3 startValue, float3 endValue, float elapsedTime, float lerpDuration)
        {
            return math.lerp(startValue, endValue, elapsedTime / lerpDuration);
        }
        
        public static quaternion SLerp(quaternion startValue, quaternion endValue, float elapsedTime, float lerpDuration)
        {
            return math.slerp(startValue, endValue, elapsedTime / lerpDuration);
        }

        public static float3 SLerp(float3 startValue, float3 endValue, float elapsedTime, float lerpDuration)
        {
            return Vector3.Slerp(startValue, endValue, elapsedTime / lerpDuration);
        }
    }
}
