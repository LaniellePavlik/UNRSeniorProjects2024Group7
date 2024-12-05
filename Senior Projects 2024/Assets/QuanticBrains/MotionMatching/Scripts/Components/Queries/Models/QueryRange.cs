using System;
using QuanticBrains.MotionMatching.Scripts.CustomAttributes;

namespace QuanticBrains.MotionMatching.Scripts.Components.Queries.Models
{
    [Serializable]
    public struct QueryRange
    {
        [ShowOnly]
        public int featureIDStart;
        [ShowOnly]
        public int featureIDStop;

        public QueryRange(int featureIDStart, int featureIDStop)
        {
            this.featureIDStart = featureIDStart;
            this.featureIDStop = featureIDStop;
        }
    }
}
