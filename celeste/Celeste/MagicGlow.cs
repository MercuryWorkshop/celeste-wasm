using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public static class MagicGlow
	{
		public static void Render(Texture2D texture, float noiseEase, float direction, Matrix matrix)
		{
			GFX.FxMagicGlow.Parameters["alpha"].SetValue(0.5f);
			GFX.FxMagicGlow.Parameters["pixel"].SetValue(new Vector2(1f / (float)texture.Width, 1f / (float)texture.Height) * 3f);
			GFX.FxMagicGlow.Parameters["noiseSample"].SetValue(new Vector2(1f, 0.5f));
			GFX.FxMagicGlow.Parameters["noiseDistort"].SetValue(new Vector2(1f, 1f));
			GFX.FxMagicGlow.Parameters["noiseEase"].SetValue(noiseEase * 0.05f);
			GFX.FxMagicGlow.Parameters["direction"].SetValue(0f - direction);
			Engine.Graphics.GraphicsDevice.Textures[1] = GFX.MagicGlowNoise.Texture;
			Engine.Graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, GFX.FxMagicGlow, matrix);
			Draw.SpriteBatch.Draw(texture, Vector2.Zero, Color.White);
			Draw.SpriteBatch.End();
		}
	}
}
