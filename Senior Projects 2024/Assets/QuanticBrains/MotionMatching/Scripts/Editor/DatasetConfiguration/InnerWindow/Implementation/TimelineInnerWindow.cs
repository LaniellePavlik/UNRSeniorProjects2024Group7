using System;
using System.Collections.Generic;
using System.Linq;
using QuanticBrains.MotionMatching.PreviewCamera;
using QuanticBrains.MotionMatching.Scripts.Editor.DatasetConfiguration.InnerWindow;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QuanticBrains.MotionMatching.Scripts.Editor.DatasetConfiguration.InnerWindow.Implementation
{
    [Serializable]
    public class TimelineInnerWindow : InnerWindowBase
    {
        #region Variables

        //Camera
        private readonly PreviewCameraBehaviour _previewCameraBehaviour;
        private readonly string[] _viewOptions;
        private int _selectedView;

        //Positions
        private const float OneColumnSpace = ColumnWidth * 2 + ColumnSpace;
        private const float TimelineLabelLength = ColumnWidth * 0.25f;
        private const float AnimTextLength = ColumnWidth;
        private const float LabelsDisplacement = 7 * ColumnSpace + AnimTextLength;

        //Anim list
        public Vector2 animsListScrollPos;

        //Labels properties
        private readonly string _warningSign = "\u26A0";
        private readonly string[] _labels = { "Walk", "Run", "Strafe", "Style" };
        private readonly int[] _labelSize = { 35, 27, 43, 60 };

        //Styles
        private readonly GUIStyle _taggedStyle;
        private readonly GUIStyle _regularStyle;

        //UI Elements
        private int _selectedFilter;

        //Filter
        private List<string> _filterOptions;
        private int _baseOptions;
        private int _currentFiltered = 0;
        
        //Tags
        private List<string> _tagsOptions;

        //Camera minMax values
        private float _minValueDepth = 1.5f;
        private float _maxValueDepth = 5;

        private float _minValue = -1.5f;
        private float _maxValue = 1.5f;

        private float _vertical;
        private float _horizontal;
        private float _depth;

        #endregion

        #region BaseMethods

        public TimelineInnerWindow(string name, DatasetSetup datasetSetup) : base(name, datasetSetup)
        {
            //Styles
            _taggedStyle = new GUIStyle();
            _taggedStyle.normal.textColor = Color.white;
            _taggedStyle.fontStyle = FontStyle.Bold;

            _regularStyle = new GUIStyle();
            _regularStyle.normal.textColor = Color.gray;
            _regularStyle.fontStyle = FontStyle.Normal;
            
            _previewCameraBehaviour = DatasetSetup.previewCam.GetComponent<PreviewCameraBehaviour>();
            _previewCameraBehaviour.SetCamera(_previewCameraBehaviour.transform);
            
            if (DatasetSetup.character) _previewCameraBehaviour.SetTarget(DatasetSetup.character.transform);

            _viewOptions = Enum.GetNames(typeof(CameraView)).ToArray();

            UpdateFilterOptions();
        }

        public void UpdateFilterOptions()
        {
            _filterOptions = Enum.GetNames(typeof(AnimationFilter)).ToList();
            _baseOptions = _filterOptions.Count;

            _filterOptions = _filterOptions.Concat(DatasetSetup.motionTags).ToList();
            _filterOptions = _filterOptions.Concat(DatasetSetup.styleTags).ToList();

            _tagsOptions = new List<string>();
            _tagsOptions = DatasetSetup.motionTags.Concat(DatasetSetup.styleTags).ToList();
        }

        public override void OnDrawWindow()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(OneColumnSpace)); //Column

            if (!DrawAndCheckIfVisible())
            {
                EditorGUILayout.EndVertical(); //End of Column
                return;
            }

            DrawAnimationList();

            if (!DatasetSetup.isRecording)
            {
                DrawTimeline();    
            }
            
            DrawAnimationPreview();

            EditorGUILayout.EndVertical(); //End of Column
        }

        #endregion

        #region DrawMethods

        private bool DrawAndCheckIfVisible()
        {
            if (DatasetSetup.animations.Count < 1)
            {
                EditorGUILayout.LabelField("Add animations to your dataset to use this panel", EditorStyles.boldLabel,
                    GUILayout.Width(ColumnWidth));
                EditorGUILayout.LabelField("Animations > Drop Area", GUILayout.Width(ColumnWidth));
                return false;
            }
            
            if (!DatasetSetup.character)
            {
                EditorGUILayout.LabelField("Select a character to use this panel", EditorStyles.boldLabel,
                    GUILayout.Width(ColumnWidth));
                EditorGUILayout.LabelField("Configuration > Character GameObject", GUILayout.Width(ColumnWidth));
                return false;
            }

            return true;
        }

        private void DrawAnimationList()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(OneColumnSpace));
            EditorGUILayout.LabelField(new GUIContent("Animations List"), EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(ColumnWidth));
            _selectedFilter = EditorGUILayout.Popup("Filtered Tags", _selectedFilter, _filterOptions.ToArray());
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(OneColumnSpace));

            var height = DatasetSetup.AnimSelectorList.elementHeight;
            animsListScrollPos = EditorGUILayout.BeginScrollView(animsListScrollPos, false, true,
                GUILayout.Height(Math.Min(height * 10f, height * (_currentFiltered + 2))));
            ResetAnimationsHeight();
            DatasetSetup.AnimSelectorList.DoLayoutList();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
        }
        
        private void ResetAnimationsHeight()
        {
            _currentFiltered = 0;
            for (int i = 0; i < DatasetSetup.animations.Count; i++)
            {
                bool[] isTagged = DatasetSetup.CheckIfTaggedByMotionOrStyles(DatasetSetup.animations[i].name);
                DatasetSetup.animationsFiltered[i] = IsFiltered(isTagged, _selectedFilter);
                
                if (DatasetSetup.animationsFiltered[i]) _currentFiltered++;
                
                DatasetSetup.animationsHeight[i] = DatasetSetup.animationsFiltered[i]
                    ? EditorGUIUtility.singleLineHeight
                    : 0;
            }
        }

        private void DrawTimeline()
        {
            if (DatasetSetup.selectedAnimation != DatasetSetup.lastSelectedAnimation)
            {
                DatasetSetup.lastSelectedAnimation = DatasetSetup.selectedAnimation;
                //DatasetSetup.ClearData(DataManagementEnum.Reinitialize);
            }

            EditorGUILayout.LabelField("Clip Tagging", EditorStyles.boldLabel, GUILayout.Width(ColumnWidth));

            // Anim control
            EditorGUILayout.BeginHorizontal();

            //Column 1
            EditorGUILayout.BeginVertical(GUILayout.Width(ColumnSpace));
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(DatasetSetup.animations[DatasetSetup.selectedAnimation].name,
                EditorStyles.boldLabel, GUILayout.Width(0.58f * ColumnWidth));
            if (GUILayout.Button("<", GUILayout.Width(30)))
            {
                DatasetSetup.selectedAnimation--;
                if (DatasetSetup.selectedAnimation < 0)
                    DatasetSetup.selectedAnimation = DatasetSetup.animations.Count - 1;
            }

            if (GUILayout.Button(">", GUILayout.Width(30)))
            {
                DatasetSetup.selectedAnimation =
                    (DatasetSetup.selectedAnimation + 1) % DatasetSetup.animations.Count;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            
            //Tags control
            EditorGUILayout.BeginHorizontal("Box");

            //Animation selected
            DatasetSetup.anim = DatasetSetup.animations[DatasetSetup.selectedAnimation];

            //Column 1
            EditorGUILayout.BeginVertical(GUILayout.Width(ColumnSpace));

            //Add intervals to tags
            DatasetSetup.selectedTag = EditorGUILayout.Popup("Available tags", DatasetSetup.selectedTag,
                _tagsOptions.ToArray());

            EditorGUILayout.BeginHorizontal();
            DatasetSetup.start = EditorGUILayout.IntField(DatasetSetup.start, GUILayout.Width(75));
            DatasetSetup.end = EditorGUILayout.IntField(DatasetSetup.end, GUILayout.Width(75));

            if (GUILayout.Button("Add interval"))
            {
                DatasetSetup.AddInterval(DatasetSetup.anim.name, DatasetSetup.start, DatasetSetup.end,
                    DatasetSetup.totalFrames, DatasetSetup.selectedTag);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            //Column 2
            EditorGUILayout.BeginVertical(GUILayout.Width(ColumnSpace));

            //Frame stepping for preview
            EditorGUILayout.LabelField("Frame stepping");

            EditorGUILayout.BeginHorizontal();
            var tempStep = EditorGUILayout.DelayedIntField(DatasetSetup.currentStep);
            StepFrame(tempStep);

            if (GUILayout.Button("<"))
            {
                StepFrame(DatasetSetup.currentStep - 1);
            }

            if (GUILayout.Button(">"))
            {
                StepFrame(DatasetSetup.currentStep + 1);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            //Timeline box
            EditorGUILayout.BeginHorizontal("Box");

            EditorGUILayout.BeginVertical(GUILayout.Width(TimelineLabelLength));

            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
            //Timeline labels
            EditorGUILayout.LabelField("Motions", EditorStyles.boldLabel, GUILayout.Width(TimelineLabelLength));

            var rect = GUILayoutUtility.GetLastRect();
            if (rect.height > 1)
            {
                DatasetSetup.motionIntervalStart = (int)(rect.y + rect.height);
            }

            EditorGUI.indentLevel++;

            var idx = 0;
            foreach (var item in DatasetSetup.motionTags)
            {
                var (_, hovered, motionLabels) = HoveredLabel();

                if (idx == hovered && motionLabels)
                {
                    EditorGUILayout.LabelField(item, EditorStyles.boldLabel, GUILayout.Width(TimelineLabelLength));
                }
                else
                {
                    EditorGUILayout.LabelField(item, GUILayout.Width(TimelineLabelLength));
                }

                idx++;
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Styles", EditorStyles.boldLabel, GUILayout.Width(TimelineLabelLength));

            rect = GUILayoutUtility.GetLastRect();
            if (rect.height > 1)
            {
                DatasetSetup.styleIntervalStart = (int)(rect.y + rect.height);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;

            for (var i = 0; i < DatasetSetup.styles.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var (_, hovered, motionLabel) = HoveredLabel();

                if (i == hovered && !motionLabel)
                {
                    EditorGUILayout.LabelField(DatasetSetup.styles[i].name, EditorStyles.boldLabel,
                        GUILayout.Width(TimelineLabelLength));
                }
                else
                {
                    EditorGUILayout.LabelField(DatasetSetup.styles[i].name, GUILayout.Width(TimelineLabelLength));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical();

            //Timeline itself
            var height = (DatasetSetup.motionTags.Count + DatasetSetup.styleTags.Count + 6) *
                         EditorGUIUtility.singleLineHeight;
            DatasetSetup.graph.rect = GUILayoutUtility.GetRect(0,
                OneColumnSpace - TimelineLabelLength, 0, height);
            DatasetSetup.graph.rect.position = new Vector2(DatasetSetup.graph.rect.position.x + 30,
                DatasetSetup.graph.rect.position.y);
            DatasetSetup.graph.rect.width -= 2 * 30;

            GUI.BeginClip(DatasetSetup.graph.rect);
            CheckTimelineInteraction();
            if (Event.current.type is EventType.Repaint)
            {
                DrawTimelineGraph();
            }

            GUI.EndClip();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawAnimationPreview()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(OneColumnSpace));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preview animation", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            var sceneRect = GUILayoutUtility.GetLastRect();
            sceneRect.x += ColumnWidth + 2 * ColumnSpace; // adjust the windows header

            EditorGUILayout.Space();

            EditorGUILayout.Space(30);
            _depth = EditorGUILayout.Slider("Camera Depth", _depth, _minValueDepth, _maxValueDepth,
                GUILayout.Width(ColumnWidth));
            EditorGUILayout.Space(20);
            _vertical = EditorGUILayout.Slider("Camera Vertical", _vertical, _minValue, _maxValue,
                GUILayout.Width(ColumnWidth));
            EditorGUILayout.Space(20);
            _horizontal = EditorGUILayout.Slider("Camera Horizontal", _horizontal, _minValue, _maxValue,
                GUILayout.Width(ColumnWidth));

            //View grid
            EditorGUILayout.Space(20);
            _selectedView = GUILayout.Toolbar(
                _selectedView, _viewOptions, GUILayout.Width(ColumnWidth), GUILayout.Height(ColumnWidth / 6f));
            EditorGUILayout.Space(10);

            UpdateCameraView();

            // Render texture
            if (Event.current.type == EventType.Repaint)
            {
                RenderTexture tex = DatasetSetup.previewCam.targetTexture;
                Texture t = tex;

                var width = ColumnWidth - 3 * ColumnSpace;
                EditorGUI.DrawPreviewTexture(new Rect(sceneRect.x, sceneRect.y - 10, width, width), t);
            }

            EditorGUILayout.EndVertical();
        }

        private void UpdateCameraView()
        {
            Enum.TryParse(_viewOptions[_selectedView], out CameraView view);

            //Update camera preview before render
            _previewCameraBehaviour.UpdateCameraView(view, _depth, _vertical, _horizontal);
        }

        public void SetPreviewTarget(Transform target)
        {
            _previewCameraBehaviour.SetTarget(target);
        }

        #endregion

        #region ClearData

        public override void ClearData(DataManagementEnum type)
        {
            if (type == DataManagementEnum.Initial)
            {
                DatasetSetup.AnimSelectorList = new ReorderableList(DatasetSetup.animations,
                    typeof(List<AnimationClip>),
                    false, false, false, false)
                {
                    drawElementCallback = DrawAnimationsItems,
                    elementHeightCallback = AnimationElementHeight,
                };
            }
        }

        #endregion

        #region ReorderableListMethods

        private float AnimationElementHeight(int index)
        {
            return DatasetSetup.animationsHeight[index];
        }

        private bool IsFiltered(bool[] isTaggedBy, int selectedOption)
        {
            //0-3 are enum based
            switch ((AnimationFilter)selectedOption)
            {
                case AnimationFilter.All:
                    return true;

                case AnimationFilter.Untagged:
                    return isTaggedBy.All(x => !x);

                case AnimationFilter.Motions:
                    return isTaggedBy.Take(DatasetSetup.motionTags.Count).Any(x => x);

                case AnimationFilter.Styles:
                    return isTaggedBy.Skip(DatasetSetup.motionTags.Count).Any(x => x);
            }

            //Rest of cases
            return isTaggedBy[selectedOption - _baseOptions];
        }

        private void DrawAnimationsItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            bool[] isTagged = DatasetSetup.CheckIfTaggedByMotionOrStyles(DatasetSetup.animations[index].name);

            //Filter
            if (!DatasetSetup.animationsFiltered[index])
            {
                DatasetSetup.animationsHeight[index] = 0;
                return;
            }

            if (isFocused) DatasetSetup.selectedAnimation = index;

            EditorGUILayout.BeginHorizontal();
            EditorGUI.LabelField(new Rect(rect.x, rect.y, AnimTextLength, EditorGUIUtility.singleLineHeight),
                DatasetSetup.animations[index].name);

            var tagsX = rect.x + LabelsDisplacement;

            if (isTagged.All(x => x == false))
            {
                EditorGUI.LabelField(new Rect(tagsX, rect.y, 20, EditorGUIUtility.singleLineHeight),
                    new GUIContent(_warningSign, "Untagged Animation"), _taggedStyle);
            }

            var space = 20;
            for (int i = 0; i < DatasetSetup.motions.Count; i++)
            {
                EditorGUI.LabelField(new Rect(tagsX + space, rect.y, _labelSize[i], EditorGUIUtility.singleLineHeight),
                    _labels[i], isTagged[i] ? _taggedStyle : _regularStyle);

                space += _labelSize[i];
            }

            //Styles
            var isTaggedByStyle = false;
            for (int i = DatasetSetup.motions.Count; i < isTagged.Length; i++)
            {
                if (!isTagged[i]) continue;

                isTaggedByStyle = true;
                break;
            }

            EditorGUI.LabelField(new Rect(tagsX + space, rect.y, _labelSize[^1], EditorGUIUtility.singleLineHeight),
                _labels[^1], isTaggedByStyle ? _taggedStyle : _regularStyle);
            //

            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region TimelineMethods

        private void DrawTimelineGraph()
        {
            DatasetSetup.timelineBeginGroup = (int)GUILayoutUtility.GetLastRect().y;

            DatasetSetup.graph.InitializePlot();
            DatasetSetup.graph.DrawBackground(DatasetSetup.motionIntervalStart - DatasetSetup.timelineBeginGroup);

            if (DatasetSetup.anim == null)
            {
                DatasetSetup.graph.FinalizePlot();
                return;
            }

            DatasetSetup.totalFrames = DatasetSetup.anim.length * DatasetSetup.anim.frameRate - 1;
            DatasetSetup.graph.DrawTime(DatasetSetup.anim.length, DatasetSetup.totalFrames);

            if (DatasetSetup.animations.Count > 0)
            {
                DatasetSetup.graph.DrawTagInterval(DatasetSetup.intervalStart, DatasetSetup.intervalEnd,
                    DatasetSetup.selectedLabel, Color.green, DatasetSetup.offset - DatasetSetup.timelineBeginGroup,
                    DatasetSetup.anim.frameRate,
                    DatasetSetup.verticalCorrection);

                foreach (var it in DatasetSetup.motions.Select((tag, index) => new { tag, index }))
                {
                    foreach (var interval in it.tag.ranges)
                    {
                        if (DatasetSetup.anim.name != interval.animName)
                        {
                            continue;
                        }

                        DatasetSetup.graph.DrawTagInterval(interval.frameStart, interval.frameStop, it.index,
                            Color.white, DatasetSetup.motionIntervalStart - DatasetSetup.timelineBeginGroup,
                            DatasetSetup.anim.frameRate);
                    }
                }

                foreach (var it in DatasetSetup.styles.Select((tag, index) => new { tag, index }))
                {
                    foreach (var interval in it.tag.ranges)
                    {
                        if (DatasetSetup.anim.name != interval.animName)
                        {
                            continue;
                        }

                        DatasetSetup.graph.DrawTagInterval(interval.frameStart, interval.frameStop, it.index,
                            Color.white, DatasetSetup.styleIntervalStart - DatasetSetup.timelineBeginGroup,
                            DatasetSetup.anim.frameRate, -1);
                    }
                }

                if (DatasetSetup.drawHighlight)
                {
                    DatasetSetup.graph.DrawTagInterval(DatasetSetup.highlightRange.frameStart,
                        DatasetSetup.highlightRange.frameStop, DatasetSetup.highlightLabel, Color.yellow,
                        DatasetSetup.offset - DatasetSetup.timelineBeginGroup, DatasetSetup.anim.frameRate,
                        DatasetSetup.verticalCorrection);
                }
            }

            DatasetSetup.graph.DrawNavigator(DatasetSetup.navPosX);

            if (DatasetSetup.mouseY < (DatasetSetup.styleIntervalStart - DatasetSetup.timelineBeginGroup))
                DatasetSetup.graph.DrawHelpers(DatasetSetup.mouseX, DatasetSetup.mouseY, DatasetSetup.col);

            DatasetSetup.graph.FinalizePlot();
        }

        private void StepFrame(int nextFrame)
        {
            if (nextFrame > DatasetSetup.totalFrames)
            {
                DatasetSetup.currentStep = (int)Math.Floor(DatasetSetup.totalFrames);
                DatasetSetup.navPosX =
                    DatasetSetup.graph.NavFrameToPos(DatasetSetup.currentStep, DatasetSetup.totalFrames);
                return;
            }

            if (nextFrame < 0)
            {
                DatasetSetup.currentStep = 0;
                DatasetSetup.navPosX = 0;
                return;
            }

            DatasetSetup.currentStep = nextFrame;
            DatasetSetup.navPosX =
                DatasetSetup.graph.NavFrameToPos(DatasetSetup.currentStep, DatasetSetup.totalFrames);

            ApplyAnimation(DatasetSetup.graph.NavPosToTime(DatasetSetup.navPosX, DatasetSetup.totalFrames,
                DatasetSetup.anim.frameRate));
        }

        private (int, int, bool) HoveredLabel()
        {
            var timelineOffset = DatasetSetup.timelineBeginGroup;
            if (DatasetSetup.mouseY < DatasetSetup.motionIntervalStart - timelineOffset)
            {
                return (-1, -1, false);
            }

            int selectedField;

            if ((DatasetSetup.mouseY)
                < (DatasetSetup.styleIntervalStart - timelineOffset))
            {
                selectedField = (int)((DatasetSetup.mouseY - DatasetSetup.motionIntervalStart + timelineOffset) /
                                      (DatasetSetup.graph.tagFieldVerticalSize +
                                       DatasetSetup.graph.spaceBetweenTextFields));

                if (selectedField >= DatasetSetup.motions.Count)
                {
                    return (-1, -1, false);
                }

                DatasetSetup.verticalCorrection = 0;
                return (DatasetSetup.motionIntervalStart, selectedField, true);
            }

            if (DatasetSetup.styles.Count > 0)
            {
                selectedField = (int)((DatasetSetup.mouseY - DatasetSetup.styleIntervalStart + timelineOffset) /
                                      (DatasetSetup.graph.styleFieldVerticalSize +
                                       DatasetSetup.graph.spaceBetweenTextFields));

                DatasetSetup.verticalCorrection = -1;
                return (DatasetSetup.styleIntervalStart, selectedField, false);
            }

            return (-1, -1, false);
        }

        private void CheckTimelineInteraction()
        {
            if (Event.current.type is EventType.Ignore)
            {
                DatasetSetup.CheckIntervalAddition();
                //DatasetSetup.offset = 0;
            }

            if (Event.current.type is EventType.MouseDown or EventType.MouseDrag)
            {
                DatasetSetup.mouseY = Event.current.mousePosition.y;
                DatasetSetup.mouseX = Event.current.mousePosition.x;

                if (DatasetSetup.mouseX < 0)
                {
                    return;
                }

                if (DatasetSetup.mouseY < DatasetSetup.graph.timeRulerLimit)
                {
                    DatasetSetup.navPosX = DatasetSetup.mouseX;
                    DatasetSetup.currentStep =
                        (int)DatasetSetup.graph.NavPosToFrame(DatasetSetup.navPosX, DatasetSetup.totalFrames);
                }

                if (DatasetSetup.character == null)
                {
                    if (DatasetSetup.animations.Count > 0)
                    {
                        EditorUtility.DisplayDialog("Error while interacting with the timeline",
                            "No target is assigned for the preview. " +
                            "Please add one in the Character parameters section.", "Ok");
                    }

                    return;
                }

                if (DatasetSetup.anim != null)
                {
                    ApplyAnimation(DatasetSetup.graph.NavPosToTime(DatasetSetup.navPosX, DatasetSetup.totalFrames,
                        DatasetSetup.anim.frameRate));
                }
            }

            if (Event.current.type is EventType.MouseDown)
            {
                GUI.FocusControl(null); // Lose focus from the dataset configuration window
                DatasetSetup.drawHighlight = false;
                (DatasetSetup.offset, DatasetSetup.selectedLabel, DatasetSetup.motionSpace) = HoveredLabel();

                if (DatasetSetup.selectedLabel >= 0)
                {
                    DatasetSetup.intervalStart =
                        (int)DatasetSetup.graph.NavPosToFrame(Event.current.mousePosition.x,
                            DatasetSetup.totalFrames);
                    DatasetSetup.intervalEnd = DatasetSetup.intervalStart;
                }
                else
                {
                    DatasetSetup.intervalStart = 0;
                    DatasetSetup.intervalEnd = 0;
                    DatasetSetup.offset = 0;
                }
            }

            if (Event.current.type is EventType.MouseDrag)
            {
                if (Event.current.mousePosition.x > 0)
                {
                    if (Event.current.mousePosition.y >=
                        (DatasetSetup.motionIntervalStart - DatasetSetup.timelineBeginGroup))
                    {
                        DatasetSetup.intervalEnd =
                            (int)DatasetSetup.graph.NavPosToFrame(Event.current.mousePosition.x,
                                DatasetSetup.totalFrames);
                    }
                }
            }

            if (Event.current.type is EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Escape)
                {
                    DatasetSetup.intervalStart = 0;
                    DatasetSetup.intervalEnd = 0;
                    DatasetSetup.drawHighlight = false;
                }

                if (Event.current.keyCode == KeyCode.Delete)
                {
                    if (DatasetSetup.drawHighlight)
                    {
                        if (DatasetSetup.motionSpace &&
                            DatasetSetup.motions[DatasetSetup.highlightLabel].ranges.Count > 0)
                        {
                            DatasetSetup.motions[DatasetSetup.highlightLabel].ranges
                                .RemoveAt(DatasetSetup.highlightIndex);
                        }
                        else if (DatasetSetup.styles[DatasetSetup.highlightLabel].ranges.Count > 0)
                        {
                            DatasetSetup.styles[DatasetSetup.highlightLabel].ranges
                                .RemoveAt(DatasetSetup.highlightIndex);
                        }

                        DatasetSetup.drawHighlight = false;
                        DatasetSetup.Repaint();
                    }
                }
            }

            if (Event.current.type is EventType.MouseUp)
            {
                DatasetSetup.CheckIntervalAddition();
            }

            if (Event.current.type is EventType.MouseMove)
            {
                DatasetSetup.mouseY = Event.current.mousePosition.y;
                DatasetSetup.mouseX = Event.current.mousePosition.x;
                DatasetSetup.col = Color.gray;

                if (DatasetSetup.mouseY < DatasetSetup.graph.timeRulerLimit)
                {
                    DatasetSetup.col = Color.clear;
                }

                if (DatasetSetup.mouseX < 0)
                {
                    DatasetSetup.col = Color.clear;
                }
            }
        }

        private void ApplyAnimation(float time)
        {
            DatasetSetup.anim.SampleAnimation(DatasetSetup.character, time);
        }

        #endregion
    }

    public enum AnimationFilter
    {
        All = 0,
        Motions = 1,
        Styles = 2,
        Untagged = 3
    }
}
