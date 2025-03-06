using System;
using UmnieDziala.Game.Player;
using UnityEngine;
using UnityEngine.UI;

namespace UmnieDziala.Game.UI
{
	public class StaminaBar : MonoBehaviour
	{
		public GamePlayer TargetPlayer => GamePlayer.Local;

		[SerializeField] private Slider StaminaSlider;
		[SerializeField] private TransparencyTarget[] TransparencyTargets;
		[SerializeField, Range(0.1f, 3f)] private float FadeTime = 0.6f;
		[SerializeField, Range(0f, 5f)] private float TimeToHideBar = 2f;
		float lastStamina = 100f;
		float hideTimer = 0f;
		float TargetValue = 0f;
		bool isVis = true;
		private void Awake()
		{
			foreach (var t in TransparencyTargets)
				t.SetAlpha(t.graphic.color.a);
		}
		private void Start()
		{
			SetTransparency(false,true);
		}
		private void Update()
		{
			if (TargetPlayer == null) return;
			if(lastStamina != TargetPlayer.MovementScript.currentStamina)
			{
				lastStamina = TargetPlayer.MovementScript.currentStamina;
				UpdateBar();
			}
			else if(isVis)
			{
				if((hideTimer += Time.deltaTime) >= TimeToHideBar)
					SetTransparency(false);
			}
			if(isVis)
				StaminaSlider.value = Mathf.Lerp(StaminaSlider.value, TargetValue, Time.deltaTime*5f);
		}

		private void UpdateBar()
		{
			TargetValue = TargetPlayer.MovementScript.currentStamina / TargetPlayer.MovementScript.maxStamina;
			hideTimer = 0f;
			SetTransparency(true);
		}

		private void SetTransparency(bool visible, bool instant = false)
		{
			if (isVis == visible) return;
			foreach(var g in TransparencyTargets)
			{
				g.graphic.CrossFadeAlpha(visible ? g.alpha : 0f, instant? 0f : FadeTime, true);
				Debug.Log($"Setting {g.graphic.gameObject.name} to {g.alpha}");
			}
			isVis = visible;
		}
		[Serializable]
		public class TransparencyTarget
		{
			public Graphic graphic;
			internal float alpha;

			public void SetAlpha(float alpha)
			{
				this.alpha=alpha;
			}
		}
	}
}
