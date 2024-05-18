using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class BloomRenderer : Renderer
	{
		public float Strength = 1f;

		public float Base;

		private MTexture gradient;

		public static readonly BlendState BlurredScreenToMask = new BlendState
		{
			ColorSourceBlend = Blend.One,
			ColorDestinationBlend = Blend.Zero,
			ColorBlendFunction = BlendFunction.Add,
			AlphaSourceBlend = Blend.Zero,
			AlphaDestinationBlend = Blend.One,
			AlphaBlendFunction = BlendFunction.Add
		};

		public static readonly BlendState AdditiveMaskToScreen = new BlendState
		{
			ColorSourceBlend = Blend.SourceAlpha,
			ColorDestinationBlend = Blend.One,
			ColorBlendFunction = BlendFunction.Add,
			AlphaSourceBlend = Blend.Zero,
			AlphaDestinationBlend = Blend.One,
			AlphaBlendFunction = BlendFunction.Add
		};

		public static readonly BlendState CutoutBlendstate = new BlendState
		{
			ColorSourceBlend = Blend.One,
			ColorDestinationBlend = Blend.One,
			AlphaSourceBlend = Blend.One,
			AlphaDestinationBlend = Blend.One,
			ColorBlendFunction = BlendFunction.Min,
			AlphaBlendFunction = BlendFunction.Min
		};

		public BloomRenderer()
		{
			gradient = GFX.Game["util/bloomgradient"];
		}

		public void Apply(VirtualRenderTarget target, Scene scene)
		{
			if (!(Strength > 0f))
			{
				return;
			}
			VirtualRenderTarget result = GameplayBuffers.TempA;
			Texture2D blurred = GaussianBlur.Blur((RenderTarget2D)target, GameplayBuffers.TempA, GameplayBuffers.TempB);
			List<Component> points = scene.Tracker.GetComponents<BloomPoint>();
			List<Component> cutouts = scene.Tracker.GetComponents<EffectCutout>();
			Engine.Instance.GraphicsDevice.SetRenderTarget(result);
			Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
			if (Base < 1f)
			{
				Camera camera = (scene as Level).Camera;
				Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, camera.Matrix);
				float scaleMult = 1f / (float)gradient.Width;
				foreach (Component item in points)
				{
					BloomPoint point = item as BloomPoint;
					if (point.Visible && !(point.Radius <= 0f) && !(point.Alpha <= 0f))
					{
						gradient.DrawCentered(point.Entity.Position + point.Position, Color.White * point.Alpha, point.Radius * 2f * scaleMult);
					}
				}
				foreach (CustomBloom custom in scene.Tracker.GetComponents<CustomBloom>())
				{
					if (custom.Visible && custom.OnRenderBloom != null)
					{
						custom.OnRenderBloom();
					}
				}
				foreach (Entity entity in scene.Tracker.GetEntities<SeekerBarrier>())
				{
					Draw.Rect(entity.Collider, Color.White);
				}
				Draw.SpriteBatch.End();
				if (cutouts.Count > 0)
				{
					Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, CutoutBlendstate, SamplerState.PointClamp, null, null, null, camera.Matrix);
					foreach (Component item2 in cutouts)
					{
						EffectCutout cutout = item2 as EffectCutout;
						if (cutout.Visible)
						{
							Draw.Rect(cutout.Left, cutout.Top, cutout.Right - cutout.Left, cutout.Bottom - cutout.Top, Color.White * (1f - cutout.Alpha));
						}
					}
					Draw.SpriteBatch.End();
				}
			}
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
			Draw.Rect(-10f, -10f, 340f, 200f, Color.White * Base);
			Draw.SpriteBatch.End();
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlurredScreenToMask);
			Draw.SpriteBatch.Draw(blurred, Vector2.Zero, Color.White);
			Draw.SpriteBatch.End();
			Engine.Instance.GraphicsDevice.SetRenderTarget(target);
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, AdditiveMaskToScreen);
			for (int i = 0; (float)i < Strength; i++)
			{
				float s = (((float)i < Strength - 1f) ? 1f : (Strength - (float)i));
				Draw.SpriteBatch.Draw((RenderTarget2D)result, Vector2.Zero, Color.White * s);
			}
			Draw.SpriteBatch.End();
		}
	}
}
