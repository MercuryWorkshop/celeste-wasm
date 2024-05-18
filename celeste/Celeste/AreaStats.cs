using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Celeste
{
	[Serializable]
	public class AreaStats
	{
		[XmlAttribute]
		public int ID;

		[XmlAttribute]
		public bool Cassette;

		public AreaModeStats[] Modes;

		public int TotalStrawberries
		{
			get
			{
				int strawberries = 0;
				for (int i = 0; i < Modes.Length; i++)
				{
					strawberries += Modes[i].TotalStrawberries;
				}
				return strawberries;
			}
		}

		public int TotalDeaths
		{
			get
			{
				int d = 0;
				for (int i = 0; i < Modes.Length; i++)
				{
					d += Modes[i].Deaths;
				}
				return d;
			}
		}

		public long TotalTimePlayed
		{
			get
			{
				long t = 0L;
				for (int i = 0; i < Modes.Length; i++)
				{
					t += Modes[i].TimePlayed;
				}
				return t;
			}
		}

		public int BestTotalDeaths
		{
			get
			{
				int d = 0;
				for (int i = 0; i < Modes.Length; i++)
				{
					d += Modes[i].BestDeaths;
				}
				return d;
			}
		}

		public int BestTotalDashes
		{
			get
			{
				int d = 0;
				for (int i = 0; i < Modes.Length; i++)
				{
					d += Modes[i].BestDashes;
				}
				return d;
			}
		}

		public long BestTotalTime
		{
			get
			{
				long t = 0L;
				for (int i = 0; i < Modes.Length; i++)
				{
					t += Modes[i].BestTime;
				}
				return t;
			}
		}

		public AreaStats(int id)
		{
			ID = id;
			int length = Enum.GetValues(typeof(AreaMode)).Length;
			Modes = new AreaModeStats[length];
			for (int i = 0; i < Modes.Length; i++)
			{
				Modes[i] = new AreaModeStats();
			}
		}

		private AreaStats()
		{
			int modes = Enum.GetValues(typeof(AreaMode)).Length;
			Modes = new AreaModeStats[modes];
			for (int mode = 0; mode < modes; mode++)
			{
				Modes[mode] = new AreaModeStats();
			}
		}

		public AreaStats Clone()
		{
			AreaStats clone = new AreaStats
			{
				ID = ID,
				Cassette = Cassette
			};
			for (int i = 0; i < Modes.Length; i++)
			{
				clone.Modes[i] = Modes[i].Clone();
			}
			return clone;
		}

		public void CleanCheckpoints()
		{
			foreach (AreaMode mode in Enum.GetValues(typeof(AreaMode)))
			{
				if (AreaData.Get(ID).Mode.Length <= (int)mode)
				{
					continue;
				}
				AreaModeStats stats = Modes[(int)mode];
				ModeProperties data = AreaData.Get(ID).Mode[(int)mode];
				HashSet<string> checkpoints = new HashSet<string>(stats.Checkpoints);
				stats.Checkpoints.Clear();
				if (data == null || data.Checkpoints == null)
				{
					continue;
				}
				CheckpointData[] checkpoints2 = data.Checkpoints;
				foreach (CheckpointData checkpointData in checkpoints2)
				{
					if (checkpoints.Contains(checkpointData.Level))
					{
						stats.Checkpoints.Add(checkpointData.Level);
					}
				}
			}
		}
	}
}
