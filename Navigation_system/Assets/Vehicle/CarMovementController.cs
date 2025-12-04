using UnityEngine;

namespace Vehicle
{
    [RequireComponent(typeof(Rigidbody))] 
    public class CarMovementController : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Units/sec")]
        [SerializeField] private float maxSpeed = 10f;
        
        [Tooltip("Degree/sec")]
        [SerializeField] private float turnSpeed = 100f;
        
        [Tooltip("Units/sec^2")]
        [SerializeField] private float acceleration = 5f;

        private float _currentSpeed = 0f;
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
		}
       
        public void Move(float throttleInput, float steeringInput)
        {
            float dt = Time.fixedDeltaTime; 
			_currentSpeed = Vector3.Dot(_rb.linearVelocity, transform.forward);

            throttleInput = Mathf.Clamp(throttleInput, -1f, 1f);
            steeringInput = Mathf.Clamp(steeringInput, -1f, 1f);

            float targetSpeed = throttleInput * maxSpeed;
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, acceleration * dt);

            // Moving
            Vector3 targetVelocity = transform.forward * _currentSpeed;
            targetVelocity.y = _rb.linearVelocity.y; 
            _rb.linearVelocity = targetVelocity;

            // Rotation
            if (Mathf.Abs(_currentSpeed) > 0.1f)
            {
                //float directionMult = (_currentSpeed >= 0) ? 1f : -1f;
                float speedFactor = Mathf.Clamp01(Mathf.Abs(_currentSpeed) / maxSpeed);
                float turnAmount = steeringInput * turnSpeed * speedFactor * /*directionMult **/ dt;
                Quaternion turnOffset = Quaternion.Euler(0, turnAmount, 0);
                
                _rb.MoveRotation(_rb.rotation * turnOffset);
            }
        }
        
        public void HardStop()
        {
            _currentSpeed = 0;
            
            _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0); 
        }
    }
}
