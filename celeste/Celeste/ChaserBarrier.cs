using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class ChaserBarrier : Entity
	{
		public ChaserBarrier(Vector2 position, int width, int height)
			: base(position)
		{
			base.Collider = new Hitbox(width, height);
		}

		public ChaserBarrier(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height)
		{
		}

		public override void Render()
		{
			base.Render();
			Draw.Rect(base.Collider, Color.Red * 0.3f);
		}
	}
}
