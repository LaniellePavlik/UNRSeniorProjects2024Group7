using QuanticBrains.MotionMatching.Scripts.Components.PoseFinders.Models;
using QuanticBrains.MotionMatching.Scripts.Components.QueryFlows;
using QuanticBrains.MotionMatching.Scripts.Components.QueryFlows.Implementations;
using QuanticBrains.MotionMatching.Scripts.Tags;
using Unity.Collections;
using Unity.Mathematics;

namespace QuanticBrains.MotionMatching.Scripts.Components.PoseFinders.Implementations
{
    public class LoopActionPoseFinder : ActionPoseFinder
    {
        public override int Find(ref NativeArray<float3> previousPositions,
            ref float timeOnLastPositions,
            out float currentMinDistance,
            float4x4 rtsModelInverse,
            PoseFinderGenericVariables poseFinderGenericVariables,
            NativeArray<float3> currentBonesPosition,
            int bonesCount,
            QueryComputedFlow queryComputedFlow,
            NativeArray<float3> futureOffsets,
            NativeArray<float3> futureDirections,
            PastTrajectory pastTrajectory,
            NativeArray<DistanceResult> poseResult,
            bool wantDebugDistances,
            bool forceContinuousPose)
        {
            currentMinDistance = 0;
            LoopActionQueryComputedFlow laqcf = (LoopActionQueryComputedFlow)queryComputedFlow;
            if (!laqcf.FirstFrame)
            {
                laqcf.FirstFrame = true;
                return laqcf.currentFeatureID;
            }

            //**
            var currentFeatures = laqcf.GetQueryComputed().featuresData[laqcf.currentFeatureID];

            if (laqcf.CurrentAnimationPoseID == -1)
            {
                laqcf.CurrentAnimationPoseID = 0;
                return laqcf.currentFeatureID;
            }

            if (!laqcf.InterruptedLoop)
            {
                //Get ActionTag behaviour
                laqcf.CurrentAnimationPoseID = currentFeatures.animFrame + 1;
                var nextFrameID = laqcf.CurrentAnimationPoseID + 1;

                var animationPoses = laqcf.dataset.animationsData[currentFeatures.animationID];

                if (animationPoses.Count >= nextFrameID)
                {
                    laqcf.isSearch = false;
                    return laqcf.currentFeatureID + 1;
                }

                //ToDo: add if not interrupted here
                if (laqcf.CurrentState.Equals(ActionTagState
                        .InProgress)) //If its currently on "in progress" state -> back to start
                {
                    int newPose = laqcf.currentFeatureID - laqcf.CurrentAnimationPoseID + 1;
                    laqcf.CurrentAnimationPoseID = 0;

                    return newPose;
                }
            }

            SetNextStateProperties(laqcf);
            return laqcf.currentFeatureID;
        }

        private void SetNextStateProperties(LoopActionQueryComputedFlow laqcf)
        {
            //Go to Next State
            HandleNextState(laqcf);

            //Check init warping and collisions+physics
            laqcf.CheckInitWarpingPropertiesAndPhysicsSetup();

            UpdateAnimationIndexes(laqcf);
            laqcf.InterruptedLoop = false;
        }
    }
}
