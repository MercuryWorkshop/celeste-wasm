using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Planets : Backdrop
	{
		private struct Planet
		{
			public MTexture Texture;

			public Vector2 Position;
		}

		private Planet[] planets;

		public const int MapWidth = 640;

		public const int MapHeight = 360;

		public Planets(int count, string size)
		{
			List<MTexture> textures = GFX.Game.GetAtlasSubtextures("bgs/10/" + size);
			planets = new Planet[count];
			for (int i = 0; i < planets.Length; i++)
			{
				planets[i].Texture = Calc.Random.Choose(textures);
				planets[i].Position = new Vector2
				{
					X = Calc.Random.NextFloat(640f),
					Y = Calc.Random.NextFloat(360f)
				};
			}
		}

		public override void Render(Scene scene)
		{
			Vector2 cam = (scene as Level).Camera.Position;
			Color color = Color * FadeAlphaMultiplier;
			for (int i = 0; i < planets.Length; i++)
			{
				Vector2 vector = default(Vector2);
				vector.X = -32f + Mod(planets[i].Position.X - cam.X * Scroll.X, 640f);
				vector.Y = -32f + Mod(planets[i].Position.Y - cam.Y * Scroll.Y, 360f);
				Vector2 at = vector;
				planets[i].Texture.DrawCentered(at, color);
			}
		}

		private float Mod(float x, float m)
		{
			return (x % m + m) % m;
		}
	}
}
