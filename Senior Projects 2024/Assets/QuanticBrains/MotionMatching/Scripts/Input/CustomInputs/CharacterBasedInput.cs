using Unity.Mathematics;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Input.CustomInputs
{
    [CreateAssetMenu(menuName = "MotionMatching/CustomInputs/CharacterBasedInput")]
    public class CharacterBasedInput : InputCustomizable
    {
        public override float3 HandleCustomInput(Vector2 input, Transform transform)
        {
            //Character based input
            return transform.forward * input.y
                   + transform.right * input.x;
        }
    }
}
