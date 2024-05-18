using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class StarsBG : Backdrop
	{
		private struct Star
		{
			public Vector2 Position;

			public int TextureSet;

			public float Timer;

			public float Rate;
		}

		private const int StarCount = 100;

		private Star[] stars;

		private Color[] colors;

		private List<List<MTexture>> textures;

		private float falling;

		private Vector2 center;

		public StarsBG()
		{
			textures = new List<List<MTexture>>();
			textures.Add(GFX.Game.GetAtlasSubtextures("bgs/02/stars/a"));
			textures.Add(GFX.Game.GetAtlasSubtextures("bgs/02/stars/b"));
			textures.Add(GFX.Game.GetAtlasSubtextures("bgs/02/stars/c"));
			center = new Vector2(textures[0][0].Width, textures[0][0].Height) / 2f;
			stars = new Star[100];
			for (int j = 0; j < stars.Length; j++)
			{
				stars[j] = new Star
				{
					Position = new Vector2(Calc.Random.NextFloat(320f), Calc.Random.NextFloat(180f)),
					Timer = Calc.Random.NextFloat((float)Math.PI * 2f),
					Rate = 2f + Calc.Random.NextFloat(2f),
					TextureSet = Calc.Random.Next(textures.Count)
				};
			}
			colors = new Color[8];
			for (int i = 0; i < colors.Length; i++)
			{
				colors[i] = Color.Teal * 0.7f * (1f - (float)i / (float)colors.Length);
			}
		}

		public override void Update(Scene scene)
		{
			base.Update(scene);
			if (Visible)
			{
				Level level = scene as Level;
				for (int i = 0; i < stars.Length; i++)
				{
					stars[i].Timer += Engine.DeltaTime * stars[i].Rate;
				}
				if (level.Session.Dreaming)
				{
					falling += Engine.DeltaTime * 12f;
				}
			}
		}

		public override void Render(Scene scene)
		{
			Draw.Rect(0f, 0f, 320f, 180f, Color.Black);
			Level level = scene as Level;
			Color color = Color.White;
			int amount = 100;
			if (level.Session.Dreaming)
			{
				color = Color.Teal * 0.7f;
			}
			else
			{
				amount /= 2;
			}
			for (int i = 0; i < amount; i++)
			{
				List<MTexture> set = textures[stars[i].TextureSet];
				int frame = (int)((Math.Sin(stars[i].Timer) + 1.0) / 2.0 * (double)set.Count);
				frame %= set.Count;
				Vector2 position = stars[i].Position;
				MTexture tex = set[frame];
				if (level.Session.Dreaming)
				{
					position.Y -= level.Camera.Y;
					position.Y += falling * stars[i].Rate;
					position.Y %= 180f;
					if (position.Y < 0f)
					{
						position.Y += 180f;
					}
					for (int j = 0; j < colors.Length; j++)
					{
						tex.Draw(position - Vector2.UnitY * j, center, colors[j]);
					}
				}
				tex.Draw(position, center, color);
			}
		}
	}
}
