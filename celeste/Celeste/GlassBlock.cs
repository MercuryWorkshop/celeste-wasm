using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class GlassBlock : Solid
	{
		private struct Line
		{
			public Vector2 A;

			public Vector2 B;

			public Line(Vector2 a, Vector2 b)
			{
				A = a;
				B = b;
			}
		}

		private bool sinks;

		private float startY;

		private List<Line> lines = new List<Line>();

		private Color lineColor = Color.White;

		public GlassBlock(Vector2 position, float width, float height, bool sinks)
			: base(position, width, height, safe: false)
		{
			this.sinks = sinks;
			startY = base.Y;
			base.Depth = -10000;
			Add(new LightOcclude());
			Add(new MirrorSurface());
			SurfaceSoundIndex = 32;
		}

		public GlassBlock(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.Bool("sinks"))
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			int columns = (int)base.Width / 8;
			int rows = (int)base.Height / 8;
			AddSide(new Vector2(0f, 0f), new Vector2(0f, -1f), columns);
			AddSide(new Vector2(columns - 1, 0f), new Vector2(1f, 0f), rows);
			AddSide(new Vector2(columns - 1, rows - 1), new Vector2(0f, 1f), columns);
			AddSide(new Vector2(0f, rows - 1), new Vector2(-1f, 0f), rows);
		}

		private float Mod(float x, float m)
		{
			return (x % m + m) % m;
		}

		private void AddSide(Vector2 start, Vector2 normal, int tiles)
		{
			Vector2 right = new Vector2(0f - normal.Y, normal.X);
			for (int t = 0; t < tiles; t++)
			{
				if (Open(start + right * t + normal))
				{
					Vector2 a = (start + right * t) * 8f + new Vector2(4f) - right * 4f + normal * 4f;
					if (!Open(start + right * (t - 1)))
					{
						a -= right;
					}
					for (; t < tiles && Open(start + right * t + normal); t++)
					{
					}
					Vector2 b = (start + right * t) * 8f + new Vector2(4f) - right * 4f + normal * 4f;
					if (!Open(start + right * t))
					{
						b += right;
					}
					lines.Add(new Line(a + normal, b + normal));
				}
			}
		}

		private bool Open(Vector2 tile)
		{
			Vector2 pos = new Vector2(base.X + tile.X * 8f + 4f, base.Y + tile.Y * 8f + 4f);
			if (!base.Scene.CollideCheck<SolidTiles>(pos))
			{
				return !base.Scene.CollideCheck<GlassBlock>(pos);
			}
			return false;
		}

		public override void Render()
		{
			foreach (Line line in lines)
			{
				Draw.Line(Position + line.A, Position + line.B, lineColor);
			}
		}
	}
}
