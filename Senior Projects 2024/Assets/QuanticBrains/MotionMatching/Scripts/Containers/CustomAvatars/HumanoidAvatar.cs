using System;
using System.Collections.Generic;
using System.Linq;
using QuanticBrains.MotionMatching.Scripts.Containers.ExclusionMasks;
using QuanticBrains.MotionMatching.Scripts.Extensions;
using Unity.Mathematics;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Containers.CustomAvatars
{
    [CreateAssetMenu(menuName = "MotionMatching/Humanoid Avatar")]
    public class HumanoidAvatar : CustomAvatar
    {
        [SerializeField]
        private HumanBodyBones humanRootBone;

        public override int GetRootBone()
        {
            return (int)humanRootBone;
        }

        public override void SetRootBone(int root)
        {
            rootBone = root;
            humanRootBone = (HumanBodyBones)root;
        }
        
        protected override int GetLength()
        {
            return Enum.GetNames(typeof(HumanBodyBones)).Length - 1;
        }

        public override Transform[] GetCharacterTransforms(Transform root, ExclusionMaskBase exclusionMask)
        {
            HumanBone[] humanBones = GetHumanBones();
            IEnumerable<HumanBodyBones> values = Enum.GetValues(typeof(HumanBodyBones)).Cast<HumanBodyBones>();
            Transform[] characterTransforms = new Transform[Length];
            foreach (var (boneValue, i) in values.Select((value, i) => (value, i)))
            {
                try
                {
                    if (i > Length - 1 || (exclusionMask != null && exclusionMask.Contains(i)))
                    {
                        continue;
                    }

                    HumanBone currentBone = humanBones.First(x => x.humanName.Equals(boneValue.ToString()));
                    characterTransforms[i] = root.FirstOrDefault(x => x.name.Equals(currentBone.boneName));
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
            //ToDo: change this method to add nulls + check bonesOptions in Configuration
            // return GetHumanBones().Select((bone, index) => new AvatarBone()
            // {
            //     id = index,
            //     alias = bone.humanName,
            //     boneName = bone.boneName
            // }).ToList();
            
            HumanBone[] humanBones = GetHumanBones();
            IEnumerable<HumanBodyBones> values = Enum.GetValues(typeof(HumanBodyBones)).Cast<HumanBodyBones>();
            List<AvatarBone> avatarBones = new List<AvatarBone>();
            foreach (var (boneValue, i) in values.Select((value, i) => (value, i)))
            {
                try
                {
                    if (i > Length - 1)
                    {
                        continue;
                    }

                    HumanBone currentBone = humanBones.First(x => x.humanName.Equals(boneValue.ToString()));
                    avatarBones.Add(new AvatarBone()
                    {
                        id = i,
                        alias = currentBone.humanName,
                        boneName = currentBone.boneName,
                    });
                }
                catch
                {
                    avatarBones.Add(new AvatarBone()
                    {
                        id = -1,
                        alias = "",
                        boneName = "",
                    });
                }
            }

            return avatarBones;
        }

        public override void GetOriginalAvatarRotations(
            out quaternion[] originalCharacterRotations,
            out quaternion[] defaultRotations,
            Transform[] characterTransforms, 
            Transform transform)
        {
            defaultRotations = new quaternion[characterTransforms.Length];
            SetTPoseRotation(characterTransforms, defaultRotations);
            GetStartingRotation(out originalCharacterRotations, characterTransforms, transform);
        }

        private HumanBone[] GetHumanBones()
        {
            return avatar.humanDescription.human
                .Select(current =>
                {
                    current.humanName = current.humanName.Replace(" ", "");
                    return current;
                })
                .ToArray();
        }
        
        private void SetTPoseRotation(Transform[] characterTransforms, quaternion[] currentRotations)
        {
            var humanBodyBoneValues = Enum.GetValues(typeof(HumanBodyBones));
            SkeletonBone[] skeletonBones = avatar.humanDescription.skeleton;
            foreach (var sb in skeletonBones)
            {
                foreach (HumanBodyBones bone in humanBodyBoneValues)
                {
                    if (bone == HumanBodyBones.LastBone)
                    {
                        continue;
                    }
                    
                    var currentTransform = characterTransforms[(int)bone];
                    if (currentTransform == null)
                    {
                        continue;
                    }
                    
                    if (!currentTransform.name.Equals(sb.name))
                    {
                        continue;
                    }
                    
                    currentRotations[(int)bone] = currentTransform.localRotation;
                    
                    var localBoneRotation = sb.rotation;
                    currentTransform.localRotation = localBoneRotation;
                }
            }
        }
        
        private void GetStartingRotation(out quaternion[] initialValues,Transform[] characterTransforms, Transform characterRoot)
        {
            var humanBodyBoneValues = Enum.GetValues(typeof(HumanBodyBones));
            initialValues = new quaternion[humanBodyBoneValues.Length];
            SkeletonBone[] skeletonBones = avatar.humanDescription.skeleton;
            foreach (var sb in skeletonBones)
            {
                foreach (HumanBodyBones bone in humanBodyBoneValues)
                {
                    if (bone == HumanBodyBones.LastBone)
                    {
                        continue;
                    }
                    
                    var currentTransform = characterTransforms[(int)bone];
                    if (currentTransform == null)
                    {
                        continue;
                    }
                    
                    if (!currentTransform.name.Equals(sb.name))
                    {
                        continue;
                    }
                
                    //Base - working
                    var relativeRotation = Quaternion.Inverse(characterRoot.rotation) * currentTransform.rotation;  //Rotation relative to character root
                    initialValues[(int)bone] = relativeRotation;
                }
            }
        }
        
        private void ResetBoneRotations(Transform[] characterTransforms, quaternion[] rotations)
        {
            for(int i = 0; i < characterTransforms.Length; i++)
            {
                if (!characterTransforms[i]) continue;
                
                characterTransforms[i].localRotation = rotations[i];
            }
        }
    }
}
