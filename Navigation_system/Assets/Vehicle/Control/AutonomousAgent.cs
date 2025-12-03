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
	/// </summary>
	public class AutonomousAgent : MonoBehaviour
	{
		void Start()
		{
        
		}

		void Update()
		{
        
		}
	}
}
