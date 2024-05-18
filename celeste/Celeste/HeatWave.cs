using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class HeatWave : Backdrop
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

		private static readonly Color[] hotColors = new Color[2]
		{
			Color.Red,
			Color.Orange
		};

		private static readonly Color[] coldColors = new Color[2]
		{
			Color.LightSkyBlue,
			Color.Teal
		};

		private Color[] currentColors;

		private float colorLerp;

		private Particle[] particles = new Particle[50];

		private float fade;

		private float heat;

		private Parallax mist1;

		private Parallax mist2;

		private bool show;

		private bool wasShow;

		public HeatWave()
		{
			for (int i = 0; i < particles.Length; i++)
			{
				Reset(i, Calc.Random.NextFloat());
			}
			currentColors = new Color[hotColors.Length];
			colorLerp = 1f;
			mist1 = new Parallax(GFX.Misc["mist"]);
			mist2 = new Parallax(GFX.Misc["mist"]);
		}

		private void Reset(int i, float p)
		{
			particles[i].Percent = p;
			particles[i].Position = new Vector2(Calc.Random.Range(0, 320), Calc.Random.Range(0, 180));
			particles[i].Speed = Calc.Random.Range(4, 14);
			particles[i].Spin = Calc.Random.Range(0.25f, (float)Math.PI * 6f);
			particles[i].Duration = Calc.Random.Range(1f, 4f);
			particles[i].Direction = Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 1f);
			particles[i].Color = Calc.Random.Next(hotColors.Length);
		}

		public override void Update(Scene scene)
		{
			Level level = scene as Level;
			show = IsVisible(level) && level.CoreMode != Session.CoreModes.None;
			if (show)
			{
				if (!wasShow)
				{
					colorLerp = ((level.CoreMode == Session.CoreModes.Hot) ? 1 : 0);
					level.NextColorGrade((level.CoreMode == Session.CoreModes.Hot) ? "hot" : "cold");
				}
				else
				{
					level.SnapColorGrade((level.CoreMode == Session.CoreModes.Hot) ? "hot" : "cold");
				}
				colorLerp = Calc.Approach(colorLerp, (level.CoreMode == Session.CoreModes.Hot) ? 1 : 0, Engine.DeltaTime * 100f);
				for (int j = 0; j < currentColors.Length; j++)
				{
					currentColors[j] = Color.Lerp(coldColors[j], hotColors[j], colorLerp);
				}
			}
			else
			{
				level.NextColorGrade("none");
			}
			for (int i = 0; i < particles.Length; i++)
			{
				if (particles[i].Percent >= 1f)
				{
					Reset(i, 0f);
				}
				float mult = 1f;
				if (level.CoreMode == Session.CoreModes.Cold)
				{
					mult = 0.25f;
				}
				particles[i].Percent += Engine.DeltaTime / particles[i].Duration;
				particles[i].Position += particles[i].Direction * particles[i].Speed * mult * Engine.DeltaTime;
				particles[i].Direction.Rotate(particles[i].Spin * Engine.DeltaTime);
				if (level.CoreMode == Session.CoreModes.Hot)
				{
					particles[i].Position.Y -= 10f * Engine.DeltaTime;
				}
			}
			fade = Calc.Approach(fade, show ? 1f : 0f, Engine.DeltaTime);
			heat = Calc.Approach(heat, (show && level.CoreMode == Session.CoreModes.Hot) ? 1f : 0f, Engine.DeltaTime * 100f);
			mist1.Color = Color.Lerp(Calc.HexToColor("639bff"), Calc.HexToColor("f1b22b"), heat) * fade * 0.7f;
			mist2.Color = Color.Lerp(Calc.HexToColor("5fcde4"), Calc.HexToColor("f12b3a"), heat) * fade * 0.7f;
			mist1.Speed = new Vector2(4f, -20f) * heat;
			mist2.Speed = new Vector2(4f, -40f) * heat;
			mist1.Update(scene);
			mist2.Update(scene);
			if (heat > 0f)
			{
				Distort.WaterSineDirection = -1f;
				Distort.WaterAlpha = heat * 0.5f;
			}
			else
			{
				Distort.WaterAlpha = 1f;
			}
			wasShow = show;
		}

		public void RenderDisplacement(Level level)
		{
			if (heat > 0f)
			{
				Draw.Rect(color: new Color(0.5f, 0.5f, 0.1f, 1f), x: level.Camera.X - 5f, y: level.Camera.Y - 5f, width: 370f, height: 190f);
			}
		}

		public override void Render(Scene scene)
		{
			if (!(fade <= 0f))
			{
				Camera camera = (scene as Level).Camera;
				for (int i = 0; i < particles.Length; i++)
				{
					Vector2 vector = default(Vector2);
					vector.X = Mod(particles[i].Position.X - camera.X, 320f);
					vector.Y = Mod(particles[i].Position.Y - camera.Y, 180f);
					Vector2 position = vector;
					float p = particles[i].Percent;
					float alpha = 0f;
					alpha = ((!(p < 0.7f)) ? Calc.ClampedMap(p, 0.7f, 1f, 1f, 0f) : Calc.ClampedMap(p, 0f, 0.3f));
					Draw.Rect(position, 1f, 1f, currentColors[particles[i].Color] * (fade * alpha));
				}
				mist1.Render(scene);
				mist2.Render(scene);
			}
		}

		private float Mod(float x, float m)
		{
			return (x % m + m) % m;
		}
	}
}
