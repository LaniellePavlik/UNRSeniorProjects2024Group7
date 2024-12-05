using System;
using QuanticBrains.MotionMatching.Scripts.Tags;

namespace QuanticBrains.MotionMatching.Scripts.Components.Queries.Implementations
{
    [Serializable]
    public class LoopActionQueryComputed : ActionQueryComputed
    {
        public LoopActionQueryComputed(ActionTag tagBase, int fEstimates, int pEstimates, int nBones) : base(tagBase, fEstimates, pEstimates, nBones)
        {
        }
    }
}
