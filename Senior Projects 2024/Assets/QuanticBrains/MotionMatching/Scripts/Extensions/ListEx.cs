using System.Collections.Generic;

namespace QuanticBrains.MotionMatching.Scripts.Extensions
{
    public static class ListEx
    {
        public static void AddOrReplace<T>(this List<T> list, int index, T element)
        {
            if (list.Count > index)
            {
                list[index] = element;
                return;
            }
            list.Insert(index, element);
        }
    }
}
