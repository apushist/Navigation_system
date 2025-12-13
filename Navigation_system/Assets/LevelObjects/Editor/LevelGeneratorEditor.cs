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
		}
	}
}