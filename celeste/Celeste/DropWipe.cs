using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class DropWipe : ScreenWipe
	{
		private const int columns = 10;

		private float[] meetings;

		private Color color;

		public DropWipe(Scene scene, bool wipeIn, Action onComplete = null)
			: base(scene, wipeIn, onComplete)
		{
			color = ScreenWipe.WipeColor;
			meetings = new float[10];
			for (int i = 0; i < 10; i++)
			{
				meetings[i] = 0.05f + Calc.Random.NextFloat() * 0.9f;
			}
		}

		public override void Render(Scene scene)
		{
			float p = (WipeIn ? (1f - Percent) : Percent);
			float size = 192f;
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Engine.ScreenMatrix);
			if (p >= 0.995f)
			{
				Draw.Rect(-10f, -10f, Engine.Width + 20, Engine.Height + 20, color);
			}
			else
			{
				for (int i = 0; i < 10; i++)
				{
					float across = (float)i / 10f;
					float delay = (WipeIn ? (1f - across) : across) * 0.3f;
					if (p > delay)
					{
						float ease = Ease.CubeIn(Math.Min(1f, (p - delay) / 0.7f));
						float topSize = 1080f * meetings[i] * ease;
						float botSize = 1080f * (1f - meetings[i]) * ease;
						Draw.Rect((float)i * size - 1f, -10f, size + 2f, topSize + 10f, color);
						Draw.Rect((float)i * size - 1f, 1080f - botSize, size + 2f, botSize + 10f, color);
					}
				}
			}
			Draw.SpriteBatch.End();
		}
	}
}
