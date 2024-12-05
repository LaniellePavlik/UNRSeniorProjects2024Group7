using QuanticBrains.MotionMatching.Scripts.Containers.CustomAvatars;
using QuanticBrains.MotionMatching.Scripts.Containers.ExclusionMasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace QuanticBrains.MotionMatching.Scripts.Editor.ExclusionMaskEditors
{
    [CustomEditor(typeof(GenericExclusionMask))]
    public class GenericExclusionMaskEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var disableRootMotion = serializedObject.FindProperty("disableRootMotion");
            var avatar = serializedObject.FindProperty("genericAvatar");
            var bonesToExclude = serializedObject.FindProperty("bonesToExclude");
            
            VisualElement root = new VisualElement();
            var avatarField = new ObjectField("Avatar")
            {
                objectType = typeof(GenericAvatar)
            };
            
            avatarField.RegisterValueChangedCallback(_ =>
            {
                CreateInspectorGUI();
            });
            avatarField.BindProperty(avatar); 
            root.Add(avatarField);

            var rootMotionToggle = new Toggle("Disable Root Motion");
            rootMotionToggle.BindProperty(disableRootMotion);
            root.Add(rootMotionToggle);
            
            if (avatar.objectReferenceValue == null)
            {
                return root;
            }

            Label labelTitle = new Label("Bones to exclude")
            {
                style = { fontSize = 14 }
            };
            
            root.Add(labelTitle);
            var genericAvatar = (GenericAvatar)avatar.objectReferenceValue;
            for (int i = 0; i < genericAvatar.Length; i++)
            {
                Box toggleBox = new Box()
                {
                    style =
                    {
                        width = Length.Percent(100),
                        display = DisplayStyle.Flex,
                        flexDirection = FlexDirection.Row
                    }
                };
                
                var toggle = new Toggle();

                if (bonesToExclude.arraySize <= i)
                {
                    bonesToExclude.InsertArrayElementAtIndex(i);
                }

                toggle.value = bonesToExclude.GetArrayElementAtIndex(i).boolValue;
                var i1 = i;
                toggle.RegisterValueChangedCallback(evt =>
                {
                    bonesToExclude.GetArrayElementAtIndex(i1).boolValue = evt.newValue;
                    serializedObject.ApplyModifiedProperties();
                });
                Color backgroundLight = new Color32(100, 100, 100, 255);
                Color backgroundDark = new Color32(64, 64, 64, 255);
                Label label = new Label(genericAvatar.GetAvatarDefinition()[i].alias)
                {
                    style =
                    {
                        width = Length.Percent(70),
                        backgroundColor = i % 2 == 0 ? backgroundLight : backgroundDark
                    }
                };
                
                toggleBox.Add(label);
                toggleBox.Add(toggle);
                root.Add(toggleBox);
            }
            serializedObject.ApplyModifiedProperties();
            return root;
        }

        private void OnValidate()
        {
            var avatar = serializedObject.FindProperty("genericAvatar");
            var bonesToExclude = serializedObject.FindProperty("bonesToExclude");

            if (avatar.objectReferenceValue == null) return;

            if (bonesToExclude.arraySize != ((GenericAvatar)avatar.objectReferenceValue).Length)
            {
                bonesToExclude.ClearArray();
            }
        }
    }
}
