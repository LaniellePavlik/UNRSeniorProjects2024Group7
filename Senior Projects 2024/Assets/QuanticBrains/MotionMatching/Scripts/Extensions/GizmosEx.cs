using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Extensions
{
    public static class GizmosEx
    {
        public static void DrawArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            if (direction == Vector3.zero) return;
            
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction.normalized) *
                            Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction.normalized) *
                           Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }
    }
}
