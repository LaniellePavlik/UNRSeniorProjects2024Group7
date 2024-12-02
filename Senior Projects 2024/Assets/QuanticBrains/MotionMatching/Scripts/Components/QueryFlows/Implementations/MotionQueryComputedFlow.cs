using QuanticBrains.MotionMatching.Scripts.Components.PoseFinders.Implementations;
using QuanticBrains.MotionMatching.Scripts.Components.PoseSetters.Implementations;
using QuanticBrains.MotionMatching.Scripts.Components.Queries;
using QuanticBrains.MotionMatching.Scripts.Containers;
using UnityEngine;
using UnityEngine.Jobs;

namespace QuanticBrains.MotionMatching.Scripts.Components.QueryFlows.Implementations
{
    public class MotionQueryComputedFlow : QueryComputedFlow
    {
        public MotionQueryComputedFlow(
            Dataset dataset, 
            Transform root, 
            CurrentBoneTransformsValues currentBoneTransformsValues,
            TransformAccessArray characterTransformsNative,
            GlobalWeights globalWeights, 
            int searchRate) 
            : base(dataset, root, currentBoneTransformsValues, characterTransformsNative, globalWeights, searchRate)
        {
            PoseFinder = new MotionPoseFinder();
            PoseSetter = new MotionPoseSetter();
        }

        public override void Build(QueryComputed queryComputed, int length)
        {
            SetQueryComputed(queryComputed);
            ManageWeights(length);
            InitializeDistanceResults();
        }
    }
}
