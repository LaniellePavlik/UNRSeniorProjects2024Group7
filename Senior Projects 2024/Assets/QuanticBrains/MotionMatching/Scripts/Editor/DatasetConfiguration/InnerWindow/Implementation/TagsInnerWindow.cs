using System;
using System.Collections.Generic;
using System.Linq;
using QuanticBrains.MotionMatching.Scripts.Containers;
using QuanticBrains.MotionMatching.Scripts.Tags;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Editor.DatasetConfiguration.InnerWindow.Implementation
{
    [Serializable]
    public class TagsInnerWindow : InnerWindowBase
    {
        #region BaseMethods

        public TagsInnerWindow(string name, DatasetSetup datasetSetup) : base(name, datasetSetup)
        {
        }

        public override void OnDrawWindow()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(ColumnWidth)); //Column 1  

            if (!DrawAndCheckIfVisible())
            {
                EditorGUILayout.EndVertical(); //End of Column 1
                return;
            }

            DrawActionTags();

            EditorGUILayout.EndVertical(); //End of Column 1
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(GUILayout.Width(ColumnWidth)); //Column 2  

            DrawIdleTags();

            EditorGUILayout.EndVertical(); //End of Column 2
        }

        #endregion

        #region DrawMethods

        private bool DrawAndCheckIfVisible()
        {
            if (!DatasetSetup.character)
            {
                EditorGUILayout.LabelField("Select a character to use this window", EditorStyles.boldLabel,
                    GUILayout.Width(ColumnWidth));
                EditorGUILayout.LabelField("Configuration > Character Gameobject", GUILayout.Width(ColumnWidth));
                return false;
            }

            if (!DatasetSetup.character.GetComponent<Animator>())
            {
                EditorGUILayout.LabelField("Your character need an Animator Controller", EditorStyles.boldLabel,
                    GUILayout.Width(ColumnWidth));
                EditorGUILayout.LabelField("Add it from editor > Animator Controller", GUILayout.Width(ColumnWidth));
                return false;
            }

            if (DatasetSetup.animations.Count < 1)
            {
                EditorGUILayout.LabelField("Add animations to your dataset to use this panel", EditorStyles.boldLabel,
                    GUILayout.Width(ColumnWidth));
                EditorGUILayout.LabelField("Animations > Drop Area", GUILayout.Width(ColumnWidth));
                return false;
            }

            return true;
        }

        private void DrawActionTags()
        {
            EditorGUILayout.LabelField("Action Tags", EditorStyles.boldLabel, GUILayout.Width(ColumnWidth));

            if (DatasetSetup.actionTagsFoldout)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical("box", GUILayout.Width(ColumnWidth));
                DatasetSetup.DefaultActionTags.DoLayoutList();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawIdleTags()
        {
            EditorGUILayout.LabelField("Idle Tags", EditorStyles.boldLabel, GUILayout.Width(ColumnWidth));

            if (DatasetSetup.idleTagFoldout)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical("box", GUILayout.Width(ColumnWidth));
                DatasetSetup.TransitionToIdleFiles.DoLayoutList();

                EditorGUILayout.Space();
                DatasetSetup.IdleFiles.DoLayoutList();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }

        #endregion

        #region ClearData

        public override void ClearData(DataManagementEnum type)
        {
            if (type == DataManagementEnum.Initial)
            {
                //Action tags list
                DatasetSetup.DefaultActionTags = new ReorderableList(DatasetSetup.actionTags,
                    typeof(List<ActionTagsData>), true, true, true, true)
                {
                    drawHeaderCallback = DrawActionsHeader,
                    drawElementCallback = DrawActionItems,
                    elementHeightCallback = ActionsElementHeight,
                    onChangedCallback = ManageChangedActions
                };

                //Idle files
                DatasetSetup.TransitionToIdleFiles = new ReorderableList(DatasetSetup.transitionAnims,
                    typeof(List<AnimFileData>), true, true, true, true)
                {
                    drawHeaderCallback = DrawTransitionHeader,
                    drawElementCallback = DrawTransitionItems,
                    elementHeightCallback = TransitionElementHeight,
                    onChangedCallback = ManageChangedTransition
                };

                DatasetSetup.IdleFiles = new ReorderableList(DatasetSetup.idleAnims, typeof(List<AnimFileData>), true,
                    true, true, true)
                {
                    drawHeaderCallback = DrawIdleHeader,
                    drawElementCallback = DrawIdleItems,
                    elementHeightCallback = TransitionIdleHeight,
                    onChangedCallback = ManageChangedIdle
                };
            }
        }

        #endregion

        #region ReorderableListMethods

        private void DrawActionsHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Action Tags");
        }

        private void DrawTransitionHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Transitions to Idle");
        }

        private void DrawIdleHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Idle Animations");
        }

        private void DrawActionItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            float height = EditorGUIUtility.singleLineHeight;

            DatasetSetup.actionsFoldout[index] = EditorGUI.Foldout(
                new Rect(rect.x, rect.y, 230, EditorGUIUtility.singleLineHeight), DatasetSetup.actionsFoldout[index],
                new GUIContent(
                    DatasetSetup.actionTags[index].actionName == ""
                        ? "New Action"
                        : DatasetSetup.actionTags[index].actionName, "Animation name"), true);

            DatasetSetup.actionsSelected[index] = isFocused;
            if (!DatasetSetup.actionsFoldout[index])
            {
                DatasetSetup.actionsHeight[index] = height;
                return;
            }

            int expandedLines = 0;

            var animList = DatasetSetup.animations.Select(anim => anim.name).ToArray();
            var noneAnimList = new[] { "None" }.Concat(animList).ToArray();

            DatasetSetup.actionTags[index].actionName = EditorGUI.DelayedTextField(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                DatasetSetup.actionTags[index].actionName);

            DatasetSetup.actionTags[index].isLoopable = EditorGUI.Toggle(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Is Loopable action?", DatasetSetup.actionTags[index].isLoopable);

            expandedLines++;
            EditorGUI.LabelField(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "States setup", EditorStyles.boldLabel);

            DatasetSetup.actionTags[index].actionAnimationID = EditorGUI.Popup(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Action Animation", DatasetSetup.actionTags[index].actionAnimationID, animList);

            DatasetSetup.actionTags[index].initAnimationID = EditorGUI.Popup(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Init Animation", DatasetSetup.actionTags[index].initAnimationID, noneAnimList);

            DatasetSetup.actionTags[index].recoveryAnimationID = EditorGUI.Popup(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Recovery Animation", DatasetSetup.actionTags[index].recoveryAnimationID, noneAnimList);

            expandedLines++;

            EditorGUI.LabelField(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Interruptible states setup", EditorStyles.boldLabel);

            DatasetSetup.actionTags[index].isInitInterruptible = EditorGUI.Toggle(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Init Interruptible?", DatasetSetup.actionTags[index].isInitInterruptible);

            DatasetSetup.actionTags[index].isInProgressInterruptible = EditorGUI.Toggle(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "InProgress Interruptible?", DatasetSetup.actionTags[index].isInProgressInterruptible);

            DatasetSetup.actionTags[index].isRecoveryInterruptible = EditorGUI.Toggle(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Recovery Interruptible?", DatasetSetup.actionTags[index].isRecoveryInterruptible);

            expandedLines++;

            EditorGUI.LabelField(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Interruptible by setup", EditorStyles.boldLabel);

            DatasetSetup.actionTags[index].interruptibleType = (InterruptibleBy)EditorGUI.Popup(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Action is Interruptible by: ", (int)DatasetSetup.actionTags[index].interruptibleType,
                Enum.GetNames(typeof(InterruptibleBy)));

            DrawNameListByAction(index, rect, height, ref expandedLines);

            if (DatasetSetup.actionTags[index].isLoopable)
            {
                EditorGUI.LabelField(
                    new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                    "Movement simulation", EditorStyles.boldLabel);

                DatasetSetup.actionTags[index].isSimulated = EditorGUI.Toggle(
                    new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                    new GUIContent("Simulate Root Motion?",
                        "Enable this will move the agent during InProgress state by a combination of the animation's root motion plus its current velocity"),
                    DatasetSetup.actionTags[index].isSimulated);
            }

            expandedLines++;

            EditorGUI.LabelField(
                new Rect(rect.x, rect.y + height * ++expandedLines, 75, EditorGUIUtility.singleLineHeight),
                "Action Warping", EditorStyles.boldLabel);

            if (!DatasetSetup.actionTags[index].isLoopable)
            {
                EditorGUI.LabelField(
                    new Rect(rect.x + 75, rect.y + height * expandedLines, 120, EditorGUIUtility.singleLineHeight),
                    new GUIContent("- InProgress State",
                        "Non loopable actions warping is applied to match the end of InProgress state"),
                    EditorStyles.label);
            }
            else
            {
                EditorGUI.LabelField(
                    new Rect(rect.x + 75, rect.y + height * expandedLines, 120, EditorGUIUtility.singleLineHeight),
                    new GUIContent("- Init State",
                        "Loopable actions warping is applied to match the end of Init state, as InProgress will be looping"),
                    EditorStyles.label);
            }

            if (DatasetSetup.actionTags[index].isLoopable && !DatasetSetup.actionTags[index].HasInitState())
            {
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontStyle = FontStyle.Italic;

                EditorGUI.LabelField(
                    new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight * 2),
                    "An animation must be selected in the \nInit state to use this section.", labelStyle);

                expandedLines++;
                DatasetSetup.actionsHeight[index] = height * (expandedLines + 2);
                return;
            }

            DatasetSetup.actionTags[index].warpingType = (WarpingType)EditorGUI.Popup(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Warping Type", (int)DatasetSetup.actionTags[index].warpingType, Enum.GetNames(typeof(WarpingType)));

            expandedLines++;

            EditorGUI.LabelField(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Position Warping", EditorStyles.boldLabel);

            DatasetSetup.actionTags[index].posWarpingMode = (WarpingMode)EditorGUI.Popup(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Position Warping Mode", (int)DatasetSetup.actionTags[index].posWarpingMode,
                Enum.GetNames(typeof(WarpingMode)));

            EditorGUI.LabelField(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Position Warping Weight");
            DatasetSetup.actionTags[index].positionWarpWeight = EditorGUI.Slider(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                DatasetSetup.actionTags[index].positionWarpWeight, 0, 1);

            expandedLines++;

            EditorGUI.LabelField(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Rotation Warping", EditorStyles.boldLabel);
            DatasetSetup.actionTags[index].rotWarpingMode = (WarpingMode)EditorGUI.Popup(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Rotation Warping Mode", (int)DatasetSetup.actionTags[index].rotWarpingMode,
                Enum.GetNames(typeof(WarpingMode)));

            EditorGUI.LabelField(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Rotation Warping Weight");
            DatasetSetup.actionTags[index].rotationWarpWeight = EditorGUI.Slider(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                DatasetSetup.actionTags[index].rotationWarpWeight, 0, 1);

            expandedLines++;

            EditorGUI.LabelField(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Warping Contacts", EditorStyles.boldLabel);

            DatasetSetup.actionTags[index].contactWarping = EditorGUI.Toggle(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Use Contacts for Warping?", DatasetSetup.actionTags[index].contactWarping);

            DrawContactBonesByAction(index, rect, height, ref expandedLines);

            expandedLines++;

            DatasetSetup.actionsHeight[index] = height * (expandedLines + 1);
        }

        private void DrawTransitionItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            float height = EditorGUIUtility.singleLineHeight;

            DatasetSetup.transitionFoldout[index] = EditorGUI.Foldout(
                new Rect(rect.x, rect.y, 230, EditorGUIUtility.singleLineHeight), DatasetSetup.transitionFoldout[index],
                new GUIContent(DatasetSetup.animations[DatasetSetup.transitionAnims[index].animationID].name, ""),
                true);

            DatasetSetup.transitionSelected[index] = isFocused;
            if (!DatasetSetup.transitionFoldout[index])
            {
                DatasetSetup.transitionHeight[index] = height;
                return;
            }

            int expandedLines = 0;
            var animList = DatasetSetup.animations.Select(anim => anim.name).ToArray();

            DatasetSetup.transitionAnims[index].animationID = EditorGUI.Popup(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Transition Animation", DatasetSetup.transitionAnims[index].animationID, animList);

            DatasetSetup.transitionHeight[index] = height * (expandedLines + 1);
        }

        private void DrawIdleItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            float height = EditorGUIUtility.singleLineHeight;

            DatasetSetup.idleFoldout[index] = EditorGUI.Foldout(
                new Rect(rect.x, rect.y, 230, EditorGUIUtility.singleLineHeight), DatasetSetup.idleFoldout[index],
                new GUIContent(DatasetSetup.animations[DatasetSetup.idleAnims[index].animationID].name, ""), true);

            DatasetSetup.idleSelected[index] = isFocused;
            if (!DatasetSetup.idleFoldout[index])
            {
                DatasetSetup.idleHeight[index] = height;
                return;
            }

            int expandedLines = 0;
            var animList = DatasetSetup.animations.Select(anim => anim.name).ToArray();

            DatasetSetup.idleAnims[index].animationID = EditorGUI.Popup(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Idle Animation", DatasetSetup.idleAnims[index].animationID, animList);

            DatasetSetup.idleHeight[index] = height * (expandedLines + 1);
        }

        private void ManageChangedActions(ReorderableList list)
        {
            if (DatasetSetup.actionTags.Count > DatasetSetup.actionsFoldout.Count)
            {
                DatasetSetup.actionsFoldout.Add(false);
                DatasetSetup.actionsSelected.Add(false);
                DatasetSetup.actionsHeight.Add(EditorGUIUtility.singleLineHeight);
                return;
            }

            int index = DatasetSetup.actionsSelected.FindIndex(cs => cs);
            if (index == -1)
            {
                index = DatasetSetup.actionsSelected.Count - 1;
            }

            DatasetSetup.actionsFoldout.RemoveAt(index);
            DatasetSetup.actionsSelected.RemoveAt(index);
            DatasetSetup.actionsHeight.RemoveAt(index);
        }

        private void ManageChangedTransition(ReorderableList list)
        {
            if (DatasetSetup.transitionAnims.Count > DatasetSetup.transitionFoldout.Count)
            {
                DatasetSetup.transitionFoldout.Add(false);
                DatasetSetup.transitionSelected.Add(false);
                DatasetSetup.transitionHeight.Add(EditorGUIUtility.singleLineHeight);
                return;
            }

            int index = DatasetSetup.transitionSelected.FindIndex(cs => cs);
            if (index == -1)
            {
                index = DatasetSetup.transitionSelected.Count - 1;
            }

            DatasetSetup.transitionFoldout.RemoveAt(index);
            DatasetSetup.transitionSelected.RemoveAt(index);
            DatasetSetup.transitionHeight.RemoveAt(index);
        }

        private void ManageChangedIdle(ReorderableList list)
        {
            if (DatasetSetup.idleAnims.Count > DatasetSetup.idleFoldout.Count)
            {
                DatasetSetup.idleFoldout.Add(false);
                DatasetSetup.idleSelected.Add(false);
                DatasetSetup.idleHeight.Add(EditorGUIUtility.singleLineHeight);
                return;
            }

            int index = DatasetSetup.idleSelected.FindIndex(cs => cs);
            if (index == -1)
            {
                index = DatasetSetup.idleSelected.Count - 1;
            }

            DatasetSetup.idleFoldout.RemoveAt(index);
            DatasetSetup.idleSelected.RemoveAt(index);
            DatasetSetup.idleHeight.RemoveAt(index);
        }

        private float ActionsElementHeight(int index)
        {
            return DatasetSetup.actionsHeight[index];
        }

        private float TransitionElementHeight(int index)
        {
            return DatasetSetup.transitionHeight[index];
        }

        private float TransitionIdleHeight(int index)
        {
            return DatasetSetup.idleHeight[index];
        }

        #endregion

        #region OtherMethods

        private void DrawNameListByAction(int index, Rect rect, float height, ref int expandedLines)
        {
            expandedLines++;

            if (DatasetSetup.actionTags[index].interruptibleType != InterruptibleBy.NameList) return;

            float baseWidth = 230;
            float buttonWidth = 50;

            EditorGUI.LabelField(
                new Rect(rect.x, rect.y + height * ++expandedLines, baseWidth, EditorGUIUtility.singleLineHeight),
                "Tags allowed to interrupt", EditorStyles.boldLabel);

            if (GUI.Button(new Rect(rect.x + baseWidth - buttonWidth, rect.y + height * expandedLines,
                    buttonWidth, EditorGUIUtility.singleLineHeight), "+"))
            {
                var styleName = "TagName" + DatasetSetup.actionTags[index].tagNamesList.Count;

                DatasetSetup.actionTags[index].tagNamesList.Add(styleName);
            }

            for (var i = 0; i < DatasetSetup.actionTags[index].tagNamesList.Count; i++)
            {
                if (GUI.Button(new Rect(rect.x + baseWidth - buttonWidth, rect.y + height * ++expandedLines,
                        buttonWidth, EditorGUIUtility.singleLineHeight), "-"))
                {
                    DatasetSetup.actionTags[index].tagNamesList.RemoveAt(i);
                    continue;
                }

                var currentName = EditorGUI.DelayedTextField(new Rect(rect.x, rect.y + height * expandedLines,
                        baseWidth - buttonWidth, EditorGUIUtility.singleLineHeight),
                    DatasetSetup.actionTags[index].tagNamesList[i]);

                if (currentName.Equals(DatasetSetup.actionTags[index].tagNamesList[i]))
                {
                    continue;
                }

                DatasetSetup.actionTags[index].tagNamesList[i] = char.ToUpper(currentName[0]) + currentName[1..];
            }
        }

        private void DrawContactBonesByAction(int index, Rect rect, float height, ref int expandedLines)
        {
            if (!DatasetSetup.actionTags[index].contactWarping) return;

            float baseWidth = 230;
            float buttonWidth = 50;

            EditorGUI.LabelField(
                new Rect(rect.x, rect.y + height * ++expandedLines, baseWidth, EditorGUIUtility.singleLineHeight),
                "Contact Warping Targets", EditorStyles.boldLabel);

            if (GUI.Button(new Rect(rect.x + baseWidth - buttonWidth, rect.y + height * expandedLines,
                    buttonWidth, EditorGUIUtility.singleLineHeight), "+"))
            {
                if (DatasetSetup.avatarBones.Count == 0)
                {
                    EditorUtility.DisplayDialog(
                        "Error", "In order to add warp Bones," +
                                 "\nyou must first add an avatar and configure" +
                                 "\nits AvatarBones (if Generic Avatar)." +
                                 "\nConfiguration > Avatar", "Ok");
                }
                else
                    DatasetSetup.actionTags[index].contactWarpBones.Add(0);
            }

            for (var i = 0; i < DatasetSetup.actionTags[index].contactWarpBones.Count; i++)
            {
                if (GUI.Button(new Rect(rect.x + baseWidth - buttonWidth, rect.y + height * ++expandedLines,
                        buttonWidth, EditorGUIUtility.singleLineHeight), "-"))
                {
                    DatasetSetup.actionTags[index].contactWarpBones.RemoveAt(i);
                    continue;
                }

                // var contactList = Enum.GetNames(typeof(HumanBodyBones)).ToList();
                // contactList.Remove(HumanBodyBones.LastBone.ToString());
                
                var contactList = DatasetSetup.avatarBones.Select(x => x.alias).ToArray();

                DatasetSetup.actionTags[index].contactWarpBones[i] = EditorGUI.Popup(   //It is stored as int, but later transformed to AvatarBone
                    new Rect(rect.x, rect.y + height * expandedLines,
                        baseWidth - buttonWidth, EditorGUIUtility.singleLineHeight),
                    DatasetSetup.actionTags[index].contactWarpBones[i], contactList.ToArray());
            }
        }

        #endregion
    }
}
