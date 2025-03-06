using FishNet.Connection;
using FishNet.Object;
using UmnieDziala.Game.Player; 
using UmnieDziala.Game.UI;
namespace UmnieDziala.Game.Items
{
	public class KillPotion : ItemBase
	{
		protected override void Awake()
		{
			base.Awake();
		}
		[Server]
		public override void UseServer()
		{
			if (Holder.TryGetComponent(out GamePlayer player))
			{
				player.IsAlive.Value = false;
				player.MovementScript.ForcePosition.Value = player.transform.position;
				player.MovementScript.ForceRotation.Value = player.transform.rotation;
				TargetDeathScreen(player.Owner);
			}
		}
		[TargetRpc]
		void TargetDeathScreen(NetworkConnection conn)
		{
			DeathUI.instance.SetPlay(true);
		}
	}
}