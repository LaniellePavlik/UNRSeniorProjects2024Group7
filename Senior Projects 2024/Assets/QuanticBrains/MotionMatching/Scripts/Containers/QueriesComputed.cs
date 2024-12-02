using System.Collections.Generic;
using QuanticBrains.MotionMatching.Scripts.Components;
using QuanticBrains.MotionMatching.Scripts.Components.Queries;
using QuanticBrains.MotionMatching.Scripts.Components.Queries.Implementations;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Containers
{
    public class QueriesComputed : ScriptableObject
    {
        public List<MotionQueryComputed> queries;
        public List<ActionQueryComputed> actionQueries;
        public List<LoopActionQueryComputed> loopActionQueries;
        public List<IdleQueryComputed> idleQueries;
    }
}
