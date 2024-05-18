using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class MirrorSurfaces : Entity
	{
		public const int MaxMirrorOffset = 32;

		private bool hasReflections;

		private VirtualRenderTarget target;

		public MirrorSurfaces()
		{
			base.Depth = 9490;
			base.Tag = Tags.Global;
			Add(new BeforeRenderHook(BeforeRender));
		}

		public void BeforeRender()
		{
			Level level = base.Scene as Level;
			List<Component> sources = base.Scene.Tracker.GetComponents<MirrorReflection>();
			List<Component> surfaces = base.Scene.Tracker.GetComponents<MirrorSurface>();
			if (!(hasReflections = surfaces.Count > 0 && sources.Count > 0))
			{
				return;
			}
			if (target == null)
			{
				target = VirtualContent.CreateRenderTarget("mirror-surfaces", 320, 180);
			}
			Matrix matrix = Matrix.CreateTranslation(32f, 32f, 0f) * level.Camera.Matrix;
			sources.Sort((Component a, Component b) => b.Entity.Depth - a.Entity.Depth);
			Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.MirrorSources);
			Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, matrix);
			foreach (MirrorReflection source in sources)
			{
				if ((source.Entity.Visible || source.IgnoreEntityVisible) && source.Visible)
				{
					source.IsRendering = true;
					source.Entity.Render();
					source.IsRendering = false;
				}
			}
			Draw.SpriteBatch.End();
			Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.MirrorMasks);
			Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone, null, matrix);
			foreach (MirrorSurface surface in surfaces)
			{
				if (surface.Visible && surface.OnRender != null)
				{
					surface.OnRender();
				}
			}
			Draw.SpriteBatch.End();
			Engine.Graphics.GraphicsDevice.SetRenderTarget(target);
			Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
			Engine.Graphics.GraphicsDevice.Textures[1] = (RenderTarget2D)GameplayBuffers.MirrorSources;
			GFX.FxMirrors.Parameters["pixel"].SetValue(new Vector2(1f / (float)GameplayBuffers.MirrorMasks.Width, 1f / (float)GameplayBuffers.MirrorMasks.Height));
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, GFX.FxMirrors, Matrix.Identity);
			Draw.SpriteBatch.Draw((RenderTarget2D)GameplayBuffers.MirrorMasks, new Vector2(-32f, -32f), Color.White);
			Draw.SpriteBatch.End();
		}

		public override void Render()
		{
			if (hasReflections)
			{
				Vector2 cam = FlooredCamera();
				Draw.SpriteBatch.Draw((RenderTarget2D)target, cam, Color.White * 0.5f);
			}
		}

		private Vector2 FlooredCamera()
		{
			Vector2 cam = (base.Scene as Level).Camera.Position;
			cam.X = (int)Math.Floor(cam.X);
			cam.Y = (int)Math.Floor(cam.Y);
			return cam;
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);
			Dispose();
		}

		public override void SceneEnd(Scene scene)
		{
			base.SceneEnd(scene);
			Dispose();
		}

		public void Dispose()
		{
			if (target != null && !target.IsDisposed)
			{
				target.Dispose();
			}
			target = null;
		}
	}
}
