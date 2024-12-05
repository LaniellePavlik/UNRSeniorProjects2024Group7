using System;
using System.Collections.Generic;
using System.Linq;
using QuanticBrains.MotionMatching.PreviewCamera;
using QuanticBrains.MotionMatching.Scripts.Containers;
using QuanticBrains.MotionMatching.Scripts.Containers.CustomAvatars;
using QuanticBrains.MotionMatching.Scripts.Editor.DatasetConfiguration.InnerWindow;
using QuanticBrains.MotionMatching.Scripts.Editor.DatasetConfiguration.InnerWindow.Implementation;
using QuanticBrains.MotionMatching.Scripts.Importer;
using QuanticBrains.MotionMatching.Scripts.Tags;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;

namespace QuanticBrains.MotionMatching.Scripts.Editor.DatasetConfiguration
{
    public class DatasetSetup : EditorWindow
    {
        #region Variables

        //General
        public bool isRecording;

        //Window tabs
        public int tabs;
        private List<InnerWindowBase> _innerWindows;
        private string[] _tabOptions;

        //Window size
        private const int WindowInsideVertical = 640;
        private const int WindowVertical = 3 * BorderSpace + WindowInsideVertical;
        private const int WindowHorizontal = 880;

        //Positional settings
        private const int BorderSpace = 10;
        private const int TabsY = 50;
        private const int TabHeight = 50; //Height per option
        private const int TabWidth = 200;
        private const int YInnerSpace = 30;

        private Rect _tabsRect;


        //Timeline Window
        public AnimationClip anim;

        // - Graph
        public TimelineGraph graph;
        public float navPosX, mouseX, mouseY;
        public Vector2 fullWindowScrollPos;

        public int nMotions,
            selectedAnimation,
            lastSelectedAnimation,
            selectedLabel,
            highlightIndex,
            highlightLabel,
            intervalStart,
            intervalEnd;

        public Color col;

        // - Timeline Ranges
        public bool drawHighlight, motionSpace;
        public TagRange highlightRange;

        public float totalFrames;
        public int currentStep;

        public int styleIntervalStart,
            motionIntervalStart,
            offset,
            verticalCorrection,
            start,
            end,
            selectedTag;

        public int timelineBeginGroup;

        // - Preview
        public Camera previewCamPrefab;
        [HideInInspector] public Camera previewCam;

        // Tag variables
        public List<TagBase> motions, styles;
        private List<ActionTag> _actions;
        private IdleTag _idleTag;

        public List<string> motionTags = new() { "Walk", "Run", "Strafing" };
        public List<string> styleTags = new();

        // - Animation selector
        public ReorderableList AnimSelectorList;

        // Dataset configurations
        // - Dataset
        public Dataset datasetToEdit;
        public bool isDatasetLoaded;

        // - Configuration
        public float poseStep = 0.05f;
        public int recordVelocity = 20;
        public float futureEstimatesTime = 1f;
        public int futureEstimates = 3;
        public int pastEstimates = 1;
        public float pastEstimatesTime = 0.10f;
        public string animationDatabaseName;

        // - Characteristics
        public ReorderableList DefaultCharacteristics;
        public List<BoneCharacteristic> characteristics;
        public List<bool> characteristicsFoldout;
        public List<float> characteristicsHeight;
        public List<bool> characteristicsSelected;
        public List<int> characteristicsBonesIndex;

        // - Avatar Bones
        public ReorderableList GenericAvatarBones;
        public List<AvatarBone> avatarBones;
        public List<bool> avatarBonesFoldout;
        public List<float> avatarBonesHeight;
        public List<bool> avatarBonesSelected;
        public List<int> avatarBonesIndex;

        // - Character
        public GameObject character;
        public Transform root;
        public Avatar avatar;
        public bool disableRootAndAvatar = true;
        public bool isCharacterInitialized;
        private GameObject _targetModel;
        private bool _initialized;
        public List<string> characterChildrenBones;

        public CustomAvatar customAvatar;
        public int rootBone = 0;

        //Animations
        public List<AnimationClip> animations;
        public List<float> animationsHeight;
        public List<bool> animationsFiltered;

        public ReorderableList AnimFileList;

        //Tags
        // - Actions
        public ReorderableList DefaultActionTags;
        public List<ActionTagsData> actionTags;
        public List<bool> actionsFoldout;
        public List<float> actionsHeight;
        public List<bool> actionsSelected;

        // - Transitions
        public ReorderableList TransitionToIdleFiles;
        public List<AnimFileData> transitionAnims;
        public List<float> transitionHeight;
        public List<bool> transitionFoldout;
        public List<bool> transitionSelected;

        // - Idle
        public ReorderableList IdleFiles;
        public List<AnimFileData> idleAnims;
        public List<float> idleHeight;
        public List<bool> idleFoldout;
        public List<bool> idleSelected;

        // - Foldout
        public bool idleTagFoldout;
        public bool actionTagsFoldout;

        // Colors
        private readonly Color _greenColor = new Color(0.36f, 0.89f, 0.37f, 1.0f);
        private readonly Color _redColor = new Color(0.94f, 0.15f, 0.15f, 1.0f);
        private readonly Color _grayColor = new Color(0.55f, 0.55f, 0.55f, 1.0f);

        #endregion

        #region GUIMethods

        [MenuItem("Tools/Motion Matching/Dataset Setup")]
        public static void ShowWindow()
        {
            DatasetSetup datasetSetup = (DatasetSetup)GetWindow(typeof(DatasetSetup), false, "Dataset Setup");
            datasetSetup.minSize = new Vector2(WindowHorizontal, WindowVertical);
            datasetSetup.maxSize = new Vector2(WindowHorizontal, WindowVertical);
        }

