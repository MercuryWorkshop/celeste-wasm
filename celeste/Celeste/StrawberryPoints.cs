using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class StrawberryPoints : Entity
	{
		private Sprite sprite;

		private bool ghostberry;

		private bool moonberry;

		private VertexLight light;

		private BloomPoint bloom;

		private int index;

		private DisplacementRenderer.Burst burst;

		public StrawberryPoints(Vector2 position, bool ghostberry, int index, bool moonberry)
			: base(position)
		{
			Add(sprite = GFX.SpriteBank.Create("strawberry"));
			Add(light = new VertexLight(Color.White, 1f, 16, 24));
			Add(bloom = new BloomPoint(1f, 12f));
			base.Depth = -2000100;
			base.Tag = (int)Tags.Persistent | (int)Tags.TransitionUpdate | (int)Tags.FrozenUpdate;
			this.ghostberry = ghostberry;
			this.moonberry = moonberry;
			this.index = index;
		}

		public override void Added(Scene scene)
		{
			index = Math.Min(5, index);
			if (index >= 5)
			{
				Achievements.Register(Achievement.ONEUP);
			}
			if (moonberry)
			{
				sprite.Play("fade_wow");
			}
			else
			{
				sprite.Play("fade" + index);
			}
			sprite.OnFinish = delegate
			{
				RemoveSelf();
			};
			base.Added(scene);
			foreach (Entity points in base.Scene.Tracker.GetEntities<StrawberryPoints>())
			{
				if (points != this && Vector2.DistanceSquared(points.Position, Position) <= 256f)
				{
					points.RemoveSelf();
				}
			}
			burst = (scene as Level).Displacement.AddBurst(Position, 0.3f, 16f, 24f, 0.3f);
		}

		public override void Update()
		{
			Level level = base.Scene as Level;
			if (level.Frozen)
			{
				if (burst != null)
				{
					burst.AlphaFrom = (burst.AlphaTo = 0f);
					burst.Percent = burst.Duration;
				}
				return;
			}
			base.Update();
			Camera cam = level.Camera;
			base.Y -= 8f * Engine.DeltaTime;
			base.X = Calc.Clamp(base.X, cam.Left + 8f, cam.Right - 8f);
			base.Y = Calc.Clamp(base.Y, cam.Top + 8f, cam.Bottom - 8f);
			light.Alpha = Calc.Approach(light.Alpha, 0f, Engine.DeltaTime * 4f);
			bloom.Alpha = light.Alpha;
			ParticleType p = (ghostberry ? Strawberry.P_GhostGlow : Strawberry.P_Glow);
			if (moonberry && !ghostberry)
			{
				p = Strawberry.P_MoonGlow;
			}
			if (base.Scene.OnInterval(0.05f))
			{
				if (sprite.Color == p.Color2)
				{
					sprite.Color = p.Color;
				}
				else
				{
					sprite.Color = p.Color2;
				}
			}
			if (base.Scene.OnInterval(0.06f) && sprite.CurrentAnimationFrame > 11)
			{
				level.ParticlesFG.Emit(p, 1, Position + Vector2.UnitY * -2f, new Vector2(8f, 4f));
			}
		}
	}
}
