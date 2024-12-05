using System;
using System.Collections;
using System.Collections.Generic;
using QuanticBrains.MotionMatching.Scripts.Extensions;
using QuanticBrains.MotionMatching.Scripts.Helpers;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Helpers
{
    public class ReorientableTargetHelper : BaseTargetHelper
    {
        private Vector3 _currentForward;
        
        protected override void Awake()
        {
            base.Awake();
            _currentForward = DefaultForward;
        }
        
        public override void UpdateCharacterBasedForward(Transform character)
        {
            //Default target helper doesn't handle reorientation
            var newForward = (transform.position - character.position);
            newForward.y = 0.0f;
            
            _currentForward = newForward.normalized;
        }

        public override TargetProperties GetTargetTransform()
        {
            var targetRot = Quaternion.LookRotation(_currentForward);
            return new TargetProperties(transform.position, targetRot);
        }

        protected override void DrawGizmos()
        {
            base.DrawGizmos();

            if (!Application.isPlaying) return;
            
            Gizmos.color = Color.magenta;
            GizmosEx.DrawArrow(transform.position + new Vector3(0.0f, 0.1f, 0.0f), _currentForward * 0.35f);
        }

        private void OnDrawGizmos()
        {
            DrawGizmos();
        }
    }
}
