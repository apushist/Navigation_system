using FuzzyLogic;
using FuzzyLogic.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Vehicle.Control
{
    /// <summary>
    /// В Start() настраивает нечеткую систему: SpeedFuzzySystem и TurnFuzzySystem.
    /// В Update():
    ///		Берет данные из CarSensors.
    ///		Вычисляет угол до цели (Vector3.SignedAngle).
    ///		Запускает FuzzyInferenceSystem.Calculate().
    ///		Передает результаты в CarMotor.
    ///
    /// Автономный агент. Создаёт два FIS (speed и turn) и в Update читает SensorArray и цель.
    /// Требует на объекте компонентов:
    /// - SensorArray
    /// - Vehicle.CarMovementController
    /// Настраиваемые поля: target (Transform), arriveRadius, safetyDistance.
    /// </summary>
    [RequireComponent(typeof(CarMovementController), typeof(SensorArray))]
    public class AutonomousAgent : MonoBehaviour
    {
        [Header("Fuzzy Logic Assets")]
        public FuzzyInferenceSystemSO speedFisAsset;
        public FuzzyInferenceSystemSO turnFisAsset;

        [Header("References")]
        public SensorArray sensors;
        public Transform target;

        [Header("Behavior")]
        public float arriveRadius = 1f;
        public float safetyDistance = 0.6f; // дистанция до препятствий, которую хотим держать

        private CarMovementController _motor;
        private FuzzyInferenceSystem speedFIS;
        private FuzzyInferenceSystem turnFIS;

        void Start()
        {
            _motor = GetComponent<CarMovementController>();
            if (sensors == null) sensors = GetComponentInChildren<SensorArray>();
            if (sensors == null) Debug.LogError("Missing SensorArray class.");

            speedFIS = BuildFisFromAsset(speedFisAsset);
            turnFIS = BuildFisFromAsset(turnFisAsset);
        }

        void FixedUpdate()
        {
            if (target == null || speedFIS == null || turnFIS == null)
            {
                if(target == null) Debug.LogError("Missing target object");
                _motor.HardStop();
                return;
            }

            float[] dists = sensors.GetDistances();
            float maxSensor = sensors.MaxSensorDist();

            // compute angle to target in degrees (signed: left negative, right positive)
            Vector3 toTarget = (target.position - transform.position);
            Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);
            float distanceToTarget = toTargetXZ.magnitude;
            float signedAngle = Vector3.SignedAngle(transform.forward, toTargetXZ.normalized, Vector3.up); // -180..180

            // prepare inputs for FIS
            float frontDist = maxSensor;
            float leftDist = maxSensor;
            float rightDist = maxSensor;
            int n = dists.Length;
            if (n > 0)
            {
                int mid = n / 2;
                frontDist = dists[mid];
                // leftmost and rightmost with bounds checking
                rightDist = dists[n - 1];
                leftDist = dists[0];
            }

            // Speed input dictionary
            var speedInputs = new Dictionary<string, float>()
            {
                { "FrontDist", frontDist },
                { "AngleAbs", Mathf.Abs(signedAngle) }, // 0..180
                { "TargetDist", distanceToTarget }
            };

            float speedOut = speedFIS.Calculate(speedInputs); // expected 0..1
            float throttle = Mathf.Clamp01(speedOut);

            // Turn inputs
            var turnInputs = new Dictionary<string, float>()
            {
                { "LeftDist", leftDist },
                { "RightDist", rightDist },
                { "FrontDist", frontDist },
                { "Angle", signedAngle } // -180..180
            };

            float turnOut = turnFIS.Calculate(turnInputs); // expected -1..1 steering
            float steering = Mathf.Clamp(turnOut, -1f, 1f);

            if (distanceToTarget <= arriveRadius)
            {
                _motor.Move(0f, 0f);
                return;
            }

            if (frontDist < safetyDistance)
            {
                throttle = 0f;
            }

            _motor.Move(throttle, steering);
        }
        
        /// <summary>
        /// Builds a runtime FuzzyInferenceSystem from a ScriptableObject asset.
        /// </summary>
		private FuzzyInferenceSystem BuildFisFromAsset(FuzzyInferenceSystemSO asset)
		{
			if (asset == null) return null;

			var fis = new FuzzyInferenceSystem();

			// Inputs
			foreach (var varDef in asset.InputVariables)
			{
				var fv = new FuzzyVariable(varDef.Name, varDef.Min, varDef.Max);
				foreach (var setDef in varDef.Sets)
				{
					fv.AddSet(new FuzzySet(setDef.Name, setDef.Curve));
				}
				fis.AddInput(fv);
			}

			// Output
			var outDef = asset.OutputVariable;
			var outFv = new FuzzyVariable(outDef.Name, outDef.Min, outDef.Max);
			foreach (var setDef in outDef.Sets)
			{
				outFv.AddSet(new FuzzySet(setDef.Name, setDef.Curve));
			}
			fis.SetOutput(outFv);

			foreach (var group in asset.RuleGroups)
			{
				foreach (var ruleDef in group.Rules)
				{
					var rule = new FuzzyRule(ruleDef.Name, ruleDef.ConsequentSetName, ruleDef.Weight);
                    
					foreach (var ant in ruleDef.Antecedents)
					{
						rule.AddAntecedent(ant.VariableName, ant.SetName);
					}
					fis.AddRule(rule);
				}
			}

			return fis;
		}
    }
}
