using FishNet.Object.Synchronizing;
using UnityEngine;

namespace UmnieDziala.Game.Items
{
	public class Cable : ItemBase
	{
		[SerializeField] private Material[] Colors;
		[SerializeField] private Sprite[] Icons;
		[SerializeField] private Renderer CableRend;
		public enum CableColor
		{
			Red,
			Green,
			Blue,
			Black,
			Purple,
			Yellow,
			Pink,
			Brown,
			Orange,
			White
		}
		public readonly SyncVar<CableColor> SyncColor = new();
		[SerializeField] private CableColor MyColor;
		protected override void Awake()
		{
			base.Awake();
			ApplyColor();
		}
		public override void OnStartServer()
		{
			base.OnStartServer();
			SyncColor.Value = MyColor;
		}
		protected override void OnValidate()
		{
			base.OnValidate();
			ApplyColor();
		}
		void ApplyColor()
		{
			ItemName = $"{SyncColor.Value} Cable";
			CableRend.material = Colors[(int)SyncColor.Value];
			itemIcon = Icons[(int)SyncColor.Value];
		}
		private void Start()
		{
			SyncColor.OnChange += (_, _, _) => { ApplyColor(); };
		}
		public override void OnStartClient()
		{
			base.OnStartClient();
			ApplyColor();
		}
	}
}