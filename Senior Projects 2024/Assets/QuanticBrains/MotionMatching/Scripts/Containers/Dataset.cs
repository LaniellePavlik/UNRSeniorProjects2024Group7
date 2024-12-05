using System;
using System.Collections.Generic;
using System.Linq;
using QuanticBrains.MotionMatching.Scripts.Components.Queries;
using QuanticBrains.MotionMatching.Scripts.Containers.CustomAvatars;
using QuanticBrains.MotionMatching.Scripts.CustomAttributes;
using Unity.Mathematics;
using UnityEngine;
using QuanticBrains.MotionMatching.Scripts.Extensions;
using Unity.Collections;

namespace QuanticBrains.MotionMatching.Scripts.Containers
{
    public class Dataset : ScriptableObject, ISerializationCallbackReceiver
    {
        public List<List<AnimationDataNative>> animationsDataNative;
        [ShowOnly]
        public CustomAvatar avatar;
        [ShowOnly]
        public int futureEstimates;
        [ShowOnly]
        public float futureEstimatesTime;
        [ShowOnly]
        public int pastEstimates;
        [ShowOnly]
        public float pastEstimatesTime;
        [HideInInspector]
        public List<List<AnimationData>> animationsData;
        [HideInInspector]
        public List<AnimationDataTemp> animationsDataTemp = new();
        [HideInInspector] 
        public quaternion[] originalBonesRotation;
        [HideInInspector]
        public float poseStep;
        [HideInInspector]
        public int recordVelocity;
        [HideInInspector]
        public List<int> lastAnimationPoses;
        [ShowOnly]
        public QueriesComputed queriesComputed;
        [ShowOnly]
        public Characteristics characteristics;
        [ShowOnly]
        public Tags tagsList;
        [ShowOnly]
        public List<string> animationPaths;
        
        public void Initialize(int futureEstimates, float futureEstimatesTime, int pastEstimates, float pastEstimatesTime)
        {
            this.futureEstimates = futureEstimates;
            this.futureEstimatesTime = futureEstimatesTime;
            this.pastEstimates   = pastEstimates;
            this.pastEstimatesTime = pastEstimatesTime;
            animationsData = new List<List<AnimationData>>();
            lastAnimationPoses = new List<int>();
        }

        public void LoadData()
        {
            LoadAnimationsData();
        }

        private void LoadAnimationsData()
        {
            if (animationsDataNative != null)
            {
                return;
            }
            animationsDataNative = new List<List<AnimationDataNative>>();
            for (int i = 0; i < animationsData.Count; i++)
            {
                var animationPoses = new List<AnimationDataNative>();
                for (int j = 0; j < animationsData[i].Count; j++)
                {
                    animationPoses.Add(new AnimationDataNative()
                    {
                        bonesData = new NativeArray<BoneData>(animationsData[i][j].bonesData, Allocator.Persistent),
                        isLoop = animationsData[i][j].isLoop,
                        rootPosition = animationsData[i][j].rootPosition,
                        rootRotation = animationsData[i][j].rootRotation
                    });
                }
                animationsDataNative.Add(animationPoses);
            }
        }

        public AnimationDataNative GetAnimationDataNativeFromFeature(int featureID, QueryComputed queryComputed)
        {
            var feature = queryComputed.featuresData[featureID];
            return animationsDataNative[feature.animationID][feature.animFrame];
        }
        
        public AnimationData GetAnimationDataFromFeature(int featureID, QueryComputed queryComputed)
        {
            var feature = queryComputed.featuresData[featureID];
            return animationsData[feature.animationID][feature.animFrame];
        }
        
        public List<AnimationData> GetAnimationPoses(int featureID, QueryComputed queryComputed)
        {
            var feature = queryComputed.featuresData[featureID];
            return animationsData[feature.animationID];
        }
        
        public List<AnimationDataNative> GetAnimationNativePoses(int featureID, QueryComputed queryComputed)
        {
            var feature = queryComputed.featuresData[featureID];
            return animationsDataNative[feature.animationID];
        }
        
        public void SetAnimationBoneData(
            int animationID,
            int animationFrame,
            int boneDatasLength,
            int boneID,
            float3 position,
            float3 localPosition,
            float3 scale,
            float3 velocity,
            float3 localVelocity,
            float3 angularVelocity,
            quaternion rotation)
        {
            AnimationData data = GetAnimationData(animationID, animationFrame, boneDatasLength);
            BoneData current = new BoneData
            {
                position = position,
                localPosition = localPosition,
                scale = scale,
                velocity = velocity,
                localVelocity = localVelocity,
                angularVelocity = angularVelocity,
                rotation = rotation,
                isValid = true
            };
            
            var index = boneID;
            data.bonesData[index] = current;
            
            animationsData[animationID].AddOrReplace(animationFrame, data);
        }
        
        public void SetAnimationIsLooping(
            int animationID,
            int animationFrame,
            int boneDatasLength)
        {
            AnimationData data = GetAnimationData(animationID, animationFrame, boneDatasLength);
            data.isLoop = true;
            animationsData[animationID].AddOrReplace(animationFrame, data);
        }

