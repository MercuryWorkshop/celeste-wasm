using Microsoft.Xna.Framework;

namespace Celeste
{
	public class NoRefillTrigger : Trigger
	{
		public bool State;

		public NoRefillTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			State = data.Bool("state");
		}

		public override void OnEnter(Player player)
		{
			base.OnEnter(player);
			SceneAs<Level>().Session.Inventory.NoRefills = State;
		}
	}
}
