using System;

namespace Monocle
{
	public static class Tiler
	{
		public enum EdgeBehavior
		{
			True,
			False,
			Wrap
		}

		public static int TileX { get; private set; }

		public static int TileY { get; private set; }

		public static bool Left { get; private set; }

		public static bool Right { get; private set; }

		public static bool Up { get; private set; }

		public static bool Down { get; private set; }

		public static bool UpLeft { get; private set; }

		public static bool UpRight { get; private set; }

		public static bool DownLeft { get; private set; }

		public static bool DownRight { get; private set; }

		public static int[,] Tile(bool[,] bits, Func<int> tileDecider, Action<int> tileOutput, int tileWidth, int tileHeight, EdgeBehavior edges)
		{
			int boundsX = bits.GetLength(0);
			int boundsY = bits.GetLength(1);
			int[,] tiles = new int[boundsX, boundsY];
			for (TileX = 0; TileX < boundsX; TileX++)
			{
				for (TileY = 0; TileY < boundsY; TileY++)
				{
					if (bits[TileX, TileY])
					{
						switch (edges)
						{
						case EdgeBehavior.True:
							Left = TileX == 0 || bits[TileX - 1, TileY];
							Right = TileX == boundsX - 1 || bits[TileX + 1, TileY];
							Up = TileY == 0 || bits[TileX, TileY - 1];
							Down = TileY == boundsY - 1 || bits[TileX, TileY + 1];
							UpLeft = TileX == 0 || TileY == 0 || bits[TileX - 1, TileY - 1];
							UpRight = TileX == boundsX - 1 || TileY == 0 || bits[TileX + 1, TileY - 1];
							DownLeft = TileX == 0 || TileY == boundsY - 1 || bits[TileX - 1, TileY + 1];
							DownRight = TileX == boundsX - 1 || TileY == boundsY - 1 || bits[TileX + 1, TileY + 1];
							break;
						case EdgeBehavior.False:
							Left = TileX != 0 && bits[TileX - 1, TileY];
							Right = TileX != boundsX - 1 && bits[TileX + 1, TileY];
							Up = TileY != 0 && bits[TileX, TileY - 1];
							Down = TileY != boundsY - 1 && bits[TileX, TileY + 1];
							UpLeft = TileX != 0 && TileY != 0 && bits[TileX - 1, TileY - 1];
							UpRight = TileX != boundsX - 1 && TileY != 0 && bits[TileX + 1, TileY - 1];
							DownLeft = TileX != 0 && TileY != boundsY - 1 && bits[TileX - 1, TileY + 1];
							DownRight = TileX != boundsX - 1 && TileY != boundsY - 1 && bits[TileX + 1, TileY + 1];
							break;
						case EdgeBehavior.Wrap:
							Left = bits[(TileX + boundsX - 1) % boundsX, TileY];
							Right = bits[(TileX + 1) % boundsX, TileY];
							Up = bits[TileX, (TileY + boundsY - 1) % boundsY];
							Down = bits[TileX, (TileY + 1) % boundsY];
							UpLeft = bits[(TileX + boundsX - 1) % boundsX, (TileY + boundsY - 1) % boundsY];
							UpRight = bits[(TileX + 1) % boundsX, (TileY + boundsY - 1) % boundsY];
							DownLeft = bits[(TileX + boundsX - 1) % boundsX, (TileY + 1) % boundsY];
							DownRight = bits[(TileX + 1) % boundsX, (TileY + 1) % boundsY];
							break;
						}
						int tile = tileDecider();
						tileOutput(tile);
						tiles[TileX, TileY] = tile;
					}
				}
			}
			return tiles;
		}

