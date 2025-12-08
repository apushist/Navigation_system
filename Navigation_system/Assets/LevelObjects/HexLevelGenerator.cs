using System.Collections.Generic;
using UnityEngine;

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
		public GameObject wallPrefab; // <-- НОВОЕ: Ссылка на префаб стены
		public Transform carTransform;
		public Transform targetTransform;

		private List<GameObject> _spawnedHexes = new List<GameObject>();

		private void Start()
		{
			//if (seed == 0) seed = Random.Range(0f, 10000f);
			Generate();
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

			for (int x = 0; x < gridWidth; x++)
			{
				for (int z = 0; z < gridHeight; z++)
				{
					// --- Проверка на границы (Стены) ---
					bool isBorder = (x == 0 || x == gridWidth - 1 || z == 0 || z == gridHeight - 1);
					
					float xPos = x * xOffset;
					float zPos = z * zOffset;

					if (z % 2 == 1)
					{
						xPos += xOffset / 2f;
					}

					Vector3 worldPos = startPos + new Vector3(xPos, 0, zPos);

					if (isBorder)
					{
						// Если это граница, спауним стену (используем wallPrefab, если он есть, иначе obstaclePrefab)
						GameObject prefabToUse = wallPrefab != null ? wallPrefab : obstaclePrefab;
						CreateHexPrism(worldPos, prefabToUse);
					}
					else
					{
						// Логика для внутренней части (препятствия)
						float noiseVal = Mathf.PerlinNoise((x + seed) * noiseScale, (z + seed) * noiseScale);

						if (noiseVal > (1f - fillThreshold))
						{
							if (!IsSafeZone(worldPos))
							{
								CreateHexPrism(worldPos, obstaclePrefab);
							}
						}
					}
				}
			}
		}

		// Метод изменен: теперь принимает prefab как аргумент
		private void CreateHexPrism(Vector3 pos, GameObject prefab)
		{
			if (prefab == null) return;

			GameObject newHexPrism = Instantiate(prefab, pos, Quaternion.identity);
			// Если нужно сохранить размер гекса для стен, оставляем как есть. 
			// Если у стен свой скейл внутри префаба, эту строку можно закомментировать для стен.
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
					rb.linearVelocity = Vector3.zero; // Обратите внимание: в Unity 6 "velocity" заменен на "linearVelocity"
			}
		}

		public void Clear()
		{
			foreach (var obj in _spawnedHexes)
			{
				if (obj != null) 
				{
					if (Application.isPlaying) Destroy(obj);
					else DestroyImmediate(obj);
				}
			}
			_spawnedHexes.Clear();

			// Дополнительная очистка детей, если список рассинхронизировался
			for (int i = transform.childCount - 1; i >= 0; i--)
			{
				Transform child = transform.GetChild(i);
				if (Application.isPlaying) Destroy(child.gameObject);
				else DestroyImmediate(child.gameObject);
			}
		}
	}
}