using System;
using System.Collections.Generic;
using System.Linq;
using QuanticBrains.MotionMatching.Scripts.Containers.CustomAvatars;
using QuanticBrains.MotionMatching.Scripts.Containers.ExclusionMasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace QuanticBrains.MotionMatching.Scripts.Editor.ExclusionMaskEditors
{
    [CustomEditor(typeof(HumanoidExclusionMask))]
    public class HumanoidExclusionMaskEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var disableRootMotion = serializedObject.FindProperty("disableRootMotion");
            var bonesToExclude = serializedObject.FindProperty("bonesToExclude");
            
            VisualElement root = new VisualElement();
            
            var rootMotionToggle = new Toggle("Disable Root Motion");
            rootMotionToggle.BindProperty(disableRootMotion);
            root.Add(rootMotionToggle);
            
            Label labelTitle = new Label("Bones to exclude")
            {
                style = { fontSize = 14 }
            };
            
            root.Add(labelTitle);

            IEnumerable<HumanBodyBones> values = Enum.GetValues(typeof(HumanBodyBones)).Cast<HumanBodyBones>();
            foreach (var (boneValue, i) in values.Select((value, i) => (value, i)))
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
                Label label = new Label(boneValue.ToString())
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
    }
}
