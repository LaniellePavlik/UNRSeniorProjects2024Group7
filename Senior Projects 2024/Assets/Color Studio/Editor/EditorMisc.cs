using UnityEngine;
using UnityEditor;

namespace ColorStudio {

    public class EditorMisc {

        public static void HighlightAsset(Object obj) {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }

    }
}
