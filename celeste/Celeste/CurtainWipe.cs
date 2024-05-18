using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class CurtainWipe : ScreenWipe
	{
		private VertexPositionColor[] vertexBufferLeft = new VertexPositionColor[192];

		private VertexPositionColor[] vertexBufferRight = new VertexPositionColor[192];

		public CurtainWipe(Scene scene, bool wipeIn, Action onComplete = null)
			: base(scene, wipeIn, onComplete)
		{
			for (int i = 0; i < vertexBufferLeft.Length; i++)
			{
				vertexBufferLeft[i].Color = ScreenWipe.WipeColor;
			}
		}

		public override void Render(Scene scene)
		{
			float p = (WipeIn ? Ease.CubeInOut : Ease.CubeInOut)(WipeIn ? (1f - Percent) : Percent);
			float slide = Math.Min(1f, p / 0.3f);
			float ease = Math.Max(0f, Math.Min(1f, (p - 0.1f) / 0.9f / 0.9f));
			Vector2 vector = new Vector2(0f, 540f * slide);
			Vector2 middleB = new Vector2(1920f, 1592f) / 2f;
			Vector2 middleControl = (vector + middleB) / 2f + Vector2.UnitY * 1080f * 0.25f;
			Vector2 top = new Vector2(896f + 200f * p, -350f + 256f * slide);
			Vector2 middle = new SimpleCurve(vector, middleB, middleControl).GetPoint(ease);
			Vector2 bottom = new Vector2(middle.X + 64f * p, 1080f);
			int i = 0;
			vertexBufferLeft[i++].Position = new Vector3(-10f, -10f, 0f);
			vertexBufferLeft[i++].Position = new Vector3(top.X, -10f, 0f);
			vertexBufferLeft[i++].Position = new Vector3(top.X, top.Y, 0f);
			vertexBufferLeft[i++].Position = new Vector3(-10f, -10f, 0f);
			vertexBufferLeft[i++].Position = new Vector3(-10f, middle.Y, 0f);
			vertexBufferLeft[i++].Position = new Vector3(middle.X, middle.Y, 0f);
			vertexBufferLeft[i++].Position = new Vector3(middle.X, middle.Y, 0f);
			vertexBufferLeft[i++].Position = new Vector3(-10f, middle.Y, 0f);
			vertexBufferLeft[i++].Position = new Vector3(-10f, 1090f, 0f);
			vertexBufferLeft[i++].Position = new Vector3(middle.X, middle.Y, 0f);
			vertexBufferLeft[i++].Position = new Vector3(-10f, 1090f, 0f);
			vertexBufferLeft[i++].Position = new Vector3(bottom.X, bottom.Y + 10f, 0f);
			int start = i;
			Vector2 last = top;
			for (; i < vertexBufferLeft.Length; i += 3)
			{
				Vector2 next = new SimpleCurve(top, middle, (top + middle) / 2f + new Vector2(0f, 384f * ease)).GetPoint((float)(i - start) / (float)(vertexBufferLeft.Length - start - 3));
				vertexBufferLeft[i].Position = new Vector3(-10f, -10f, 0f);
				vertexBufferLeft[i + 1].Position = new Vector3(last, 0f);
				vertexBufferLeft[i + 2].Position = new Vector3(next, 0f);
				last = next;
			}
			for (i = 0; i < vertexBufferLeft.Length; i++)
			{
				vertexBufferRight[i] = vertexBufferLeft[i];
				vertexBufferRight[i].Position.X = 1920f - vertexBufferRight[i].Position.X;
			}
			ScreenWipe.DrawPrimitives(vertexBufferLeft);
			ScreenWipe.DrawPrimitives(vertexBufferRight);
		}
	}
}
