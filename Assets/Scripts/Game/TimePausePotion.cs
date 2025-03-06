using FishNet.Object;

namespace UmnieDziala.Game.Items
{
	public class TimePausePotion : ItemBase
	{
		[Client(RequireOwnership = true)]
		public override void UseClient()
		{
			
		}
		[Server]
		public override void UseServer()
		{
			GameManager.instance.PauseTimer();
		}
	}
}