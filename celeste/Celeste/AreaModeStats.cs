using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Celeste
{
	[Serializable]
	public class AreaModeStats
	{
		[XmlAttribute]
		public int TotalStrawberries;

		[XmlAttribute]
		public bool Completed;

		[XmlAttribute]
		public bool SingleRunCompleted;

		[XmlAttribute]
		public bool FullClear;

		[XmlAttribute]
		public int Deaths;

		[XmlAttribute]
		public long TimePlayed;

		[XmlAttribute]
		public long BestTime;

		[XmlAttribute]
		public long BestFullClearTime;

		[XmlAttribute]
		public int BestDashes;

		[XmlAttribute]
		public int BestDeaths;

		[XmlAttribute]
		public bool HeartGem;

		public HashSet<EntityID> Strawberries = new HashSet<EntityID>();

		public HashSet<string> Checkpoints = new HashSet<string>();

		public AreaModeStats Clone()
		{
			return new AreaModeStats
			{
				TotalStrawberries = TotalStrawberries,
				Strawberries = new HashSet<EntityID>(Strawberries),
				Completed = Completed,
				SingleRunCompleted = SingleRunCompleted,
				FullClear = FullClear,
				Deaths = Deaths,
				TimePlayed = TimePlayed,
				BestTime = BestTime,
				BestFullClearTime = BestFullClearTime,
				BestDashes = BestDashes,
				BestDeaths = BestDeaths,
				HeartGem = HeartGem,
				Checkpoints = new HashSet<string>(Checkpoints)
			};
		}
	}
}
