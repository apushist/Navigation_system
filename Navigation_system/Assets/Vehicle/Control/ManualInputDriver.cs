using UnityEngine;
using UnityEngine.InputSystem;

namespace Vehicle.Control
{
	/// <summary>
	/// Only for debug purposes
	/// </summary>
	[RequireComponent(typeof(CarMovementController))]
	public class ManualInputDriver : MonoBehaviour
	{
		private CarMovementController _carController;

		[Header("Input Setup")]
		[SerializeField] private InputActionReference moveInputRef;

		private void Awake()
		{
			_carController = GetComponent<CarMovementController>();
		}

		private void OnEnable()
		{
			if (moveInputRef != null && moveInputRef.action != null)
			{
				moveInputRef.action.Enable();
			}
		}

		private void OnDisable()
		{
			if (moveInputRef != null && moveInputRef.action != null)
			{
				moveInputRef.action.Disable();
			}
		}

		private void Update()
		{
			if (moveInputRef == null) 
				return;

			Vector2 inputVector = moveInputRef.action.ReadValue<Vector2>();
			_carController.Move(inputVector.y, inputVector.x);
		}
	}
}
