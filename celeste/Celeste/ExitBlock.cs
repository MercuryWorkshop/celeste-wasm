using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class ExitBlock : Solid
	{
		private TileGrid tiles;

		private TransitionListener tl;

		private EffectCutout cutout;

		private float startAlpha;

		private char tileType;

		public ExitBlock(Vector2 position, float width, float height, char tileType)
			: base(position, width, height, safe: true)
		{
			base.Depth = -13000;
			this.tileType = tileType;
			tl = new TransitionListener();
			tl.OnOutBegin = OnTransitionOutBegin;
			tl.OnInBegin = OnTransitionInBegin;
			Add(tl);
			Add(cutout = new EffectCutout());
			SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
			EnableAssistModeChecks = false;
		}

		public ExitBlock(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.Char("tileType", '3'))
		{
		}

		private void OnTransitionOutBegin()
		{
			if (Collide.CheckRect(this, SceneAs<Level>().Bounds))
			{
				tl.OnOut = OnTransitionOut;
				startAlpha = tiles.Alpha;
			}
		}

		private void OnTransitionOut(float percent)
		{
			cutout.Alpha = (tiles.Alpha = MathHelper.Lerp(startAlpha, 0f, percent));
			cutout.Update();
		}

		private void OnTransitionInBegin()
		{
			if (Collide.CheckRect(this, SceneAs<Level>().PreviousBounds.Value) && !CollideCheck<Player>())
			{
				cutout.Alpha = 0f;
				tiles.Alpha = 0f;
				tl.OnIn = OnTransitionIn;
			}
		}

		private void OnTransitionIn(float percent)
		{
			cutout.Alpha = (tiles.Alpha = percent);
			cutout.Update();
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Level level = SceneAs<Level>();
			Rectangle mapBounds = level.Session.MapData.TileBounds;
			VirtualMap<char> mapSolids = level.SolidsData;
			int x = (int)(base.X / 8f) - mapBounds.Left;
			int y = (int)(base.Y / 8f) - mapBounds.Top;
			int w = (int)base.Width / 8;
			int h = (int)base.Height / 8;
			tiles = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, w, h, mapSolids).TileGrid;
			Add(tiles);
			Add(new TileInterceptor(tiles, highPriority: false));
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (CollideCheck<Player>())
			{
				cutout.Alpha = (tiles.Alpha = 0f);
				Collidable = false;
			}
		}

		public override void Update()
		{
			base.Update();
			if (Collidable)
			{
				cutout.Alpha = (tiles.Alpha = Calc.Approach(tiles.Alpha, 1f, Engine.DeltaTime));
			}
			else if (!CollideCheck<Player>())
			{
				Collidable = true;
				Audio.Play("event:/game/general/passage_closed_behind", base.Center);
			}
		}

		public override void Render()
		{
			if (tiles.Alpha >= 1f)
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
