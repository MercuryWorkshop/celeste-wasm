using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class SolidTiles : Solid
	{
		public TileGrid Tiles;

		public AnimatedTiles AnimatedTiles;

		public Grid Grid;

		private VirtualMap<char> tileTypes;

		public SolidTiles(Vector2 position, VirtualMap<char> data)
			: base(position, 0f, 0f, safe: true)
		{
			base.Tag = Tags.Global;
			base.Depth = -10000;
			tileTypes = data;
			EnableAssistModeChecks = false;
			AllowStaticMovers = false;
			base.Collider = (Grid = new Grid(data.Columns, data.Rows, 8f, 8f));
			for (int sx = 0; sx < data.Columns; sx += 50)
			{
				for (int sy = 0; sy < data.Rows; sy += 50)
				{
					if (!data.AnyInSegmentAtTile(sx, sy))
					{
						continue;
					}
					int tx = sx;
					for (int txw = Math.Min(tx + 50, data.Columns); tx < txw; tx++)
					{
						int ty = sy;
						for (int tyh = Math.Min(ty + 50, data.Rows); ty < tyh; ty++)
						{
							if (data[tx, ty] != '0')
							{
								Grid[tx, ty] = true;
							}
						}
					}
				}
			}
			Autotiler.Generated result = GFX.FGAutotiler.GenerateMap(data, paddingIgnoreOutOfLevel: true);
			Tiles = result.TileGrid;
			Tiles.VisualExtend = 1;
			Add(Tiles);
			Add(AnimatedTiles = result.SpriteOverlay);
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Tiles.ClipCamera = SceneAs<Level>().Camera;
			AnimatedTiles.ClipCamera = Tiles.ClipCamera;
		}

		private int CoreTileSurfaceIndex()
		{
			Level level = base.Scene as Level;
			if (level.CoreMode == Session.CoreModes.Hot)
			{
				return 37;
			}
			if (level.CoreMode == Session.CoreModes.Cold)
			{
				return 36;
			}
			return 3;
		}

		private int SurfaceSoundIndexAt(Vector2 readPosition)
		{
			int tx = (int)((readPosition.X - base.X) / 8f);
			int ty = (int)((readPosition.Y - base.Y) / 8f);
			if (tx >= 0 && ty >= 0 && tx < Grid.CellsX && ty < Grid.CellsY)
			{
				char tileType = tileTypes[tx, ty];
				switch (tileType)
				{
				case 'k':
					return CoreTileSurfaceIndex();
				default:
					if (SurfaceIndex.TileToIndex.ContainsKey(tileType))
					{
						return SurfaceIndex.TileToIndex[tileType];
					}
					break;
				case '0':
					break;
				}
			}
			return -1;
		}

		public override int GetWallSoundIndex(Player player, int side)
		{
			int value = SurfaceSoundIndexAt(player.Center + Vector2.UnitX * side * 8f);
			if (value < 0)
			{
				value = SurfaceSoundIndexAt(player.Center + new Vector2(side * 8, -6f));
			}
			if (value < 0)
			{
				value = SurfaceSoundIndexAt(player.Center + new Vector2(side * 8, 6f));
			}
			return value;
		}

		public override int GetStepSoundIndex(Entity entity)
		{
			int index = SurfaceSoundIndexAt(entity.BottomCenter + Vector2.UnitY * 4f);
			if (index == -1)
			{
				index = SurfaceSoundIndexAt(entity.BottomLeft + Vector2.UnitY * 4f);
			}
			if (index == -1)
			{
				index = SurfaceSoundIndexAt(entity.BottomRight + Vector2.UnitY * 4f);
			}
			return index;
		}

		public override int GetLandSoundIndex(Entity entity)
		{
			int index = SurfaceSoundIndexAt(entity.BottomCenter + Vector2.UnitY * 4f);
			if (index == -1)
			{
				index = SurfaceSoundIndexAt(entity.BottomLeft + Vector2.UnitY * 4f);
			}
			if (index == -1)
			{
				index = SurfaceSoundIndexAt(entity.BottomRight + Vector2.UnitY * 4f);
			}
			return index;
		}
	}
}
