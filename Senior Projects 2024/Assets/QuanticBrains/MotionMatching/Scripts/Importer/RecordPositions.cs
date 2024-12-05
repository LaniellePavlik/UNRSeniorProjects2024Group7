#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuanticBrains.MotionMatching.Scripts.Components.Queries;
using QuanticBrains.MotionMatching.Scripts.Components.Queries.Implementations;
using QuanticBrains.MotionMatching.Scripts.Components.Queries.Models;
using QuanticBrains.MotionMatching.Scripts.Containers;
using QuanticBrains.MotionMatching.Scripts.Containers.CustomAvatars;
using QuanticBrains.MotionMatching.Scripts.Extensions;
using QuanticBrains.MotionMatching.Scripts.Tags;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using MathUtils = QuanticBrains.MotionMatching.Scripts.Extensions.MathUtils;

namespace QuanticBrains.MotionMatching.Scripts.Importer
{
    public class RecordPositions : MonoBehaviour
    {
        private Dataset _dataset;
        private List<FeatureData> _tempFeaturesData;
        private Animator _animator;
        private List<AnimationClip> _animClips;
        private float _poseStep;
        private string _databaseName;
        private Coroutine _currentCoroutine;
        private CustomAvatar _avatar;
        private readonly List<float3> _previousBonesPosition = new();
        private readonly List<float3> _previousBonesLocalPosition = new();
        private readonly List<quaternion> _previousBonesRotation = new();
        private float _initialDeltaTime;
        private Transform _root;
        private int _recordVelocity;

        private float3 _previousRootPosition;
        private quaternion _previousRootRotation;

        private bool _isNextAnimation;
        private int _currentAnimIndex;
        private float _lengthAnimationRecorded;
        private int _currentFrame;
        private int _currentFeatureID;
        private AnimationClip _currentAnimation;
        private float _rateToCalculateFuture;
        private float _rateToCalculatePast;

        private float3 _lastOffsetFuturePositions;
        private quaternion _lastOffsetFutureRotations;

        private float _recordFixedUpdate;
        private List<List<(float3, quaternion, float3)>> _positionsByFrame;
        private (int, int) _lastFrameForFuture;
        private int _framesToGoBack;
        private int _framesToGoBackInPast;
        private int _lastAnimIndex;
        
        private Transform[] _characterTransforms;
        private int _featuresLength;
        private int _bonesDataLength;

        private List<BoneCharacteristic> _bonesCharacteristics;
        private IEnumerable<List<TagBase>> _combinations;

        private Dictionary<string, (int, int)> _rangesByAnim;
        private Quaternion _startingRotation;
        private Vector3 _startingPosition;

        private quaternion[] _originalRotations;

        private bool _startRecord;

        private bool _animationFinished;
        private int _futureEstimates;
        private int _pastEstimates;

        /*private bool[] _stdChecker;
        private bool[] _meanChecker;
        private bool[] _normalizeChecker;*/

        private void OnDestroy()
        {
            Time.fixedDeltaTime = _initialDeltaTime;
        }

        private void Initialize()
        {
            _tempFeaturesData = new List<FeatureData>();
            _rangesByAnim = new Dictionary<string, (int, int)>();
            _animator = gameObject.GetComponent<Animator>();
            
            for (var i = 0; i < _avatar.Length; i++)
            {
                _previousBonesPosition.Add(new float3(math.NAN, math.NAN, math.NAN));
                _previousBonesLocalPosition.Add(new float3(math.NAN, math.NAN, math.NAN));
                _previousBonesRotation.Add(new quaternion(math.NAN, math.NAN, math.NAN, math.NAN));
            }
        }

        public void ProcessData(ref List<AnimationClip> animClips, List<string> animPaths,
            CustomAvatar customAvatar,
            float poseStep,
            int futureEstimates,
            float futureEstimatesTime,
            int pastEstimates,
            float pastEstimatesTime,
            string databaseName,
            Transform root,
            int recordVelocity,
            IEnumerable<List<TagBase>> combinations,
            List<TagBase> tags,
            List<ActionTag> actionTags,
            List<IdleTag> idleTags,
            List<BoneCharacteristic> characteristics,
            RuntimeAnimatorController rac
        )
        {
            _futureEstimates = futureEstimates;
            _pastEstimates = pastEstimates;
            _bonesCharacteristics = characteristics;
            _positionsByFrame = new List<List<(float3, quaternion, float3)>>();
            _initialDeltaTime = Time.fixedDeltaTime;
            
            _avatar = customAvatar;
            
            _featuresLength = _avatar.Length * 2;
            _bonesDataLength = _avatar.Length;

            _characterTransforms = _avatar.GetCharacterTransforms(root, null);
            if(_avatar is HumanoidAvatar humanoidAvatar)
                InitBonesAndRotations(humanoidAvatar, root);    //Avatar to T-Pose - on humanoid
            
            Initialize();

            if (_currentCoroutine != null)
            {
                Debug.LogError("Already processing the dataset.");
                return;
            }

            _recordVelocity = recordVelocity;
            _root = root;
            _startingRotation = root.rotation;
            _startingPosition = root.position;

            _recordFixedUpdate = poseStep / _recordVelocity;
            Time.fixedDeltaTime = _recordFixedUpdate;

            /*var stepsBySecond = 1 / poseStep;
            _rateToCalculateFuture = stepsBySecond / _futureEstimates;
            _rateToCalculatePast = stepsBySecond / _pastEstimates;
            _framesToGoBack = (int)Math.Floor(_rateToCalculateFuture);
            _framesToGoBackInPast = (int)Math.Floor(_rateToCalculatePast);*/
            
            var stepsBySecond = futureEstimatesTime / poseStep;
            var stepsInPast = pastEstimatesTime / poseStep; 
            _rateToCalculateFuture = stepsBySecond / _futureEstimates;
            _rateToCalculatePast = stepsInPast / _pastEstimates;
            _framesToGoBack = (int)Math.Floor(_rateToCalculateFuture);
            _framesToGoBackInPast = (int)Math.Floor(_rateToCalculatePast);

            _animClips = animClips;
            _poseStep = poseStep;
            _databaseName = databaseName;
            _dataset = ScriptableObject.CreateInstance<Dataset>()
                .Also(d => d.Initialize(_futureEstimates, futureEstimatesTime, _pastEstimates, pastEstimatesTime));
            _dataset.poseStep = _poseStep;
            _dataset.recordVelocity = recordVelocity;
            
            _dataset.originalBonesRotation = _originalRotations;

            Debug.Log("Processing dataset with name: " + _databaseName);

            _isNextAnimation = true;

            _animator.speed = _recordVelocity;
            _animator.runtimeAnimatorController = rac;

            _previousRootPosition = _root.position;
            _previousRootRotation = _root.rotation;

            _combinations = combinations;
            CreateTags(tags, actionTags, idleTags);

            _dataset.animationPaths = animPaths;
            _startRecord = true;
        }
        
        private void InitBonesAndRotations(HumanoidAvatar bodyAvatar, Transform bodyRoot)
        {
            bodyAvatar.GetOriginalAvatarRotations(out _originalRotations, out var defaultRotations, _characterTransforms, bodyRoot);
        }

        private void CreateTags(List<TagBase> baseTags, List<ActionTag> actionTags, List<IdleTag> idleTags)
        {
            Containers.Tags tags = ScriptableObject.CreateInstance<Containers.Tags>();
            tags.tags = baseTags;
            tags.actionTags = actionTags;
            tags.idleTags = idleTags;

            _dataset.tagsList = tags;
        }

        private void CreateQueriesComputed()
        {
            QueriesComputed queriesComputed = ScriptableObject.CreateInstance<QueriesComputed>();

            GenerateQueries(_combinations, queriesComputed);
            GenerateActionQueries(_dataset.tagsList.actionTags, queriesComputed);
            GenerateLoopQueries(_dataset.tagsList.idleTags, queriesComputed);

            _dataset.queriesComputed = queriesComputed;
        }

