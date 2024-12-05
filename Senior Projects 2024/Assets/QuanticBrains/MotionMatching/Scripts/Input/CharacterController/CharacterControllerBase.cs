using QuanticBrains.MotionMatching.Scripts.Input.CustomInputs;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace QuanticBrains.MotionMatching.Scripts.Input.CharacterController
{
    public abstract class CharacterControllerBase: ScriptableObject
    {
        [Header("Custom input method")] [Tooltip("GameObject with your custom input implementation")]
        public InputCustomizable customInput;
        [HideInInspector]
        public float3 currentMoveInput;
        protected Vector2 CurrentRawInput;
        [HideInInspector]
        public float3 currentForward;
        protected MotionMatching MotionMatching;
        
        //Collisions and physics
        [HideInInspector]
        public bool collisionsEnabled = true;
        [HideInInspector]
        public bool physicsEnabled = true;
        

        public virtual void Initialize(MonoBehaviour mb)
        {
            if (!customInput)
            {
                Debug.Log("Custom input ERROR. When using the CustomUserInput input type, it is needed to select an IInputCustomizable object which handles it");
                
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
                Application.Quit();
#endif
            }

            MotionMatching = (MotionMatching)mb;
        }

        protected abstract Vector2 GetRawInput();
        protected abstract Vector3 GetForward(Vector3 input);

        protected void ProcessInput()
        {
            currentMoveInput = customInput.HandleCustomInput(CurrentRawInput, MotionMatching.transform);
        }
        
        public abstract void Move(float3 position, quaternion rotation, float time);

        public virtual void UpdateMotion(float time)
        {
            CurrentRawInput = GetRawInput();
            ProcessInput();
            currentForward = GetForward(currentMoveInput);
        }

        public virtual void ToggleCollisionsAndPhysics(bool physicsEnabled, bool collisionsEnabled)
        {
            ToggleCollisions(collisionsEnabled);
            TogglePhysics(physicsEnabled);

            this.collisionsEnabled = collisionsEnabled;
            this.physicsEnabled = physicsEnabled;
        }

        public virtual bool IsStrafing()
        {
            return false;
        }
        
        public virtual void OnCollisionEnter(Collision collision){}
        public virtual void OnCollisionExit(Collision collision){}
        public virtual void OnCollisionStay(Collision collision){}
        public virtual void OnTriggerEnter(Collider other){}
        public virtual void OnTriggerExit(Collider other){}
        public virtual void OnTriggerStay(Collider other){}
        
        protected abstract void ToggleCollisions(bool isEnabled);
        protected abstract void TogglePhysics(bool isEnabled);
    }
}