		public static int[,] Tile(bool[,] bits, bool[,] mask, Func<int> tileDecider, Action<int> tileOutput, int tileWidth, int tileHeight, EdgeBehavior edges)
		{
			int boundsX = bits.GetLength(0);
			int boundsY = bits.GetLength(1);
			int[,] tiles = new int[boundsX, boundsY];
			for (TileX = 0; TileX < boundsX; TileX++)
			{
				for (TileY = 0; TileY < boundsY; TileY++)
				{
					if (bits[TileX, TileY])
					{
						switch (edges)
						{
						case EdgeBehavior.True:
							Left = TileX == 0 || bits[TileX - 1, TileY] || mask[TileX - 1, TileY];
							Right = TileX == boundsX - 1 || bits[TileX + 1, TileY] || mask[TileX + 1, TileY];
							Up = TileY == 0 || bits[TileX, TileY - 1] || mask[TileX, TileY - 1];
							Down = TileY == boundsY - 1 || bits[TileX, TileY + 1] || mask[TileX, TileY + 1];
							UpLeft = TileX == 0 || TileY == 0 || bits[TileX - 1, TileY - 1] || mask[TileX - 1, TileY - 1];
							UpRight = TileX == boundsX - 1 || TileY == 0 || bits[TileX + 1, TileY - 1] || mask[TileX + 1, TileY - 1];
							DownLeft = TileX == 0 || TileY == boundsY - 1 || bits[TileX - 1, TileY + 1] || mask[TileX - 1, TileY + 1];
							DownRight = TileX == boundsX - 1 || TileY == boundsY - 1 || bits[TileX + 1, TileY + 1] || mask[TileX + 1, TileY + 1];
							break;
						case EdgeBehavior.False:
							Left = TileX != 0 && (bits[TileX - 1, TileY] || mask[TileX - 1, TileY]);
							Right = TileX != boundsX - 1 && (bits[TileX + 1, TileY] || mask[TileX + 1, TileY]);
							Up = TileY != 0 && (bits[TileX, TileY - 1] || mask[TileX, TileY - 1]);
							Down = TileY != boundsY - 1 && (bits[TileX, TileY + 1] || mask[TileX, TileY + 1]);
							UpLeft = TileX != 0 && TileY != 0 && (bits[TileX - 1, TileY - 1] || mask[TileX - 1, TileY - 1]);
							UpRight = TileX != boundsX - 1 && TileY != 0 && (bits[TileX + 1, TileY - 1] || mask[TileX + 1, TileY - 1]);
							DownLeft = TileX != 0 && TileY != boundsY - 1 && (bits[TileX - 1, TileY + 1] || mask[TileX - 1, TileY + 1]);
							DownRight = TileX != boundsX - 1 && TileY != boundsY - 1 && (bits[TileX + 1, TileY + 1] || mask[TileX + 1, TileY + 1]);
							break;
						case EdgeBehavior.Wrap:
							Left = bits[(TileX + boundsX - 1) % boundsX, TileY] || mask[(TileX + boundsX - 1) % boundsX, TileY];
							Right = bits[(TileX + 1) % boundsX, TileY] || mask[(TileX + 1) % boundsX, TileY];
							Up = bits[TileX, (TileY + boundsY - 1) % boundsY] || mask[TileX, (TileY + boundsY - 1) % boundsY];
							Down = bits[TileX, (TileY + 1) % boundsY] || mask[TileX, (TileY + 1) % boundsY];
							UpLeft = bits[(TileX + boundsX - 1) % boundsX, (TileY + boundsY - 1) % boundsY] || mask[(TileX + boundsX - 1) % boundsX, (TileY + boundsY - 1) % boundsY];
							UpRight = bits[(TileX + 1) % boundsX, (TileY + boundsY - 1) % boundsY] || mask[(TileX + 1) % boundsX, (TileY + boundsY - 1) % boundsY];
							DownLeft = bits[(TileX + boundsX - 1) % boundsX, (TileY + 1) % boundsY] || mask[(TileX + boundsX - 1) % boundsX, (TileY + 1) % boundsY];
							DownRight = bits[(TileX + 1) % boundsX, (TileY + 1) % boundsY] || mask[(TileX + 1) % boundsX, (TileY + 1) % boundsY];
							break;
						}
						int tile = tileDecider();
						tileOutput(tile);
						tiles[TileX, TileY] = tile;
					}
				}
			}
			return tiles;
		}

		public static int[,] Tile(bool[,] bits, AutotileData autotileData, Action<int> tileOutput, int tileWidth, int tileHeight, EdgeBehavior edges)
		{
			return Tile(bits, autotileData.TileHandler, tileOutput, tileWidth, tileHeight, edges);
		}

		public static int[,] Tile(bool[,] bits, bool[,] mask, AutotileData autotileData, Action<int> tileOutput, int tileWidth, int tileHeight, EdgeBehavior edges)
		{
			return Tile(bits, mask, autotileData.TileHandler, tileOutput, tileWidth, tileHeight, edges);
		}
	}
}
