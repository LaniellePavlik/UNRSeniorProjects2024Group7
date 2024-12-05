using QuanticBrains.MotionMatching.Scripts.Components.QueryFlows.Implementations;
using QuanticBrains.MotionMatching.Scripts.Extensions;
using QuanticBrains.MotionMatching.Scripts.Helpers;
using Unity.Mathematics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace QuanticBrains.MotionMatching.Scripts.Input.CharacterController
{
    [CreateAssetMenu(menuName = "MotionMatching/Character Controller/Basic Motion")]
    public class CharacterControllerBasicMotion : CharacterControllerBase
    {
        [Header("Input manager")] public InputManagerBasicMotion inputManagerBasicMotion;
        protected InputManagerBasicMotion InputManagerBasicMotionInstantiated;
        private UnityEngine.CharacterController _characterController;
        protected bool IsStopped;
        private Vector3 _lastGlobalPosition;
        private Vector3 _lastGlobalRotation;
        protected Vector3 Velocity;
        private Camera _camera;

#if UNITY_2022_2_OR_NEWER
        [Header("Character Controller Exclude Layers")] [Tooltip("Exclude layer by default on character controller")]
        public LayerMask defaultExcludeLayer;

        [Tooltip("Exclude layer used when disabling physics while using actions")]
        public LayerMask actionExcludeLayer;
#endif

        //Jumps
        protected BaseTargetHelper ActionTarget;
        private string _defaultJumpMoveAction = "JumpFront";
        private string _defaultJumpIPCAction = "JumpIPC";

        //Falling
        private string _defaultFallingActionLow = "FallingLow";
        private string _defaultFallingActionHigh = "FallingHigh";
        protected bool IsGrounded = true;
        protected bool IsFalling;

        protected float TimeFromTryFalling;

        [Tooltip("Time to wait before triggering falling action when not grounded")]
        public float fallingDelay = 0.25f;

        [Tooltip("Min velocity to trigger falling")]
        public float fallingVelThreshold = -0.01f;

        [Tooltip("Min height to trigger falling")]
        public float minFallingHeight = 0.5f;

        [Tooltip("Height threshold to trigger each falling type action (low or high)")]
        public float fallingTypeThreshold = 2f;

        //Idle
        protected float TimeFromTryStopped;
        protected const float IdleDelay = 0.1f;

        public override void Initialize(MonoBehaviour mb)
        {
            base.Initialize(mb);

            InputManagerBasicMotionInstantiated = Instantiate(inputManagerBasicMotion).Also(im => im.Initialize(mb));
            _characterController = mb.GetComponent<UnityEngine.CharacterController>();
            _lastGlobalPosition = _characterController.transform.position;
            _camera = Camera.main;

            InputManagerBasicMotionInstantiated.SubscribeToJumpEvent(Jump);
        }

        protected override Vector2 GetRawInput()
        {
            return InputManagerBasicMotionInstantiated.GetRawInput();
        }

        protected override Vector3 GetForward(Vector3 input)
        {
            if (!IsStrafing())
            {
                var forward = new float3(input.x, 0.0f, input.z);
                return math.all(forward == float3.zero) ? new float3(0, 0, 1) : forward;
            }

            var camForward = _camera.transform.forward;
            return new Vector3(camForward.x, 0, camForward.z).normalized;
        }

        public override void Move(float3 position, quaternion rotation, float time)
        {
            var diffPosition = position - (float3)_lastGlobalPosition;
            if (physicsEnabled)
            {
                Velocity.y += Physics.gravity.y * time * time;
                diffPosition.y = Velocity.y;
            }

            _characterController.Move(diffPosition);
            var charTransform = _characterController.transform;
            charTransform.rotation = rotation;
            _lastGlobalPosition = charTransform.position;
        }

        public override void UpdateMotion(float time)
        {
            base.UpdateMotion(time);

            IsGrounded = _characterController.isGrounded;

            if (CurrentRawInput != Vector2.zero)
            {
                ManageMovement();
            }

            if (CurrentRawInput == Vector2.zero)
            {
                ManageIdle();
            }

            ManageFalling();
        }

        private void EndFalling()
        {
            IsFalling = false;
            MotionMatching.EndLoopQuery();
        }

        public virtual void Jump()
        {
            if (!IsGrounded) return;

            if (ActionTarget != null)
            {
                var dot = Vector3.Dot(ActionTarget.GetTargetForward(), MotionMatching.transform.forward);
                if (dot > ActionTarget.GetDotTolerance())
                {
                    var actionName = ActionTarget.GetActionName();

                    ActionTarget.UpdateCharacterBasedForward(MotionMatching.transform);
                    var target = ActionTarget.isTarget
                        ? ActionTarget.GetTargetTransform()
                        : (TargetProperties?)null;

                    MotionMatching.SendActionQuery(actionName, target,
                        actionSetup: CollisionsPhysicsSetup.Disabled, recoverySetup: CollisionsPhysicsSetup.Disabled);
                    return;
                }
            }

            if (CurrentRawInput == Vector2.zero)
            {
                MotionMatching.SendActionQuery(_defaultJumpIPCAction,
                    actionSetup: CollisionsPhysicsSetup.CollisionsEnabled);
                return;
            }

            MotionMatching.SendActionQuery(_defaultJumpMoveAction,
                actionSetup: CollisionsPhysicsSetup.CollisionsEnabled);
        }

        protected override void ToggleCollisions(bool isEnabled)
        {
            if (isEnabled)
            {
#if UNITY_2022_2_OR_NEWER
                _characterController.excludeLayers = defaultExcludeLayer;
#else
                _characterController.detectCollisions = true;
#endif
                return;
            }
#if UNITY_2022_2_OR_NEWER
            _characterController.excludeLayers = actionExcludeLayer;
#else
            _characterController.detectCollisions = false;
#endif
        }

        protected override void TogglePhysics(bool isEnabled)
        {
        }

        protected virtual void ManageIdle()
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
            IsStopped = MotionMatching.SendIdleQuery();
        }

        private void ManageFalling()
        {
            //If grounded, reset timer and set velocity to zero
            if (IsGrounded && Velocity.y < 0.0f)
            {
                TimeFromTryFalling = 0f;

                if (IsFalling)
                    EndFalling();

                Velocity.y = 0f;
                return;
            }

            //If not grounded and moving down more than threshold, start checking falling
            if (!IsGrounded)
            {
                CheckIfFalling();
            }
        }

        protected virtual void CheckIfFalling()
        {
            if (IsFalling) return;

            if (TimeFromTryFalling == 0f)
            {
                TimeFromTryFalling = Time.time;
                return;
            }

            if (Time.time - TimeFromTryFalling < fallingDelay)
            {
                return;
            }

            if (Velocity.y >= fallingVelThreshold)
                return;

            var fallingAction = _defaultFallingActionLow;
            if (Physics.Raycast(MotionMatching.transform.position, Vector3.down, out var hit, Mathf.Infinity))
            {
                Debug.DrawRay(MotionMatching.transform.position, Vector3.down * hit.distance, Color.yellow);
                if (hit.distance < minFallingHeight) return;

                fallingAction =
                    hit.distance < fallingTypeThreshold ? _defaultFallingActionLow : _defaultFallingActionHigh;
            }

            //Call Falling action after selected delay - keep calling until SendAction returns true
            IsFalling = MotionMatching.SendActionQuery(fallingAction);

            if (IsFalling) TimeFromTryFalling = 0f;
        }

        public override bool IsStrafing()
        {
            return InputManagerBasicMotionInstantiated.isStrafing;
        }

        protected virtual void ManageMovement()
        {
            TimeFromTryStopped = 0f;
            if (!IsStopped && !InputManagerBasicMotionInstantiated.reCheckMovement)
            {
                return;
            }

            IsStopped = false;
            InputManagerBasicMotionInstantiated.reCheckMovement = false;

            if (InputManagerBasicMotionInstantiated.isStrafing)
            {
                MotionMatching.SendQuery("Strafing", "Walk");
                return;
            }

            if (InputManagerBasicMotionInstantiated.isRunning)
            {
                MotionMatching.SendQuery("Run");
                return;
            }

            MotionMatching.SendQuery("Walk");
        }

        public override void OnTriggerEnter(Collider other)
        {
            var newTarget = other.GetComponent<BaseTargetHelper>();
            if (newTarget)
                ActionTarget = newTarget;
        }

        public override void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<BaseTargetHelper>() == ActionTarget)
            {
                ActionTarget = null;
            }
        }

        private void OnDestroy()
        {
            InputManagerBasicMotionInstantiated.Destroy();
        }
    }
}
