using System;
using System.Collections.Generic;
using System.Linq;
using QuanticBrains.MotionMatching.Scripts.Containers;
using QuanticBrains.MotionMatching.Scripts.Containers.CustomAvatars;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QuanticBrains.MotionMatching.Scripts.Editor.DatasetConfiguration.InnerWindow.Implementation
{
    public class ConfigurationInnerWindow : InnerWindowBase
    {
        private string[] _bonesOptions;
        private GUIStyle _labelStyle;

        #region BaseMethods

        public ConfigurationInnerWindow(string name, DatasetSetup datasetSetup) : base(name, datasetSetup)
        {
        }

        public override void OnDrawWindow()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(ColumnWidth)); //Column 1  

            DrawDatasetConfiguration();
            DrawCharacteristics();

            EditorGUILayout.EndVertical(); //End of Column 1
            EditorGUILayout.Space(ColumnSpace * 2);
            EditorGUILayout.BeginVertical(GUILayout.Width(ColumnWidth - ColumnSpace)); //Column 2  

            DrawModelPreview();

            DrawCustomAvatar();

            EditorGUILayout.EndVertical(); //End of Column 2
        }

        #endregion

        #region DrawMethods

        private void DrawDatasetConfiguration()
        {
            float height = EditorGUIUtility.singleLineHeight;

            EditorGUILayout.LabelField("Dataset to edit", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.fontStyle = FontStyle.Italic;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Dataset", GUILayout.Width(50));
            EditorGUILayout.LabelField("- Optional", _labelStyle, GUILayout.Width(100));

            EditorGUI.BeginChangeCheck();
            DatasetSetup.datasetToEdit = (Dataset)EditorGUILayout.ObjectField(
                DatasetSetup.datasetToEdit, typeof(Dataset), false, GUILayout.Width(150));

            if (EditorGUI.EndChangeCheck())
            {
                DatasetSetup.isDatasetLoaded = false;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(height);

            EditorGUILayout.LabelField(
                "Processing parameters", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DatasetSetup.poseStep = EditorGUILayout.Slider(
                "Pose step",
                DatasetSetup.poseStep,
                0.05f,
                0.5f
            );

            DatasetSetup.recordVelocity = EditorGUILayout.IntSlider(
                "Record velocity",
                DatasetSetup.recordVelocity,
                1,
                50
            );
            DatasetSetup.animationDatabaseName = EditorGUILayout.TextField(
                "Dataset name", DatasetSetup.animationDatabaseName);

            EditorGUILayout.Space(height);
            EditorGUILayout.LabelField(
                "Future estimates", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            DatasetSetup.futureEstimates = EditorGUILayout.IntSlider(
                "Number of points",
                DatasetSetup.futureEstimates,
                1,
                5
            );

            DatasetSetup.futureEstimatesTime = EditorGUILayout.Slider(
                "Total time",
                DatasetSetup.futureEstimatesTime,
                0.1f,
                2.5f);
            ClampFutureEstimates();

            EditorGUILayout.Space(height);
            EditorGUILayout.LabelField(
                "Past estimates", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DatasetSetup.pastEstimates = EditorGUILayout.IntSlider(
                "Number of points",
                DatasetSetup.pastEstimates,
                1,
                3
            );

            DatasetSetup.pastEstimatesTime = EditorGUILayout.Slider(
                "Total time",
                DatasetSetup.pastEstimatesTime,
                0.1f,
                1.5f);
            ClampPastEstimates();

            EditorGUILayout.Space(height);

            EditorGUILayout.LabelField(
                "Character parameters", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            var previousAvatar = DatasetSetup.avatar; 
            var previousCharacter = DatasetSetup.character; 
            var previousRoot = DatasetSetup.root; 
            
            DatasetSetup.character = (GameObject)EditorGUILayout.ObjectField(
                "Character GameObject", DatasetSetup.character, typeof(GameObject),
                true);

            EditorGUI.BeginDisabledGroup(DatasetSetup.disableRootAndAvatar);
            DatasetSetup.root = (Transform)EditorGUILayout.ObjectField(
                "Root", DatasetSetup.root, typeof(Transform), true);

            DatasetSetup.avatar = (Avatar)EditorGUILayout.ObjectField(
                "Avatar", DatasetSetup.avatar, typeof(Avatar), true);

            if (EditorGUI.EndChangeCheck())
            {
                if (DatasetSetup.ConfirmChanges(previousAvatar))
                {
                    DatasetSetup.InitializeCharacter();
                    DatasetSetup.InitializeCustomAvatar(previousAvatar);   
                }
                else
                {
                    //If cancelled, then replace with previous
                    DatasetSetup.character = previousCharacter;
                    DatasetSetup.root = previousRoot;
                    DatasetSetup.avatar = previousAvatar;
                }
            }

            EditorGUI.EndDisabledGroup();

            if (DatasetSetup.datasetToEdit != null && !DatasetSetup.isDatasetLoaded)
            {
                var dataset = DatasetSetup.datasetToEdit;
                var tempChar = DatasetSetup.character;

                //DatasetSetup.ClearData(DataManagementEnum.Complete);

                DatasetSetup.character = tempChar;
                DatasetSetup.datasetToEdit = dataset;

                DatasetSetup.TryRecoverStyleTags();

                DatasetSetup.isDatasetLoaded = true;
                DatasetSetup.poseStep = DatasetSetup.datasetToEdit.poseStep;
                DatasetSetup.recordVelocity = DatasetSetup.datasetToEdit.recordVelocity;
                DatasetSetup.animationDatabaseName = DatasetSetup.datasetToEdit.name;
                DatasetSetup.futureEstimates = DatasetSetup.datasetToEdit.futureEstimates;
                DatasetSetup.futureEstimatesTime = DatasetSetup.datasetToEdit.futureEstimatesTime;
                DatasetSetup.pastEstimates = DatasetSetup.datasetToEdit.pastEstimates;
                DatasetSetup.pastEstimatesTime = DatasetSetup.datasetToEdit.pastEstimatesTime;

                DatasetSetup.ResetAnimations();
                var animationsNotLoaded = new List<string>();
                foreach (var completePath in DatasetSetup.datasetToEdit.animationPaths)
                {
                    bool isFBX = false;
                    string[] animName = new[] { completePath, "" }; //The object's path + its anim name

                    if (completePath.Contains(".fbx//"))
                    {
                        animName = completePath.Split("//");
                        isFBX = true;
                    }

                    Object[] objects = AssetDatabase.LoadAllAssetsAtPath(animName[0]);

                    bool loaded = false;
                    foreach (Object obj in objects)
                    {
                        var anim = obj as AnimationClip;

                        if (anim == null) continue;
                        if (anim.name.StartsWith("__preview__")) continue;
                        if (isFBX && anim.name != animName[1]) continue;
                        if (DatasetSetup.animations.Any(storedAnim => storedAnim.name == anim.name)) continue;

                        DatasetSetup.animations.Add(anim);
                        DatasetSetup.animationsHeight.Add(EditorGUIUtility.singleLineHeight);
                        DatasetSetup.animationsFiltered.Add(true);
                        loaded = true;
                    }

                    if (!loaded)
                        animationsNotLoaded.Add(completePath);
                }

                DatasetSetup.LookForPreviousTags(animationsNotLoaded);
                DatasetSetup.LookForPreviousActionTags();
                DatasetSetup.LookForPreviousIdleTag();
                
                bool filled = TryToFillAvatarWithNewDataset();
                
                //Only load characteristics if generic avatar has been loaded or is Human
                if (filled ||
                    (dataset.avatar && dataset.avatar.avatar.isHuman &&
                     DatasetSetup.avatar && DatasetSetup.avatar.isHuman))
                {
                    
                    DatasetSetup.LoadCharacteristics();
                }
                else if(DatasetSetup.avatar)
                {
                    var message = "Dataset type and Avatar mismatch." +
                                  "\nIgnoring Characteristics + Contact Bones.\nPlease check:"
                                  + "\nConfiguration > Default Characteristics"
                                  + "\nActions & Idling > Action Tags > Contact Warping";
                    EditorUtility.DisplayDialog("Restore warning", message, "Ok");
                }

                if (animationsNotLoaded.Count > 0)
                {
                    var result = animationsNotLoaded[0];
                    for (var i = 1; i < animationsNotLoaded.Count; i++)
                    {
                        result += ", " + animationsNotLoaded[i];
                    }

                    EditorUtility.DisplayDialog("Warning while loading animation",
                        "Found multiple entries of the animation(s) named '"
                        + result
                        + "'.",
                        "Ok");

                    foreach (var anim in animationsNotLoaded)
                    {
                        int id = DatasetSetup.datasetToEdit.animationPaths.IndexOf(anim);

                        //Remove from paths and data - also check references for action tags
                        DatasetSetup.datasetToEdit.animationPaths.RemoveAt(id);
                        DatasetSetup.datasetToEdit.animationsData.RemoveAt(id);
                        DatasetSetup.ReviewActionTags(id, ref DatasetSetup.actionTags);
                    }
                }

                var erasedAnims = new List<string>();

                foreach (var (recordedAnim, index) in DatasetSetup.datasetToEdit.animationsData.Select(
                             (recordedAnim, index) =>
                                 (recordedAnim, index)))
                {
                    var basePath = DatasetSetup.datasetToEdit.animationPaths[index];
                    string[] animPath = new[] { basePath, "" };

                    if (animPath[0].Contains(".fbx//"))
                    {
                        animPath = basePath.Split("//");
                    }

                    if (DatasetSetup.animations.Count > index)
                    {
                        var anim = DatasetSetup.animations[index];
                        var recordedFPS = 1 / Math.Round(DatasetSetup.poseStep, 2);
                        var recordedProportion = anim.frameRate / recordedFPS;
                        var recordedLength = recordedProportion * recordedAnim.Count;
                        var animLength = anim.frameRate * anim.length;

                        if (!(Math.Abs(recordedLength - animLength) > 5))
                        {
                            continue;
                        }

                        //If animation has different length
                    }

                    //If animation is out of bounds
                    DatasetSetup.ReviewTags(animPath[1],
                        ref DatasetSetup.motions);
                    DatasetSetup.ReviewTags(animPath[1], ref DatasetSetup.styles);

                    erasedAnims.Add(basePath);
                }

                if (erasedAnims.Count > 0)
                {
                    EditorUtility.DisplayDialog("Warning while loading dataset", "The animation file(s) "
                        + erasedAnims.Aggregate("", (current, animName) => current + (animName + ", "))
                        + " has/have a different length than previously recorded. Its tags were erased.", "Ok");
                }
            }
        }

        private bool TryToFillAvatarWithNewDataset()
        {
            //if (!DatasetSetup.avatar || DatasetSetup.avatar && DatasetSetup.avatar == dataset.avatar.avatar){...}
            //Fill bones only if datasetAvatar is not human + its the same as the selected (if selected)
            var dataset = DatasetSetup.datasetToEdit;
            if (!DatasetSetup.avatar) return false;
            if (!dataset.avatar || dataset.avatar.avatar.isHuman) return false;
            if (DatasetSetup.avatar && DatasetSetup.avatar != dataset.avatar.avatar) return false;

            if (!EditorUtility.DisplayDialog("Avatar selection", 
                    "Fill custom avatar with the dataset avatar data?",
                    "Yes", "No, keep my changes")) return false;
            
            DatasetSetup.FillAvatarBonesByDataset(true);
            DatasetSetup.InitChildrenFromDataset();

            return true;
        }

        private void DrawCharacteristics()
        {
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

            EditorGUILayout.LabelField(
                "Default Characteristics", EditorStyles.boldLabel);

            if (!DatasetSetup.character || !DatasetSetup.avatar)
            {
                EditorGUILayout.LabelField("Select a character and avatar first.", EditorStyles.boldLabel,
                    GUILayout.Width(ColumnWidth));
                EditorGUILayout.LabelField("Configuration > Character Parameters", GUILayout.Width(ColumnWidth));
                return;
            }

            _bonesOptions = DatasetSetup.avatarBones.Where(x => x.id != -1).Select(x => x.alias).ToArray();
            
            if (DatasetSetup.avatarBones.Count == 0)
            {
                EditorGUILayout.LabelField("Fill Avatar Bones first.", EditorStyles.boldLabel,
                    GUILayout.Width(ColumnWidth));
                EditorGUILayout.LabelField("Configuration > Custom Generic Avatar", GUILayout.Width(ColumnWidth));
                return;
            }
            
            //Draw characteristics list
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical(GUILayout.Width(ColumnWidth + ColumnSpace));
            DatasetSetup.DefaultCharacteristics.DoLayoutList();
            EditorGUILayout.EndVertical();
        }

        private void DrawModelPreview()
        {
            float height = EditorGUIUtility.singleLineHeight;

            if (!DatasetSetup.character)
            {
                EditorGUILayout.LabelField("Selected character: None", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("");
                return;
            }

            Texture2D texture = AssetPreview.GetAssetPreview(DatasetSetup.character);
            EditorGUILayout.LabelField("Selected character:", EditorStyles.boldLabel);

            EditorGUILayout.LabelField(DatasetSetup.character.name);
            if (texture)
            {
                Rect textureRect = GUILayoutUtility.GetLastRect();
                EditorGUI.DrawPreviewTexture(new Rect(textureRect.x, 2f * height + textureRect.y, ColumnWidth * 0.75f,
                    ColumnWidth * 0.75f), texture);
            }
            else
            {
                EditorGUILayout.LabelField("Unable to get character preview");
            }
        }

        private void DrawCustomAvatar()
        {
            if (!DatasetSetup.datasetToEdit && !DatasetSetup.avatar) return;
            if (!DatasetSetup.avatar &&
                DatasetSetup.datasetToEdit && 
                DatasetSetup.datasetToEdit.avatar && 
                DatasetSetup.datasetToEdit.avatar.avatar.isHuman) return;
            if (DatasetSetup.avatar && DatasetSetup.avatar.isHuman) return;

            EditorGUILayout.Space(310);

            EditorGUILayout.LabelField(
                "Custom Generic Avatar", EditorStyles.boldLabel);

            if (!DatasetSetup.character)
            {
                EditorGUILayout.LabelField("- Add Character GameObject first", _labelStyle, GUILayout.Width(190));
                return;
            }

            if (!DatasetSetup.avatar)
            {
                EditorGUILayout.LabelField("- Add Avatar first", _labelStyle, GUILayout.Width(190));
                return;
            }

            EditorGUILayout.BeginHorizontal(GUILayout.Width(ColumnWidth));
            if (GUILayout.Button(
                    "Auto-Fill",
                    GUILayout.Height(EditorGUIUtility.singleLineHeight),
                    GUILayout.Width(80)))
            {
                DatasetSetup.FillAvatarBonesByCharacter();
            }

            if (GUILayout.Button(
                    "Dataset Fill",
                    GUILayout.Height(EditorGUIUtility.singleLineHeight),
                    GUILayout.Width(80)))
            {
                DatasetSetup.FillAvatarBonesByDataset(true);
            }

            if (GUILayout.Button(
                    "Remove All",
                    GUILayout.Height(EditorGUIUtility.singleLineHeight),
                    GUILayout.Width(80)))
            {
                DatasetSetup.RemoveAllAvatarBonesWithWarning();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Width(ColumnWidth + ColumnSpace));
            EditorGUILayout.LabelField("Root Bone", GUILayout.Width(60));

            if (_bonesOptions != null && _bonesOptions.Length != 0)
            {
                DatasetSetup.rootBone = EditorGUILayout.Popup(DatasetSetup.rootBone,
                    _bonesOptions, GUILayout.Width(190));
            }
            else
            {
                EditorGUILayout.LabelField("- Add Avatar Bones first", _labelStyle, GUILayout.Width(190));
            }

            EditorGUILayout.EndHorizontal();

            //Draw characteristics list
            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(GUILayout.Width(ColumnWidth - 2 * ColumnSpace));
            DatasetSetup.GenericAvatarBones.DoLayoutList();
            EditorGUILayout.EndVertical();
        }

        private void ClampFutureEstimates()
        {
            if ((DatasetSetup.futureEstimatesTime / DatasetSetup.poseStep) / DatasetSetup.futureEstimates < 1)
            {
                DatasetSetup.futureEstimatesTime = DatasetSetup.futureEstimates * DatasetSetup.poseStep;
            }
        }

        private void ClampPastEstimates()
        {
            if ((DatasetSetup.pastEstimatesTime / DatasetSetup.poseStep) / DatasetSetup.pastEstimates < 1)
            {
                DatasetSetup.pastEstimatesTime = DatasetSetup.pastEstimates * DatasetSetup.poseStep;
            }
        }

        #endregion

        #region ClearData

        public override void ClearData(DataManagementEnum type)
        {
            if (type == DataManagementEnum.Initial)
            {
                DatasetSetup.DefaultCharacteristics =
                    new ReorderableList(DatasetSetup.characteristics, typeof(List<BoneCharacteristic>), true, true,
                        true, true)
                    {
                        drawHeaderCallback = DrawCharacteristicsHeader,
                        drawElementCallback = DrawCharacteristicsItems,
                        elementHeightCallback = CharacteristicElementHeight,
                        onChangedCallback = ManageChangedCharacteristics,
                        onAddCallback = ManageAddCharacteristics,
                        onRemoveCallback = ManageRemoveCharacteristics
                    };

                DatasetSetup.GenericAvatarBones =
                    new ReorderableList(DatasetSetup.avatarBones, typeof(List<BoneCharacteristic>), true, true,
                        true, true)
                    {
                        drawHeaderCallback = DrawAvatarBonesHeader,
                        drawElementCallback = DrawAvatarBonesItems,
                        elementHeightCallback = AvatarBonesElementHeight,
                        onChangedCallback = ManageChangedAvatarBones,
                        onAddCallback = ManageAddAvatarBones,
                        onRemoveCallback = ManageRemoveAvatarBones
                    };
            }
        }

        #endregion

        #region CharacteristicsReorderableMethods

        private void DrawCharacteristicsHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Characteristics List");
        }

        private void DrawCharacteristicsItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            float height = EditorGUIUtility.singleLineHeight;

            DatasetSetup.characteristicsFoldout[index] = EditorGUI.Foldout(
                new Rect(rect.x, rect.y, 230, EditorGUIUtility.singleLineHeight),
                DatasetSetup.characteristicsFoldout[index],
                new GUIContent("Human Bone -> " + DatasetSetup.characteristics[index].bone.alias,
                    "Spring Settings"), true);
            DatasetSetup.characteristicsSelected[index] = isFocused;
            if (!DatasetSetup.characteristicsFoldout[index])
            {
                DatasetSetup.characteristicsHeight[index] = height;
                return;
            }

            int expandedLines = 0;
            // DatasetSetup.characteristics[index].bone = EditorGUI.EnumPopup(
            //     new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
            //     DatasetSetup.characteristics[index].bone
            // );

            DatasetSetup.characteristicsBonesIndex[index] = EditorGUI.Popup(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                DatasetSetup.characteristicsBonesIndex[index], _bonesOptions);

            DatasetSetup.characteristics[index].bone =
                DatasetSetup.avatarBones[DatasetSetup.characteristicsBonesIndex[index]];


            EditorGUI.LabelField(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Weight Position");
            DatasetSetup.characteristics[index].weightPosition = EditorGUI.Slider(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                DatasetSetup.characteristics[index].weightPosition,
                0,
                1
            );
            EditorGUI.LabelField(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                "Weight Velocity");
            DatasetSetup.characteristics[index].weightVelocity = EditorGUI.Slider(
                new Rect(rect.x, rect.y + height * ++expandedLines, 230, EditorGUIUtility.singleLineHeight),
                DatasetSetup.characteristics[index].weightVelocity,
                0,
                1);
            DatasetSetup.characteristicsHeight[index] = height * (expandedLines + 1);
        }

        private float CharacteristicElementHeight(int index)
        {
            return DatasetSetup.characteristicsHeight[index];
        }

        private void ManageChangedCharacteristics(ReorderableList list)
        {
            if (DatasetSetup.characteristics.Count <= DatasetSetup.characteristicsFoldout.Count) return;

            DatasetSetup.characteristicsFoldout.Add(false);
            DatasetSetup.characteristicsSelected.Add(false);
            DatasetSetup.characteristicsBonesIndex.Add(0);
            DatasetSetup.characteristicsHeight.Add(EditorGUIUtility.singleLineHeight);
        }

        private void ManageAddCharacteristics(ReorderableList list)
        {
            if (DatasetSetup.avatarBones.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Error", "In order to add characteristics," +
                             "\nyou must first add an avatar and configure" +
                             "\nits AvatarBones (if Generic Avatar).", "Ok");
                return;
            }
            
            DatasetSetup.characteristics.Add(new BoneCharacteristic()
            {
                bone = DatasetSetup.avatarBones[0],
                weightPosition = 1.0f,
                weightVelocity = 1.0f
            });
        }

        private void ManageRemoveCharacteristics(ReorderableList list)
        {
            int index = DatasetSetup.characteristicsSelected.FindIndex(cs => cs);
            if (index == -1)
            {
                index = DatasetSetup.characteristicsSelected.Count - 1;
            }

            DatasetSetup.characteristics.RemoveAt(index);
            DatasetSetup.characteristicsFoldout.RemoveAt(index);
            DatasetSetup.characteristicsHeight.RemoveAt(index);
            DatasetSetup.characteristicsSelected.RemoveAt(index);
            DatasetSetup.characteristicsBonesIndex.RemoveAt(index);
        }


        // - Custom Avatar Bones: for Generic avatars
        private void DrawAvatarBonesHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Generic Avatar Bones List");
        }

        private void DrawAvatarBonesItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            float height = EditorGUIUtility.singleLineHeight;

            DatasetSetup.avatarBonesFoldout[index] = EditorGUI.Foldout(
                new Rect(rect.x, rect.y, 230, EditorGUIUtility.singleLineHeight),
                DatasetSetup.avatarBonesFoldout[index],
                new GUIContent(
                    string.IsNullOrEmpty(DatasetSetup.avatarBones[index].alias)
                        ? "New Bone"
                        : DatasetSetup.avatarBones[index].alias,
                    "Bone Setup"), true);

            DatasetSetup.avatarBonesSelected[index] = isFocused;
            if (!DatasetSetup.avatarBonesFoldout[index])
            {
                DatasetSetup.avatarBonesHeight[index] = height;
                return;
            }


            int expandedLines = 0;

            var datasetSetupAvatarBone = DatasetSetup.avatarBones[index];

            EditorGUI.LabelField(
                new Rect(rect.x, rect.y + height * ++expandedLines, 70, EditorGUIUtility.singleLineHeight),
                "Bone Alias");

            datasetSetupAvatarBone.alias = EditorGUI.DelayedTextField(
                new Rect(rect.x + 75, rect.y + height * expandedLines, 150, EditorGUIUtility.singleLineHeight),
                datasetSetupAvatarBone.alias);

            EditorGUI.LabelField(
                new Rect(rect.x, rect.y + height * ++expandedLines, 70, EditorGUIUtility.singleLineHeight),
                "Bone Name");

            // datasetSetupAvatarBone.boneName = EditorGUI.DelayedTextField(
            //     new Rect(rect.x + 75, rect.y + height * expandedLines, 175, EditorGUIUtility.singleLineHeight),
            //     datasetSetupAvatarBone.boneName);
            //
            DatasetSetup.avatarBonesIndex[index] = EditorGUI.Popup(
                new Rect(rect.x + 75, rect.y + height * expandedLines, 150, EditorGUIUtility.singleLineHeight),
                DatasetSetup.avatarBonesIndex[index], DatasetSetup.characterChildrenBones.ToArray());

            datasetSetupAvatarBone.boneName = DatasetSetup.characterChildrenBones[DatasetSetup.avatarBonesIndex[index]];

            DatasetSetup.avatarBones[index] = datasetSetupAvatarBone;

            DatasetSetup.avatarBonesHeight[index] = height * (expandedLines + 1);
        }

        private float AvatarBonesElementHeight(int index)
        {
            return DatasetSetup.avatarBonesHeight[index];
        }

        private void ManageChangedAvatarBones(ReorderableList list)
        {
            if (DatasetSetup.avatarBones.Count <= DatasetSetup.avatarBonesSelected.Count) return;

            DatasetSetup.avatarBonesHeight.Add(EditorGUIUtility.singleLineHeight);
            DatasetSetup.avatarBonesSelected.Add(false);
            DatasetSetup.avatarBonesFoldout.Add(false);
            DatasetSetup.avatarBonesIndex.Add(0);
        }

        private void ManageAddAvatarBones(ReorderableList list)
        {
            DatasetSetup.avatarBones.Add(
                new AvatarBone()
                {
                    alias = "NewBone" + DatasetSetup.avatarBones.Count,
                    boneName = DatasetSetup.characterChildrenBones[0]
                });
        }

        private void ManageRemoveAvatarBones(ReorderableList list)
        {
            int index = DatasetSetup.avatarBonesSelected.FindIndex(cs => cs);
            if (index == -1)
            {
                index = DatasetSetup.avatarBonesSelected.Count - 1;
            }

            DatasetSetup.avatarBones.RemoveAt(index);
            DatasetSetup.avatarBonesHeight.RemoveAt(index);
            DatasetSetup.avatarBonesFoldout.RemoveAt(index);
            DatasetSetup.avatarBonesSelected.RemoveAt(index);
            DatasetSetup.avatarBonesIndex.RemoveAt(index);

            //Update AvatarBone ID + update dependencies
            DatasetSetup.UpdateAvatarBonesId(index);
            DatasetSetup.CheckCharacteristicsAndContactDependencies(index);
        }

        #endregion
    }
}
