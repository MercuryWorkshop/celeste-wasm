using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Snow : Backdrop
	{
		private struct Particle
		{
			public Vector2 Position;

			public int Color;

			public float Speed;

			public float Sin;

			public void Init(int maxColors, float speedMin, float speedMax)
			{
				Position = new Vector2(Calc.Random.NextFloat(320f), Calc.Random.NextFloat(180f));
				Color = Calc.Random.Next(maxColors);
				Speed = Calc.Random.Range(speedMin, speedMax);
				Sin = Calc.Random.NextFloat((float)Math.PI * 2f);
			}
		}

		public static readonly Color[] ForegroundColors = new Color[2]
		{
			Color.White,
			Color.CornflowerBlue
		};

		public static readonly Color[] BackgroundColors = new Color[2]
		{
			new Color(0.2f, 0.2f, 0.2f, 1f),
			new Color(0.1f, 0.2f, 0.5f, 1f)
		};

		public float Alpha = 1f;

		private float visibleFade = 1f;

		private float linearFade = 1f;

		private Color[] colors;

		private Color[] blendedColors;

		private Particle[] particles = new Particle[60];

		public Snow(bool foreground)
		{
			colors = (foreground ? ForegroundColors : BackgroundColors);
			blendedColors = new Color[colors.Length];
			int speedMin = (foreground ? 120 : 40);
			int speedMax = (foreground ? 300 : 100);
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Init(colors.Length, speedMin, speedMax);
			}
		}

		public override void Update(Scene scene)
		{
			base.Update(scene);
			visibleFade = Calc.Approach(visibleFade, IsVisible(scene as Level) ? 1 : 0, Engine.DeltaTime * 2f);
			if (FadeX != null)
			{
				linearFade = FadeX.Value((scene as Level).Camera.X + 160f);
			}
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Position.X -= particles[i].Speed * Engine.DeltaTime;
				particles[i].Position.Y += (float)Math.Sin(particles[i].Sin) * particles[i].Speed * 0.2f * Engine.DeltaTime;
				particles[i].Sin += Engine.DeltaTime;
			}
		}

		public override void Render(Scene scene)
		{
			if (!(Alpha <= 0f) && !(visibleFade <= 0f) && !(linearFade <= 0f))
			{
				for (int j = 0; j < blendedColors.Length; j++)
				{
					blendedColors[j] = colors[j] * (Alpha * visibleFade * linearFade);
				}
				Camera camera = (scene as Level).Camera;
				for (int i = 0; i < particles.Length; i++)
				{
					Vector2 pos = new Vector2(mod(particles[i].Position.X - camera.X, 320f), mod(particles[i].Position.Y - camera.Y, 180f));
					Color color = blendedColors[particles[i].Color];
					Draw.Pixel.DrawCentered(pos, color);
				}
			}
		}

		private float mod(float x, float m)
		{
			return (x % m + m) % m;
		}
	}
}
