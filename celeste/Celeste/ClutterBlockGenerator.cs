using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public static class ClutterBlockGenerator
	{
		private struct Tile
		{
			public int Color;

			public bool Wall;

			public ClutterBlock Block;

			public bool Empty
			{
				get
				{
					if (!Wall)
					{
						return Color == -1;
					}
					return false;
				}
			}
		}

		private class TextureSet
		{
			public int Columns;

			public int Rows;

			public List<MTexture> textures = new List<MTexture>();
		}

		private static Level level;

		private static Tile[,] tiles;

		private static List<Point> active = new List<Point>();

		private static List<List<TextureSet>> textures;

		private static int columns;

		private static int rows;

		private static bool[] enabled = new bool[3];

		private static bool initialized;

		public static void Init(Level lvl)
		{
			if (initialized)
			{
				return;
			}
			initialized = true;
			level = lvl;
			columns = level.Bounds.Width / 8;
			rows = level.Bounds.Height / 8 + 1;
			if (tiles == null)
			{
				tiles = new Tile[200, 200];
			}
			for (int x2 = 0; x2 < columns; x2++)
			{
				for (int y = 0; y < rows; y++)
				{
					tiles[x2, y].Color = -1;
					tiles[x2, y].Block = null;
				}
			}
			for (int j = 0; j < enabled.Length; j++)
			{
				enabled[j] = !level.Session.GetFlag("oshiro_clutter_cleared_" + j);
			}
			if (textures == null)
			{
				textures = new List<List<TextureSet>>();
				for (int i = 0; i < 3; i++)
				{
					List<TextureSet> sets = new List<TextureSet>();
					Atlas game = GFX.Game;
					ClutterBlock.Colors colors = (ClutterBlock.Colors)i;
					foreach (MTexture tex in game.GetAtlasSubtextures("objects/resortclutter/" + colors.ToString() + "_"))
					{
						int tColumns = tex.Width / 8;
						int tRows = tex.Height / 8;
						TextureSet set = null;
						foreach (TextureSet next in sets)
						{
							if (next.Columns == tColumns && next.Rows == tRows)
							{
								set = next;
								break;
							}
						}
						if (set == null)
						{
							TextureSet obj = new TextureSet
							{
								Columns = tColumns,
								Rows = tRows
							};
							set = obj;
							sets.Add(obj);
						}
						set.textures.Add(tex);
					}
					sets.Sort((TextureSet a, TextureSet b) => -Math.Sign(a.Columns * a.Rows - b.Columns * b.Rows));
					textures.Add(sets);
				}
			}
			Point solidsOffset = level.LevelSolidOffset;
			for (int x = 0; x < columns; x++)
			{
				for (int y2 = 0; y2 < rows; y2++)
				{
					tiles[x, y2].Wall = level.SolidsData[solidsOffset.X + x, solidsOffset.Y + y2] != '0';
				}
			}
		}

		public static void Dispose()
		{
			textures = null;
			tiles = null;
			initialized = false;
		}

		public static void Add(int x, int y, int w, int h, ClutterBlock.Colors color)
		{
			level.Add(new ClutterBlockBase(new Vector2(level.Bounds.X, level.Bounds.Y) + new Vector2(x, y) * 8f, w * 8, h * 8, enabled[(int)color], color));
			if (!enabled[(int)color])
			{
				return;
			}
			int i = Math.Max(0, x);
			for (int c = Math.Min(columns, x + w); i < c; i++)
			{
				int j = Math.Max(0, y);
				for (int r = Math.Min(rows, y + h); j < r; j++)
				{
					Point point = new Point(i, j);
					tiles[point.X, point.Y].Color = (int)color;
					active.Add(point);
				}
			}
		}

		public static void Generate()
		{
			if (!initialized)
			{
				return;
			}
			active.Shuffle();
			List<ClutterBlock> blocks = new List<ClutterBlock>();
			Rectangle levelbounds = level.Bounds;
			foreach (Point point in active)
			{
				if (tiles[point.X, point.Y].Block != null)
				{
					continue;
				}
				int index = 0;
				int color;
				TextureSet set;
				while (true)
				{
					color = tiles[point.X, point.Y].Color;
					set = textures[color][index];
					bool fits = true;
					if (point.X + set.Columns <= columns && point.Y + set.Rows <= rows)
					{
						int tx3 = point.X;
						int c = point.X + set.Columns;
						while (fits && tx3 < c)
						{
							int ty3 = point.Y;
							int r = point.Y + set.Rows;
							while (fits && ty3 < r)
							{
								Tile tile2 = tiles[tx3, ty3];
								if (tile2.Block != null || tile2.Color != color)
								{
									fits = false;
								}
								ty3++;
							}
							tx3++;
						}
						if (fits)
						{
							break;
						}
					}
					index++;
				}
				ClutterBlock block3 = new ClutterBlock(new Vector2(levelbounds.X, levelbounds.Y) + new Vector2(point.X, point.Y) * 8f, Calc.Random.Choose(set.textures), (ClutterBlock.Colors)color);
				for (int tx2 = point.X; tx2 < point.X + set.Columns; tx2++)
				{
					for (int ty2 = point.Y; ty2 < point.Y + set.Rows; ty2++)
					{
						tiles[tx2, ty2].Block = block3;
					}
				}
				blocks.Add(block3);
				level.Add(block3);
			}
			for (int tx = 0; tx < columns; tx++)
			{
				for (int ty = 0; ty < rows; ty++)
				{
					Tile tile = tiles[tx, ty];
					if (tile.Block == null)
					{
						continue;
					}
					ClutterBlock block = tile.Block;
					if (!block.TopSideOpen && (ty == 0 || tiles[tx, ty - 1].Empty))
					{
						block.TopSideOpen = true;
					}
					if (!block.LeftSideOpen && (tx == 0 || tiles[tx - 1, ty].Empty))
					{
						block.LeftSideOpen = true;
					}
					if (!block.RightSideOpen && (tx == columns - 1 || tiles[tx + 1, ty].Empty))
					{
						block.RightSideOpen = true;
					}
					if (!block.OnTheGround && ty < rows - 1)
					{
						Tile below = tiles[tx, ty + 1];
						if (below.Wall)
						{
							block.OnTheGround = true;
						}
						else if (below.Block != null && below.Block != block && !block.HasBelow.Contains(below.Block))
						{
							block.HasBelow.Add(below.Block);
							block.Below.Add(below.Block);
							below.Block.Above.Add(block);
						}
					}
				}
			}
			foreach (ClutterBlock block2 in blocks)
			{
				if (block2.OnTheGround)
				{
					SetAboveToOnGround(block2);
				}
			}
			initialized = false;
			level = null;
			active.Clear();
		}

		private static void SetAboveToOnGround(ClutterBlock block)
		{
			foreach (ClutterBlock above in block.Above)
			{
				if (!above.OnTheGround)
				{
					above.OnTheGround = true;
					SetAboveToOnGround(above);
				}
			}
		}
	}
}
