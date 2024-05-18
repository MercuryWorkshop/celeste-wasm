using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Monocle
{
	public class TimerText : GraphicsComponent
	{
		public enum CountModes
		{
			Down,
			Up
		}

		public enum TimerModes
		{
			SecondsMilliseconds
		}

		private const float DELTA_TIME = 1f / 60f;

		private SpriteFont font;

		private int frames;

		private TimerModes timerMode;

		private Vector2 justify;

		public Action OnComplete;

		public CountModes CountMode;

		public string Text { get; private set; }

		public SpriteFont Font
		{
			get
			{
				return font;
			}
			set
			{
				font = value;
				CalculateOrigin();
			}
		}

		public int Frames
		{
			get
			{
				return frames;
			}
			set
			{
				if (frames != value)
				{
					frames = value;
					UpdateText();
					CalculateOrigin();
				}
			}
		}

		public Vector2 Justify
		{
			get
			{
				return justify;
			}
			set
			{
				justify = value;
				CalculateOrigin();
			}
		}

		public float Width => font.MeasureString(Text).X;

		public float Height => font.MeasureString(Text).Y;

		public TimerText(SpriteFont font, TimerModes mode, CountModes countMode, int frames, Vector2 justify, Action onComplete = null)
			: base(active: true)
		{
			this.font = font;
			timerMode = mode;
			CountMode = countMode;
			this.frames = frames;
			this.justify = justify;
			OnComplete = onComplete;
			UpdateText();
			CalculateOrigin();
		}

		private void UpdateText()
		{
			if (timerMode == TimerModes.SecondsMilliseconds)
			{
				Text = ((float)(frames / 60) + (float)(frames % 60) * (1f / 60f)).ToString("0.00");
			}
		}

		private void CalculateOrigin()
		{
			Origin = (font.MeasureString(Text) * justify).Floor();
		}

		public override void Update()
		{
			base.Update();
			if (CountMode == CountModes.Down)
			{
				if (frames > 0)
				{
					frames--;
					if (frames == 0 && OnComplete != null)
					{
						OnComplete();
					}
					UpdateText();
					CalculateOrigin();
				}
			}
			else
			{
				frames++;
				UpdateText();
				CalculateOrigin();
			}
		}

		public override void Render()
		{
			Draw.SpriteBatch.DrawString(font, Text, base.RenderPosition, Color, Rotation, Origin, Scale, Effects, 0f);
		}
	}
}
