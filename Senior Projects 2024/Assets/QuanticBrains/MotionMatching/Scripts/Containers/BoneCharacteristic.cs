using System;
using QuanticBrains.MotionMatching.Scripts.Containers.CustomAvatars;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Containers
{
    [Serializable]
    public class BoneCharacteristic
    {
        public AvatarBone bone;
        [Range(0f, 1f)]
        public float weightPosition = 1;
        [Range(0f, 1f)]
        public float weightVelocity = 1;
    }
}