        private void CreateCharacteristics()
        {
            Characteristics characteristics = ScriptableObject.CreateInstance<Characteristics>();
            characteristics.characteristicsByTags = new List<CharacteristicsByTag>();
            foreach (var element in _dataset.tagsList.tags.Concat(_dataset.tagsList.actionTags)
                         .Concat((_dataset.tagsList.idleTags)))
            {
                characteristics.characteristicsByTags.Add(new CharacteristicsByTag()
                {
                    id = element.name,
                    characteristics = _bonesCharacteristics.Select(bc => new BoneCharacteristic
                        {
                            bone = bc.bone,
                            weightPosition = bc.weightPosition,
                            weightVelocity = bc.weightVelocity
                        })
                        .ToList(), //Avoid  references between them
                    weightFutureDirection = 1,
                    weightFutureOffset = 1,
                    weightPastDirection = 1,
                    weightPastOffset = 1
                });
            }

            _dataset.characteristics = characteristics;
        }

        private void CancelProcess()
        {
            if (_currentCoroutine == null)
            {
                return;
            }

            StopCoroutine(_currentCoroutine);
            _currentCoroutine = null;
        }

        private void SaveDataset()
        {
            var basePath = "Assets/QuanticBrains/MotionMatching/AnimationDatasets/" + _databaseName;
            try
            {
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }

            var path = basePath + "/" + _databaseName + ".asset";
            AssetDatabase.CreateAsset(_dataset, path);

            var characteristics = _dataset.characteristics;
            var queries = _dataset.queriesComputed;
            var tags = _dataset.tagsList;
            
            SaveCharacteristics(characteristics, basePath, _databaseName);
            SaveQueries(queries, basePath, _databaseName);
            SaveTags(tags, basePath, _databaseName);
            SaveCustomAvatar(basePath);
            
            EditorUtility.SetDirty(_dataset); // This ensures the modified data in the scriptable object is saved

            Debug.Log("Dataset named '" + _databaseName + "' successfully saved at: " + path);
            EditorApplication.ExitPlaymode();
        }

        private void SaveCharacteristics(Characteristics characteristics, string basePath, string datasetName)
        {
            basePath += "/" + datasetName + "_Characteristics.asset";
            AssetDatabase.CreateAsset(characteristics, basePath);
            
            var newCharacteristics = AssetDatabase.LoadAssetAtPath<Characteristics>(basePath); 
            _dataset.characteristics = newCharacteristics;
            
            EditorUtility.SetDirty(_dataset
                .characteristics); // This ensures the modified data in the scriptable object is saved
        }

        private void SaveQueries(QueriesComputed queries, string basePath, string datasetName)
        {
            basePath += "/" + datasetName + "_QueriesComputed.asset";
            AssetDatabase.CreateAsset(queries, basePath);
            
            var newQueries = AssetDatabase.LoadAssetAtPath<QueriesComputed>(basePath); 
            _dataset.queriesComputed = newQueries;
            
            EditorUtility.SetDirty(_dataset
                .queriesComputed); // This ensures the modified data in the scriptable object is saved
        }

        private void SaveTags(Containers.Tags tags, string basePath, string datasetName)
        {
            basePath += "/" + datasetName + "_Tags.asset";
            AssetDatabase.CreateAsset(tags, basePath);
            
            var newTagList = AssetDatabase.LoadAssetAtPath<Containers.Tags>(basePath);
            _dataset.tagsList = newTagList;
            
            EditorUtility.SetDirty(_dataset
                .tagsList); // This ensures the modified data in the scriptable object is saved
        }
        
        private void SaveCustomAvatar(string basePath)
        {
            basePath += "/" + "Custom" + _avatar.avatar.name + ".asset";
            if (_avatar is HumanoidAvatar humanoidAvatar)
            {
                //Humanoid Avatar
                AssetDatabase.CreateAsset(humanoidAvatar, basePath);
                var newAvatar = AssetDatabase.LoadAssetAtPath<HumanoidAvatar>(basePath);
                _dataset.avatar = newAvatar;
            }
            else if(_avatar is GenericAvatar genericAvatar)
            {
                //Generic Avatar
                AssetDatabase.CreateAsset(genericAvatar, basePath);
                var newAvatar = AssetDatabase.LoadAssetAtPath<GenericAvatar>(basePath);
                _dataset.avatar = newAvatar;
            }
            
            EditorUtility.SetDirty(_dataset
                .avatar); // This ensures the modified data in the scriptable object is saved
        }

        private void FixedUpdate()
        {
            if (!_startRecord)
            {
                return;
            }

            if (_isNextAnimation)
            {
                if (_currentAnimIndex < _animClips.Count)
                {
                    SetAnimation(_currentAnimIndex);
                    _animator.Update(0);
                    _isNextAnimation = false;
                    _animationFinished = false;
                    _lengthAnimationRecorded = 0f;
                    _rangesByAnim.Add(_currentAnimation.name, (_currentFeatureID, 0));
                    return;
                }

                _currentFrame = 0;
                _currentFeatureID = 0;
                _lengthAnimationRecorded = 0f;
                _isNextAnimation = false;
                _startRecord = false;
                _dataset.avatar = _avatar;


                ManagePoses();

                CreateCharacteristics();
                CreateQueriesComputed();
                NormalizeDataset();

                
                SaveDataset();
                return;
            }

            if (!_animationFinished)
            {
                SetDatasetPose(_currentAnimIndex, _currentFrame, _currentFeatureID, _poseStep);

                _currentFrame++;
                _currentFeatureID++;

                if (_lengthAnimationRecorded + _poseStep >= _currentAnimation.length)
                {
                    _animationFinished = true;
                }
                else
                {
                    _lengthAnimationRecorded += _poseStep;
                }

                _animator.Update(_recordFixedUpdate);
            }
            else
            {
                _rangesByAnim[_currentAnimation.name] =
                    (_rangesByAnim[_currentAnimation.name].Item1, _currentFeatureID - 1);
                _currentFrame--;
                if (_currentAnimation.isLooping)
                {
                    _dataset.SetAnimationIsLooping(_currentAnimIndex, _currentFrame, _bonesDataLength);

                    for (int position = 0; position < _futureEstimates; position++)
                    {
                        ManageLastLoopingFutures(_currentAnimIndex, _currentFrame, _currentFeatureID - 1, position);
                    }
                    
                    for (int position = 0; position < _pastEstimates; position++)
                    {
                        ManageLoopPasts(_currentAnimIndex, _currentFrame, _currentFeatureID - 1, position);
                    }

                    UpdateFirstFrameVelocityLoop(_rangesByAnim[_currentAnimation.name].Item1,
                        _rangesByAnim[_currentAnimation.name].Item2);
                    //Root inverse model
                    var inverseModel4X4 = math.inverse(
                        float4x4.TRS(_previousRootPosition,
                            _previousRootRotation, new float3(1)));

                    var offsetInLocalSpace = math.transform(
                        inverseModel4X4
                        , _root.position);

                    quaternion offsetRotInLocalSpace =
                        math.normalize(((quaternion)_root.rotation).Diff(_previousRootRotation));

                    _dataset.SetAnimationRootData(
                        _currentAnimIndex,
                        0,
                        _bonesDataLength,
                        offsetRotInLocalSpace, //
                        offsetInLocalSpace //
                    );
                }
                else
                {
                    ManageLastFutures(_currentFeatureID - 1);
                    for (int position = 0; position < _pastEstimates; position++)
                    {
                        ManageFirstPasts(_currentAnimIndex, _currentFrame, _currentFeatureID - 1, position);    
                    }

                    UpdateFirstFrameVelocity(_rangesByAnim[_currentAnimation.name].Item1);
                }

                _dataset.lastAnimationPoses.Add(_currentFrame);
                _currentFrame = 0;
                _lengthAnimationRecorded = 0;
                _currentAnimIndex++;
                _isNextAnimation = true;
                _root.position = _startingPosition;
                _root.rotation = _startingRotation;
                _previousRootPosition = _startingPosition;
                _previousRootRotation = _startingRotation;
            }
        }

