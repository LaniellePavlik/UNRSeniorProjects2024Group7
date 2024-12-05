using Unity.Mathematics;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Input.CustomInputs
{
    public abstract class InputCustomizable : ScriptableObject
    {
        public abstract float3 HandleCustomInput(Vector2 input, Transform transform);
    }
}
