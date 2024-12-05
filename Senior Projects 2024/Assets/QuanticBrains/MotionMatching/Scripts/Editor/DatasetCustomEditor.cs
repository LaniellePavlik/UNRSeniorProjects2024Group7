using QuanticBrains.MotionMatching.Scripts.Containers;
using UnityEditor;

namespace QuanticBrains.MotionMatching.Scripts.Editor
{
    /// <summary>
    /// This custom editor implementation for the Dataset class will enable the display of the information inside the
    /// ScriptableObject containing the dataset. 
    /// </summary>
    [CustomEditor(typeof(Dataset), true)]
    public class DatasetCustomEditor : UnityEditor.Editor
    {
        private Dataset _target;
        private string _tagNames;
        
        public void OnEnable()
        {
            if (target == null)
            {
                return;
            }
            _target = (Dataset)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("This dataset contains " + _target.animationsData.Count + " animations.", MessageType.None);
            //EditorGUILayout.HelpBox("This dataset contains " + _target.featuresData.Count + " poses.", MessageType.None);
        }
    }
}
