using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

//Basic Input Manager, can be modified for adding new approaches like jump
namespace QuanticBrains.MotionMatching.Scripts.Input
{
    [CreateAssetMenu(menuName = "MotionMatching/Input Manager/Basic Motion")]
    public class InputManagerBasicMotion : ScriptableObject
    {
        protected MotionInput Input;
    
        //Movement direction
        public bool isRunning;
        public bool isStrafing;
        public bool reCheckMovement;
        
        private float _currentValueX;
        private float _currentValueY;
        private int _lastDirectionX;
        private int _lastDirectionY;

        private UnityAction _jumpAction;

        public virtual void Initialize(MonoBehaviour mm)
        {
            isRunning = false;
            isStrafing = false;
            reCheckMovement = false;
            Input = new MotionInput();
            Input.Player.Run.performed += OnRun;
            Input.Player.Run.canceled += OnRun;

            Input.Player.Strafing.performed += OnStrafe;
            Input.Player.Strafing.canceled += OnStrafe;

            _jumpAction = null;
            Input.Player.Jump.performed += OnJump;

            Input.Enable();
        }

        private void OnStrafe(InputAction.CallbackContext context)
        {
            reCheckMovement = true;
            isStrafing = !isStrafing;
        }
        
        private void OnRun(InputAction.CallbackContext context)
        {
            reCheckMovement = true;
            isRunning = !isRunning;
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            _jumpAction.Invoke();
        }

        public Vector2 GetRawInput()
        {
            return Input.Player.Move.ReadValue<Vector2>();
        }

        public void SubscribeToJumpEvent(UnityAction action)
        {
            _jumpAction += action;
        }

        public void Destroy()
        {
            Input.Disable();
        }
    }
}
