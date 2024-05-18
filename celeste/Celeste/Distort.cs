using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public static class Distort
	{
		private static Vector2 anxietyOrigin;

		private static float anxiety = 0f;

		private static float gamerate = 1f;

		private static float waterSine = 0f;

		public static float WaterSineDirection = 1f;

		private static float waterCameraY = 0f;

		private static float waterAlpha = 1f;

		public static Vector2 AnxietyOrigin
		{
			get
			{
				return anxietyOrigin;
			}
			set
			{
				GFX.FxDistort.Parameters["anxietyOrigin"].SetValue(anxietyOrigin = value);
			}
		}

		public static float Anxiety
		{
			get
			{
				return anxiety;
			}
			set
			{
				anxiety = value;
				GFX.FxDistort.Parameters["anxiety"].SetValue((!Settings.Instance.DisableFlashes) ? anxiety : 0f);
			}
		}

		public static float GameRate
		{
			get
			{
				return gamerate;
			}
			set
			{
				GFX.FxDistort.Parameters["gamerate"].SetValue(gamerate = value);
			}
		}

		public static float WaterSine
		{
			get
			{
				return waterSine;
			}
			set
			{
				GFX.FxDistort.Parameters["waterSine"].SetValue(waterSine = WaterSineDirection * value);
			}
		}

		public static float WaterCameraY
		{
			get
			{
				return waterCameraY;
			}
			set
			{
				GFX.FxDistort.Parameters["waterCameraY"].SetValue(waterCameraY = value);
			}
		}

		public static float WaterAlpha
		{
			get
			{
				return waterAlpha;
			}
			set
			{
				GFX.FxDistort.Parameters["waterAlpha"].SetValue(waterAlpha = value);
			}
		}

		public static void Render(Texture2D source, Texture2D map, bool hasDistortion)
		{
			Effect effect = GFX.FxDistort;
			if (effect != null && (anxiety > 0f || gamerate < 1f || hasDistortion))
			{
				if (anxiety > 0f || gamerate < 1f)
				{
					effect.CurrentTechnique = effect.Techniques["Distort"];
				}
				else
				{
					effect.CurrentTechnique = effect.Techniques["Displace"];
				}
				Engine.Graphics.GraphicsDevice.Textures[1] = map;
				Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, effect);
				Draw.SpriteBatch.Draw(source, Vector2.Zero, Color.White);
				Draw.SpriteBatch.End();
			}
			else
			{
				Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
				Draw.SpriteBatch.Draw(source, Vector2.Zero, Color.White);
				Draw.SpriteBatch.End();
			}
		}
	}
}
