using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QuanticBrains.MotionMatching.Scripts.Containers;
using UnityEditor;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Editor.Analysis
{
    /// <summary>
    /// This class implements the transitioning cost metrics
    /// </summary>
    public class TransitionCost : EditorWindow
    {
        public GameObject characterToMonitor;

        private MotionMatching _mmData;
        private TransitionCostGraph _graph;
        private AnimationData _poseInfo;
        private List<GraphPointInfo> _transitionCostAndPose;
        private GraphPointInfo _shownInfo;
        private bool _dataInitialized;
        private float _mouseX;
        private int _clickedPose;
        private Vector2 _infoScrollPos;
        private float _windowWidth, _time;
        private List<string> _animationNames;
        private GUIStyle _style;

        [MenuItem("Tools/Motion Matching/Dataset Analysis/Live Metrics/Transition Cost")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            GetWindow(typeof(TransitionCost), false, "Transition Cost");
        }

        private void OnEnable()
        {
            _graph = CreateInstance(typeof(TransitionCostGraph)) as TransitionCostGraph;

            ClearData();
        }

        private void TryInitializeData()
        {
            if (_dataInitialized)
            {
                return;
            }
            
            _style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };

            _mmData = characterToMonitor.GetComponent<MotionMatching>();
            _clickedPose = -1;
            
            foreach (var path in _mmData.dataset.animationPaths)
            {
                _animationNames.Add(Path.GetFileNameWithoutExtension(path));
            }
            
            _dataInitialized = true;
        }

        /// <summary>
        /// Checks the cost associated to the current transition and manages the cost container if it is full 
        /// </summary>
        private void TransitioningCost()
        {
            if ((int)_windowWidth == _transitionCostAndPose.Count && _transitionCostAndPose.Count > 0)
            {
                _transitionCostAndPose.RemoveAt(0);
            }
            
            _transitionCostAndPose.Add(new GraphPointInfo(_mmData.currentQueryFlow.currentMinDistance, _mmData.currentAnimationID, _mmData.currentAnimationFrame));

            _clickedPose++;
        }

        /// <summary>
        /// Resets containers to original values
        /// </summary>
        private void ClearData()
        {
            _transitionCostAndPose = new List<GraphPointInfo>();
            _dataInitialized       = false;
            _animationNames = new List<string>();
            _clickedPose = -1;
            _shownInfo = null;
        }

        /// <summary>
        /// This functions draws the info panel for the selected pose on the right side of the window
        /// </summary>
        private void DrawInfoPanel()
        {
            if (_transitionCostAndPose.Count == 0)
            {
                return;
            }

            if (_poseInfo.bonesData == null || _poseInfo.bonesData.Length == 0)
            {
                return;
            }

            if (_shownInfo == null)
            {
                return;
            }

            EditorGUILayout.LabelField("Animation: " + _animationNames[_shownInfo.animation]);
            EditorGUILayout.IntField("Pose: ", _shownInfo.pose, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Root", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.Vector3Field("Position",_poseInfo.rootPosition);
            EditorGUILayout.Vector3Field("Rotation", ((Quaternion)_poseInfo.rootRotation).eulerAngles);
            
            
            var boneList = _mmData.avatar.GetAvatarDefinition();
            foreach (var (boneName, index) in boneList.Select((boneName, index) => (boneName.alias, index)))
            {
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
                EditorGUILayout.LabelField(boneName, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.Vector3Field("Position", _poseInfo.bonesData[index].position);
                EditorGUILayout.Vector3Field("Rotation", ((Quaternion)_poseInfo.bonesData[index].rotation).eulerAngles);
                EditorGUILayout.Vector3Field("Velocity", _poseInfo.bonesData[index].velocity);
                EditorGUILayout.Vector3Field("Angular Velocity", _poseInfo.bonesData[index].angularVelocity);
            }
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Analysis parameters");

            EditorGUI.indentLevel = 1;

            characterToMonitor =
                EditorGUILayout.ObjectField("Character to Monitor", characterToMonitor, typeof(GameObject), true) as GameObject;

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            _graph.rect = GUILayoutUtility.GetRect(500, 1000, 500, 1000);

            if (!Application.isPlaying)
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                return;
            }

            if (characterToMonitor == null)
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                return;
            }

            if (!_dataInitialized)
            {
                TryInitializeData();
            }

            if (Event.current.type is EventType.MouseDown)
            {
                if (Event.current.mousePosition.x <= _windowWidth)
                {
                    _mouseX = Event.current.mousePosition.x;

                    _clickedPose = (int)(_windowWidth - _mouseX);

                    _shownInfo = _transitionCostAndPose[^_clickedPose];

                    _poseInfo = _mmData.dataset.animationsData[_shownInfo.animation][_shownInfo.pose];
                }
            }

            GUI.BeginClip(_graph.rect);
            if (Event.current.type is EventType.Repaint)
            {
                _graph.InitializePlot();
                _graph.DrawLabels(8);

                if (_graph.rect.width > 1)
                {
                    _windowWidth = _graph.rect.width;
                }

                _graph.DrawTransitionCost(ref _transitionCostAndPose);

                if (_shownInfo != null)
                {
                    _graph.DrawClick(_clickedPose, _transitionCostAndPose[^_clickedPose].distance);
                }

                _graph.FinalizePlot();
            }
            GUI.EndClip();

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();

            _infoScrollPos = EditorGUILayout.BeginScrollView(_infoScrollPos, GUILayout.Width(315));
            
            DrawInfoPanel();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
        }

        public void Update()
        {
            if (EditorApplication.isPaused)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                ClearData();
                _graph.Reset();
                return;
            }
            
            if (characterToMonitor == null)
            {
                ClearData();
                _graph.Reset();
                return;
            }
            
            _time += Time.deltaTime;

            if (_time < 1f/3f)
            {
                TransitioningCost();
                return;
            }
            
            _time = 0;

            Repaint();
        }
    }

    public class GraphPointInfo : IComparable<GraphPointInfo>
    {
        public float distance;
        public int animation;
        public int pose;

        public GraphPointInfo(float d, int a, int p)
        {
            distance = d;
            animation = a;
            pose = p;
        }

        public int CompareTo(GraphPointInfo other)
        {
            return distance.CompareTo(other.distance);
        }
    }
}
