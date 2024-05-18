using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class SeekerBarrierRenderer : Entity
	{
		private class Edge
		{
			public SeekerBarrier Parent;

			public bool Visible;

			public Vector2 A;

			public Vector2 B;

			public Vector2 Min;

			public Vector2 Max;

			public Vector2 Normal;

			public Vector2 Perpendicular;

			public float[] Wave;

			public float Length;

			public Edge(SeekerBarrier parent, Vector2 a, Vector2 b)
			{
				Parent = parent;
				Visible = true;
				A = a;
				B = b;
				Min = new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
				Max = new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
				Normal = (b - a).SafeNormalize();
				Perpendicular = -Normal.Perpendicular();
				Length = (a - b).Length();
			}

			public void UpdateWave(float time)
			{
				if (Wave == null || (float)Wave.Length <= Length)
				{
					Wave = new float[(int)Length + 2];
				}
				for (int i = 0; (float)i <= Length; i++)
				{
					Wave[i] = GetWaveAt(time, i, Length);
				}
			}

			private float GetWaveAt(float offset, float along, float length)
			{
				if (along <= 1f || along >= length - 1f)
				{
					return 0f;
				}
				if (Parent.Solidify >= 1f)
				{
					return 0f;
				}
				float t = offset + along * 0.25f;
				float sin = (float)(Math.Sin(t) * 2.0 + Math.Sin(t * 0.25f));
				return (1f + sin * Ease.SineInOut(Calc.YoYo(along / length))) * (1f - Parent.Solidify);
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

		private List<SeekerBarrier> list = new List<SeekerBarrier>();

		private List<Edge> edges = new List<Edge>();

		private VirtualMap<bool> tiles;

		private Rectangle levelTileBounds;

		private bool dirty;

		public SeekerBarrierRenderer()
		{
			base.Tag = (int)Tags.Global | (int)Tags.TransitionUpdate;
			base.Depth = 0;
			Add(new CustomBloom(OnRenderBloom));
		}

		public void Track(SeekerBarrier block)
		{
			list.Add(block);
			if (tiles == null)
			{
				levelTileBounds = (base.Scene as Level).TileBounds;
				tiles = new VirtualMap<bool>(levelTileBounds.Width, levelTileBounds.Height, emptyValue: false);
			}
			for (int x = (int)block.X / 8; (float)x < block.Right / 8f; x++)
			{
				for (int y = (int)block.Y / 8; (float)y < block.Bottom / 8f; y++)
				{
					tiles[x - levelTileBounds.X, y - levelTileBounds.Y] = true;
				}
			}
			dirty = true;
		}

		public void Untrack(SeekerBarrier block)
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
			UpdateEdges();
		}

		public void UpdateEdges()
		{
			Camera cam = (base.Scene as Level).Camera;
			Rectangle view = new Rectangle((int)cam.Left - 4, (int)cam.Top - 4, (int)(cam.Right - cam.Left) + 8, (int)(cam.Bottom - cam.Top) + 8);
			for (int i = 0; i < edges.Count; i++)
			{
				if (edges[i].Visible)
				{
					if (base.Scene.OnInterval(0.25f, (float)i * 0.01f) && !edges[i].InView(ref view))
					{
						edges[i].Visible = false;
					}
				}
				else if (base.Scene.OnInterval(0.05f, (float)i * 0.01f) && edges[i].InView(ref view))
				{
					edges[i].Visible = true;
				}
				if (edges[i].Visible && (base.Scene.OnInterval(0.05f, (float)i * 0.01f) || edges[i].Wave == null))
				{
					edges[i].UpdateWave(base.Scene.TimeActive * 3f);
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
			foreach (SeekerBarrier barrier in list)
			{
				for (int x = (int)barrier.X / 8; (float)x < barrier.Right / 8f; x++)
				{
					for (int y = (int)barrier.Y / 8; (float)y < barrier.Bottom / 8f; y++)
					{
						Point[] array = normals;
						for (int i = 0; i < array.Length; i++)
						{
							Point norm = array[i];
							Point along = new Point(-norm.Y, norm.X);
							if (!Inside(x + norm.X, y + norm.Y) && (!Inside(x - along.X, y - along.Y) || Inside(x + norm.X - along.X, y + norm.Y - along.Y)))
							{
								Point from = new Point(x, y);
								Point to = new Point(x + along.X, y + along.Y);
								Vector2 offset = new Vector2(4f) + new Vector2(norm.X - along.X, norm.Y - along.Y) * 4f;
								while (Inside(to.X, to.Y) && !Inside(to.X + norm.X, to.Y + norm.Y))
								{
									to.X += along.X;
									to.Y += along.Y;
								}
								Vector2 a = new Vector2(from.X, from.Y) * 8f + offset - barrier.Position;
								Vector2 b = new Vector2(to.X, to.Y) * 8f + offset - barrier.Position;
								edges.Add(new Edge(barrier, a, b));
							}
						}
					}
				}
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
			foreach (SeekerBarrier block in list)
			{
				if (block.Visible)
				{
					Draw.Rect(block.X, block.Y, block.Width, block.Height, Color.White);
				}
			}
			foreach (Edge edge in edges)
			{
				if (edge.Visible)
				{
					Vector2 from = edge.Parent.Position + edge.A;
					_ = edge.Parent.Position + edge.B;
					for (int i = 0; (float)i <= edge.Length; i++)
					{
						Vector2 vector = from + edge.Normal * i;
						Draw.Line(vector, vector + edge.Perpendicular * edge.Wave[i], Color.White);
					}
				}
			}
		}

		public override void Render()
		{
			if (list.Count <= 0)
			{
				return;
			}
			Color fillColor = Color.White * 0.15f;
			Color edgeColor = Color.White * 0.25f;
			foreach (SeekerBarrier block in list)
			{
				if (block.Visible)
				{
					Draw.Rect(block.Collider, fillColor);
				}
			}
			if (edges.Count <= 0)
			{
				return;
			}
			foreach (Edge edge in edges)
			{
				if (edge.Visible)
				{
					Vector2 from = edge.Parent.Position + edge.A;
					_ = edge.Parent.Position + edge.B;
					Color.Lerp(edgeColor, Color.White, edge.Parent.Flash);
					for (int i = 0; (float)i <= edge.Length; i++)
					{
						Vector2 vector = from + edge.Normal * i;
						Draw.Line(vector, vector + edge.Perpendicular * edge.Wave[i], fillColor);
					}
				}
			}
		}
	}
}
