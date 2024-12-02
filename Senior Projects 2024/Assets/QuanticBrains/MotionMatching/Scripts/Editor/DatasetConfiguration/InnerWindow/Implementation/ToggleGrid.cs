using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace QuanticBrains.MotionMatching.Scripts.Editor.DatasetConfiguration.InnerWindow.Implementation
{
    public class ToggleGrid
    {
        public bool[] PressedOptions { get; private set; }
        private readonly string[] _optionsName;

        private readonly int _xCount;
        private readonly int _rows;

        private readonly float _buttonWidth;
        private readonly float _buttonHeight;


        public ToggleGrid(List<string> options, int xCount, float buttonWidth, float buttonHeight)
        {
            PressedOptions = new bool[options.Count];
            _optionsName = options.ToArray();
            _xCount = xCount;
            _rows = (int)Math.Ceiling(options.Count / (float)xCount);

            if (_xCount > options.Count) _xCount = options.Count;

            _buttonWidth = buttonWidth;
            _buttonHeight = buttonHeight;
        }

        public void DrawToggleGrid()
        {
            EditorGUILayout.BeginVertical();

            for (int row = 0; row < _rows; row++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int col = 0; col < _xCount; col++)
                {
                    var index = row * _xCount + col;
                    if (index >= _optionsName.Length) break;
                
                    PressedOptions[index] = GUILayout.Toggle(PressedOptions[index], _optionsName[index],
                        "Button", GUILayout.Width(_buttonWidth), GUILayout.Height(_buttonHeight));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }
    }
}
