using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class SolidOnInvinciblePlayer : Component
	{
		private class Outline : Entity
		{
			public SolidOnInvinciblePlayer Parent;

			public Outline(SolidOnInvinciblePlayer parent)
			{
				Parent = parent;
				base.Depth = -10;
			}

			public override void Render()
			{
				if (Parent != null && Parent.Entity != null)
				{
					Entity e = Parent.Entity;
					int left = (int)e.Left;
					int right = (int)e.Right;
					int top = (int)e.Top;
					int bottom = (int)e.Bottom;
					Draw.Rect(left + 4, top + 4, e.Width - 8f, e.Height - 8f, Color.White * 0.25f);
					for (float x = left; x < (float)(right - 3); x += 3f)
					{
						Draw.Line(x, top, x + 2f, top, Color.White);
						Draw.Line(x, bottom - 1, x + 2f, bottom - 1, Color.White);
					}
					for (float y = top; y < (float)(bottom - 3); y += 3f)
					{
						Draw.Line(left + 1, y, left + 1, y + 2f, Color.White);
						Draw.Line(right, y, right, y + 2f, Color.White);
					}
					Draw.Rect(left + 1, top, 1f, 2f, Color.White);
					Draw.Rect(right - 2, top, 2f, 2f, Color.White);
					Draw.Rect(left, bottom - 2, 2f, 2f, Color.White);
					Draw.Rect(right - 2, bottom - 2, 2f, 2f, Color.White);
				}
			}
		}

		private bool wasCollidable;

		private bool wasVisible;

		private Outline outline;

		public SolidOnInvinciblePlayer()
			: base(active: true, visible: false)
		{
		}

		public override void Added(Entity entity)
		{
			base.Added(entity);
			Audio.Play("event:/game/general/assist_nonsolid_in", entity.Center);
			wasCollidable = entity.Collidable;
			wasVisible = entity.Visible;
			entity.Collidable = false;
			entity.Visible = false;
			if (entity.Scene != null)
			{
				entity.Scene.Add(outline = new Outline(this));
			}
		}

		public override void Update()
		{
			base.Update();
			base.Entity.Collidable = true;
			if (!base.Entity.CollideCheck<Player>() && !base.Entity.CollideCheck<TheoCrystal>())
			{
				RemoveSelf();
			}
			else
			{
				base.Entity.Collidable = false;
			}
		}

		public override void Removed(Entity entity)
		{
			Audio.Play("event:/game/general/assist_nonsolid_out", entity.Center);
			entity.Collidable = wasCollidable;
			entity.Visible = wasVisible;
			if (outline != null)
			{
				outline.RemoveSelf();
			}
			base.Removed(entity);
		}
	}
}
