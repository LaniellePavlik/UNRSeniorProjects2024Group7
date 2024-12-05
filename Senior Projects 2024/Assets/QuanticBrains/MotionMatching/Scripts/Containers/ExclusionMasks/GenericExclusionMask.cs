using System.Collections.Generic;
using QuanticBrains.MotionMatching.Scripts.Containers.CustomAvatars;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Containers.ExclusionMasks
{
    [CreateAssetMenu(menuName = "MotionMatching/Generic Exclusion Mask")]
    public class GenericExclusionMask: ExclusionMaskBase
    {
        public GenericAvatar genericAvatar;
        
        public override bool Contains(int id)
        {
            var bones = genericAvatar.GetAvatarDefinition();
            if (bones.Count != bonesToExclude.Count)
            {
                return false;
            }

            if (bonesToExclude.Count <= id)
            {
                return false;
            }
            
            return bonesToExclude[id];
        }
    }
}
