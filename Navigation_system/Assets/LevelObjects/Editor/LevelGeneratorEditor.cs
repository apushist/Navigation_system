using UnityEditor;
using UnityEngine;

namespace LevelObjects.Editor
{
	[CustomEditor(typeof(HexLevelGenerator))]
	public class LevelGeneratorEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			HexLevelGenerator generator = (HexLevelGenerator)target;

			GUILayout.Space(10); 

			if (GUILayout.Button("Generate Level", GUILayout.Height(40)))
			{
				generator.Generate();
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(generator.gameObject.scene);
			}

			if (GUILayout.Button("Clear Level"))
			{
				generator.Clear(); 
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(generator.gameObject.scene);
			}

			GUILayout.Space(10);

			if (GUILayout.Button("Open Level Painter", GUILayout.Height(30)))
			{
				LevelPainterWindow.Open(generator);
			}

			GUILayout.Space(10);
			EditorGUILayout.LabelField("Save/Load Layout", EditorStyles.boldLabel);

			generator.layoutName = EditorGUILayout.TextField("Layout Name", generator.layoutName);

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Save Layout"))
			{
				generator.SaveLayout();
			}

			if (GUILayout.Button("Load Layout"))
			{
				if (UnityEditor.EditorUtility.DisplayDialog("Load Layout",
					"This will replace current obstacles. Continue?", "Yes", "No"))
				{
					generator.LoadLayout();
				}
			}
			EditorGUILayout.EndHorizontal();
		}
	}
}