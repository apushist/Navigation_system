using FuzzyLogic;
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
    [RequireComponent(typeof(CarMovementController),typeof(SensorArray))]
    public class AutonomousAgent : MonoBehaviour
    {
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

            BuildSpeedFIS();
            BuildTurnFIS();
        }

        void Update()
        {
            if (target == null)
            {
                Debug.LogError("Missing target object");
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
            // map to throttle (-1..1). We assume no reverse: throttle 0..1
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

            // If arrived, stop
            if (distanceToTarget <= arriveRadius)
            {
                // small stop and no steering
                _motor.Move(0f, 0f);
                return;
            }

            // Keep safety: if obstacle too close directly, reduce throttle
            if (frontDist < safetyDistance)
            {
                throttle = 0f;
            }

            _motor.Move(throttle, steering);
        }

        #region FIS builders

        private void BuildSpeedFIS()
        {
            speedFIS = new FuzzyInferenceSystem();

            // Input: FrontDist (0..maxSensorDist)
            float maxSensor = sensors.MaxSensorDist();
            var front = new FuzzyVariable("FrontDist", 0f, maxSensor);
            front.AddSet(FuzzySet.Trapezoid("VeryNear", 0f, 0f, 0f, 0.1f));
            front.AddSet(FuzzySet.Trapezoid("Near", 0f, 0.1f, 0.2f, 0.3f));
            front.AddSet(FuzzySet.Trapezoid("Mid", 0.2f, 0.3f, 0.5f, 0.7f));
            front.AddSet(FuzzySet.Trapezoid("Far", 0.5f, 0.7f, 1f, 1f));
            speedFIS.AddInput(front);

            // Input: AngleAbs (0..180)
            var angleAbs = new FuzzyVariable("AngleAbs", 0f, 180f);
            angleAbs.AddSet(FuzzySet.Trapezoid("Small", 0f, 0f, 10f / 180f, 30f / 180f));
            angleAbs.AddSet(FuzzySet.Trapezoid("Mid", 10f / 180f, 30f / 180f, 70f / 180f, 100f / 180f));
            angleAbs.AddSet(FuzzySet.Trapezoid("Large", 70f / 180f, 100f / 180f, 1f, 1f));
            speedFIS.AddInput(angleAbs);

            // Input: TargetDist (0..some)
            var targetDist = new FuzzyVariable("TargetDist", 0f, 20f);//20 - max dist? Нужно ли расстояние вообще?
            targetDist.AddSet(FuzzySet.Trapezoid("VeryNear", 0f, 0f, 0.0f, 0.05f));
            targetDist.AddSet(FuzzySet.Trapezoid("Near", 0f, 0.05f, 0.1f, 0.2f));
            targetDist.AddSet(FuzzySet.Trapezoid("Mid", 0.1f, 0.2f, 0.3f, 0.4f));
            targetDist.AddSet(FuzzySet.Trapezoid("Far", 0.3f, 0.4f, 1f, 1f));
            speedFIS.AddInput(targetDist);

            // Output: Speed (0..1)
            var speedOut = new FuzzyVariable("Speed", 0f, 1f);
            speedOut.AddSet(FuzzySet.Trapezoid("Stop", 0f, 0f, 0.05f, 0.1f));
            speedOut.AddSet(FuzzySet.Trapezoid("Slow", 0.05f, 0.1f, 0.2f, 0.3f));
            speedOut.AddSet(FuzzySet.Trapezoid("Mid", 0.2f, 0.3f, 0.5f, 0.6f));
            speedOut.AddSet(FuzzySet.Trapezoid("Fast", 0.5f, 0.6f, 1f, 1f));
            speedFIS.SetOutput(speedOut);

            // Rules
            speedFIS.AddRule(new FuzzyRule("Slow").AddAntecedent("FrontDist", "VeryNear").AddAntecedent("TargetDist", "VeryNear"));
            speedFIS.AddRule(new FuzzyRule("Slow").AddAntecedent("FrontDist", "VeryNear").AddAntecedent("TargetDist", "Near"));
            speedFIS.AddRule(new FuzzyRule("Slow").AddAntecedent("FrontDist", "VeryNear").AddAntecedent("TargetDist", "Mid"));
            speedFIS.AddRule(new FuzzyRule("Slow").AddAntecedent("FrontDist", "VeryNear").AddAntecedent("TargetDist", "Far"));

            speedFIS.AddRule(new FuzzyRule("Slow").AddAntecedent("FrontDist", "Near").AddAntecedent("TargetDist", "VeryNear"));
            speedFIS.AddRule(new FuzzyRule("Slow").AddAntecedent("FrontDist", "Near").AddAntecedent("TargetDist", "Near"));
            speedFIS.AddRule(new FuzzyRule("Slow").AddAntecedent("FrontDist", "Near").AddAntecedent("TargetDist", "Mid"));
            speedFIS.AddRule(new FuzzyRule("Slow").AddAntecedent("FrontDist", "Near").AddAntecedent("TargetDist", "Far"));

            speedFIS.AddRule(new FuzzyRule("Slow").AddAntecedent("FrontDist", "Mid").AddAntecedent("TargetDist", "VeryNear"));
            speedFIS.AddRule(new FuzzyRule("Slow").AddAntecedent("FrontDist", "Mid").AddAntecedent("TargetDist", "Near"));
            speedFIS.AddRule(new FuzzyRule("Mid").AddAntecedent("FrontDist", "Mid").AddAntecedent("TargetDist", "Mid"));
            speedFIS.AddRule(new FuzzyRule("Mid").AddAntecedent("FrontDist", "Mid").AddAntecedent("TargetDist", "Far"));

            speedFIS.AddRule(new FuzzyRule("Slow").AddAntecedent("FrontDist", "Far").AddAntecedent("TargetDist", "VeryNear"));
            speedFIS.AddRule(new FuzzyRule("Slow").AddAntecedent("FrontDist", "Far").AddAntecedent("TargetDist", "Near"));
            speedFIS.AddRule(new FuzzyRule("Mid").AddAntecedent("FrontDist", "Far").AddAntecedent("TargetDist", "Mid"));
            speedFIS.AddRule(new FuzzyRule("Fast").AddAntecedent("FrontDist", "Far").AddAntecedent("TargetDist", "Far"));

            // If VeryNear then Stop
            //var r1 = new FuzzyRule("Stop").AddAntecedent("TargetDist", "VeryNear");
            //speedFIS.AddRule(r1);

            //// If Near then Slow
            //var r2 = new FuzzyRule("Slow").AddAntecedent("FrontDist", "Near");
            //speedFIS.AddRule(r2);

            //// If FrontDist is Far AND AngleAbs is Small AND TargetDist is Far -> Fast
            //var r3 = new FuzzyRule("Fast").AddAntecedent("FrontDist", "Far").AddAntecedent("AngleAbs", "Small").AddAntecedent("TargetDist", "Far");
            //speedFIS.AddRule(r3);

            //// If Angle big then Slow (need to turn)
            //var r4 = new FuzzyRule("Slow").AddAntecedent("AngleAbs", "Large");
            //speedFIS.AddRule(r4);

            //// If Medium but target close -> Slow
            //var r5 = new FuzzyRule("Slow").AddAntecedent("FrontDist", "Medium").AddAntecedent("TargetDist", "Near");
            //speedFIS.AddRule(r5);

            //// Default: Cruise when medium frontdist and small angle
            //var r6 = new FuzzyRule("Mid").AddAntecedent("FrontDist", "Medium").AddAntecedent("AngleAbs", "Small");
            //speedFIS.AddRule(r6);
        }

        private void BuildTurnFIS()
        {
            turnFIS = new FuzzyInferenceSystem();

            float maxSensor = sensors.MaxSensorDist();

            // Input: FrontDist, LeftDist, RightDist - данные с датчиков
            var left = new FuzzyVariable("LeftDist", 0f, maxSensor);
            left.AddSet(FuzzySet.Trapezoid("Near", 0f, 0f, 0.1f, 0.3f));
            left.AddSet(FuzzySet.Trapezoid("Mid", 0.1f, 0.3f, 0.5f, 0.7f));
            left.AddSet(FuzzySet.Trapezoid("Far", 0.5f, 0.7f, 1f, 1f));
            turnFIS.AddInput(left);

            var right = new FuzzyVariable("RightDist", 0f, maxSensor);
            right.AddSet(FuzzySet.Trapezoid("Near", 0f, 0f, 0.1f, 0.3f));
            right.AddSet(FuzzySet.Trapezoid("Mid", 0.1f, 0.3f, 0.5f, 0.7f));
            right.AddSet(FuzzySet.Trapezoid("Far", 0.5f, 0.7f, 1f, 1f));
            turnFIS.AddInput(right);

            var front = new FuzzyVariable("FrontDist", 0f, maxSensor);
            front.AddSet(FuzzySet.Trapezoid("Near", 0f, 0f, 0.1f, 0.3f));
            front.AddSet(FuzzySet.Trapezoid("Mid", 0.1f, 0.3f, 0.5f, 0.7f));
            front.AddSet(FuzzySet.Trapezoid("Far", 0.5f, 0.7f, 1f, 1f));
            turnFIS.AddInput(front);

            // угол до цели
            var angle = new FuzzyVariable("Angle", -180f, 180f);
            angle.AddSet(FuzzySet.Trapezoid("LeftLarge", 0f, 0f, 135f / 360f, 150f / 360f));
            angle.AddSet(FuzzySet.Trapezoid("LeftSmall", 135f / 360f, 150f / 360f, 170f / 360f, 180f / 360f));
            angle.AddSet(FuzzySet.Trapezoid("Center", 170f / 360f, 180f / 360f, 180f / 360f, 190f / 360f));
            angle.AddSet(FuzzySet.Trapezoid("RightSmall", 180f / 360f, 190f / 360f, 210f / 360f, 225f / 360f));
            angle.AddSet(FuzzySet.Trapezoid("RightLarge", 210f / 360f, 225f / 360f, 1f, 1f));
            turnFIS.AddInput(angle);

            // Output: Steering -1..1 (left negative, right positive)
            var steerOut = new FuzzyVariable("Steer", -1f, 1f);
            steerOut.AddSet(FuzzySet.Trapezoid("LL", 0f, 0f, 0.15f, 0.25f));
            steerOut.AddSet(FuzzySet.Trapezoid("L", 0.15f, 0.25f, 0.35f, 0.45f));
            steerOut.AddSet(FuzzySet.Trapezoid("F", 0.35f, 0.45f, 0.55f, 0.65f));
            steerOut.AddSet(FuzzySet.Trapezoid("R", 0.55f, 0.65f, 0.75f, 0.85f));
            steerOut.AddSet(FuzzySet.Trapezoid("RR", 0.75f, 0.85f, 1f, 1f));
            turnFIS.SetOutput(steerOut);

            // Rules:
            turnFIS.AddRule(new FuzzyRule("RR", 0.2f).AddAntecedent("Angle", "RightLarge"));
            turnFIS.AddRule(new FuzzyRule("R", 0.2f).AddAntecedent("Angle", "RightSmall"));
            turnFIS.AddRule(new FuzzyRule("LL", 0.2f).AddAntecedent("Angle", "LeftLarge"));
            turnFIS.AddRule(new FuzzyRule("L", 0.2f).AddAntecedent("Angle", "LeftSmall"));
            turnFIS.AddRule(new FuzzyRule("F", 0.2f).AddAntecedent("Angle", "Center"));

            //obstacle avoiding rules
            turnFIS.AddRule(new FuzzyRule("RR").AddAntecedent("RightDist", "Near").AddAntecedent("FrontDist", "Near").AddAntecedent("LeftDist", "Near"));//
            turnFIS.AddRule(new FuzzyRule("LL").AddAntecedent("RightDist", "Near").AddAntecedent("FrontDist", "Near").AddAntecedent("LeftDist", "Mid"));
            turnFIS.AddRule(new FuzzyRule("LL").AddAntecedent("RightDist", "Near").AddAntecedent("FrontDist", "Near").AddAntecedent("LeftDist", "Far"));
            turnFIS.AddRule(new FuzzyRule("LL").AddAntecedent("RightDist", "Near").AddAntecedent("FrontDist", "Mid").AddAntecedent("LeftDist", "Near"));
            turnFIS.AddRule(new FuzzyRule("LL").AddAntecedent("RightDist", "Near").AddAntecedent("FrontDist", "Mid").AddAntecedent("LeftDist", "Mid"));
            turnFIS.AddRule(new FuzzyRule("LL").AddAntecedent("RightDist", "Near").AddAntecedent("FrontDist", "Mid").AddAntecedent("LeftDist", "Far"));
            turnFIS.AddRule(new FuzzyRule("F").AddAntecedent("RightDist", "Near").AddAntecedent("FrontDist", "Far").AddAntecedent("LeftDist", "Near"));
            turnFIS.AddRule(new FuzzyRule("LL").AddAntecedent("RightDist", "Near").AddAntecedent("FrontDist", "Far").AddAntecedent("LeftDist", "Mid"));
            turnFIS.AddRule(new FuzzyRule("LL").AddAntecedent("RightDist", "Near").AddAntecedent("FrontDist", "Far").AddAntecedent("LeftDist", "Far"));

            turnFIS.AddRule(new FuzzyRule("RR").AddAntecedent("RightDist", "Mid").AddAntecedent("FrontDist", "Near").AddAntecedent("LeftDist", "Near"));
            turnFIS.AddRule(new FuzzyRule("RR").AddAntecedent("RightDist", "Mid").AddAntecedent("FrontDist", "Near").AddAntecedent("LeftDist", "Mid"));
            turnFIS.AddRule(new FuzzyRule("LL").AddAntecedent("RightDist", "Mid").AddAntecedent("FrontDist", "Near").AddAntecedent("LeftDist", "Far"));
            turnFIS.AddRule(new FuzzyRule("RR").AddAntecedent("RightDist", "Mid").AddAntecedent("FrontDist", "Mid").AddAntecedent("LeftDist", "Near"));
            turnFIS.AddRule(new FuzzyRule("RR").AddAntecedent("RightDist", "Mid").AddAntecedent("FrontDist", "Mid").AddAntecedent("LeftDist", "Mid"));
            turnFIS.AddRule(new FuzzyRule("LL").AddAntecedent("RightDist", "Mid").AddAntecedent("FrontDist", "Mid").AddAntecedent("LeftDist", "Far"));
            turnFIS.AddRule(new FuzzyRule("RR").AddAntecedent("RightDist", "Mid").AddAntecedent("FrontDist", "Far").AddAntecedent("LeftDist", "Near"));
            turnFIS.AddRule(new FuzzyRule("F").AddAntecedent("RightDist", "Mid").AddAntecedent("FrontDist", "Far").AddAntecedent("LeftDist", "Mid"));
            turnFIS.AddRule(new FuzzyRule("LL", 0.5f).AddAntecedent("RightDist", "Mid").AddAntecedent("FrontDist", "Far").AddAntecedent("LeftDist", "Far"));

            turnFIS.AddRule(new FuzzyRule("RR").AddAntecedent("RightDist", "Far").AddAntecedent("FrontDist", "Near").AddAntecedent("LeftDist", "Near"));
            turnFIS.AddRule(new FuzzyRule("RR").AddAntecedent("RightDist", "Far").AddAntecedent("FrontDist", "Near").AddAntecedent("LeftDist", "Mid"));
            turnFIS.AddRule(new FuzzyRule("RR").AddAntecedent("RightDist", "Far").AddAntecedent("FrontDist", "Near").AddAntecedent("LeftDist", "Far"));
            turnFIS.AddRule(new FuzzyRule("RR").AddAntecedent("RightDist", "Far").AddAntecedent("FrontDist", "Mid").AddAntecedent("LeftDist", "Near"));
            turnFIS.AddRule(new FuzzyRule("RR").AddAntecedent("RightDist", "Far").AddAntecedent("FrontDist", "Mid").AddAntecedent("LeftDist", "Mid"));
            turnFIS.AddRule(new FuzzyRule("RR",0.5f).AddAntecedent("RightDist", "Far").AddAntecedent("FrontDist", "Mid").AddAntecedent("LeftDist", "Far"));
            turnFIS.AddRule(new FuzzyRule("RR").AddAntecedent("RightDist", "Far").AddAntecedent("FrontDist", "Far").AddAntecedent("LeftDist", "Near"));
            turnFIS.AddRule(new FuzzyRule("RR",0.5f).AddAntecedent("RightDist", "Far").AddAntecedent("FrontDist", "Far").AddAntecedent("LeftDist", "Mid"));
            turnFIS.AddRule(new FuzzyRule("F",0.5f).AddAntecedent("RightDist", "Far").AddAntecedent("FrontDist", "Far").AddAntecedent("LeftDist", "Far"));

            
        }

        #endregion
    }
}
