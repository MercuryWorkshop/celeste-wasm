using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class MountainWipe : ScreenWipe
	{
		private VertexPositionColor[] vertexBuffer = new VertexPositionColor[9];

		public MountainWipe(Scene scene, bool wipeIn, Action onComplete = null)
			: base(scene, wipeIn, onComplete)
		{
			for (int i = 0; i < vertexBuffer.Length; i++)
			{
				vertexBuffer[i].Color = ScreenWipe.WipeColor;
			}
		}

		public override void Render(Scene scene)
		{
			float ease = Percent;
			int height = 1080;
			Vector2 tip = new Vector2(960f, (float)height - (float)(height * 2) * ease);
			Vector2 left = new Vector2(-10f, (float)(height * 2) * (1f - ease));
			Vector2 right = new Vector2(base.Right, (float)(height * 2) * (1f - ease));
			if (!WipeIn)
			{
				vertexBuffer[0].Position = new Vector3(tip, 0f);
				vertexBuffer[1].Position = new Vector3(left, 0f);
				vertexBuffer[2].Position = new Vector3(right, 0f);
				vertexBuffer[3].Position = new Vector3(left, 0f);
				vertexBuffer[4].Position = new Vector3(right, 0f);
				vertexBuffer[5].Position = new Vector3(left.X, left.Y + (float)height + 10f, 0f);
				vertexBuffer[6].Position = new Vector3(right, 0f);
				vertexBuffer[8].Position = new Vector3(right.X, right.Y + (float)height + 10f, 0f);
				vertexBuffer[7].Position = new Vector3(left.X, left.Y + (float)height + 10f, 0f);
			}
			else
			{
				vertexBuffer[0].Position = new Vector3(left.X, tip.Y - (float)height - 10f, 0f);
				vertexBuffer[1].Position = new Vector3(right.X, tip.Y - (float)height - 10f, 0f);
				vertexBuffer[2].Position = new Vector3(tip, 0f);
				vertexBuffer[3].Position = new Vector3(left.X, tip.Y - (float)height - 10f, 0f);
				vertexBuffer[4].Position = new Vector3(tip, 0f);
				vertexBuffer[5].Position = new Vector3(left, 0f);
				vertexBuffer[6].Position = new Vector3(right.X, tip.Y - (float)height - 10f, 0f);
				vertexBuffer[7].Position = new Vector3(right, 0f);
				vertexBuffer[8].Position = new Vector3(tip, 0f);
			}
			ScreenWipe.DrawPrimitives(vertexBuffer);
		}
	}
}
