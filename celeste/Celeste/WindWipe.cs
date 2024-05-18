using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class WindWipe : ScreenWipe
	{
		private int t;

		private int columns;

		private int rows;

		private VertexPositionColor[] vertexBuffer;

		public WindWipe(Scene scene, bool wipeIn, Action onComplete = null)
			: base(scene, wipeIn, onComplete)
		{
			t = 40;
			columns = 1920 / t + 1;
			rows = 1080 / t + 1;
			vertexBuffer = new VertexPositionColor[columns * rows * 6];
			for (int i = 0; i < vertexBuffer.Length; i++)
			{
				vertexBuffer[i].Color = ScreenWipe.WipeColor;
			}
		}

		public override void Render(Scene scene)
		{
			float total = columns * rows;
			int i = 0;
			for (int x = 0; x < columns; x++)
			{
				for (int y = 0; y < rows; y++)
				{
					int tx = (WipeIn ? (columns - x - 1) : x);
					float min = (float)((y + tx % 2) % 2 * (rows + y / 2) + (y + tx % 2 + 1) % 2 * (y / 2) + tx * rows) / total * 0.5f;
					float max = min + 300f / total;
					float v = (Math.Max(min, Math.Min(max, WipeIn ? (1f - Percent) : Percent)) - min) / (max - min);
					float left = ((float)x - 0.5f) * (float)t;
					float top = ((float)y - 0.5f) * (float)t - (float)t * 0.5f * v;
					float right = left + (float)t;
					float bottom = top + (float)t * v;
					vertexBuffer[i].Position = new Vector3(left, top, 0f);
					vertexBuffer[i + 1].Position = new Vector3(right, top, 0f);
					vertexBuffer[i + 2].Position = new Vector3(left, bottom, 0f);
					vertexBuffer[i + 3].Position = new Vector3(right, top, 0f);
					vertexBuffer[i + 4].Position = new Vector3(right, bottom, 0f);
					vertexBuffer[i + 5].Position = new Vector3(left, bottom, 0f);
					i += 6;
				}
			}
			ScreenWipe.DrawPrimitives(vertexBuffer);
		}
	}
}
