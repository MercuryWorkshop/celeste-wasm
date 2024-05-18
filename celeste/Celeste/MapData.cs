using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class MapData
	{
		public AreaKey Area;

		public AreaData Data;

		public ModeProperties ModeData;

		public int DetectedStrawberries;

		public bool DetectedRemixNotes;

		public bool DetectedHeartGem;

		public List<LevelData> Levels = new List<LevelData>();

		public List<Rectangle> Filler = new List<Rectangle>();

		public List<EntityData> Strawberries = new List<EntityData>();

		public List<EntityData> Goldenberries = new List<EntityData>();

		public Color BackgroundColor = Color.Black;

		public BinaryPacker.Element Foreground;

		public BinaryPacker.Element Background;

		public Rectangle Bounds;

		public string Filename => Data.Mode[(int)Area.Mode].Path;

		public string Filepath => Path.Combine(Engine.ContentDirectory, "Maps", Filename + ".bin");

		public Rectangle TileBounds => new Rectangle(Bounds.X / 8, Bounds.Y / 8, (int)Math.Ceiling((float)Bounds.Width / 8f), (int)Math.Ceiling((float)Bounds.Height / 8f));

		public int LoadSeed
		{
			get
			{
				int seed = 0;
				string name = Data.Name;
				foreach (char c in name)
				{
					seed += c;
				}
				return seed;
			}
		}

		public int LevelCount
		{
			get
			{
				int count = 0;
				foreach (LevelData level in Levels)
				{
					if (!level.Dummy)
					{
						count++;
					}
				}
				return count;
			}
		}

		public MapData(AreaKey area)
		{
			Area = area;
			Data = AreaData.Areas[Area.ID];
			ModeData = Data.Mode[(int)Area.Mode];
			Load();
		}

		public LevelData GetTransitionTarget(Level level, Vector2 position)
		{
			return GetAt(position);
		}

		public bool CanTransitionTo(Level level, Vector2 position)
		{
			LevelData data = GetTransitionTarget(level, position);
			if (data != null)
			{
				return !data.Dummy;
			}
			return false;
		}

		public void Reload()
		{
			Load();
		}

		private void Load()
		{
			if (!File.Exists(Filepath))
			{
				return;
			}
			Strawberries = new List<EntityData>();
			BinaryPacker.Element element = BinaryPacker.FromBinary(Filepath);
			// if (!element.Package.Equals(ModeData.Path))
			// {
			// 	throw new Exception("Corrupted Level Data");
			// }
			foreach (BinaryPacker.Element child in element.Children)
			{
				if (child.Name == "levels")
				{
					Levels = new List<LevelData>();
					foreach (BinaryPacker.Element child2 in child.Children)
					{
						LevelData level2 = new LevelData(child2);
						DetectedStrawberries += level2.Strawberries;
						if (level2.HasGem)
						{
							DetectedRemixNotes = true;
						}
						if (level2.HasHeartGem)
						{
							DetectedHeartGem = true;
						}
						Levels.Add(level2);
					}
				}
				else if (child.Name == "Filler")
				{
					Filler = new List<Rectangle>();
					if (child.Children == null)
					{
						continue;
					}
					foreach (BinaryPacker.Element filler in child.Children)
					{
						Filler.Add(new Rectangle((int)filler.Attributes["x"], (int)filler.Attributes["y"], (int)filler.Attributes["w"], (int)filler.Attributes["h"]));
					}
				}
				else
				{
					if (!(child.Name == "Style"))
					{
						continue;
					}
					if (child.HasAttr("color"))
					{
						BackgroundColor = Calc.HexToColor(child.Attr("color"));
					}
					if (child.Children == null)
					{
						continue;
					}
					foreach (BinaryPacker.Element layer in child.Children)
					{
						if (layer.Name == "Backgrounds")
						{
							Background = layer;
						}
						else if (layer.Name == "Foregrounds")
						{
							Foreground = layer;
						}
					}
				}
			}
			foreach (LevelData level3 in Levels)
			{
				foreach (EntityData entity in level3.Entities)
				{
					if (entity.Name == "strawberry")
					{
						Strawberries.Add(entity);
					}
					else if (entity.Name == "goldenBerry")
					{
						Goldenberries.Add(entity);
					}
				}
			}
			int left = int.MaxValue;
			int top = int.MaxValue;
			int right = int.MinValue;
			int bottom = int.MinValue;
			foreach (LevelData level in Levels)
			{
				if (level.Bounds.Left < left)
				{
					left = level.Bounds.Left;
				}
				if (level.Bounds.Top < top)
				{
					top = level.Bounds.Top;
				}
				if (level.Bounds.Right > right)
				{
					right = level.Bounds.Right;
				}
				if (level.Bounds.Bottom > bottom)
				{
					bottom = level.Bounds.Bottom;
				}
			}
			foreach (Rectangle fill in Filler)
			{
				if (fill.Left < left)
				{
					left = fill.Left;
				}
				if (fill.Top < top)
				{
					top = fill.Top;
				}
				if (fill.Right > right)
				{
					right = fill.Right;
				}
				if (fill.Bottom > bottom)
				{
					bottom = fill.Bottom;
				}
			}
			int padding = 64;
			Bounds = new Rectangle(left - padding, top - padding, right - left + padding * 2, bottom - top + padding * 2);
			ModeData.TotalStrawberries = 0;
			ModeData.StartStrawberries = 0;
			ModeData.StrawberriesByCheckpoint = new EntityData[10, 25];
			int i = 0;
			while (ModeData.Checkpoints != null && i < ModeData.Checkpoints.Length)
			{
				if (ModeData.Checkpoints[i] != null)
				{
					ModeData.Checkpoints[i].Strawberries = 0;
				}
				i++;
			}
			foreach (EntityData strawb in Strawberries)
			{
				if (!strawb.Bool("moon"))
				{
					int checkpoint = strawb.Int("checkpointID");
					int order = strawb.Int("order");
					if (ModeData.StrawberriesByCheckpoint[checkpoint, order] == null)
					{
						ModeData.StrawberriesByCheckpoint[checkpoint, order] = strawb;
					}
					if (checkpoint == 0)
					{
						ModeData.StartStrawberries++;
					}
					else if (ModeData.Checkpoints != null)
					{
						ModeData.Checkpoints[checkpoint - 1].Strawberries++;
					}
					ModeData.TotalStrawberries++;
				}
			}
		}

		public int[] GetStrawberries(out int total)
		{
			total = 0;
			int[] berries = new int[10];
			foreach (LevelData level in Levels)
			{
				foreach (EntityData entity in level.Entities)
				{
					if (entity.Name == "strawberry")
					{
						total++;
						berries[entity.Int("checkpointID")]++;
					}
				}
			}
			return berries;
		}

		public LevelData StartLevel()
		{
			return GetAt(Vector2.Zero);
		}

		public LevelData GetAt(Vector2 at)
		{
			foreach (LevelData level in Levels)
			{
				if (level.Check(at))
				{
					return level;
				}
			}
			return null;
		}

		public LevelData Get(string levelName)
		{
			foreach (LevelData level in Levels)
			{
				if (level.Name.Equals(levelName))
				{
					return level;
				}
			}
			return null;
		}

		public List<Backdrop> CreateBackdrops(BinaryPacker.Element data)
		{
			List<Backdrop> backdrops = new List<Backdrop>();
			if (data != null && data.Children != null)
			{
				foreach (BinaryPacker.Element element in data.Children)
				{
					if (element.Name.Equals("apply", StringComparison.OrdinalIgnoreCase))
					{
						if (element.Children != null)
						{
							foreach (BinaryPacker.Element subnode in element.Children)
							{
								backdrops.Add(ParseBackdrop(subnode, element));
							}
						}
					}
					else
					{
						backdrops.Add(ParseBackdrop(element, null));
					}
				}
				return backdrops;
			}
			return backdrops;
		}

		private Backdrop ParseBackdrop(BinaryPacker.Element child, BinaryPacker.Element above)
		{
			Backdrop backdrop = null;
			if (child.Name.Equals("parallax", StringComparison.OrdinalIgnoreCase))
			{
				string textureName = child.Attr("texture");
				string atlasName = child.Attr("atlas", "game");
				MTexture texture = null;
				texture = ((atlasName == "game" && GFX.Game.Has(textureName)) ? GFX.Game[textureName] : ((!(atlasName == "gui") || !GFX.Gui.Has(textureName)) ? GFX.Misc[textureName] : GFX.Gui[textureName]));
				Parallax parallax = new Parallax(texture);
				backdrop = parallax;
				string blend = "";
				if (child.HasAttr("blendmode"))
				{
					blend = child.Attr("blendmode", "alphablend").ToLower();
				}
				else if (above != null && above.HasAttr("blendmode"))
				{
					blend = above.Attr("blendmode", "alphablend").ToLower();
				}
				if (blend.Equals("additive"))
				{
					parallax.BlendState = BlendState.Additive;
				}
				parallax.DoFadeIn = bool.Parse(child.Attr("fadeIn", "false"));
			}
			else if (child.Name.Equals("snowfg", StringComparison.OrdinalIgnoreCase))
			{
				backdrop = new Snow(foreground: true);
			}
			else if (child.Name.Equals("snowbg", StringComparison.OrdinalIgnoreCase))
			{
				backdrop = new Snow(foreground: false);
			}
			else if (child.Name.Equals("windsnow", StringComparison.OrdinalIgnoreCase))
			{
				backdrop = new WindSnowFG();
			}
			else if (child.Name.Equals("dreamstars", StringComparison.OrdinalIgnoreCase))
			{
				backdrop = new DreamStars();
			}
			else if (child.Name.Equals("stars", StringComparison.OrdinalIgnoreCase))
			{
				backdrop = new StarsBG();
			}
			else if (child.Name.Equals("mirrorfg", StringComparison.OrdinalIgnoreCase))
			{
				backdrop = new MirrorFG();
			}
			else if (child.Name.Equals("reflectionfg", StringComparison.OrdinalIgnoreCase))
			{
				backdrop = new ReflectionFG();
			}
			else if (child.Name.Equals("godrays", StringComparison.OrdinalIgnoreCase))
			{
				backdrop = new Godrays();
			}
			else if (child.Name.Equals("tentacles", StringComparison.OrdinalIgnoreCase))
			{
				backdrop = new Tentacles((Tentacles.Side)Enum.Parse(typeof(Tentacles.Side), child.Attr("side", "Right")), Calc.HexToColor(child.Attr("color")), child.AttrFloat("offset"));
			}
			else if (child.Name.Equals("northernlights", StringComparison.OrdinalIgnoreCase))
			{
				backdrop = new NorthernLights();
			}
			else if (child.Name.Equals("bossStarField", StringComparison.OrdinalIgnoreCase))
			{
				backdrop = new FinalBossStarfield();
			}
			else if (child.Name.Equals("petals", StringComparison.OrdinalIgnoreCase))
			{
				backdrop = new Petals();
			}
			else if (child.Name.Equals("heatwave", StringComparison.OrdinalIgnoreCase))
			{
				backdrop = new HeatWave();
			}
			else if (child.Name.Equals("corestarsfg", StringComparison.OrdinalIgnoreCase))
			{
				backdrop = new CoreStarsFG();
			}
			else if (child.Name.Equals("starfield", StringComparison.OrdinalIgnoreCase))
			{
				backdrop = new Starfield(Calc.HexToColor(child.Attr("color")), child.AttrFloat("speed", 1f));
			}
			else if (child.Name.Equals("planets", StringComparison.OrdinalIgnoreCase))
			{
				backdrop = new Planets((int)child.AttrFloat("count", 32f), child.Attr("size", "small"));
			}
			else if (child.Name.Equals("rain", StringComparison.OrdinalIgnoreCase))
			{
				backdrop = new RainFG();
			}
			else if (child.Name.Equals("stardust", StringComparison.OrdinalIgnoreCase))
			{
				backdrop = new StardustFG();
			}
			else
			{
				if (!child.Name.Equals("blackhole", StringComparison.OrdinalIgnoreCase))
				{
					throw new Exception("Background type " + child.Name + " does not exist");
				}
				backdrop = new BlackholeBG();
			}
			if (child.HasAttr("tag"))
			{
				backdrop.Tags.Add(child.Attr("tag"));
			}
			if (above != null && above.HasAttr("tag"))
			{
				backdrop.Tags.Add(above.Attr("tag"));
			}
			if (child.HasAttr("x"))
			{
				backdrop.Position.X = child.AttrFloat("x");
			}
			else if (above != null && above.HasAttr("x"))
			{
				backdrop.Position.X = above.AttrFloat("x");
			}
			if (child.HasAttr("y"))
			{
				backdrop.Position.Y = child.AttrFloat("y");
			}
			else if (above != null && above.HasAttr("y"))
			{
				backdrop.Position.Y = above.AttrFloat("y");
			}
			if (child.HasAttr("scrollx"))
			{
				backdrop.Scroll.X = child.AttrFloat("scrollx");
			}
			else if (above != null && above.HasAttr("scrollx"))
			{
				backdrop.Scroll.X = above.AttrFloat("scrollx");
			}
			if (child.HasAttr("scrolly"))
			{
				backdrop.Scroll.Y = child.AttrFloat("scrolly");
			}
			else if (above != null && above.HasAttr("scrolly"))
			{
				backdrop.Scroll.Y = above.AttrFloat("scrolly");
			}
			if (child.HasAttr("speedx"))
			{
				backdrop.Speed.X = child.AttrFloat("speedx");
			}
			else if (above != null && above.HasAttr("speedx"))
			{
				backdrop.Speed.X = above.AttrFloat("speedx");
			}
			if (child.HasAttr("speedy"))
			{
				backdrop.Speed.Y = child.AttrFloat("speedy");
			}
			else if (above != null && above.HasAttr("speedy"))
			{
				backdrop.Speed.Y = above.AttrFloat("speedy");
			}
			backdrop.Color = Color.White;
			if (child.HasAttr("color"))
			{
				backdrop.Color = Calc.HexToColor(child.Attr("color"));
			}
			else if (above != null && above.HasAttr("color"))
			{
				backdrop.Color = Calc.HexToColor(above.Attr("color"));
			}
			if (child.HasAttr("alpha"))
			{
				backdrop.Color *= child.AttrFloat("alpha");
			}
			else if (above != null && above.HasAttr("alpha"))
			{
				backdrop.Color *= above.AttrFloat("alpha");
			}
			if (child.HasAttr("flipx"))
			{
				backdrop.FlipX = child.AttrBool("flipx");
			}
			else if (above != null && above.HasAttr("flipx"))
			{
				backdrop.FlipX = above.AttrBool("flipx");
			}
			if (child.HasAttr("flipy"))
			{
				backdrop.FlipY = child.AttrBool("flipy");
			}
			else if (above != null && above.HasAttr("flipy"))
			{
				backdrop.FlipY = above.AttrBool("flipy");
			}
			if (child.HasAttr("loopx"))
			{
				backdrop.LoopX = child.AttrBool("loopx");
			}
			else if (above != null && above.HasAttr("loopx"))
			{
				backdrop.LoopX = above.AttrBool("loopx");
			}
			if (child.HasAttr("loopy"))
			{
				backdrop.LoopY = child.AttrBool("loopy");
			}
			else if (above != null && above.HasAttr("loopy"))
			{
				backdrop.LoopY = above.AttrBool("loopy");
			}
			if (child.HasAttr("wind"))
			{
				backdrop.WindMultiplier = child.AttrFloat("wind");
			}
			else if (above != null && above.HasAttr("wind"))
			{
				backdrop.WindMultiplier = above.AttrFloat("wind");
			}
			string exclude = null;
			if (child.HasAttr("exclude"))
			{
				exclude = child.Attr("exclude");
			}
			else if (above != null && above.HasAttr("exclude"))
			{
				exclude = above.Attr("exclude");
			}
			if (exclude != null)
			{
				backdrop.ExcludeFrom = ParseLevelsList(exclude);
			}
			string only = null;
			if (child.HasAttr("only"))
			{
				only = child.Attr("only");
			}
			else if (above != null && above.HasAttr("only"))
			{
				only = above.Attr("only");
			}
			if (only != null)
			{
				backdrop.OnlyIn = ParseLevelsList(only);
			}
			string flag3 = null;
			if (child.HasAttr("flag"))
			{
				flag3 = child.Attr("flag");
			}
			else if (above != null && above.HasAttr("flag"))
			{
				flag3 = above.Attr("flag");
			}
			if (flag3 != null)
			{
				backdrop.OnlyIfFlag = flag3;
			}
			string flag2 = null;
			if (child.HasAttr("notflag"))
			{
				flag2 = child.Attr("notflag");
			}
			else if (above != null && above.HasAttr("notflag"))
			{
				flag2 = above.Attr("notflag");
			}
			if (flag2 != null)
			{
				backdrop.OnlyIfNotFlag = flag2;
			}
			string flag = null;
			if (child.HasAttr("always"))
			{
				flag = child.Attr("always");
			}
			else if (above != null && above.HasAttr("always"))
			{
				flag = above.Attr("always");
			}
			if (flag != null)
			{
				backdrop.AlsoIfFlag = flag;
			}
			bool? dreaming = null;
			if (child.HasAttr("dreaming"))
			{
				dreaming = child.AttrBool("dreaming");
			}
			else if (above != null && above.HasAttr("dreaming"))
			{
				dreaming = above.AttrBool("dreaming");
			}
			if (dreaming.HasValue)
			{
				backdrop.Dreaming = dreaming;
			}
			if (child.HasAttr("instantIn"))
			{
				backdrop.InstantIn = child.AttrBool("instantIn");
			}
			else if (above != null && above.HasAttr("instantIn"))
			{
				backdrop.InstantIn = above.AttrBool("instantIn");
			}
			if (child.HasAttr("instantOut"))
			{
				backdrop.InstantOut = child.AttrBool("instantOut");
			}
			else if (above != null && above.HasAttr("instantOut"))
			{
				backdrop.InstantOut = above.AttrBool("instantOut");
			}
			string fadeValue2 = null;
			if (child.HasAttr("fadex"))
			{
				fadeValue2 = child.Attr("fadex");
			}
			else if (above != null && above.HasAttr("fadex"))
			{
				fadeValue2 = above.Attr("fadex");
			}
			if (fadeValue2 != null)
			{
				backdrop.FadeX = new Backdrop.Fader();
				string[] array = fadeValue2.Split(':');
				for (int i = 0; i < array.Length; i++)
				{
					string[] values2 = array[i].Split(',');
					if (values2.Length == 2)
					{
						string[] pos2 = values2[0].Split('-');
						string[] array2 = values2[1].Split('-');
						float valFrom2 = float.Parse(array2[0], CultureInfo.InvariantCulture);
						float valTo2 = float.Parse(array2[1], CultureInfo.InvariantCulture);
						int signPos = 1;
						int signPos3 = 1;
						if (pos2[0][0] == 'n')
						{
							signPos = -1;
							pos2[0] = pos2[0].Substring(1);
						}
						if (pos2[1][0] == 'n')
						{
							signPos3 = -1;
							pos2[1] = pos2[1].Substring(1);
						}
						backdrop.FadeX.Add(signPos * int.Parse(pos2[0]), signPos3 * int.Parse(pos2[1]), valFrom2, valTo2);
					}
				}
			}
			string fadeValue = null;
			if (child.HasAttr("fadey"))
			{
				fadeValue = child.Attr("fadey");
			}
			else if (above != null && above.HasAttr("fadey"))
			{
				fadeValue = above.Attr("fadey");
			}
			if (fadeValue != null)
			{
				backdrop.FadeY = new Backdrop.Fader();
				string[] array = fadeValue.Split(':');
				for (int i = 0; i < array.Length; i++)
				{
					string[] values = array[i].Split(',');
					if (values.Length == 2)
					{
						string[] pos = values[0].Split('-');
						string[] array3 = values[1].Split('-');
						float valFrom = float.Parse(array3[0], CultureInfo.InvariantCulture);
						float valTo = float.Parse(array3[1], CultureInfo.InvariantCulture);
						int signPos0 = 1;
						int signPos2 = 1;
						if (pos[0][0] == 'n')
						{
							signPos0 = -1;
							pos[0] = pos[0].Substring(1);
						}
						if (pos[1][0] == 'n')
						{
							signPos2 = -1;
							pos[1] = pos[1].Substring(1);
						}
						backdrop.FadeY.Add(signPos0 * int.Parse(pos[0]), signPos2 * int.Parse(pos[1]), valFrom, valTo);
					}
				}
			}
			return backdrop;
		}

		private HashSet<string> ParseLevelsList(string list)
		{
			HashSet<string> result = new HashSet<string>();
			string[] array = list.Split(',');
			foreach (string name in array)
			{
				if (name.Contains('*'))
				{
					string search = "^" + Regex.Escape(name).Replace("\\*", ".*") + "$";
					foreach (LevelData level in Levels)
					{
						if (Regex.IsMatch(level.Name, search))
						{
							result.Add(level.Name);
						}
					}
				}
				else
				{
					result.Add(name);
				}
			}
			return result;
		}
	}
}
