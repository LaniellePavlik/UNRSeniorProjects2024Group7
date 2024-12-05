using System.Collections.Generic;
using QuanticBrains.MotionMatching.Scripts.Components.PoseBlenders.Models;
using QuanticBrains.MotionMatching.Scripts.Containers;
using QuanticBrains.MotionMatching.Scripts.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace QuanticBrains.MotionMatching.Scripts.Components.PoseBlenders
{
    public abstract class PoseBlender
    {
        protected const int RootResultStart = 0;
        protected const int RootResultEnd = 1;

        public static void CalculateRootBoundariesMotion(
            ref BlendBoundaries boundaries, 
            NativeArray<float3> rootPositionBoundariesResults,
            NativeArray<quaternion> rootRotationBoundariesResults, 
            List<AnimationData> animation, 
            int animationPoseID,
            float delta,
            Transform root,
            FuturePrediction nextPrediction, 
            float responsivenessDirections,
            bool isActionOrIdle = false,
            bool isForcedContinuous = false)
        {
            quaternion nextRootLocalRotation;
            float3 nextRootLocalPosition; 
            if (animation.Count <= animationPoseID + 1) //Final frame of animation
            {
                nextRootLocalRotation = animation[animationPoseID].rootRotation;
                nextRootLocalPosition = animation[animationPoseID].rootPosition;
            }
            else
            {
                nextRootLocalRotation = animation[animationPoseID + 1].rootRotation;
                nextRootLocalPosition = animation[animationPoseID + 1].rootPosition;
            }
            
            var calculateRootBoundariesJob = new CalculateRootBoundariesJob()
            {
                startRootPositionToBlend = boundaries.startRootPositionToBlend,
                endRootPositionToBlend = boundaries.endRootPositionToBlend,
                endRootRotationToBlend = boundaries.endRootRotationToBlend,
                delta = delta,
                forward = root.forward,
                nextPrediction = nextPrediction,
                responsivenessDirections = responsivenessDirections,
                nextRootLocalPosition = nextRootLocalPosition,
                nextRootLocalRotation = nextRootLocalRotation,
                rootPosition = root.position,
                rotationsResults = rootRotationBoundariesResults,
                positionsResults = rootPositionBoundariesResults,
                isActionOrIdle = isActionOrIdle,
                isForcedContinuous = isForcedContinuous
            };
            calculateRootBoundariesJob.Schedule().Complete();
            boundaries.startRootPositionToBlend = rootPositionBoundariesResults[RootResultStart];
            boundaries.startRootRotationToBlend = rootRotationBoundariesResults[RootResultStart];
            boundaries.endRootPositionToBlend = rootPositionBoundariesResults[RootResultEnd];
            boundaries.endRootRotationToBlend = rootRotationBoundariesResults[RootResultEnd];
        }
        
        [BurstCompile]
        private struct CalculateRootBoundariesJob : IJob
        {
            [ReadOnly] public float3 forward;
            [ReadOnly] public float3 rootPosition;
            [ReadOnly] public quaternion nextRootLocalRotation;
            [ReadOnly] public float3 nextRootLocalPosition;
            [ReadOnly] public float responsivenessDirections;
            [ReadOnly] public float delta;
            [ReadOnly] public FuturePrediction nextPrediction;
            [ReadOnly] public quaternion endRootRotationToBlend;
            [ReadOnly] public float3 startRootPositionToBlend;
            [ReadOnly] public float3 endRootPositionToBlend;
            [ReadOnly] public bool isActionOrIdle;
            [ReadOnly] public bool isForcedContinuous;

            public NativeArray<float3> positionsResults;
            public NativeArray<quaternion> rotationsResults;

            public void Execute()
            {
                // Course correction: this uses the spring decayment, from inertialization, to correct the root orientation of the character.
                // It is attached to the directional responsiveness factor of the main motion matching algorithm.
                // The direction is calculated with the decayment and blended in through the regular blending algorithm.
                quaternion nextRotation = CalculateNextRootRotation(endRootRotationToBlend, nextRootLocalRotation,
                    forward, nextPrediction.futureDirections[^1], responsivenessDirections, delta, isActionOrIdle, isForcedContinuous);
                
                rotationsResults[RootResultStart] = endRootRotationToBlend;
                rotationsResults[RootResultEnd] = nextRotation;
                
                var initialDist = math.distance(endRootPositionToBlend, startRootPositionToBlend);
                var rest = math.distance(endRootPositionToBlend, rootPosition);
                
                float3 nextPositionInGlobalSpace;
                if (rest / initialDist < 0.8f)
                {
                    nextPositionInGlobalSpace =
                        MathUtils.TranslateToGlobal(
                            endRootPositionToBlend,
                            endRootRotationToBlend,
                            nextRootLocalPosition
                        );
                    positionsResults[RootResultStart] = endRootPositionToBlend;
                }
                else
                {
                    nextPositionInGlobalSpace =
                        MathUtils.TranslateToGlobal(
                            rootPosition,
                            endRootRotationToBlend,
                            nextRootLocalPosition
                        );
                    positionsResults[RootResultStart] = rootPosition;
                }
                positionsResults[RootResultEnd] = nextPositionInGlobalSpace;
                
            }
        }

        private static quaternion CalculateNextRootRotation(
            quaternion endRootRotationToBlend, 
            quaternion nextRootLocalRotation,
            float3 forward,
            float3 lastFutureDirection,
            float responsivenessDirections,
            float delta,
            bool isActionOrIdle,
            bool isForcedContinuous)
        {
            if (isActionOrIdle || isForcedContinuous)
            {
                return endRootRotationToBlend.Add(nextRootLocalRotation);
            }

            if (math.isnan(lastFutureDirection.x))
            {
                return endRootRotationToBlend.Add(nextRootLocalRotation);
            }
            
            //return endRootRotationToBlend.Add(nextRootLocalRotation); //ToDo: use this once blendspaces are available
            Quaternion nextRotation = endRootRotationToBlend.Add(nextRootLocalRotation);
            var nextDirection = nextRotation * Vector3.forward;
            
            // Additional rotation from next to desiredRotation based on futureDirs
            var angleDiff = MathUtils.SignedAngle(nextDirection, lastFutureDirection, Vector3.up);
            quaternion offset = new Quaternion
            {
                eulerAngles = new Vector3(0, angleDiff, 0)
            };
            var angularSpeed = new float3(0, 0f, 0);
            SpringUtils.DecaySpringRotation(ref offset, ref angularSpeed, delta, 0.005f * responsivenessDirections);
            return nextRotation * offset;
        }

        public static void CalculateNextBonePositionJob(
            ref BlendBoundaries boundaries, 
            int index,
            NativeArray<BoneData> currentBonesDatas, 
            NativeArray<BoneData> nextBonesDatas, 
            int rootNode,
            bool wantApplyPositions,
            bool wantApplyScales,
            bool isLastPose)
        {
            quaternion nextBoneRotation = GetNextBoneRotation(nextBonesDatas, isLastPose, index, currentBonesDatas[index]);
            float3 nextBonePosition = float3.zero;
            if (index == rootNode || wantApplyPositions)
            {
                nextBonePosition = GetNextBoneLocalPosition(nextBonesDatas, isLastPose,index, currentBonesDatas[index]);
            }
            
            float3 nextBoneScale = float3.zero;
            if (wantApplyScales)
            {
                nextBoneScale = GetNextBoneScale(nextBonesDatas, isLastPose,index, currentBonesDatas[index]);
            }
                        
            boundaries.startRotationValues[index] = currentBonesDatas[index].rotation;
            boundaries.endRotationValues[index]   = nextBoneRotation;
            boundaries.startPositionValues[index] = currentBonesDatas[index].localPosition;
            boundaries.endPositionValues[index]   = nextBonePosition;
            boundaries.startScaleValues[index]    = currentBonesDatas[index].scale;
            boundaries.endScaleValues[index]      = nextBoneScale;
        }

        public static void BlendRoot(
            ref BlendingResults blendingResults, 
            BlendBoundaries boundaries, 
            BlendingTypes blendingType,
            float elapsedTime, 
            float lerpDuration,
            bool isBlendActivated)
        {
            blendingResults.rootPosition = DoBlend(boundaries.startRootPositionToBlend, 
                boundaries.endRootPositionToBlend, blendingType, elapsedTime, lerpDuration);
            blendingResults.rootRotation = DoBlend(boundaries.startRootRotationToBlend,
                boundaries.endRootRotationToBlend, blendingType, elapsedTime, lerpDuration);
        }
        
        public static void BlendBone(
            ref BlendingResults blendingResults, 
            int index, 
            int rootNode,
            BlendBoundaries boundaries, 
            BlendingTypes blendingType, 
            float elapsedTime, 
            float lerpDuration,
            bool wantApplyPositions,
            bool wantApplyScales)
        {
            blendingResults.bonesRotation[index] = DoBlend(boundaries.startRotationValues[index], boundaries.endRotationValues[index], blendingType, elapsedTime, lerpDuration);
            if (index == rootNode || wantApplyPositions)
            {
                blendingResults.bonesPosition[index] = DoBlend(boundaries.startPositionValues[index], boundaries.endPositionValues[index], blendingType, elapsedTime, lerpDuration);    
            }

            if (wantApplyScales)
            {
                blendingResults.bonesScale[index] = DoBlend(boundaries.startScaleValues[index], boundaries.endScaleValues[index], blendingType, elapsedTime, lerpDuration);
            }
        }
        
        private static float3 DoBlend(float3 start, float3 end, BlendingTypes blendingType, float elapsedTime, float lerpDuration)
        {
            return blendingType switch
            {
                BlendingTypes.Lerp => BlendingMethods.Lerp(start, end, elapsedTime, lerpDuration),
                BlendingTypes.Slerp => BlendingMethods.SLerp(start, end, elapsedTime, lerpDuration),
                _ => BlendingMethods.Lerp(start, end, elapsedTime, lerpDuration)
            };
        }

        private static quaternion DoBlend(quaternion start, quaternion end, BlendingTypes blendingType, float elapsedTime, float lerpDuration)
        {
            return blendingType switch
            {
                BlendingTypes.Lerp => BlendingMethods.Lerp(start, end, elapsedTime, lerpDuration),
                BlendingTypes.Slerp => BlendingMethods.SLerp(start, end, elapsedTime, lerpDuration),
                _ => BlendingMethods.Lerp(start, end, elapsedTime, lerpDuration)
            };
        }
        
        private static quaternion GetNextBoneRotation(NativeArray<BoneData> nextBonesDatas, bool isLastPose, int boneID, BoneData boneData)
        {
            if (!isLastPose)
                return nextBonesDatas[boneID].rotation;

            //Estimate new bone rotation
            quaternion previousRotation = nextBonesDatas[boneID].rotation;
            quaternion diffQuaternion = boneData.rotation.Diff(previousRotation);
            return boneData.rotation.Add(diffQuaternion);
        }
        
        private static float3 GetNextBoneLocalPosition(NativeArray<BoneData> nextBonesDatas, bool isLastPose, int boneID, BoneData boneData)
        {
            if (!isLastPose)
                return nextBonesDatas[boneID].localPosition;

            //Estimate new bone position
            float3 previousPosition = nextBonesDatas[boneID].localPosition;
            float3 diff = boneData.localPosition - previousPosition;
            return boneData.localPosition + diff;
        }
        
        private static float3 GetNextBoneScale(NativeArray<BoneData> nextBonesDatas, bool isLastPose, int boneID, BoneData boneData)
        {
            if (!isLastPose)
                return nextBonesDatas[boneID].scale;

            //Estimate new bone position
            float3 previousScale = nextBonesDatas[boneID].scale;
            float3 diff = boneData.scale - previousScale;
            return boneData.scale + diff;
        }
    }
}
