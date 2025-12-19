using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using LevelObjects;

namespace Vehicle
{
	public class Menu : MonoBehaviour
	{
		[SerializeField] private GameObject car;
		[SerializeField] private GameObject menuPanel;
		[SerializeField] private GameObject endPanel;
		[SerializeField] private TMP_Dropdown levelDropdown;
		[SerializeField] private HexLevelGenerator levelGenerator;


		private CarMovementController _carMovementController;
		private Vector3 _initialCarPosition;
		private bool _isPaused = false;
		private float _currentTimeSpeed = 1.0f;

		private PlayerInputActions _inputActions;

		void Start()
		{
			menuPanel.SetActive(true);

			_carMovementController = car.GetComponentInChildren<CarMovementController>();
			_carMovementController.maxSpeed = 0f;
			_initialCarPosition = car.transform.position;

			_inputActions = new PlayerInputActions();
			_inputActions.UI.Enable();
			_inputActions.UI.Pause.performed += ctx => TogglePause();

			levelDropdown.ClearOptions();

			List<string> options = new List<string> { "Без препятствий", "Пример 1", "Пример 2", "Подкова" };
			levelDropdown.AddOptions(options);
			levelDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
		}

		private void OnDropdownValueChanged(int index)
		{
			switch (index)
			{
				case 0:
					
					levelGenerator.LoadLayout("Empty");
					break;
				case 1:
					levelGenerator.LoadLayout("Level1");

					break;
				case 2:
					levelGenerator.LoadLayout("Level45");

					break;
				case 3:
					levelGenerator.LoadLayout("Horseshoe");
					break;
			}
		}

		void OnDestroy()
		{
			if (_inputActions != null)
			{
				_inputActions.UI.Pause.performed -= ctx => TogglePause();
				_inputActions.Dispose();
			}
		}

		private void TogglePause()
		{
			if (_isPaused)
				_ResumeGame();
			else
				_PauseGame();
		}

		public void _ResumeGame()
		{
			Time.timeScale = _currentTimeSpeed;
			menuPanel.SetActive(false);
			//endPanel.SetActive(false);
			_isPaused = false;
		}

		public void _PauseGame()
		{
			Time.timeScale = 0f;
			menuPanel.SetActive(true);
			endPanel.SetActive(false);
			_isPaused = true;
		}

		public void _StartSystem()
		{
			Time.timeScale = _currentTimeSpeed;
			car.transform.position = _initialCarPosition;

			menuPanel.SetActive(false);
			StartCoroutine(WaitALittle());
			_carMovementController.maxSpeed = 7f;

		}

		private IEnumerator WaitALittle()
		{
			yield return new WaitForSeconds(0.05f);

		}

		public void _BackToStart()
		{
			car.transform.position = _initialCarPosition;
			menuPanel.SetActive(true);
			endPanel.SetActive(false);
		}

		public void _QuitGame()
		{
			Application.Quit();

#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}

		public void _Times1()
		{
			_currentTimeSpeed = 1f;
			Time.timeScale = _currentTimeSpeed;

		}

		public void _Times2()
		{
			_currentTimeSpeed = 2f;
			Time.timeScale = _currentTimeSpeed;
		}

		public void _Times5()
		{
			_currentTimeSpeed = 5f;
			Time.timeScale = _currentTimeSpeed;
		}

		public void _Times10()
		{
			_currentTimeSpeed = 10f;
			Time.timeScale = _currentTimeSpeed;
		}

		public void _EndScreen()
		{
			endPanel.SetActive(true);
			menuPanel.SetActive(false);
			_carMovementController.maxSpeed = 0f;

		}
	}
}