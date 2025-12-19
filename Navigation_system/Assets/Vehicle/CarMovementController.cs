using UnityEngine;

namespace Vehicle
{
    [RequireComponent(typeof(Rigidbody))]
    public class CarMovementController : MonoBehaviour
    {
        [Header("Car Specs")]
        [SerializeField] public float maxSpeed = 10f;
        [SerializeField] private float maxSteerAngle = 35f;
        [SerializeField] private float wheelBase = 2f;
        [SerializeField] private float acceleration = 5f;

        private Rigidbody _rb;
        private float _currentSpeed = 0f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void Move(float throttleInput, float steeringInput)
        {
            float dt = Time.fixedDeltaTime;

            // Linear speed
            float forwardSpeed = Vector3.Dot(_rb.linearVelocity, transform.forward);

            throttleInput = Mathf.Clamp(throttleInput, -1f, 1f);
            steeringInput = Mathf.Clamp(steeringInput, -1f, 1f);

            // Throttle
            float targetSpeed = 0f;
            if (throttleInput != 0)
            {
                targetSpeed = throttleInput * maxSpeed;
                _currentSpeed = Mathf.MoveTowards(forwardSpeed, targetSpeed, acceleration * dt);
            }
            else
            {
                _currentSpeed = Mathf.MoveTowards(forwardSpeed, 0, acceleration * dt);
            }

            // Apply speed
            Vector3 velocityVector = transform.forward * _currentSpeed;
            velocityVector.y = _rb.linearVelocity.y;
            _rb.linearVelocity = velocityVector;

            // Rotation
            if (Mathf.Abs(_currentSpeed) > 0.01f)
            {
                float steerAngleRad = steeringInput * maxSteerAngle * Mathf.Deg2Rad;
                float turnRate = (_currentSpeed / wheelBase) * Mathf.Tan(steerAngleRad);
                float turnAmountDegrees = turnRate * Mathf.Rad2Deg * dt;
                Quaternion turnOffset = Quaternion.Euler(0, turnAmountDegrees, 0);
                _rb.MoveRotation(_rb.rotation * turnOffset);
            }
        }

		public float GetFrontSpeed()
		{
			return Vector3.Dot(_rb.linearVelocity, transform.forward);
		}
    }
}