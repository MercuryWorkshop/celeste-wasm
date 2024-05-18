using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public static class Glitch
	{
		public static float Value;

		public static void Apply(VirtualRenderTarget source, float timer, float seed, float amplitude)
		{
			if (Value > 0f && !Settings.Instance.DisableFlashes)
			{
				Effect effect = GFX.FxGlitch;
				Vector2 size = new Vector2(Engine.Graphics.GraphicsDevice.Viewport.Width, Engine.Graphics.GraphicsDevice.Viewport.Height);
				effect.Parameters["dimensions"].SetValue(size);
				effect.Parameters["amplitude"].SetValue(amplitude);
				effect.Parameters["minimum"].SetValue(-1f);
				effect.Parameters["glitch"].SetValue(Value);
				effect.Parameters["timer"].SetValue(timer);
				effect.Parameters["seed"].SetValue(seed);
				VirtualRenderTarget result = GameplayBuffers.TempA;
				Engine.Instance.GraphicsDevice.SetRenderTarget(result);
				Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
				Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, effect);
				Draw.SpriteBatch.Draw((RenderTarget2D)source, Vector2.Zero, Color.White);
				Draw.SpriteBatch.End();
				Engine.Instance.GraphicsDevice.SetRenderTarget(source);
				Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
				Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, effect);
				Draw.SpriteBatch.Draw((RenderTarget2D)result, Vector2.Zero, Color.White);
				Draw.SpriteBatch.End();
			}
		}
	}
}
