using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class LookoutBlocker : Entity
	{
		public LookoutBlocker(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			base.Collider = new Hitbox(data.Width, data.Height);
		}
	}
}
