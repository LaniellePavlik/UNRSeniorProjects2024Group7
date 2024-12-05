using System;

namespace QuanticBrains.MotionMatching.Scripts.Extensions
{
    public static class ObjectEx
    {
        public static T Also<T>(this T self, Action<T> block) where T: class
        {
            block(self);
            return self;
        }
    }
}
