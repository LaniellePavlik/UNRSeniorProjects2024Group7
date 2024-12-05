using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Helpers
{
    public abstract class AITarget : MonoBehaviour
    {
        public abstract Vector3 GetTargetPosition();
        public abstract float GetCurrentDistance(Vector3 currentPosition);
    }
}
