using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace QuanticBrains.MotionMatching.Scripts.Input
{
    [CreateAssetMenu(menuName = "MotionMatching/Input Manager/Interaction")]
    public class InputManagerInteraction : InputManagerBasicMotion
    {
        private UnityAction _interactAction;
        
        public override void Initialize(MonoBehaviour mm)
        {
            base.Initialize(mm);

            _interactAction = null;
            Input.Player.Interact.performed += OnInteract;
            
            //ToDo: check if i need to enable after performed
        }
        
        private void OnInteract(InputAction.CallbackContext context)
        {
            _interactAction.Invoke();
        }
        
        public void SubscribeToInteractEvent(UnityAction action)
        {
            _interactAction += action;
        }
    }
}
