using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class LightningRenderer : Entity
	{
		private class Bolt
		{
			private List<Vector2> nodes = new List<Vector2>();

			private Coroutine routine;

			private bool visible;

			private float size;

			private float gap;

			private float alpha;

			private uint seed;

			private float flash;

			private readonly Color color;

			private readonly float scale;

			private readonly int width;

			private readonly int height;

			public Bolt(Color color, float scale, int width, int height)
			{
				this.color = color;
				this.width = width;
				this.height = height;
				this.scale = scale;
				routine = new Coroutine(Run());
			}

			public void Update(Scene scene)
			{
				routine.Update();
				flash = Calc.Approach(flash, 0f, Engine.DeltaTime * 2f);
			}

			private IEnumerator Run()
			{
				yield return Calc.Random.Range(0f, 4f);
				while (true)
				{
					List<Vector2> slots = new List<Vector2>();
					for (int l = 0; l < 3; l++)
					{
						Vector2 p = Calc.Random.Choose(new Vector2(0f, Calc.Random.Range(8, height - 16)), new Vector2(Calc.Random.Range(8, width - 16), 0f), new Vector2(width, Calc.Random.Range(8, height - 16)), new Vector2(Calc.Random.Range(8, width - 16), height));
						Vector2 op = ((p.X <= 0f || p.X >= (float)width) ? new Vector2((float)width - p.X, p.Y) : new Vector2(p.X, (float)height - p.Y));
						slots.Add(p);
						slots.Add(op);
					}
					List<Vector2> centers = new List<Vector2>();
					for (int k = 0; k < 3; k++)
					{
						centers.Add(new Vector2(Calc.Random.Range(0.25f, 0.75f) * (float)width, Calc.Random.Range(0.25f, 0.75f) * (float)height));
					}
					nodes.Clear();
					foreach (Vector2 slot in slots)
					{
						nodes.Add(slot);
						nodes.Add(centers.ClosestTo(slot));
					}
					Vector2 last = centers[centers.Count - 1];
					foreach (Vector2 next in centers)
					{
						nodes.Add(last);
						nodes.Add(next);
						last = next;
					}
					flash = 1f;
					visible = true;
					size = 5f;
					gap = 0f;
					alpha = 1f;
					for (int j = 0; j < 4; j++)
					{
						seed = (uint)Calc.Random.Next();
						yield return 0.1f;
					}
					for (int j = 0; j < 5; j++)
					{
						if (!Settings.Instance.DisableFlashes)
						{
							visible = false;
						}
						yield return 0.05f + (float)j * 0.02f;
						float ease = (float)j / 5f;
						visible = true;
						size = (1f - ease) * 5f;
						gap = ease;
						alpha = 1f - ease;
						visible = true;
						seed = (uint)Calc.Random.Next();
						yield return 0.025f;
					}
					visible = false;
					yield return Calc.Random.Range(4f, 8f);
				}
			}

			public void Render()
			{
				if (flash > 0f && !Settings.Instance.DisableFlashes)
				{
					Draw.Rect(0f, 0f, width, height, Color.White * flash * 0.15f * scale);
				}
				if (visible)
				{
					for (int i = 0; i < nodes.Count; i += 2)
					{
						DrawFatLightning(seed, nodes[i], nodes[i + 1], size * scale, gap, color * alpha);
					}
				}
			}
		}

		private class Edge
		{
			public Lightning Parent;

			public bool Visible;

			public Vector2 A;

			public Vector2 B;

			public Vector2 Min;

			public Vector2 Max;

			public Edge(Lightning parent, Vector2 a, Vector2 b)
			{
				Parent = parent;
				Visible = true;
				A = a;
				B = b;
				Min = new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
				Max = new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
			}

			public bool InView(ref Rectangle view)
			{
				if ((float)view.Left < Parent.X + Max.X && (float)view.Right > Parent.X + Min.X && (float)view.Top < Parent.Y + Max.Y)
				{
					return (float)view.Bottom > Parent.Y + Min.Y;
				}
				return false;
			}
		}

		private List<Lightning> list = new List<Lightning>();

		private List<Edge> edges = new List<Edge>();

		private List<Bolt> bolts = new List<Bolt>();

		private VertexPositionColor[] edgeVerts;

		private VirtualMap<bool> tiles;

		private Rectangle levelTileBounds;

		private uint edgeSeed;

		private uint leapSeed;

		private bool dirty;

		private Color[] electricityColors = new Color[2]
		{
			Calc.HexToColor("fcf579"),
			Calc.HexToColor("8cf7e2")
		};

		private Color[] electricityColorsLerped;

		public float Fade;

		public bool UpdateSeeds = true;

		public const int BoltBufferSize = 160;

		public bool DrawEdges = true;

		public SoundSource AmbientSfx;

		public LightningRenderer()
		{
			base.Tag = (int)Tags.Global | (int)Tags.TransitionUpdate;
			base.Depth = -1000100;
			electricityColorsLerped = new Color[electricityColors.Length];
			Add(new CustomBloom(OnRenderBloom));
			Add(new BeforeRenderHook(OnBeforeRender));
			Add(AmbientSfx = new SoundSource());
			AmbientSfx.DisposeOnTransition = false;
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			for (int i = 0; i < 4; i++)
			{
				bolts.Add(new Bolt(electricityColors[0], 1f, 160, 160));
				bolts.Add(new Bolt(electricityColors[1], 0.35f, 160, 160));
			}
		}

		public void StartAmbience()
		{
			if (!AmbientSfx.Playing)
			{
				AmbientSfx.Play("event:/new_content/env/10_electricity");
			}
		}

		public void StopAmbience()
		{
			AmbientSfx.Stop();
		}

		public void Reset()
		{
			UpdateSeeds = true;
			Fade = 0f;
		}

		public void Track(Lightning block)
		{
			list.Add(block);
			if (tiles == null)
			{
				levelTileBounds = (base.Scene as Level).TileBounds;
				tiles = new VirtualMap<bool>(levelTileBounds.Width, levelTileBounds.Height, emptyValue: false);
			}
			for (int x = (int)block.X / 8; x < ((int)block.X + block.VisualWidth) / 8; x++)
			{
				for (int y = (int)block.Y / 8; y < ((int)block.Y + block.VisualHeight) / 8; y++)
				{
					tiles[x - levelTileBounds.X, y - levelTileBounds.Y] = true;
				}
			}
			dirty = true;
		}

		public void Untrack(Lightning block)
		{
			list.Remove(block);
			if (list.Count <= 0)
			{
				tiles = null;
			}
			else
			{
				for (int x = (int)block.X / 8; (float)x < block.Right / 8f; x++)
				{
					for (int y = (int)block.Y / 8; (float)y < block.Bottom / 8f; y++)
					{
						tiles[x - levelTileBounds.X, y - levelTileBounds.Y] = false;
					}
				}
			}
			dirty = true;
		}

		public override void Update()
		{
			if (dirty)
			{
				RebuildEdges();
			}
			ToggleEdges();
			if (list.Count <= 0)
			{
				return;
			}
			foreach (Bolt bolt in bolts)
			{
				bolt.Update(base.Scene);
			}
			if (UpdateSeeds)
			{
				if (base.Scene.OnInterval(0.1f))
				{
					edgeSeed = (uint)Calc.Random.Next();
				}
				if (base.Scene.OnInterval(0.7f))
				{
					leapSeed = (uint)Calc.Random.Next();
				}
			}
		}

		public void ToggleEdges(bool immediate = false)
		{
			Camera cam = (base.Scene as Level).Camera;
			Rectangle view = new Rectangle((int)cam.Left - 4, (int)cam.Top - 4, (int)(cam.Right - cam.Left) + 8, (int)(cam.Bottom - cam.Top) + 8);
			for (int i = 0; i < edges.Count; i++)
			{
				if (immediate)
				{
					edges[i].Visible = edges[i].InView(ref view);
				}
				else if (!edges[i].Visible && base.Scene.OnInterval(0.05f, (float)i * 0.01f) && edges[i].InView(ref view))
				{
					edges[i].Visible = true;
				}
				else if (edges[i].Visible && base.Scene.OnInterval(0.25f, (float)i * 0.01f) && !edges[i].InView(ref view))
				{
					edges[i].Visible = false;
				}
			}
		}

		private void RebuildEdges()
		{
			dirty = false;
			edges.Clear();
			if (list.Count <= 0)
			{
				return;
			}
			Level obj = base.Scene as Level;
			_ = obj.TileBounds.Left;
			_ = obj.TileBounds.Top;
			_ = obj.TileBounds.Right;
			_ = obj.TileBounds.Bottom;
			Point[] normals = new Point[4]
			{
				new Point(0, -1),
				new Point(0, 1),
				new Point(-1, 0),
				new Point(1, 0)
			};
			foreach (Lightning lightning in list)
			{
				for (int x = (int)lightning.X / 8; (float)x < lightning.Right / 8f; x++)
				{
					for (int y = (int)lightning.Y / 8; (float)y < lightning.Bottom / 8f; y++)
					{
						Point[] array = normals;
						for (int i = 0; i < array.Length; i++)
						{
							Point norm = array[i];
							Point along = new Point(-norm.Y, norm.X);
							if (Inside(x + norm.X, y + norm.Y) || (Inside(x - along.X, y - along.Y) && !Inside(x + norm.X - along.X, y + norm.Y - along.Y)))
							{
								continue;
							}
							Point from = new Point(x, y);
							Point to = new Point(x + along.X, y + along.Y);
							Vector2 offset = new Vector2(4f) + new Vector2(norm.X - along.X, norm.Y - along.Y) * 4f;
							int len = 1;
							while (Inside(to.X, to.Y) && !Inside(to.X + norm.X, to.Y + norm.Y))
							{
								to.X += along.X;
								to.Y += along.Y;
								len++;
								if (len > 8)
								{
									Vector2 a = new Vector2(from.X, from.Y) * 8f + offset - lightning.Position;
									Vector2 b = new Vector2(to.X, to.Y) * 8f + offset - lightning.Position;
									edges.Add(new Edge(lightning, a, b));
									len = 0;
									from = to;
								}
							}
							if (len > 0)
							{
								Vector2 a = new Vector2(from.X, from.Y) * 8f + offset - lightning.Position;
								Vector2 b = new Vector2(to.X, to.Y) * 8f + offset - lightning.Position;
								edges.Add(new Edge(lightning, a, b));
							}
						}
					}
				}
			}
			if (edgeVerts == null)
			{
				edgeVerts = new VertexPositionColor[1024];
			}
		}

		private bool Inside(int tx, int ty)
		{
			return tiles[tx - levelTileBounds.X, ty - levelTileBounds.Y];
		}

		private void OnRenderBloom()
		{
			Camera cam = (base.Scene as Level).Camera;
			new Rectangle((int)cam.Left, (int)cam.Top, (int)(cam.Right - cam.Left), (int)(cam.Bottom - cam.Top));
			Color col = Color.White * (0.25f + Fade * 0.75f);
			foreach (Edge edge in edges)
			{
				if (edge.Visible)
				{
					Draw.Line(edge.Parent.Position + edge.A, edge.Parent.Position + edge.B, col, 4f);
				}
			}
			foreach (Lightning i in list)
			{
				if (i.Visible)
				{
					Draw.Rect(i.X, i.Y, i.VisualWidth, i.VisualHeight, col);
				}
			}
			if (Fade > 0f)
			{
				Level level = base.Scene as Level;
				Draw.Rect(level.Camera.X, level.Camera.Y, 320f, 180f, Color.White * Fade);
			}
		}

		private void OnBeforeRender()
		{
			if (list.Count <= 0)
			{
				return;
			}
			Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Lightning);
			Engine.Graphics.GraphicsDevice.Clear(Color.Lerp(Calc.HexToColor("f7b262") * 0.1f, Color.White, Fade));
			Draw.SpriteBatch.Begin();
			foreach (Bolt bolt in bolts)
			{
				bolt.Render();
			}
			Draw.SpriteBatch.End();
		}

		public override void Render()
		{
			if (list.Count <= 0)
			{
				return;
			}
			Camera cam = (base.Scene as Level).Camera;
			new Rectangle((int)cam.Left, (int)cam.Top, (int)(cam.Right - cam.Left), (int)(cam.Bottom - cam.Top));
			foreach (Lightning j in list)
			{
				if (j.Visible)
				{
					Draw.SpriteBatch.Draw((RenderTarget2D)GameplayBuffers.Lightning, j.Position, new Rectangle((int)j.X, (int)j.Y, j.VisualWidth, j.VisualHeight), Color.White);
				}
			}
			if (edges.Count <= 0 || !DrawEdges)
			{
				return;
			}
			for (int i = 0; i < electricityColorsLerped.Length; i++)
			{
				electricityColorsLerped[i] = Color.Lerp(electricityColors[i], Color.White, Fade);
			}
			int edgeVertCount = 0;
			uint leapBaseSeed = leapSeed;
			foreach (Edge edge in edges)
			{
				if (edge.Visible)
				{
					DrawSimpleLightning(ref edgeVertCount, ref edgeVerts, edgeSeed, edge.Parent.Position, edge.A, edge.B, electricityColorsLerped[0], 1f + Fade * 3f);
					DrawSimpleLightning(ref edgeVertCount, ref edgeVerts, edgeSeed + 1, edge.Parent.Position, edge.A, edge.B, electricityColorsLerped[1], 1f + Fade * 3f);
					if (PseudoRand(ref leapBaseSeed) % 30u == 0)
					{
						DrawBezierLightning(ref edgeVertCount, ref edgeVerts, edgeSeed, edge.Parent.Position, edge.A, edge.B, 24f, 10, electricityColorsLerped[1]);
					}
				}
			}
			if (edgeVertCount > 0)
			{
				GameplayRenderer.End();
				GFX.DrawVertices(cam.Matrix, edgeVerts, edgeVertCount);
				GameplayRenderer.Begin();
			}
		}

		private static void DrawSimpleLightning(ref int index, ref VertexPositionColor[] verts, uint seed, Vector2 pos, Vector2 a, Vector2 b, Color color, float thickness = 1f)
		{
			seed += (uint)(a.GetHashCode() + b.GetHashCode());
			a += pos;
			b += pos;
			float dist = (b - a).Length();
			Vector2 norm = (b - a) / dist;
			Vector2 perp = norm.TurnRight();
			a += perp;
			b += perp;
			Vector2 prev = a;
			int sign = ((PseudoRand(ref seed) % 2u != 0) ? 1 : (-1));
			float offset = PseudoRandRange(ref seed, 0f, (float)Math.PI * 2f);
			float moved = 0f;
			float maxVertsUsed = (float)index + ((b - a).Length() / 4f + 1f) * 6f;
			while (maxVertsUsed >= (float)verts.Length)
			{
				Array.Resize(ref verts, verts.Length * 2);
			}
			for (int i = index; (float)i < maxVertsUsed; i++)
			{
				verts[i].Color = color;
			}
			do
			{
				float rand = PseudoRandRange(ref seed, 0f, 4f);
				offset += 0.1f;
				moved += 4f + rand;
				Vector2 next = a + norm * moved;
				if (moved < dist)
				{
					next += sign * perp * rand - perp;
				}
				else
				{
					next = b;
				}
				verts[index++].Position = new Vector3(prev - perp * thickness, 0f);
				verts[index++].Position = new Vector3(next - perp * thickness, 0f);
				verts[index++].Position = new Vector3(next + perp * thickness, 0f);
				verts[index++].Position = new Vector3(prev - perp * thickness, 0f);
				verts[index++].Position = new Vector3(next + perp * thickness, 0f);
				verts[index++].Position = new Vector3(prev, 0f);
				prev = next;
				sign = -sign;
			}
			while (moved < dist);
		}

		private static void DrawBezierLightning(ref int index, ref VertexPositionColor[] verts, uint seed, Vector2 pos, Vector2 a, Vector2 b, float anchor, int steps, Color color)
		{
			seed += (uint)(a.GetHashCode() + b.GetHashCode());
			a += pos;
			b += pos;
			Vector2 perp = (b - a).SafeNormalize().TurnRight();
			SimpleCurve bezier = new SimpleCurve(a, b, (b + a) / 2f + perp * anchor);
			int maxVertsUsed = index + (steps + 2) * 6;
			while (maxVertsUsed >= verts.Length)
			{
				Array.Resize(ref verts, verts.Length * 2);
			}
			Vector2 prev = bezier.GetPoint(0f);
			for (int i = 0; i <= steps; i++)
			{
				Vector2 next = bezier.GetPoint((float)i / (float)steps);
				if (i != steps)
				{
					next += new Vector2(PseudoRandRange(ref seed, -2f, 2f), PseudoRandRange(ref seed, -2f, 2f));
				}
				verts[index].Position = new Vector3(prev - perp, 0f);
				verts[index++].Color = color;
				verts[index].Position = new Vector3(next - perp, 0f);
				verts[index++].Color = color;
				verts[index].Position = new Vector3(next, 0f);
				verts[index++].Color = color;
				verts[index].Position = new Vector3(prev - perp, 0f);
				verts[index++].Color = color;
				verts[index].Position = new Vector3(next, 0f);
				verts[index++].Color = color;
				verts[index].Position = new Vector3(prev, 0f);
				verts[index++].Color = color;
				prev = next;
			}
		}

		private static void DrawFatLightning(uint seed, Vector2 a, Vector2 b, float size, float gap, Color color)
		{
			seed += (uint)(a.GetHashCode() + b.GetHashCode());
			float dist = (b - a).Length();
			Vector2 norm = (b - a) / dist;
			Vector2 perp = norm.TurnRight();
			Vector2 prev = a;
			int sign = 1;
			PseudoRandRange(ref seed, 0f, (float)Math.PI * 2f);
			float moved = 0f;
			do
			{
				moved += PseudoRandRange(ref seed, 10f, 14f);
				Vector2 next = a + norm * moved;
				if (moved < dist)
				{
					next += sign * perp * PseudoRandRange(ref seed, 0f, 6f);
				}
				else
				{
					next = b;
				}
				Vector2 drawNext = next;
				if (gap > 0f)
				{
					drawNext = prev + (next - prev) * (1f - gap);
					Draw.Line(prev, next + norm, color, size * 0.5f);
				}
				Draw.Line(prev, drawNext + norm, color, size);
				prev = next;
				sign = -sign;
			}
			while (moved < dist);
		}

		private static uint PseudoRand(ref uint seed)
		{
			seed ^= seed << 13;
			seed ^= seed >> 17;
			return seed;
		}

		public static float PseudoRandRange(ref uint seed, float min, float max)
		{
			return min + (float)(PseudoRand(ref seed) & 0x3FFu) / 1024f * (max - min);
		}
	}
}
