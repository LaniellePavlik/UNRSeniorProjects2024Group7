using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Containers.ExclusionMasks
{
    [CreateAssetMenu(menuName = "MotionMatching/Humanoid Exclusion Mask")]
    public class HumanoidExclusionMask: ExclusionMaskBase
    {
        public override bool Contains(int id)
        {
            if (bonesToExclude.Count <= id)
            {
                return false;
            }
            
            return bonesToExclude[id];
        }
    }
}
