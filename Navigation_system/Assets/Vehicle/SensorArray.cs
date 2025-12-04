using UnityEngine;

namespace Vehicle
{
    /// <summary>
    /// Пускает 3-5 лучей (Physics.Raycast).
    /// Возвращает дистанции до препятствий. Если препятствия нет — возвращает MaxSensorDist.
    /// </summary>
    public class SensorArray : MonoBehaviour
    {
        [Header("Sensor Settings")]
        [Range(3, 10)]
        [SerializeField] private int sensorCount = 5;

        [Tooltip("Максимальная дистанция сенсоров (если нет препятствий — вернётся это значение)")]
        [SerializeField] private float maxSensorDist = 10f;

        [Tooltip("Общий угол веера датчиков")]
        [SerializeField] private float spreadAngle = 60f;

        [Tooltip("Смещение точки запуска лучей от центра объекта")]
        [SerializeField] private Vector3 sensorOriginOffset = new Vector3(0, 0.5f, 0.5f);

        private float[] _distances;

        internal float MaxSensorDist() => maxSensorDist;

        void Awake()
        {
            _distances = new float[sensorCount];
        }

        void Update()
        {
            UpdateSensors();
        }

        /// <summary>
        /// Возвращает последнюю измеренную дистанцию для всех сенсоров.
        /// </summary>
        public float[] GetDistances()
        {
            return _distances;
        }

        /// <summary>
        /// Рэйкасты.
        /// </summary>
        private void UpdateSensors()
        {
            Vector3 origin = transform.TransformPoint(sensorOriginOffset);

            // Центральная ось
            float half = spreadAngle * 0.5f;

            for (int i = 0; i < sensorCount; i++)
            {
                // Нормализуем позицию сенсора от -1 до 1
                float t = Mathf.Lerp(-1f, 1f, i / (float)(sensorCount - 1));

                float angle = t * half;

                // Поворачиваем forward на angle градусов
                Quaternion rot = Quaternion.Euler(0, angle, 0);
                Vector3 dir = rot * transform.forward;

                if (Physics.Raycast(origin, dir, out RaycastHit hit, maxSensorDist))
                {
                    _distances[i] = hit.distance;
                }
                else
                {
                    _distances[i] = maxSensorDist;
                }

#if UNITY_EDITOR
                Debug.DrawRay(origin, dir * _distances[i], Color.green);
                if (_distances[i] >= maxSensorDist - 0.01f)
                    Debug.DrawRay(origin, dir * maxSensorDist, Color.gray);
#endif
            }
        }
    }
}