        public void SetAnimationRootData(
            int animationID,
            int animationFrame,
            int boneDatasLength,
            quaternion rootRotation,
            float3 rootPosition)
        {
            AnimationData data = GetAnimationData(animationID, animationFrame, boneDatasLength);

            data.rootPosition = rootPosition;
            data.rootRotation = rootRotation;
            animationsData[animationID].AddOrReplace(animationFrame, data);
        }

        private AnimationData GetAnimationData(int animationID, int animationFrame, int boneDatasLength)
        {
            if (animationsData.Count > animationID)
            {
                if (animationsData[animationID].Count > animationFrame)
                {
                    return animationsData[animationID][animationFrame];
                }
                
                return new AnimationData
                {
                    bonesData = new BoneData[boneDatasLength]
                };
            }
            
            animationsData.AddOrReplace(animationID, new List<AnimationData>());
            return new AnimationData
            {
                bonesData = new BoneData[boneDatasLength]
            };
        }

        public void OnBeforeSerialize()
        {
            animationsDataTemp?.Clear();
            if (animationsData == null) return;
            
            for (int i = 0; i < animationsData.Count; i++)
            {
                for (int j = 0; j < animationsData[i].Count; j++)
                {
                    AnimationData anim = animationsData[i][j];
                    animationsDataTemp.Add(new AnimationDataTemp
                    {
                        animationID = i,
                        keyFrame = j,
                        bonesData = anim.bonesData,
                        rootPosition = anim.rootPosition,
                        rootRotation = anim.rootRotation,
                        isLoop = anim.isLoop
                    });
                }
            }
        }

        public void OnAfterDeserialize()
        {
            animationsData = new List<List<AnimationData>>();
            foreach (var animationDataTemp in animationsDataTemp)
            {
                if (animationsData.Count <= animationDataTemp.animationID)
                {
                    animationsData.Insert(animationDataTemp.animationID, new List<AnimationData>());
                }
                
                animationsData[animationDataTemp.animationID].Insert(
                    animationDataTemp.keyFrame,
                    new AnimationData
                    {
                        isLoop = animationDataTemp.isLoop,
                        bonesData = animationDataTemp.bonesData,
                        rootPosition = animationDataTemp.rootPosition,
                        rootRotation = animationDataTemp.rootRotation
                    });
            }
        }

        public void Unload()
        {
            if (animationsDataNative == null)
            {
                return;
            }

            foreach (var animation in animationsDataNative)
            {
                foreach (var pose in animation)
                {
                    var bonesData = pose.bonesData;
                    bonesData.Dispose();
                }
            }

            animationsDataNative = null;
        }
    }

    [Serializable]
    public struct FeatureData
    {
        public int animationID;
        public int animFrame;
        public float3[] positionsAndVelocities;
        public float3[] futureOffsets;
        public float3[] futureDirections;
        public float3[] pastOffsets;
        public float3[] pastDirections;

        public (float3[], float3[]) UnNormalizeValues(float3[] meansPos, float3[] stdsPos, float3[] meansVel, float3[] stdsVel, float[] weights)
        {
            float3[] unNormalizedPos = new float3[weights.Count(w => w == 1f) / 2];
            float3[] unNormalizedVel = new float3[weights.Count(w => w == 1f) / 2];
            int counterPos = 0;
            int counterVel = 0;
            for (int i = 0; i < positionsAndVelocities.Length; i++)
            {
                if (weights[i] == 0)
                {
                    continue;
                }

                float3 real;
                if (i % 2 == 0)
                {
                    real = positionsAndVelocities[i] * stdsVel[i / 2] + meansVel[i / 2];
                    unNormalizedVel[counterVel] = real;
                    counterVel++;
                    continue;
                }
                real = positionsAndVelocities[i] * stdsPos[i / 2] + meansPos[i / 2];
                unNormalizedPos[counterPos] = real;
                counterPos++;
            }

            return (unNormalizedPos, unNormalizedVel);
        }

        public override string ToString()
        {
            string result = "[" + animationID + "][" + animFrame +"]";
            result += "\nOffsets \n";
            futureOffsets.ForEach(offsets =>
            {
                result += offsets;
            });
            result += "\nDirections \n";
            futureDirections.ForEach(dir =>
            {
                result += dir;
            });

            result += "\nBones \n";
            positionsAndVelocities.ForEach(pos =>
            {
                result += pos;
            });
            return result;
        }
    }
    
    [Serializable]
    public struct AnimationData
    {
        public quaternion rootRotation;
        public float3 rootPosition;
        public BoneData[] bonesData;
        public bool isLoop;
    }

    [Serializable]
    public struct BoneData
    {
        public bool isValid;
        public float3 position;
        public float3 localPosition;
        public float3 scale;
        public float3 velocity;
        public float3 localVelocity;
        public float3 angularVelocity;
        public quaternion rotation;
    }

    [Serializable]
    public struct AnimationDataTemp
    {
        public bool isLoop;
        public int animationID;
        public int keyFrame;
        public BoneData[] bonesData;
        public quaternion rootRotation;
        public float3 rootPosition;
    }
    
    public struct AnimationDataNative
    {
        public bool isLoop;
        public NativeArray<BoneData> bonesData;
        public quaternion rootRotation;
        public float3 rootPosition;
    }

    public struct FeatureDataAnim
    {
        public int animationID;
        public int animFrame;
    }
}
