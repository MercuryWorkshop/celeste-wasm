using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Petals : Backdrop
	{
		private struct Particle
		{
			public Vector2 Position;

			public float Speed;

			public float Spin;

			public float MaxRotate;

			public int Color;

			public float RotationCounter;
		}

		private static readonly Color[] colors = new Color[1] { Calc.HexToColor("ff3aa3") };

		private Particle[] particles = new Particle[40];

		private float fade;

		public Petals()
		{
			for (int i = 0; i < particles.Length; i++)
			{
				Reset(i);
			}
		}

		private void Reset(int i)
		{
			particles[i].Position = new Vector2(Calc.Random.Range(0, 352), Calc.Random.Range(0, 212));
			particles[i].Speed = Calc.Random.Range(6f, 16f);
			particles[i].Spin = Calc.Random.Range(8f, 12f) * 0.2f;
			particles[i].Color = Calc.Random.Next(colors.Length);
			particles[i].RotationCounter = Calc.Random.NextAngle();
			particles[i].MaxRotate = Calc.Random.Range(0.3f, 0.6f) * ((float)Math.PI / 2f);
		}

		public override void Update(Scene scene)
		{
			base.Update(scene);
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Position.Y += particles[i].Speed * Engine.DeltaTime;
				particles[i].RotationCounter += particles[i].Spin * Engine.DeltaTime;
			}
			fade = Calc.Approach(fade, Visible ? 1f : 0f, Engine.DeltaTime);
		}

		public override void Render(Scene level)
		{
			if (!(fade <= 0f))
			{
				Camera camera = (level as Level).Camera;
				MTexture particle = GFX.Game["particles/petal"];
				for (int i = 0; i < particles.Length; i++)
				{
					Vector2 at = default(Vector2);
					at.X = -16f + Mod(particles[i].Position.X - camera.X, 352f);
					at.Y = -16f + Mod(particles[i].Position.Y - camera.Y, 212f);
					float rot = (float)(1.5707963705062866 + Math.Sin(particles[i].RotationCounter * particles[i].MaxRotate) * 1.0);
					at += Calc.AngleToVector(rot, 4f);
					particle.DrawCentered(at, colors[particles[i].Color] * fade, 1f, rot - 0.8f);
				}
			}
		}

		private float Mod(float x, float m)
		{
			return (x % m + m) % m;
		}
	}
}
