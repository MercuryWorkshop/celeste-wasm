using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class SummitCheckpoint : Entity
	{
		public class ConfettiRenderer : Entity
		{
			private struct Particle
			{
				public Vector2 Position;

				public Color Color;

				public Vector2 Speed;

				public float Timer;

				public float Percent;

				public float Duration;

				public float Alpha;

				public float Approach;
			}

			private static readonly Color[] confettiColors = new Color[3]
			{
				Calc.HexToColor("fe2074"),
				Calc.HexToColor("205efe"),
				Calc.HexToColor("cefe20")
			};

			private Particle[] particles = new Particle[30];

			public ConfettiRenderer(Vector2 position)
				: base(position)
			{
				base.Depth = -10010;
				for (int i = 0; i < particles.Length; i++)
				{
					particles[i].Position = Position + new Vector2(Calc.Random.Range(-3, 3), Calc.Random.Range(-3, 3));
					particles[i].Color = Calc.Random.Choose(confettiColors);
					particles[i].Timer = Calc.Random.NextFloat();
					particles[i].Duration = Calc.Random.Range(2, 4);
					particles[i].Alpha = 1f;
					float angle = -(float)Math.PI / 2f + Calc.Random.Range(-0.5f, 0.5f);
					int spd = Calc.Random.Range(140, 220);
					particles[i].Speed = Calc.AngleToVector(angle, spd);
				}
			}

			public override void Update()
			{
				for (int i = 0; i < particles.Length; i++)
				{
					particles[i].Position += particles[i].Speed * Engine.DeltaTime;
					particles[i].Speed.X = Calc.Approach(particles[i].Speed.X, 0f, 80f * Engine.DeltaTime);
					particles[i].Speed.Y = Calc.Approach(particles[i].Speed.Y, 20f, 500f * Engine.DeltaTime);
					particles[i].Timer += Engine.DeltaTime;
					particles[i].Percent += Engine.DeltaTime / particles[i].Duration;
					particles[i].Alpha = Calc.ClampedMap(particles[i].Percent, 0.9f, 1f, 1f, 0f);
					if (particles[i].Speed.Y > 0f)
					{
						particles[i].Approach = Calc.Approach(particles[i].Approach, 5f, Engine.DeltaTime * 16f);
					}
				}
			}

			public override void Render()
			{
				for (int i = 0; i < particles.Length; i++)
				{
					float rot = 0f;
					Vector2 pos = particles[i].Position;
					if (particles[i].Speed.Y < 0f)
					{
						rot = particles[i].Speed.Angle();
					}
					else
					{
						rot = (float)Math.Sin(particles[i].Timer * 4f) * 1f;
						pos += Calc.AngleToVector((float)Math.PI / 2f + rot, particles[i].Approach);
					}
					GFX.Game["particles/confetti"].DrawCentered(pos + Vector2.UnitY, Color.Black * (particles[i].Alpha * 0.5f), 1f, rot);
					GFX.Game["particles/confetti"].DrawCentered(pos, particles[i].Color * particles[i].Alpha, 1f, rot);
				}
			}
		}

		private const string Flag = "summit_checkpoint_";

		public bool Activated;

		public readonly int Number;

		private string numberString;

		private Vector2 respawn;

		private MTexture baseEmpty;

		private MTexture baseToggle;

		private MTexture baseActive;

		private List<MTexture> numbersEmpty;

		private List<MTexture> numbersActive;

		public SummitCheckpoint(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			Number = data.Int("number");
			numberString = Number.ToString("D2");
			baseEmpty = GFX.Game["scenery/summitcheckpoints/base00"];
			baseToggle = GFX.Game["scenery/summitcheckpoints/base01"];
			baseActive = GFX.Game["scenery/summitcheckpoints/base02"];
			numbersEmpty = GFX.Game.GetAtlasSubtextures("scenery/summitcheckpoints/numberbg");
			numbersActive = GFX.Game.GetAtlasSubtextures("scenery/summitcheckpoints/number");
			base.Collider = new Hitbox(32f, 32f, -16f, -8f);
			base.Depth = 8999;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if ((scene as Level).Session.GetFlag("summit_checkpoint_" + Number))
			{
				Activated = true;
			}
			respawn = SceneAs<Level>().GetSpawnPoint(Position);
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (!Activated && CollideCheck<Player>())
			{
				Activated = true;
				Level obj = base.Scene as Level;
				obj.Session.SetFlag("summit_checkpoint_" + Number);
				obj.Session.RespawnPoint = respawn;
			}
		}

		public override void Update()
		{
			if (!Activated)
			{
				Player player = CollideFirst<Player>();
				if (player != null && player.OnGround() && player.Speed.Y >= 0f)
				{
					Level obj = base.Scene as Level;
					Activated = true;
					obj.Session.SetFlag("summit_checkpoint_" + Number);
					obj.Session.RespawnPoint = respawn;
					obj.Session.UpdateLevelStartDashes();
					obj.Session.HitCheckpoint = true;
					obj.Displacement.AddBurst(Position, 0.5f, 4f, 24f, 0.5f);
					obj.Add(new ConfettiRenderer(Position));
					Audio.Play("event:/game/07_summit/checkpoint_confetti", Position);
				}
			}
		}

		public override void Render()
		{
			List<MTexture> obj = (Activated ? numbersActive : numbersEmpty);
			MTexture bg = baseActive;
			if (!Activated)
			{
				bg = (base.Scene.BetweenInterval(0.25f) ? baseEmpty : baseToggle);
			}
			bg.Draw(Position - new Vector2(bg.Width / 2 + 1, bg.Height / 2));
			obj[numberString[0] - 48].DrawJustified(Position + new Vector2(-1f, 1f), new Vector2(1f, 0f));
			obj[numberString[1] - 48].DrawJustified(Position + new Vector2(0f, 1f), new Vector2(0f, 0f));
		}
	}
}
