using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class WaterFall : Entity
	{
		private float height;

		private Water water;

		private Solid solid;

		private SoundSource loopingSfx;

		private SoundSource enteringSfx;

		public WaterFall(Vector2 position)
			: base(position)
		{
			base.Depth = -9999;
			base.Tag = Tags.TransitionUpdate;
		}

		public WaterFall(EntityData data, Vector2 offset)
			: this(data.Position + offset)
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			Level level = base.Scene as Level;
			bool deep = false;
			height = 8f;
			while (base.Y + height < (float)level.Bounds.Bottom && (water = base.Scene.CollideFirst<Water>(new Rectangle((int)base.X, (int)(base.Y + height), 8, 8))) == null && ((solid = base.Scene.CollideFirst<Solid>(new Rectangle((int)base.X, (int)(base.Y + height), 8, 8))) == null || !solid.BlockWaterfalls))
			{
				height += 8f;
				solid = null;
			}
			if (water != null && !base.Scene.CollideCheck<Solid>(new Rectangle((int)base.X, (int)(base.Y + height), 8, 16)))
			{
				deep = true;
			}
			Add(loopingSfx = new SoundSource());
			loopingSfx.Play("event:/env/local/waterfall_small_main");
			Add(enteringSfx = new SoundSource());
			enteringSfx.Play(deep ? "event:/env/local/waterfall_small_in_deep" : "event:/env/local/waterfall_small_in_shallow");
			enteringSfx.Position.Y = height;
			Add(new DisplacementRenderHook(RenderDisplacement));
		}

		public override void Update()
		{
			Vector2 cam = (base.Scene as Level).Camera.Position;
			loopingSfx.Position.Y = Calc.Clamp(cam.Y + 90f, base.Y, height);
			if (water != null && base.Scene.OnInterval(0.3f))
			{
				water.TopSurface.DoRipple(new Vector2(base.X + 4f, water.Y), 0.75f);
			}
			if (water != null || solid != null)
			{
				Vector2 particlePosition = new Vector2(base.X + 4f, base.Y + height + 2f);
				(base.Scene as Level).ParticlesFG.Emit(Water.P_Splash, 1, particlePosition, new Vector2(8f, 2f), new Vector2(0f, -1f).Angle());
			}
			base.Update();
		}

		public void RenderDisplacement()
		{
			Draw.Rect(base.X, base.Y, 8f, height, new Color(0.5f, 0.5f, 0.8f, 1f));
		}

		public override void Render()
		{
			if (water == null || water.TopSurface == null)
			{
				Draw.Rect(base.X + 1f, base.Y, 6f, height, Water.FillColor);
				Draw.Rect(base.X - 1f, base.Y, 2f, height, Water.SurfaceColor);
				Draw.Rect(base.X + 7f, base.Y, 2f, height, Water.SurfaceColor);
				return;
			}
			Water.Surface surface = water.TopSurface;
			float h = height + water.TopSurface.Position.Y - water.Y;
			for (int x = 0; x < 6; x++)
			{
				Draw.Rect(base.X + (float)x + 1f, base.Y, 1f, h - surface.GetSurfaceHeight(new Vector2(base.X + 1f + (float)x, water.Y)), Water.FillColor);
			}
			Draw.Rect(base.X - 1f, base.Y, 2f, h - surface.GetSurfaceHeight(new Vector2(base.X, water.Y)), Water.SurfaceColor);
			Draw.Rect(base.X + 7f, base.Y, 2f, h - surface.GetSurfaceHeight(new Vector2(base.X + 8f, water.Y)), Water.SurfaceColor);
		}
	}
}
