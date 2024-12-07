namespace TobyFredson
{
	using UnityEngine;
	using UnityEditor;

	[CustomEditor(typeof(TobyGlobalShadersController))]
	public class TobyGlobalShadersController_Editor : Editor

	{
		Texture2D HFGUI_GrassifyHeader;

		    SerializedProperty windType;
		    SerializedProperty windStrength;
			SerializedProperty WindSpeed;
		    SerializedProperty season;

	
		void OnEnable()
		{
			HFGUI_GrassifyHeader = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Toby Fredson/The Toby Foliage Engine/(TTFE)_Core/Resources/(TTFE) GLOBAL CONTROLLER/Scripts/Editor/TTFE_Graphic Editor.png");

			windType = serializedObject.FindProperty("windType");
			windStrength = serializedObject.FindProperty("windStrength");
			WindSpeed = serializedObject.FindProperty("windSpeed");
			season = serializedObject.FindProperty("season");
		}
		public override void OnInspectorGUI()
		{

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(HFGUI_GrassifyHeader, GUILayout.Width(192), GUILayout.Height(96));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.LabelField("The Toby Foliage Engine version 1.0.0", EditorStyles.centeredGreyMiniLabel);
			EditorGUILayout.LabelField("Global Wind Parameters", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(windType);
			EditorGUILayout.PropertyField(windStrength);
			EditorGUILayout.PropertyField(WindSpeed);
			EditorGUILayout.PropertyField(season);
			EditorGUILayout.EndVertical();
			EditorGUILayout.HelpBox("Choose between two types of wind phases. You can choose only one type simultaneously as they are fundamentally different.", MessageType.Info);

			serializedObject.ApplyModifiedProperties();
		}
	}
}