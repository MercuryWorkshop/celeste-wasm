using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class GlassBlockBg : Entity
	{
		private struct Star
		{
			public Vector2 Position;

			public MTexture Texture;

			public Color Color;

			public Vector2 Scroll;
		}

		private struct Ray
		{
			public Vector2 Position;

			public float Width;

			public float Length;

			public Color Color;
		}

		private static readonly Color[] starColors = new Color[3]
		{
			Calc.HexToColor("7f9fba"),
			Calc.HexToColor("9bd1cd"),
			Calc.HexToColor("bacae3")
		};

		private const int StarCount = 100;

		private const int RayCount = 50;

		private Star[] stars = new Star[100];

		private Ray[] rays = new Ray[50];

		private VertexPositionColor[] verts = new VertexPositionColor[2700];

		private Vector2 rayNormal = new Vector2(-5f, -8f).SafeNormalize();

		private Color bgColor = Calc.HexToColor("0d2e89");

		private VirtualRenderTarget beamsTarget;

		private VirtualRenderTarget starsTarget;

		private bool hasBlocks;

		public GlassBlockBg()
		{
			base.Tag = Tags.Global;
			Add(new BeforeRenderHook(BeforeRender));
			Add(new DisplacementRenderHook(OnDisplacementRender));
			base.Depth = -9990;
			List<MTexture> textures = GFX.Game.GetAtlasSubtextures("particles/stars/");
			for (int j = 0; j < stars.Length; j++)
			{
				stars[j].Position.X = Calc.Random.Next(320);
				stars[j].Position.Y = Calc.Random.Next(180);
				stars[j].Texture = Calc.Random.Choose(textures);
				stars[j].Color = Calc.Random.Choose(starColors);
				stars[j].Scroll = Vector2.One * Calc.Random.NextFloat(0.05f);
			}
			for (int i = 0; i < rays.Length; i++)
			{
				rays[i].Position.X = Calc.Random.Next(320);
				rays[i].Position.Y = Calc.Random.Next(180);
				rays[i].Width = Calc.Random.Range(4f, 16f);
				rays[i].Length = Calc.Random.Choose(48, 96, 128);
				rays[i].Color = Color.White * Calc.Random.Range(0.2f, 0.4f);
			}
		}

		private void BeforeRender()
		{
			List<Entity> blocks = base.Scene.Tracker.GetEntities<GlassBlock>();
			if (!(hasBlocks = blocks.Count > 0))
			{
				return;
			}
			Camera camera = (base.Scene as Level).Camera;
			int w = 320;
			int h = 180;
			if (starsTarget == null)
			{
				starsTarget = VirtualContent.CreateRenderTarget("glass-block-surfaces", 320, 180);
			}
			Engine.Graphics.GraphicsDevice.SetRenderTarget(starsTarget);
			Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
			Vector2 orig = new Vector2(8f, 8f);
			for (int j = 0; j < stars.Length; j++)
			{
				MTexture tex = stars[j].Texture;
				Color col = stars[j].Color;
				Vector2 scroll = stars[j].Scroll;
				Vector2 at = default(Vector2);
				at.X = Mod(stars[j].Position.X - camera.X * (1f - scroll.X), w);
				at.Y = Mod(stars[j].Position.Y - camera.Y * (1f - scroll.Y), h);
				tex.Draw(at, orig, col);
				if (at.X < orig.X)
				{
					tex.Draw(at + new Vector2(w, 0f), orig, col);
				}
				else if (at.X > (float)w - orig.X)
				{
					tex.Draw(at - new Vector2(w, 0f), orig, col);
				}
				if (at.Y < orig.Y)
				{
					tex.Draw(at + new Vector2(0f, h), orig, col);
				}
				else if (at.Y > (float)h - orig.Y)
				{
					tex.Draw(at - new Vector2(0f, h), orig, col);
				}
			}
			Draw.SpriteBatch.End();
			int vertex = 0;
			for (int i = 0; i < rays.Length; i++)
			{
				Vector2 at2 = default(Vector2);
				at2.X = Mod(rays[i].Position.X - camera.X * 0.9f, w);
				at2.Y = Mod(rays[i].Position.Y - camera.Y * 0.9f, h);
				DrawRay(at2, ref vertex, ref rays[i]);
				if (at2.X < 64f)
				{
					DrawRay(at2 + new Vector2(w, 0f), ref vertex, ref rays[i]);
				}
				else if (at2.X > (float)(w - 64))
				{
					DrawRay(at2 - new Vector2(w, 0f), ref vertex, ref rays[i]);
				}
				if (at2.Y < 64f)
				{
					DrawRay(at2 + new Vector2(0f, h), ref vertex, ref rays[i]);
				}
				else if (at2.Y > (float)(h - 64))
				{
					DrawRay(at2 - new Vector2(0f, h), ref vertex, ref rays[i]);
				}
			}
			if (beamsTarget == null)
			{
				beamsTarget = VirtualContent.CreateRenderTarget("glass-block-beams", 320, 180);
			}
			Engine.Graphics.GraphicsDevice.SetRenderTarget(beamsTarget);
			Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
			GFX.DrawVertices(Matrix.Identity, verts, vertex);
		}

		private void OnDisplacementRender()
		{
			foreach (Entity block in base.Scene.Tracker.GetEntities<GlassBlock>())
			{
				Draw.Rect(block.X, block.Y, block.Width, block.Height, new Color(0.5f, 0.5f, 0.2f, 1f));
			}
		}

		private void DrawRay(Vector2 position, ref int vertex, ref Ray ray)
		{
			Vector2 vector = new Vector2(0f - rayNormal.Y, rayNormal.X);
			Vector2 w = rayNormal * ray.Width * 0.5f;
			Vector2 i = vector * ray.Length * 0.25f * 0.5f;
			Vector2 l2 = vector * ray.Length * 0.5f * 0.5f;
			Vector2 a0 = position + w - i - l2;
			Vector2 a1 = position - w - i - l2;
			Vector2 b0 = position + w - i;
			Vector2 b1 = position - w - i;
			Vector2 c0 = position + w + i;
			Vector2 c1 = position - w + i;
			Vector2 d0 = position + w + i + l2;
			Vector2 d1 = position - w + i + l2;
			Color ct = Color.Transparent;
			Color cr = ray.Color;
			Quad(ref vertex, a0, b0, b1, a1, ct, cr, cr, ct);
			Quad(ref vertex, b0, c0, c1, b1, cr, cr, cr, cr);
			Quad(ref vertex, c0, d0, d1, c1, cr, ct, ct, cr);
		}

		private void Quad(ref int vertex, Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3, Color c0, Color c1, Color c2, Color c3)
		{
			verts[vertex].Position.X = v0.X;
			verts[vertex].Position.Y = v0.Y;
			verts[vertex++].Color = c0;
			verts[vertex].Position.X = v1.X;
			verts[vertex].Position.Y = v1.Y;
			verts[vertex++].Color = c1;
			verts[vertex].Position.X = v2.X;
			verts[vertex].Position.Y = v2.Y;
			verts[vertex++].Color = c2;
			verts[vertex].Position.X = v0.X;
			verts[vertex].Position.Y = v0.Y;
			verts[vertex++].Color = c0;
			verts[vertex].Position.X = v2.X;
			verts[vertex].Position.Y = v2.Y;
			verts[vertex++].Color = c2;
			verts[vertex].Position.X = v3.X;
			verts[vertex].Position.Y = v3.Y;
			verts[vertex++].Color = c3;
		}

		public override void Render()
		{
			if (!hasBlocks)
			{
				return;
			}
			Vector2 camera = (base.Scene as Level).Camera.Position;
			List<Entity> blocks = base.Scene.Tracker.GetEntities<GlassBlock>();
			foreach (Entity block3 in blocks)
			{
				Draw.Rect(block3.X, block3.Y, block3.Width, block3.Height, bgColor);
			}
			if (starsTarget != null && !starsTarget.IsDisposed)
			{
				foreach (Entity block2 in blocks)
				{
					Rectangle clip2 = new Rectangle((int)(block2.X - camera.X), (int)(block2.Y - camera.Y), (int)block2.Width, (int)block2.Height);
					Draw.SpriteBatch.Draw((RenderTarget2D)starsTarget, block2.Position, clip2, Color.White);
				}
			}
			if (beamsTarget == null || beamsTarget.IsDisposed)
			{
				return;
			}
			foreach (Entity block in blocks)
			{
				Rectangle clip = new Rectangle((int)(block.X - camera.X), (int)(block.Y - camera.Y), (int)block.Width, (int)block.Height);
				Draw.SpriteBatch.Draw((RenderTarget2D)beamsTarget, block.Position, clip, Color.White);
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

		public void Dispose()
		{
			if (starsTarget != null && !starsTarget.IsDisposed)
			{
				starsTarget.Dispose();
			}
			if (beamsTarget != null && !beamsTarget.IsDisposed)
			{
				beamsTarget.Dispose();
			}
			starsTarget = null;
			beamsTarget = null;
		}

		private float Mod(float x, float m)
		{
			return (x % m + m) % m;
		}
	}
}
