using UnityEditor;
using UnityEngine;

namespace LevelObjects.Editor
{
	public class LevelPainterWindow : EditorWindow
	{
		private HexLevelGenerator _generator;
		private bool _isPainting;

		public static void Open(HexLevelGenerator generator)
		{
			LevelPainterWindow window = GetWindow<LevelPainterWindow>("Level Painter");
			window._generator = generator;
			window.Show();
		}

		private void OnEnable()
		{
			SceneView.duringSceneGui += OnSceneGUI;
		}

		private void OnDisable()
		{
			SceneView.duringSceneGui -= OnSceneGUI;
		}

		private void OnGUI()
		{
			GUILayout.Label("Level Painter Tool", EditorStyles.boldLabel);

			if (_generator == null)
			{
				GUILayout.Label("No generator selected.");
				return;
			}

			EditorGUILayout.HelpBox(
				"Controls:\n" +
				"LMB (Left Click): Paint Obstacle\n" +
				"RMB (Right Click): Erase Obstacle\n\n" +
				"Border walls are protected automatically.\n" +
				"Parameters from HexLevelGenerator are used.", 
				MessageType.Info);

			if (GUILayout.Button("Close"))
			{
				Close();
			}
		}

		private void OnSceneGUI(SceneView sceneView)
		{
			if (_generator == null) return;

			Event e = Event.current;
			int controlID = GUIUtility.GetControlID(FocusType.Passive);

			// Перехватываем ввод, чтобы не выделять объекты в сцене при рисовании
			if (e.type == EventType.Layout)
			{
				HandleUtility.AddDefaultControl(controlID);
			}

			// Определяем плоскость рисования (на уровне Y генератора)
			Plane plane = new Plane(Vector3.up, new Vector3(0, _generator.transform.position.y, 0));
			Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

			if (plane.Raycast(ray, out float enter))
			{
				Vector3 hitPoint = ray.GetPoint(enter);

				// Рисуем маркер кисти для наглядности
				Handles.color = new Color(0, 1, 0, 0.5f);
				Handles.DrawWireDisc(hitPoint, Vector3.up, _generator.hexRadius);
				
				// Обработка кликов
				if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag))
				{
					if (e.button == 0) // ЛКМ - Рисование
					{
						_generator.TryPaintObstacle(hitPoint);
						e.Use(); // Поглощаем событие, чтобы не работали стандартные инструменты Unity
					}
					else if (e.button == 1) // ПКМ - Стирание
					{
						_generator.TryEraseObstacle(hitPoint);
						e.Use();
					}
				}
			}
			
			// Обновляем SceneView, чтобы видеть изменения сразу
			if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag)
			{
				sceneView.Repaint();
			}
		}
	}
}