        private void NormalizeDataset()
        {
            /*_stdChecker = new bool[_tempFeaturesData.Count];
            _meanChecker = new bool[_tempFeaturesData.Count];
            _normalizeChecker = new bool[_tempFeaturesData.Count];*/
            ComputeStdAndMean();
            RemapZNormDataset();
        }

        private void GenerateQueries(IEnumerable<List<TagBase>> combinations, QueriesComputed queriesComputed)
        {
            // This ensures the modified data in the scriptable object
            
            queriesComputed.queries = new List<MotionQueryComputed>();
            foreach (var combination in combinations)
            {
                //TODO add length from avatar
                MotionQueryComputed queryComputed = new MotionQueryComputed(combination, _futureEstimates, _pastEstimates, _avatar.Length);
                if (queryComputed.GetRanges().Count == 0)
                {
                    continue;
                }

                queryComputed.query = combination.Select(currentTag => currentTag.name).ToArray();
                var velocities = CalculateMaxVelocities(queryComputed);
                queryComputed.forwardSpeed = velocities.Item1;
                queryComputed.backwardSpeed = velocities.Item2;
                queryComputed.sideSpeed = velocities.Item3;
                queriesComputed.queries.Add(queryComputed);
            }

            queriesComputed.queries.ForEach(q =>
            {
                q.ranges = AddFeaturesAndRemapRanges(q.featuresData, q.GetRanges());
            });

            // Update to the queries, excluding overlapping parts from motion queries 
            //queriesComputed.queries = MotionQueryComputed.ManageExclusions(queriesComputed.queries);
        }

        private List<QueryRange> AddFeaturesAndRemapRanges(List<FeatureData> featureDatas, List<QueryRange> queryRanges)
        {
            var newRanges = new List<QueryRange>();
            foreach (var currentRange in queryRanges)
            {
                var origStart   = currentRange.featureIDStart;
                var origStop    = currentRange.featureIDStop;

                QueryRange qr = new QueryRange
                {
                    featureIDStart = 0,
                    featureIDStop = 0
                };
                
                if (origStart != 0 || origStop != 0)
                {
                    qr.featureIDStart = featureDatas.Count;
                    qr.featureIDStop = featureDatas.Count + (origStop - origStart);
                    
                    for (int i = origStart; i <= origStop; i++)
                    {
                        featureDatas.Add(new FeatureData
                        {
                            animationID = _tempFeaturesData[i].animationID,
                            animFrame = _tempFeaturesData[i].animFrame,
                            positionsAndVelocities = _tempFeaturesData[i].positionsAndVelocities.Clone() as float3[],
                            futureOffsets = _tempFeaturesData[i].futureOffsets.Clone() as float3[],
                            futureDirections = _tempFeaturesData[i].futureDirections.Clone() as float3[],
                            pastOffsets = _tempFeaturesData[i].pastOffsets.Clone() as float3[],
                            pastDirections = _tempFeaturesData[i].pastDirections.Clone() as float3[]
                        });
                    }
                }
                newRanges.Add(qr);
            }

            return newRanges;
        }
        
        private (float, float, float) CalculateMaxVelocities(QueryComputed queryComputed)
        {
            List<float> xVelocities = new List<float>();
            List<float> zVelocities = new List<float>();
            for (int i = 0; i < queryComputed.GetRanges().Count; i++)
            {
                for (int k = queryComputed.GetRanges()[i].featureIDStart;
                     k <= queryComputed.GetRanges()[i].featureIDStop;
                     k++)
                {
                    if (k == queryComputed.GetRanges()[i].featureIDStart)
                    {
                        continue;
                    }

                    FeatureData currentFeature = _tempFeaturesData[k];
                    FeatureData previousFeature = _tempFeaturesData[k - 1];

                    if (previousFeature.animationID != currentFeature.animationID)
                    {
                        continue;
                    }

                    var currentPos = _dataset.animationsData[currentFeature.animationID][currentFeature.animFrame]
                        .rootPosition;
                    var currentVel = currentPos / _poseStep;

                    xVelocities.Add(currentVel.x);
                    zVelocities.Add(currentVel.z);
                }
            }

            List<float> smoothXVelocities = new List<float>();
            List<float> smoothZVelocities = new List<float>();
            int window = 3;
            for (int i = 0; i < xVelocities.Count; i++)
            {
                float sum = 0;
                int count = 0;
                for (int j = i - window / 2; j <= i + window / 2; j++)
                {
                    if (j >= 0 && j < xVelocities.Count)
                    {
                        sum += xVelocities[j];
                        count++;
                    }
                }
                smoothXVelocities.Add(sum / count);
            }
            
            for (int i = 0; i < zVelocities.Count; i++)
            {
                float sum = 0;
                int count = 0;
                for (int j = i - window / 2; j <= i + window / 2; j++)
                {
                    if (j >= 0 && j < zVelocities.Count)
                    {
                        sum += zVelocities[j];
                        count++;
                    }
                }
                smoothZVelocities.Add(sum / count);
            }
            
            //Forward, Backward, Side
            return (smoothZVelocities.Max(), math.abs(smoothZVelocities.Min()), smoothXVelocities.Max());
        }

        private void GenerateActionQueries(List<ActionTag> actionTags, QueriesComputed queries)
        {
            queries.actionQueries = new List<ActionQueryComputed>();
            queries.loopActionQueries = new List<LoopActionQueryComputed>();
            
            foreach (var action in actionTags)
            {
                //Loop Query
                if (action is LoopActionTag)
                {
                    //TODO add length from avatar
                    var loopQueryComputed = new LoopActionQueryComputed(action, _futureEstimates, _pastEstimates, _avatar.Length)
                    {
                        query = new[] { action.name }
                    };
                    
                    queries.loopActionQueries.Add(loopQueryComputed);
                    continue;
                }

                //TODO add length from avatar
                //Regular action query
                var actionQueryComputed = new ActionQueryComputed(action, _futureEstimates, _pastEstimates, _avatar.Length)
                {
                    query = new[] { action.name }
                };

                queries.actionQueries.Add(actionQueryComputed);
            }
            queries.actionQueries.ForEach(q =>
            {
                q.ranges = AddFeaturesAndRemapRanges(q.featuresData, q.GetRanges());
            });
            queries.loopActionQueries.ForEach(q =>
            {
                q.ranges = AddFeaturesAndRemapRanges(q.featuresData, q.GetRanges());
            });
        }

        private void GenerateLoopQueries(List<IdleTag> loopTags, QueriesComputed queries)
        {
            queries.idleQueries = new List<IdleQueryComputed>();
            foreach (var idle in loopTags) //It only contains one
            {
                //TODO add length from avatar
                IdleQueryComputed queryComputed = new IdleQueryComputed(idle, _futureEstimates, _pastEstimates, _avatar.Length)
                {
                    query = new[] { idle.name }
                };

                queries.idleQueries.Add(queryComputed);
            }
            
            queries.idleQueries.ForEach(q =>
            {
                q.idleRanges = AddFeaturesAndRemapRanges(q.featuresData, q.idleRanges);
            });
            
            queries.idleQueries.ForEach(q =>
            {
                q.initRanges = AddFeaturesAndRemapRanges(q.featuresData, q.initRanges);
            });
        }

