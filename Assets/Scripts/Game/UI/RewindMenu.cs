using UmnieDziala.Game.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UmnieDziala.Game.UI
{
	[ExecuteInEditMode]
	public class RewindMenu : MonoBehaviour
	{
		public static RewindMenu instance;
		[SerializeField] private GameObject Content;
		[SerializeField, Range(0, 5)] private byte TargetHour = 0;
		[SerializeField] private float[] Rotations = new float[6];
		[SerializeField] private Transform Arrow;
		[SerializeField] private float MouseMove;
		[SerializeField] private InputActionReference MouseClickAction;
		[SerializeField] private InputActionReference MouseDeltaAction;

		private void Awake()
		{
			instance = this;
		}
		private void Start()
		{
		}
		public void Open(bool open)
		{
			Content.SetActive(open);
			CursorManager.instance.IsRewindMenu = open;
		}

		private void Update()
		{
			UpdateTargetHourFromMouse();
			float targetRot = Rotations[TargetHour];
			float currentRot = Mathf.LerpAngle(Arrow.transform.localEulerAngles.z, targetRot, Time.unscaledDeltaTime * 10f);
			Arrow.transform.localEulerAngles = new Vector3(0, 0, currentRot);
		}

		private void UpdateTargetHourFromMouse()
		{
			if(CursorManager.instance)
				CursorManager.instance.IsRewindMenuHoldingMouse = MouseClickAction.action.IsPressed() && Content.activeInHierarchy;
			if (!MouseClickAction.action.IsPressed()) return;
			MouseMove -= MouseDeltaAction.action.ReadValue<Vector2>().y * 0.05f;

			MouseMove = Mathf.Clamp(MouseMove, 0f, GameManager.instance != null? GameManager.instance.CurrentHour.Value : 5f);
			TargetHour = (byte)MouseMove;
		}
		public void ConfirmGoBack()
		{
			GameManager.instance.CmdRequestRewind(TargetHour, GamePlayer.Local!=null? GamePlayer.Local.gameObject : null);
			TargetHour = 0;
			MouseMove = 0;
			Open(false);
		}
	}
}