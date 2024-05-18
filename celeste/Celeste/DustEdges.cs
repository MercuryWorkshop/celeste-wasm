using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class DustEdges : Entity
	{
		public static int DustGraphicEstabledCounter;

		private bool hasDust;

		private float noiseEase;

		private Vector2 noiseFromPos;

		private Vector2 noiseToPos;

		private VirtualTexture DustNoiseFrom;

		private VirtualTexture DustNoiseTo;

		public DustEdges()
		{
			AddTag((int)Tags.Global | (int)Tags.TransitionUpdate);
			base.Depth = -48;
			Add(new BeforeRenderHook(BeforeRender));
		}

		private void CreateTextures()
		{
			DustNoiseFrom = VirtualContent.CreateTexture("dust-noise-a", 128, 72, Color.White);
			DustNoiseTo = VirtualContent.CreateTexture("dust-noise-b", 128, 72, Color.White);
			Color[] colors = new Color[DustNoiseFrom.Width * DustNoiseTo.Height];
			for (int j = 0; j < colors.Length; j++)
			{
				colors[j] = new Color(Calc.Random.NextFloat(), 0f, 0f, 0f);
			}
			DustNoiseFrom.Texture.SetData(colors);
			for (int i = 0; i < colors.Length; i++)
			{
				colors[i] = new Color(Calc.Random.NextFloat(), 0f, 0f, 0f);
			}
			DustNoiseTo.Texture.SetData(colors);
		}

		public override void Update()
		{
			noiseEase = Calc.Approach(noiseEase, 1f, Engine.DeltaTime);
			if (noiseEase == 1f)
			{
				VirtualTexture temp = DustNoiseFrom;
				DustNoiseFrom = DustNoiseTo;
				DustNoiseTo = temp;
				noiseFromPos = noiseToPos;
				noiseToPos = new Vector2(Calc.Random.NextFloat(), Calc.Random.NextFloat());
				noiseEase = 0f;
			}
			DustGraphicEstabledCounter = 0;
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

		public override void HandleGraphicsReset()
		{
			base.HandleGraphicsReset();
			Dispose();
		}

		private void Dispose()
		{
			if (DustNoiseFrom != null)
			{
				DustNoiseFrom.Dispose();
			}
			if (DustNoiseTo != null)
			{
				DustNoiseTo.Dispose();
			}
		}

		public void BeforeRender()
		{
			List<Component> dusts = base.Scene.Tracker.GetComponents<DustEdge>();
			hasDust = dusts.Count > 0;
			if (!hasDust)
			{
				return;
			}
			Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.TempA);
			Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, (base.Scene as Level).Camera.Matrix);
			foreach (Component item in dusts)
			{
				DustEdge edge = item as DustEdge;
				if (edge.Visible && edge.Entity.Visible)
				{
					edge.RenderDust();
				}
			}
			Draw.SpriteBatch.End();
			if (DustNoiseFrom == null || DustNoiseFrom.IsDisposed)
			{
				CreateTextures();
			}
			Vector2 cam = FlooredCamera();
			Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.ResortDust);
			Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
			Engine.Graphics.GraphicsDevice.Textures[1] = DustNoiseFrom.Texture;
			Engine.Graphics.GraphicsDevice.Textures[2] = DustNoiseTo.Texture;
			GFX.FxDust.Parameters["colors"].SetValue(DustStyles.Get(base.Scene).EdgeColors);
			GFX.FxDust.Parameters["noiseEase"].SetValue(noiseEase);
			GFX.FxDust.Parameters["noiseFromPos"].SetValue(noiseFromPos + new Vector2(cam.X / 320f, cam.Y / 180f));
			GFX.FxDust.Parameters["noiseToPos"].SetValue(noiseToPos + new Vector2(cam.X / 320f, cam.Y / 180f));
			GFX.FxDust.Parameters["pixel"].SetValue(new Vector2(0.003125f, 1f / 180f));
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, GFX.FxDust, Matrix.Identity);
			Draw.SpriteBatch.Draw((RenderTarget2D)GameplayBuffers.TempA, Vector2.Zero, Color.White);
			Draw.SpriteBatch.End();
		}

		public override void Render()
		{
			if (hasDust)
			{
				Vector2 cam = FlooredCamera();
				Draw.SpriteBatch.Draw((RenderTarget2D)GameplayBuffers.ResortDust, cam, Color.White);
			}
		}

		private Vector2 FlooredCamera()
		{
			Vector2 cam = (base.Scene as Level).Camera.Position;
			cam.X = (int)Math.Floor(cam.X);
			cam.Y = (int)Math.Floor(cam.Y);
			return cam;
		}
	}
}
