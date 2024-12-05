using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.CustomAttributes
{
    public class ImageInScriptAttribute : PropertyAttribute
    {
        public readonly string imagePath;

        public ImageInScriptAttribute(string imagePath)
        {
            this.imagePath = imagePath;
        }
    }
}
