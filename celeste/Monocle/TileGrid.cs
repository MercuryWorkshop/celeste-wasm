using System;
using Microsoft.Xna.Framework;

namespace Monocle
{
	public class TileGrid : Component
	{
		public Vector2 Position;

		public Color Color = Color.White;

		public int VisualExtend;

		public VirtualMap<MTexture> Tiles;

		public Camera ClipCamera;

		public float Alpha = 1f;

		public int TileWidth { get; private set; }

		public int TileHeight { get; private set; }

		public int TilesX => Tiles.Columns;

		public int TilesY => Tiles.Rows;

		public TileGrid(int tileWidth, int tileHeight, int tilesX, int tilesY)
			: base(active: false, visible: true)
		{
			TileWidth = tileWidth;
			TileHeight = tileHeight;
			Tiles = new VirtualMap<MTexture>(tilesX, tilesY);
		}

		public void Populate(Tileset tileset, int[,] tiles, int offsetX = 0, int offsetY = 0)
		{
			for (int x = 0; x < tiles.GetLength(0) && x + offsetX < TilesX; x++)
			{
				for (int y = 0; y < tiles.GetLength(1) && y + offsetY < TilesY; y++)
				{
					Tiles[x + offsetX, y + offsetY] = tileset[tiles[x, y]];
				}
			}
		}

		public void Overlay(Tileset tileset, int[,] tiles, int offsetX = 0, int offsetY = 0)
		{
			for (int x = 0; x < tiles.GetLength(0) && x + offsetX < TilesX; x++)
			{
				for (int y = 0; y < tiles.GetLength(1) && y + offsetY < TilesY; y++)
				{
					if (tiles[x, y] >= 0)
					{
						Tiles[x + offsetX, y + offsetY] = tileset[tiles[x, y]];
					}
				}
			}
		}

		public void Extend(int left, int right, int up, int down)
		{
			Position -= new Vector2(left * TileWidth, up * TileHeight);
			int newWidth = TilesX + left + right;
			int newHeight = TilesY + up + down;
			if (newWidth <= 0 || newHeight <= 0)
			{
				Tiles = new VirtualMap<MTexture>(0, 0);
				return;
			}
			VirtualMap<MTexture> newTiles = new VirtualMap<MTexture>(newWidth, newHeight);
			for (int x5 = 0; x5 < TilesX; x5++)
			{
				for (int y = 0; y < TilesY; y++)
				{
					int atX = x5 + left;
					int atY = y + up;
					if (atX >= 0 && atX < newWidth && atY >= 0 && atY < newHeight)
					{
						newTiles[atX, atY] = Tiles[x5, y];
					}
				}
			}
			for (int x4 = 0; x4 < left; x4++)
			{
				for (int y2 = 0; y2 < newHeight; y2++)
				{
					newTiles[x4, y2] = Tiles[0, Calc.Clamp(y2 - up, 0, TilesY - 1)];
				}
			}
			for (int x3 = newWidth - right; x3 < newWidth; x3++)
			{
				for (int y3 = 0; y3 < newHeight; y3++)
				{
					newTiles[x3, y3] = Tiles[TilesX - 1, Calc.Clamp(y3 - up, 0, TilesY - 1)];
				}
			}
			for (int y5 = 0; y5 < up; y5++)
			{
				for (int x = 0; x < newWidth; x++)
				{
					newTiles[x, y5] = Tiles[Calc.Clamp(x - left, 0, TilesX - 1), 0];
				}
			}
			for (int y4 = newHeight - down; y4 < newHeight; y4++)
			{
				for (int x2 = 0; x2 < newWidth; x2++)
				{
					newTiles[x2, y4] = Tiles[Calc.Clamp(x2 - left, 0, TilesX - 1), TilesY - 1];
				}
			}
			Tiles = newTiles;
		}

		public void FillRect(int x, int y, int columns, int rows, MTexture tile)
		{
			int num = Math.Max(0, x);
			int top = Math.Max(0, y);
			int right = Math.Min(TilesX, x + columns);
			int bottom = Math.Min(TilesY, y + rows);
			for (int tx = num; tx < right; tx++)
			{
				for (int ty = top; ty < bottom; ty++)
				{
					Tiles[tx, ty] = tile;
				}
			}
		}

		public void Clear()
		{
			for (int tx = 0; tx < TilesX; tx++)
			{
				for (int ty = 0; ty < TilesY; ty++)
				{
					Tiles[tx, ty] = null;
				}
			}
		}

		public Rectangle GetClippedRenderTiles()
		{
			Vector2 pos = base.Entity.Position + Position;
			int left;
			int top;
			int right;
			int bottom;
			if (ClipCamera == null)
			{
				left = -VisualExtend;
				top = -VisualExtend;
				right = TilesX + VisualExtend;
				bottom = TilesY + VisualExtend;
			}
			else
			{
				Camera camera = ClipCamera;
				left = (int)Math.Max(0.0, Math.Floor((camera.Left - pos.X) / (float)TileWidth) - (double)VisualExtend);
				top = (int)Math.Max(0.0, Math.Floor((camera.Top - pos.Y) / (float)TileHeight) - (double)VisualExtend);
				right = (int)Math.Min(TilesX, Math.Ceiling((camera.Right - pos.X) / (float)TileWidth) + (double)VisualExtend);
				bottom = (int)Math.Min(TilesY, Math.Ceiling((camera.Bottom - pos.Y) / (float)TileHeight) + (double)VisualExtend);
			}
			left = Math.Max(left, 0);
			top = Math.Max(top, 0);
			right = Math.Min(right, TilesX);
			bottom = Math.Min(bottom, TilesY);
			return new Rectangle(left, top, right - left, bottom - top);
		}

		public override void Render()
		{
			RenderAt(base.Entity.Position + Position);
		}

		public void RenderAt(Vector2 position)
		{
			if (Alpha <= 0f)
			{
				return;
			}
			Rectangle clip = GetClippedRenderTiles();
			Color color = Color * Alpha;
			for (int tx = clip.Left; tx < clip.Right; tx++)
			{
				for (int ty = clip.Top; ty < clip.Bottom; ty++)
				{
					Tiles[tx, ty]?.Draw(position + new Vector2(tx * TileWidth, ty * TileHeight), Vector2.Zero, color);
				}
			}
		}
	}
}
