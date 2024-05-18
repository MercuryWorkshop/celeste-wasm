using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class EffectCutout : Component
	{
		public float Alpha = 1f;

		private Rectangle lastSize;

		private bool lastVisible;

		private float lastAlpha;

		public int Left => (int)base.Entity.Collider.AbsoluteLeft;

		public int Right => (int)base.Entity.Collider.AbsoluteRight;

		public int Top => (int)base.Entity.Collider.AbsoluteTop;

		public int Bottom => (int)base.Entity.Collider.AbsoluteBottom;

		public Rectangle Bounds => base.Entity.Collider.Bounds;

		public EffectCutout()
			: base(active: true, visible: true)
		{
		}

		public override void Update()
		{
			bool nextVisible = Visible && base.Entity.Visible;
			Rectangle nextSize = Bounds;
			if (lastSize != nextSize || lastAlpha != Alpha || lastVisible != nextVisible)
			{
				MakeLightsDirty();
				lastSize = nextSize;
				lastAlpha = Alpha;
				lastVisible = nextVisible;
			}
		}

		public override void Removed(Entity entity)
		{
			MakeLightsDirty();
			base.Removed(entity);
		}

		public override void EntityRemoved(Scene scene)
		{
			MakeLightsDirty();
			base.EntityRemoved(scene);
		}

		private void MakeLightsDirty()
		{
			Rectangle nextSize = Bounds;
			foreach (VertexLight light in base.Entity.Scene.Tracker.GetComponents<VertexLight>())
			{
				if (!light.Dirty)
				{
					Rectangle lightBounds = new Rectangle((int)(light.Center.X - light.EndRadius), (int)(light.Center.Y - light.EndRadius), (int)light.EndRadius * 2, (int)light.EndRadius * 2);
					if (nextSize.Intersects(lightBounds) || lastSize.Intersects(lightBounds))
					{
						light.Dirty = true;
					}
				}
			}
		}
	}
}
