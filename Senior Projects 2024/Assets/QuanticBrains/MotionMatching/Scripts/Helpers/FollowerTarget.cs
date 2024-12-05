using System.Collections.Generic;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Helpers
{
    public class FollowerTarget : AITarget
    {
        [SerializeField] private Transform dynamicTarget;

        public override Vector3 GetTargetPosition()
        {
            return dynamicTarget.position;
        }
    
        public override float GetCurrentDistance(Vector3 currentPosition)
        {
            return (dynamicTarget.position - currentPosition).magnitude;
        }
    }
}
