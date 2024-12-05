using QuanticBrains.MotionMatching.Scripts.Components.PoseBlenders.Models;
using QuanticBrains.MotionMatching.Scripts.Components.QueryFlows;
using QuanticBrains.MotionMatching.Scripts.Extensions;
using QuanticBrains.MotionMatching.Scripts.Input.CharacterController;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace QuanticBrains.MotionMatching.Scripts.Components.PoseSetters
{
    public abstract class PoseSetter
    {
        public static void GetNewBoneValues(
            ref NativeArray<OffsetBone> offsetsNative, 
            int index,
            BlendingResults blendingResults,
            NativeArray<float3> resultPositions,
            NativeArray<float3> resultScales,
            NativeArray<quaternion> resultRotations,
            NativeArray<quaternion> originalDiffRotations,
            quaternion rootRotation,
            int rootNode,
            float fixedDeltaTime,
            float halfLife,
            bool wantApplyPositions,
            bool wantApplyScales)
        {
            if (math.isnan(blendingResults.bonesPosition[index].x)) return;
            
            var decayRotationResult = SpringUtils.DecaySpringRotation(offsetsNative[index].rotation, offsetsNative[index].angularVelocity, fixedDeltaTime, halfLife);

            var decayPositionResult = (float3.zero, float3.zero);
            var decayScaleResult = (float3.zero, float3.zero);
            if (index == rootNode || wantApplyPositions)
            {
                decayPositionResult = SpringUtils.DecaySpringPosition(offsetsNative[index].position, offsetsNative[index].velocity, fixedDeltaTime, halfLife);
                resultPositions[index] = blendingResults.bonesPosition[index] + decayPositionResult.Item1;
            }

            if (wantApplyScales)
            {
                decayScaleResult = SpringUtils.DecaySpringPosition(offsetsNative[index].scale, offsetsNative[index].scaleVelocity, fixedDeltaTime, halfLife);
                resultScales[index] = blendingResults.bonesScale[index] + decayScaleResult.Item1;
            }
            
            //Apply decayed offset
            var relativeRotation = math.mul(blendingResults.bonesRotation[index], decayRotationResult.Item1);
            resultRotations[index] = math.mul(math.mul(rootRotation, relativeRotation), originalDiffRotations[index]);
            
            offsetsNative[index] = new OffsetBone
            {
                rotation = decayRotationResult.Item1,
                angularVelocity = decayRotationResult.Item2,
                position = decayPositionResult.Item1,
                velocity = decayPositionResult.Item2,
                scale = decayScaleResult.Item1,
                scaleVelocity = decayScaleResult.Item2
            };
        }
        
        public abstract void SetRootPose(BlendingResults blendingResults, CharacterControllerBase characterControllerBase);
    }

    [BurstCompile]
    public struct SetBonesJob: IJobParallelForTransform
    {
        [ReadOnly] public NativeArray<float3> finalPositionsNative;
        [ReadOnly] public NativeArray<float3> finalScalesNative;
        [ReadOnly] public NativeArray<quaternion> finalRotationsNative;
        [ReadOnly] public int rootNode;
        [ReadOnly] public bool wantApplyPositions;
        [ReadOnly] public bool wantApplyScales;

        public void Execute(int index, TransformAccess transform)
        {
            if (!transform.isValid) return;
            if (index == rootNode || wantApplyPositions)
            {
                transform.localPosition = finalPositionsNative[index];
            }

            if (wantApplyScales)
            {
                transform.localScale = finalScalesNative[index];
            }

            transform.rotation = finalRotationsNative[index];   //Now it is rotation as its relative
        }
    }
}
