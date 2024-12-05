using System;
using System.Collections.Generic;
using System.Linq;
using QuanticBrains.MotionMatching.Scripts.Components.Queries;
using QuanticBrains.MotionMatching.Scripts.Components.QueryFlows;
using QuanticBrains.MotionMatching.Scripts.Containers;
using QuanticBrains.MotionMatching.Scripts.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Components
{
    public class TrajectoryEstimation
    {
        private const int Velocity = 0;
        private const int Acceleration = 1;
        private const int AngularVelocity = 2;

        //Prediction lists and variables
        private NativeArray<quaternion> _predictedRots;
        private NativeArray<float3> _predictedDirs;
        private NativeArray<float3> _predictedAngular;
        private NativeArray<float3> _predictedPos;
        private NativeArray<float3> _predictedLocalOffset;
        private NativeArray<float3> _predictedLocalDir;
        private NativeArray<float3> _predictedVel;
        private NativeArray<float3> _predictedAccel;
        private NativeArray<float3> _desiredVelocities;

        //Frame predictions count
        private NativeArray<float> _frameFuturePredictions;

        //Responsiveness
        private readonly float _responsivenessDirections;
        private readonly float _responsivenessPositions;

        private float _lastDeltaTime;

        private FuturePrediction _futureAndPastPrediction;
        private PastTrajectory _pastTrajectory;
        private float3 _lastPosition;
        private float3 _lastForward;
        private float3[] _pastPositions;
        private float3[] _pastForward;
        private int _currentID;

        //Constructor
        public TrajectoryEstimation(int futureEstimates, int pastEstimates, float respDir, float respPos,
            Dataset dataset, float3 position, float3 forward)
        {
            var futureFrameDisplacement = dataset.futureEstimatesTime / futureEstimates;

            _predictedRots = new NativeArray<quaternion>(futureEstimates, Allocator.Persistent);
            _predictedDirs = new NativeArray<float3>(futureEstimates, Allocator.Persistent);
            _predictedPos = new NativeArray<float3>(futureEstimates, Allocator.Persistent);
            _predictedVel = new NativeArray<float3>(futureEstimates, Allocator.Persistent);
            _predictedAccel = new NativeArray<float3>(futureEstimates, Allocator.Persistent);
            _predictedAngular = new NativeArray<float3>(futureEstimates, Allocator.Persistent);
            _desiredVelocities = new NativeArray<float3>(futureEstimates, Allocator.Persistent);

            _frameFuturePredictions = new NativeArray<float>(futureEstimates, Allocator.Persistent);
            _frameFuturePredictions[0] = futureFrameDisplacement;
            for (int i = 1; i < _frameFuturePredictions.Length; i++)
            {
                _frameFuturePredictions[i] = _frameFuturePredictions[i - 1] + futureFrameDisplacement;
            }

            _futureAndPastPrediction = new FuturePrediction();
            _futureAndPastPrediction.Create(futureEstimates);

            _pastTrajectory = new PastTrajectory();
            _pastTrajectory.Create(pastEstimates, forward);

            _pastPositions = Enumerable
                .Repeat(position, (int)math.round(dataset.pastEstimatesTime / Time.fixedDeltaTime)).ToArray();
            _pastForward = Enumerable.Repeat(forward, (int)math.round(dataset.pastEstimatesTime / Time.fixedDeltaTime))
                .ToArray();

            _responsivenessDirections = respDir;
            _responsivenessPositions = respPos;
        }

        //Main method
        public FuturePrediction GetFutureEstimations(
            ref NativeArray<float3> trajectoryFloat3Results,
            QueryComputed queryComputed,
            float4x4 modelInverse,
            Transform characterPos,
            float3 inputMovement,
            float3 inputForward,
            bool isStrafing,
            float deltaTime,
            float avgDeltaTime)
        {
            //Compute new position and direction predictions
            _lastDeltaTime = deltaTime;

            return ComputeNewPrediction(ref trajectoryFloat3Results, queryComputed, modelInverse, characterPos,
                inputMovement, inputForward, isStrafing);
        }

        //Compute new prediction - rotations + positions
        private FuturePrediction ComputeNewPrediction(
            ref NativeArray<float3> trajectoryFloat3Results,
            QueryComputed queryComputed,
            float4x4 modelInverse,
            Transform characterPos,
            float3 inputMovement,
            float3 inputForward,
            bool isStrafing)
        {
            var computeNewPredictionJob = new ComputeNewPredictionJob()
            {
                inputForward = inputForward,
                inputMovement = inputMovement,
                characterPositionUp = characterPos.up,
                characterRotation = characterPos.rotation,
                characterPosition = characterPos.position,
                modelInverse = modelInverse,
                lastDeltaTime = _lastDeltaTime,
                responsivenessDirections = _responsivenessDirections,
                responsivenessPositions = _responsivenessPositions,
                framePredictions = _frameFuturePredictions,
                predictedVel = _predictedVel,
                predictedAccel = _predictedAccel,
                desiredVelocities = _desiredVelocities,
                float3Results = trajectoryFloat3Results,
                FutureAndPastPrediction = _futureAndPastPrediction,
                sideSpeed = queryComputed.sideSpeed,
                forwardSpeed = queryComputed.forwardSpeed,
                backwardSpeed = queryComputed.backwardSpeed,
                isStrafing = isStrafing
            };
            computeNewPredictionJob.Schedule().Complete();

            return _futureAndPastPrediction;
        }

        [BurstCompile]
        private struct ComputeNewPredictionJob : IJob
        {
            [ReadOnly] public float3 inputForward;
            [ReadOnly] public float3 inputMovement;
            [ReadOnly] public float3 characterPositionUp;
            [ReadOnly] public quaternion characterRotation;
            [ReadOnly] public float4x4 modelInverse;
            [ReadOnly] public float3 characterPosition;
            [ReadOnly] public float responsivenessDirections;
            [ReadOnly] public float responsivenessPositions;
            [ReadOnly] public float lastDeltaTime;
            [ReadOnly] public float sideSpeed;
            [ReadOnly] public float forwardSpeed;
            [ReadOnly] public float backwardSpeed;
            [ReadOnly] public bool isStrafing;

            [ReadOnly] public NativeArray<float> framePredictions;

            public NativeArray<float3> desiredVelocities;
            public NativeArray<float3> predictedVel;
            public NativeArray<float3> predictedAccel;
            public NativeArray<float3> float3Results;
            public FuturePrediction FutureAndPastPrediction;

            public void Execute()
            {
                var currentVelocity     = float3Results[Velocity];
                var currentAcceleration = float3Results[Acceleration];
                var currentAngular      = float3Results[AngularVelocity];
                
                float3 desiredSpeed = DesiredVelocity(
                    modelInverse,
                    inputMovement,
                    characterRotation,
                    sideSpeed,
                    forwardSpeed,
                    backwardSpeed
                );
                
                //Rotation - desiredRotation is based on input direction (Y component is based on jump)
                quaternion desiredRotation = 
                    GetDesiredRotation(
                        inputForward, 
                        inputMovement, 
                        characterPositionUp, 
                        characterRotation, 
                        isStrafing);
                
                if (isStrafing)
                {
                    PredictRotations(
                        ref FutureAndPastPrediction.futureStrafingDirections,
                        ref FutureAndPastPrediction.futureGlobalRotations,
                        framePredictions,
                        desiredRotation,
                        desiredRotation,
                        responsivenessDirections,
                        currentAngular,
                        new float3(0, 0, 1));
                    
                    desiredRotation = quaternion.LookRotation(inputForward, characterPositionUp);
                }
                
                PredictRotations(
                    ref FutureAndPastPrediction.futureDirections,
                    ref FutureAndPastPrediction.futureGlobalRotations,
                    framePredictions,
                    characterRotation,
                    desiredRotation,
                    responsivenessDirections,
                    currentAngular,
                    new float3(0, 0, 1));

                PredictDesiredVelocities(ref desiredVelocities,
                    isStrafing ? FutureAndPastPrediction.futureStrafingDirections : FutureAndPastPrediction.futureDirections,
                    FutureAndPastPrediction.futureGlobalRotations,
                    modelInverse,
                    sideSpeed,
                    forwardSpeed,
                    backwardSpeed);

                //Get next frame rotation based on prediction
                GetNewRot(
                    ref currentAngular,
                    characterRotation,
                    desiredRotation,
                    responsivenessDirections,
                    lastDeltaTime);

                // Positions
                PredictPositions(
                    ref FutureAndPastPrediction.futurePositions,
                    ref predictedVel,
                    ref predictedAccel,
                    framePredictions,
                    characterPosition,
                    desiredVelocities,
                    currentVelocity,
                    currentAcceleration,
                    responsivenessPositions);

                //Get next frame position based on prediction
                GetNewPos(
                    ref currentVelocity,
                    ref currentAcceleration,
                    characterPosition,
                    desiredSpeed,
                    responsivenessPositions,
                    lastDeltaTime);

                //Transform predicted world directions and positions to relative local
                TransformToLocal(
                    ref FutureAndPastPrediction.futureOffsets,
                    ref FutureAndPastPrediction.futureOffsetDirections,
                    FutureAndPastPrediction.futurePositions,
                    FutureAndPastPrediction.futureDirections,
                    characterPosition,
                    characterRotation);

                float3Results[Velocity]         = currentVelocity;
                float3Results[Acceleration]     = currentAcceleration;
                float3Results[AngularVelocity]  = currentAngular;
            }
        }

        private static quaternion GetDesiredRotation(
            float3 inputForward,
            float3 inputMovement,
            float3 characterUp,
            quaternion characterRotation,
            bool isStrafing)
        {
            var desiredRot = quaternion.LookRotation(inputForward, characterUp);

            if (isStrafing)
            {
                desiredRot = quaternion.LookRotation(new float3(inputMovement.x, 0, inputMovement.z), characterUp);
            }

            if (inputMovement.Equals(float3.zero))
            {
                desiredRot = characterRotation;
            }

            return desiredRot;
        }
        
        public PastTrajectory ManagePastTrajectory(Transform transform)
        {
            _pastPositions[_currentID] = transform.position;
            _pastForward[_currentID] = transform.forward;

            UpdatePastTrajectory();

            _currentID = (_currentID + 1) % _pastPositions.Length;
            return _pastTrajectory;
        }

        private void UpdatePastTrajectory()
        {
            var pastDisplacement = (float)_pastPositions.Length / _pastTrajectory.pastGlobalPosition.Length;

            for (int i = 0; i < _pastTrajectory.pastGlobalPosition.Length; i++)
            {
                var index = ((int)math.round(pastDisplacement * i) + _currentID + 1) % _pastPositions.Length;
                _pastTrajectory.pastGlobalPosition[^(i + 1)] = _pastPositions[index];
                _pastTrajectory.pastGlobalDirection[^(i + 1)] = _pastForward[index];
            }
        }

        public PastTrajectory FillInPastTrajectory(float3[] positions, float3[] directions)
        {
            if (positions.Length > _pastPositions.Length) 
                return _pastTrajectory;
            if (directions.Length > _pastForward.Length) 
                return _pastTrajectory;
            
            //Fill pastPositions and pastForward
            _pastPositions = positions;
            _pastForward = directions;

            // Set currentID to latest (Length - 1) and update trajectory
            _currentID = _pastPositions.Length - 1;
            
            UpdatePastTrajectory();
            
            // Return PastTrajectory filled with input lists - using same method as previous one
            return _pastTrajectory;
        }

        //Predict rotations
        private static void PredictRotations(
            ref NativeArray<float3> predictedDirs,
            ref NativeArray<quaternion> predictedGlobalRot,
            NativeArray<float> framePredictions,
            quaternion currentRot,
            quaternion desiredRot,
            float responsivenessDirections,
            float3 angularVelocity,
            float3 forward)
        {

            for (int i = 0; i < predictedDirs.Length; i++)
            {
                var currentPredictedRot = currentRot;
                var currentAngular = angularVelocity;
                SpringUtils.SpringRotationPrediction(ref currentPredictedRot, ref currentAngular, desiredRot,
                    1 - responsivenessDirections, framePredictions[i]);
                
                predictedDirs[i] = math.mul(currentPredictedRot, forward);
                predictedGlobalRot[i] = currentPredictedRot;
            }
        }

        private static void PredictDesiredVelocities(
            ref NativeArray<float3> predictedVelocities,
            NativeArray<float3> predictedDirections,
            NativeArray<quaternion> predictedRotations,
            float4x4 modelInverse,
            float sideSpeed,
            float forwardSpeed,
            float backwardSpeed)
        {
            for (int i = 0; i < predictedVelocities.Length; i++)
            {
                predictedVelocities[i] = 
                    DesiredVelocity(
                        modelInverse, 
                        predictedDirections[i], 
                        predictedRotations[i], 
                        sideSpeed, 
                        forwardSpeed, 
                        backwardSpeed);
            }
        }

        private static float3 DesiredVelocity(
            float4x4 modelInverse, 
            float3 currentGlobalDirection,
            quaternion currentGlobalRotation,
            float sideSpeed,
            float forwardSpeed,
            float backwardSpeed)
        {
            if (math.all(currentGlobalDirection == float3.zero))
            {
                return float3.zero;
            }

            var localDir = math.normalize(math.mul(modelInverse, new float4(currentGlobalDirection, 0.0f)).xyz);

            var localVelocity = localDir.z > 0.0f
                ? new float3(sideSpeed, 0.0f, forwardSpeed) * localDir
                : new float3(sideSpeed, 0.0f, backwardSpeed) * localDir;

            return math.mul(currentGlobalRotation, localVelocity);
        }
        
        //Predict positions
        private static void PredictPositions(
            ref NativeArray<float3> predictedPos,
            ref NativeArray<float3> predictedVel,
            ref NativeArray<float3> predictedAccel,
            NativeArray<float> framePredictions,
            float3 currentPos,
            NativeArray<float3> desiredVelocities,
            //float3 desiredSpeed,
            float3 velocity,
            float3 acceleration,
            float responsivenessPositions)
        {
            float lastPredictionFrame = 0;
            for (int i = 0; i < predictedPos.Length; i++)
            {
                //Always use the latest computed values
                var currentPredictedPos = i == 0 ? currentPos : predictedPos[i - 1];
                var currentPredictedVel = i == 0 ? velocity : predictedVel[i - 1];
                var currentPredictedAccel = i == 0 ? acceleration : predictedAccel[i - 1];

                //Apply spring prediction
                float diffPredictionFrames = framePredictions[i] - lastPredictionFrame;
                lastPredictionFrame = framePredictions[i];

                SpringUtils.SpringPositionPrediction(ref currentPredictedPos, ref currentPredictedVel,
                    ref currentPredictedAccel,
                    desiredVelocities[i], 1.0f - responsivenessPositions, diffPredictionFrames);

                predictedPos[i] = currentPredictedPos;
                predictedVel[i] = currentPredictedVel;
                predictedAccel[i] = currentPredictedAccel;
            }
        }

        private static void TransformToLocal(
            ref NativeArray<float3> predictedLocalPos,
            ref NativeArray<float3> predictedLocalDir,
            NativeArray<float3> predictedPos,
            NativeArray<float3> predictedDirs,
            float3 originalPos,
            quaternion originalRot)
        {
            float4x4 inverseModel = math.inverse(
                float4x4.TRS(
                    originalPos,
                    originalRot,
                    new float3(1)
                ));
            for (int i = 0; i < predictedLocalPos.Length; i++)
            {
                //Get Local position and direction
                predictedLocalPos[i] = math.transform(inverseModel, predictedPos[i]);
                predictedLocalDir[i] = math.normalize(math.mul(inverseModel, new float4(predictedDirs[i], 0.0f)).xyz);
            }
        }

        /// Returns the predicted position at the next frame
        private static float3 GetNewPos(
            ref float3 velocity,
            ref float3 acceleration,
            float3 currentPos,
            float3 desiredSpeed,
            float responsivenessPositions,
            float lastDeltaTime)
        {
            SpringUtils.SpringPositionPrediction(ref currentPos, ref velocity, ref acceleration, desiredSpeed,
                1.0f - responsivenessPositions, lastDeltaTime);
            return currentPos;
        }

        /// Returns the predicted rotation at the next fram
        private static quaternion GetNewRot(ref float3 angularVelocity, quaternion currentRot, quaternion desiredRot,
            float responsivenessDirections, float lastDeltaTime)
        {
            SpringUtils.SpringRotationPrediction(ref currentRot, ref angularVelocity, desiredRot,
                1.0f - responsivenessDirections, lastDeltaTime);
            return currentRot;
        }


        /// DEBUG GIZMOS ///

        //Debug gizmos
        public void DrawGizmos(Transform root, Transform[] characterTransforms, FuturePrediction nextPrediction,
            PastTrajectory pastTrajectory, QueryComputedFlow query)
        {
            float height = root.position.y + 0.05f;
            DebugRootProperties(height, root);
            DebugTrajectoryPredictions(height, root, nextPrediction);
            DebugPastTrajectory(height, root.position, pastTrajectory);
            DebugRecordedFutureTrajectory(height, root, query.GetQueryComputed(), query.currentFeatureID);
            DebugRecordedPastTrajectory(height, root, query.GetQueryComputed(), query.currentFeatureID);
            DebugCharacteristics(height, characterTransforms, query.bonesWeights.weights);
        }

        private void DebugRootProperties(float height, Transform root)
        {
            //Draw current root position projected
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(new float3(root.transform.position.x, height, root.transform.position.z), 0.05f);
            GizmosEx.DrawArrow(new float3(root.transform.position.x, height, root.transform.position.z),
                root.transform.forward, 0.15f);
        }

        private void DebugCharacteristics(float height, Transform[] characterTransforms, NativeArray<float> weights)
        {
            //Get selected features current velocity and position
            for (int i = 0; i < characterTransforms.Length; i++)
            {
                if (characterTransforms[i] == null)
                {
                    continue;
                }

                if (weights[i * 2] == 0)
                {
                    continue;
                }

                var pos = characterTransforms[i].transform.position;

                //Draw next prediction
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(pos, 0.05f);
            }
        }

        private void DebugTrajectoryPredictions(float height, float3 position, FuturePrediction nextPrediction)
        {
            //Draw position and trajectory predictions
            for (int i = -1; i < nextPrediction.futurePositions.Length - 1; i++)
            {
                float2 A, C;

                if (i == -1)
                {
                    //At start, A is current pos
                    A = position.xz;
                    //C is the first prediction
                    C = nextPrediction.futurePositions[i + 1].xz;
                }
                else
                {
                    //Compute curve for next section - actual position to next prediction
                    A = nextPrediction.futurePositions[i].xz;
                    C = nextPrediction.futurePositions[i + 1].xz;
                }

                //Draw next prediction
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(new float3(C.x, height, C.y), 0.05f);

                Gizmos.color = Color.yellow;
                GizmosEx.DrawArrow(new float3(C.x, height, C.y), nextPrediction.futureDirections[i + 1] * 0.3f, 0.15f);

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(new float3(A.x, height, A.y),
                    new float3(C.x, height, C.y));
            }
        }

        private void DebugTrajectoryPredictions(float height, Transform root, FuturePrediction nextPrediction)
        {
            //Compute model and current pos and rot
            float3 rootPos = root.position;
            quaternion rootRot = root.rotation;

            var originalModel4x4 =
                float4x4.TRS(rootPos,
                    rootRot, new float3(1));

            //Draw position and trajectory predictions
            for (int i = -1; i < nextPrediction.futureOffsets.Length - 1; i++)
            {
                var posInGlobalSpace = math.transform(
                    originalModel4x4,
                    nextPrediction.futureOffsets[i + 1]);

                var dirInGlobalSpace =
                    math.normalize(math.mul(originalModel4x4,
                        new float4(nextPrediction.futureOffsetDirections[i + 1], 0.0f)).xyz);

                float2 A = rootPos.xz;

                rootPos = posInGlobalSpace;

                float2 C = rootPos.xz;

                //Draw next prediction
                Gizmos.color = new Color(0.1f, 0.27f, 0.11f);
                Gizmos.DrawSphere(new float3(C.x, height, C.y), 0.05f);
                Gizmos.color = Color.green;
                GizmosEx.DrawArrow(new float3(C.x, height, C.y), dirInGlobalSpace * 0.3f, 0.15f);
                Gizmos.color = new Color(0.635f, 0.98f, 0.635f);
                Gizmos.DrawLine(new float3(A.x, height, A.y),
                    new float3(C.x, height, C.y));
            }
        }

        private void DebugPastTrajectory(float height, float3 position, PastTrajectory pastPoses)
        {
            //Draw position and trajectory predictions
            for (int i = -1; i < pastPoses.pastGlobalPosition.Length - 1; i++)
            {
                float2 A, C;

                if (i == -1)
                {
                    //At start, A is current pos
                    A = position.xz;
                    //C is the first prediction
                    C = pastPoses.pastGlobalPosition[i + 1].xz;
                }
                else
                {
                    //Compute curve for next section - actual position to next prediction
                    A = pastPoses.pastGlobalPosition[i].xz;
                    C = pastPoses.pastGlobalPosition[i + 1].xz;
                }

                //Draw next prediction
                //Gizmos.color = new Color(0.1f, 0.27f, 0.11f);
                Gizmos.color = new Color(0, 204f, 102f);
                Gizmos.DrawSphere(new float3(C.x, height, C.y), 0.05f);

                //Gizmos.color = Color.green;
                GizmosEx.DrawArrow(new float3(C.x, height, C.y), pastPoses.pastGlobalDirection[i + 1] * 0.3f, 0.15f);

                //Gizmos.color = new Color(0.635f, 0.98f, 0.635f);
                Gizmos.DrawLine(new float3(A.x, height, A.y),
                    new float3(C.x, height, C.y));
            }
        }

        private void DebugRecordedFutureTrajectory(float height, Transform root, QueryComputed query,
            int currentFeatureID)
        {
            //Draw in red current animation predictions
            float3[] animationFutureOffsets = new float3[query.featuresData[currentFeatureID].futureOffsets.Length];
            float3[] animationFutureDirections = new float3[query.featuresData[currentFeatureID].futureOffsets.Length];

            //De-normalize offsets and directions
            for (int i = 0; i < query.featuresData[currentFeatureID].futureOffsets.Length; i++)
            {
                animationFutureOffsets[i] = query.featuresData[currentFeatureID].futureOffsets[i]
                    * query.stdFutureOffset[i] + query.meanFutureOffset[i];

                animationFutureDirections[i] = query.featuresData[currentFeatureID].futureDirections[i]
                    * query.stdFutureDirection[i] + query.meanFutureDirection[i];
            }

            //Compute model and current pos and rot
            float3 rootPos = root.position;
            quaternion rootRot = root.rotation;

            var originalModel4x4 =
                float4x4.TRS(rootPos,
                    rootRot, new float3(1));

            //Draw position and trajectory predictions
            for (int i = -1; i < animationFutureOffsets.Length - 1; i++)
            {
                var posInGlobalSpace = math.transform(
                    originalModel4x4,
                    animationFutureOffsets[i + 1]);

                var dirInGlobalSpace =
                    math.normalize(math.mul(originalModel4x4, new float4(animationFutureDirections[i + 1], 0.0f)).xyz);

                float2 A = rootPos.xz;

                rootPos = posInGlobalSpace;

                float2 C = rootPos.xz;

                //Draw next prediction
                Gizmos.color = new Color(0.8f, 0.1f, 0.1f);
                Gizmos.DrawSphere(new float3(C.x, height, C.y), 0.05f);

                Gizmos.color = Color.red;
                GizmosEx.DrawArrow(new float3(C.x, height, C.y), dirInGlobalSpace * 0.3f, 0.15f);

                Gizmos.color = new Color(0.7f, 0.2f, 0.2f);
                Gizmos.DrawLine(new float3(A.x, height, A.y),
                    new float3(C.x, height, C.y));
            }
        }

        private void DebugRecordedPastTrajectory(float height, Transform root, QueryComputed query,
            int currentFeatureID)
        {
            //Draw in red current animation predictions
            float3[] animationPastOffsets = new float3[query.featuresData[currentFeatureID].pastOffsets.Length];
            float3[] animationPastDirections = new float3[query.featuresData[currentFeatureID].pastOffsets.Length];

            //De-normalize offsets and directions
            for (int i = 0; i < query.featuresData[currentFeatureID].pastOffsets.Length; i++)
            {
                animationPastOffsets[i] = query.featuresData[currentFeatureID].pastOffsets[i]
                    * query.stdPastOffset[i] + query.meanPastOffset[i];

                animationPastDirections[i] = query.featuresData[currentFeatureID].pastDirections[i]
                    * query.stdPastDirection[i] + query.meanPastDirection[i];
            }

            //Compute model and current pos and rot
            float3 rootPos = root.transform.position;
            quaternion rootRot = root.transform.rotation;

            var originalModel4x4 =
                float4x4.TRS(rootPos,
                    rootRot, new float3(1));

            //Draw position and trajectory predictions
            for (int i = -1; i < animationPastOffsets.Length - 1; i++)
            {
                var posInGlobalSpace = math.transform(
                    originalModel4x4,
                    animationPastOffsets[i + 1]);

                var dirInGlobalSpace =
                    math.normalize(math.mul(originalModel4x4, new float4(animationPastDirections[i + 1], 0.0f)).xyz);

                float2 A = rootPos.xz;

                rootPos = posInGlobalSpace;

                float2 C = rootPos.xz;

                //Draw next prediction
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(new float3(C.x, height, C.y), 0.05f);
                
                GizmosEx.DrawArrow(new float3(C.x, height, C.y), dirInGlobalSpace * 0.3f, 0.15f);
                
                Gizmos.DrawLine(new float3(A.x, height, A.y),
                    new float3(C.x, height, C.y));
            }
        }

        public void Destroy()
        {
            _predictedRots.Dispose();
            _predictedDirs.Dispose();
            _predictedAngular.Dispose();
            _desiredVelocities.Dispose();
            _predictedPos.Dispose();
            _predictedLocalOffset.Dispose();
            _predictedLocalDir.Dispose();
            _predictedVel.Dispose();
            _predictedAccel.Dispose();
            _frameFuturePredictions.Dispose();
            _futureAndPastPrediction.Destroy();
            _pastTrajectory.Destroy();
        }
    }

    /// STRUCTS ///
    //Future prediction struct
    [Serializable]
    public struct FuturePrediction
    {
        public NativeArray<float3> futurePositions;
        public NativeArray<quaternion> futureGlobalRotations;
        public NativeArray<float3> futureDirections;
        public NativeArray<float3> futureStrafingDirections;
        public NativeArray<float3> futureOffsets;
        public NativeArray<float3> futureOffsetDirections;

        public void Create(int futurePredictions)
        {
            futurePositions = new NativeArray<float3>(futurePredictions, Allocator.Persistent);
            futureDirections = new NativeArray<float3>(futurePredictions, Allocator.Persistent);
            futureStrafingDirections = new NativeArray<float3>(futurePredictions, Allocator.Persistent);
            futureGlobalRotations = new NativeArray<quaternion>(futurePredictions, Allocator.Persistent);
            futureOffsets = new NativeArray<float3>(futurePredictions, Allocator.Persistent);
            futureOffsetDirections = new NativeArray<float3>(futurePredictions, Allocator.Persistent);
        }

        public override string ToString()
        {
            return "[" + futurePositions[0] + futurePositions[1] + futurePositions[2] + "] - [" + futureDirections[0] +
                   futureDirections[1] + futureDirections[2] + "]";
        }

        public void Destroy()
        {
            futurePositions.Dispose();
            futureDirections.Dispose();
            futureStrafingDirections.Dispose();
            futureGlobalRotations.Dispose();
            futureOffsets.Dispose();
            futureOffsetDirections.Dispose();
        }
    }

    public struct PastTrajectory
    {
        public NativeArray<float3> pastGlobalDirection;
        public NativeArray<float3> pastGlobalPosition;

        public void Create(int pastEstimates, float3 forward)
        {
            pastGlobalDirection = new NativeArray<float3>(pastEstimates, Allocator.Persistent);
            pastGlobalPosition = new NativeArray<float3>(pastEstimates, Allocator.Persistent);

            for (int i = 0; i < pastEstimates; i++)
            {
                pastGlobalDirection[i] = forward;
                pastGlobalPosition[i] = float3.zero;
            }
        }

        public override string ToString()
        {
            return "[" + pastGlobalDirection[0] + pastGlobalDirection[1] + pastGlobalDirection[2] + "] - [" +
                   pastGlobalPosition[0] +
                   pastGlobalPosition[1] + pastGlobalPosition[2] + "]";
        }

        public void Destroy()
        {
            pastGlobalDirection.Dispose();
            pastGlobalPosition.Dispose();
        }
    }
}
