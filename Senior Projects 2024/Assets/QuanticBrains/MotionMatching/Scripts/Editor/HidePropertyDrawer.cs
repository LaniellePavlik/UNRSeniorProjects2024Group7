using UnityEditor;

namespace QuanticBrains.MotionMatching.Scripts.Editor
{
    [CustomEditor(typeof(MotionMatching))]
    public class HidePropertyDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
