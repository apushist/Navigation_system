using UnityEditor;
using UnityEngine;

namespace LevelObjects
{
	[CustomEditor(typeof(HexLevelGenerator))]
	public class LevelGeneratorEditor : Editor
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
			}
		}
	}
}