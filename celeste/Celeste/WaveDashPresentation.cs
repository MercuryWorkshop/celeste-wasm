using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class WaveDashPresentation : Entity
	{
		public Vector2 ScaleInPoint = new Vector2(1920f, 1080f) / 2f;

		public readonly int ScreenWidth = 1920;

		public readonly int ScreenHeight = 1080;

		private float ease;

		private bool loading;

		private float waitingForInputTime;

		private VirtualRenderTarget screenBuffer;

		private VirtualRenderTarget prevPageBuffer;

		private VirtualRenderTarget currPageBuffer;

		private int pageIndex;

		private List<WaveDashPage> pages = new List<WaveDashPage>();

		private float pageEase;

		private bool pageTurning;

		private bool pageUpdating;

		private bool waitingForPageTurn;

		private VertexPositionColorTexture[] verts = new VertexPositionColorTexture[6];

		private EventInstance usingSfx;

		public bool Viewing { get; private set; }

		public Atlas Gfx { get; private set; }

		public bool ShowInput
		{
			get
			{
				if (!waitingForPageTurn)
				{
					if (CurrPage != null)
					{
						return CurrPage.WaitingForInput;
					}
					return false;
				}
				return true;
			}
		}

		private WaveDashPage PrevPage
		{
			get
			{
				if (pageIndex <= 0)
				{
					return null;
				}
				return pages[pageIndex - 1];
			}
		}

		private WaveDashPage CurrPage
		{
			get
			{
				if (pageIndex >= pages.Count)
				{
					return null;
				}
				return pages[pageIndex];
			}
		}

		public WaveDashPresentation(EventInstance usingSfx = null)
		{
			base.Tag = Tags.HUD;
			Viewing = true;
			loading = true;
			Add(new Coroutine(Routine()));
			this.usingSfx = usingSfx;
            LoadingThread();
			// RunThread.Start(LoadingThread, "Wave Dash Presentation Loading", highPriority: true);
		}

		private void LoadingThread()
		{
			Gfx = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "WaveDashing"), Atlas.AtlasDataFormat.Packer);
			loading = false;
		}

		private IEnumerator Routine()
		{
			while (loading)
			{
				yield return null;
			}
			pages.Add(new WaveDashPage00());
			pages.Add(new WaveDashPage01());
			pages.Add(new WaveDashPage02());
			pages.Add(new WaveDashPage03());
			pages.Add(new WaveDashPage04());
			pages.Add(new WaveDashPage05());
			pages.Add(new WaveDashPage06());
			foreach (WaveDashPage page in pages)
			{
				page.Added(this);
			}
			Add(new BeforeRenderHook(BeforeRender));
			while (ease < 1f)
			{
				ease = Calc.Approach(ease, 1f, Engine.DeltaTime * 2f);
				yield return null;
			}
			while (pageIndex < pages.Count)
			{
				pageUpdating = true;
				yield return CurrPage.Routine();
				if (!CurrPage.AutoProgress)
				{
					waitingForPageTurn = true;
					while (!Input.MenuConfirm.Pressed)
					{
						yield return null;
					}
					waitingForPageTurn = false;
					Audio.Play("event:/new_content/game/10_farewell/ppt_mouseclick");
				}
				pageUpdating = false;
				pageIndex++;
				if (pageIndex < pages.Count)
				{
					float duration = 0.5f;
					if (CurrPage.Transition == WaveDashPage.Transitions.Rotate3D)
					{
						duration = 1.5f;
					}
					else if (CurrPage.Transition == WaveDashPage.Transitions.Blocky)
					{
						duration = 1f;
					}
					pageTurning = true;
					pageEase = 0f;
					Add(new Coroutine(TurnPage(duration)));
					yield return duration * 0.8f;
				}
			}
			if (usingSfx != null)
			{
				Audio.SetParameter(usingSfx, "end", 1f);
				usingSfx.release();
			}
			Audio.Play("event:/new_content/game/10_farewell/cafe_computer_off");
			while (ease > 0f)
			{
				ease = Calc.Approach(ease, 0f, Engine.DeltaTime * 2f);
				yield return null;
			}
			Viewing = false;
			RemoveSelf();
		}

		private IEnumerator TurnPage(float duration)
		{
			if (CurrPage.Transition != 0 && CurrPage.Transition != WaveDashPage.Transitions.FadeIn)
			{
				if (CurrPage.Transition == WaveDashPage.Transitions.Rotate3D)
				{
					Audio.Play("event:/new_content/game/10_farewell/ppt_cube_transition");
				}
				else if (CurrPage.Transition == WaveDashPage.Transitions.Blocky)
				{
					Audio.Play("event:/new_content/game/10_farewell/ppt_dissolve_transition");
				}
				else if (CurrPage.Transition == WaveDashPage.Transitions.Spiral)
				{
					Audio.Play("event:/new_content/game/10_farewell/ppt_spinning_transition");
				}
			}
			while (pageEase < 1f)
			{
				pageEase += Engine.DeltaTime / duration;
				yield return null;
			}
			pageTurning = false;
		}

		private void BeforeRender()
		{
			if (loading)
			{
				return;
			}
			if (screenBuffer == null || screenBuffer.IsDisposed)
			{
				screenBuffer = VirtualContent.CreateRenderTarget("WaveDash-Buffer", ScreenWidth, ScreenHeight, depth: true);
			}
			if (prevPageBuffer == null || prevPageBuffer.IsDisposed)
			{
				prevPageBuffer = VirtualContent.CreateRenderTarget("WaveDash-Screen1", ScreenWidth, ScreenHeight);
			}
			if (currPageBuffer == null || currPageBuffer.IsDisposed)
			{
				currPageBuffer = VirtualContent.CreateRenderTarget("WaveDash-Screen2", ScreenWidth, ScreenHeight);
			}
			if (pageTurning && PrevPage != null)
			{
				Engine.Graphics.GraphicsDevice.SetRenderTarget(prevPageBuffer);
				Engine.Graphics.GraphicsDevice.Clear(PrevPage.ClearColor);
				Draw.SpriteBatch.Begin();
				PrevPage.Render();
				Draw.SpriteBatch.End();
			}
			if (CurrPage != null)
			{
				Engine.Graphics.GraphicsDevice.SetRenderTarget(currPageBuffer);
				Engine.Graphics.GraphicsDevice.Clear(CurrPage.ClearColor);
				Draw.SpriteBatch.Begin();
				CurrPage.Render();
				Draw.SpriteBatch.End();
			}
			Engine.Graphics.GraphicsDevice.SetRenderTarget(screenBuffer);
			Engine.Graphics.GraphicsDevice.Clear(Color.Black);
			if (pageTurning)
			{
				if (CurrPage.Transition == WaveDashPage.Transitions.ScaleIn)
				{
					Draw.SpriteBatch.Begin();
					Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
					Vector2 scale2 = Vector2.One * pageEase;
					Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, ScaleInPoint, currPageBuffer.Bounds, Color.White, 0f, ScaleInPoint, scale2, SpriteEffects.None, 0f);
					Draw.SpriteBatch.End();
				}
				else if (CurrPage.Transition == WaveDashPage.Transitions.FadeIn)
				{
					Draw.SpriteBatch.Begin();
					Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
					Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, Vector2.Zero, Color.White * pageEase);
					Draw.SpriteBatch.End();
				}
				else if (CurrPage.Transition == WaveDashPage.Transitions.Rotate3D)
				{
					float rot2 = -(float)Math.PI / 2f * pageEase;
					RenderQuad((RenderTarget2D)prevPageBuffer, pageEase, rot2);
					RenderQuad((RenderTarget2D)currPageBuffer, pageEase, (float)Math.PI / 2f + rot2);
				}
				else if (CurrPage.Transition == WaveDashPage.Transitions.Blocky)
				{
					Draw.SpriteBatch.Begin();
					Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
					uint seed = 1u;
					int block = ScreenWidth / 60;
					for (int x = 0; x < ScreenWidth; x += block)
					{
						for (int y = 0; y < ScreenHeight; y += block)
						{
							if (PseudoRandRange(ref seed, 0f, 1f) <= pageEase)
							{
								Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, new Rectangle(x, y, block, block), new Rectangle(x, y, block, block), Color.White);
							}
						}
					}
					Draw.SpriteBatch.End();
				}
				else if (CurrPage.Transition == WaveDashPage.Transitions.Spiral)
				{
					Draw.SpriteBatch.Begin();
					Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
					Vector2 scale = Vector2.One * pageEase;
					float rot = (1f - pageEase) * 12f;
					Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, Celeste.TargetCenter, currPageBuffer.Bounds, Color.White, rot, Celeste.TargetCenter, scale, SpriteEffects.None, 0f);
					Draw.SpriteBatch.End();
				}
			}
			else
			{
				Draw.SpriteBatch.Begin();
				Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, Vector2.Zero, Color.White);
				Draw.SpriteBatch.End();
			}
		}

		private void RenderQuad(Texture texture, float ease, float rotation)
		{
			float aspect = (float)screenBuffer.Width / (float)screenBuffer.Height;
			float w = aspect;
			float h = 1f;
			Vector3 a = new Vector3(0f - w, h, 0f);
			Vector3 b = new Vector3(w, h, 0f);
			Vector3 c = new Vector3(w, 0f - h, 0f);
			Vector3 d = new Vector3(0f - w, 0f - h, 0f);
			verts[0].Position = a;
			verts[0].TextureCoordinate = new Vector2(0f, 0f);
			verts[0].Color = Color.White;
			verts[1].Position = b;
			verts[1].TextureCoordinate = new Vector2(1f, 0f);
			verts[1].Color = Color.White;
			verts[2].Position = c;
			verts[2].TextureCoordinate = new Vector2(1f, 1f);
			verts[2].Color = Color.White;
			verts[3].Position = a;
			verts[3].TextureCoordinate = new Vector2(0f, 0f);
			verts[3].Color = Color.White;
			verts[4].Position = c;
			verts[4].TextureCoordinate = new Vector2(1f, 1f);
			verts[4].Color = Color.White;
			verts[5].Position = d;
			verts[5].TextureCoordinate = new Vector2(0f, 1f);
			verts[5].Color = Color.White;
			float dist = 4.15f + Calc.YoYo(ease) * 1.7f;
			Matrix matrix = Matrix.CreateTranslation(0f, 0f, aspect) * Matrix.CreateRotationY(rotation) * Matrix.CreateTranslation(0f, 0f, 0f - dist) * Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4f, aspect, 1f, 10f);
			Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
			Engine.Instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;
			Engine.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			Engine.Instance.GraphicsDevice.Textures[0] = texture;
			GFX.FxTexture.Parameters["World"].SetValue(matrix);
			foreach (EffectPass pass in GFX.FxTexture.CurrentTechnique.Passes)
			{
				pass.Apply();
				Engine.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, verts.Length / 3);
			}
		}

		public override void Update()
		{
			base.Update();
			if (ShowInput)
			{
				waitingForInputTime += Engine.DeltaTime;
			}
			else
			{
				waitingForInputTime = 0f;
			}
			if (!loading && CurrPage != null && pageUpdating)
			{
				CurrPage.Update();
			}
		}

		public override void Render()
		{
			if (!loading && screenBuffer != null && !screenBuffer.IsDisposed)
			{
				float w = (float)ScreenWidth * Ease.CubeOut(Calc.ClampedMap(ease, 0f, 0.5f));
				float h = (float)ScreenHeight * Ease.CubeInOut(Calc.ClampedMap(ease, 0.5f, 1f, 0.2f));
				Rectangle dest = new Rectangle((int)((1920f - w) / 2f), (int)((1080f - h) / 2f), (int)w, (int)h);
				Draw.SpriteBatch.Draw((RenderTarget2D)screenBuffer, dest, Color.White);
				if (ShowInput && waitingForInputTime > 0.2f)
				{
					GFX.Gui["textboxbutton"].DrawCentered(new Vector2(1856f, 1016 + ((base.Scene.TimeActive % 1f < 0.25f) ? 6 : 0)), Color.Black);
				}
				if ((base.Scene as Level).Paused)
				{
					Draw.Rect(dest, Color.Black * 0.7f);
				}
			}
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

		private void Dispose()
		{
			while (loading)
			{
				Thread.Sleep(1);
			}
			if (screenBuffer != null)
			{
				screenBuffer.Dispose();
			}
			screenBuffer = null;
			if (prevPageBuffer != null)
			{
				prevPageBuffer.Dispose();
			}
			prevPageBuffer = null;
			if (currPageBuffer != null)
			{
				currPageBuffer.Dispose();
			}
			currPageBuffer = null;
			Gfx.Dispose();
			Gfx = null;
		}

		private static uint PseudoRand(ref uint seed)
		{
			uint x = seed;
			x ^= x << 13;
			x ^= x >> 17;
			return seed = x ^ (x << 5);
		}

		public static float PseudoRandRange(ref uint seed, float min, float max)
		{
			return min + (float)(PseudoRand(ref seed) % 1000u) / 1000f * (max - min);
		}
	}
}
