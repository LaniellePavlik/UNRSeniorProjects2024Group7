using Unity.Mathematics;

namespace QuanticBrains.MotionMatching.Scripts.Extensions
{
    public static class Float3Ex
    {
        public static readonly float3 Forward = new(0, 0, 1);
        public static readonly float3 Up = new(0, 1, 0);
        public static bool NearZero(this float3 value)
        {
            return math.abs(value.x) < 0.001 && math.abs(value.y) < 0.001 && math.abs(value.z) < 0.001;
        }

        public static float3 Sanitize(this float3 value)
        {
            if (math.isnan(value.x) || math.isinf(value.x)) 
                value.x = 0.0f;
            if (math.isnan(value.y) || math.isinf(value.y)) 
                value.y = 0.0f;
            if (math.isnan(value.z) || math.isinf(value.z)) 
                value.z = 0.0f;
            return value;
        }
    }
}
