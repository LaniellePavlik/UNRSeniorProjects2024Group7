using System;
using System.Collections.Generic;
using System.Linq;
using QuanticBrains.MotionMatching.Scripts.Tags;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QuanticBrains.MotionMatching.Scripts.Editor.DatasetConfiguration.InnerWindow.Implementation
{
    [Serializable]
    public class AnimationsInnerWindow : InnerWindowBase
    {
        #region Variables

        private ToggleGrid _motionGrid;
        private ToggleGrid _styleGrid;

        //Positions
        private const float OneColumnSpace = ColumnWidth * 2 + ColumnSpace;
        private const float AnimTextLength = ColumnWidth;
        private const float LabelsDisplacement = 7 * ColumnSpace + AnimTextLength;

        //Labels properties
        private readonly string _warningSign = "\u26A0";
        private readonly string[] _labels = { "Walk", "Run", "Strafe", "Style" };
        private readonly int[] _labelSize = { 35, 27, 43, 60 };

        //Styles
        private readonly GUIStyle _taggedStyle;
        private readonly GUIStyle _regularStyle;

        //Style tag name add and remove
        private string _newStyleName = "";
        private int _selectedForRemove;

        #endregion

        #region BaseMethods

        public AnimationsInnerWindow(string name, DatasetSetup datasetSetup) : base(name, datasetSetup)
        {
            List<string> motionOptions = DatasetSetup.motions.Select(x => x.name).ToList();
            List<string> styleOptions = DatasetSetup.styles.Select(x => x.name).ToList();

            _motionGrid = new ToggleGrid(motionOptions, 5, BoxWidth / 5.0f - ColumnSpace, 30);
            _styleGrid = new ToggleGrid(styleOptions, 5, BoxWidth / 5.0f - ColumnSpace, 30);

            //Styles
            _taggedStyle = new GUIStyle();
            _taggedStyle.normal.textColor = Color.white;
            _taggedStyle.fontStyle = FontStyle.Bold;

            _regularStyle = new GUIStyle();
            _regularStyle.normal.textColor = Color.gray;
            _regularStyle.fontStyle = FontStyle.Normal;
        }

        public void InitializeStyleGrid()
        {
            List<string> styleOptions = DatasetSetup.styles.Select(x => x.name).ToList();
            _styleGrid = new ToggleGrid(styleOptions, 5, BoxWidth / 5.0f - ColumnSpace, 30);
        }

        public override void OnDrawWindow()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(OneColumnSpace)); //Column

            DrawTagsSelector();
            DrawAnimationList();

            EditorGUILayout.EndVertical(); //End of Column
        }

        #endregion

        #region DrawMethods

        private void DrawTagsSelector()
        {
            EditorGUILayout.LabelField("Predefined Tags Setup", EditorStyles.boldLabel, GUILayout.Width(ColumnWidth));
            EditorGUILayout.Space();
            DrawMotionTagsSelector();
            DrawStyleTagsSelector();
        }

        private void DrawMotionTagsSelector()
        {
            EditorGUILayout.LabelField("Motion Tags", EditorStyles.boldLabel, GUILayout.Width(ColumnWidth));
            _motionGrid.DrawToggleGrid();

            EditorGUILayout.Space();
        }

        private void DrawStyleTagsSelector()
        {
            EditorGUILayout.LabelField("Custom Style Tags", EditorStyles.boldLabel, GUILayout.Width(ColumnWidth));

            if (DatasetSetup.styles.Count == 0)
            {
                EditorGUILayout.LabelField("There are no custom style tags defined", GUILayout.Width(ColumnWidth));
                ManageStyleTags();
                return;
            }

            EditorGUILayout.Space();
            _styleGrid.DrawToggleGrid();

            ManageStyleTags();
        }

        private void ManageStyleTags()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal(GUILayout.Width(OneColumnSpace));

            EditorGUILayout.BeginVertical(GUILayout.Width(ColumnWidth));
            AddStyleButton();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(ColumnWidth));
            RemoveStyleButton();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void AddStyleButton()
        {
            EditorGUILayout.LabelField("Add New Style Tag", EditorStyles.boldLabel, GUILayout.Width(ColumnWidth));
            EditorGUILayout.BeginHorizontal(GUILayout.Width(ColumnWidth));
            _newStyleName = EditorGUILayout.TextField(_newStyleName, GUILayout.Width(ColumnWidth * 0.5f));
            if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
            {
                AddStyleTag(_newStyleName);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void RemoveStyleButton()
        {
            EditorGUILayout.LabelField("Remove Style Tag", EditorStyles.boldLabel, GUILayout.Width(ColumnWidth));
            EditorGUILayout.BeginHorizontal(GUILayout.Width(ColumnWidth));

            _selectedForRemove = EditorGUILayout.Popup(_selectedForRemove, DatasetSetup.styleTags.ToArray(),
                GUILayout.Width(ColumnWidth * 0.5f));
            if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20)))
            {
                RemoveStyleTag(_selectedForRemove);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void AddStyleTag(string tagName)
        {
            if (tagName.Equals("")
                || char.IsDigit(tagName[0])
                || tagName.Any(char.IsWhiteSpace)
                || DatasetSetup.styles.Find(s => s.name.Equals(tagName)) != null)
            {
                EditorUtility.DisplayDialog("Error in the interval naming convention",
                    "The style tag name cannot be empty, start by a digit, have white spaces or be repeated.",
                    "Ok");
                return;
            }

            var name = char.ToUpper(tagName[0]) + tagName[1..];
            DatasetSetup.styles.Add(new TagBase(name));
            DatasetSetup.styleTags.Add(name);

            DatasetSetup.UpdateStyleTagsDependencies();
            _newStyleName = "";
        }

        private void RemoveStyleTag(int selectedTag)
        {
            if (DatasetSetup.styles.Count == 0) return;
            DatasetSetup.styles.RemoveAt(selectedTag);
            DatasetSetup.styleTags.RemoveAt(selectedTag);

            DatasetSetup.UpdateStyleTagsDependencies();
            _selectedForRemove = 0;
        }

        private void DrawAnimationList()
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Animation Files", EditorStyles.boldLabel, GUILayout.Width(ColumnWidth));

            DropAreaGUI();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("Box", GUILayout.Width(OneColumnSpace));
            DatasetSetup.AnimFileList.DoLayoutList();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        private void DropAreaGUI()
        {
            var evt = Event.current;

            GUILayout.Box("\n\nDrag and drop animations here",
                GUILayout.Height(100), GUILayout.Width(BoxWidth - ColumnSpace));

            switch (evt.type)
            {
                case EventType.DragUpdated:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    break;
                case EventType.DragPerform:
                    if (!GUILayoutUtility.GetLastRect().Contains(evt.mousePosition))
                        return;

                    DragAndDrop.AcceptDrag();

                    foreach (string path in DragAndDrop.paths)
                    {
                        Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);
                        bool animLoaded = false;
                        foreach (Object obj in objects)
                        {
                            AnimationClip anim = obj as AnimationClip;
                            if (anim == null) continue;
                            if (anim.name.StartsWith("__preview__")) continue;

                            var settings = AnimationUtility.GetAnimationClipSettings(anim);

                            if (DatasetSetup.animations.Any(storedAnim => storedAnim.name == anim.name))
                            {
                                EditorUtility.DisplayDialog("Warning while loading animation",
                                    "Animation named '" + anim.name +
                                    "' already present in the list, duplicate not added.",
                                    "Ok");
                                continue;
                            }

                            if (settings.loopBlendOrientation)
                            {
                                EditorUtility.DisplayDialog("Warning while loading animation",
                                    "Root orientation is constant for animation '" + anim.name +
                                    "'. Make sure that this animation only walks/runs straight, otherwise this will cause errors.",
                                    "Ok");
                            }

                            DatasetSetup.animations.Add(anim);
                            DatasetSetup.animationsHeight.Add(EditorGUIUtility.singleLineHeight);
                            DatasetSetup.animationsFiltered.Add(true);

                            //Add ranges to added anim
                            AddPredefinedRanges(anim);
                            animLoaded = true;
                        }

                        if (animLoaded) continue;
                        EditorUtility.DisplayDialog("Error while loading animation",
                            "Problem loading asset at: " + path, "Ok");
                    }

                    break;
            }
        }

        private void AddPredefinedRanges(AnimationClip anim)
        {
            var totalFrames = anim.length * anim.frameRate - 1;
            var motionTags = _motionGrid.PressedOptions;
            for (int i = 0; i < motionTags.Length; i++)
            {
                if (!motionTags[i]) continue;
                DatasetSetup.AddInterval(anim.name, 0, (int)totalFrames, totalFrames, i);
            }

            var styleTags = _styleGrid.PressedOptions;
            for (int i = 0; i < styleTags.Length; i++)
            {
                if (!styleTags[i]) continue;
                DatasetSetup.AddInterval(anim.name, 0, (int)totalFrames, totalFrames, i + motionTags.Length);
            }
        }

        #endregion

        #region ClearData

        public override void ClearData(DataManagementEnum type)
        {
            if (type == DataManagementEnum.Initial)
            {
                DatasetSetup.AnimFileList = new ReorderableList(DatasetSetup.animations, typeof(List<AnimationClip>),
                    false, false, false, true)
                {
                    drawElementCallback = DrawAnimationsItems,
                    onChangedCallback = ManageAnimationsChanged,
                    onRemoveCallback = ManageRemoveAnimation
                };
            }
        }

        #endregion

        #region ReorderableListMethods

        private void DrawAnimationsItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            bool[] isTagged = DatasetSetup.CheckIfTaggedByMotionOrStyles(DatasetSetup.animations[index].name);

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

            EditorGUILayout.EndHorizontal();
        }

        private void ManageAnimationsChanged(ReorderableList list)
        {
            DatasetSetup.selectedAnimation = 0;
        }

        private void ManageRemoveAnimation(ReorderableList list)
        {
            foreach (var index in list.selectedIndices)
            {
                DatasetSetup.motions.ForEach(motion =>
                    motion.ranges.RemoveAll(range => range.animName.Equals(DatasetSetup.animations[index].name)));
                DatasetSetup.styles.ForEach(style =>
                    style.ranges.RemoveAll(range => range.animName.Equals(DatasetSetup.animations[index].name)));
                DatasetSetup.animations.RemoveAt(index);

                DatasetSetup.UpdateAnimationDependenciesForTags(index);
            }
        }

        #endregion
    }
}

