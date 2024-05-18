using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class Parallax : Backdrop
	{
		public Vector2 CameraOffset = Vector2.Zero;

		public BlendState BlendState = BlendState.AlphaBlend;

		public MTexture Texture;

		public bool DoFadeIn;

		public float Alpha = 1f;

		private float fadeIn = 1f;

		public Parallax(MTexture texture)
		{
			Name = texture.AtlasPath;
			Texture = texture;
		}

		public override void Update(Scene scene)
		{
			base.Update(scene);
			Position += Speed * Engine.DeltaTime;
			Position += WindMultiplier * (scene as Level).Wind * Engine.DeltaTime;
			if (DoFadeIn)
			{
				fadeIn = Calc.Approach(fadeIn, Visible ? 1 : 0, Engine.DeltaTime);
			}
			else
			{
				fadeIn = (Visible ? 1 : 0);
			}
		}

		public override void Render(Scene scene)
		{
			Vector2 cam = ((scene as Level).Camera.Position + CameraOffset).Floor();
			Vector2 origin = (Position - cam * Scroll).Floor();
			float alpha = fadeIn * Alpha * FadeAlphaMultiplier;
			if (FadeX != null)
			{
				alpha *= FadeX.Value(cam.X + 160f);
			}
			if (FadeY != null)
			{
				alpha *= FadeY.Value(cam.Y + 90f);
			}
			Color color = Color;
			if (alpha < 1f)
			{
				color *= alpha;
			}
			if (color.A <= 1)
			{
				return;
			}
			if (LoopX)
			{
				while (origin.X < 0f)
				{
					origin.X += Texture.Width;
				}
				while (origin.X > 0f)
				{
					origin.X -= Texture.Width;
				}
			}
			if (LoopY)
			{
				while (origin.Y < 0f)
				{
					origin.Y += Texture.Height;
				}
				while (origin.Y > 0f)
				{
					origin.Y -= Texture.Height;
				}
			}
			SpriteEffects flip = SpriteEffects.None;
			if (FlipX && FlipY)
			{
				flip = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
			}
			else if (FlipX)
			{
				flip = SpriteEffects.FlipHorizontally;
			}
			else if (FlipY)
			{
				flip = SpriteEffects.FlipVertically;
			}
			for (float x = origin.X; x < 320f; x += (float)Texture.Width)
			{
				for (float y = origin.Y; y < 180f; y += (float)Texture.Height)
				{
					Texture.Draw(new Vector2(x, y), Vector2.Zero, color, 1f, 0f, flip);
					if (!LoopY)
					{
						break;
					}
				}
				if (!LoopX)
				{
					break;
				}
			}
		}
	}
}
