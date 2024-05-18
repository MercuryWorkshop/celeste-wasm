using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class SpawnFacingTrigger : Entity
	{
		public Facings Facing;

		public SpawnFacingTrigger(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			base.Collider = new Hitbox(data.Width, data.Height);
			Facing = data.Enum("facing", (Facings)0);
			Visible = (Active = false);
		}
	}
}
