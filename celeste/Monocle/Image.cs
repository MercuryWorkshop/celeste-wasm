using Microsoft.Xna.Framework;

namespace Monocle
{
	public class Image : GraphicsComponent
	{
		public MTexture Texture;

		public bool TEST;

		public virtual float Width => Texture.Width;

		public virtual float Height => Texture.Height;

		public Image(MTexture texture)
			: base(active: false)
		{
			Texture = texture;
		}

		internal Image(MTexture texture, bool active)
			: base(active)
		{
			Texture = texture;
		}

		public override void Render()
		{
			if (Texture != null)
			{
				Texture.Draw(base.RenderPosition, Origin, Color, Scale, Rotation, Effects);
			}
		}

		public Image SetOrigin(float x, float y)
		{
			Origin.X = x;
			Origin.Y = y;
			return this;
		}

		public Image CenterOrigin()
		{
			Origin.X = Width / 2f;
			Origin.Y = Height / 2f;
			return this;
		}

		public Image JustifyOrigin(Vector2 at)
		{
			Origin.X = Width * at.X;
			Origin.Y = Height * at.Y;
			return this;
		}

		public Image JustifyOrigin(float x, float y)
		{
			Origin.X = Width * x;
			Origin.Y = Height * y;
			return this;
		}

		public Image SetColor(Color color)
		{
			Color = color;
			return this;
		}
	}
}
