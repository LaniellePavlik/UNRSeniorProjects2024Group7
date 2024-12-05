using QuanticBrains.MotionMatching.Scripts.Components.Queries.Models;

namespace QuanticBrains.MotionMatching.Scripts.Components.PoseFinders.Models
{
    public struct DistanceResult
    {
        public int index;
        public float distance;
        public int pose;
        public QueryRange queryRange;
    }
}
