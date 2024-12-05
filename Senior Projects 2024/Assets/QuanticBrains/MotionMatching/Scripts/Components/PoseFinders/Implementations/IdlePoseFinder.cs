using QuanticBrains.MotionMatching.Scripts.Components.PoseFinders.Models;
using QuanticBrains.MotionMatching.Scripts.Components.QueryFlows;
using QuanticBrains.MotionMatching.Scripts.Components.QueryFlows.Implementations;
using QuanticBrains.MotionMatching.Scripts.Tags;
using Unity.Collections;
using Unity.Mathematics;

namespace QuanticBrains.MotionMatching.Scripts.Components.PoseFinders.Implementations
{
    public class IdlePoseFinder : MotionPoseFinder
    {
        public override int Find(
            ref NativeArray<float3> previousPositions, 
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
            IdleQueryComputedFlow iqcf = (IdleQueryComputedFlow)queryComputedFlow;
            int newPose;
            if (Counter == 0)
            {
                return FindLoopPose(ref timeOnLastPositions, 
                    ref previousPositions,  
                    out currentMinDistance,
                    rtsModelInverse,
                    poseFinderGenericVariables,
                    currentBonesPosition,
                    bonesCount,
                    iqcf,
                    futureOffsets, 
                    futureDirections,
                    pastTrajectory,
                    poseResult,
                    wantDebugDistances);
            }

            var currentFeatures = queryComputedFlow.GetQueryComputed().featuresData[queryComputedFlow.currentFeatureID];
            
            //Get ActionTag behaviour
            iqcf.isSearch = false;
            iqcf.currentAnimationPoseID = currentFeatures.animFrame + 1;
            Counter++;
            var nextFrameID = iqcf.currentAnimationPoseID + 1;

            var animationPoses = iqcf.dataset.animationsData[currentFeatures.animationID];
            if (animationPoses.Count >= nextFrameID)
            {
                currentMinDistance = 0;
                return queryComputedFlow.currentFeatureID + 1;
            }

            if (iqcf.currentState.Equals(ActionTagState.InProgress))    //If its currently on "in progress" state -> back to start
            {
                currentMinDistance = 0;
                newPose = queryComputedFlow.currentFeatureID - iqcf.currentAnimationPoseID + 1;
                iqcf.currentAnimationPoseID = 0;
                return newPose;
            }
            
            //Go to Next State
            HandleNextState(iqcf);
            
            //Set new frame for next state
            return FindLoopPose(ref timeOnLastPositions, 
                ref previousPositions,  
                out currentMinDistance, 
                rtsModelInverse,
                poseFinderGenericVariables,
                currentBonesPosition,
                bonesCount,
                iqcf,
                futureOffsets, 
                futureDirections,
                pastTrajectory,
                poseResult, 
                wantDebugDistances);
        }

        private int FindLoopPose(
            ref float timeOnLastPositions, 
            ref NativeArray<float3> previousPositions,
            out float currentMinDistance, 
            float4x4 rtsModelInverse,
            PoseFinderGenericVariables poseFinderGenericVariables,
            NativeArray<float3> currentBonesPosition,
            int bonesCount,
            IdleQueryComputedFlow lqcf, 
            NativeArray<float3> futureOffsets, 
            NativeArray<float3> futureDirections,
            PastTrajectory pastTrajectory,
            NativeArray<DistanceResult> poseResult,
            bool wantDebugDistances)
        {
            Counter++;
            //This will be called on init and in progress start
            var newPose = GetNewPose(
                ref previousPositions,  
                out currentMinDistance,
                rtsModelInverse,
                poseFinderGenericVariables,
                currentBonesPosition,
                bonesCount,
                timeOnLastPositions, 
                lqcf,
                futureOffsets, 
                futureDirections,
                pastTrajectory,
                poseResult,
                wantDebugDistances,
                true);
            lqcf.currentAnimationPoseID = lqcf.GetQueryComputed().featuresData[newPose].animFrame;
            return newPose;
        }
        
        private void HandleNextState(IdleQueryComputedFlow lqcf)
        {
            lqcf.currentState = lqcf.currentState switch
            {
                ActionTagState.Init => ActionTagState.InProgress,
                ActionTagState.InProgress => ActionTagState.InProgress,
                _ => ActionTagState.Init
            };

            if (lqcf.currentState.Equals(ActionTagState.InProgress))
            {
                //Change ranges and call new mmsearch with best idle
                lqcf.currentRanges = lqcf.GetLoopableQueryComputed().GetIdleRanges();
                lqcf.distanceResults = lqcf.idleDistanceResults;
                Counter = 0;
            }

            lqcf.isSearch = true;
        }
        
        public override void ResetCounter(int searchRate)
        {
            Counter = 0;
        }
    }
}
