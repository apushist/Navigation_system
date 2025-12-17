using UnityEngine;

namespace Vehicle
{
    /// <summary>
    /// Пускает веер лучей вперед и один луч назад.
    /// </summary>
    public class SensorArray : MonoBehaviour
    {
        [Header("Front Sensors Settings")]
        [Range(3, 10)]
        [SerializeField] private int sensorCount = 5;

        [Tooltip("Максимальная дистанция сенсоров")]
        [SerializeField] private float maxSensorDist = 10f;

        [Tooltip("Общий угол веера передних датчиков")]
        [SerializeField] private float spreadAngle = 60f;

        [Tooltip("Смещение точки запуска передних лучей")]
        [SerializeField] private Vector3 sensorOriginOffset = new Vector3(0, 0.5f, 0.5f);

        [Header("Rear Sensor Settings")]
        [Tooltip("Смещение точки запуска заднего луча")]
        [SerializeField] private Vector3 rearSensorOriginOffset = new Vector3(0, 0.5f, -0.5f);

        private float[] _frontDistances;
        private float _rearDistance;

        internal float MaxSensorDist() => maxSensorDist;

        void Awake()
        {
            _frontDistances = new float[sensorCount];
            _rearDistance = maxSensorDist;
        }

        void Update()
        {
            UpdateSensors();
        }

        /// <summary>
        /// Возвращает массив дистанций передних сенсоров.
        /// </summary>
        public float[] GetFrontDistances()
        {
            return _frontDistances;
        }

        /// <summary>
        /// Возвращает дистанцию заднего сенсора.
        /// </summary>
        public float GetRearDistance()
        {
            return _rearDistance;
        }

        private void UpdateSensors()
        {
            // --- FRONT SENSORS ---
            Vector3 origin = transform.TransformPoint(sensorOriginOffset);
            float half = spreadAngle * 0.5f;

            for (int i = 0; i < sensorCount; i++)
            {
                // Нормализуем позицию сенсора от -1 до 1
                float t = Mathf.Lerp(-1f, 1f, i / (float)(sensorCount - 1));
                float angle = t * half;

                Quaternion rot = Quaternion.Euler(0, angle, 0);
                Vector3 dir = rot * transform.forward;

				bool hitted = Physics.Raycast(origin, dir, out RaycastHit hit, maxSensorDist);
                if (hitted)
                {
                    _frontDistances[i] = hit.distance;
                }
                else
                {
                    _frontDistances[i] = maxSensorDist;
                }

#if UNITY_EDITOR
                // Рисуем передние лучи
                Debug.DrawRay(origin, dir * _frontDistances[i], hitted ? Color.green : Color.aliceBlue);
#endif
            }

            // --- REAR SENSOR ---
            Vector3 rearOrigin = transform.TransformPoint(rearSensorOriginOffset);
            Vector3 rearDir = -transform.forward; // Строго назад

			bool rearHitted = Physics.Raycast(rearOrigin, rearDir, out RaycastHit rearHit, maxSensorDist);
            if (rearHitted)
            {
                _rearDistance = rearHit.distance;
            }
            else
            {
                _rearDistance = maxSensorDist;
            }

#if UNITY_EDITOR
            // Рисуем задний луч (синий)
            Debug.DrawRay(rearOrigin, rearDir * _rearDistance, rearHitted ? Color.blue : Color.aliceBlue);
#endif
        }
    }
}