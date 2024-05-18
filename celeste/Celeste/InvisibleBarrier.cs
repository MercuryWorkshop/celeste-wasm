using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class InvisibleBarrier : Solid
	{
		public InvisibleBarrier(Vector2 position, float width, float height)
			: base(position, width, height, safe: true)
		{
			base.Tag = Tags.TransitionUpdate;
			Collidable = false;
			Visible = false;
			Add(new ClimbBlocker(edge: true));
			SurfaceSoundIndex = 33;
		}

		public InvisibleBarrier(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height)
		{
		}

		public override void Update()
		{
			Collidable = true;
			if (CollideCheck<Player>())
			{
				Collidable = false;
			}
			if (!Collidable)
			{
				Active = false;
			}
		}
	}
}
