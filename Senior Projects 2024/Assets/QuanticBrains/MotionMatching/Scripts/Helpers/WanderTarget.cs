using System.Collections.Generic;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Helpers
{
    public class WanderTarget : AITarget
    {
        [SerializeField] private List<Transform> targetsList;
        private Vector3 _currentTarget;

        void Start()
        {
            SelectRandomTarget();
        }
    
        public void SelectRandomTarget()
        {
            Vector3 newTarget = _currentTarget;
            while (newTarget == _currentTarget)
            {
                newTarget = targetsList[Random.Range(0, targetsList.Count)].transform.position;
            }
            _currentTarget = newTarget;
        }

        public override Vector3 GetTargetPosition()
        {
            return _currentTarget;
        }
    
        public override float GetCurrentDistance(Vector3 currentPosition)
        {
            return (_currentTarget - currentPosition).magnitude;
        }
    }
}
