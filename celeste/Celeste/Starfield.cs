using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Starfield : Backdrop
	{
		public struct Star
		{
			public MTexture Texture;

			public Vector2 Position;

			public Color Color;

			public int NodeIndex;

			public float NodePercent;

			public float Distance;

			public float Sine;
		}

		public const int StepSize = 32;

		public const int Steps = 15;

		public const float MinDist = 4f;

		public const float MaxDist = 24f;

		public float FlowSpeed;

		public List<float> YNodes = new List<float>();

		public Star[] Stars = new Star[128];

		public Starfield(Color color, float speed = 1f)
		{
			Color = color;
			FlowSpeed = speed;
			float y = Calc.Random.NextFloat(180f);
			int x = 0;
			while (x < 15)
			{
				YNodes.Add(y);
				x++;
				y += (float)Calc.Random.Choose(-1, 1) * (16f + Calc.Random.NextFloat(24f));
			}
			for (int j = 0; j < 4; j++)
			{
				YNodes[YNodes.Count - 1 - j] = Calc.LerpClamp(YNodes[YNodes.Count - 1 - j], YNodes[0], 1f - (float)j / 4f);
			}
			List<MTexture> textures = GFX.Game.GetAtlasSubtextures("particles/starfield/");
			for (int i = 0; i < Stars.Length; i++)
			{
				float p = Calc.Random.NextFloat(1f);
				Stars[i].NodeIndex = Calc.Random.Next(YNodes.Count - 1);
				Stars[i].NodePercent = Calc.Random.NextFloat(1f);
				Stars[i].Distance = 4f + p * 20f;
				Stars[i].Sine = Calc.Random.NextFloat((float)Math.PI * 2f);
				Stars[i].Position = GetTargetOfStar(ref Stars[i]);
				Stars[i].Color = Color.Lerp(Color, Color.Transparent, p * 0.5f);
				int index = (int)Calc.Clamp(Ease.CubeIn(1f - p) * (float)textures.Count, 0f, textures.Count - 1);
				Stars[i].Texture = textures[index];
			}
		}

		public override void Update(Scene scene)
		{
			base.Update(scene);
			for (int i = 0; i < Stars.Length; i++)
			{
				UpdateStar(ref Stars[i]);
			}
		}

		private void UpdateStar(ref Star star)
		{
			star.Sine += Engine.DeltaTime * FlowSpeed;
			star.NodePercent += Engine.DeltaTime * 0.25f * FlowSpeed;
			if (star.NodePercent >= 1f)
			{
				star.NodePercent -= 1f;
				star.NodeIndex++;
				if (star.NodeIndex >= YNodes.Count - 1)
				{
					star.NodeIndex = 0;
					star.Position.X -= 448f;
				}
			}
			star.Position += (GetTargetOfStar(ref star) - star.Position) / 50f;
		}

		private Vector2 GetTargetOfStar(ref Star star)
		{
			Vector2 last = new Vector2(star.NodeIndex * 32, YNodes[star.NodeIndex]);
			Vector2 next = new Vector2((star.NodeIndex + 1) * 32, YNodes[star.NodeIndex + 1]);
			Vector2 vector = last + (next - last) * star.NodePercent;
			Vector2 normal = (next - last).SafeNormalize();
			Vector2 perp = new Vector2(0f - normal.Y, normal.X);
			return vector + perp * star.Distance * (float)Math.Sin(star.Sine);
		}

		public override void Render(Scene scene)
		{
			Vector2 cam = (scene as Level).Camera.Position;
			for (int i = 0; i < Stars.Length; i++)
			{
				Vector2 vector = default(Vector2);
				vector.X = -64f + Mod(Stars[i].Position.X - cam.X * Scroll.X, 448f);
				vector.Y = -16f + Mod(Stars[i].Position.Y - cam.Y * Scroll.Y, 212f);
				Vector2 at = vector;
				Stars[i].Texture.DrawCentered(at, Stars[i].Color * FadeAlphaMultiplier);
			}
		}

		private float Mod(float x, float m)
		{
			return (x % m + m) % m;
		}
	}
}
