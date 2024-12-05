using System;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Extensions
{
    public static class TransformEx
    {
        public static Transform FirstOrDefault(this Transform transform, Func<Transform, bool> query)
        {
            if (query(transform)) {
                return transform;
            }

            for (var i = 0; i < transform.childCount; i++)
            {
                var result = FirstOrDefault(transform.GetChild(i), query);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
