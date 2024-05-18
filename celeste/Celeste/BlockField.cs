using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class BlockField : Entity
	{
		public BlockField(Vector2 position, int width, int height)
			: base(position)
		{
			base.Collider = new Hitbox(width, height);
		}

		public BlockField(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height)
		{
		}
	}
}