        private void ManagePoses()
        {
            foreach (var currentAnimation in _animClips)
            {
                var totalFrames = currentAnimation.length * currentAnimation.frameRate - 1;
                var totalFramesInFeatureSpace = _rangesByAnim[currentAnimation.name].Item2 -
                                                _rangesByAnim[currentAnimation.name].Item1;
                foreach (var currentTag in _dataset.tagsList.tags)
                {
                    currentTag.ranges
                        .Where(range => range.animName.Equals(currentAnimation.name))
                        .ForEach(range =>
                        {
                            var startFramesInFeatureSpace =
                                (int)Math.Ceiling((range.frameStart * totalFramesInFeatureSpace) / totalFrames);
                            var finalStartPose = _rangesByAnim[currentAnimation.name].Item1 + startFramesInFeatureSpace;

                            if ((range.frameStart * totalFramesInFeatureSpace) % totalFrames != 0)
                            {
                                finalStartPose = _tempFeaturesData[finalStartPose].animFrame == 1
                                    ? finalStartPose - 1
                                    : finalStartPose;
                            }

                            range.poseStart = finalStartPose;

                            var stopFramesInFeatureSpace =
                                (int)Math.Ceiling(((range.frameStop * totalFramesInFeatureSpace) / totalFrames));
                            var finalStopPose = _rangesByAnim[currentAnimation.name].Item1 + stopFramesInFeatureSpace;
                            if ((range.frameStart * totalFramesInFeatureSpace) % totalFrames != 0)
                            {
                                if (_tempFeaturesData.Count > finalStopPose + 2)
                                {
                                    finalStopPose = _tempFeaturesData[finalStopPose + 2].animFrame == 0
                                        ? finalStopPose + 1
                                        : finalStopPose;
                                }
                                else
                                {
                                    finalStopPose = _tempFeaturesData.Count - 1;
                                }
                            }

                            if (_dataset.animationsData[_tempFeaturesData[finalStopPose].animationID][
                                    _tempFeaturesData[finalStopPose].animFrame].isLoop)
                            {
                                finalStopPose--; //We have to avoid last frame for loops since they are exactly the same
                            }

                            range.poseStop = finalStopPose;
                        });
                }

                foreach (var currentTag in _dataset.tagsList.actionTags)
                {
                    currentTag.ranges
                        .Where(range => range.animName.Equals(currentAnimation.name))
                        .ForEach(range =>
                        {
                            var startFramesInFeatureSpace =
                                (int)((range.frameStart * totalFramesInFeatureSpace) / totalFrames);
                            range.poseStart = _rangesByAnim[currentAnimation.name].Item1 + startFramesInFeatureSpace;

                            var stopFramesInFeatureSpace =
                                (int)((range.frameStop * totalFramesInFeatureSpace) / totalFrames);
                            range.poseStop = _rangesByAnim[currentAnimation.name].Item1 + stopFramesInFeatureSpace;
                        });
                }

                foreach (var currentTag in _dataset.tagsList.idleTags)
                {
                    currentTag.initRanges
                        .Where(range => range.animName.Equals(currentAnimation.name))
                        .ForEach(range =>
                        {
                            var startFramesInFeatureSpace =
                                (int)((range.frameStart * totalFramesInFeatureSpace) / totalFrames);
                            range.poseStart = _rangesByAnim[currentAnimation.name].Item1 + startFramesInFeatureSpace;

                            var stopFramesInFeatureSpace =
                                (int)((range.frameStop * totalFramesInFeatureSpace) / totalFrames);
                            range.poseStop = _rangesByAnim[currentAnimation.name].Item1 + stopFramesInFeatureSpace;
                        });

                    currentTag.loopRanges
                        .Where(range => range.animName.Equals(currentAnimation.name))
                        .ForEach(range =>
                        {
                            var startFramesInFeatureSpace =
                                (int)((range.frameStart * totalFramesInFeatureSpace) / totalFrames);
                            range.poseStart = _rangesByAnim[currentAnimation.name].Item1 + startFramesInFeatureSpace;

                            var stopFramesInFeatureSpace =
                                (int)((range.frameStop * totalFramesInFeatureSpace) / totalFrames);
                            range.poseStop = _rangesByAnim[currentAnimation.name].Item1 + stopFramesInFeatureSpace;
                        });
                }
            }
        }

        private void ComputeStdAndMean()
        {
            //Bones length
            int lengthBones = _tempFeaturesData[0].positionsAndVelocities.Length / 2;
            ComputeQueriesStdAndMean(lengthBones, _dataset.queriesComputed.queries.Cast<QueryComputed>().ToList());
            ComputeQueriesStdAndMean(lengthBones,
                _dataset.queriesComputed.actionQueries.Cast<QueryComputed>().ToList());
            ComputeQueriesStdAndMean(lengthBones,
                _dataset.queriesComputed.loopActionQueries.Cast<QueryComputed>().ToList());
            ComputeQueriesStdAndMean(lengthBones, _dataset.queriesComputed.idleQueries.Cast<QueryComputed>().ToList());
        }

        private void ComputeQueriesStdAndMean(int lengthBones, List<QueryComputed> queries)
        {
            foreach (var query in queries)
            {
                int totalFeatures = query.GetRanges().Aggregate(0,
                    (acc, range) => acc + (range.featureIDStop - range.featureIDStart) + 1);
                
                //Mean
                foreach (var feature in query.featuresData)
                {
                    for (int i = 0; i < feature.positionsAndVelocities.Length; i++)
                    {
                        //Positions
                        int boneId;
                        if (i % 2 == 0)
                        {
                            boneId = i / 2;
                            query.meanFeaturePosition[boneId] +=
                                feature.positionsAndVelocities[i] / totalFeatures;
                            continue;
                        }

                        //Velocities
                        boneId = (i - 1) / 2;
                        query.meanFeatureVelocity[boneId] +=
                            feature.positionsAndVelocities[i] / totalFeatures;
                    }

                    //Future directions and positions
                    for (int i = 0; i < feature.futureOffsets.Length; i++)
                    {
                        query.meanFutureOffset[i] +=
                            feature.futureOffsets[i] / totalFeatures;
                        query.meanFutureDirection[i] +=
                            feature.futureDirections[i] / totalFeatures;
                    }
                        
                    //Past directions and positions
                    //Debug.Log("---------Mean Past -> " + featureID);
                    for (int i = 0; i < feature.pastOffsets.Length; i++)
                    {
                        query.meanPastOffset[i] +=
                            feature.pastOffsets[i] / totalFeatures;
                        query.meanPastDirection[i] +=
                            feature.pastDirections[i] / totalFeatures;
                    }
                }
                
                //Standard deviation
                foreach (var feature in query.featuresData)
                {
                    for (int i = 0; i < feature.positionsAndVelocities.Length; i++)
                    {
                        float3 squaredDist;
                        //Positions
                        int boneId;
                        if (i % 2 == 0)
                        {
                            boneId = i / 2;
                            squaredDist = feature.positionsAndVelocities[i] -
                                          query.meanFeaturePosition[boneId];
                            squaredDist *= squaredDist; //Componentwise multiplication

                            query.stdFeaturePosition[i / 2] += squaredDist / totalFeatures;
                            continue;
                        }

                        //Velocities
                        boneId = (i - 1) / 2;
                        squaredDist = feature.positionsAndVelocities[i] -
                                      query.meanFeatureVelocity[boneId];
                        squaredDist *= squaredDist; //Componentwise multiplication

                        query.stdFeatureVelocity[boneId] += squaredDist / totalFeatures;
                    }

                    //Future directions and positions
                    for (int i = 0; i < feature.futureOffsets.Length; i++)
                    {
                        float3 squaredDist = feature.futureOffsets[i] -
                                             query.meanFutureOffset[i];
                        squaredDist *= squaredDist;
                        query.stdFutureOffset[i] += squaredDist / totalFeatures;

                        squaredDist = feature.futureDirections[i] -
                                      query.meanFutureDirection[i];
                        squaredDist *= squaredDist;
                        query.stdFutureDirection[i] += squaredDist / totalFeatures;
                    }
                    
                    //Past directions and positions
                    for (int i = 0; i < feature.pastOffsets.Length; i++)
                    {
                        float3 squaredDist = feature.pastOffsets[i] -
                                             query.meanPastOffset[i];
                        squaredDist *= squaredDist;
                        query.stdPastOffset[i] += squaredDist / totalFeatures;

                        squaredDist = feature.pastDirections[i] -
                                      query.meanPastDirection[i];
                        squaredDist *= squaredDist;
                        query.stdPastDirection[i] += squaredDist / totalFeatures;
                    }
                }

                if (query.featuresData.Count == 0) continue;
                
                for (int i = 0; i < lengthBones; i++)
                {
                    query.stdFeaturePosition[i] = math.sqrt(query.stdFeaturePosition[i]);
                    query.stdFeatureVelocity[i] = math.sqrt(query.stdFeatureVelocity[i]);
                }
                
                //Futures
                for (int i = 0; i < query.featuresData[0].futureOffsets.Length; i++)
                {
                    query.stdFutureOffset[i] = math.sqrt(query.stdFutureOffset[i]);
                    query.stdFutureDirection[i] = math.sqrt(query.stdFutureDirection[i]);
                }
                
                //Pasts
                for (int i = 0; i < query.featuresData[0].pastOffsets.Length; i++)
                {
                    query.stdPastOffset[i] = math.sqrt(query.stdPastOffset[i]);
                    query.stdPastDirection[i] = math.sqrt(query.stdPastDirection[i]);
                }
            }
        }

