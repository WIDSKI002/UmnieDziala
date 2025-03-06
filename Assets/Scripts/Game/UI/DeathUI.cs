using Unity.VisualScripting;
using UnityEngine;

namespace UmnieDziala.Game.UI
{
	public class DeathUI : MonoBehaviour
	{
		public static DeathUI instance;
		[SerializeField] private Animator DeathAnimator;
		[SerializeField] private GameObject ContentObject;
		public bool IsPlayed;
		private void Awake()
		{
			instance = this;
		}
		public void SetPlay(bool play)
		{
			Debug.Log($"[DEATH UI] SetPlay({play})");
			if (!play && IsPlayed)
				DeathAnimator.Rebind();
			IsPlayed=play;
			ContentObject.SetActive(play);
			ItemUI.instance.SetVisible(!play);
			NoteUI.instance.Close();
			if (play)
			{
				DeathAnimator.Play("Die");
			}
		}
	}
}