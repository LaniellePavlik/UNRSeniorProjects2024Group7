using QuanticBrains.MotionMatching.Scripts.Components.PoseBlenders.Models;
using QuanticBrains.MotionMatching.Scripts.Components.QueryFlows;
using QuanticBrains.MotionMatching.Scripts.Input.CharacterController;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Components.PoseSetters.Implementations
{
    public class MotionPoseSetter : PoseSetter
    {
        public override void SetRootPose(BlendingResults blendingResults, CharacterControllerBase characterControllerBase)
        {
            characterControllerBase.Move(blendingResults.rootPosition, blendingResults.rootRotation, Time.fixedDeltaTime);
        }
    }
}
