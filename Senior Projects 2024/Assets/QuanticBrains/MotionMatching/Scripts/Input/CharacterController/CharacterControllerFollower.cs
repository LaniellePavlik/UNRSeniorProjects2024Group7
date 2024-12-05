using System.Collections;
using QuanticBrains.MotionMatching.Scripts.Components.QueryFlows.Implementations;
using QuanticBrains.MotionMatching.Scripts.Helpers;
using QuanticBrains.MotionMatching.Scripts.Input.CustomInputs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Input.CharacterController
{
    [CreateAssetMenu(menuName = "MotionMatching/Character Controller/Follower AI Motion")]
    public class CharacterControllerFollower : CharacterControllerBase
    {
        [SerializeField] private float runDistanceThreshold = 2.5f;
        [SerializeField] private float stopDistanceThreshold = 0.1f;
        private FollowerTarget _followerTarget;


        private UnityEngine.CharacterController _characterController;
        private Transform _characterTransform;
        private bool _isStopped;
        private Vector3 _lastGlobalPosition;
        private Vector3 _velocity;

#if UNITY_2022_2_OR_NEWER
        [Header("Character Controller Exclude Layers")] [Tooltip("Exclude layer by default on character controller")]
        public LayerMask defaultExcludeLayer;

        [Tooltip("Exclude layer used when disabling physics while using actions")]
        public LayerMask actionExcludeLayer;
#endif

        private BaseTargetHelper _actionTarget;

        //Falling
        private string _defaultFallingActionLow = "FallingLow";
        private string _defaultFallingActionHigh = "FallingHigh";
        private bool _isGrounded = true;
        private bool _isFalling;

        private float _timeFromTryFalling;

        [Tooltip("Time to wait before triggering falling action when not grounded")]
        public float fallingDelay = 0.25f;

        [Tooltip("Min velocity to trigger falling")]
        public float fallingVelThreshold = -0.01f;

        [Tooltip("Min height to trigger falling")]
        public float minFallingHeight = 0.5f;

        [Tooltip("Height threshold to trigger each falling type action (low or high)")]
        public float fallingTypeThreshold = 2f;

        public override void Initialize(MonoBehaviour mb)
        {
            base.Initialize(mb);

            _characterController = mb.GetComponent<UnityEngine.CharacterController>();
            _characterTransform = mb.transform;
            _lastGlobalPosition = _characterController.transform.position;

            _followerTarget = mb.GetComponent<FollowerTarget>();

            CheckAssertConditions();
        }

        private void CheckAssertConditions()
        {
            if (!_followerTarget)
            {
                Debug.LogError(
                    $"<color=red>ERROR. WanderTarget script must be added to AI Motion Matching Character object</color>");

#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
                Application.Quit();
#endif
            }

            if (customInput is not WorldBasedInput)
            {
                Debug.LogError($"<color=red>Custom input ERROR. AI Controller must use World Based Input</color>");

#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
                Application.Quit();
#endif
            }
        }

        protected override Vector2 GetRawInput()
        {
            var dir = (_followerTarget.GetTargetPosition() - MotionMatching.transform.position);

            if (dir.magnitude > stopDistanceThreshold) _isStopped = false;

            return _isStopped ? Vector2.zero : new Vector2(dir.x, dir.z).normalized;
        }

        protected override Vector3 GetForward(Vector3 input)
        {
            return new float3(input.x, 0.0f, input.z);
        }

        public override void Move(float3 position, quaternion rotation, float time)
        {
            var diffPosition = position - (float3)_lastGlobalPosition;
            if (physicsEnabled)
            {
                _velocity.y += Physics.gravity.y * time * time;
                diffPosition.y = _velocity.y;
            }

            _characterController.Move(diffPosition);
            _characterTransform.rotation = rotation;
            _lastGlobalPosition = _characterTransform.position;
        }

        public override void UpdateMotion(float time)
        {
            base.UpdateMotion(time);

            _isGrounded = _characterController.isGrounded;

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

        private void ManageFalling()
        {
            //If grounded, reset timer and set velocity to zero
            if (_isGrounded && _velocity.y < 0.0f)
            {
                _timeFromTryFalling = 0f;

                if (_isFalling)
                    EndFalling();

                _velocity.y = 0f;
                return;
            }

            //If not grounded and moving down more than threshold, start checking falling
            if (!_isGrounded)
            {
                CheckIfFalling();
            }
        }

        private void EndFalling()
        {
            _isFalling = false;
            MotionMatching.EndLoopQuery();
        }

        private void CheckIfFalling()
        {
            if (_isFalling) return;

            if (_timeFromTryFalling == 0f)
            {
                _timeFromTryFalling = Time.time;
                return;
            }

            if (Time.time - _timeFromTryFalling < fallingDelay)
            {
                return;
            }

            if (_velocity.y >= fallingVelThreshold)
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
            _isFalling = MotionMatching.SendActionQuery(fallingAction);

            if (_isFalling) _timeFromTryFalling = 0f;
        }

        private bool Jump(BaseTargetHelper currentTarget)
        {
            var dot = Vector3.Dot(currentTarget.GetTargetForward(), MotionMatching.transform.forward);
            if (!(dot > _actionTarget.GetDotTolerance())) return false;

            var actionName = currentTarget.GetActionName();

            currentTarget.UpdateCharacterBasedForward(MotionMatching.transform);
            var target = currentTarget.isTarget ? currentTarget.GetTargetTransform() : (TargetProperties?)null;

            MotionMatching.SendActionQuery(actionName, target,
                actionSetup: CollisionsPhysicsSetup.Disabled, recoverySetup: CollisionsPhysicsSetup.Disabled);
            return true;
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

        private void ManageIdle()
        {
            if (MotionMatching.currentPlayedQuery[0] == "Idle") return;

            if (MotionMatching.currentPlayedQuery[0] != "Walk" &&
                _followerTarget.GetCurrentDistance(MotionMatching.transform.position) < stopDistanceThreshold)
            {
                MotionMatching.SendQuery("Walk");
                return;
            }

            MotionMatching.SendIdleQuery();
        }

        private void ManageMovement()
        {
            var dist = _followerTarget.GetCurrentDistance(MotionMatching.transform.position);

            if (dist < stopDistanceThreshold)
            {
                if (_isStopped) return;

                MotionMatching.SendIdleQuery();
                _isStopped = true;
                return;
            }

            _isStopped = false;
            var query = dist > runDistanceThreshold ? "Run" : "Walk";

            if (query != MotionMatching.currentPlayedQuery[0])
            {
                MotionMatching.SendQuery(query);
            }
        }

        private void CheckForJump(Collider other)
        {
            _actionTarget = other.GetComponent<BaseTargetHelper>();
            if (!_actionTarget) return;
            if (_actionTarget is BaseInteractionTargetHelper) return;

            var distToTarget = _followerTarget.GetCurrentDistance(MotionMatching.transform.position);
            if (distToTarget < stopDistanceThreshold)
            {
                _actionTarget = null;
                return;
            }

            var jump = Jump(_actionTarget);
            if (!jump) _actionTarget = null;
        }

        public override void OnTriggerEnter(Collider other)
        {
            CheckForJump(other);
        }

        public override void OnTriggerStay(Collider other)
        {
            if (_actionTarget) return;

            CheckForJump(other);
        }

        public override void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<BaseTargetHelper>() == _actionTarget)
            {
                _actionTarget = null;
            }
        }
    }
}
