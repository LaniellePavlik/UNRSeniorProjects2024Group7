using System.Collections.Generic;
using QuanticBrains.MotionMatching.Scripts.CustomAttributes;
using UnityEditor;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Editor
{
    [CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
    public class ShowOnlyDrawer : PropertyDrawer {

        public override void OnGUI (
            Rect position,
            SerializedProperty prop,
            GUIContent label
        ) {

            var valueString = ManageElement(prop);
            
            EditorGUI.LabelField(position, label.text, valueString);
        }

        private string ManageElement(SerializedProperty property)
        {
            return property.propertyType switch
            {
                SerializedPropertyType.Boolean => property.boolValue.ToString(),
                SerializedPropertyType.Integer => property.intValue.ToString(),
                SerializedPropertyType.Float => property.floatValue.ToString(),
                SerializedPropertyType.String => property.stringValue,
                SerializedPropertyType.ObjectReference => ManageObject(property.objectReferenceValue),
                _ => "( Not Supported )"
            };
        }
        
        private string ManageObject(Object currentObject)
        {
            if (currentObject is Avatar)
            {
                return currentObject.name;
            }
            
            if (currentObject is ScriptableObject)
            {
                return currentObject.name;
            }
            return "( Not Supported )";
        }
    }
}
