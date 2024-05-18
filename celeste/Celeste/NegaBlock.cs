using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class NegaBlock : Solid
	{
		public NegaBlock(Vector2 position, float width, float height)
			: base(position, width, height, safe: false)
		{
		}

		public NegaBlock(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height)
		{
		}

		public override void Render()
		{
			base.Render();
			Draw.Rect(base.Collider, Color.Red);
		}
	}
}
