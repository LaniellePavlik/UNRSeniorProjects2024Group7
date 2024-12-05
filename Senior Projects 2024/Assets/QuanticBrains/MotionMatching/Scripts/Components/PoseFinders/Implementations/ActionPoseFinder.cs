using QuanticBrains.MotionMatching.Scripts.Components.PoseFinders.Models;
using QuanticBrains.MotionMatching.Scripts.Components.QueryFlows;
using QuanticBrains.MotionMatching.Scripts.Components.QueryFlows.Implementations;
using QuanticBrains.MotionMatching.Scripts.Tags;
using Unity.Collections;
using Unity.Mathematics;

namespace QuanticBrains.MotionMatching.Scripts.Components.PoseFinders.Implementations
{
    public class ActionPoseFinder : PoseFinder
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
            bool wantDebugDistance, 
            bool forceContinuousPose)
        {
            currentMinDistance = 0;
            ActionQueryComputedFlow aqcf = (ActionQueryComputedFlow)queryComputedFlow;
            var currentFeatures = aqcf.GetQueryComputed().featuresData[queryComputedFlow.currentFeatureID];
            aqcf.CurrentAnimationPoseID = currentFeatures.animFrame + 1;
            if (!aqcf.FirstFrame)
            {
                //aqcf.CurrentAnimationPoseID = 0;
                aqcf.FirstFrame = true;
                return queryComputedFlow.currentFeatureID;
            }
            
            /*if (aqcf.CurrentAnimationPoseID == -1)
            {
                aqcf.CurrentAnimationPoseID = 0;
                return aqcf.currentFeatureID;
            }*/
            
            var nextFrameID = aqcf.CurrentAnimationPoseID + 1;
            
            var animationPoses = aqcf.dataset.animationsData[currentFeatures.animationID];

            if (animationPoses.Count > nextFrameID)
            {
                aqcf.isSearch = false;
                return queryComputedFlow.currentFeatureID + 1;
            }
            
            //Go to Next State
            HandleNextState(aqcf);

            //Check init warping and collisions+physics
            aqcf.CheckInitWarpingPropertiesAndPhysicsSetup();
            
            UpdateAnimationIndexes(aqcf);
            return aqcf.currentFeatureID;
        }
        
        public void HandleNextState(ActionQueryComputedFlow aqcf)
        {
            aqcf.CurrentState = aqcf.CurrentState switch
            {
                ActionTagState.Init => ActionTagState.InProgress,
                ActionTagState.InProgress => ActionTagState.Recovery,
                _ => ActionTagState.Init
            };

            //Check whether this Action has init and recovery states
            ActionTag actionTag = aqcf.GetActionQueryComputed().actionTag;
            if (!actionTag.HasRecoveryState() && aqcf.CurrentState == ActionTagState.Recovery)
                aqcf.CurrentState = ActionTagState.Init;

            if (aqcf.CurrentState == ActionTagState.Init)
            {
                //Set isDone = true
                aqcf.isQueryDone = true;
                if (!actionTag.HasInitState())
                {
                    aqcf.CurrentState = ActionTagState.InProgress;
                }

                return;
            }
            aqcf.isSearch = true;
        }
        
        public void UpdateAnimationIndexes(ActionQueryComputedFlow aqcf)
        {
            if (aqcf.isQueryDone)
            {
                aqcf.CurrentAnimationPoseID = -1;
                return;
            }
            int state = (int)aqcf.CurrentState;
            aqcf.CurrentAnimationPoseID = -1;//aqcf.GetActionQueryComputed().actionTag.ranges[state].frameStart;
            aqcf.currentFeatureID = aqcf.GetRanges()[state].featureIDStart; //aqcf.GetActionQueryComputed().actionTag.ranges[state].poseStart;
        }
        
        public void UpdateAnimationIndexesByTime(ActionQueryComputedFlow aqcf, float time)
        {
            int state = (int)aqcf.CurrentState;
            
            //Get current feature by state time
            int currentFeature =
                (int)((aqcf.GetRanges()[state].featureIDStop - aqcf.GetRanges()[state].featureIDStart) * time +
                aqcf.GetRanges()[state].featureIDStart);
            
            //Apply it to pose and feature
            aqcf.currentFeatureID = currentFeature;
            aqcf.CurrentAnimationPoseID = aqcf.GetQueryComputed().featuresData[aqcf.currentFeatureID].animFrame; 
        }
    }
}
