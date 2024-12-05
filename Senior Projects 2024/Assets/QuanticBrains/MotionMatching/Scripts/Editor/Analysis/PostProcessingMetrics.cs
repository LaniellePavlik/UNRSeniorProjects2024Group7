using System;
using System.Collections.Generic;
using System.Linq;
using QuanticBrains.MotionMatching.Scripts.Containers;
using UnityEditor;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Editor.Analysis
{
    public class PostProcessingMetrics : EditorWindow
    {
        public Dataset datasetToAnalyse;
        public bool redundancy;

        private List<List<float>> _linearVelocities, _angularVelocities;
        private List<float> _redundancyScores;
        private PostProcessingGraph _graph;
        private int _xMax, _yMax, _xMin, _yMin, _angularStep;
        private float _mouseX, _mouseY;
        private Vector2 _infoPanelScrollPos;
        private AnimationData _pose;
        private (int, int) _animAndPose;
        private bool _isRedundancyPrepared;
        private string _datasetName;

        private void OnEnable()
        {
            _linearVelocities = new List<List<float>>();
            _angularVelocities = new List<List<float>>();
            _redundancyScores = new List<float>();
            _graph = CreateInstance(typeof(PostProcessingGraph)) as PostProcessingGraph;

            datasetToAnalyse = null;
            redundancy = false;
            _isRedundancyPrepared = false;
        }

        [MenuItem("Tools/Motion Matching/Dataset Analysis/Post Processing Metrics")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            GetWindow(typeof(PostProcessingMetrics), false, "Post Processing Metrics");
        }

        /// <summary>
        /// Loads the dataset as linear and angular speed arrays
        /// </summary>
        private void LoadDataset()
        {
            _linearVelocities = new List<List<float>>();
            _angularVelocities = new List<List<float>>();

            var prevPos = Vector3.zero;

            foreach (var animation in datasetToAnalyse.animationsData)
            {
                var lv = new List<float>();
                var av = new List<float>();
                foreach (var pose in animation)
                {
                    var linearVelocity = (Vector3)pose.rootPosition / datasetToAnalyse.poseStep;
                    var rootBone = datasetToAnalyse.avatar.GetRootBone();
                    var angularVelocity = pose.bonesData[rootBone].angularVelocity.y * Mathf.Rad2Deg;

                    switch (angularVelocity)
                    {
                        case > 180f:
                            angularVelocity = 180;
                            break;
                        case < -180f:
                            angularVelocity = -180;
                            break;
                    }
                    
                    lv.Add(Vector3.Magnitude(linearVelocity));
                    av.Add(angularVelocity);
                }

                _linearVelocities.Add(lv);
                _angularVelocities.Add(av);
            }
        }

        /// <summary>
        /// Returns poses according to linear and angular speed limits
        /// </summary>
        /// <param name="linearSpeedLimits">Tuple representing linear speed limits</param>
        /// <param name="angularSpeedLimits">Tuple representing angular speed limits</param>
        /// <returns></returns>
        private List<AnimationData> FindRelevantPoses((float, float) linearSpeedLimits, (float, float) angularSpeedLimits)
        {
            var poses = new List<AnimationData>();

            for (var i = 0; i < datasetToAnalyse.animationsData.Count; i++)
            {
                for (var j = 0; j < datasetToAnalyse.animationsData[i].Count; j++)
                {
                    if (!(_linearVelocities[i][j] >= linearSpeedLimits.Item1 &&
                          _linearVelocities[i][j] < linearSpeedLimits.Item2))
                    {
                        continue;
                    }
                
                    if (!(_angularVelocities[i][j] >= angularSpeedLimits.Item1 &&
                          _angularVelocities[i][j] < angularSpeedLimits.Item2))
                    {
                        continue;
                    }
                    
                    poses.Add(datasetToAnalyse.animationsData[i][j]);
                }
            }

            return poses;
        }

        /// <summary>
        /// Calculates the redundancy of a dataset based on positional similarity
        /// </summary>
        /// <param name="poses">Poses to be considered for analysis</param>
        /// <returns></returns>
        private float CalculateLocalRedundancy(List<AnimationData> poses)
        {
            var redundancyScores = new List<float>();
            
            // This loops through all poses in the dataset
            foreach (var targetPose in poses)
            {
                var jointsAverage = new List<float>();

                foreach (var comparisonPose in poses)
                {
                    var positionSimilarities = new List<float>();
                    var rotationSimilarities = new List<float>();
                    
                    // Each joint in every pose
                    for (var i = 0; i < targetPose.bonesData.Length; i++)
                    {
                        var targetJoint = targetPose.bonesData[i];
                        var comparisonJoint = comparisonPose.bonesData[i];
                        
                        // Store the difference in positions and rotations
                        // The dot function is used to check how much one vector follows the other
                        positionSimilarities.Add(Vector3.Dot(
                            Vector3.Normalize(targetJoint.position), 
                            Vector3.Normalize(comparisonJoint.position)) + 1
                        );
                        rotationSimilarities.Add(Vector3.Dot(
                            Vector3.Normalize(((Quaternion)targetJoint.rotation).eulerAngles), 
                            Vector3.Normalize(((Quaternion)comparisonJoint.rotation).eulerAngles)) + 1
                        );
                    }

                    // The average of both metrics is divided by 4, as both vary in [0;2]
                    jointsAverage.Add((positionSimilarities.Average() + rotationSimilarities.Average()) / 4);
                }
                
                redundancyScores.Add(jointsAverage.Average());
            }

            return redundancyScores.Average();
        }

        /// <summary>
        /// This functions draws the info panel for the selected pose on the right side of the window
        /// </summary>
        /// <param name="pose">Information to be displayed for the selected pose</param>
        /// <param name="poseIndex">Index of the selected pose in the dataset</param>
        private void DrawInfoPanel(AnimationData pose, (int, int) animAndPose)
        {
            EditorGUILayout.IntField("Animation: ", animAndPose.Item1, EditorStyles.boldLabel);
            EditorGUILayout.IntField("Pose: ", animAndPose.Item2, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Root", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.Vector3Field("Position",pose.rootPosition);
            EditorGUILayout.Vector3Field("Rotation", ((Quaternion)pose.rootRotation).eulerAngles);

            var boneList = datasetToAnalyse.avatar.GetAvatarDefinition();
            
            foreach (var (boneName, index) in boneList.Select((boneName, index) => (boneName.alias, index)))
            {
                if (boneName.Equals("")) continue;
                
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
                EditorGUILayout.LabelField(boneName, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.Vector3Field("Position", pose.bonesData[index].position);
                EditorGUILayout.Vector3Field("Rotation",((Quaternion)pose.bonesData[index].rotation).eulerAngles);
                EditorGUILayout.Vector3Field("Velocity",pose.bonesData[index].velocity);
                EditorGUILayout.Vector3Field("Angular Velocity",pose.bonesData[index].angularVelocity);
            }
        }

        /// <summary>
        /// This function find the closest point in the graph to highlight and return information
        /// </summary>
        /// <param name="speeds">An object containing the desired angular and linear speeds to match</param>
        /// <returns></returns>
        private (AnimationData, (int, int)) FindClosestPose(Vector2 speeds)
        {
            var distance = float.MaxValue;
            var finalPose = (0, 0);

            for (var i = 0; i < datasetToAnalyse.animationsData.Count; i++)
            {
                for (var j = 0; j < datasetToAnalyse.animationsData[i].Count; j++)
                {
                    var tempSpeedPos = _graph.RectPosition(_angularVelocities[i][j], _linearVelocities[i][j]);
                    var speedPos = _graph.RectPosition(speeds.x, speeds.y);

                    var tempDistance = Vector2.Distance(speedPos, tempSpeedPos);

                    if (tempDistance < distance)
                    {
                        finalPose = (i, j);
                        distance = tempDistance;
                    }
                }
            }

            return (datasetToAnalyse.animationsData[finalPose.Item1][finalPose.Item2], finalPose);
        }
        
        

        /// <summary>
        /// Preprocesses data for the the redundancy measurement before graphing it
        /// </summary>
        private void PrepareRedundancy()
        {
            // Division by 12 is just a sensible division to define the step at which to account for
            // similarity in an area
            _angularStep = (int)Math.Ceiling(_angularVelocities.SelectMany(i => i).Max()) / 12;
                
            _yMax = (int)Math.Ceiling(_linearVelocities.SelectMany(i => i).Max());
            _yMin = (int)Math.Floor(_linearVelocities.SelectMany(i => i).Min());
            _xMax = (int)Math.Ceiling(_angularVelocities.SelectMany(i => i).Max() / _angularStep);
            _xMin = (int)Math.Floor(_angularVelocities.SelectMany(i => i).Min() / _angularStep);

            // Find relevant poses to look for in an area, without the need to compare to poses outside
            // of the limits and calculate redundancy scores
            for (var i = _yMin; i < _yMax; i++)
            {
                for (var j = _xMin; j < _xMax; j++)
                {
                    var poses = FindRelevantPoses(
                        (i, i + 1), 
                        (j * _angularStep, (j + 1) * _angularStep)
                    );

                    if (poses.Count == 0)
                    {
                        _redundancyScores.Add(-1);
                        continue;
                    }

                    var score = CalculateLocalRedundancy(poses);
                    _redundancyScores.Add(score);
                }
            }
        }
        
        public void OnGUI()
        {
            if (Event.current.type is EventType.MouseDown)
            {
                _mouseX = Event.current.mousePosition.x;
                _mouseY = Event.current.mousePosition.y;
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Analysis parameters");

            EditorGUI.indentLevel = 1;

            EditorGUI.BeginChangeCheck();
            
            datasetToAnalyse =
                EditorGUILayout.ObjectField("Dataset", datasetToAnalyse, typeof(Dataset), false) as Dataset;
            redundancy = EditorGUILayout.Toggle("Display redundancy", redundancy);

            EditorGUI.EndChangeCheck();

            if (datasetToAnalyse == null)
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                return;
            }

            if (datasetToAnalyse.name != _datasetName)
            {
                LoadDataset();
                _pose = datasetToAnalyse.animationsData[0][0];
                _animAndPose = (0, 0);
                _datasetName = datasetToAnalyse.name;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            
            EditorGUILayout.LabelField("Coverage: (Angular Velocity, Linear Velocity)", style);
            
            _graph.rect = GUILayoutUtility.GetRect(500, 1000, 500, 1000);
            
            GUI.BeginClip(_graph.rect);
            if (Event.current.type is EventType.Repaint)
            {
                // Draw background, grid and initiate the GL calls
                _graph.InitializePlot();

                // Draw the labels for the axes
                _graph.DrawLabels(8);

                if (!(_linearVelocities.Count > 0))
                {
                    _graph.FinalizePlot();
                    GUI.EndClip();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    return;
                }

                // Draw the coverage points on the graph
                _graph.GraphDatasetCoverage(ref _linearVelocities, ref _angularVelocities);

                // With the scores calculated on the previous step, display them with their respective colors on the graph
                if (redundancy)
                {
                    if (!_isRedundancyPrepared)
                    {
                        PrepareRedundancy();
                        _isRedundancyPrepared = true;
                    }
                    
                    var idx = 0;

                    for (var i = _yMin; i < _yMax; i++)
                    {
                        for (var j = _xMin; j < _xMax; j++)
                        {
                            var pos = (new Vector2(j * _angularStep, i),
                                new Vector2((j + 1) * _angularStep, i + 1));

                            _graph.DisplayRedundancy(pos, _redundancyScores[idx] / _redundancyScores.Max());
                            idx++;
                        }
                    }
                }

                var speeds = _graph.RectPositionToSpeed(_mouseX, _mouseY);

                (_pose, _animAndPose) = FindClosestPose(speeds);

                _graph.DrawClick(_angularVelocities[_animAndPose.Item1][_animAndPose.Item2],
                    _linearVelocities[_animAndPose.Item1][_animAndPose.Item2]);

                // Finalize calls to GL and order a draw from the GPU 
                _graph.FinalizePlot();
            }
            GUI.EndClip();

            EditorGUILayout.EndVertical();

            _infoPanelScrollPos = EditorGUILayout.BeginScrollView(_infoPanelScrollPos, GUILayout.Width(315));
            
            DrawInfoPanel(_pose, _animAndPose);
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndHorizontal();
        }
    }
}
