using Microsoft.Xna.Framework;

namespace Celeste
{
	public class StopBoostTrigger : Trigger
	{
		public StopBoostTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
		}

		public override void OnEnter(Player player)
		{
			base.OnEnter(player);
			if (player.StateMachine.State == 10)
			{
				player.StopSummitLaunch();
			}
		}
	}
}
