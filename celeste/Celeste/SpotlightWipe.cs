using System;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class SpotlightWipe : ScreenWipe
	{
		public static Vector2 FocusPoint;

		public static float Modifier = 0f;

		public bool Linear;

		private const float SmallCircleRadius = 288f;

		private const float EaseDuration = 1.8f;

		private const float EaseOpenPercent = 0.2f;

		private const float EaseClosePercent = 0.2f;

		private static VertexPositionColor[] vertexBuffer = new VertexPositionColor[768];

		private EventInstance sfx;

		public SpotlightWipe(Scene scene, bool wipeIn, Action onComplete = null)
			: base(scene, wipeIn, onComplete)
		{
			Duration = 1.8f;
			Modifier = 0f;
			if (wipeIn)
			{
				sfx = Audio.Play("event:/game/general/spotlight_intro");
			}
			else
			{
				sfx = Audio.Play("event:/game/general/spotlight_outro");
			}
		}

		public override void Cancel()
		{
			if (sfx != null)
			{
				sfx.stop(STOP_MODE.IMMEDIATE);
				sfx.release();
				sfx = null;
			}
			base.Cancel();
		}

		public override void Render(Scene scene)
		{
			float ease = (WipeIn ? Percent : (1f - Percent));
			Vector2 origin = FocusPoint;
			if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
			{
				origin.X = 320f - origin.X;
			}
			origin.X *= 6f;
			origin.Y *= 6f;
			float radius = 0f;
			float openRadius = 288f + Modifier;
			DrawSpotlight(radius: Linear ? (Ease.CubeInOut(ease) * 1920f) : ((ease < 0.2f) ? (Ease.CubeInOut(ease / 0.2f) * openRadius) : ((!(ease < 0.8f)) ? (openRadius + (ease - 0.8f) / 0.2f * (1920f - openRadius)) : openRadius)), position: origin, color: ScreenWipe.WipeColor);
		}

		public static void DrawSpotlight(Vector2 position, float radius, Color color)
		{
			Vector2 lastAngle = new Vector2(1f, 0f);
			for (int i = 0; i < vertexBuffer.Length; i += 12)
			{
				Vector2 nextAngle = Calc.AngleToVector(((float)i + 12f) / (float)vertexBuffer.Length * ((float)Math.PI * 2f), 1f);
				vertexBuffer[i].Position = new Vector3(position + lastAngle * 5000f, 0f);
				vertexBuffer[i].Color = color;
				vertexBuffer[i + 1].Position = new Vector3(position + lastAngle * radius, 0f);
				vertexBuffer[i + 1].Color = color;
				vertexBuffer[i + 2].Position = new Vector3(position + nextAngle * radius, 0f);
				vertexBuffer[i + 2].Color = color;
				vertexBuffer[i + 3].Position = new Vector3(position + lastAngle * 5000f, 0f);
				vertexBuffer[i + 3].Color = color;
				vertexBuffer[i + 4].Position = new Vector3(position + nextAngle * 5000f, 0f);
				vertexBuffer[i + 4].Color = color;
				vertexBuffer[i + 5].Position = new Vector3(position + nextAngle * radius, 0f);
				vertexBuffer[i + 5].Color = color;
				vertexBuffer[i + 6].Position = new Vector3(position + lastAngle * radius, 0f);
				vertexBuffer[i + 6].Color = color;
				vertexBuffer[i + 7].Position = new Vector3(position + lastAngle * (radius - 2f), 0f);
				vertexBuffer[i + 7].Color = Color.Transparent;
				vertexBuffer[i + 8].Position = new Vector3(position + nextAngle * (radius - 2f), 0f);
				vertexBuffer[i + 8].Color = Color.Transparent;
				vertexBuffer[i + 9].Position = new Vector3(position + lastAngle * radius, 0f);
				vertexBuffer[i + 9].Color = color;
				vertexBuffer[i + 10].Position = new Vector3(position + nextAngle * radius, 0f);
				vertexBuffer[i + 10].Color = color;
				vertexBuffer[i + 11].Position = new Vector3(position + nextAngle * (radius - 2f), 0f);
				vertexBuffer[i + 11].Color = Color.Transparent;
				lastAngle = nextAngle;
			}
			ScreenWipe.DrawPrimitives(vertexBuffer);
		}
	}
}
