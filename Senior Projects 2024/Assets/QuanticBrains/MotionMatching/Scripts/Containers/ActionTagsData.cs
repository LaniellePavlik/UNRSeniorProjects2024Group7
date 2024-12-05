using System;
using System.Collections.Generic;
using QuanticBrains.MotionMatching.Scripts.Tags;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Containers
{
    [Serializable]
    public class ActionTagsData
    {
        public string actionName;
        public int initAnimationID;
        public int actionAnimationID;
        public int recoveryAnimationID;

        public bool isLoopable;
        public bool isSimulated;

        public InterruptibleBy interruptibleType = InterruptibleBy.None;
        public List<string> tagNamesList = new List<string>();

        public bool contactWarping = false;
        public List<int> contactWarpBones = new List<int>();
        
        //Interruptible states
        public bool isInitInterruptible = false;
        public bool isInProgressInterruptible = false;
        public bool isRecoveryInterruptible = false;

        //Warping properties
        public WarpingType warpingType;
        public WarpingMode posWarpingMode;
        public WarpingMode rotWarpingMode;
    
        [Range(0,1)]
        public float positionWarpWeight = 0.0f;
        [Range(0,1)]
        public float rotationWarpWeight = 0.0f;

        public bool HasInitState()
        {
            return initAnimationID != 0;
        }
        
        public bool HasRecoveryState()
        {
            return recoveryAnimationID != 0;
        }
    }

    [Serializable]
    public class AnimFileData
    {
        public int animationID;
    }

    [Serializable]
    public enum InterruptibleBy
    {
        None = 0,
        All = 1,
        Motions = 2,
        Actions = 3,
        NameList = 4
    }
}
