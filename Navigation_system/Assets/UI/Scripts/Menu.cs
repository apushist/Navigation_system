using UnityEngine;

using System.Collections;

namespace Vehicle
{
	public class Menu : MonoBehaviour
	{
		[SerializeField] private GameObject car;
		[SerializeField] private GameObject menuPanel;

		private CarMovementController _carMovementController;

		void Start()
		{
			_carMovementController = car.GetComponentInChildren<CarMovementController>();
		}

		public void StartSystem()
		{
			menuPanel.SetActive(false);
			StartCoroutine(WaitALittle());
			_carMovementController.maxSpeed = 7f;

		}

		IEnumerator WaitALittle()
		{
			yield return new WaitForSeconds(0.05f);

		}
	}
}