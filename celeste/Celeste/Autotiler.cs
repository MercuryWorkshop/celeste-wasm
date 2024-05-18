using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Autotiler
	{
		private class TerrainType
		{
			public char ID;

			public HashSet<char> Ignores = new HashSet<char>();

			public List<Masked> Masked = new List<Masked>();

			public Tiles Center = new Tiles();

			public Tiles Padded = new Tiles();

			public TerrainType(char id)
			{
				ID = id;
			}

			public bool Ignore(char c)
			{
				if (ID != c)
				{
					if (!Ignores.Contains(c))
					{
						return Ignores.Contains('*');
					}
					return true;
				}
				return false;
			}
		}

		private class Masked
		{
			public byte[] Mask = new byte[9];

			public Tiles Tiles = new Tiles();
		}

		private class Tiles
		{
			public List<MTexture> Textures = new List<MTexture>();

			public List<string> OverlapSprites = new List<string>();

			public bool HasOverlays;
		}

		public struct Generated
		{
			public TileGrid TileGrid;

			public AnimatedTiles SpriteOverlay;
		}

		public struct Behaviour
		{
			public bool PaddingIgnoreOutOfLevel;

			public bool EdgesIgnoreOutOfLevel;

			public bool EdgesExtend;
		}

		public List<Rectangle> LevelBounds = new List<Rectangle>();

		private Dictionary<char, TerrainType> lookup = new Dictionary<char, TerrainType>();

		private byte[] adjacent = new byte[9];

		public Autotiler(string filename)
		{
			Dictionary<char, XmlElement> xmls = new Dictionary<char, XmlElement>();
			foreach (XmlElement xml in Calc.LoadContentXML(filename).GetElementsByTagName("Tileset"))
			{
				char id = xml.AttrChar("id");
				Tileset tileset = new Tileset(GFX.Game["tilesets/" + xml.Attr("path")], 8, 8);
				TerrainType type = new TerrainType(id);
				ReadInto(type, tileset, xml);
				if (xml.HasAttr("copy"))
				{
					char copy = xml.AttrChar("copy");
					if (!xmls.ContainsKey(copy))
					{
						throw new Exception("Copied tilesets must be defined before the tilesets that copy them!");
					}
					ReadInto(type, tileset, xmls[copy]);
				}
				if (xml.HasAttr("ignores"))
				{
					string[] array = xml.Attr("ignores").Split(',');
					foreach (string c in array)
					{
						if (c.Length > 0)
						{
							type.Ignores.Add(c[0]);
						}
					}
				}
				xmls.Add(id, xml);
				lookup.Add(id, type);
			}
		}

		private void ReadInto(TerrainType data, Tileset tileset, XmlElement xml)
		{
			foreach (object maskNode in xml)
			{
				if (maskNode is XmlComment)
				{
					continue;
				}
				XmlElement tileList = maskNode as XmlElement;
				string mask = tileList.Attr("mask");
				Tiles tiles;
				if (mask == "center")
				{
					tiles = data.Center;
				}
				else if (mask == "padding")
				{
					tiles = data.Padded;
				}
				else
				{
					Masked set = new Masked();
					tiles = set.Tiles;
					int i = 0;
					int index = 0;
					for (; i < mask.Length; i++)
					{
						if (mask[i] == '0')
						{
							set.Mask[index++] = 0;
						}
						else if (mask[i] == '1')
						{
							set.Mask[index++] = 1;
						}
						else if (mask[i] == 'x' || mask[i] == 'X')
						{
							set.Mask[index++] = 2;
						}
					}
					data.Masked.Add(set);
				}
				string[] array = tileList.Attr("tiles").Split(';');
				for (int j = 0; j < array.Length; j++)
				{
					string[] array2 = array[j].Split(',');
					int tx = int.Parse(array2[0]);
					int ty = int.Parse(array2[1]);
					MTexture tex = tileset[tx, ty];
					tiles.Textures.Add(tex);
				}
				if (tileList.HasAttr("sprites"))
				{
					array = tileList.Attr("sprites").Split(',');
					foreach (string name in array)
					{
						tiles.OverlapSprites.Add(name);
					}
					tiles.HasOverlays = true;
				}
			}
			data.Masked.Sort(delegate(Masked a, Masked b)
			{
				int num = 0;
				int num2 = 0;
				for (int k = 0; k < 9; k++)
				{
					if (a.Mask[k] == 2)
					{
						num++;
					}
					if (b.Mask[k] == 2)
					{
						num2++;
					}
				}
				return num - num2;
			});
		}

		public Generated GenerateMap(VirtualMap<char> mapData, bool paddingIgnoreOutOfLevel)
		{
			Behaviour behaviour2 = default(Behaviour);
			behaviour2.EdgesExtend = true;
			behaviour2.EdgesIgnoreOutOfLevel = false;
			behaviour2.PaddingIgnoreOutOfLevel = paddingIgnoreOutOfLevel;
			Behaviour behaviour = behaviour2;
			return Generate(mapData, 0, 0, mapData.Columns, mapData.Rows, forceSolid: false, '0', behaviour);
		}

		public Generated GenerateMap(VirtualMap<char> mapData, Behaviour behaviour)
		{
			return Generate(mapData, 0, 0, mapData.Columns, mapData.Rows, forceSolid: false, '0', behaviour);
		}

		public Generated GenerateBox(char id, int tilesX, int tilesY)
		{
			return Generate(null, 0, 0, tilesX, tilesY, forceSolid: true, id, default(Behaviour));
		}

		public Generated GenerateOverlay(char id, int x, int y, int tilesX, int tilesY, VirtualMap<char> mapData)
		{
			Behaviour behaviour2 = default(Behaviour);
			behaviour2.EdgesExtend = true;
			behaviour2.EdgesIgnoreOutOfLevel = true;
			behaviour2.PaddingIgnoreOutOfLevel = true;
			Behaviour behaviour = behaviour2;
			return Generate(mapData, x, y, tilesX, tilesY, forceSolid: true, id, behaviour);
		}

		private Generated Generate(VirtualMap<char> mapData, int startX, int startY, int tilesX, int tilesY, bool forceSolid, char forceID, Behaviour behaviour)
		{
			TileGrid grid = new TileGrid(8, 8, tilesX, tilesY);
			AnimatedTiles overlay = new AnimatedTiles(tilesX, tilesY, GFX.AnimatedTilesBank);
			Rectangle forceFill = Rectangle.Empty;
			if (forceSolid)
			{
				forceFill = new Rectangle(startX, startY, tilesX, tilesY);
			}
			if (mapData != null)
			{
				for (int x2 = startX; x2 < startX + tilesX; x2 += 50)
				{
					for (int y2 = startY; y2 < startY + tilesY; y2 += 50)
					{
						if (!mapData.AnyInSegmentAtTile(x2, y2))
						{
							y2 = y2 / 50 * 50;
							continue;
						}
						int sx = x2;
						for (int sxw = Math.Min(x2 + 50, startX + tilesX); sx < sxw; sx++)
						{
							int sy = y2;
							for (int syh = Math.Min(y2 + 50, startY + tilesY); sy < syh; sy++)
							{
								Tiles tile2 = TileHandler(mapData, sx, sy, forceFill, forceID, behaviour);
								if (tile2 != null)
								{
									grid.Tiles[sx - startX, sy - startY] = Calc.Random.Choose(tile2.Textures);
									if (tile2.HasOverlays)
									{
										overlay.Set(sx - startX, sy - startY, Calc.Random.Choose(tile2.OverlapSprites));
									}
								}
							}
						}
					}
				}
			}
			else
			{
				for (int x = startX; x < startX + tilesX; x++)
				{
					for (int y = startY; y < startY + tilesY; y++)
					{
						Tiles tile = TileHandler(null, x, y, forceFill, forceID, behaviour);
						if (tile != null)
						{
							grid.Tiles[x - startX, y - startY] = Calc.Random.Choose(tile.Textures);
							if (tile.HasOverlays)
							{
								overlay.Set(x - startX, y - startY, Calc.Random.Choose(tile.OverlapSprites));
							}
						}
					}
				}
			}
			Generated result = default(Generated);
			result.TileGrid = grid;
			result.SpriteOverlay = overlay;
			return result;
		}

		private Tiles TileHandler(VirtualMap<char> mapData, int x, int y, Rectangle forceFill, char forceID, Behaviour behaviour)
		{
			char id = GetTile(mapData, x, y, forceFill, forceID, behaviour);
			if (IsEmpty(id))
			{
				return null;
			}
			TerrainType set = lookup[id];
			bool center = true;
			int index = 0;
			for (int ty = -1; ty < 2; ty++)
			{
				for (int tx = -1; tx < 2; tx++)
				{
					bool solid = CheckTile(set, mapData, x + tx, y + ty, forceFill, behaviour);
					if (!solid && behaviour.EdgesIgnoreOutOfLevel && !CheckForSameLevel(x, y, x + tx, y + ty))
					{
						solid = true;
					}
					adjacent[index++] = (byte)(solid ? 1u : 0u);
					if (!solid)
					{
						center = false;
					}
				}
			}
			if (center)
			{
				bool pad = false;
				if (behaviour.PaddingIgnoreOutOfLevel ? ((!CheckTile(set, mapData, x - 2, y, forceFill, behaviour) && CheckForSameLevel(x, y, x - 2, y)) || (!CheckTile(set, mapData, x + 2, y, forceFill, behaviour) && CheckForSameLevel(x, y, x + 2, y)) || (!CheckTile(set, mapData, x, y - 2, forceFill, behaviour) && CheckForSameLevel(x, y, x, y - 2)) || (!CheckTile(set, mapData, x, y + 2, forceFill, behaviour) && CheckForSameLevel(x, y, x, y + 2))) : (!CheckTile(set, mapData, x - 2, y, forceFill, behaviour) || !CheckTile(set, mapData, x + 2, y, forceFill, behaviour) || !CheckTile(set, mapData, x, y - 2, forceFill, behaviour) || !CheckTile(set, mapData, x, y + 2, forceFill, behaviour)))
				{
					return lookup[id].Padded;
				}
				return lookup[id].Center;
			}
			foreach (Masked subset in set.Masked)
			{
				bool matches = true;
				for (int i = 0; i < 9 && matches; i++)
				{
					if (subset.Mask[i] != 2 && subset.Mask[i] != adjacent[i])
					{
						matches = false;
					}
				}
				if (matches)
				{
					return subset.Tiles;
				}
			}
			return null;
		}

		private bool CheckForSameLevel(int x1, int y1, int x2, int y2)
		{
			foreach (Rectangle rect in LevelBounds)
			{
				if (rect.Contains(x1, y1) && rect.Contains(x2, y2))
				{
					return true;
				}
			}
			return false;
		}

		private bool CheckTile(TerrainType set, VirtualMap<char> mapData, int x, int y, Rectangle forceFill, Behaviour behaviour)
		{
			if (forceFill.Contains(x, y))
			{
				return true;
			}
			if (mapData == null)
			{
				return behaviour.EdgesExtend;
			}
			if (x < 0 || y < 0 || x >= mapData.Columns || y >= mapData.Rows)
			{
				if (!behaviour.EdgesExtend)
				{
					return false;
				}
				char c = mapData[Calc.Clamp(x, 0, mapData.Columns - 1), Calc.Clamp(y, 0, mapData.Rows - 1)];
				if (!IsEmpty(c))
				{
					return !set.Ignore(c);
				}
				return false;
			}
			char c2 = mapData[x, y];
			if (!IsEmpty(c2))
			{
				return !set.Ignore(c2);
			}
			return false;
		}

		private char GetTile(VirtualMap<char> mapData, int x, int y, Rectangle forceFill, char forceID, Behaviour behaviour)
		{
			if (forceFill.Contains(x, y))
			{
				return forceID;
			}
			if (mapData == null)
			{
				if (!behaviour.EdgesExtend)
				{
					return '0';
				}
				return forceID;
			}
			if (x < 0 || y < 0 || x >= mapData.Columns || y >= mapData.Rows)
			{
				if (!behaviour.EdgesExtend)
				{
					return '0';
				}
				int atX = Calc.Clamp(x, 0, mapData.Columns - 1);
				int atY = Calc.Clamp(y, 0, mapData.Rows - 1);
				return mapData[atX, atY];
			}
			return mapData[x, y];
		}

		private bool IsEmpty(char id)
		{
			if (id != '0')
			{
				return id == '\0';
			}
			return true;
		}
	}
}
