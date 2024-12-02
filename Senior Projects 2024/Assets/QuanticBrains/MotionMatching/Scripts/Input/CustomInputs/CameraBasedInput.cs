using Unity.Mathematics;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Input.CustomInputs
{
    [CreateAssetMenu(menuName = "MotionMatching/CustomInputs/CameraBasedInput")]
    public class CameraBasedInput : InputCustomizable
    {
        public override float3 HandleCustomInput(Vector2 input, Transform transform)
        {
            //Camera based input
            float verticalInput = input.y;
            float horizontalInput = input.x;

            //Camera vectors
            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;
            forward.y = 0;
            forward = forward.normalized;
            right.y = 0;
            right = right.normalized;

            //Directional inputs
            Vector3 forwardVerticalInput = verticalInput * forward;
            Vector3 rightHorizontalInput = horizontalInput * right;

            //Final input
            return forwardVerticalInput + rightHorizontalInput;
        }
    }
}
