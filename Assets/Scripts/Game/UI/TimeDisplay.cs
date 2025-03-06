using System;
using TMPro;
using UnityEngine;

namespace UmnieDziala.Game.UI
{
	public class TimeDisplay : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private TextMeshProUGUI TimeText;
		[SerializeField] private TextMeshProUGUI PauseText;
		private void OnEnable()
		{
			GameManager.instance.CurrentHour.OnChange += OnTimeChanged;
			GameManager.instance.CurrentMins.OnChange += OnTimeChanged;

			GameManager.instance.pauseTimer.OnChange += PauseTimerChanged;
		}
		private void OnDisable()
		{
			GameManager.instance.CurrentHour.OnChange -= OnTimeChanged;
			GameManager.instance.CurrentMins.OnChange -= OnTimeChanged;
			GameManager.instance.pauseTimer.OnChange -= PauseTimerChanged;
		}

		private void PauseTimerChanged(float prev, float next, bool asServer)
		{
			if (next <= 0)
				PauseText.text = "";
			else
			{
				int h = Mathf.FloorToInt(next / 60);
				int m = Mathf.FloorToInt(next % 60);
				PauseText.text = $"{h:D2}:{m:D2}";
			}
		}

		private void OnTimeChanged(byte prev, byte next, bool asServer)
		{
			TimeText.text = $"{GameManager.instance.CurrentHour.Value:D2}:{GameManager.instance.CurrentMins.Value:D2} AM";
		}
	}
}