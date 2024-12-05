using System;
using System.Collections.Generic;
using QuanticBrains.MotionMatching.Scripts.Components.Queries.Models;
using QuanticBrains.MotionMatching.Scripts.Tags;

namespace QuanticBrains.MotionMatching.Scripts.Components.Queries.Implementations
{
    [Serializable]
    public class ActionQueryComputed : QueryComputed
    {
        public ActionTag actionTag;
        public ActionQueryComputed(ActionTag tagBase, int fEstimates, int pEstimates, int nBones) : base(fEstimates, pEstimates, nBones)
        {
            actionTag = tagBase;
            ranges = CreateQueryRange(actionTag.ranges);
        }

        protected List<QueryRange> CreateQueryRange(List<TagRange> tagRanges)
        {
            List<QueryRange> newQueryRanges = new List<QueryRange>();
            foreach (var range in tagRanges)
            {
                QueryRange newRange = new QueryRange(range.poseStart, range.poseStop);
                newQueryRanges.Add(newRange);
            }

            return newQueryRanges;
        }
    }
}
