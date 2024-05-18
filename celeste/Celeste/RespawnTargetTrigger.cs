using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class RespawnTargetTrigger : Entity
	{
		public Vector2 Target;

		public RespawnTargetTrigger(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			base.Collider = new Hitbox(data.Width, data.Height);
			Target = data.Nodes[0] + offset;
			Visible = (Active = false);
		}
	}
}
