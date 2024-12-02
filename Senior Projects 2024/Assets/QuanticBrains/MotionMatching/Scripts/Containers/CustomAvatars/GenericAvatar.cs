using System;
using System.Collections.Generic;
using System.Linq;
using QuanticBrains.MotionMatching.Scripts.Containers.ExclusionMasks;
using QuanticBrains.MotionMatching.Scripts.Extensions;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Containers.CustomAvatars
{
    public class GenericAvatar : CustomAvatar
    {
        [SerializeField]
        private List<AvatarBone> avatarBones;

        protected override int GetLength()
        {
            return avatarBones.Count;
        }

        public override Transform[] GetCharacterTransforms(Transform root, ExclusionMaskBase exclusionMask)
        {
            Transform[] characterTransforms = new Transform[Length];
            for(int i = 0; i < avatarBones.Count; i++)
            {
                try
                {
                    if (i > Length - 1 || (exclusionMask != null && exclusionMask.Contains(i)))
                    {
                        continue;
                    }
                    characterTransforms[i] = root.FirstOrDefault(x => x.name.Equals(avatarBones[i].boneName));
                }
                catch
                {
                    // ignored
                }
                
            }
            
            return characterTransforms;
        }

        public override List<AvatarBone> GetAvatarDefinition()
        {
            return avatarBones;
        }

        public void SetAvatarDefinition(List<AvatarBone> bones)
        {
            avatarBones = new List<AvatarBone>(bones);
        }

        public override void GetOriginalAvatarRotations(
            out quaternion[] originalCharacterRotations,
            out quaternion[] defaultRotations,
            Transform[] characterTransforms, 
            Transform transform)
        {
            originalCharacterRotations = new quaternion[characterTransforms.Length];
            for (int i = 0; i < characterTransforms.Length; i++)
            {
                if (characterTransforms[i] == null)
                {
                    originalCharacterRotations[i] = Quaternion.identity;
                    continue;
                }

                originalCharacterRotations[i] = characterTransforms[i].localRotation;
            }

            defaultRotations = originalCharacterRotations;
        }
    }
}
