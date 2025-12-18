using UnityEngine;

using System.Collections;

namespace Vehicle
{
	public class Menu : MonoBehaviour
	{
		[SerializeField] private GameObject car;
		[SerializeField] private GameObject menuPanel;
		[SerializeField] private GameObject endPanel;

		private CarMovementController _carMovementController;
		private Vector3 _initialCarPosition;

		void Start()
		{
			_carMovementController = car.GetComponentInChildren<CarMovementController>();
			_initialCarPosition = car.transform.position;
		}

		public void _StartSystem()
		{
			menuPanel.SetActive(false);
			StartCoroutine(WaitALittle());
			_carMovementController.maxSpeed = 7f;

		}

		IEnumerator WaitALittle()
		{
			yield return new WaitForSeconds(0.05f);

		}

		public void _BackToStart()
		{
			car.transform.position = _initialCarPosition;
			menuPanel.SetActive(true);
			endPanel.SetActive(false);
		}
	}
}