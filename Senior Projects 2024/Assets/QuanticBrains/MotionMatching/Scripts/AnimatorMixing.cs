using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts
{
    public class AnimatorMixing : MonoBehaviour
    {
        private Animator _animator;
        private MotionMatching _motionMatching;
        
        public RuntimeAnimatorController runtimeAnimatorController;
        public bool applyRootMotion;
        public AnimatorCullingMode cullingMode;

        private void Awake()
        {
            _motionMatching = GetComponent<MotionMatching>();
            _animator = gameObject.AddComponent<Animator>();
            _animator.applyRootMotion = applyRootMotion;
            _animator.runtimeAnimatorController = runtimeAnimatorController; 
            _animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
            _animator.cullingMode = cullingMode;
            _animator.avatar = _motionMatching.avatar ? _motionMatching.avatar.avatar : _motionMatching.dataset.avatar.avatar;
        }
    }
}
