using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Celeste
{
	[Serializable]
	public class SaveData
	{
		public const int MaxStrawberries = 175;

		public const int MaxGoldenStrawberries = 25;

		public const int MaxStrawberriesDLC = 202;

		public const int MaxHeartGems = 24;

		public const int MaxCassettes = 8;

		public const int MaxCompletions = 8;

		public static SaveData Instance;

		public string Version;

		public string Name = "Madeline";

		public long Time;

		public DateTime LastSave;

		public bool CheatMode;

		public bool AssistMode;

		public bool VariantMode;

		public Assists Assists = Assists.Default;

		public string TheoSisterName;

		public int UnlockedAreas;

		public int TotalDeaths;

		public int TotalStrawberries;

		public int TotalGoldenStrawberries;

		public int TotalJumps;

		public int TotalWallJumps;

		public int TotalDashes;

		public HashSet<string> Flags = new HashSet<string>();

		public List<string> Poem = new List<string>();

		public bool[] SummitGems;

		public bool RevealedChapter9;

		public AreaKey LastArea;

		public Session CurrentSession;

		public List<AreaStats> Areas = new List<AreaStats>();

		[NonSerialized]
		[XmlIgnore]
		public int FileSlot;

		[NonSerialized]
		[XmlIgnore]
		public bool DoNotSave;

		[NonSerialized]
		[XmlIgnore]
		public bool DebugMode;

		public int UnlockedModes
		{
			get
			{
				if (DebugMode || CheatMode)
				{
					return 3;
				}
				if (TotalHeartGems >= 16)
				{
					return 3;
				}
				for (int i = 1; i <= MaxArea; i++)
				{
					if (Areas[i].Cassette)
					{
						return 2;
					}
				}
				return 1;
			}
		}

		public int MaxArea
		{
			get
			{
				if (Celeste.PlayMode == Celeste.PlayModes.Event)
				{
					return 2;
				}
				return AreaData.Areas.Count - 1;
			}
		}

		public int MaxAssistArea => AreaData.Areas.Count - 1;

		public int TotalHeartGems
		{
			get
			{
				int count = 0;
				foreach (AreaStats area in Areas)
				{
					for (int i = 0; i < area.Modes.Length; i++)
					{
						if (area.Modes[i] != null && area.Modes[i].HeartGem)
						{
							count++;
						}
					}
				}
				return count;
			}
		}

		public int TotalCassettes
		{
			get
			{
				int count = 0;
				for (int i = 0; i <= MaxArea; i++)
				{
					if (!AreaData.Get(i).Interlude && Areas[i].Cassette)
					{
						count++;
					}
				}
				return count;
			}
		}

		public int TotalCompletions
		{
			get
			{
				int count = 0;
				for (int i = 0; i <= MaxArea; i++)
				{
					if (!AreaData.Get(i).Interlude && Areas[i].Modes[0].Completed)
					{
						count++;
					}
				}
				return count;
			}
		}

		public bool HasAllFullClears
		{
			get
			{
				for (int i = 0; i <= MaxArea; i++)
				{
					if (AreaData.Get(i).CanFullClear && !Areas[i].Modes[0].FullClear)
					{
						return false;
					}
				}
				return true;
			}
		}

		public int CompletionPercent
		{
			get
			{
				float percent = 0f;
				percent = ((TotalHeartGems < 24) ? (percent + (float)TotalHeartGems / 24f * 24f) : (percent + 24f));
				percent = ((TotalStrawberries < 175) ? (percent + (float)TotalStrawberries / 175f * 55f) : (percent + 55f));
				percent = ((TotalCassettes < 8) ? (percent + (float)TotalCassettes / 8f * 7f) : (percent + 7f));
				percent = ((TotalCompletions < 8) ? (percent + (float)TotalCompletions / 8f * 14f) : (percent + 14f));
				if (percent < 0f)
				{
					percent = 0f;
				}
				else if (percent > 100f)
				{
					percent = 100f;
				}
				return (int)percent;
			}
		}

		public static void Start(SaveData data, int slot)
		{
			Instance = data;
			Instance.FileSlot = slot;
			Instance.AfterInitialize();
		}

		public static string GetFilename(int slot)
		{
			if (slot == 4)
			{
				return "debug";
			}
			return slot.ToString();
		}

		public static string GetFilename()
		{
			return GetFilename(Instance.FileSlot);
		}

		public static void InitializeDebugMode(bool loadExisting = true)
		{
			SaveData savedata = null;
			if (loadExisting && UserIO.Open(UserIO.Mode.Read))
			{
				savedata = UserIO.Load<SaveData>(GetFilename(4));
				UserIO.Close();
			}
			if (savedata == null)
			{
				savedata = new SaveData();
			}
			savedata.DebugMode = true;
			savedata.CurrentSession = null;
			Start(savedata, 4);
		}

		public static bool TryDelete(int slot)
		{
			return UserIO.Delete(GetFilename(slot));
		}

		public void AfterInitialize()
		{
			while (Areas.Count < AreaData.Areas.Count)
			{
				Areas.Add(new AreaStats(Areas.Count));
			}
			while (Areas.Count > AreaData.Areas.Count)
			{
				Areas.RemoveAt(Areas.Count - 1);
			}
			int furthest = -1;
			for (int j = 0; j < Areas.Count; j++)
			{
				if (Areas[j].Modes[0].Completed || (Areas[j].Modes.Length > 1 && Areas[j].Modes[1].Completed))
				{
					furthest = j;
				}
			}
			if (UnlockedAreas < furthest + 1 && MaxArea >= furthest + 1)
			{
				UnlockedAreas = furthest + 1;
			}
			if (DebugMode)
			{
				CurrentSession = null;
				RevealedChapter9 = true;
				UnlockedAreas = MaxArea;
			}
			if (CheatMode)
			{
				UnlockedAreas = MaxArea;
			}
			if (string.IsNullOrEmpty(TheoSisterName))
			{
				TheoSisterName = Dialog.Clean("THEO_SISTER_NAME");
				if (Name.IndexOf(TheoSisterName, StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					TheoSisterName = Dialog.Clean("THEO_SISTER_ALT_NAME");
				}
			}
			AssistModeChecks();
			foreach (AreaStats area in Areas)
			{
				area.CleanCheckpoints();
			}
			if (Version == null || !(new Version(Version) < new Version(1, 2, 1, 1)))
			{
				return;
			}
			for (int i = 0; i < Areas.Count; i++)
			{
				if (Areas[i] == null)
				{
					continue;
				}
				for (int k = 0; k < Areas[i].Modes.Length; k++)
				{
					if (Areas[i].Modes[k] != null)
					{
						if (Areas[i].Modes[k].BestTime > 0)
						{
							Areas[i].Modes[k].SingleRunCompleted = true;
						}
						Areas[i].Modes[k].BestTime = 0L;
						Areas[i].Modes[k].BestFullClearTime = 0L;
					}
				}
			}
		}

		public void AssistModeChecks()
		{
			if (!VariantMode && !AssistMode)
			{
				Assists = default(Assists);
			}
			else if (!VariantMode)
			{
				Assists.EnfornceAssistMode();
			}
			if (Assists.GameSpeed < 5 || Assists.GameSpeed > 20)
			{
				Assists.GameSpeed = 10;
			}
			Input.MoveX.Inverted = (Input.Aim.InvertedX = (Input.Feather.InvertedX = Assists.MirrorMode));
		}

		public static void NoFileAssistChecks()
		{
			Input.MoveX.Inverted = (Input.Aim.InvertedX = (Input.Feather.InvertedX = false));
		}

		public void BeforeSave()
		{
			Instance.Version = Celeste.Instance.Version.ToString();
		}

		public void StartSession(Session session)
		{
			LastArea = session.Area;
			CurrentSession = session;
			if (DebugMode)
			{
				AreaModeStats stats = Areas[session.Area.ID].Modes[(int)session.Area.Mode];
				AreaModeStats obj = session.OldStats.Modes[(int)session.Area.Mode];
				Instance.TotalStrawberries -= stats.TotalStrawberries;
				stats.Strawberries.Clear();
				stats.TotalStrawberries = 0;
				obj.Strawberries.Clear();
				obj.TotalStrawberries = 0;
			}
		}

		public void AddDeath(AreaKey area)
		{
			TotalDeaths++;
			Areas[area.ID].Modes[(int)area.Mode].Deaths++;
			Stats.Increment(Stat.DEATHS);
			StatsForStadia.Increment(StadiaStat.DEATHS);
		}

		public void AddStrawberry(AreaKey area, EntityID strawberry, bool golden)
		{
			AreaModeStats stats = Areas[area.ID].Modes[(int)area.Mode];
			if (!stats.Strawberries.Contains(strawberry))
			{
				stats.Strawberries.Add(strawberry);
				stats.TotalStrawberries++;
				TotalStrawberries++;
				if (golden)
				{
					TotalGoldenStrawberries++;
				}
				if (TotalStrawberries >= 30)
				{
					Achievements.Register(Achievement.STRB1);
				}
				if (TotalStrawberries >= 80)
				{
					Achievements.Register(Achievement.STRB2);
				}
				if (TotalStrawberries >= 175)
				{
					Achievements.Register(Achievement.STRB3);
				}
				StatsForStadia.SetIfLarger(StadiaStat.BERRIES, TotalStrawberries);
			}
			Stats.Increment(golden ? Stat.GOLDBERRIES : Stat.BERRIES);
		}

		public void AddStrawberry(EntityID strawberry, bool golden)
		{
			AddStrawberry(CurrentSession.Area, strawberry, golden);
		}

		public bool CheckStrawberry(AreaKey area, EntityID strawberry)
		{
			return Areas[area.ID].Modes[(int)area.Mode].Strawberries.Contains(strawberry);
		}

		public bool CheckStrawberry(EntityID strawberry)
		{
			return CheckStrawberry(CurrentSession.Area, strawberry);
		}

		public void AddTime(AreaKey area, long time)
		{
			Time += time;
			Areas[area.ID].Modes[(int)area.Mode].TimePlayed += time;
		}

		public void RegisterHeartGem(AreaKey area)
		{
			Areas[area.ID].Modes[(int)area.Mode].HeartGem = true;
			if (area.Mode == AreaMode.Normal)
			{
				if (area.ID == 1)
				{
					Achievements.Register(Achievement.HEART1);
				}
				else if (area.ID == 2)
				{
					Achievements.Register(Achievement.HEART2);
				}
				else if (area.ID == 3)
				{
					Achievements.Register(Achievement.HEART3);
				}
				else if (area.ID == 4)
				{
					Achievements.Register(Achievement.HEART4);
				}
				else if (area.ID == 5)
				{
					Achievements.Register(Achievement.HEART5);
				}
				else if (area.ID == 6)
				{
					Achievements.Register(Achievement.HEART6);
				}
				else if (area.ID == 7)
				{
					Achievements.Register(Achievement.HEART7);
				}
				else if (area.ID == 9)
				{
					Achievements.Register(Achievement.HEART8);
				}
			}
			else if (area.Mode == AreaMode.BSide)
			{
				if (area.ID == 1)
				{
					Achievements.Register(Achievement.BSIDE1);
				}
				else if (area.ID == 2)
				{
					Achievements.Register(Achievement.BSIDE2);
				}
				else if (area.ID == 3)
				{
					Achievements.Register(Achievement.BSIDE3);
				}
				else if (area.ID == 4)
				{
					Achievements.Register(Achievement.BSIDE4);
				}
				else if (area.ID == 5)
				{
					Achievements.Register(Achievement.BSIDE5);
				}
				else if (area.ID == 6)
				{
					Achievements.Register(Achievement.BSIDE6);
				}
				else if (area.ID == 7)
				{
					Achievements.Register(Achievement.BSIDE7);
				}
				else if (area.ID == 9)
				{
					Achievements.Register(Achievement.BSIDE8);
				}
			}
			StatsForStadia.SetIfLarger(StadiaStat.HEARTS, TotalHeartGems);
		}

		public void RegisterCassette(AreaKey area)
		{
			Areas[area.ID].Cassette = true;
			Achievements.Register(Achievement.CASS);
		}

		public bool RegisterPoemEntry(string id)
		{
			id = id.ToLower();
			if (Poem.Contains(id))
			{
				return false;
			}
			Poem.Add(id);
			return true;
		}

		public void RegisterSummitGem(int id)
		{
			if (SummitGems == null)
			{
				SummitGems = new bool[6];
			}
			SummitGems[id] = true;
		}

		public void RegisterCompletion(Session session)
		{
			AreaKey area = session.Area;
			AreaModeStats stats = Areas[area.ID].Modes[(int)area.Mode];
			if (session.GrabbedGolden)
			{
				stats.BestDeaths = 0;
			}
			if (session.StartedFromBeginning)
			{
				stats.SingleRunCompleted = true;
				if (stats.BestTime <= 0 || session.Deaths < stats.BestDeaths)
				{
					stats.BestDeaths = session.Deaths;
				}
				if (stats.BestTime <= 0 || session.Dashes < stats.BestDashes)
				{
					stats.BestDashes = session.Dashes;
				}
				if (stats.BestTime <= 0 || session.Time < stats.BestTime)
				{
					if (stats.BestTime > 0)
					{
						session.BeatBestTime = true;
					}
					stats.BestTime = session.Time;
				}
				if (area.Mode == AreaMode.Normal && session.FullClear)
				{
					stats.FullClear = true;
					if (session.StartedFromBeginning && (stats.BestFullClearTime <= 0 || session.Time < stats.BestFullClearTime))
					{
						stats.BestFullClearTime = session.Time;
					}
				}
			}
			if (area.ID + 1 > UnlockedAreas && area.ID < MaxArea)
			{
				UnlockedAreas = area.ID + 1;
			}
			stats.Completed = true;
			session.InArea = false;
		}

		public bool SetCheckpoint(AreaKey area, string level)
		{
			AreaModeStats stats = Areas[area.ID].Modes[(int)area.Mode];
			if (!stats.Checkpoints.Contains(level))
			{
				stats.Checkpoints.Add(level);
				return true;
			}
			return false;
		}

		public bool HasCheckpoint(AreaKey area, string level)
		{
			return Areas[area.ID].Modes[(int)area.Mode].Checkpoints.Contains(level);
		}

		public bool FoundAnyCheckpoints(AreaKey area)
		{
			if (Celeste.PlayMode == Celeste.PlayModes.Event)
			{
				return false;
			}
			if (DebugMode || CheatMode)
			{
				ModeProperties a = AreaData.Areas[area.ID].Mode[(int)area.Mode];
				if (a != null && a.Checkpoints != null)
				{
					return a.Checkpoints.Length != 0;
				}
				return false;
			}
			return Areas[area.ID].Modes[(int)area.Mode].Checkpoints.Count > 0;
		}

		public HashSet<string> GetCheckpoints(AreaKey area)
		{
			if (Celeste.PlayMode == Celeste.PlayModes.Event)
			{
				return new HashSet<string>();
			}
			if (DebugMode || CheatMode)
			{
				HashSet<string> all = new HashSet<string>();
				ModeProperties a = AreaData.Areas[area.ID].Mode[(int)area.Mode];
				if (a.Checkpoints != null)
				{
					CheckpointData[] checkpoints = a.Checkpoints;
					foreach (CheckpointData cp in checkpoints)
					{
						all.Add(cp.Level);
					}
				}
				return all;
			}
			return Areas[area.ID].Modes[(int)area.Mode].Checkpoints;
		}

		public bool HasFlag(string flag)
		{
			return Flags.Contains(flag);
		}

		public void SetFlag(string flag)
		{
			if (!HasFlag(flag))
			{
				Flags.Add(flag);
			}
		}
	}
}
