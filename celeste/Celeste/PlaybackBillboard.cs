using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class PlaybackBillboard : Entity
	{
		private class FG : Entity
		{
			public PlaybackBillboard Parent;

			public FG(PlaybackBillboard parent)
			{
				Parent = parent;
				base.Depth = Parent.Depth - 5;
			}

			public override void Render()
			{
				uint rand = Parent.Seed;
				DrawNoise(Parent.Collider.Bounds, ref rand, Color.White * 0.1f);
				for (int y = (int)Parent.Y; (float)y < Parent.Bottom; y += 2)
				{
					float alpha = 0.05f + (1f + (float)Math.Sin((float)y / 16f + base.Scene.TimeActive * 2f)) / 2f * 0.2f;
					Draw.Line(Parent.X, y, Parent.X + Parent.Width, y, Color.Teal * alpha);
				}
			}
		}

		public const int BGDepth = 9010;

		public static readonly Color BackgroundColor = Color.Lerp(Color.DarkSlateBlue, Color.Black, 0.6f);

		public uint Seed;

		private MTexture[,] tiles;

		public PlaybackBillboard(EntityData e, Vector2 offset)
		{
			Position = e.Position + offset;
			base.Collider = new Hitbox(e.Width, e.Height);
			base.Depth = 9010;
			Add(new CustomBloom(RenderBloom));
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			scene.Add(new FG(this));
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			MTexture tex = GFX.Game["scenery/tvSlices"];
			tiles = new MTexture[tex.Width / 8, tex.Height / 8];
			for (int x2 = 0; x2 < tex.Width / 8; x2++)
			{
				for (int y = 0; y < tex.Height / 8; y++)
				{
					tiles[x2, y] = tex.GetSubtexture(new Rectangle(x2 * 8, y * 8, 8, 8));
				}
			}
			int columns = (int)(base.Width / 8f);
			int rows = (int)(base.Height / 8f);
			for (int x = -1; x <= columns; x++)
			{
				AutoTile(x, -1);
				AutoTile(x, rows);
			}
			for (int y2 = 0; y2 < rows; y2++)
			{
				AutoTile(-1, y2);
				AutoTile(columns, y2);
			}
		}

		private void AutoTile(int x, int y)
		{
			if (Empty(x, y))
			{
				bool i = !Empty(x - 1, y);
				bool r = !Empty(x + 1, y);
				bool u = !Empty(x, y - 1);
				bool d = !Empty(x, y + 1);
				bool ul = !Empty(x - 1, y - 1);
				bool ur = !Empty(x + 1, y - 1);
				bool dl = !Empty(x - 1, y + 1);
				bool dr = !Empty(x + 1, y + 1);
				if (!r && !d && dr)
				{
					Tile(x, y, tiles[0, 0]);
				}
				else if (!i && !d && dl)
				{
					Tile(x, y, tiles[2, 0]);
				}
				else if (!u && !r && ur)
				{
					Tile(x, y, tiles[0, 2]);
				}
				else if (!u && !i && ul)
				{
					Tile(x, y, tiles[2, 2]);
				}
				else if (r && d)
				{
					Tile(x, y, tiles[3, 0]);
				}
				else if (i && d)
				{
					Tile(x, y, tiles[4, 0]);
				}
				else if (r && u)
				{
					Tile(x, y, tiles[3, 2]);
				}
				else if (i && u)
				{
					Tile(x, y, tiles[4, 2]);
				}
				else if (d)
				{
					Tile(x, y, tiles[1, 0]);
				}
				else if (r)
				{
					Tile(x, y, tiles[0, 1]);
				}
				else if (i)
				{
					Tile(x, y, tiles[2, 1]);
				}
				else if (u)
				{
					Tile(x, y, tiles[1, 2]);
				}
			}
		}

		private void Tile(int x, int y, MTexture tile)
		{
			Image image = new Image(tile);
			image.Position = new Vector2(x, y) * 8f;
			Add(image);
		}

		private bool Empty(int x, int y)
		{
			return !base.Scene.CollideCheck<PlaybackBillboard>(new Rectangle((int)base.X + x * 8, (int)base.Y + y * 8, 8, 8));
		}

		public override void Update()
		{
			base.Update();
			if (base.Scene.OnInterval(0.1f))
			{
				Seed++;
			}
		}

		private void RenderBloom()
		{
			Draw.Rect(base.Collider, Color.White * 0.4f);
		}

		public override void Render()
		{
			base.Render();
			uint rand = Seed;
			Draw.Rect(base.Collider, BackgroundColor);
			DrawNoise(base.Collider.Bounds, ref rand, Color.White * 0.1f);
		}

		public static void DrawNoise(Rectangle bounds, ref uint seed, Color color)
		{
			MTexture tex = GFX.Game["util/noise"];
			Vector2 offset = new Vector2(PseudoRandRange(ref seed, 0f, tex.Width / 2), PseudoRandRange(ref seed, 0f, tex.Height / 2));
			Vector2 step = new Vector2(tex.Width, tex.Height) / 2f;
			for (float x = 0f; x < (float)bounds.Width; x += step.X)
			{
				float sw = Math.Min((float)bounds.Width - x, step.X);
				for (float y = 0f; y < (float)bounds.Height; y += step.Y)
				{
					float sh = Math.Min((float)bounds.Height - y, step.Y);
					int sx = (int)((float)tex.ClipRect.X + offset.X);
					int sy = (int)((float)tex.ClipRect.Y + offset.Y);
					Rectangle source = new Rectangle(sx, sy, (int)sw, (int)sh);
					Draw.SpriteBatch.Draw(tex.Texture.Texture, new Vector2((float)bounds.X + x, (float)bounds.Y + y), source, color);
				}
			}
		}

		private static uint PseudoRand(ref uint seed)
		{
			seed ^= seed << 13;
			seed ^= seed >> 17;
			return seed;
		}

		private static float PseudoRandRange(ref uint seed, float min, float max)
		{
			return min + (float)(PseudoRand(ref seed) % 1000u) / 1000f * (max - min);
		}
	}
}
