using System;
using System.Collections.Generic;
using System.Linq;
using QuanticBrains.MotionMatching.Scripts.Components.Queries.Models;
using QuanticBrains.MotionMatching.Scripts.Tags;
using Unity.Collections;

namespace QuanticBrains.MotionMatching.Scripts.Components.Queries.Implementations
{
    [Serializable]
    public class IdleQueryComputed : QueryComputed
    {
        public IdleTag loopTag;
        public List<QueryRange> initRanges;
        public List<QueryRange> idleRanges;

        private NativeArray<QueryRange> _initRanges;
        private NativeArray<QueryRange> _idleRanges;

        public IdleQueryComputed(IdleTag tagBase, int fEstimates, int pEstimates, int nBones) : base(fEstimates, pEstimates, nBones)
        {
            loopTag = tagBase;
            InitRanges();
        }

        private void InitRanges()
        {
            initRanges = CreateQueryRange(loopTag.initRanges);
            idleRanges = CreateQueryRange(loopTag.loopRanges);
        }

        private List<QueryRange> CreateQueryRange(List<TagRange> tagRanges)
        {
            List<QueryRange> newQueryRanges = new List<QueryRange>();
            foreach (var range in tagRanges)
            {
                QueryRange newRange = new QueryRange(range.poseStart, range.poseStop);
                newQueryRanges.Add(newRange);
            }

            return newQueryRanges;
        }

        public override List<QueryRange> GetRanges()
        {
            return initRanges.Concat(idleRanges).ToList();
        }

        public NativeArray<QueryRange> GetInitRanges()
        {
            if (_initRanges.IsCreated)
            {
                return _initRanges;
            }

            _initRanges = new NativeArray<QueryRange>(initRanges.ToArray(), Allocator.Persistent);
            return _initRanges;
        }
        
        public NativeArray<QueryRange> GetIdleRanges()
        {
            if (_idleRanges.IsCreated)
            {
                return _idleRanges;
            }

            _idleRanges = new NativeArray<QueryRange>(idleRanges.ToArray(), Allocator.Persistent);
            return _idleRanges;
        }

        public override void Destroy()
        {
            base.Destroy();
            
            if (_initRanges.IsCreated)
            {
                _initRanges.Dispose();
            }

            if (_idleRanges.IsCreated)
            {
                _idleRanges.Dispose();
            }
        }
    }
}
