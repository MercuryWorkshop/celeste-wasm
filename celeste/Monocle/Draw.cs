using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Monocle
{
	public static class Draw
	{
		public static MTexture Particle;

		public static MTexture Pixel;

		private static Rectangle rect;

		public static Renderer Renderer { get; internal set; }

		public static SpriteBatch SpriteBatch { get; private set; }

		public static SpriteFont DefaultFont { get; private set; }

		internal static void Initialize(GraphicsDevice graphicsDevice)
		{
			SpriteBatch = new SpriteBatch(graphicsDevice);
			DefaultFont = Engine.Instance.Content.Load<SpriteFont>("Monocle\\MonocleDefault");
			UseDebugPixelTexture();
		}

		public static void UseDebugPixelTexture()
		{
			MTexture parent = new MTexture(VirtualContent.CreateTexture("debug-pixel", 3, 3, Color.White));
			Pixel = new MTexture(parent, 1, 1, 1, 1);
			Particle = new MTexture(parent, 1, 1, 1, 1);
		}

		public static void Point(Vector2 at, Color color)
		{
			SpriteBatch.Draw(Pixel.Texture.Texture, at, Pixel.ClipRect, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}

		public static void Line(Vector2 start, Vector2 end, Color color)
		{
			LineAngle(start, Calc.Angle(start, end), Vector2.Distance(start, end), color);
		}

		public static void Line(Vector2 start, Vector2 end, Color color, float thickness)
		{
			LineAngle(start, Calc.Angle(start, end), Vector2.Distance(start, end), color, thickness);
		}

		public static void Line(float x1, float y1, float x2, float y2, Color color)
		{
			Line(new Vector2(x1, y1), new Vector2(x2, y2), color);
		}

		public static void Line(float x1, float y1, float x2, float y2, Color color, float thickness)
		{
			Line(new Vector2(x1, y1), new Vector2(x2, y2), color, thickness);
		}

		public static void LineAngle(Vector2 start, float angle, float length, Color color)
		{
			SpriteBatch.Draw(Pixel.Texture.Texture, start, Pixel.ClipRect, color, angle, Vector2.Zero, new Vector2(length, 1f), SpriteEffects.None, 0f);
		}

		public static void LineAngle(Vector2 start, float angle, float length, Color color, float thickness)
		{
			SpriteBatch.Draw(Pixel.Texture.Texture, start, Pixel.ClipRect, color, angle, new Vector2(0f, 0.5f), new Vector2(length, thickness), SpriteEffects.None, 0f);
		}

		public static void LineAngle(float startX, float startY, float angle, float length, Color color)
		{
			LineAngle(new Vector2(startX, startY), angle, length, color);
		}

		public static void Circle(Vector2 position, float radius, Color color, int resolution)
		{
			Vector2 last = Vector2.UnitX * radius;
			Vector2 lastP = last.Perpendicular();
			for (int i = 1; i <= resolution; i++)
			{
				Vector2 at = Calc.AngleToVector((float)i * ((float)Math.PI / 2f) / (float)resolution, radius);
				Vector2 atP = at.Perpendicular();
				Line(position + last, position + at, color);
				Line(position - last, position - at, color);
				Line(position + lastP, position + atP, color);
				Line(position - lastP, position - atP, color);
				last = at;
				lastP = atP;
			}
		}

		public static void Circle(float x, float y, float radius, Color color, int resolution)
		{
			Circle(new Vector2(x, y), radius, color, resolution);
		}

		public static void Circle(Vector2 position, float radius, Color color, float thickness, int resolution)
		{
			Vector2 last = Vector2.UnitX * radius;
			Vector2 lastP = last.Perpendicular();
			for (int i = 1; i <= resolution; i++)
			{
				Vector2 at = Calc.AngleToVector((float)i * ((float)Math.PI / 2f) / (float)resolution, radius);
				Vector2 atP = at.Perpendicular();
				Line(position + last, position + at, color, thickness);
				Line(position - last, position - at, color, thickness);
				Line(position + lastP, position + atP, color, thickness);
				Line(position - lastP, position - atP, color, thickness);
				last = at;
				lastP = atP;
			}
		}

		public static void Circle(float x, float y, float radius, Color color, float thickness, int resolution)
		{
			Circle(new Vector2(x, y), radius, color, thickness, resolution);
		}

		public static void Rect(float x, float y, float width, float height, Color color)
		{
			rect.X = (int)x;
			rect.Y = (int)y;
			rect.Width = (int)width;
			rect.Height = (int)height;
			SpriteBatch.Draw(Pixel.Texture.Texture, rect, Pixel.ClipRect, color);
		}

		public static void Rect(Vector2 position, float width, float height, Color color)
		{
			Rect(position.X, position.Y, width, height, color);
		}

		public static void Rect(Rectangle rect, Color color)
		{
			Draw.rect = rect;
			SpriteBatch.Draw(Pixel.Texture.Texture, rect, Pixel.ClipRect, color);
		}

		public static void Rect(Collider collider, Color color)
		{
			Rect(collider.AbsoluteLeft, collider.AbsoluteTop, collider.Width, collider.Height, color);
		}

		public static void HollowRect(float x, float y, float width, float height, Color color)
		{
			rect.X = (int)x;
			rect.Y = (int)y;
			rect.Width = (int)width;
			rect.Height = 1;
			SpriteBatch.Draw(Pixel.Texture.Texture, rect, Pixel.ClipRect, color);
			rect.Y += (int)height - 1;
			SpriteBatch.Draw(Pixel.Texture.Texture, rect, Pixel.ClipRect, color);
			rect.Y -= (int)height - 1;
			rect.Width = 1;
			rect.Height = (int)height;
			SpriteBatch.Draw(Pixel.Texture.Texture, rect, Pixel.ClipRect, color);
			rect.X += (int)width - 1;
			SpriteBatch.Draw(Pixel.Texture.Texture, rect, Pixel.ClipRect, color);
		}

		public static void HollowRect(Vector2 position, float width, float height, Color color)
		{
			HollowRect(position.X, position.Y, width, height, color);
		}

		public static void HollowRect(Rectangle rect, Color color)
		{
			HollowRect(rect.X, rect.Y, rect.Width, rect.Height, color);
		}

		public static void HollowRect(Collider collider, Color color)
		{
			HollowRect(collider.AbsoluteLeft, collider.AbsoluteTop, collider.Width, collider.Height, color);
		}

		public static void Text(SpriteFont font, string text, Vector2 position, Color color)
		{
			SpriteBatch.DrawString(font, text, position.Floor(), color);
		}

		public static void Text(SpriteFont font, string text, Vector2 position, Color color, Vector2 origin, Vector2 scale, float rotation)
		{
			SpriteBatch.DrawString(font, text, position.Floor(), color, rotation, origin, scale, SpriteEffects.None, 0f);
		}

		public static void TextJustified(SpriteFont font, string text, Vector2 position, Color color, Vector2 justify)
		{
			Vector2 origin = font.MeasureString(text);
			origin.X *= justify.X;
			origin.Y *= justify.Y;
			SpriteBatch.DrawString(font, text, position.Floor(), color, 0f, origin, 1f, SpriteEffects.None, 0f);
		}

		public static void TextJustified(SpriteFont font, string text, Vector2 position, Color color, float scale, Vector2 justify)
		{
			Vector2 origin = font.MeasureString(text);
			origin.X *= justify.X;
			origin.Y *= justify.Y;
			SpriteBatch.DrawString(font, text, position.Floor(), color, 0f, origin, scale, SpriteEffects.None, 0f);
		}

		public static void TextCentered(SpriteFont font, string text, Vector2 position)
		{
			Text(font, text, position - font.MeasureString(text) * 0.5f, Color.White);
		}

		public static void TextCentered(SpriteFont font, string text, Vector2 position, Color color)
		{
			Text(font, text, position - font.MeasureString(text) * 0.5f, color);
		}

		public static void TextCentered(SpriteFont font, string text, Vector2 position, Color color, float scale)
		{
			Text(font, text, position, color, font.MeasureString(text) * 0.5f, Vector2.One * scale, 0f);
		}

		public static void TextCentered(SpriteFont font, string text, Vector2 position, Color color, float scale, float rotation)
		{
			Text(font, text, position, color, font.MeasureString(text) * 0.5f, Vector2.One * scale, rotation);
		}

		public static void OutlineTextCentered(SpriteFont font, string text, Vector2 position, Color color, float scale)
		{
			Vector2 origin = font.MeasureString(text) / 2f;
			for (int i = -1; i < 2; i++)
			{
				for (int j = -1; j < 2; j++)
				{
					if (i != 0 || j != 0)
					{
						SpriteBatch.DrawString(font, text, position.Floor() + new Vector2(i, j), Color.Black, 0f, origin, scale, SpriteEffects.None, 0f);
					}
				}
			}
			SpriteBatch.DrawString(font, text, position.Floor(), color, 0f, origin, scale, SpriteEffects.None, 0f);
		}

		public static void OutlineTextCentered(SpriteFont font, string text, Vector2 position, Color color, Color outlineColor)
		{
			Vector2 origin = font.MeasureString(text) / 2f;
			for (int i = -1; i < 2; i++)
			{
				for (int j = -1; j < 2; j++)
				{
					if (i != 0 || j != 0)
					{
						SpriteBatch.DrawString(font, text, position.Floor() + new Vector2(i, j), outlineColor, 0f, origin, 1f, SpriteEffects.None, 0f);
					}
				}
			}
			SpriteBatch.DrawString(font, text, position.Floor(), color, 0f, origin, 1f, SpriteEffects.None, 0f);
		}

		public static void OutlineTextCentered(SpriteFont font, string text, Vector2 position, Color color, Color outlineColor, float scale)
		{
			Vector2 origin = font.MeasureString(text) / 2f;
			for (int i = -1; i < 2; i++)
			{
				for (int j = -1; j < 2; j++)
				{
					if (i != 0 || j != 0)
					{
						SpriteBatch.DrawString(font, text, position.Floor() + new Vector2(i, j), outlineColor, 0f, origin, scale, SpriteEffects.None, 0f);
					}
				}
			}
			SpriteBatch.DrawString(font, text, position.Floor(), color, 0f, origin, scale, SpriteEffects.None, 0f);
		}

		public static void OutlineTextJustify(SpriteFont font, string text, Vector2 position, Color color, Color outlineColor, Vector2 justify)
		{
			Vector2 origin = font.MeasureString(text) * justify;
			for (int i = -1; i < 2; i++)
			{
				for (int j = -1; j < 2; j++)
				{
					if (i != 0 || j != 0)
					{
						SpriteBatch.DrawString(font, text, position.Floor() + new Vector2(i, j), outlineColor, 0f, origin, 1f, SpriteEffects.None, 0f);
					}
				}
			}
			SpriteBatch.DrawString(font, text, position.Floor(), color, 0f, origin, 1f, SpriteEffects.None, 0f);
		}

		public static void OutlineTextJustify(SpriteFont font, string text, Vector2 position, Color color, Color outlineColor, Vector2 justify, float scale)
		{
			Vector2 origin = font.MeasureString(text) * justify;
			for (int i = -1; i < 2; i++)
			{
				for (int j = -1; j < 2; j++)
				{
					if (i != 0 || j != 0)
					{
						SpriteBatch.DrawString(font, text, position.Floor() + new Vector2(i, j), outlineColor, 0f, origin, scale, SpriteEffects.None, 0f);
					}
				}
			}
			SpriteBatch.DrawString(font, text, position.Floor(), color, 0f, origin, scale, SpriteEffects.None, 0f);
		}

		public static void SineTextureH(MTexture tex, Vector2 position, Vector2 origin, Vector2 scale, float rotation, Color color, SpriteEffects effects, float sineCounter, float amplitude = 2f, int sliceSize = 2, float sliceAdd = (float)Math.PI / 4f)
		{
			position = position.Floor();
			Rectangle clip = tex.ClipRect;
			clip.Width = sliceSize;
			int num = 0;
			while (clip.X < tex.ClipRect.X + tex.ClipRect.Width)
			{
				Vector2 add = new Vector2(sliceSize * num, (float)Math.Round(Math.Sin(sineCounter + sliceAdd * (float)num) * (double)amplitude));
				SpriteBatch.Draw(tex.Texture.Texture, position, clip, color, rotation, origin - add, scale, effects, 0f);
				num++;
				clip.X += sliceSize;
				clip.Width = Math.Min(sliceSize, tex.ClipRect.X + tex.ClipRect.Width - clip.X);
			}
		}

		public static void SineTextureV(MTexture tex, Vector2 position, Vector2 origin, Vector2 scale, float rotation, Color color, SpriteEffects effects, float sineCounter, float amplitude = 2f, int sliceSize = 2, float sliceAdd = (float)Math.PI / 4f)
		{
			position = position.Floor();
			Rectangle clip = tex.ClipRect;
			clip.Height = sliceSize;
			int num = 0;
			while (clip.Y < tex.ClipRect.Y + tex.ClipRect.Height)
			{
				Vector2 add = new Vector2((float)Math.Round(Math.Sin(sineCounter + sliceAdd * (float)num) * (double)amplitude), sliceSize * num);
				SpriteBatch.Draw(tex.Texture.Texture, position, clip, color, rotation, origin - add, scale, effects, 0f);
				num++;
				clip.Y += sliceSize;
				clip.Height = Math.Min(sliceSize, tex.ClipRect.Y + tex.ClipRect.Height - clip.Y);
			}
		}

		public static void TextureBannerV(MTexture tex, Vector2 position, Vector2 origin, Vector2 scale, float rotation, Color color, SpriteEffects effects, float sineCounter, float amplitude = 2f, int sliceSize = 2, float sliceAdd = (float)Math.PI / 4f)
		{
			position = position.Floor();
			Rectangle clip = tex.ClipRect;
			clip.Height = sliceSize;
			int num = 0;
			while (clip.Y < tex.ClipRect.Y + tex.ClipRect.Height)
			{
				float fade = (float)(clip.Y - tex.ClipRect.Y) / (float)tex.ClipRect.Height;
				clip.Height = (int)MathHelper.Lerp(sliceSize, 1f, fade);
				clip.Height = Math.Min(sliceSize, tex.ClipRect.Y + tex.ClipRect.Height - clip.Y);
				Vector2 add = new Vector2((float)Math.Round(Math.Sin(sineCounter + sliceAdd * (float)num) * (double)amplitude * (double)fade), clip.Y - tex.ClipRect.Y);
				SpriteBatch.Draw(tex.Texture.Texture, position, clip, color, rotation, origin - add, scale, effects, 0f);
				num++;
				clip.Y += clip.Height;
			}
		}
	}
}