        private void OnEnable()
        {
            ClearData(DataManagementEnum.Initial);

            //Instantiate cam
            if (!previewCam)
            {
                previewCam =
                    Instantiate(previewCamPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            }

            //Inner Windows
            _innerWindows = new List<InnerWindowBase>
            {
                new ConfigurationInnerWindow("Configuration", this),
                new AnimationsInnerWindow("Animations", this),
                new TagsInnerWindow("Actions & Idling", this),
                new TimelineInnerWindow("Timeline", this)
            };

            _tabOptions = _innerWindows.Select(x => x.WindowName).ToArray();
            _tabsRect = new Rect(BorderSpace, TabsY + BorderSpace, TabWidth, TabHeight * _tabOptions.Length);
            tabs = 0;
        }

        private void OnDestroy()
        {
            if (previewCam)
                DestroyImmediate(previewCam.gameObject);
            else
            {
                var cam = FindObjectOfType<PreviewCameraBehaviour>();
                if(cam)
                    DestroyImmediate(cam.gameObject);
            }
                
        }

        private void OnGUI()
        {
            if (isRecording && !EditorApplication.isPlaying)
            {
                isRecording = false;
            }

            InitialChecks();

            if (Event.current.type is EventType.MouseMove or EventType.MouseDrag or EventType.MouseDown
                or EventType.MouseUp)
            {
                Repaint();
            }

            EditorGUILayout.BeginHorizontal(GUILayout.Height(WindowVertical), GUILayout.Width(WindowHorizontal));

            //Tabs Menu
            ShowLeftMenu();

            //Tab content
            ShowTabContent();

            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region DrawMethods

        private void ShowLeftMenu()
        {
            EditorGUILayout.BeginVertical(GUILayout.Height(WindowVertical), GUILayout.Width(_tabsRect.width));

            EditorGUILayout.BeginVertical(GUILayout.Height(_tabsRect.height), GUILayout.Width(_tabsRect.width));
            ShowTabs();
            EditorGUILayout.EndVertical();

            GUILayout.Box("", new GUIStyle(), GUILayout.Height(320), GUILayout.Width(_tabsRect.width - 50));

            GUI.enabled = !isRecording; //This makes the window unable to be used when pressed process

            EditorGUILayout.BeginVertical(GUILayout.Height(TabHeight * 2), GUILayout.Width(_tabsRect.width));
            ProcessButton();
            ClearButton();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }

        private void ShowTabs()
        {
            GUILayout.Space(YInnerSpace);

            var previousColor = GUI.backgroundColor;
            GUI.backgroundColor = _grayColor;

            tabs = GUILayout.SelectionGrid(
                tabs, _tabOptions, 1, GUILayout.Width(_tabsRect.width), GUILayout.Height(_tabsRect.height));

            GUI.backgroundColor = previousColor;
        }

        private void ShowTabContent()
        {
            ShowCurrentTab(tabs);
        }

        private void ShowCurrentTab(int index)
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField(_tabOptions[index], EditorStyles.boldLabel);

            fullWindowScrollPos =
                EditorGUILayout.BeginScrollView(fullWindowScrollPos, GUI.skin.verticalScrollbar,
                    GUILayout.Width(_innerWindows[index].ScrollWidth));

            EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(_innerWindows[index].BoxWidth),
                GUILayout.Height(WindowInsideVertical)); //Inner window horizontal

            _innerWindows[index].OnDrawWindow();

            EditorGUILayout.EndHorizontal(); //End of box styled horizontal
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        #endregion

        #region ProcessButtons

        private void ProcessButton()
        {
            var previousColor = GUI.backgroundColor;
            GUI.backgroundColor = _greenColor;

            if (GUILayout.Button("Process dataset", GUILayout.Height(TabHeight), GUILayout.Width(_tabsRect.width)))
            {
                if (!CanProcess())
                {
                    return;
                }

                CheckAnimatorComponent(forceAvatar:true);
                
                var rp = character.GetComponent<RecordPositions>();
                if (rp == null)
                {
                    rp = character.AddComponent<RecordPositions>();
                }

                ProcessActionTags();
                ProcessIdleTag();

                var tags = motions.Concat(styles).ToList();

                var tagsToCombine = tags.Where(tag => tag.ranges is { Count: > 0 }).ToList();
                var combinations = GetCombinations(tagsToCombine);
                var rac = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                    "Assets/QuanticBrains/MotionMatching/Animator/recordAnimatorController.controller");

                var animPaths = new List<string>();
                foreach (var animationClip in animations)
                {
                    //animPaths.Add(AssetDatabase.GetAssetPath(animationClip)+"/"+animationClip.name);
                    var assetPath = AssetDatabase.GetAssetPath(animationClip);
                    if (assetPath.Contains(".fbx"))
                        assetPath += "//" + animationClip.name;

                    animPaths.Add(assetPath);
                }

                //Create custom avatar before recording
                CreateCustomAvatar();

                isRecording = true;
                rp.ProcessData(
                    ref animations,
                    animPaths,
                    customAvatar,
                    poseStep,
                    futureEstimates,
                    futureEstimatesTime,
                    pastEstimates,
                    pastEstimatesTime,
                    animationDatabaseName,
                    root,
                    recordVelocity,
                    combinations,
                    tags,
                    _actions,
                    new List<IdleTag> { _idleTag },
                    characteristics,
                    rac);

                ClearData(DataManagementEnum.Reinitialize);
            }

            GUI.backgroundColor = previousColor;
        }

        private void ClearButton()
        {
            var previousColor = GUI.backgroundColor;
            GUI.backgroundColor = _redColor;
            if (GUILayout.Button("Clear all", GUILayout.Height(TabHeight), GUILayout.Width(_tabsRect.width)))
            {
                if (EditorUtility.DisplayDialog("Warning",
                        "You are about to clean all data. This action can't be undone. \nAre you sure you want to clean it anyway?",
                        "Ok", "Cancel"))
                {
                    ClearData(DataManagementEnum.Complete);
                }
            }

            GUI.backgroundColor = previousColor;
        }

        #endregion

        #region ProcessAuxiliaryMethods

