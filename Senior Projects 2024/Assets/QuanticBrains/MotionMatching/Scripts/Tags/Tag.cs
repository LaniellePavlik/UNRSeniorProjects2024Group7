using System;
using System.Collections.Generic;
using QuanticBrains.MotionMatching.Scripts.Containers;
using QuanticBrains.MotionMatching.Scripts.Containers.CustomAvatars;
using QuanticBrains.MotionMatching.Scripts.CustomAttributes;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Tags
{
    public class Tag : ScriptableObject
    {
        public List<TagRange> ranges;

        public Tag(string name)
        {
            this.name = name;
            ranges = new List<TagRange>();
        }

        public void Init(TagBase tb)
        {
            name = tb.name;
            ranges = tb.ranges;
        }
    }

    [Serializable]
    public class TagBase
    {
        [ShowOnly] public string name;
        public List<TagRange> ranges;

        public TagBase(string name)
        {
            this.name = name;
            ranges = new List<TagRange>();
        }

        public TagBase(Tag so)
        {
            name = so.name;
            ranges = so.ranges;
        }

        public virtual bool IsDone()
        {
            return false;
        }
    }

    [Serializable]
    public enum WarpingType
    {
        None = 0,
        Position = 1,
        Rotation = 2,

        //Time = 4,
        PositionRotation = 3,
        // PositionTime = 5,
        // RotationTime = 6,
        // PositionRotationTime = 7
    }

    [Serializable]
    public enum WarpingMode
    {
        None = 0,
        Linear = 1,
        Quadratic = 2,
        Exponential = 3,
        DecayLogarithmic = 4,
        Custom = 5,
        Dynamic = 6
    }

    //Action Tag
    [Serializable]
    public class ActionTag : TagBase
    {
        public bool hasRecoveryState;
        public bool hasInitState;

        [HideInInspector] public int[] animationIDSerialized = new int[3];

        //Interruptions
        public bool[] isInterruptibleByState = new bool[3];
        public InterruptibleBy interruptibleType = InterruptibleBy.None;
        public List<string> allowedInterruptionNames = new List<string>();

        //Warping properties
        public WarpingType warpingType;
        public WarpingMode posWarpingMode;
        public WarpingMode rotWarpingMode;

        [Range(0, 1)] public float positionWarpWeight = 0.0f;
        [Range(0, 1)] public float rotationWarpWeight = 0.0f;

        //Custom curves
        [HideInInspector] public AnimationCurve customWarpPositionCurve;
        [HideInInspector] public AnimationCurve customWarpRotationCurve;

        public bool contactWarping;
        public List<AvatarBone> warpContactBones;
        
        [HideInInspector] public bool simulateRootMotion; //Only used on loop actions

        //By inheritance, it has ranges property (each range has one anim) //0 - Init, 1 - Action, 2 - Recovery

        public ActionTag(string name) : base(name)
        {
        }

        public ActionTag(string name, bool hasInitState = false, bool hasRecoveryState = false) : base(name)
        {
            //Let's assume every state is stored inside a range

            this.hasInitState = hasInitState;
            this.hasRecoveryState = hasRecoveryState;
        }

        public ActionTag(Tag so, bool hasInitState = false, bool hasRecoveryState = false) : base(so)
        {
            //Let's assume every state is stored inside a range

            this.hasInitState = hasInitState;
            this.hasRecoveryState = hasRecoveryState;
        }

        public bool HasInitState()
        {
            return hasInitState;
        }

        public bool HasRecoveryState()
        {
            return hasRecoveryState;
        }
    }

    //Action Tag
    [Serializable]
    public class IdleTag : TagBase
    {
        public bool hasRecoveryState;
        public bool hasInitState;

        public List<TagRange> initRanges;
        public List<TagRange> loopRanges;

        [HideInInspector] public int[] transitionIDSerialized;
        [HideInInspector] public int[] loopIDSerialized;

        //By inheritance, it has ranges property (each range has one anim) //0 - Init, 1 - Action, 2 - Recovery

        public IdleTag(string name) : base(name)
        {
            hasInitState = true;
            hasRecoveryState = false;
        }

        public IdleTag(string name, bool hasTransition) : base(name)
        {
            hasInitState = hasTransition;
            hasRecoveryState = false;
        }

        public IdleTag(Tag so, List<TagRange> init, List<TagRange> loops) : base(so)
        {
            //Let's assume every state is stored inside a range
            hasInitState = true;
            hasRecoveryState = false;

            initRanges = init;
            loopRanges = loops;
        }

        public bool HasInitState()
        {
            return hasInitState;
        }

        public bool HasRecoveryState()
        {
            return hasRecoveryState;
        }
    }

    //Loop Action Tag
    [Serializable]
    public class LoopActionTag : ActionTag
    {
        public LoopActionTag(string name) : base(name)
        {
        }

        public LoopActionTag(string name, bool hasInitState = false, bool hasRecoveryState = false,
            bool simulate = false) : base(name, hasInitState, hasRecoveryState)
        {
            simulateRootMotion = simulate;
        }

        public LoopActionTag(Tag so, bool hasInitState = false, bool hasRecoveryState = false) : base(so)
        {
        }
    }


    [Serializable]
    public class TagRange
    {
        [ShowOnly] public string animName;
        [ShowOnly] public int poseStart; //featureIDStart in QueryRange
        [ShowOnly] public int poseStop; //featureIDStop in QueryRange
        [ShowOnly] public int frameStart;
        [ShowOnly] public int frameStop;

        public TagRange()
        {
        }

        public TagRange(string name, int frameStart, int frameStop)
        {
            animName = name;
            this.frameStart = frameStart;
            this.frameStop = frameStop;
        }
    }

    [Serializable]
    public enum ActionTagState
    {
        Init = 0,
        InProgress = 1,
        Recovery = 2
    }
}
