using Unity.Collections;

namespace QuanticBrains.MotionMatching.Scripts.Components.QueryFlows.Models
{
    public struct BonesWeights
    {
        public NativeArray<float> weights;
        public float weightFutureOffset;
        public float weightFutureDirection;
        public float weightPastOffset;
        public float weightPastDirection;
        public float totalWeightPositions;
        public float totalWeightVelocities;
    }
}
