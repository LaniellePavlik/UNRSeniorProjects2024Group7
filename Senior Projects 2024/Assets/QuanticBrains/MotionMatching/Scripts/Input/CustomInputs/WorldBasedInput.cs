using Unity.Mathematics;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Input.CustomInputs
{
    [CreateAssetMenu(menuName = "MotionMatching/CustomInputs/WorldBasedInput")]
    public class WorldBasedInput : InputCustomizable
    {
        public override float3 HandleCustomInput(Vector2 input, Transform transform)
        {
            //World based input
            return new Vector3(input.x, 0f, input.y);
        }
    }
}
