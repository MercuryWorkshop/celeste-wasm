using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace Celeste
{
	public class LevelData
	{
		public string Name;

		public bool Dummy;

		public int Strawberries;

		public bool HasGem;

		public bool HasHeartGem;

		public bool HasCheckpoint;

		public bool DisableDownTransition;

		public Rectangle Bounds;

		public List<EntityData> Entities;

		public List<EntityData> Triggers;

		public List<Vector2> Spawns;

		public List<DecalData> FgDecals;

		public List<DecalData> BgDecals;

		public string Solids = "";

		public string Bg = "";

		public string FgTiles = "";

		public string BgTiles = "";

		public string ObjTiles = "";

		public WindController.Patterns WindPattern;

		public Vector2 CameraOffset;

		public bool Dark;

		public bool Underwater;

		public bool Space;

		public string Music = "";

		public string AltMusic = "";

		public string Ambience = "";

		public float[] MusicLayers = new float[4];

		public int MusicProgress = -1;

		public int AmbienceProgress = -1;

		public bool MusicWhispers;

		public bool DelayAltMusic;

		public int EnforceDashNumber;

		public int EditorColorIndex;

		public Rectangle TileBounds => new Rectangle(Bounds.X / 8, Bounds.Y / 8, (int)Math.Ceiling((float)Bounds.Width / 8f), (int)Math.Ceiling((float)Bounds.Height / 8f));

		public Vector2 Position
		{
			get
			{
				return new Vector2(Bounds.X, Bounds.Y);
			}
			set
			{
				for (int i = 0; i < Spawns.Count; i++)
				{
					Spawns[i] -= Position;
				}
				Bounds.X = (int)value.X;
				Bounds.Y = (int)value.Y;
				for (int j = 0; j < Spawns.Count; j++)
				{
					Spawns[j] += Position;
				}
			}
		}

		public int LoadSeed
		{
			get
			{
				int seed = 0;
				string name = Name;
				foreach (char c in name)
				{
					seed += c;
				}
				return seed;
			}
		}

		public LevelData(BinaryPacker.Element data)
		{
			Bounds = default(Rectangle);
			foreach (KeyValuePair<string, object> attr in data.Attributes)
			{
				switch (attr.Key)
				{
				case "name":
					Name = attr.Value.ToString().Substring(4);
					break;
				case "x":
					Bounds.X = (int)attr.Value;
					break;
				case "y":
					Bounds.Y = (int)attr.Value;
					break;
				case "width":
					Bounds.Width = (int)attr.Value;
					break;
				case "height":
					Bounds.Height = (int)attr.Value;
					if (Bounds.Height == 184)
					{
						Bounds.Height = 180;
					}
					break;
				case "c":
					EditorColorIndex = (int)attr.Value;
					break;
				case "music":
					Music = (string)attr.Value;
					break;
				case "alt_music":
					AltMusic = (string)attr.Value;
					break;
				case "ambience":
					Ambience = (string)attr.Value;
					break;
				case "windPattern":
					WindPattern = (WindController.Patterns)Enum.Parse(typeof(WindController.Patterns), (string)attr.Value);
					break;
				case "dark":
					Dark = (bool)attr.Value;
					break;
				case "underwater":
					Underwater = (bool)attr.Value;
					break;
				case "space":
					Space = (bool)attr.Value;
					break;
				case "cameraOffsetX":
					CameraOffset.X = Convert.ToSingle(attr.Value, CultureInfo.InvariantCulture);
					break;
				case "cameraOffsetY":
					CameraOffset.Y = Convert.ToSingle(attr.Value, CultureInfo.InvariantCulture);
					break;
				case "musicLayer1":
					MusicLayers[0] = (((bool)attr.Value) ? 1f : 0f);
					break;
				case "musicLayer2":
					MusicLayers[1] = (((bool)attr.Value) ? 1f : 0f);
					break;
				case "musicLayer3":
					MusicLayers[2] = (((bool)attr.Value) ? 1f : 0f);
					break;
				case "musicLayer4":
					MusicLayers[3] = (((bool)attr.Value) ? 1f : 0f);
					break;
				case "musicProgress":
				{
					string mprogress = attr.Value.ToString();
					if (string.IsNullOrEmpty(mprogress) || !int.TryParse(mprogress, out MusicProgress))
					{
						MusicProgress = -1;
					}
					break;
				}
				case "ambienceProgress":
				{
					string aprogress = attr.Value.ToString();
					if (string.IsNullOrEmpty(aprogress) || !int.TryParse(aprogress, out AmbienceProgress))
					{
						AmbienceProgress = -1;
					}
					break;
				}
				case "whisper":
					MusicWhispers = (bool)attr.Value;
					break;
				case "disableDownTransition":
					DisableDownTransition = (bool)attr.Value;
					break;
				case "delayAltMusicFade":
					DelayAltMusic = (bool)attr.Value;
					break;
				case "enforceDashNumber":
					EnforceDashNumber = (int)attr.Value;
					break;
				}
			}
			Spawns = new List<Vector2>();
			Entities = new List<EntityData>();
			Triggers = new List<EntityData>();
			BgDecals = new List<DecalData>();
			FgDecals = new List<DecalData>();
			foreach (BinaryPacker.Element child in data.Children)
			{
				if (child.Name == "entities")
				{
					if (child.Children == null)
					{
						continue;
					}
					foreach (BinaryPacker.Element entity2 in child.Children)
					{
						if (entity2.Name == "player")
						{
							Spawns.Add(new Vector2((float)Bounds.X + Convert.ToSingle(entity2.Attributes["x"], CultureInfo.InvariantCulture), (float)Bounds.Y + Convert.ToSingle(entity2.Attributes["y"], CultureInfo.InvariantCulture)));
						}
						else if (entity2.Name == "strawberry" || entity2.Name == "snowberry")
						{
							Strawberries++;
						}
						else if (entity2.Name == "shard")
						{
							HasGem = true;
						}
						else if (entity2.Name == "blackGem")
						{
							HasHeartGem = true;
						}
						else if (entity2.Name == "checkpoint")
						{
							HasCheckpoint = true;
						}
						if (!entity2.Name.Equals("player"))
						{
							Entities.Add(CreateEntityData(entity2));
						}
					}
				}
				else if (child.Name == "triggers")
				{
					if (child.Children == null)
					{
						continue;
					}
					foreach (BinaryPacker.Element entity in child.Children)
					{
						Triggers.Add(CreateEntityData(entity));
					}
				}
				else if (child.Name == "bgdecals")
				{
					if (child.Children == null)
					{
						continue;
					}
					foreach (BinaryPacker.Element decal2 in child.Children)
					{
						BgDecals.Add(new DecalData
						{
							Position = new Vector2(Convert.ToSingle(decal2.Attributes["x"], CultureInfo.InvariantCulture), Convert.ToSingle(decal2.Attributes["y"], CultureInfo.InvariantCulture)),
							Scale = new Vector2(Convert.ToSingle(decal2.Attributes["scaleX"], CultureInfo.InvariantCulture), Convert.ToSingle(decal2.Attributes["scaleY"], CultureInfo.InvariantCulture)),
							Texture = (string)decal2.Attributes["texture"]
						});
					}
				}
				else if (child.Name == "fgdecals")
				{
					if (child.Children == null)
					{
						continue;
					}
					foreach (BinaryPacker.Element decal in child.Children)
					{
						FgDecals.Add(new DecalData
						{
							Position = new Vector2(Convert.ToSingle(decal.Attributes["x"], CultureInfo.InvariantCulture), Convert.ToSingle(decal.Attributes["y"], CultureInfo.InvariantCulture)),
							Scale = new Vector2(Convert.ToSingle(decal.Attributes["scaleX"], CultureInfo.InvariantCulture), Convert.ToSingle(decal.Attributes["scaleY"], CultureInfo.InvariantCulture)),
							Texture = (string)decal.Attributes["texture"]
						});
					}
				}
				else if (child.Name == "solids")
				{
					Solids = child.Attr("innerText");
				}
				else if (child.Name == "bg")
				{
					Bg = child.Attr("innerText");
				}
				else if (child.Name == "fgtiles")
				{
					FgTiles = child.Attr("innerText");
				}
				else if (child.Name == "bgtiles")
				{
					BgTiles = child.Attr("innerText");
				}
				else if (child.Name == "objtiles")
				{
					ObjTiles = child.Attr("innerText");
				}
			}
			Dummy = Spawns.Count <= 0;
		}

		private EntityData CreateEntityData(BinaryPacker.Element entity)
		{
			EntityData data = new EntityData();
			data.Name = entity.Name;
			data.Level = this;
			if (entity.Attributes != null)
			{
				foreach (KeyValuePair<string, object> attr2 in entity.Attributes)
				{
					if (attr2.Key == "id")
					{
						data.ID = (int)attr2.Value;
						continue;
					}
					if (attr2.Key == "x")
					{
						data.Position.X = Convert.ToSingle(attr2.Value, CultureInfo.InvariantCulture);
						continue;
					}
					if (attr2.Key == "y")
					{
						data.Position.Y = Convert.ToSingle(attr2.Value, CultureInfo.InvariantCulture);
						continue;
					}
					if (attr2.Key == "width")
					{
						data.Width = (int)attr2.Value;
						continue;
					}
					if (attr2.Key == "height")
					{
						data.Height = (int)attr2.Value;
						continue;
					}
					if (attr2.Key == "originX")
					{
						data.Origin.X = Convert.ToSingle(attr2.Value, CultureInfo.InvariantCulture);
						continue;
					}
					if (attr2.Key == "originY")
					{
						data.Origin.Y = Convert.ToSingle(attr2.Value, CultureInfo.InvariantCulture);
						continue;
					}
					if (data.Values == null)
					{
						data.Values = new Dictionary<string, object>();
					}
					data.Values.Add(attr2.Key, attr2.Value);
				}
			}
			data.Nodes = new Vector2[(entity.Children != null) ? entity.Children.Count : 0];
			for (int i = 0; i < data.Nodes.Length; i++)
			{
				foreach (KeyValuePair<string, object> attr in entity.Children[i].Attributes)
				{
					if (attr.Key == "x")
					{
						data.Nodes[i].X = Convert.ToSingle(attr.Value, CultureInfo.InvariantCulture);
					}
					else if (attr.Key == "y")
					{
						data.Nodes[i].Y = Convert.ToSingle(attr.Value, CultureInfo.InvariantCulture);
					}
				}
			}
			return data;
		}

		public bool Check(Vector2 at)
		{
			if (at.X >= (float)Bounds.Left && at.Y >= (float)Bounds.Top && at.X < (float)Bounds.Right)
			{
				return at.Y < (float)Bounds.Bottom;
			}
			return false;
		}
	}
}
