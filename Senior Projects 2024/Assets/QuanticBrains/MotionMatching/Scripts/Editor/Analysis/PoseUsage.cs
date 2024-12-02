using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using QuanticBrains.MotionMatching.Scripts.Containers;
using UnityEditor;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Editor.Analysis
{
    /// <summary>
    /// This class implements the animation usage metrics
    /// </summary>
    public class PoseUsage : EditorWindow
    {
        public GameObject characterToMonitor;

        private Dataset _dataset;
        private MotionMatching _mmData;
        private List<bool> _animationUsage;
        private List<List<int>> _poseUsage;
        private Queue<string> _poseHistory;
        private List<string> _animationNames;
        private int _clickedAnimation;
        private Vector2 _animsScrollPos, _posesScrollPos, _historyScrollPos;
        private bool _dataInitialized;
        private const int HistorySize = 100;
        private float _time;

        private GUIStyle _boldToolbarStyle;

        [MenuItem("Tools/Motion Matching/Dataset Analysis/Live Metrics/Pose Usage")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            GetWindow(typeof(PoseUsage), false, "Pose Usage");
        }

        private void OnEnable()
        {
            ClearData();
        }

        private void TryInitializeData()
        {
            if (_dataInitialized)
            {
                return;
            }

            _mmData = characterToMonitor.GetComponent<MotionMatching>();
            
            _dataset = _mmData.dataset;
            foreach (var path in _dataset.animationPaths)
            {
                _animationNames.Add(Path.GetFileNameWithoutExtension(path));
            }
            _poseHistory = new Queue<string>();

            foreach (var lap in _dataset.lastAnimationPoses)
            {
                _animationUsage.Add(false);
                _poseUsage.Add(new List<int>());

                var animIdx = _poseUsage.Count - 1;

                for (var i = 0; i <= lap; i++)
                {
                    _poseUsage[animIdx].Add(0);
                }
            }

            _dataInitialized = true;

            for (var i = 0; i < HistorySize; i++)
            {
                _poseHistory.Enqueue("-1");
            }
        }

        /// <summary>
        /// This function accounts the use of a specific pose for the current animation
        /// </summary>
        private void UpdateAnimationUsage()
        {
            if (_mmData == null)
            {
                return;
            }
            
            var (animation, pose) = (_mmData.currentAnimationID, _mmData.currentAnimationFrame);

            _poseHistory.Dequeue();
            _poseHistory.Enqueue(pose.ToString());
            
            _animationUsage[animation] = true;
            
            _poseUsage[animation][pose] += 1;
        }
        
        /// <summary>
        /// This function resets the data containers to their original state
        /// </summary>
        private void ClearData()
        {
            _poseUsage = new List<List<int>>();
            _animationUsage = new List<bool>();
            _animationNames = new List<string>();
            _mmData = null;
            _dataset = null;
            _dataInitialized = false;
            _clickedAnimation = -1;

            _boldToolbarStyle ??= new GUIStyle(EditorStyles.toolbarButton)
            {
                fontStyle = FontStyle.Normal
            };
        }

        /// <summary>
        /// This function displays the used poses from an animation and the amount of times each of them
        /// were used in a ScrollView on the right side of the window. Poses with no use are not displayed.
        /// </summary>
        private void DisplayClickedAnimation()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Animation poses", EditorStyles.boldLabel);
            _posesScrollPos = EditorGUILayout.BeginScrollView(_posesScrollPos);

            if (_clickedAnimation < 0)
            {
                //Styles
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontStyle = FontStyle.Italic;
                
                EditorGUILayout.LabelField("Click on an animation used in"+"\nthe left bar to see its selected poses", 
                    labelStyle, GUILayout.Height(50));
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                return;
            }
            
            for (var i = 0; i < _poseUsage[_clickedAnimation].Count; i++)
            {
                if (_poseUsage[_clickedAnimation][i] == 0)
                {
                    continue;
                }
                
                EditorGUILayout.LabelField("Pose " + i + ": " + _poseUsage[_clickedAnimation][i] + " uses");
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// This function draws the available animations into a ScrollView on the left side of the window.
        /// </summary>
        private void DisplayAnimations()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Animation names", EditorStyles.boldLabel);
            _animsScrollPos = EditorGUILayout.BeginScrollView(_animsScrollPos);
            for (var i = 0; i < _animationUsage.Count; i++)
            {
                var count = ((float)_poseUsage[i].Count(x => x > 0) / _poseUsage[i].Count) * 100f;

                var animation = _animationNames[i] + " (" + count.ToString("n1") + "%)";

                var style = EditorStyles.toolbarButton;
                if (_clickedAnimation == i)
                {
                    style = _boldToolbarStyle;
                    style.fontStyle = FontStyle.Bold;
                }
                
                if (GUILayout.Button(animation, style))
                {
                    _clickedAnimation = i;
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// This function draws the pose history on the right side of the window. 
        /// </summary>
        private void DisplayHistory()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Pose history", EditorStyles.boldLabel);
            _historyScrollPos = EditorGUILayout.BeginScrollView(_historyScrollPos, GUILayout.Width(250));
            //string.Join(',', _poseHistory);
            StringBuilder sb = new StringBuilder();
            foreach (var pose in _poseHistory)
            {
                sb.AppendLine(pose);
            }
            
            EditorGUILayout.TextArea(sb.ToString());
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        public void OnGUI()
        {
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Analysis parameters");
            
            EditorGUI.indentLevel = 1;
            
            characterToMonitor =
                EditorGUILayout.ObjectField("Character to Monitor", characterToMonitor, typeof(GameObject), true) as GameObject;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            var usedAnims = _animationUsage.Count(val => val);

            if (!Application.isPlaying)
            {
                return;
            }

            if (characterToMonitor == null)
            {
                return;
            }

            if (!_dataInitialized)
            {
                TryInitializeData();
            }
            
            EditorGUI.ProgressBar(
                new Rect(3, 50, position.width - 6, 25), 
                usedAnims / (float)_animationUsage.Count, 
                usedAnims + " of " + _animationUsage.Count + " animations used.");
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            DisplayAnimations();
            DisplayClickedAnimation();
            DisplayHistory();
            EditorGUILayout.EndHorizontal();
        }

        public void Update()
        {
            if (EditorApplication.isPaused)
            {
                return;
            }

            if (Application.isPlaying)
            {
                _time += Time.deltaTime;

                if (_time < (1 / 15f))
                {
                    UpdateAnimationUsage();
                    return;
                }

                _time = 0;
                
                if (characterToMonitor == null)
                {
                    ClearData();
                    return;
                }
                
                UpdateAnimationUsage();
                
                // This function is called to ensure that a repaint occurs and the window is updated
                Repaint();

                return;
            }

            if (_poseUsage.Count == 0)
            {
                return;
            }
            
            ClearData();
        }
    }
}
