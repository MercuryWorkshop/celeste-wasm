using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class NorthernLights : Backdrop
	{
		private class Strand
		{
			public List<Node> Nodes = new List<Node>();

			public float Duration;

			public float Percent;

			public float Alpha;

			public Strand()
			{
				Reset(Calc.Random.NextFloat());
			}

			public void Reset(float startPercent)
			{
				Percent = startPercent;
				Duration = Calc.Random.Range(12f, 32f);
				Alpha = 0f;
				Nodes.Clear();
				Vector2 current = new Vector2(Calc.Random.Range(-40, 60), Calc.Random.Range(40, 90));
				float texOffset = Calc.Random.NextFloat();
				Color color = Calc.Random.Choose(colors);
				for (int i = 0; i < 40; i++)
				{
					Node node = new Node
					{
						Position = current,
						TextureOffset = texOffset,
						Height = Calc.Random.Range(10, 80),
						TopAlpha = Calc.Random.Range(0.3f, 0.8f),
						BottomAlpha = Calc.Random.Range(0.5f, 1f),
						SineOffset = Calc.Random.NextFloat() * ((float)Math.PI * 2f),
						Color = Color.Lerp(color, Calc.Random.Choose(colors), Calc.Random.Range(0f, 0.3f))
					};
					texOffset += Calc.Random.Range(0.02f, 0.2f);
					current += new Vector2(Calc.Random.Range(4, 20), Calc.Random.Range(-15, 15));
					Nodes.Add(node);
				}
			}
		}

		private class Node
		{
			public Vector2 Position;

			public float TextureOffset;

			public float Height;

			public float TopAlpha;

			public float BottomAlpha;

			public float SineOffset;

			public Color Color;
		}

		private struct Particle
		{
			public Vector2 Position;

			public float Speed;

			public Color Color;
		}

		private static readonly Color[] colors = new Color[4]
		{
			Calc.HexToColor("2de079"),
			Calc.HexToColor("62f4f6"),
			Calc.HexToColor("45bc2e"),
			Calc.HexToColor("3856f0")
		};

		private List<Strand> strands = new List<Strand>();

		private Particle[] particles = new Particle[50];

		private VertexPositionColorTexture[] verts = new VertexPositionColorTexture[1024];

		private VertexPositionColor[] gradient = new VertexPositionColor[6];

		private VirtualRenderTarget buffer;

		private float timer;

		public float OffsetY;

		public float NorthernLightsAlpha = 1f;

		public NorthernLights()
		{
			for (int j = 0; j < 3; j++)
			{
				strands.Add(new Strand());
			}
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Position = new Vector2(Calc.Random.Range(0, 320), Calc.Random.Range(0, 180));
				particles[i].Speed = Calc.Random.Range(4, 14);
				particles[i].Color = Calc.Random.Choose(colors);
			}
			Color topColor = Calc.HexToColor("020825");
			Color botColor = Calc.HexToColor("170c2f");
			gradient[0] = new VertexPositionColor(new Vector3(0f, 0f, 0f), topColor);
			gradient[1] = new VertexPositionColor(new Vector3(320f, 0f, 0f), topColor);
			gradient[2] = new VertexPositionColor(new Vector3(320f, 180f, 0f), botColor);
			gradient[3] = new VertexPositionColor(new Vector3(0f, 0f, 0f), topColor);
			gradient[4] = new VertexPositionColor(new Vector3(320f, 180f, 0f), botColor);
			gradient[5] = new VertexPositionColor(new Vector3(0f, 180f, 0f), botColor);
		}

		public override void Update(Scene scene)
		{
			if (Visible)
			{
				timer += Engine.DeltaTime;
				foreach (Strand strand in strands)
				{
					strand.Percent += Engine.DeltaTime / strand.Duration;
					strand.Alpha = Calc.Approach(strand.Alpha, (strand.Percent < 1f) ? 1 : 0, Engine.DeltaTime);
					if (strand.Alpha <= 0f && strand.Percent >= 1f)
					{
						strand.Reset(0f);
					}
					foreach (Node node in strand.Nodes)
					{
						node.SineOffset += Engine.DeltaTime;
					}
				}
				for (int i = 0; i < particles.Length; i++)
				{
					particles[i].Position.Y += particles[i].Speed * Engine.DeltaTime;
				}
			}
			base.Update(scene);
		}

		public override void BeforeRender(Scene scene)
		{
			if (buffer == null)
			{
				buffer = VirtualContent.CreateRenderTarget("northern-lights", 320, 180);
			}
			int v = 0;
			foreach (Strand strand in strands)
			{
				Node last = strand.Nodes[0];
				for (int j = 1; j < strand.Nodes.Count; j++)
				{
					Node next = strand.Nodes[j];
					float lastAlpha = Math.Min(1f, (float)j / 4f) * NorthernLightsAlpha;
					float nextAlpha = Math.Min(1f, (float)(strand.Nodes.Count - j) / 4f) * NorthernLightsAlpha;
					float lastSinY = OffsetY + (float)Math.Sin(last.SineOffset) * 3f;
					float nextSinY = OffsetY + (float)Math.Sin(next.SineOffset) * 3f;
					Set(ref v, last.Position.X, last.Position.Y + lastSinY, last.TextureOffset, 1f, last.Color * (last.BottomAlpha * strand.Alpha * lastAlpha));
					Set(ref v, last.Position.X, last.Position.Y - last.Height + lastSinY, last.TextureOffset, 0.05f, last.Color * (last.TopAlpha * strand.Alpha * lastAlpha));
					Set(ref v, next.Position.X, next.Position.Y - next.Height + nextSinY, next.TextureOffset, 0.05f, next.Color * (next.TopAlpha * strand.Alpha * nextAlpha));
					Set(ref v, last.Position.X, last.Position.Y + lastSinY, last.TextureOffset, 1f, last.Color * (last.BottomAlpha * strand.Alpha * lastAlpha));
					Set(ref v, next.Position.X, next.Position.Y - next.Height + nextSinY, next.TextureOffset, 0.05f, next.Color * (next.TopAlpha * strand.Alpha * nextAlpha));
					Set(ref v, next.Position.X, next.Position.Y + nextSinY, next.TextureOffset, 1f, next.Color * (next.BottomAlpha * strand.Alpha * nextAlpha));
					last = next;
				}
			}
			Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
			GFX.DrawVertices(Matrix.Identity, gradient, gradient.Length);
			Engine.Graphics.GraphicsDevice.Textures[0] = GFX.Misc["northernlights"].Texture.Texture;
			Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
			GFX.DrawVertices(Matrix.Identity, verts, v, GFX.FxTexture);
			bool clear = false;
			GaussianBlur.Blur((RenderTarget2D)buffer, GameplayBuffers.TempA, buffer, 0f, clear, GaussianBlur.Samples.Five, 0.25f, GaussianBlur.Direction.Vertical);
			Draw.SpriteBatch.Begin();
			Camera camera = (scene as Level).Camera;
			for (int i = 0; i < particles.Length; i++)
			{
				Vector2 at = default(Vector2);
				at.X = mod(particles[i].Position.X - camera.X * 0.2f, 320f);
				at.Y = mod(particles[i].Position.Y - camera.Y * 0.2f, 180f);
				Draw.Rect(at, 1f, 1f, particles[i].Color);
			}
			Draw.SpriteBatch.End();
		}

		public override void Ended(Scene scene)
		{
			if (buffer != null)
			{
				buffer.Dispose();
			}
			buffer = null;
			base.Ended(scene);
		}

		private void Set(ref int vert, float px, float py, float tx, float ty, Color color)
		{
			verts[vert].Color = color;
			verts[vert].Position.X = px;
			verts[vert].Position.Y = py;
			verts[vert].TextureCoordinate.X = tx;
			verts[vert].TextureCoordinate.Y = ty;
			vert++;
		}

		public override void Render(Scene scene)
		{
			Draw.SpriteBatch.Draw((RenderTarget2D)buffer, Vector2.Zero, Color.White);
		}

		private float mod(float x, float m)
		{
			return (x % m + m) % m;
		}
	}
}
