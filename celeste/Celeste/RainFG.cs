using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class RainFG : Backdrop
	{
		private struct Particle
		{
			public Vector2 Position;

			public Vector2 Speed;

			public float Rotation;

			public Vector2 Scale;

			public void Init()
			{
				Position = new Vector2(-32f + Calc.Random.NextFloat(384f), -32f + Calc.Random.NextFloat(244f));
				Rotation = (float)Math.PI / 2f + Calc.Random.Range(-0.05f, 0.05f);
				Speed = Calc.AngleToVector(Rotation, Calc.Random.Range(200f, 600f));
				Scale = new Vector2(4f + (Speed.Length() - 200f) / 400f * 12f, 1f);
			}
		}

		public float Alpha = 1f;

		private float visibleFade = 1f;

		private float linearFade = 1f;

		private Particle[] particles = new Particle[240];

		public RainFG()
		{
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Init();
			}
		}

		public override void Update(Scene scene)
		{
			base.Update(scene);
			bool isVisible = ((scene as Level).Raining = IsVisible(scene as Level));
			visibleFade = Calc.Approach(visibleFade, isVisible ? 1 : 0, Engine.DeltaTime * (isVisible ? 10f : 0.25f));
			if (FadeX != null)
			{
				linearFade = FadeX.Value((scene as Level).Camera.X + 160f);
			}
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Position += particles[i].Speed * Engine.DeltaTime;
			}
		}

		public override void Render(Scene scene)
		{
			if (!(Alpha <= 0f) && !(visibleFade <= 0f) && !(linearFade <= 0f))
			{
				Color color = Calc.HexToColor("161933") * 0.5f * Alpha * linearFade * visibleFade;
				Camera camera = (scene as Level).Camera;
				for (int i = 0; i < particles.Length; i++)
				{
					Vector2 pos = new Vector2(mod(particles[i].Position.X - camera.X - 32f, 384f), mod(particles[i].Position.Y - camera.Y - 32f, 244f));
					Draw.Pixel.DrawCentered(pos, color, particles[i].Scale, particles[i].Rotation);
				}
			}
		}

		private float mod(float x, float m)
		{
			return (x % m + m) % m;
		}
	}
}
