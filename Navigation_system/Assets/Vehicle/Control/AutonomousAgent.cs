using FuzzyLogic;
using FuzzyLogic.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Vehicle.Control
{
    [RequireComponent(typeof(CarMovementController), typeof(SensorArray))]
    [RequireComponent(typeof(Rigidbody))] // Нужен для определения вектора скорости
    public class AutonomousAgent : MonoBehaviour
    {
        [Header("Fuzzy Logic Assets")]
        public FuzzyInferenceSystemSO speedFisAsset;
        public FuzzyInferenceSystemSO turnFisAsset;

        [Header("References")]
        public SensorArray sensors;
        public Transform target;

        [Header("Settings")]
        public float arriveRadius = 2.0f;
        public float safetyDistance = 0.6f;

        [Header("Features Toggle")]
        [Tooltip("Использовать ли задний сенсор в логике")]
        public bool useRearSensor = true;
        [Tooltip("Использовать ли гистерезис (подавление колебаний у стен)")]
        public bool useHysteresis = true;

        [Header("Hysteresis Configuration")]
        [Tooltip("Дистанция, которая считается 'стеной' для срабатывания таймера")]
        public float wallDetectionThreshold = 9.0f;
        [Tooltip("Сколько секунд ехать вдоль стены, прежде чем включится подавление")]
        public float timeToTriggerHysteresis = 6.0f;
        [Tooltip("Длительность эффекта подавления поворота")]
        public float hysteresisDuration = 2.0f;
		public float maxBackwardTime = 3f;

		[Header("UI References")]
		public Menu gameMenu;

		// Внутренние переменные
		private CarMovementController _motor;
        private Rigidbody _rb;
        private FuzzyInferenceSystem speedFIS;
        private FuzzyInferenceSystem turnFIS;

        // Таймеры
        private float _timeNearWallLeft = 0f;
        private float _timeNearWallRight = 0f;
        private float _hysteresisTimer = 0f;
		private float _backwardTimer = 0f;
		
		private bool _leftHysteresisTriggered = false;
		private bool _rightHysteresisTriggered = false;

        void Start()
        {
            _motor = GetComponent<CarMovementController>();
            _rb = GetComponent<Rigidbody>();
            
            if (sensors == null) sensors = GetComponentInChildren<SensorArray>();
            if (sensors == null) Debug.LogError("Missing SensorArray class.");

            speedFIS = BuildFisFromAsset(speedFisAsset);
            turnFIS = BuildFisFromAsset(turnFisAsset);

			_backwardTimer = maxBackwardTime;
		}

        void FixedUpdate()
		{
			float speed = _motor.GetFrontSpeed();
			if (speed > 0f && _backwardTimer != 0f)
			{
				_backwardTimer = 0f;
			}
			
            float[] dists = sensors.GetFrontDistances();
            float maxSensor = sensors.MaxSensorDist();

            // 1. Расчет угла и дистанции до цели
            Vector3 toTarget = (target.position - transform.position);
            Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);
            float distanceToTarget = toTargetXZ.magnitude;
            float signedAngle = Vector3.SignedAngle(transform.forward, toTargetXZ.normalized, Vector3.up);

            // 2. Обработка передних сенсоров (находим Min Left, Center, Min Right)
            float frontDist = maxSensor;
            float leftDist = maxSensor;
            float rightDist = maxSensor;

            int n = dists.Length;
            if (n > 0)
            {
                int mid = n / 2;
                frontDist = dists[mid];

                // Левая сторона (от 0 до mid)
                for (int i = 0; i < mid; i++)
                {
                    if (dists[i] < leftDist) leftDist = dists[i];
                }

                // Правая сторона (от mid+1 до конца)
                for (int i = mid + 1; i < n; i++)
                {
                    if (dists[i] < rightDist) rightDist = dists[i];
                }
            }

            // 3. Логика Заднего Сенсора
            float rearDistInput = maxSensor; // По умолчанию считаем, что сзади чисто (10)
            if (useRearSensor)
            {
                // Получаем локальную скорость. Z > 0 едем вперед, Z < 0 едем назад.

                // Если реально едем назад (с небольшим порогом), то учитываем датчик
                if (speed < 0f && _backwardTimer < maxBackwardTime)
                {
                    rearDistInput = sensors.GetRearDistance();
					_backwardTimer += Time.fixedDeltaTime;
                }
                // Иначе оставляем maxSensor, чтобы FIS не тормозил нас при движении вперед
            }

            // 4. Логика Гистерезиса (Damping)
            float dampingInput = 0f; // Low (0) по умолчанию
            if (useHysteresis)
            {
                // Обновляем таймеры нахождения у стен
                if (leftDist < wallDetectionThreshold) _timeNearWallLeft += Time.fixedDeltaTime;
                else _timeNearWallLeft = 0f;

                if (rightDist < wallDetectionThreshold) _timeNearWallRight += Time.fixedDeltaTime;
                else _timeNearWallRight = 0f;

                // Если едем у стены достаточно долго -> взводим таймер подавления
                if (_timeNearWallLeft > timeToTriggerHysteresis)
				{
					_leftHysteresisTriggered = true;
					
					_timeNearWallLeft = 0f;
				}
				
				if (_timeNearWallRight > timeToTriggerHysteresis)
				{
					_rightHysteresisTriggered = true;
					
					_timeNearWallRight = 0f;
				}

				if ((leftDist > wallDetectionThreshold) && (_leftHysteresisTriggered))
				{
					_hysteresisTimer = hysteresisDuration;
					_leftHysteresisTriggered = false;
				}
				
				if ((rightDist > wallDetectionThreshold) && (_rightHysteresisTriggered))
				{
					_hysteresisTimer = hysteresisDuration;
					_rightHysteresisTriggered = false;
				}

                // Вычисляем значение переменной TurningDamp
                if (_hysteresisTimer > 0f)
                {
                    _hysteresisTimer -= Time.fixedDeltaTime;
                    // Линейно от 1 до 0 (или можно кривую использовать)
                    dampingInput = Mathf.Clamp01(_hysteresisTimer / hysteresisDuration);
                }
            }

            // 5. FIS Calculation
			
			float averageDist = (frontDist + leftDist + rightDist) / 3f;
			float backwardInput = 1f - _backwardTimer / maxBackwardTime;
			float speedInput = Mathf.Clamp(speed, -1f, 1f);
			Debug.Log(speedInput);
            
            // --- Speed FIS ---
            // Теперь можно добавить RearDist в логику скорости, если нужно (например, тормозить при движении назад)
            var speedInputs = new Dictionary<string, float>()
            {
				{ "FrontDist", frontDist },
                { "MinDist", Mathf.Min(frontDist, Mathf.Min(rightDist, leftDist)) },
                { "TargetDist", distanceToTarget },
                { "RearDist", rearDistInput }, 
				{ "AverageDist", averageDist },
				
				{ "BackwardInput", backwardInput},
				{ "Speed", speedInput }
            };

            float speedOut = speedFIS.Calculate(speedInputs);
            float throttle = speedOut;
			if (Mathf.Abs(throttle) < 0.1f)
			{
				throttle = 0.5f;
			}

            // --- Turn FIS ---
            var turnInputs = new Dictionary<string, float>()
            {
                { "LeftDist", leftDist },
                { "RightDist", rightDist },
                { "FrontDist", frontDist },
                { "RearDist", rearDistInput }, // Можно добавить логику инверсии руля при заднем ходе
                { "Angle", signedAngle },
                { "TargetDist", distanceToTarget },
                { "TurningDamp", dampingInput }, // Новая переменная для гистерезиса
				{ "Speed", speedInput }
            };

            float turnOut = turnFIS.Calculate(turnInputs);
            float steering = Mathf.Clamp(turnOut, -1f, 1f);
			if (speed < 0f)
			{
				steering *= -1f;
			}

            // 6. Финальные проверки (Hard logic overrides)
            if (distanceToTarget <= arriveRadius)
            {
                _motor.Move(0f, 0f);
                gameMenu._EndScreen();
				return;
            }

            _motor.Move(throttle, steering);
        }
        
        // --- Builder Helper (без изменений) ---
        private FuzzyInferenceSystem BuildFisFromAsset(FuzzyInferenceSystemSO asset)
        {
            if (asset == null) return null;
            var fis = new FuzzyInferenceSystem();
            foreach (var varDef in asset.InputVariables)
            {
                var fv = new FuzzyVariable(varDef.Name, varDef.Min, varDef.Max);
                foreach (var setDef in varDef.Sets) fv.AddSet(new FuzzySet(setDef.Name, setDef.Curve));
                fis.AddInput(fv);
            }
            var outDef = asset.OutputVariable;
            var outFv = new FuzzyVariable(outDef.Name, outDef.Min, outDef.Max);
            foreach (var setDef in outDef.Sets) outFv.AddSet(new FuzzySet(setDef.Name, setDef.Curve));
            fis.SetOutput(outFv);
            foreach (var group in asset.RuleGroups)
            {
                foreach (var ruleDef in group.Rules)
                {
                    var rule = new FuzzyRule(ruleDef.Name, ruleDef.ConsequentSetName, ruleDef.Weight);
                    foreach (var ant in ruleDef.Antecedents) rule.AddAntecedent(ant.VariableName, ant.SetName);
                    fis.AddRule(rule);
                }
            }
            return fis;
        }
    }
}