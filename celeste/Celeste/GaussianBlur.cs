using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public static class GaussianBlur
	{
		public enum Samples
		{
			Three,
			Five,
			Nine
		}

		public enum Direction
		{
			Both,
			Horizontal,
			Vertical
		}

		private static string[] techniques = new string[3] { "GaussianBlur3", "GaussianBlur5", "GaussianBlur9" };

		public static Texture2D Blur(Texture2D texture, VirtualRenderTarget temp, VirtualRenderTarget output, float fade = 0f, bool clear = true, Samples samples = Samples.Nine, float sampleScale = 1f, Direction direction = Direction.Both, float alpha = 1f)
		{
			Effect effect = GFX.FxGaussianBlur;
			string technique = techniques[(int)samples];
			if (effect != null)
			{
				effect.CurrentTechnique = effect.Techniques[technique];
				effect.Parameters["fade"].SetValue(fade);
				effect.Parameters["pixel"].SetValue(new Vector2(1f / (float)temp.Width, 0f) * sampleScale);
				Engine.Instance.GraphicsDevice.SetRenderTarget(temp);
				if (clear)
				{
					Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
				}
				Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, (direction != Direction.Vertical) ? effect : null);
				Draw.SpriteBatch.Draw(texture, new Rectangle(0, 0, temp.Width, temp.Height), Color.White);
				Draw.SpriteBatch.End();
				effect.Parameters["pixel"].SetValue(new Vector2(0f, 1f / (float)output.Height) * sampleScale);
				Engine.Instance.GraphicsDevice.SetRenderTarget(output);
				if (clear)
				{
					Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
				}
				Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, (direction != Direction.Horizontal) ? effect : null);
				Draw.SpriteBatch.Draw((RenderTarget2D)temp, new Rectangle(0, 0, output.Width, output.Height), Color.White);
				Draw.SpriteBatch.End();
				return (RenderTarget2D)output;
			}
			return texture;
		}
	}
}