        void RemapZNormDataset()
        {
            RemapByQueries(_dataset.queriesComputed.queries.Cast<QueryComputed>().ToList());
            RemapByQueries(_dataset.queriesComputed.actionQueries.Cast<QueryComputed>().ToList());
            RemapByQueries(_dataset.queriesComputed.loopActionQueries.Cast<QueryComputed>().ToList());
            RemapByQueries(_dataset.queriesComputed.idleQueries.Cast<QueryComputed>().ToList());
        }

        private void RemapByQueries(List<QueryComputed> queries)
        {
            foreach (var query in queries)
            {
                foreach (var feature in query.featuresData)
                {
                    for (int i = 0; i < feature.positionsAndVelocities.Length; i++)
                    {
                        int boneId;
                        //Positions
                        if (i % 2 == 0)
                        {
                            boneId = i / 2;
                            feature.positionsAndVelocities[i] =
                                (feature.positionsAndVelocities[i] -
                                 query.meanFeaturePosition[boneId]) /
                                query.stdFeaturePosition[boneId];
                            continue;
                        }

                        boneId = (i - 1) / 2;
                        //Velocities
                        feature.positionsAndVelocities[i] =
                            (feature.positionsAndVelocities[i] -
                             query.meanFeatureVelocity[boneId]) /
                            query.stdFeatureVelocity[boneId];
                    }

                    //Futures
                    for (int i = 0; i < feature.futureOffsets.Length; i++)
                    {
                        feature.futureOffsets[i] =
                            (feature.futureOffsets[i] - query.meanFutureOffset[i]) /
                            query.stdFutureOffset[i];
                        feature.futureOffsets[i] =
                            feature.futureOffsets[i].Sanitize();

                        feature.futureDirections[i] =
                            (feature.futureDirections[i] - query.meanFutureDirection[i]) /
                            query.stdFutureDirection[i];

                        feature.futureDirections[i] =
                            feature.futureDirections[i].Sanitize();
                    }
                    
                    //Pasts
                    for (int i = 0; i < feature.pastOffsets.Length; i++)
                    {
                        feature.pastOffsets[i] =
                            (feature.pastOffsets[i] - query.meanPastOffset[i]) /
                            query.stdPastOffset[i];
                        feature.pastOffsets[i] =
                            feature.pastOffsets[i].Sanitize();

                        feature.pastDirections[i] =
                            (feature.pastDirections[i] - query.meanPastDirection[i]) /
                            query.stdPastDirection[i];

                        feature.pastDirections[i] =
                            feature.pastDirections[i].Sanitize();
                    }
                }
            }
        }

        private void SetAnimation(int animationIndex)
        {
            var controller = (AnimatorController)_animator.runtimeAnimatorController;
            var state = controller.layers[0].stateMachine.defaultState;
            _currentAnimation = _animClips[animationIndex];
            controller.SetStateEffectiveMotion(state, _currentAnimation);
            _animator.Update(0);
            if (_currentAnimation.isLooping)
            {
                var framePercentageDeviation = _currentAnimation.length % _poseStep;
                var framesToRecord = (int)(_currentAnimation.length / _poseStep);

                var currentStep = _poseStep + framePercentageDeviation / framesToRecord;
                _recordFixedUpdate = currentStep / _recordVelocity;
                Time.fixedDeltaTime = _recordFixedUpdate;
            }
            else
            {
                _recordFixedUpdate = _poseStep / _recordVelocity;
                Time.fixedDeltaTime = _recordFixedUpdate;
            }
        }

        private float3 CalculateVelocity(float3 currentPosition, int boneIndex, float time)
        {
            if (math.isnan(_previousBonesPosition[boneIndex].x))
            {
                _previousBonesPosition[boneIndex] = currentPosition;
                return new float3(0, 0, 0);
            }

            var x = (currentPosition.x - _previousBonesPosition[boneIndex].x) / time;
            var y = (currentPosition.y - _previousBonesPosition[boneIndex].y) / time;
            var z = (currentPosition.z - _previousBonesPosition[boneIndex].z) / time;

            var vel = new float3(x, y, z);

            return vel;
        }
        
        private float3 CalculateLocalVelocity(float3 currentPosition, int boneIndex, float time)
        {
            if (math.isnan(_previousBonesLocalPosition[boneIndex].x))
            {
                _previousBonesLocalPosition[boneIndex] = currentPosition;
                return new float3(0, 0, 0);
            }

            var x = (currentPosition.x - _previousBonesLocalPosition[boneIndex].x) / time;
            var y = (currentPosition.y - _previousBonesLocalPosition[boneIndex].y) / time;
            var z = (currentPosition.z - _previousBonesLocalPosition[boneIndex].z) / time;

            var vel = new float3(x, y, z);

            return vel;
        }

        private float3 CalculateAngularVelocity(Transform charTransform, int boneIndex, float time)
        {
            var charRotation = charTransform.localRotation;

            if (math.isnan(_previousBonesRotation[boneIndex].value.x))
            {
                _previousBonesRotation[boneIndex] = charRotation;
                return new float3(0, 0, 0);
            }

            //Angle normalized
            return MathUtils.AngularVelocity(_previousBonesRotation[boneIndex],
                charRotation, time);
        }

