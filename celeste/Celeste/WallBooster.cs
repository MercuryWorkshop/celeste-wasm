using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class WallBooster : Entity
	{
		public Facings Facing;

		private StaticMover staticMover;

		private ClimbBlocker climbBlocker;

		private SoundSource idleSfx;

		public bool IceMode;

		private bool notCoreMode;

		private List<Sprite> tiles;

		public WallBooster(Vector2 position, float height, bool left, bool notCoreMode)
			: base(position)
		{
			base.Tag = Tags.TransitionUpdate;
			base.Depth = 1999;
			this.notCoreMode = notCoreMode;
			if (left)
			{
				Facing = Facings.Left;
				base.Collider = new Hitbox(2f, height);
			}
			else
			{
				Facing = Facings.Right;
				base.Collider = new Hitbox(2f, height, 6f);
			}
			Add(new CoreModeListener(OnChangeMode));
			Add(staticMover = new StaticMover());
			Add(climbBlocker = new ClimbBlocker(edge: false));
			Add(idleSfx = new SoundSource());
			tiles = BuildSprite(left);
		}

		public WallBooster(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Height, data.Bool("left"), data.Bool("notCoreMode"))
		{
		}

		private List<Sprite> BuildSprite(bool left)
		{
			List<Sprite> imgs = new List<Sprite>();
			for (int y = 0; (float)y < base.Height; y += 8)
			{
				string spriteName = ((y == 0) ? "WallBoosterTop" : ((!((float)(y + 16) > base.Height)) ? "WallBoosterMid" : "WallBoosterBottom"));
				Sprite img = GFX.SpriteBank.Create(spriteName);
				if (!left)
				{
					img.FlipX = true;
					img.Position = new Vector2(4f, y);
				}
				else
				{
					img.Position = new Vector2(0f, y);
				}
				imgs.Add(img);
				Add(img);
			}
			return imgs;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Session.CoreModes mode = Session.CoreModes.None;
			if (SceneAs<Level>().CoreMode == Session.CoreModes.Cold || notCoreMode)
			{
				mode = Session.CoreModes.Cold;
			}
			OnChangeMode(mode);
		}

		private void OnChangeMode(Session.CoreModes mode)
		{
			IceMode = mode == Session.CoreModes.Cold;
			climbBlocker.Blocking = IceMode;
			tiles.ForEach(delegate(Sprite t)
			{
				t.Play(IceMode ? "ice" : "hot");
			});
			if (IceMode)
			{
				idleSfx.Stop();
			}
			else if (!idleSfx.Playing)
			{
				idleSfx.Play("event:/env/local/09_core/conveyor_idle");
			}
		}

		public override void Update()
		{
			PositionIdleSfx();
			if (!(base.Scene as Level).Transitioning)
			{
				base.Update();
			}
		}

		private void PositionIdleSfx()
		{
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				idleSfx.Position = Calc.ClosestPointOnLine(Position, Position + new Vector2(0f, base.Height), player.Center) - Position;
				idleSfx.UpdateSfxPosition();
			}
		}
	}
}
