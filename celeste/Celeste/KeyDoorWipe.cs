using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class KeyDoorWipe : ScreenWipe
	{
		private VertexPositionColor[] vertex = new VertexPositionColor[57];

		public KeyDoorWipe(Scene scene, bool wipeIn, Action onComplete = null)
			: base(scene, wipeIn, onComplete)
		{
			for (int i = 0; i < vertex.Length; i++)
			{
				vertex[i].Color = ScreenWipe.WipeColor;
			}
		}

		public override void Render(Scene scene)
		{
			int bot = 1090;
			int h = 540;
			float p = (WipeIn ? (1f - Percent) : Percent);
			float centerEase = Ease.SineInOut(Math.Min(1f, p / 0.5f));
			float keyScale = Ease.SineInOut(1f - Calc.Clamp((p - 0.5f) / 0.3f, 0f, 1f));
			float xScale = centerEase;
			float yScale = 1f + (1f - centerEase) * 0.5f;
			float centerX = 960f * centerEase;
			float circleRadX = 128f * keyScale * xScale;
			float circleRadY = 128f * keyScale * yScale;
			float circleY = (float)h - (float)h * 0.3f * keyScale * yScale;
			float keyBottom = (float)h + (float)h * 0.5f * keyScale * yScale;
			float radFrom = 0f;
			float radTo = 0f;
			int i = 0;
			vertex[i++].Position = new Vector3(-10f, -10f, 0f);
			vertex[i++].Position = new Vector3(centerX, -10f, 0f);
			vertex[i++].Position = new Vector3(centerX, circleY - circleRadY, 0f);
			for (int r2 = 1; r2 <= 8; r2++)
			{
				radFrom = -(float)Math.PI / 2f - (float)(r2 - 1) / 8f * ((float)Math.PI / 2f);
				radTo = -(float)Math.PI / 2f - (float)r2 / 8f * ((float)Math.PI / 2f);
				vertex[i++].Position = new Vector3(-10f, -10f, 0f);
				vertex[i++].Position = new Vector3(new Vector2(centerX, circleY) + Calc.AngleToVector(radFrom, 1f) * new Vector2(circleRadX, circleRadY), 0f);
				vertex[i++].Position = new Vector3(new Vector2(centerX, circleY) + Calc.AngleToVector(radTo, 1f) * new Vector2(circleRadX, circleRadY), 0f);
			}
			vertex[i++].Position = new Vector3(-10f, -10f, 0f);
			vertex[i++].Position = new Vector3(centerX - circleRadX, circleY, 0f);
			vertex[i++].Position = new Vector3(-10f, bot, 0f);
			for (int r = 1; r <= 6; r++)
			{
				radFrom = (float)Math.PI - (float)(r - 1) / 8f * ((float)Math.PI / 2f);
				radTo = (float)Math.PI - (float)r / 8f * ((float)Math.PI / 2f);
				vertex[i++].Position = new Vector3(-10f, bot, 0f);
				vertex[i++].Position = new Vector3(new Vector2(centerX, circleY) + Calc.AngleToVector(radFrom, 1f) * new Vector2(circleRadX, circleRadY), 0f);
				vertex[i++].Position = new Vector3(new Vector2(centerX, circleY) + Calc.AngleToVector(radTo, 1f) * new Vector2(circleRadX, circleRadY), 0f);
			}
			vertex[i++].Position = new Vector3(-10f, bot, 0f);
			vertex[i++].Position = new Vector3(new Vector2(centerX, circleY) + Calc.AngleToVector(radTo, 1f) * new Vector2(circleRadX, circleRadY), 0f);
			vertex[i++].Position = new Vector3(centerX - circleRadX * 0.8f, keyBottom, 0f);
			vertex[i++].Position = new Vector3(-10f, bot, 0f);
			vertex[i++].Position = new Vector3(centerX - circleRadX * 0.8f, keyBottom, 0f);
			vertex[i++].Position = new Vector3(centerX, keyBottom, 0f);
			vertex[i++].Position = new Vector3(-10f, bot, 0f);
			vertex[i++].Position = new Vector3(centerX, keyBottom, 0f);
			vertex[i++].Position = new Vector3(centerX, bot, 0f);
			ScreenWipe.DrawPrimitives(vertex);
			for (i = 0; i < vertex.Length; i++)
			{
				vertex[i].Position.X = 1920f - vertex[i].Position.X;
			}
			ScreenWipe.DrawPrimitives(vertex);
		}
	}
}
