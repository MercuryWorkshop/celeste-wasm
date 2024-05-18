using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Monocle
{
	public class Text : GraphicsComponent
	{
		public enum HorizontalAlign
		{
			Left,
			Center,
			Right
		}

		public enum VerticalAlign
		{
			Top,
			Center,
			Bottom
		}

		private SpriteFont font;

		private string text;

		private HorizontalAlign horizontalOrigin;

		private VerticalAlign verticalOrigin;

		private Vector2 size;

		public SpriteFont Font
		{
			get
			{
				return font;
			}
			set
			{
				font = value;
				UpdateSize();
			}
		}

		public string DrawText
		{
			get
			{
				return text;
			}
			set
			{
				text = value;
				UpdateSize();
			}
		}

		public HorizontalAlign HorizontalOrigin
		{
			get
			{
				return horizontalOrigin;
			}
			set
			{
				horizontalOrigin = value;
				UpdateCentering();
			}
		}

		public VerticalAlign VerticalOrigin
		{
			get
			{
				return verticalOrigin;
			}
			set
			{
				verticalOrigin = value;
				UpdateCentering();
			}
		}

		public float Width => size.X;

		public float Height => size.Y;

		public Text(SpriteFont font, string text, Vector2 position, Color color, HorizontalAlign horizontalAlign = HorizontalAlign.Center, VerticalAlign verticalAlign = VerticalAlign.Center)
			: base(active: false)
		{
			this.font = font;
			this.text = text;
			Position = position;
			Color = color;
			horizontalOrigin = horizontalAlign;
			verticalOrigin = verticalAlign;
			UpdateSize();
		}

		public Text(SpriteFont font, string text, Vector2 position, HorizontalAlign horizontalAlign = HorizontalAlign.Center, VerticalAlign verticalAlign = VerticalAlign.Center)
			: this(font, text, position, Color.White, horizontalAlign, verticalAlign)
		{
		}

		private void UpdateSize()
		{
			size = font.MeasureString(text);
			UpdateCentering();
		}

		private void UpdateCentering()
		{
			if (horizontalOrigin == HorizontalAlign.Left)
			{
				Origin.X = 0f;
			}
			else if (horizontalOrigin == HorizontalAlign.Center)
			{
				Origin.X = size.X / 2f;
			}
			else
			{
				Origin.X = size.X;
			}
			if (verticalOrigin == VerticalAlign.Top)
			{
				Origin.Y = 0f;
			}
			else if (verticalOrigin == VerticalAlign.Center)
			{
				Origin.Y = size.Y / 2f;
			}
			else
			{
				Origin.Y = size.Y;
			}
			Origin = Origin.Floor();
		}

		public override void Render()
		{
			Draw.SpriteBatch.DrawString(font, text, base.RenderPosition, Color, Rotation, Origin, Scale, Effects, 0f);
		}
	}
}
