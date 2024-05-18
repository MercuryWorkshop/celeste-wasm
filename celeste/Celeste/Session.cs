using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Serializable]
	public class Session
	{
		[Serializable]
		public class Counter
		{
			[XmlAttribute("key")]
			public string Key;

			[XmlAttribute("value")]
			public int Value;
		}

		public enum CoreModes
		{
			None,
			Hot,
			Cold
		}

		public AreaKey Area;

		public Vector2? RespawnPoint;

		public AudioState Audio = new AudioState();

		public PlayerInventory Inventory;

		public HashSet<string> Flags = new HashSet<string>();

		public HashSet<string> LevelFlags = new HashSet<string>();

		public HashSet<EntityID> Strawberries = new HashSet<EntityID>();

		public HashSet<EntityID> DoNotLoad = new HashSet<EntityID>();

		public HashSet<EntityID> Keys = new HashSet<EntityID>();

		public List<Counter> Counters = new List<Counter>();

		public bool[] SummitGems = new bool[6];

		public AreaStats OldStats;

		public bool UnlockedCSide;

		public string FurthestSeenLevel;

		public bool BeatBestTime;

		[XmlAttribute]
		public string Level;

		[XmlAttribute]
		public long Time;

		[XmlAttribute]
		public bool StartedFromBeginning;

		[XmlAttribute]
		public int Deaths;

		[XmlAttribute]
		public int Dashes;

		[XmlAttribute]
		public int DashesAtLevelStart;

		[XmlAttribute]
		public int DeathsInCurrentLevel;

		[XmlAttribute]
		public bool InArea;

		[XmlAttribute]
		public string StartCheckpoint;

		[XmlAttribute]
		public bool FirstLevel = true;

		[XmlAttribute]
		public bool Cassette;

		[XmlAttribute]
		public bool HeartGem;

		[XmlAttribute]
		public bool Dreaming;

		[XmlAttribute]
		public string ColorGrade;

		[XmlAttribute]
		public float LightingAlphaAdd;

		[XmlAttribute]
		public float BloomBaseAdd;

		[XmlAttribute]
		public float DarkRoomAlpha = 0.75f;

		[XmlAttribute]
		public CoreModes CoreMode;

		[XmlAttribute]
		public bool GrabbedGolden;

		[XmlAttribute]
		public bool HitCheckpoint;

		[NonSerialized]
		[XmlIgnore]
		public bool JustStarted;

		public MapData MapData => AreaData.Areas[Area.ID].Mode[(int)Area.Mode].MapData;

		public LevelData LevelData => MapData.Get(Level);

		public bool FullClear
		{
			get
			{
				if (Area.Mode == AreaMode.Normal && Cassette && HeartGem && Strawberries.Count >= MapData.DetectedStrawberries)
				{
					if (Area.ID == 7)
					{
						return HasAllSummitGems;
					}
					return true;
				}
				return false;
			}
		}

		public bool ShouldAdvance
		{
			get
			{
				if (Area.Mode == AreaMode.Normal && !OldStats.Modes[0].Completed)
				{
					return Area.ID < SaveData.Instance.MaxArea;
				}
				return false;
			}
		}

		public bool HasAllSummitGems
		{
			get
			{
				for (int i = 0; i < SummitGems.Length; i++)
				{
					if (!SummitGems[i])
					{
						return false;
					}
				}
				return true;
			}
		}

		private Session()
		{
			JustStarted = true;
			InArea = true;
		}

		public Session(AreaKey area, string checkpoint = null, AreaStats oldStats = null)
			: this()
		{
			Area = area;
			StartCheckpoint = checkpoint;
			ColorGrade = MapData.Data.ColorGrade;
			Dreaming = AreaData.Areas[area.ID].Dreaming;
			Inventory = AreaData.GetCheckpointInventory(area, checkpoint);
			CoreMode = AreaData.Areas[area.ID].CoreMode;
			FirstLevel = true;
			Audio = MapData.ModeData.AudioState.Clone();
			if (StartCheckpoint == null)
			{
				Level = MapData.StartLevel().Name;
				StartedFromBeginning = true;
			}
			else
			{
				Level = StartCheckpoint;
				StartedFromBeginning = false;
				Dreaming = AreaData.GetCheckpointDreaming(area, checkpoint);
				CoreMode = AreaData.GetCheckpointCoreMode(area, checkpoint);
				AudioState audioState = AreaData.GetCheckpointAudioState(area, checkpoint);
				if (audioState != null)
				{
					if (audioState.Music != null)
					{
						Audio.Music = audioState.Music.Clone();
					}
					if (audioState.Ambience != null)
					{
						Audio.Ambience = audioState.Ambience.Clone();
					}
				}
				string colorGrading = AreaData.GetCheckpointColorGrading(area, checkpoint);
				if (colorGrading != null)
				{
					ColorGrade = colorGrading;
				}
				CheckpointData cp = AreaData.GetCheckpoint(area, checkpoint);
				if (cp != null && cp.Flags != null)
				{
					foreach (string flag in cp.Flags)
					{
						SetFlag(flag);
					}
				}
			}
			if (oldStats != null)
			{
				OldStats = oldStats;
			}
			else
			{
				OldStats = SaveData.Instance.Areas[Area.ID].Clone();
			}
		}

		public Session Restart(string intoLevel = null)
		{
			Session newSession = new Session(Area, StartCheckpoint, OldStats)
			{
				UnlockedCSide = UnlockedCSide
			};
			if (intoLevel != null)
			{
				newSession.Level = intoLevel;
				if (intoLevel != MapData.StartLevel().Name)
				{
					newSession.StartedFromBeginning = false;
				}
			}
			return newSession;
		}

		public void UpdateLevelStartDashes()
		{
			DashesAtLevelStart = Dashes;
		}

		public Vector2 GetSpawnPoint(Vector2 from)
		{
			return LevelData.Spawns.ClosestTo(from);
		}

		public bool GetFlag(string flag)
		{
			return Flags.Contains(flag);
		}

		public void SetFlag(string flag, bool setTo = true)
		{
			if (setTo)
			{
				Flags.Add(flag);
			}
			else
			{
				Flags.Remove(flag);
			}
		}

		public int GetCounter(string counter)
		{
			for (int i = 0; i < Counters.Count; i++)
			{
				if (Counters[i].Key.Equals(counter))
				{
					return Counters[i].Value;
				}
			}
			return 0;
		}

		public void SetCounter(string counter, int value)
		{
			for (int i = 0; i < Counters.Count; i++)
			{
				if (Counters[i].Key.Equals(counter))
				{
					Counters[i].Value = value;
					return;
				}
			}
			Counters.Add(new Counter
			{
				Key = counter,
				Value = value
			});
		}

		public void IncrementCounter(string counter)
		{
			for (int i = 0; i < Counters.Count; i++)
			{
				if (Counters[i].Key.Equals(counter))
				{
					Counters[i].Value++;
					return;
				}
			}
			Counters.Add(new Counter
			{
				Key = counter,
				Value = 1
			});
		}

		public bool GetLevelFlag(string level)
		{
			return LevelFlags.Contains(level);
		}
	}
}
