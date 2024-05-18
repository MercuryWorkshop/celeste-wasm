using System;
using Microsoft.Xna.Framework.Graphics;

namespace Monocle
{
	public class NumberText : GraphicsComponent
	{
		private SpriteFont font;

		private int value;

		private string prefix;

		private string drawString;

		private bool centered;

		public Action<int> OnValueUpdate;

		public int Value
		{
			get
			{
				return value;
			}
			set
			{
				if (this.value != value)
				{
					int oldValue = this.value;
					this.value = value;
					UpdateString();
					if (OnValueUpdate != null)
					{
						OnValueUpdate(oldValue);
					}
				}
			}
		}

		public float Width => font.MeasureString(drawString).X;

		public float Height => font.MeasureString(drawString).Y;

		public NumberText(SpriteFont font, string prefix, int value, bool centered = false)
			: base(active: false)
		{
			this.font = font;
			this.prefix = prefix;
			this.value = value;
			this.centered = centered;
			UpdateString();
		}

		public void UpdateString()
		{
			drawString = prefix + value;
			if (centered)
			{
				Origin = (font.MeasureString(drawString) / 2f).Floor();
			}
		}

		public override void Render()
		{
			Draw.SpriteBatch.DrawString(font, drawString, base.RenderPosition, Color, Rotation, Origin, Scale, Effects, 0f);
		}
	}
}
