using QuanticBrains.MotionMatching.Scripts.Components.QueryFlows.Implementations;
using QuanticBrains.MotionMatching.Scripts.Helpers;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Input.CharacterController
{
    [CreateAssetMenu(menuName = "MotionMatching/Character Controller/Interaction")]
    public class CharacterControllerInteraction : CharacterControllerBasicMotion
    {
        private BaseInteractionTargetHelper _interactionTarget;

        public override void Initialize(MonoBehaviour mb)
        {
            base.Initialize(mb);
            ((InputManagerInteraction)InputManagerBasicMotionInstantiated).SubscribeToInteractEvent(Interact);
        }

        public void Interact()
        {
            if (_interactionTarget == null) return;

            var interactionName = _interactionTarget.GetActionName();
            _interactionTarget.UpdateCharacterBasedForward(MotionMatching.transform);
            var target = _interactionTarget.isTarget
                ? _interactionTarget.GetTargetTransform()
                : (TargetProperties?)null;
            
            MotionMatching.SendActionQuery(interactionName, target, initSetup: CollisionsPhysicsSetup.Disabled,
                actionSetup: CollisionsPhysicsSetup.Disabled, recoverySetup: CollisionsPhysicsSetup.Disabled);
        }

        protected override void ManageIdle()
        {
            if (IsStopped)
            {
                return;
            }

            if (TimeFromTryStopped == 0f)
            {
                TimeFromTryStopped = Time.time;
                return;
            }

            if (Time.time - TimeFromTryStopped < IdleDelay)
            {
                return;
            }
            
            TimeFromTryStopped = 0f;
            InputManagerBasicMotionInstantiated.reCheckMovement = true;
            IsStopped = MotionMatching.SendIdleQuery();
        }

        public override void OnTriggerEnter(Collider other)
        {
            var newTarget = other.GetComponent<BaseTargetHelper>();
            if (!newTarget) return;

            if (newTarget is BaseInteractionTargetHelper newInteractionTarget)
            {
                _interactionTarget = newInteractionTarget;
                return;
            }

            ActionTarget = newTarget;
        }

        public override void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<BaseInteractionTargetHelper>() == _interactionTarget)
            {
                _interactionTarget = null;
            }

            base.OnTriggerExit(other);
        }
    }
}
