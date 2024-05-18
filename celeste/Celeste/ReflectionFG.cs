using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class ReflectionFG : Backdrop
	{
		private struct Particle
		{
			public Vector2 Position;

			public float Percent;

			public float Duration;

			public Vector2 Direction;

			public float Speed;

			public float Spin;

			public int Color;
		}

		private static readonly Color[] colors = new Color[1] { Calc.HexToColor("f52b63") };

		private Particle[] particles = new Particle[50];

		private float fade;

		public ReflectionFG()
		{
			for (int i = 0; i < particles.Length; i++)
			{
				Reset(i, Calc.Random.NextFloat());
			}
		}

		private void Reset(int i, float p)
		{
			particles[i].Percent = p;
			particles[i].Position = new Vector2(Calc.Random.Range(0, 320), Calc.Random.Range(0, 180));
			particles[i].Speed = Calc.Random.Range(4, 14);
			particles[i].Spin = Calc.Random.Range(0.25f, (float)Math.PI * 6f);
			particles[i].Duration = Calc.Random.Range(1f, 4f);
			particles[i].Direction = Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 1f);
			particles[i].Color = Calc.Random.Next(colors.Length);
		}

		public override void Update(Scene scene)
		{
			base.Update(scene);
			for (int i = 0; i < particles.Length; i++)
			{
				if (particles[i].Percent >= 1f)
				{
					Reset(i, 0f);
				}
				particles[i].Percent += Engine.DeltaTime / particles[i].Duration;
				particles[i].Position += particles[i].Direction * particles[i].Speed * Engine.DeltaTime;
				particles[i].Direction.Rotate(particles[i].Spin * Engine.DeltaTime);
			}
			fade = Calc.Approach(fade, Visible ? 1f : 0f, Engine.DeltaTime);
		}

		public override void Render(Scene level)
		{
			if (!(fade <= 0f))
			{
				Camera camera = (level as Level).Camera;
				for (int i = 0; i < particles.Length; i++)
				{
					Vector2 at = default(Vector2);
					at.X = mod(particles[i].Position.X - camera.X, 320f);
					at.Y = mod(particles[i].Position.Y - camera.Y, 180f);
					float p = particles[i].Percent;
					float alpha = 0f;
					alpha = ((!(p < 0.7f)) ? Calc.ClampedMap(p, 0.7f, 1f, 1f, 0f) : Calc.ClampedMap(p, 0f, 0.3f));
					Draw.Rect(at, 1f, 1f, colors[particles[i].Color] * (fade * alpha));
				}
			}
		}

		private float mod(float x, float m)
		{
			return (x % m + m) % m;
		}
	}
}
