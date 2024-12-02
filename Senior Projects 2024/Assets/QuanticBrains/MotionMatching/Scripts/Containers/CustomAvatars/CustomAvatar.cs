using System.Collections.Generic;
using QuanticBrains.MotionMatching.Scripts.Containers.ExclusionMasks;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace QuanticBrains.MotionMatching.Scripts.Containers.CustomAvatars
{
    public abstract class CustomAvatar : ScriptableObject
    {
        public Avatar avatar;
        
        [HideInInspector]
        public int rootBone;
        
        private int _length;
        public int Length
        {
            get => GetLength();
            protected set => _length = value;
        }

        public virtual int GetRootBone()
        {
            return rootBone;
        }
        public virtual void SetRootBone(int root)
        {
            rootBone = root;
        }

        protected abstract int GetLength();
        
        public abstract Transform[] GetCharacterTransforms(Transform root, ExclusionMaskBase exclusionMask);

        public abstract List<AvatarBone> GetAvatarDefinition();

        public abstract void GetOriginalAvatarRotations(
            out quaternion[] originalCharacterRotations,
            out quaternion[] defaultRotations,
            Transform[] characterTransforms, 
            Transform transform);
    }
}