        private void SetDatasetPose(int animationID, int animationFrame, int featureID, float time)
        {
            for(int id = 0; id < _avatar.Length; id++)
            {
                var currentTransform = _characterTransforms[id];
                if (currentTransform == null)
                {
                    continue;
                }


                var positionInRootScale = (float3)_root.InverseTransformPoint(currentTransform.position);

                // Store position
                var position = positionInRootScale;
                SetFeaturePosition(animationID, animationFrame, featureID, _featuresLength, id, position);

                //Set to zero initially
                float3 angularVelocity = float3.zero;
                float3 velocity = float3.zero;
                float3 localVelocity = float3.zero;

                if (animationFrame != 0)
                {
                    // Store angular velocity
                    angularVelocity = CalculateAngularVelocity(currentTransform, id, time);
                    velocity = CalculateVelocity(positionInRootScale, id, time);
                    localVelocity = CalculateLocalVelocity(currentTransform.localPosition, id, time);
                    
                    SetFeatureVelocity(animationID, animationFrame, featureID, _featuresLength, id,
                        velocity);
                }

                //Compute relative rotation
                var relativeRotation = 
                    Quaternion.Inverse(_root.rotation) * currentTransform.rotation; //Relative rotation
                
                _dataset.SetAnimationBoneData(
                    animationID,
                    animationFrame,
                    _bonesDataLength,
                    id,
                    positionInRootScale,
                    currentTransform.localPosition,
                    currentTransform.localScale,
                    velocity,
                    localVelocity,
                    angularVelocity,
                    relativeRotation
                );

                //Update previous position
                _previousBonesPosition[id] = positionInRootScale;
                _previousBonesLocalPosition[id] = currentTransform.localPosition;
                _previousBonesRotation[id] = currentTransform.localRotation;
            }

            //Root inverse model
            var inverseModel4X4 = math.inverse(
                float4x4.TRS(_previousRootPosition,
                    _previousRootRotation, new float3(1)));

            var offsetInLocalSpace = math.transform(
                inverseModel4X4
                , _root.position);

            quaternion offsetRotInLocalSpace = math.normalize(((quaternion)_root.rotation).Diff(_previousRootRotation));
            _dataset.SetAnimationRootData(
                animationID,
                animationFrame,
                _bonesDataLength,
                offsetRotInLocalSpace, //
                offsetInLocalSpace //
            );

            _previousRootPosition = _root.position;
            _previousRootRotation = _root.rotation;

            for (int position = 0; position < _pastEstimates; position++)
            {
                ManagePastOffset(animationID, animationFrame, featureID, position, _framesToGoBackInPast);
                ManagePastDirection(animationID, animationFrame, featureID, position, _framesToGoBackInPast);
            }
            
            for (int position = 0; position < _futureEstimates; position++)
            {
                ManageFutureOffset(animationID, animationFrame, featureID, position, _framesToGoBack);
                ManageFutureDirection(animationID, animationFrame, featureID, position, _framesToGoBack);
            }

            if (_positionsByFrame.Count > animationID)
            {
                _positionsByFrame[animationID].AddOrReplace(animationFrame, (_root.position, _root.rotation, _root.forward));
                return;
            }
            
            _positionsByFrame.AddOrReplace(animationID, new List<(float3, quaternion, float3)>());
            _positionsByFrame[animationID].AddOrReplace(animationFrame, (_root.position, _root.rotation, _root.forward));
        }

        private void ManagePastOffset(
            int animationID,
            int animationFrame,
            int featureID,
            int position,
            int framesToGoBackInPast)
        {
            var multiplier = position + 1;
            var currentFramesToGoBack = framesToGoBackInPast * multiplier;
            
            if (animationFrame - currentFramesToGoBack < 0)
            {
                return;
            }
            
            var frameToTakeFrom = animationFrame - currentFramesToGoBack;
            var approximationError = _rateToCalculatePast % framesToGoBackInPast;
            var transformFrameToGo = _positionsByFrame[animationID][frameToTakeFrom];
            
            float4x4 inverseModel4X4Future = math.inverse(
                float4x4.TRS(_root.position,
                    _root.rotation, new float3(1)));

            float3 offsetInLocalSpaceFuture = math.transform(
                inverseModel4X4Future,
                transformFrameToGo.Item1);

            offsetInLocalSpaceFuture += (offsetInLocalSpaceFuture / currentFramesToGoBack) * approximationError;
            SetFeaturePastOffsets(
                animationID, 
                animationFrame,
                featureID,
                position,
                _featuresLength,
                offsetInLocalSpaceFuture);
        }
        
        private void ManagePastDirection(
            int animationID,
            int animationFrame,
            int featureID,
            int position,
            int framesToGoBackInPast)
        {
            var multiplier = position + 1;
            var currentFramesToGoBack = framesToGoBackInPast * multiplier;
            
            if (animationFrame - currentFramesToGoBack < 0)
            {
                return;
            }
            
            var frameToTakeFrom = animationFrame - currentFramesToGoBack;
            var transformFrameToGo = _positionsByFrame[animationID][frameToTakeFrom];
            
            float4x4 inverseModel4X4Future = math.inverse(
                float4x4.TRS(_root.position,
                    _root.rotation, new float3(1)));

            float3 offsetDirectionInLocalSpace = math.normalize(
                math.mul(inverseModel4X4Future, new float4(transformFrameToGo.Item3, 0.0f)).xyz);

            SetFeaturePastDirections(
                animationID, 
                animationFrame,
                featureID,
                position,
                _featuresLength,
                offsetDirectionInLocalSpace);
        }
        
        private void ManageFutureOffset(
            int animationID,
            int animationFrame,
            int featureID,
            int position,
            int framesToGoBack)
        {
            var multiplier = position + 1;
            var currentFramesToGoBack = framesToGoBack * multiplier;
            if (animationFrame - currentFramesToGoBack < 0)
            {
                return;
            }

            var finalFrameToGo = animationFrame - currentFramesToGoBack;
            var approximationError = _rateToCalculateFuture % framesToGoBack;
            var transformFrameToGo = _positionsByFrame[animationID][finalFrameToGo];

            float4x4 inverseModel4X4Future = math.inverse(
                float4x4.TRS(transformFrameToGo.Item1,
                    transformFrameToGo.Item2, new float3(1)));

            float3 offsetInLocalSpaceFuture = math.transform(
                inverseModel4X4Future,
                _root.position);

            offsetInLocalSpaceFuture += (offsetInLocalSpaceFuture / currentFramesToGoBack) * approximationError;
            SetFeatureFutureOffsets(
                animationID,
                finalFrameToGo,
                featureID - currentFramesToGoBack,
                position,
                _featuresLength,
                offsetInLocalSpaceFuture);
        }

        private void ManageFutureDirection(
            int animationID,
            int animationFrame,
            int featureID,
            int position,
            int framesToGoBack)
        {
            var multiplier = position + 1;
            var currentFramesToGoBack = framesToGoBack * multiplier;
            if (animationFrame - currentFramesToGoBack < 0)
            {
                return;
            }

            var finalFrameToGo = animationFrame - currentFramesToGoBack;
            var transformFrameToGo = _positionsByFrame[animationID][finalFrameToGo];

            float4x4 inverseModel4X4Future = math.inverse(
                float4x4.TRS(transformFrameToGo.Item1,
                    transformFrameToGo.Item2, new float3(1)));

            float3 offsetDirectionInLocalSpace = math.normalize(
                math.mul(inverseModel4X4Future, new float4(_root.forward, 0.0f)).xyz);

            SetFeatureFutureDirections(
                animationID,
                finalFrameToGo,
                featureID - currentFramesToGoBack,
                position,
                _featuresLength,
                offsetDirectionInLocalSpace);
        }

        private void ManageLastFutures(int featureID)
        {
            for (int positions = 0; positions < _futureEstimates; positions++)
            {
                ManageFutureByFeature(positions, featureID);
            }
        }

        private void ManageFutureByFeature(int positionFutureIndex, int featureIndex)
        {
            var feature = _tempFeaturesData[featureIndex];
            int startingID = featureIndex - feature.animFrame;

            for (int i = featureIndex - feature.animFrame; i < _tempFeaturesData.Count; i++)
            {
                if (math.isnan(_tempFeaturesData[i].futureOffsets[positionFutureIndex].x))
                {
                    break;
                }

                startingID = i;
            }

            FeatureData previousFeature = _tempFeaturesData[featureIndex - 1];
            (float3, quaternion, float3) previousTransformValues = _positionsByFrame[previousFeature.animationID][previousFeature.animFrame];
            (float3, quaternion, float3) currentTransformValues = _positionsByFrame[feature.animationID][feature.animFrame];

            float distance = math.distance(currentTransformValues.Item1, previousTransformValues.Item1);

            for (int i = startingID; i <= featureIndex; i++)
            {
                var featureToFill = _tempFeaturesData[i];
                if (!math.isnan(featureToFill.futureOffsets[positionFutureIndex].x))
                {
                    continue;
                }

                (float3, float3) futureInLocalSpace = GetLastFutureValues(
                    i,
                    positionFutureIndex,
                    featureIndex,
                    distance,
                    featureToFill,
                    currentTransformValues,
                    previousTransformValues
                );

                SetFeatureFutureOffsets(
                    featureToFill.animationID,
                    featureToFill.animFrame,
                    i,
                    positionFutureIndex,
                    _featuresLength,
                    futureInLocalSpace.Item1);

                SetFeatureFutureDirections(
                    featureToFill.animationID,
                    featureToFill.animFrame,
                    i,
                    positionFutureIndex,
                    _featuresLength,
                    futureInLocalSpace.Item2);
            }
        }

