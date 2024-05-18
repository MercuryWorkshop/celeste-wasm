using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class FakeWall : Entity
	{
		public enum Modes
		{
			Wall,
			Block
		}

		private Modes mode;

		private char fillTile;

		private TileGrid tiles;

		private bool fade;

		private EffectCutout cutout;

		private float transitionStartAlpha;

		private bool transitionFade;

		private EntityID eid;

		private bool playRevealWhenTransitionedInto;

		public FakeWall(EntityID eid, Vector2 position, char tile, float width, float height, Modes mode)
			: base(position)
		{
			this.mode = mode;
			this.eid = eid;
			fillTile = tile;
			base.Collider = new Hitbox(width, height);
			base.Depth = -13000;
			Add(cutout = new EffectCutout());
		}

		public FakeWall(EntityID eid, EntityData data, Vector2 offset, Modes mode)
			: this(eid, data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height, mode)
		{
			playRevealWhenTransitionedInto = data.Bool("playTransitionReveal");
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			int w = (int)base.Width / 8;
			int h = (int)base.Height / 8;
			if (mode == Modes.Wall)
			{
				Level level = SceneAs<Level>();
				Rectangle mapBounds = level.Session.MapData.TileBounds;
				VirtualMap<char> mapSolids = level.SolidsData;
				int x = (int)base.X / 8 - mapBounds.Left;
				int y = (int)base.Y / 8 - mapBounds.Top;
				tiles = GFX.FGAutotiler.GenerateOverlay(fillTile, x, y, w, h, mapSolids).TileGrid;
			}
			else if (mode == Modes.Block)
			{
				tiles = GFX.FGAutotiler.GenerateBox(fillTile, w, h).TileGrid;
			}
			Add(tiles);
			Add(new TileInterceptor(tiles, highPriority: false));
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (CollideCheck<Player>())
			{
				tiles.Alpha = 0f;
				fade = true;
				cutout.Visible = false;
				if (playRevealWhenTransitionedInto)
				{
					Audio.Play("event:/game/general/secret_revealed", base.Center);
				}
				SceneAs<Level>().Session.DoNotLoad.Add(eid);
			}
			else
			{
				TransitionListener tl = new TransitionListener();
				tl.OnOut = OnTransitionOut;
				tl.OnOutBegin = OnTransitionOutBegin;
				tl.OnIn = OnTransitionIn;
				tl.OnInBegin = OnTransitionInBegin;
				Add(tl);
			}
		}

		private void OnTransitionOutBegin()
		{
			if (Collide.CheckRect(this, SceneAs<Level>().Bounds))
			{
				transitionFade = true;
				transitionStartAlpha = tiles.Alpha;
			}
			else
			{
				transitionFade = false;
			}
		}

		private void OnTransitionOut(float percent)
		{
			if (transitionFade)
			{
				tiles.Alpha = transitionStartAlpha * (1f - percent);
			}
		}

		private void OnTransitionInBegin()
		{
			Level level = SceneAs<Level>();
			if (level.PreviousBounds.HasValue && Collide.CheckRect(this, level.PreviousBounds.Value))
			{
				transitionFade = true;
				tiles.Alpha = 0f;
			}
			else
			{
				transitionFade = false;
			}
		}

		private void OnTransitionIn(float percent)
		{
			if (transitionFade)
			{
				tiles.Alpha = percent;
			}
		}

		public override void Update()
		{
			base.Update();
			if (fade)
			{
				tiles.Alpha = Calc.Approach(tiles.Alpha, 0f, 2f * Engine.DeltaTime);
				cutout.Alpha = tiles.Alpha;
				if (tiles.Alpha <= 0f)
				{
					RemoveSelf();
				}
				return;
			}
			Player player = CollideFirst<Player>();
			if (player != null && player.StateMachine.State != 9)
			{
				SceneAs<Level>().Session.DoNotLoad.Add(eid);
				fade = true;
				Audio.Play("event:/game/general/secret_revealed", base.Center);
			}
		}

		public override void Render()
		{
			if (mode == Modes.Wall)
			{
				Level level = base.Scene as Level;
				if (level.ShakeVector.X < 0f && level.Camera.X <= (float)level.Bounds.Left && base.X <= (float)level.Bounds.Left)
				{
					tiles.RenderAt(Position + new Vector2(-3f, 0f));
				}
				if (level.ShakeVector.X > 0f && level.Camera.X + 320f >= (float)level.Bounds.Right && base.X + base.Width >= (float)level.Bounds.Right)
				{
					tiles.RenderAt(Position + new Vector2(3f, 0f));
				}
				if (level.ShakeVector.Y < 0f && level.Camera.Y <= (float)level.Bounds.Top && base.Y <= (float)level.Bounds.Top)
				{
					tiles.RenderAt(Position + new Vector2(0f, -3f));
				}
				if (level.ShakeVector.Y > 0f && level.Camera.Y + 180f >= (float)level.Bounds.Bottom && base.Y + base.Height >= (float)level.Bounds.Bottom)
				{
					tiles.RenderAt(Position + new Vector2(0f, 3f));
				}
			}
			base.Render();
		}
	}
}
