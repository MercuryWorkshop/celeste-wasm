using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class HeartWipe : ScreenWipe
	{
		private VertexPositionColor[] vertex = new VertexPositionColor[111];

		public HeartWipe(Scene scene, bool wipeIn, Action onComplete = null)
			: base(scene, wipeIn, onComplete)
		{
			for (int i = 0; i < vertex.Length; i++)
			{
				vertex[i].Color = ScreenWipe.WipeColor;
			}
		}

		public override void Render(Scene scene)
		{
			float p = ((WipeIn ? Percent : (1f - Percent)) - 0.2f) / 0.8f;
			if (p <= 0f)
			{
				Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);
				Draw.Rect(-1f, -1f, Engine.Width + 2, Engine.Height + 2, ScreenWipe.WipeColor);
				Draw.SpriteBatch.End();
				return;
			}
			Vector2 center = new Vector2(Engine.Width, Engine.Height) / 2f;
			float radius = (float)Engine.Width * 0.75f * p;
			float edge = (float)Engine.Width * p;
			float radStart = -0.25f;
			float radTop = -(float)Math.PI / 2f;
			Vector2 circleCenter = center + new Vector2((0f - (float)Math.Cos(radStart)) * radius, (0f - radius) / 2f);
			int i = 0;
			for (int r = 1; r <= 16; r++)
			{
				float radFrom = radStart + (radTop - radStart) * ((float)(r - 1) / 16f);
				float radTo = radStart + (radTop - radStart) * ((float)r / 16f);
				vertex[i++].Position = new Vector3(center.X, 0f - edge, 0f);
				vertex[i++].Position = new Vector3(circleCenter + Calc.AngleToVector(radFrom, radius), 0f);
				vertex[i++].Position = new Vector3(circleCenter + Calc.AngleToVector(radTo, radius), 0f);
			}
			vertex[i++].Position = new Vector3(center.X, 0f - edge, 0f);
			vertex[i++].Position = new Vector3(circleCenter + new Vector2(0f, 0f - radius), 0f);
			vertex[i++].Position = new Vector3(0f - edge, 0f - edge, 0f);
			vertex[i++].Position = new Vector3(0f - edge, 0f - edge, 0f);
			vertex[i++].Position = new Vector3(circleCenter + new Vector2(0f, 0f - radius), 0f);
			vertex[i++].Position = new Vector3(0f - edge, circleCenter.Y, 0f);
			float curveAmount = (float)Math.PI * 3f / 4f;
			for (int r2 = 1; r2 <= 16; r2++)
			{
				float radFrom2 = -(float)Math.PI / 2f - (float)(r2 - 1) / 16f * curveAmount;
				float radTo2 = -(float)Math.PI / 2f - (float)r2 / 16f * curveAmount;
				vertex[i++].Position = new Vector3(0f - edge, circleCenter.Y, 0f);
				vertex[i++].Position = new Vector3(circleCenter + Calc.AngleToVector(radFrom2, radius), 0f);
				vertex[i++].Position = new Vector3(circleCenter + Calc.AngleToVector(radTo2, radius), 0f);
			}
			Vector2 curveEnd = circleCenter + Calc.AngleToVector(-(float)Math.PI / 2f - curveAmount, radius);
			Vector2 heartEnd = center + new Vector2(0f, radius * 1.8f);
			vertex[i++].Position = new Vector3(0f - edge, circleCenter.Y, 0f);
			vertex[i++].Position = new Vector3(curveEnd, 0f);
			vertex[i++].Position = new Vector3(0f - edge, (float)Engine.Height + edge, 0f);
			vertex[i++].Position = new Vector3(0f - edge, (float)Engine.Height + edge, 0f);
			vertex[i++].Position = new Vector3(curveEnd, 0f);
			vertex[i++].Position = new Vector3(heartEnd, 0f);
			vertex[i++].Position = new Vector3(0f - edge, (float)Engine.Height + edge, 0f);
			vertex[i++].Position = new Vector3(heartEnd, 0f);
			vertex[i++].Position = new Vector3(center.X, (float)Engine.Height + edge, 0f);
			ScreenWipe.DrawPrimitives(vertex);
			for (i = 0; i < vertex.Length; i++)
			{
				vertex[i].Position.X = 1920f - vertex[i].Position.X;
			}
			ScreenWipe.DrawPrimitives(vertex);
		}
	}
}