        private (float3, float3) GetLastFutureValues(
            int indexFeatureToBeFilled, 
            int positionFutureIndex, 
            int featureIndex, 
            float distance, 
            FeatureData featureToFill, 
            (float3, quaternion, float3) currentTransformValues,
            (float3, quaternion, float3) previousTransformValues)
            {
            var multiplier = (positionFutureIndex + 1) * _framesToGoBack - (featureIndex - indexFeatureToBeFilled); //Calculate how many frames I have to predict
            float3 forward = math.mul(currentTransformValues.Item2, new float3(0, 0, 1));

            float3 posDirection = math.normalize(currentTransformValues.Item1 - previousTransformValues.Item1);
            float3 predictedGlobalPosition = currentTransformValues.Item1 + posDirection * distance * multiplier;

            (float3, quaternion, float3) featureToFillTransformValues = _positionsByFrame[featureToFill.animationID][featureToFill.animFrame];
            float4x4 inverseModel4X4Future = math.inverse(
                float4x4.TRS(featureToFillTransformValues.Item1,
                    featureToFillTransformValues.Item2, new float3(1)));

            var offsetInLocalSpaceFuture = math.transform(
                inverseModel4X4Future,
                predictedGlobalPosition);

            var approximationError = _rateToCalculateFuture % _framesToGoBack;
            offsetInLocalSpaceFuture += (offsetInLocalSpaceFuture / ((positionFutureIndex + 1) * _framesToGoBack)) *
                                        approximationError;

            var offsetDirectionInLocalSpace = math.normalize(
                math.mul(inverseModel4X4Future, new float4(forward, 0.0f)).xyz);
            return (offsetInLocalSpaceFuture, offsetDirectionInLocalSpace);
        }

        private void UpdateFirstFrameVelocityLoop(int firstFeature, int lastFeature)
        {
            for(int id = 0; id < _avatar.Length; id++)
            {
                var lastVel = _tempFeaturesData[lastFeature].positionsAndVelocities[id * 2 + 1];
                var vel = new float3(lastVel.x, lastVel.y, lastVel.z);
                _tempFeaturesData[firstFeature].positionsAndVelocities[id * 2 + 1] = vel;
            }
        }
        
        private void UpdateFirstFrameVelocity(int firstFeature)
        {
            for(int id = 0; id < _avatar.Length; id++)
            {
                var secondVelocity = _tempFeaturesData[firstFeature + 1].positionsAndVelocities[id * 2 + 1];
                var vel = new float3(secondVelocity.x, secondVelocity.y, secondVelocity.z);
                _tempFeaturesData[firstFeature].positionsAndVelocities[id * 2 + 1] = vel;
            }
        }

        private void ManageLastLoopingFutures(
            int animationID,
            int animFrame,
            int featureID,
            int position)
        {
            var multiplier = position + 1;
            var currentFramesToGoBack = _framesToGoBack * multiplier;
            var animFrameToGo = animFrame - currentFramesToGoBack;
            if (animFrame - currentFramesToGoBack < 0)
            {
                animFrameToGo = 0;
            }

            var approximationError = _rateToCalculateFuture % _framesToGoBack;

            var lastAnimFrameID = animFrame;
            var lastTransform = _positionsByFrame[animationID][animFrame];
            for (int frame = animFrameToGo; frame <= animFrame; frame++)
            {
                if (currentFramesToGoBack > animFrame && frame == 0) // If you reached end of animation
                {
                    var counter = 0;
                    var totalFrames = currentFramesToGoBack - animFrame;
                    while (counter != totalFrames)
                    {
                        UpdateLastGlobalValues(animationID, ref lastAnimFrameID, ref lastTransform, animFrame);
                        counter++;
                    }
                }
                else
                {
                    UpdateLastGlobalValues(animationID, ref lastAnimFrameID, ref lastTransform, animFrame);
                }

                var transformToFill = _positionsByFrame[animationID][frame];
                float4x4 inverseModel4X4Future = math.inverse(
                    float4x4.TRS(transformToFill.Item1,
                        transformToFill.Item2, new float3(1)));
                float3 offsetInLocalSpaceFuture = math.transform(
                    inverseModel4X4Future,
                    lastTransform.Item1);
                var targetForward = math.mul(lastTransform.Item2, new float3(0, 0, 1));
                float3 offsetDirectionInLocalSpace = math.normalize(
                    math.mul(inverseModel4X4Future, new float4(targetForward, 0.0f)).xyz);

                offsetInLocalSpaceFuture += (offsetInLocalSpaceFuture / currentFramesToGoBack) * approximationError;
                int finalFeatureID;
                if (animFrame - currentFramesToGoBack < 0)
                {
                    finalFeatureID = featureID - animFrame + frame;
                }
                else
                {
                    finalFeatureID = featureID - currentFramesToGoBack + (frame - animFrameToGo);
                }

                SetFeatureFutureOffsets(
                    animationID,
                    frame,
                    finalFeatureID,
                    position,
                    _featuresLength,
                    offsetInLocalSpaceFuture);

                SetFeatureFutureDirections(
                    animationID,
                    frame,
                    finalFeatureID,
                    position,
                    _featuresLength,
                    offsetDirectionInLocalSpace);
            }
        }
        
        private void ManageLoopPasts(
            int animationID,
            int lastFrame,
            int featureID,
            int position)
        {
            var multiplier = position + 1;
            var currentFramesToGoBack = _framesToGoBackInPast * multiplier;

            var approximationError = _rateToCalculatePast % _framesToGoBackInPast;
            for (int frame = 0; frame <= lastFrame; frame++)
            {
                var featureToUpdate = featureID - lastFrame + frame;
                if (!math.isnan(_tempFeaturesData[featureToUpdate].pastOffsets[position].x))
                {
                    continue;
                }
                
                var globalFramePosition = _positionsByFrame[animationID][frame].Item1;
                var globalFrameRotation = _positionsByFrame[animationID][frame].Item2;
                
                for (int i = 1; i <= currentFramesToGoBack; i++)
                {
                    var currentFrame = 0;
                    var previousFrame = 0;
                    
                    currentFrame = (frame - i) switch
                    {
                        < 0 => lastFrame - ((i - frame + 1) % lastFrame),
                        0 => lastFrame,
                        _ => frame - i
                    };

                    previousFrame = currentFrame - 1;

                    var currentTransform = _positionsByFrame[animationID][currentFrame];
                    var previousTransform = _positionsByFrame[animationID][previousFrame];
                    
                    float4x4 inverseModelPrevious = math.inverse(
                        float4x4.TRS(currentTransform.Item1,
                            currentTransform.Item2, new float3(1)));

                    var localOffset = math.transform(inverseModelPrevious, previousTransform.Item1);
                    globalFramePosition = MathUtils.TranslateToGlobal(globalFramePosition, globalFrameRotation, localOffset);
                    
                    var diffGlobalRotation   = currentTransform.Item2.Diff(previousTransform.Item2);
                    globalFrameRotation = globalFrameRotation.Diff(diffGlobalRotation);
                }

                
                float4x4 inverseModel = math.inverse(
                    float4x4.TRS(_positionsByFrame[animationID][frame].Item1,
                        _positionsByFrame[animationID][frame].Item2, new float3(1)));
                
                float3 offsetInLocalSpace = math.transform(
                    inverseModel,
                    globalFramePosition);

                offsetInLocalSpace += (offsetInLocalSpace / currentFramesToGoBack) * approximationError;
                SetFeaturePastOffsets(
                    animationID, 
                    frame,
                    featureToUpdate,
                    position,
                    _featuresLength,
                    offsetInLocalSpace);

                SetFeaturePastDirections(
                    animationID, 
                    frame,
                    featureToUpdate,
                    position,
                    _featuresLength,
                    math.normalize(
                        math.mul(inverseModel, 
                            new float4(math.normalize(math.mul(globalFrameRotation, Float3Ex.Forward)), 0.0f)).xyz)
                    );
            }
        }

