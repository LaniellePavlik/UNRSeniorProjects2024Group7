using QuanticBrains.MotionMatching.Scripts.Extensions;
using Unity.Mathematics;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Helpers
{
    public class BaseTargetHelper : MonoBehaviour
    {
        [Header("Jump Target Setup")]
        [Tooltip("The action name toggled near this obstacle")]
        [SerializeField] private string actionName;

        [Tooltip("Whether this gameobject must be used as target or not")]
        public bool isTarget = true;
        protected Vector3 DefaultForward;
        
        [Tooltip("The dot tolerance [cos(angle)] threshold from which this action can be triggered")]
        [Range(-1f, 1f)]
        public float dotTolerance = 0.6f;

        protected virtual void Awake()
        {
            //Used for comparison threshold
            DefaultForward = transform.forward;
        }
        
        public Vector3 GetTargetForward()
        {
            return DefaultForward;
        }

        public virtual TargetProperties GetTargetTransform()
        {
            return new TargetProperties(transform.position, transform.rotation);
        }

        public virtual void UpdateCharacterBasedForward(Transform character)
        {
            //Default target helper doesn't handle reorientation
        }

        public string GetActionName()
        {
            return actionName;
        }
        
        public float GetDotTolerance()
        {
            return dotTolerance;
        }

        protected virtual void DrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.1f);
        
            Gizmos.color = Color.red;
            GizmosEx.DrawArrow(transform.position + new Vector3(0.0f, 0.1f, 0.0f), transform.forward * 0.5f);
        }
        
        private void OnDrawGizmos()
        {
            DrawGizmos();
        }
    }

    public struct TargetProperties
    {
        public float3 Position;
        public quaternion Rotation;

        public TargetProperties(float3 pos, quaternion rot)
        {
            Position = pos;
            Rotation = rot;
        }
    }
}
