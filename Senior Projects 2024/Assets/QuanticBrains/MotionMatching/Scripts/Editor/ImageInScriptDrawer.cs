using QuanticBrains.MotionMatching.Scripts.CustomAttributes;
using UnityEditor;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Editor
{
    [CustomPropertyDrawer(typeof(ImageInScriptAttribute))]
    public class ImageInScriptDrawer : PropertyDrawer
    {
        private float _height;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (attribute is not ImageInScriptAttribute imageAttribute)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            if (string.IsNullOrEmpty(imageAttribute.imagePath)) return;
            Texture2D image = EditorGUIUtility.Load(imageAttribute.imagePath) as Texture2D;

            if (image == null) return;
            _height = image.height;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(image);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            //position.y += 64;
        }

        /*public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ImageInScriptAttribute imageAttribute = attribute as ImageInScriptAttribute;
            float heightOffset = !string.IsNullOrEmpty(imageAttribute.imagePath) ? _height + 20 : 0;
            return EditorGUI.GetPropertyHeight(property, label) + heightOffset;
        }*/
    }
}
