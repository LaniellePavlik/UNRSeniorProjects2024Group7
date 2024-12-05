using System;
using System.Collections.Generic;
using QuanticBrains.MotionMatching.Scripts.Tags;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Containers
{
    [Serializable]
    public class CharacteristicsByTag
    {
        public string id;
        public List<BoneCharacteristic> characteristics;
        [Range(0f, 1f)]
        public float weightFutureOffset = 1;
        [Range(0f, 1f)]
        public float weightFutureDirection = 1;
        [Range(0f, 1f)]
        public float weightPastOffset = 1;
        [Range(0f, 1f)]
        public float weightPastDirection = 1;
    }
}
