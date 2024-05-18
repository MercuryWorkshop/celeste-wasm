using Microsoft.Xna.Framework;

namespace Celeste
{
	public class WindAttackTrigger : Trigger
	{
		public WindAttackTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
		}

		public override void OnEnter(Player player)
		{
			base.OnEnter(player);
			if (base.Scene.Entities.FindFirst<Snowball>() == null)
			{
				base.Scene.Add(new Snowball());
			}
			RemoveSelf();
		}
	}
}
