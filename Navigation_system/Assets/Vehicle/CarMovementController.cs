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
		
        /// <param name="throttleInput">Вход газа: от -1 (назад) до 1 (вперед).</param>
        /// <param name="steeringInput">Вход руля: от -1 (влево) до 1 (вправо).</param>
        public void Move(float throttleInput, float steeringInput)
        {
            throttleInput = Mathf.Clamp(throttleInput, -1f, 1f);
            steeringInput = Mathf.Clamp(steeringInput, -1f, 1f);

            // Velocity calculation
            float targetSpeed = throttleInput * maxSpeed;
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, acceleration * Time.deltaTime);

            // Moving
            Vector3 translation = Vector3.forward * _currentSpeed * Time.deltaTime;
            transform.Translate(translation);

            // Steering
            if (Mathf.Abs(_currentSpeed) > 0.1f)
            {
				// TODO: ??? Invert steering when moving backwards?
				float speedFactor = _currentSpeed / maxSpeed;
                float turnAmount = steeringInput * turnSpeed * speedFactor * Time.deltaTime;
                transform.Rotate(0, turnAmount, 0);
            }
        }
        
        public void HardStop()
        {
            _currentSpeed = 0;
        }
    }
}
