using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class AngledWipe : ScreenWipe
	{
		private const int rows = 6;

		private const float angleSize = 64f;

		private VertexPositionColor[] vertexBuffer = new VertexPositionColor[36];

		public AngledWipe(Scene scene, bool wipeIn, Action onComplete = null)
			: base(scene, wipeIn, onComplete)
		{
			for (int i = 0; i < vertexBuffer.Length; i++)
			{
				vertexBuffer[i].Color = ScreenWipe.WipeColor;
			}
		}

		public override void Render(Scene scene)
		{
			float rowHeight = 183.33333f;
			float left = -64f;
			float width = 1984f;
			for (int j = 0; j < 6; j++)
			{
				int v = j * 6;
				float x = left;
				float y = -10f + (float)j * rowHeight;
				float e = 0f;
				float across = (float)j / 6f;
				float delay = (WipeIn ? (1f - across) : across) * 0.3f;
				if (Percent > delay)
				{
					e = Math.Min(1f, (Percent - delay) / 0.7f);
				}
				if (WipeIn)
				{
					e = 1f - e;
				}
				float w = width * e;
				vertexBuffer[v].Position = new Vector3(x, y, 0f);
				vertexBuffer[v + 1].Position = new Vector3(x + w, y, 0f);
				vertexBuffer[v + 2].Position = new Vector3(x, y + rowHeight, 0f);
				vertexBuffer[v + 3].Position = new Vector3(x + w, y, 0f);
				vertexBuffer[v + 4].Position = new Vector3(x + w + 64f, y + rowHeight, 0f);
				vertexBuffer[v + 5].Position = new Vector3(x, y + rowHeight, 0f);
			}
			if (WipeIn)
			{
				for (int i = 0; i < vertexBuffer.Length; i++)
				{
					vertexBuffer[i].Position.X = 1920f - vertexBuffer[i].Position.X;
					vertexBuffer[i].Position.Y = 1080f - vertexBuffer[i].Position.Y;
				}
			}
			ScreenWipe.DrawPrimitives(vertexBuffer);
		}
	}
}
