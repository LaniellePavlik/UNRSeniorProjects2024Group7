using System.Collections.Generic;
using QuanticBrains.MotionMatching.Scripts.Tags;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Containers
{
    public class Tags : ScriptableObject
    {
        public List<TagBase> tags;
        public List<ActionTag> actionTags;
        public List<IdleTag> idleTags;
    }
}