        private void ManageFirstPasts(
            int animationID,
            int lastFrame,
            int featureID,
            int position)
        {
            var multiplier = position + 1;
            var currentFramesToGoBack = _framesToGoBackInPast * multiplier;
            var approximationError = _rateToCalculatePast % _framesToGoBackInPast;
            
            for (int frame = 0; frame <= lastFrame; frame++)
            {
                var featureToUpdate = featureID - lastFrame + frame;
                if (!math.isnan(_tempFeaturesData[featureToUpdate].pastOffsets[position].x))
                {
                    continue;
                }

                var currentFrame    = _positionsByFrame[animationID][frame];
                var zeroFrame       = _positionsByFrame[animationID][0];
                var firstFrame      = _positionsByFrame[animationID][1];
                
                var missingFrames = currentFramesToGoBack - frame;
                //Offset from current frame until zero frame
                float4x4 inverseCurrentFrame = math.inverse(
                    float4x4.TRS(currentFrame.Item1,
                        currentFrame.Item2, new float3(1)));

                float3 offsetInLocalSpace = math.transform(inverseCurrentFrame, zeroFrame.Item1);
                
                //Offset from first frame until zero frame
                float4x4 inverseModelFirst = math.inverse(
                    float4x4.TRS(firstFrame.Item1,
                        firstFrame.Item2, new float3(1)));

                float3 offsetMissingFrame = math.transform(inverseModelFirst, zeroFrame.Item1);
                
                offsetInLocalSpace += offsetMissingFrame * missingFrames;
                offsetInLocalSpace += (offsetInLocalSpace / currentFramesToGoBack) * approximationError;
                SetFeaturePastOffsets(
                    animationID, 
                    frame,
                    featureToUpdate,
                    position,
                    _featuresLength,
                    offsetInLocalSpace);

                float3 offsetDirectionInLocalSpace = math.normalize(
                    math.mul(inverseModelFirst, new float4(zeroFrame.Item3, 0.0f)).xyz);

                SetFeaturePastDirections(
                    animationID, 
                    frame,
                    featureToUpdate,
                    position,
                    _featuresLength,
                    offsetDirectionInLocalSpace);
            }
        }

        private void UpdateLastGlobalValues(
            int animationID,
            ref int lastAnimFrameID,
            ref (float3, quaternion, float3) lastGlobalValues,
            int maxFrameID)
        {
            var nextFrameID = lastAnimFrameID + 1;
            if (lastAnimFrameID == maxFrameID)
            {
                nextFrameID = 0;
            }

            var targetFutureFrameGlobalPosition =
                MathUtils.TranslateToGlobal(
                    lastGlobalValues.Item1,
                    lastGlobalValues.Item2,
                    _dataset.animationsData[animationID][nextFrameID].rootPosition
                );
            quaternion targetFutureFrameGlobalRotation = math.mul(
                lastGlobalValues.Item2,
                _dataset.animationsData[animationID][nextFrameID].rootRotation);
            lastGlobalValues = (targetFutureFrameGlobalPosition, targetFutureFrameGlobalRotation, lastGlobalValues.Item3);
            lastAnimFrameID = nextFrameID;
        }
        
         public void SetFeaturePosition(
            int animationID,
            int animationFrame,
            int featureID,
            int featuresLength,
            int boneID,
            float3 position)
        {
            var index = boneID * 2;
            SetFeature(
                animationID,
                animationFrame,
                featureID,
                featuresLength,
                (feature) =>
                {
                    feature.positionsAndVelocities[index] = position;
                    return feature;
                });
        }
        
        public void SetFeatureVelocity(
            int animationID,
            int animationFrame,
            int featureID,
            int featuresLength,
            int boneID,
            float3 velocity)
        {
            var index = boneID * 2 + 1;
            SetFeature(
                animationID,
                animationFrame,
                featureID,
                featuresLength,
                (feature) =>
                {
                    feature.positionsAndVelocities[index] = velocity;
                    return feature;
                });
        }

        public void SetFeatureFutureOffsets(
            int animationID,
            int animationFrame,
            int featureID,
            int index,
            int featuresLength,
            float3 position)
        {
            SetFeature(
                animationID,
                animationFrame,
                featureID,
                featuresLength,
                (feature) =>
                {
                    feature.futureOffsets[index] = new float3(position.x, 0, position.z);
                    return feature;
                });
        }
        
        public void SetFeatureFutureDirections(
            int animationID,
            int animationFrame,
            int featureID,
            int index,
            int featuresLength,
            float3 direction)
        {
            SetFeature(
                animationID,
                animationFrame,
                featureID,
                featuresLength,
                (feature) =>
                {
                    feature.futureDirections[index] = new float3(direction.x, 0, direction.z);
                    return feature;
                });
        }
        
        public void SetFeaturePastOffsets(
            int animationID,
            int animationFrame,
            int featureID,
            int index,
            int featuresLength,
            float3 position)
        {
            SetFeature(
                animationID,
                animationFrame,
                featureID,
                featuresLength,
                (feature) =>
                {
                    feature.pastOffsets[index] = new float3(position.x, 0, position.z);
                    return feature;
                });
        }
        
        public void SetFeaturePastDirections(
            int animationID,
            int animationFrame,
            int featureID,
            int index,
            int featuresLength,
            float3 direction)
        {
            SetFeature(
                animationID,
                animationFrame,
                featureID,
                featuresLength,
                (feature) =>
                {
                    feature.pastDirections[index] = new float3(direction.x, 0, direction.z);
                    return feature;
                });
        }

        private void SetFeature(
            int animationID,
            int animationFrame,
            int featureID,
            int featuresLength,
            Func<FeatureData, FeatureData> featureSetter)
        {
            (FeatureData, int) featureData = GetOrCreateFeatureData(featureID, featuresLength);
            featureData.Item1 = featureSetter(featureData.Item1);
            featureData.Item1.animationID = animationID;
            featureData.Item1.animFrame = animationFrame;
            if (featureData.Item2 == -1)
            {
                _tempFeaturesData.Add(featureData.Item1);
                return;
            }

            _tempFeaturesData[featureData.Item2] = featureData.Item1;
        }

        private (FeatureData, int) GetOrCreateFeatureData(int featureID, int featuresLength)
        {
            if (featureID < _tempFeaturesData.Count) return (_tempFeaturesData[featureID], featureID);

            var defaultInitialization = new float3(math.NAN, math.NAN, math.NAN);
            var featureData = new FeatureData
            {
                positionsAndVelocities  = new float3[featuresLength],
                futureOffsets           = Enumerable.Repeat(defaultInitialization, _futureEstimates).ToArray(),
                futureDirections        = Enumerable.Repeat(defaultInitialization, _futureEstimates).ToArray(),
                pastOffsets             = Enumerable.Repeat(defaultInitialization, _pastEstimates).ToArray(),
                pastDirections          = Enumerable.Repeat(defaultInitialization, _pastEstimates).ToArray(),
            };
            return (featureData, -1);
        }
    }
}
#endif