        private bool CanProcess()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Error when trying to process dataset", "The scene must be in play mode.",
                    "Ok");
                return false;
            }

            if (character == null)
            {
                EditorUtility.DisplayDialog("Error when trying to process dataset",
                    "Must assign a character to the component.", "Ok");
                return false;
            }

            if (animationDatabaseName == "")
            {
                EditorUtility.DisplayDialog("Error when trying to process dataset", "You must give the dataset a name.",
                    "Ok");
                return false;
            }

            if (animations == null || animations.Count < 1)
            {
                EditorUtility.DisplayDialog("Error when trying to process dataset", "Not enough animations.", "Ok");
                return false;
            }

            // Check if any tag was set
            var hasTags = motions.Concat(styles).Any(tag => tag.ranges.Count > 0);
            if (!hasTags)
            {
                EditorUtility.DisplayDialog("Error when trying to process dataset", "No motion tag ranges were set.",
                    "Ok");
                return false;
            }

            var hasIdle = idleAnims.Count > 0;
            if (!hasIdle)
            {
                EditorUtility.DisplayDialog("Error when trying to process dataset", "No idle animations were set.",
                    "Ok");
                return false;
            }

            if (characteristics.Count < 1)
            {
                EditorUtility.DisplayDialog("Error when trying to process dataset", "No characteristics were set.",
                    "Ok");
                return false;
            }

            if (avatarBones.Count < 1)
            {
                EditorUtility.DisplayDialog("Error when trying to process dataset",
                    "No Bones were set for Custom Avatar.",
                    "Ok");
                return false;
            }

            if (!character.activeSelf)
            {
                EditorUtility.DisplayDialog("Error when trying to process dataset",
                    "Character GameObject is not active.", "Ok");
                return false;
            }

            return true;
        }

        //Process Action Tags with window parameters
        private void ProcessActionTags()
        {
            _actions = new List<ActionTag>();
            foreach (var actionTag in actionTags)
            {
                if (!actionTag.isLoopable)
                    CreateAndAddActionTag(actionTag);
                else
                    CreateAndAddLoopableActionTag(actionTag);
            }
        }

        private void CreateAndAddActionTag(ActionTagsData actionTagData)
        {
            //None option is the first one
            bool hasInit = actionTagData.HasInitState();
            bool hasRecovery = actionTagData.HasRecoveryState();

            //Create action tag
            ActionTag tag = new ActionTag(actionTagData.actionName, hasInit, hasRecovery);

            //States interrupted
            tag.isInterruptibleByState[0] = actionTagData.isInitInterruptible;
            tag.isInterruptibleByState[1] = actionTagData.isInProgressInterruptible;
            tag.isInterruptibleByState[2] = actionTagData.isRecoveryInterruptible;
            tag.interruptibleType = actionTagData.interruptibleType;

            tag.allowedInterruptionNames = actionTagData.tagNamesList;

            tag.animationIDSerialized = new[]
            {
                actionTagData.initAnimationID, actionTagData.actionAnimationID,
                actionTagData.recoveryAnimationID
            };

            List<TagRange> ranges = new List<TagRange>();
            //Init ranges
            AddToRangeListIfExists(hasInit, actionTagData.initAnimationID - 1, ref ranges);
            AddToRangeListIfExists(true, actionTagData.actionAnimationID, ref ranges);
            AddToRangeListIfExists(hasRecovery, actionTagData.recoveryAnimationID - 1, ref ranges);
            tag.ranges = ranges;

            //Warping
            tag.warpingType = actionTagData.warpingType;
            tag.positionWarpWeight = actionTagData.positionWarpWeight;
            tag.posWarpingMode = actionTagData.posWarpingMode;

            tag.rotationWarpWeight = actionTagData.rotationWarpWeight;
            tag.rotWarpingMode = actionTagData.rotWarpingMode;

            tag.contactWarping = actionTagData.contactWarping;
            tag.warpContactBones = new List<AvatarBone>();

            foreach (var contactID in actionTagData.contactWarpBones)
            {
                tag.warpContactBones.Add(avatarBones[contactID]);
            }

            //Curves
            var actionAnim = animations[actionTagData.actionAnimationID];
            var bindings = AnimationUtility.GetCurveBindings(actionAnim);

            foreach (var b in bindings)
            {
                switch (b.propertyName)
                {
                    case "WarpPosition":
                        tag.customWarpPositionCurve = GetAnimationCurve(actionAnim, b);
                        continue;

                    case "WarpRotation":
                        tag.customWarpRotationCurve = GetAnimationCurve(actionAnim, b);
                        continue;
                }
            }

            _actions.Add(tag);
        }

        private void CreateAndAddLoopableActionTag(ActionTagsData actionTagData)
        {
            //None option is the first one
            bool hasInit = actionTagData.HasInitState();
            bool hasRecovery = actionTagData.HasRecoveryState();

            //Create Loop Action Tag
            LoopActionTag tag =
                new LoopActionTag(actionTagData.actionName, hasInit, hasRecovery, actionTagData.isSimulated);

            //States interrupted
            tag.isInterruptibleByState[0] = actionTagData.isInitInterruptible;
            tag.isInterruptibleByState[1] = actionTagData.isInProgressInterruptible;
            tag.isInterruptibleByState[2] = actionTagData.isRecoveryInterruptible;
            tag.interruptibleType = actionTagData.interruptibleType;

            tag.allowedInterruptionNames = actionTagData.tagNamesList;

            tag.animationIDSerialized = new[]
            {
                actionTagData.initAnimationID, actionTagData.actionAnimationID,
                actionTagData.recoveryAnimationID
            };

            List<TagRange> ranges = new List<TagRange>();
            //Init ranges
            AddToRangeListIfExists(hasInit, actionTagData.initAnimationID - 1, ref ranges);
            AddToRangeListIfExists(true, actionTagData.actionAnimationID, ref ranges);
            AddToRangeListIfExists(hasRecovery, actionTagData.recoveryAnimationID - 1, ref ranges);
            tag.ranges = ranges;

            //Warping
            tag.warpingType = actionTagData.warpingType;
            tag.positionWarpWeight = actionTagData.positionWarpWeight;
            tag.posWarpingMode = actionTagData.posWarpingMode;

            tag.rotationWarpWeight = actionTagData.rotationWarpWeight;
            tag.rotWarpingMode = actionTagData.rotWarpingMode;

            tag.contactWarping = actionTagData.contactWarping;
            tag.warpContactBones = new List<AvatarBone>();
            foreach (var contactID in actionTagData.contactWarpBones)
            {
                tag.warpContactBones.Add(avatarBones[contactID]);
            }

            if (actionTagData.initAnimationID != 0)
            {
                //Curves
                var initAnim = animations[actionTagData.initAnimationID - 1];
                var bindings = AnimationUtility.GetCurveBindings(initAnim);

                foreach (var b in bindings)
                {
                    switch (b.propertyName)
                    {
                        case "WarpPosition":
                            tag.customWarpPositionCurve = GetAnimationCurve(initAnim, b);
                            continue;

                        case "WarpRotation":
                            tag.customWarpRotationCurve = GetAnimationCurve(initAnim, b);
                            continue;
                    }
                    //ToDo: add assert here and show message if there is no named curves
                }
            }

            _actions.Add(tag);
        }

        //Process Action Tags with window parameters
        private void ProcessIdleTag()
        {
            bool hasTransition = transitionAnims.Count != 0;
            _idleTag = new IdleTag("Idle", hasTransition);

            List<TagRange> transitionRanges = new List<TagRange>();
            int[] transitionIDs = new int[transitionAnims.Count];

            //Init ranges
            for (var i = 0; i < transitionAnims.Count; i++)
            {
                var transitionAnim = transitionAnims[i];
                transitionIDs[i] = transitionAnim.animationID;

                List<TagRange> ranges = new List<TagRange>();
                AddToRangeListIfExists(true, transitionAnim.animationID, ref ranges);
                transitionRanges = transitionRanges.Concat(ranges).ToList();
            }

            _idleTag.initRanges = transitionRanges;
            _idleTag.transitionIDSerialized = transitionIDs;

            List<TagRange> idleRanges = new List<TagRange>();
            int[] loopIDs = new int[idleAnims.Count];

            //Idle Ranges
            for (var i = 0; i < idleAnims.Count; i++)
            {
                var idleAnim = idleAnims[i];
                loopIDs[i] = idleAnim.animationID;

                List<TagRange> ranges = new List<TagRange>();
                AddToRangeListIfExists(true, idleAnim.animationID, ref ranges);
                idleRanges = idleRanges.Concat(ranges).ToList();
            }

            _idleTag.loopRanges = idleRanges;
            _idleTag.loopIDSerialized = loopIDs;

            _idleTag.ranges = _idleTag.initRanges;
        }

        //Auxiliary method for retrieving animation curve binded to a certain property name
        //Regular method has some problems, as it resizes the curve length, so this one fixes it
        private AnimationCurve GetAnimationCurve(AnimationClip actionAnim, EditorCurveBinding b)
        {
            var curve = AnimationUtility.GetEditorCurve(actionAnim, b);
            AnimationCurve newCurve = new AnimationCurve();
            Keyframe[] newKeys = new Keyframe[curve.keys.Length];

            for (var index = 0; index < curve.keys.Length; index++)
            {
                var keyframe = curve.keys[index];
                Keyframe newKey = new Keyframe
                {
                    outTangent = keyframe.outTangent,
                    outWeight = keyframe.outWeight,
                    time = keyframe.time / actionAnim.averageDuration,
                    value = keyframe.value,
                    weightedMode = keyframe.weightedMode
                };

                newKeys[index] = newKey;
            }

            newCurve.keys = newKeys;

            //Set tangent mode
            for (int k = 0; k < newKeys.Length; k++)
            {
                var leftTan = AnimationUtility.GetKeyLeftTangentMode(curve, k);
                AnimationUtility.SetKeyLeftTangentMode(newCurve, k, leftTan);

                var rightTan = AnimationUtility.GetKeyRightTangentMode(curve, k);
                AnimationUtility.SetKeyRightTangentMode(newCurve, k, rightTan);
            }

            return newCurve;
        }

        private void AddToRangeListIfExists(bool exists, int animationID, ref List<TagRange> ranges)
        {
            if (exists)
            {
                var currentAnim = animations[animationID];

                int frameStart = 0;
                int frameEnd = (int)(currentAnim.length * currentAnim.frameRate);

                TagRange initRange = new TagRange(currentAnim.name, frameStart, frameEnd);
                ranges.Add(initRange);
            }
            else
            {
                ranges.Add(new TagRange("", 0, 0));
            }
        }

        public void ReviewTags(String animName, ref List<TagBase> tagBase)
        {
            foreach (var (tag, tagIndex) in motions.Select((tag, tagIndex) => (tag, tagIndex)))
            {
                var newRange = new List<TagRange>();
                foreach (var range in tag.ranges)
                {
                    if (range.animName != animName)
                    {
                        newRange.Add(range);
                    }
                }

                motions[tagIndex].ranges = newRange;
            }
        }

        public void ReviewActionTags(int index, ref List<ActionTagsData> actionTagsData)
        {
            foreach (var actionData in actionTagsData)
            {
                //If its the same, then keep it - user must be fixing it later
                //If its greater, then decrease so the reference is still the same
                //If its lower, then keep it the same, as it is not affected

                //Note that init and recovery have indexes 1 greater than real correspondence, as they handle "None"
                if ((actionData.initAnimationID - 1) > index)
                {
                    actionData.initAnimationID -= 1;
                }

                if ((actionData.recoveryAnimationID - 1) > index)
                {
                    actionData.recoveryAnimationID -= 1;
                }

                if (actionData.actionAnimationID > index)
                {
                    actionData.actionAnimationID -= 1;
                }
            }
        }

        private static IEnumerable<List<TagBase>> GetCombinations(List<TagBase> tags)
        {
            return Enumerable
                .Range(1, (1 << tags.Count) - 1) //Exclude empty combination
                .Select(index => tags
                    .Where((_, i) => (index & (1 << i)) != 0)
                    .ToList());
        }

        #endregion

        #region RecoveryMethods

        public void TryRecoverStyleTags()
        {
            if (datasetToEdit.tagsList.tags.Count <= nMotions) return;

            var tagNames = datasetToEdit.tagsList.tags.Aggregate(Array.Empty<string>(),
                (acc, tag) => acc.Concat(new[] { tag.name }).ToArray()).Distinct();
            foreach (var tagName in tagNames)
            {
                if (motionTags.Contains(tagName))
                {
                    continue;
                }

                TagBase tag = datasetToEdit.tagsList.tags.Find(t => t.name.Equals(tagName));
                styleTags.Add(tag.name);
                styles.Add(tag);
            }

            UpdateStyleTagsDependencies();
        }

        public void UpdateStyleTagsDependencies()
        {
            var animWindow = _innerWindows.First(x => x.WindowName == "Animations");
            var timelineWindow = _innerWindows.First(x => x.WindowName == "Timeline");

            ((AnimationsInnerWindow)animWindow).InitializeStyleGrid();
            ((TimelineInnerWindow)timelineWindow).UpdateFilterOptions();
        }

        public void LoadCharacteristics()
        {
            if (datasetToEdit.characteristics == null)
            {
                EditorUtility.DisplayDialog("Warning when loading dataset",
                    "No Characteristics asset was found in the dataset. Please review the dataset.", "Ok");
                return;
            }

            characteristics.Clear();
            characteristicsHeight.Clear();
            characteristicsSelected.Clear();
            characteristicsBonesIndex.Clear();
            characteristicsFoldout.Clear();

            var characteristicsByTags = datasetToEdit.characteristics.characteristicsByTags
                .Select(feature => feature.characteristics).ToList();
            foreach (var boneChars in characteristicsByTags)
            {
                if (boneChars.Count == 0)
                {
                    continue;
                }

                foreach (var boneChar in boneChars)
                {
                    characteristicsFoldout.Add(false);
                    characteristicsSelected.Add(false);
                    characteristicsBonesIndex.Add(boneChar.bone.id);
                    characteristicsHeight.Add(EditorGUIUtility.singleLineHeight);
                    characteristics.Add(new BoneCharacteristic()
                    {
                        bone = boneChar.bone,
                        weightPosition = boneChar.weightPosition,
                        weightVelocity = boneChar.weightVelocity
                    });
                }

                return;
            }
        }

        public void LookForPreviousTags(List<string> missingAnims)
        {
            motions = SearchMotion(motions, missingAnims, datasetToEdit.tagsList.tags);
            styles = SearchMotion(styles, missingAnims, datasetToEdit.tagsList.tags);
        }

        private List<TagBase> SearchMotion(List<TagBase> array, List<string> missingAnims,
            List<TagBase> datasetRefArray)
        {
            var tempArray = new List<TagBase>();

            foreach (var it in array.Select((motion, index) => new { motion, index }))
            {
                tempArray.Add(it.motion);
                var tag = datasetRefArray.FirstOrDefault(x => x.name == it.motion.name);

                if (missingAnims.Count > 0 && tag != null)
                {
                    var ranges = new List<TagRange>();
                    foreach (var range in tag.ranges)
                    {
                        var missing = false;
                        foreach (var ma in missingAnims)
                        {
                            if (range.animName == ma)
                            {
                                missing = true;
                                break;
                            }
                        }

                        if (!missing)
                        {
                            ranges.Add(range);
                        }
                    }

                    tag.ranges = ranges;
                }

                tempArray[it.index] = tag;
            }

            return tempArray;
        }

        public void LookForPreviousActionTags()
        {
            if (datasetToEdit == null) return;

            ResetActionTags();

            foreach (var actionQuery in datasetToEdit.queriesComputed.actionQueries)
            {
                ActionTag targetActionTag = actionQuery.actionTag;
                FillActionTagData(targetActionTag);
            }

            foreach (var loopQuery in datasetToEdit.queriesComputed.loopActionQueries)
            {
                ActionTag targetActionTag = loopQuery.actionTag;
                FillActionTagData(targetActionTag, true);
            }
        }

        private void FillActionTagData(ActionTag targetActionTag, bool isLoopable = false)
        {
            ActionTagsData newData = new ActionTagsData();
            newData.actionName = targetActionTag.name;
            newData.isLoopable = isLoopable;

            //States interrupted
            newData.isInitInterruptible = targetActionTag.isInterruptibleByState[0];
            newData.isInProgressInterruptible = targetActionTag.isInterruptibleByState[1];
            newData.isRecoveryInterruptible = targetActionTag.isInterruptibleByState[2];
            newData.interruptibleType = targetActionTag.interruptibleType;

            newData.tagNamesList = new List<string>(targetActionTag.allowedInterruptionNames);

            //IDs
            newData.initAnimationID = targetActionTag.animationIDSerialized[0];
            newData.actionAnimationID = targetActionTag.animationIDSerialized[1];
            newData.recoveryAnimationID = targetActionTag.animationIDSerialized[2];

            //Warping
            newData.warpingType = targetActionTag.warpingType;
            newData.posWarpingMode = targetActionTag.posWarpingMode;
            newData.rotWarpingMode = targetActionTag.rotWarpingMode;
            newData.positionWarpWeight = targetActionTag.positionWarpWeight;
            newData.rotationWarpWeight = targetActionTag.rotationWarpWeight;

            newData.contactWarping = targetActionTag.contactWarping;
            newData.contactWarpBones = new List<int>();

            foreach (var contactBone in targetActionTag.warpContactBones)
            {
                newData.contactWarpBones.Add(contactBone.id);
            }

            //Root simulation for loops
            newData.isSimulated = targetActionTag.simulateRootMotion;

            actionTags.Add(newData);
            actionsFoldout.Add(false);
            actionsSelected.Add(false);
            actionsHeight.Add(EditorGUIUtility.singleLineHeight);
        }

        public void LookForPreviousIdleTag()
        {
            if (datasetToEdit == null) return;

            ResetIdleTags();

            foreach (var idleQuery in datasetToEdit.queriesComputed.idleQueries)
            {
                IdleTag targetIdleTag = idleQuery.loopTag;

                foreach (var transition in targetIdleTag.transitionIDSerialized)
                {
                    AnimFileData transitionData = new AnimFileData();
                    transitionData.animationID = transition;
                    transitionAnims.Add(transitionData);
                    transitionHeight.Add(EditorGUIUtility.singleLineHeight);
                    transitionFoldout.Add(false);
                    transitionSelected.Add(false);
                }

                foreach (var idle in targetIdleTag.loopIDSerialized)
                {
                    AnimFileData idleData = new AnimFileData();
                    idleData.animationID = idle;
                    idleAnims.Add(idleData);
                    idleHeight.Add(EditorGUIUtility.singleLineHeight);
                    idleFoldout.Add(false);
                    idleSelected.Add(false);
                }
            }
        }

        //Reset Tags - When restoring dataset
        private void ResetIdleTags()
        {
            transitionAnims.Clear();
            transitionHeight.Clear();
            transitionSelected.Clear();
            transitionFoldout.Clear();

            idleAnims.Clear();
            idleHeight.Clear();
            idleSelected.Clear();
            idleFoldout.Clear();
        }

        private void ResetActionTags()
        {
            actionTags.Clear();
            actionsFoldout.Clear();
            actionsSelected.Clear();
            actionsHeight.Clear();
        }

        public void ResetAnimations()
        {
            animations.Clear();
            animationsHeight.Clear();
            animationsFiltered.Clear();
        }

        public bool ConfirmChanges(Avatar previousAvatar)
        {
            var newAvatar = avatar;

            if (disableRootAndAvatar)
            {
                var newAnim = character.GetComponent<Animator>();
                if (!newAnim) return true;
                
                newAvatar = newAnim.avatar;
            }

            if (!previousAvatar) return true;
            if (previousAvatar.isHuman && newAvatar.isHuman) return true;
            if (newAvatar == previousAvatar) return true;

            //Else, generic > humanoid, humanoid > generic ask for change
            return EditorUtility.DisplayDialog("Warning",
                "You are about to change your avatar type: \n" +
                (previousAvatar.isHuman ? "Human" : "Generic") + ">" +
                (newAvatar.isHuman ? "Human. " : "Generic.") +
                "\n\nAre you sure you want to change it?",
                "Ok", "Cancel");
        }

        public void InitializeCharacter()
        {
            if (!character)
            {
                //Removed character
                isCharacterInitialized = false;
                disableRootAndAvatar = true;
                root = null;
                avatar = null;
                return;
            }

            character.SetActive(true);
            root = character.transform;

            var animator = CheckAnimatorComponent(warningLog:true);

            if (!animator.avatar && !avatar)
            {
                //If animator doesn't have avatar (or new animator has been added)
                PropagateCharacterDependencies(character.transform);

                if (!disableRootAndAvatar) return;
                //If avatar selection is disabled -> enable it

                disableRootAndAvatar = false;
                EditorUtility.DisplayDialog("Warning",
                    "An Avatar component is missing. Please add one in the Character parameters section.", "Ok");
                return;
            }

            if (disableRootAndAvatar)
            {
                avatar = animator
                    .avatar;

                if(!avatar)
                    disableRootAndAvatar = false;
                
                isCharacterInitialized = true;
                PropagateCharacterDependencies(character.transform);
                return;
            }

            //If avatar/root has been enabled + 
            animator.avatar = avatar;
            disableRootAndAvatar = true;
            isCharacterInitialized = true;
            PropagateCharacterDependencies(character.transform);
        }

        private Animator CheckAnimatorComponent(bool forceAvatar = false, bool warningLog = false)
        {
            var animator = character.GetComponent<Animator>();

            if (!animator)
            {
                //If animator - doesn't exist, add component before continuing
                animator = character.AddComponent<Animator>();
                
                if(warningLog)
                    EditorUtility.DisplayDialog("Warning",
                        "An Animator component was added to the character, as it did not have one.", "Ok");
            }

            //Force animator setup
            animator.enabled = false;
            animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
            animator.applyRootMotion = true;

            if (forceAvatar)
                animator.avatar = avatar;
            
            return animator;
        }

        public void InitializeCustomAvatar(Avatar previousAvatar)
        {
            //If avatar changes - avatar from animator when adding gameobject
            if (!character) return;

            if (datasetToEdit && datasetToEdit.avatar.avatar == avatar)
            {
                if (!avatar.isHuman) //Fill with dataset - remove bones if previous avatar was different
                    FillAvatarBonesByDataset(previousAvatar != avatar);
                else
                    InitHumanAvatarBones();

                //Restore characteristics, as it is the same avatar in both dataset and window
                LoadCharacteristics();
                InitChildrenFromDataset();
                return;
            }

            InitChildrenTransforms();

            if (!avatar)
                return;

            //Reset if generic>human, human>generic or generic>generic
            if (!avatar.isHuman)
            {
                ResetCharacteristicsAndContactBones();
            }
            else if (previousAvatar && previousAvatar.isHuman != avatar.isHuman)
            {
                ResetCharacteristicsAndContactBones();
            }

            //Do specific tasks depending on current new avatar
            if (!avatar.isHuman)
            {
                avatarBones.Clear();
            }
            else
            {
                InitHumanAvatarBones();
                InitializeHumanDefaultCharacteristics();
            }
        }

        private void InitHumanAvatarBones()
        {
            var auxAvatar = CreateInstance<HumanoidAvatar>();
            auxAvatar.avatar = avatar;
            avatarBones = auxAvatar.GetAvatarDefinition();
            DestroyImmediate(auxAvatar);
        }

        public void InitChildrenFromDataset()
        {
            characterChildrenBones = new List<string>();
            foreach (var avatarBone in avatarBones)
            {
                characterChildrenBones.Add(avatarBone.boneName);
            }
        }

        private void InitChildrenTransforms()
        {
            characterChildrenBones = new List<string>();
            GetChildAvatarBones(character.transform, ref characterChildrenBones);
        }

        private void CreateCustomAvatar()
        {
            if (!avatar.isHuman)
            {
                customAvatar = CreateInstance<GenericAvatar>();
                customAvatar.avatar = avatar;
                ((GenericAvatar)customAvatar).SetAvatarDefinition(avatarBones);
                customAvatar.SetRootBone(rootBone);
            }
            else
            {
                customAvatar = CreateInstance<HumanoidAvatar>();
                customAvatar.avatar = avatar;
                avatarBones = customAvatar.GetAvatarDefinition();
                customAvatar.SetRootBone((int)HumanBodyBones.Hips);
            }
        }

        private void ResetCharacteristicsAndContactBones()
        {
            var charStr = "";
            var contactStr = "";
            var charUpdated = false;

            if (characteristics.Count != 0)
            {
                charUpdated = true;
                charStr = "\nConfiguration > Default Characteristics";
            }

            characteristics.Clear();
            characteristicsHeight.Clear();
            characteristicsSelected.Clear();
            characteristicsBonesIndex.Clear();
            characteristicsFoldout.Clear();

            bool warpUpdated = false;
            foreach (var tag in actionTags)
            {
                if (!tag.isLoopable) continue;
                if (tag.contactWarpBones.Count == 0) continue;

                tag.contactWarpBones.Clear();
                warpUpdated = true;
            }

            if (warpUpdated)
                contactStr = "\nActions & Idling > Action Tags > Contact Warping";

            if (!warpUpdated && !charUpdated) return;

            var message = "Replacing/Deleting avatar has modified Avatar Bones." +
                          "\nIts usage has been reset to default value.\nPlease check:"
                          + charStr + contactStr;

            EditorUtility.DisplayDialog("Warning",
                message,
                "Ok");
        }

        private void InitializeHumanDefaultCharacteristics()
        {
            var hipsID = avatarBones.FindIndex(x =>
                x.alias == HumanBodyBones.Hips.ToString());
            var leftFootID = avatarBones.FindIndex(x =>
                x.alias == HumanBodyBones.LeftFoot.ToString());
            var rightFootID = avatarBones.FindIndex(x =>
                x.alias == HumanBodyBones.RightFoot.ToString());

            characteristics = new List<BoneCharacteristic>
            {
                new()
                {
                    bone = avatarBones[hipsID]
                },
                new()
                {
                    bone = avatarBones[leftFootID]
                },
                new()
                {
                    bone = avatarBones[rightFootID]
                },
            };
            characteristicsFoldout = new List<bool> { false, false, false };
            characteristicsHeight = new List<float>
            {
                EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight,
                EditorGUIUtility.singleLineHeight
            };

            characteristicsSelected = new List<bool> { false, false, false };
            characteristicsBonesIndex = new List<int> { hipsID, leftFootID, rightFootID };

            _innerWindows[0].ClearData(DataManagementEnum.Initial);
        }

        public void UpdateAvatarBonesId(int id)
        {
            // Re-order the id given the new array
            for (int i = 0; i < avatarBones.Count; i++)
            {
                if (i < id) continue;

                var avatarBone = avatarBones[i];
                avatarBone.id = i;
                avatarBones[i] = avatarBone;
            }
        }

        public void CheckCharacteristicsAndContactDependencies(int id)
        {
            var characteristicsChanged = false;
            var actionsChanged = false;

            if (avatarBones.Count == 0 && characteristics.Count != 0)
            {
                characteristicsChanged = true;
                characteristics.Clear();
            }

            //1- Check characteristics using the new deleted index
            for (int i = 0; i < characteristics.Count; i++)
            {
                //Lower, then keep it the same
                if (characteristicsBonesIndex[i] < id) continue;

                //Using that index -> change to default
                if (characteristicsBonesIndex[i] == id)
                {
                    characteristics[i].bone = avatarBones[0];
                    characteristicsBonesIndex[i] = characteristics[i].bone.id;

                    characteristicsChanged = true;
                    continue;
                }

                //Greater, then update
                characteristics[i].bone = avatarBones[characteristics[i].bone.id - 1];
                characteristicsBonesIndex[i] = characteristics[i].bone.id;
            }

            //2- Check WarpContactBones
            foreach (var actionTag in actionTags)
            {
                if (avatarBones.Count == 0)
                {
                    //If there are no avatar bones left, clear
                    if (actionTag.contactWarpBones.Count == 0) continue;

                    actionTag.contactWarpBones.Clear();
                    actionsChanged = true;
                    continue;
                }

                for (int i = 0; i < actionTag.contactWarpBones.Count; i++)
                {
                    //If index is lower than deleted one, then it remains the same
                    if (actionTag.contactWarpBones[i] < id) continue;

                    //if it equals that bone, then it must be replaced
                    if (actionTag.contactWarpBones[i] == id)
                    {
                        actionTag.contactWarpBones[i] = 0;
                        actionsChanged = true;
                        continue;
                    }

                    //If it is >= deletedID but decrease because of deletion
                    actionTag.contactWarpBones[i] = Math.Max(actionTag.contactWarpBones[i] - 1, 0);
                }
            }

            if (!actionsChanged && !characteristicsChanged) return;

            var contactBonesStr = actionsChanged ? "\nActions & Idling > Action Tags" : "";
            var characteristicsStr = characteristicsChanged ? "\nConfiguration > Default Characteristics" : "";
            var message = "The deleted Avatar Bone was being used on the dataset." +
                          "\nIts usage has been reset to default value.\nPlease check:"
                          + contactBonesStr +
                          characteristicsStr;

            EditorUtility.DisplayDialog("Warning",
                message,
                "Ok");
        }

        public void UpdateAnimationDependenciesForTags(int id)
        {
            //1- Check Action Tags dependencies
            foreach (var actionTag in actionTags)
            {
                actionTag.actionAnimationID = 
                    GetUpdatedAnimationIndex(actionTag.actionAnimationID, id);
                actionTag.initAnimationID = 
                    GetUpdatedAnimationIndex(actionTag.initAnimationID - 1, id) + 1;
                actionTag.recoveryAnimationID = 
                    GetUpdatedAnimationIndex(actionTag.recoveryAnimationID - 1, id) + 1;
            }
            
            //2- Check for Idle dependencies
            foreach (var idleAnim in idleAnims)
            {
                idleAnim.animationID = 
                    GetUpdatedAnimationIndex(idleAnim.animationID, id);
            }
            
            //3- Check for Idle dependencies
            foreach (var transitionAnim in transitionAnims)
            {
                transitionAnim.animationID = 
                    GetUpdatedAnimationIndex(transitionAnim.animationID, id);
            }
        }

        private int GetUpdatedAnimationIndex(int index, int deletedIndex)
        {
            //Lower, then keep it the same
            if (index < deletedIndex) return index;

            //Using that index -> change to default
            if (index == deletedIndex)
            {
                return 0;
            }
            
            //Greater, then update
            return index - 1;
        }

        public bool RemoveAllAvatarBonesWithWarning()
        {
            if (avatarBones.Count == 0) return true;
            if (!EditorUtility.DisplayDialog("Warning",
                    "This action will replace your existing Avatar Bones data\nand its dependencies." +
                    "\nAre you sure you want to delete it?",
                    "Ok", "Cancel")) return false;

            RemoveAllAvatarBones();
            return true;
        }

        private void RemoveAllAvatarBones()
        {
            avatarBones.Clear();
            avatarBonesHeight.Clear();
            avatarBonesFoldout.Clear();
            avatarBonesSelected.Clear();
            avatarBonesIndex.Clear();

            ResetCharacteristicsAndContactBones();
            rootBone = 0;
        }

        public void FillAvatarBonesByDataset(bool removeCurrentBones)
        {
            if (!datasetToEdit)
            {
                EditorUtility.DisplayDialog("Error while filling from dataset",
                    "You have not selected a Dataset to Edit." +
                    "\nTo use this button, select it under" + "\nConfiguration > Dataset To Edit > Dataset ",
                    "Ok");
                return;
            }

            if (!datasetToEdit.avatar)
            {
                EditorUtility.DisplayDialog("Error while filling from dataset",
                    "Your dataset does not contains any avatar." +
                    "\nAre you using a dataset from previous versions?" +"\nUpdate it first by recording again.",
                    "Ok");
                return; //This one doesn't have warning, as its for legacy datasets support
            }
            
            if (!datasetToEdit.avatar.avatar.isHuman && !avatar.isHuman && datasetToEdit.avatar.avatar != avatar)
            {
                EditorUtility.DisplayDialog("Error while filling from dataset",
                    "You can not fill from dataset." +
                    "\nYour dataset Generic Avatar doesn't match" +"\nwith your chosen Character's Avatar.",
                    "Ok");
                return; //Both avatars are generic but different
            }

            if (datasetToEdit.avatar.avatar.isHuman != avatar.isHuman)
            {
                EditorUtility.DisplayDialog("Error while filling from dataset",
                    "You can not fill from dataset." +
                    "\nYour dataset avatar and the chosen Character's" +"\navatar do not match (Generic vs Humanoid).",
                    "Ok");
                return; //Avatar have different types
            }

            if (removeCurrentBones)
                RemoveAllAvatarBones();

            var avatarDefinition = new List<AvatarBone>(datasetToEdit.avatar.GetAvatarDefinition());
            for (int i = 0; i < avatarDefinition.Count; i++)
            {
                var avatarBone = avatarDefinition[i];

                avatarBones.Add(avatarBone);
                avatarBonesHeight.Add(EditorGUIUtility.singleLineHeight);
                avatarBonesFoldout.Add(false);
                avatarBonesSelected.Add(false);
                avatarBonesIndex.Add(i);
            }

            rootBone = datasetToEdit.avatar.GetRootBone();
        }

        public void FillAvatarBonesByCharacter()
        {
            if (!character) return;

            if (!RemoveAllAvatarBonesWithWarning()) return;

            for (int i = 0; i < characterChildrenBones.Count; i++)
            {
                var child = characterChildrenBones[i];
                AvatarBone bone = new AvatarBone
                {
                    id = i,
                    alias = child,
                    boneName = child
                };

                avatarBones.Add(bone);
                avatarBonesHeight.Add(EditorGUIUtility.singleLineHeight);
                avatarBonesFoldout.Add(false);
                avatarBonesSelected.Add(false);
                avatarBonesIndex.Add(i);
            }

            rootBone = 0;
        }

        private void GetChildAvatarBones(Transform currentChild, ref List<string> children)
        {
            for (int i = 0; i < currentChild.childCount; i++)
            {
                var child = currentChild.GetChild(i);
                children.Add(child.name);

                GetChildAvatarBones(child, ref children);
            }
        }

        public void PropagateCharacterDependencies(Transform target)
        {
            var timelineWindow = _innerWindows.First(x => x.WindowName == "Timeline");

            ((TimelineInnerWindow)timelineWindow).SetPreviewTarget(target);
        }

        #endregion

        #region IntervalMethods

        public void AddInterval(string animName, int startFrame, int endFrame, float nTotalFrames,
            int currentSelectedTag)
        {
            var add = true;

            if (startFrame > nTotalFrames)
            {
                EditorUtility.DisplayDialog("Error when adding interval",
                    "The first interval to add is bigger than the animation.", "Ok");
                add = false;
            }

            if (endFrame > nTotalFrames)
            {
                EditorUtility.DisplayDialog("Error when adding interval",
                    "The last interval to add is bigger than the animation.", "Ok");
                add = false;
            }

            if (startFrame > endFrame)
            {
                EditorUtility.DisplayDialog("Error when adding interval",
                    "An Avatar component is missing. Please add one in the Character parameters section.", "Ok");
                add = false;
            }

            if (add)
            {
                if (currentSelectedTag < nMotions)
                {
                    motions[currentSelectedTag].ranges = CheckOverlap(
                        motions[currentSelectedTag].ranges,
                        startFrame,
                        endFrame,
                        animName);
                }
                else
                {
                    styles[currentSelectedTag - nMotions].ranges = CheckOverlap(
                        styles[currentSelectedTag - nMotions].ranges,
                        startFrame,
                        endFrame,
                        animName);
                }
            }
        }

        private List<TagRange> CheckOverlap(List<TagRange> ranges, int startFrame, int stop, string animName)
        {
            ranges.Add(
                new TagRange(
                    animName,
                    startFrame,
                    stop));

            ranges = CheckIntervals(ranges, animName);

            return ranges;
        }

        private List<TagRange> CheckIntervals(List<TagRange> ranges, string animName)
        {
            var overlappingIndexes = new List<int>();
            for (var i = 0; i < ranges.Count; i++)
            {
                var r = ranges[i];
                
                // Only check for animName animation
                if (r.animName != animName)
                {
                    continue;
                }
                
                for (var j = i + 1; j < ranges.Count; j++)
                {
                    var r2 = ranges[j];

                    // If they are not the same animation, don't check overlap
                    if (r.animName != r2.animName)
                    {
                        continue;
                    }
                    
                    if (r2.frameStart > r.frameStop)
                    {
                        continue;
                    }

                    if (r2.frameStop < r.frameStart)
                    {
                        continue;
                    }

                    // Drawing starts before interval, ends after
                    if (r2.frameStart <= r.frameStart && r2.frameStop >= r.frameStop)
                    {
                        r.frameStart = r2.frameStart;
                        r.frameStop = r2.frameStop;
                        ranges[i] = r;
                        overlappingIndexes.Add(i);
                        overlappingIndexes.Add(j);
                        continue;
                    }

                    // Drawing starts before interval, ends inside
                    if (r2.frameStart <= r.frameStart && r2.frameStop <= r.frameStop)
                    {
                        r.frameStart = r2.frameStart;
                        ranges[i] = r;
                        overlappingIndexes.Add(i);
                        overlappingIndexes.Add(j);
                        continue;
                    }

                    // Drawing starts inside interval, ends after
                    if (r2.frameStart >= r.frameStart && r2.frameStop >= r.frameStop)
                    {
                        r.frameStop = r2.frameStop;
                        ranges[i] = r;
                        overlappingIndexes.Add(i);
                        overlappingIndexes.Add(j);
                    }
                }
            }

            if (overlappingIndexes.Count > 0)
            {
                var min = int.MaxValue;
                var max = int.MinValue;

                foreach (var index in overlappingIndexes)
                {
                    if (ranges[index].frameStart < min)
                    {
                        min = ranges[index].frameStart;
                    }

                    if (ranges[index].frameStop > max)
                    {
                        max = ranges[index].frameStop;
                    }
                }

                var newRanges = new List<TagRange>();

                foreach (var it in ranges.Select((item, index) => new { item, index }))
                {
                    if (overlappingIndexes.Contains(it.index))
                    {
                        continue;
                    }

                    newRanges.Add(it.item);
                }

                newRanges.Add(
                    new TagRange(
                        animName,
                        min,
                        max));

                return newRanges;
            }

            return ranges;
        }

        private (TagRange, int) CheckIntervalPresence(int tagIndex, int pos)
        {
            List<TagRange> ranges;

            if (tagIndex < 0)
            {
                return (new TagRange(), -1);
            }

            if (motionSpace)
            {
                ranges = motions[tagIndex].ranges;
            }
            else
            {
                if (tagIndex < styles.Count)
                {
                    ranges = styles[tagIndex].ranges;
                }
                else
                {
                    return (new TagRange(), -1);
                }
            }

            for (var i = 0; i < ranges.Count; i++)
            {
                if (ranges[i].frameStart > pos)
                {
                    continue;
                }

                if (ranges[i].frameStop < pos)
                {
                    continue;
                }

                if (ranges[i].animName != anim.name)
                {
                    continue;
                }

                return (ranges[i], i);
            }

            return (new TagRange(), -1);
        }

        public void CheckIntervalAddition()
        {
            if (mouseY >= (motionIntervalStart - timelineBeginGroup))
            {
                if (Math.Abs(intervalStart - intervalEnd) < 0.1f)
                {
                    var (range, index) = CheckIntervalPresence(selectedLabel, intervalStart);

                    if (index >= 0)
                    {
                        highlightRange = range;
                        highlightIndex = index;
                        highlightLabel = selectedLabel;
                        drawHighlight = true;
                    }
                    else
                    {
                        intervalStart = 0;
                        intervalEnd = 0;
                        selectedLabel = -1;
                        return;
                    }
                }

                if (selectedLabel >= 0 && (Math.Abs(intervalStart - intervalEnd) > 0.1f))
                {
                    if (intervalStart > intervalEnd)
                    {
                        (intervalStart, intervalEnd) = (intervalEnd, intervalStart);
                    }

                    if (motionSpace)
                    {
                        motions[selectedLabel].ranges = CheckOverlap(
                            motions[selectedLabel].ranges,
                            intervalStart,
                            intervalEnd,
                            anim.name);
                    }
                    else
                    {
                        if (selectedLabel < styles.Count)
                        {
                            styles[selectedLabel].ranges = CheckOverlap(
                                styles[selectedLabel].ranges,
                                intervalStart,
                                intervalEnd,
                                anim.name);
                        }
                    }
                }
            }

            intervalStart = 0;
            intervalEnd = 0;
            selectedLabel = -1;
        }

        private void ResetAnimationInterval()
        {
            if (Application.isPlaying)
            {
                if (motions != null)
                {
                    return;
                }
            }

            motions = motionTags.Select(mn => new TagBase(mn)).ToList();
            styles = styleTags.Select(sn => new TagBase(sn)).ToList();
        }

        #endregion

        #region OtherMethods

        public bool[] CheckIfTaggedByMotionOrStyles(string animName)
        {
            bool[] isTagged = new bool[motions.Count + styles.Count];
            int currentMotion = 0;

            //Check motions
            foreach (var motion in motions)
            {
                foreach (var range in motion.ranges)
                {
                    if (!range.animName.Equals(animName)) continue;

                    isTagged[currentMotion] = true;
                    break;
                }

                currentMotion++;
            }

            //Check styles
            int currentStyle = 0;
            foreach (var style in styles)
            {
                foreach (var range in style.ranges)
                {
                    if (!range.animName.Equals(animName)) continue;

                    isTagged[currentMotion + currentStyle] = true;
                    break;
                }

                currentStyle++;
            }

            return isTagged;
        }

        private void InitialChecks()
        {
            if (character == null)
            {
                return;
            }

            if (!_initialized)
            {
                return;
            }

            var cam = SceneView.lastActiveSceneView;
            if (cam != null)
            {
                cam.pivot = character.transform.position;
            }

            if (Application.isPlaying)
            {
                return;
            }

            var animator = character.GetComponent<Animator>();
            if(animator) animator.enabled = false;
            
            DestroyImmediate(character.GetComponent<RecordPositions>());
        }

        #endregion

        public void ClearData(DataManagementEnum type)
        {
            if (type is DataManagementEnum.Initial)
            {
                if (character == null && !isDatasetLoaded)
                {
                    animations = new List<AnimationClip>();

                    characteristics = new List<BoneCharacteristic>();
                    characteristicsFoldout = new List<bool>();
                    characteristicsHeight = new List<float>();
                    characteristicsSelected = new List<bool>();
                    characteristicsBonesIndex = new List<int>();

                    avatarBones = new List<AvatarBone>();
                    avatarBonesHeight = new List<float>();
                    avatarBonesFoldout = new List<bool>();
                    avatarBonesSelected = new List<bool>();
                    avatarBonesIndex = new List<int>();

                    characterChildrenBones = new List<string>();

                    actionTags = new List<ActionTagsData>();
                    actionsFoldout = new List<bool>();
                    actionsHeight = new List<float>();
                    actionsSelected = new List<bool>();

                    transitionAnims = new List<AnimFileData>();
                    transitionHeight = new List<float>();
                    transitionFoldout = new List<bool>();
                    transitionSelected = new List<bool>();

                    idleAnims = new List<AnimFileData>();
                    idleHeight = new List<float>();
                    idleFoldout = new List<bool>();
                    idleSelected = new List<bool>();

                    actionTagsFoldout = true;
                    idleTagFoldout = true;
                    styleTags = new List<string>();

                    animationsHeight = new List<float>();
                    animationsFiltered = new List<bool>();

                    ResetAnimationInterval();
                }

                //Graph
                graph = CreateInstance(typeof(TimelineGraph)) as TimelineGraph;
                nMotions = motionTags.Count;
                col = Color.clear;
                wantsMouseMove = true; // Enables the MouseMove event
                drawHighlight = false;
                datasetToEdit = null;

                _innerWindows?.ForEach(x => x.ClearData(type));

                intervalStart = 0;
                intervalEnd = 0;

                if (motions != null && motions.Count < nMotions)
                {
                    ResetAnimationInterval();
                }
            }

            if (type is DataManagementEnum.Reinitialize)
            {
                animations ??= new List<AnimationClip>();
                nMotions = motionTags.Count;
                styleTags = new();
                drawHighlight = false;
                _initialized = true;
            }

            if (type is DataManagementEnum.Complete)
            {
                character = null;
                root = null;
                avatar = null;
                poseStep = 0.05f;
                recordVelocity = 20;
                futureEstimates = 3;
                futureEstimatesTime = 1;
                pastEstimates = 1;
                pastEstimatesTime = 0.1f;
                animationDatabaseName = "";
                isDatasetLoaded = false;
                disableRootAndAvatar = true;
                isCharacterInitialized = false;
                anim = null;
                ClearData(DataManagementEnum.Initial);
                ClearData(DataManagementEnum.Reinitialize);
            }

            Repaint();
        }
    }
}
