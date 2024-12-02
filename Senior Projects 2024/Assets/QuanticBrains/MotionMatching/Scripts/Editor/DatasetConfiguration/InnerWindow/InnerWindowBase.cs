using System;
using UnityEditor;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Editor.DatasetConfiguration.InnerWindow
{
    [Serializable]
    public class InnerWindowBase
    {
        public readonly string WindowName;
        
        protected readonly DatasetSetup DatasetSetup;
        private Rect _baseRect;
        
        protected const int ColumnWidth = 300;
        protected const int ColumnSpace = 20;

        public readonly int BoxWidth;
        public readonly int ScrollWidth;

        protected InnerWindowBase(string name, DatasetSetup datasetSetup)
        {
            WindowName = name;
            DatasetSetup = datasetSetup;
            
            BoxWidth = ColumnWidth * 2 + ColumnSpace * 2;
            ScrollWidth = ColumnWidth * 2 + ColumnSpace * 3;
            
            ClearData(DataManagementEnum.Initial);
        }

        public virtual void OnDrawWindow()
        {
            EditorGUILayout.LabelField("This is a inner window test -> " + WindowName , EditorStyles.boldLabel, GUILayout.Width(ColumnWidth));
        }
        
        public virtual void ClearData(DataManagementEnum type){}
    }
}
