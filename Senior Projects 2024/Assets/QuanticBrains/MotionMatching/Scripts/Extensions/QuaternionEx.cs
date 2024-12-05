using Unity.Mathematics;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Extensions
{
    public static class QuaternionEx
    {
        public static bool NearTo(this Quaternion value, Quaternion target)
        {
            var angle = Quaternion.Angle(value, target);
            return angle < 0.00001;
        }
        
        public static quaternion Diff(this quaternion to, quaternion from)
        {
            return math.mul(to, math.inverse(from));
        }
        
        public static quaternion Add(this quaternion start, quaternion diff)
        {
            return math.mul(diff, start);
        }

        public static float3 GetDirection(this quaternion q)
        {
            return math.mul(q, Float3Ex.Forward);
        }

        public static quaternion AddAngularVelocity(this quaternion q, float3 angularVelocity, float time)
        {
            if (math.isnan(angularVelocity.x))
            {
                return q;
            }
            
            float3 euler = ((Quaternion)q).eulerAngles;
            var anglesSpace = angularVelocity * time;
            euler += anglesSpace;
            return Quaternion.Euler(euler);
        }
    }
}
