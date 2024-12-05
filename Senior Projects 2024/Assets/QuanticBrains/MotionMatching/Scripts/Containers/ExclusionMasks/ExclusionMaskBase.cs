using System.Collections.Generic;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Containers.ExclusionMasks
{
    public abstract class ExclusionMaskBase : ScriptableObject
    {
        public bool disableRootMotion;
        public List<bool> bonesToExclude;

        public abstract bool Contains(int id);
    }
}
