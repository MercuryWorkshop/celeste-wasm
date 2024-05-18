using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class DreamWipe : ScreenWipe
	{
		private struct Circle
		{
			public Vector2 Position;

			public float Radius;

			public float Delay;
		}

		private readonly int circleColumns = 15;

		private readonly int circleRows = 8;

		private const int circleSegments = 32;

		private const float circleFillSpeed = 400f;

		private static Circle[] circles;

		private static VertexPositionColor[] vertexBuffer;

		public DreamWipe(Scene scene, bool wipeIn, Action onComplete = null)
			: base(scene, wipeIn, onComplete)
		{
			if (vertexBuffer == null)
			{
				vertexBuffer = new VertexPositionColor[(circleColumns + 2) * (circleRows + 2) * 32 * 3];
			}
			if (circles == null)
			{
				circles = new Circle[(circleColumns + 2) * (circleRows + 2)];
			}
			for (int j = 0; j < vertexBuffer.Length; j++)
			{
				vertexBuffer[j].Color = ScreenWipe.WipeColor;
			}
			int tileX = 1920 / circleColumns;
			int tileY = 1080 / circleRows;
			int x = 0;
			int i = 0;
			for (; x < circleColumns + 2; x++)
			{
				for (int y = 0; y < circleRows + 2; y++)
				{
					circles[i].Position = new Vector2(((float)(x - 1) + 0.2f + Calc.Random.NextFloat(0.6f)) * (float)tileX, ((float)(y - 1) + 0.2f + Calc.Random.NextFloat(0.6f)) * (float)tileY);
					circles[i].Delay = Calc.Random.NextFloat(0.05f) + (float)(WipeIn ? (circleColumns - x) : x) * 0.018f;
					circles[i].Radius = (WipeIn ? (400f * (Duration - circles[i].Delay)) : 0f);
					i++;
				}
			}
		}

		public override void Update(Scene scene)
		{
			base.Update(scene);
			for (int i = 0; i < circles.Length; i++)
			{
				if (!WipeIn)
				{
					circles[i].Delay -= Engine.DeltaTime;
					if (circles[i].Delay <= 0f)
					{
						circles[i].Radius += Engine.DeltaTime * 400f;
					}
				}
				else if (circles[i].Radius > 0f)
				{
					circles[i].Radius -= Engine.DeltaTime * 400f;
				}
				else
				{
					circles[i].Radius = 0f;
				}
			}
		}

		public override void Render(Scene scene)
		{
			int v = 0;
			for (int i = 0; i < circles.Length; i++)
			{
				Circle circle = circles[i];
				Vector2 last = new Vector2(1f, 0f);
				for (float r = 0f; r < 32f; r += 1f)
				{
					Vector2 next = Calc.AngleToVector((r + 1f) / 32f * ((float)Math.PI * 2f), 1f);
					vertexBuffer[v++].Position = new Vector3(circle.Position, 0f);
					vertexBuffer[v++].Position = new Vector3(circle.Position + last * circle.Radius, 0f);
					vertexBuffer[v++].Position = new Vector3(circle.Position + next * circle.Radius, 0f);
					last = next;
				}
			}
			ScreenWipe.DrawPrimitives(vertexBuffer);
		}
	}
}
