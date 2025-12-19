using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace LevelObjects
{
	public class HexLevelGenerator : MonoBehaviour
	{
		[Header("Grid Settings")]
		public int gridWidth = 20;
		public int gridHeight = 20;
    
		[Header("Hex Settings")]
		public float hexRadius = 2f;
		public float gap = 0.05f;

		[Header("Generation Logic")]
		public float noiseScale = 0.15f;
		[Range(0, 1)] public float fillThreshold = 0.4f;
		public float seed = 0;

		[Header("References")]
		public GameObject obstaclePrefab;
		public GameObject wallPrefab;
		public Transform carTransform;
		public Transform targetTransform;

		[Header("Save/Load")]
		public string layoutName = "MyLayout";
		private List<Vector2Int> _paintedPositions = new List<Vector2Int>();

		[SerializeField] // Чтобы Unity не теряла список при перезагрузках скриптов, но поле остается приватным
		private List<GameObject> _spawnedHexes = new List<GameObject>();

		private void Start()
		{
			//Generate();
		}

		private void Update()
		{
			/*if (Input.GetKeyDown(KeyCode.Space))
			{
				seed = Random.Range(0f, 10000f);
				Generate();
			}*/
		}

		[ContextMenu("Generate Hex Grid")]
		public void Generate()
		{
			Clear();
			
			float r = hexRadius; 
        
			float xOffset = Mathf.Sqrt(3) * r + gap; 
			float zOffset = 1.5f * r + gap;

			float mapPixelWidth = gridWidth * xOffset;
			float mapPixelHeight = gridHeight * zOffset;
			
			Vector3 startPos = new Vector3(-mapPixelWidth / 2f, 0, -mapPixelHeight / 2f);
			PlaceCarAndTarget(startPos, mapPixelWidth, mapPixelHeight);

			GenerateWalls(startPos, xOffset, zOffset);

			for (int x = 1; x < gridWidth - 1; x++)
			{
				for (int z = 1; z < gridHeight - 1; z++)
				{
					Vector3 worldPos = CalculateWorldPosition(x, z, startPos, xOffset, zOffset);

					float noiseVal = Mathf.PerlinNoise((x + seed) * noiseScale, (z + seed) * noiseScale);

					if (noiseVal > (1f - fillThreshold))
					{
						if (!IsSafeZone(worldPos))
						{
							CreateHexPrism(worldPos, GetRandomObstaclePrefab());
						}
					}
				}
			}

			Combine();
		}

		/// <summary>
		/// Генерирует стены по периметру игрового поля
		/// </summary>
		private void GenerateWalls(Vector3 startPos, float xOffset, float zOffset)
		{
			for (int x = 0; x < gridWidth; x++)
			{
				for (int z = 0; z < gridHeight; z++)
				{
					bool isBorder = (x == 0 || x == gridWidth - 1 || z == 0 || z == gridHeight - 1);

					if (isBorder)
					{
						Vector3 worldPos = CalculateWorldPosition(x, z, startPos, xOffset, zOffset);

						GameObject prefabToUse = wallPrefab != null ? wallPrefab : GetRandomObstaclePrefab();
						CreateHexPrism(worldPos, prefabToUse);
					}
				}
			}
		}

		private Vector3 CalculateWorldPosition(int x, int z, Vector3 startPos, float xOffset, float zOffset)
		{
			float xPos = x * xOffset;
			float zPos = z * zOffset;

			if (z % 2 == 1)
			{
				xPos += xOffset / 2f;
			}

			return startPos + new Vector3(xPos, 0, zPos);
		}

		private void CreateHexPrism(Vector3 pos, GameObject prefab)
		{
			if (prefab == null) return;

			GameObject newHexPrism = Instantiate(prefab, pos, Quaternion.identity);
			newHexPrism.transform.localScale = new Vector3(hexRadius, newHexPrism.transform.localScale.y, hexRadius);
			
			newHexPrism.transform.SetParent(this.transform);
			_spawnedHexes.Add(newHexPrism);
		}

		private bool IsSafeZone(Vector3 pos)
		{
			if (carTransform && Vector3.Distance(pos, carTransform.position) < 6f) 
				return true;
			if (targetTransform && Vector3.Distance(pos, targetTransform.position) < 6f) 
				return true;
			return false;
		}

		private void PlaceCarAndTarget(Vector3 mapStart, float w, float h)
		{
			if (carTransform)
			{
				if (carTransform.TryGetComponent<Rigidbody>(out var rb)) 
					rb.linearVelocity = Vector3.zero; 
			}
		}

		public void Clear()
		{
			for (int i = _spawnedHexes.Count - 1; i >= 0; i--)
			{
				var obj = _spawnedHexes[i];
				if (obj != null) 
				{
					if (Application.isPlaying) Destroy(obj);
					else DestroyImmediate(obj);
				}
			}
			_spawnedHexes.Clear();

			// Удаляем оставшихся детей, если они есть (на всякий случай)
			for (int i = transform.childCount - 1; i >= 0; i--)
			{
				Transform child = transform.GetChild(i);
				if (Application.isPlaying) Destroy(child.gameObject);
				else DestroyImmediate(child.gameObject);
			}

			var m = GetComponent<MeshFilter>();
            if (m) m.mesh = null;//очистка созданного меша
		}

		// === Методы для Level Painter ===

		public void TryPaintObstacle(Vector3 hitPoint)
		{
			Vector2Int gridPos = GetClosestGridCoordinate(hitPoint);
			if (gridPos.x == -1) return; // Невалидная координата

			// Проверка на границы (стены нельзя закрашивать или менять)
			if (gridPos.x == 0 || gridPos.x == gridWidth - 1 || gridPos.y == 0 || gridPos.y == gridHeight - 1)
				return;

			// Получаем точную позицию центра гекса
			float r = hexRadius;
			float xOffset = Mathf.Sqrt(3) * r + gap;
			float zOffset = 1.5f * r + gap;
			float mapPixelWidth = gridWidth * xOffset;
			float mapPixelHeight = gridHeight * zOffset;
			Vector3 startPos = new Vector3(-mapPixelWidth / 2f, 0, -mapPixelHeight / 2f);
			
			Vector3 targetPos = CalculateWorldPosition(gridPos.x, gridPos.y, startPos, xOffset, zOffset);

			// Проверка на SafeZone
			if (IsSafeZone(targetPos)) return;

			// Проверка: занята ли клетка
			if (IsOccupied(targetPos)) return;

			// Создаем препятствие
			CreateHexPrism(targetPos, GetRandomObstaclePrefab());
		}

		public void TryEraseObstacle(Vector3 hitPoint)
		{
			Vector2Int gridPos = GetClosestGridCoordinate(hitPoint);
			if (gridPos.x == -1) return;

			// Нельзя стирать стены
			if (gridPos.x == 0 || gridPos.x == gridWidth - 1 || gridPos.y == 0 || gridPos.y == gridHeight - 1)
				return;
			
			float r = hexRadius;
			float xOffset = Mathf.Sqrt(3) * r + gap;
			float zOffset = 1.5f * r + gap;
			float mapPixelWidth = gridWidth * xOffset;
			float mapPixelHeight = gridHeight * zOffset;
			Vector3 startPos = new Vector3(-mapPixelWidth / 2f, 0, -mapPixelHeight / 2f);

			Vector3 targetPos = CalculateWorldPosition(gridPos.x, gridPos.y, startPos, xOffset, zOffset);

			GameObject objToRemove = GetObjectAt(targetPos);
			if (objToRemove != null)
			{
				_spawnedHexes.Remove(objToRemove);
				DestroyImmediate(objToRemove);
			}
		}

		private Vector2Int GetClosestGridCoordinate(Vector3 hitPos)
		{
			float r = hexRadius;
			float xOffset = Mathf.Sqrt(3) * r + gap;
			float zOffset = 1.5f * r + gap;
			float mapPixelWidth = gridWidth * xOffset;
			float mapPixelHeight = gridHeight * zOffset;
			Vector3 startPos = new Vector3(-mapPixelWidth / 2f, 0, -mapPixelHeight / 2f);

			float minInfoDist = float.MaxValue;
			Vector2Int bestCoord = new Vector2Int(-1, -1);

			// Перебираем все возможные координаты (для сетки 20х20 это быстро)
			// Более оптимальный способ - математический расчет, но перебор надежнее для сохранения точности с Generate()
			for (int x = 0; x < gridWidth; x++)
			{
				for (int z = 0; z < gridHeight; z++)
				{
					Vector3 hexPos = CalculateWorldPosition(x, z, startPos, xOffset, zOffset);
					float d = Vector3.Distance(hitPos, hexPos);
					if (d < hexRadius && d < minInfoDist)
					{
						minInfoDist = d;
						bestCoord = new Vector2Int(x, z);
					}
				}
			}
			return bestCoord;
		}

		private bool IsOccupied(Vector3 pos)
		{
			return GetObjectAt(pos) != null;
		}

		private GameObject GetObjectAt(Vector3 pos)
		{
			// Ищем объект в списке _spawnedHexes, который находится близко к позиции
			// Используем небольшой порог расстояния
			foreach (var obj in _spawnedHexes)
			{
				if (obj == null) continue;
				if (Vector3.Distance(obj.transform.position, pos) < 0.1f)
				{
					return obj;
				}
			}
			return null;
		}




        // === Weighted Obstacles System ===

        [System.Serializable]
        public class WeightedPrefab
        {
            public GameObject prefab;

            [Range(0f, 1f)]
            public float weight = 1f;
        }

        [Header("Obstacle Variants")]
        [SerializeField]
        private List<WeightedPrefab> obstaclePrefabs = new List<WeightedPrefab>();

		[SerializeField] bool combineAfterGeneration = true;

        private GameObject GetRandomObstaclePrefab()
        {
            // Если список пуст — используем старый obstaclePrefab
            if (obstaclePrefabs == null || obstaclePrefabs.Count == 0)
                return obstaclePrefab;

            float totalWeight = 0f;

            for (int i = 0; i < obstaclePrefabs.Count; i++)
            {
                if (obstaclePrefabs[i].prefab != null && obstaclePrefabs[i].weight > 0f)
                    totalWeight += obstaclePrefabs[i].weight;
            }

            // Если все веса нулевые или префабы отсутствуют
            if (totalWeight <= 0f)
                return obstaclePrefab;

            float rnd = Random.value * totalWeight;
            float cumulative = 0f;

            for (int i = 0; i < obstaclePrefabs.Count; i++)
            {
                var entry = obstaclePrefabs[i];
                if (entry.prefab == null || entry.weight <= 0f)
                    continue;

                cumulative += entry.weight;
                if (rnd <= cumulative)
                    return entry.prefab;
            }

            // Фолбэк (на всякий случай)
            return obstaclePrefab;
        }

		private void Combine()
		{
			if (!combineAfterGeneration) return;
			MeshCombiner combiner = GetComponent<MeshCombiner>();
			if (!combiner) return;
			combiner.CombineMeshes(true);
		}

		//=== Методы для Save/Load System

		public void SaveLayout(string lName = "")
		{
			if (lName == "")
			{
				lName = layoutName;
			}
			// Собираем все позиции (кроме стен)
			List<Vector2IntSerializable> positions = new List<Vector2IntSerializable>();

			foreach (GameObject obj in _spawnedHexes)
			{
				if (obj == null) continue;

				// Находим позицию в сетке
				Vector2Int gridPos = GetClosestGridCoordinate(obj.transform.position);
				if (gridPos.x == -1) continue;

				// Пропускаем стены
				if (gridPos.x == 0 || gridPos.x == gridWidth - 1 ||
					gridPos.y == 0 || gridPos.y == gridHeight - 1)
					continue;

				positions.Add(new Vector2IntSerializable(gridPos.x, gridPos.y));
			}

			// Создаем и сохраняем
			LevelLayout layout = new LevelLayout
			{
				obstaclePositions = positions.ToArray(),
				carPosition = carTransform ? new Vector3Serializable(
					carTransform.position.x,
					carTransform.position.y,
					carTransform.position.z) : new Vector3Serializable(0, 0, 0),
				carRotation = carTransform ? new Vector4Serializable(
					carTransform.rotation.x,
					carTransform.rotation.y,
					carTransform.rotation.z,
					carTransform.rotation.w) : new Vector4Serializable(0, 0, 0, 1),
				targetPosition = targetTransform ? new Vector3Serializable(
					targetTransform.position.x,
					targetTransform.position.y,
					targetTransform.position.z) : new Vector3Serializable(0, 0, 0)
			};

			string json = JsonUtility.ToJson(layout, true);
			string folderPath = Application.dataPath + "/LevelObjects/LevelLayouts/";

			if (!System.IO.Directory.Exists(folderPath))
				System.IO.Directory.CreateDirectory(folderPath);

			string filePath = folderPath + lName + ".json";
			System.IO.File.WriteAllText(filePath, json);

			Debug.Log($"Saved layout: {filePath} ({positions.Count} obstacles)" +
			  (carTransform ? $", Car: {carTransform.position}" : "") +
			  (targetTransform ? $", Target: {targetTransform.position}" : ""));
		}

		private string GetLevelFilePath(string fileName)
		{
			string fileNameWithExt = fileName + ".json";

			// Сначала проверяем в StreamingAssets (для билда)
			string streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, "LevelLayouts", fileNameWithExt);

			// В редакторе используем старый путь для удобства
#if UNITY_EDITOR
			string editorPath = Application.dataPath + "/LevelObjects/LevelLayouts/" + fileNameWithExt;
			if (File.Exists(editorPath))
				return editorPath;
#endif


			return streamingAssetsPath;

		}

		// Загрузить сохраненный уровень
		public void LoadLayout(string lName = "")
		{
			Clear();
			if (lName == "")
			{
				lName = layoutName;
			}

			string filePath = GetLevelFilePath(lName);

			if (!System.IO.File.Exists(filePath))
			{
				Debug.LogWarning($"Layout not found: {filePath}");
				return;
			}

			string json = System.IO.File.ReadAllText(filePath);
			LevelLayout layout = JsonUtility.FromJson<LevelLayout>(json);

			if (carTransform)
			{
				carTransform.position = layout.carPosition;
				carTransform.rotation = layout.carRotation;

				if (carTransform.TryGetComponent<Rigidbody>(out var rb))
				{
					rb.linearVelocity = Vector3.zero;
					rb.angularVelocity = Vector3.zero;
				}


			}

			if (targetTransform)
			{
				targetTransform.position = layout.targetPosition;
			}

			float r = hexRadius;
			float xOffset = Mathf.Sqrt(3) * r + gap;
			float zOffset = 1.5f * r + gap;
			float mapWidth = gridWidth * xOffset;
			float mapHeight = gridHeight * zOffset;
			Vector3 startPos = new Vector3(-mapWidth / 2f, 0, -mapHeight / 2f);

			GenerateWalls(startPos, xOffset, zOffset);

			foreach (var pos in layout.obstaclePositions)
			{
				Vector2Int gridPos = new Vector2Int(pos.x, pos.y);
				Vector3 worldPos = CalculateWorldPosition(gridPos.x, gridPos.y, startPos, xOffset, zOffset);

				if (!IsSafeZone(worldPos))
				{
					CreateHexPrism(worldPos, GetRandomObstaclePrefab());
				}
			}

			Combine();//opti


			Debug.Log($"Loaded layout: {layoutName} ({layout.obstaclePositions.Length} obstacles)" +
					  (carTransform ? $", Car: {carTransform.position}" : "") +
					  (targetTransform ? $", Target: {targetTransform.position}" : ""));
		}

		// Вспомогательный метод для конвертации
		private Vector3 GetWorldPositionFromGrid(Vector2Int gridPos)
		{
			float xOffset = Mathf.Sqrt(3) * hexRadius + gap;
			float zOffset = 1.5f * hexRadius + gap;
			float mapWidth = gridWidth * xOffset;
			float mapHeight = gridHeight * zOffset;
			Vector3 startPos = new Vector3(-mapWidth / 2f, 0, -mapHeight / 2f);

			return CalculateWorldPosition(gridPos.x, gridPos.y, startPos, xOffset, zOffset);
		}


	}
}