using Monocle;

namespace Celeste
{
	public class AutoSplitterInfo
	{
		public int Chapter;

		public int Mode;

		public string Level;

		public bool TimerActive;

		public bool ChapterStarted;

		public bool ChapterComplete;

		public long ChapterTime;

		public int ChapterStrawberries;

		public bool ChapterCassette;

		public bool ChapterHeart;

		public long FileTime;

		public int FileStrawberries;

		public int FileCassettes;

		public int FileHearts;

		public void Update()
		{
			Level level = Engine.Scene as Level;
			ChapterStarted = level != null;
			ChapterComplete = ChapterStarted && level.Completed;
			TimerActive = ChapterStarted && !level.Completed;
			Chapter = (ChapterStarted ? level.Session.Area.ID : (-1));
			Mode = (int)(ChapterStarted ? level.Session.Area.Mode : ((AreaMode)(-1)));
			Level = (ChapterStarted ? level.Session.Level : "");
			ChapterTime = (ChapterStarted ? level.Session.Time : 0);
			FileTime = ((SaveData.Instance != null) ? SaveData.Instance.Time : 0);
			ChapterStrawberries = (ChapterStarted ? level.Session.Strawberries.Count : 0);
			FileStrawberries = ((SaveData.Instance != null) ? SaveData.Instance.TotalStrawberries : 0);
			ChapterHeart = ChapterStarted && level.Session.HeartGem;
			FileHearts = ((SaveData.Instance != null) ? SaveData.Instance.TotalHeartGems : 0);
			ChapterCassette = ChapterStarted && level.Session.Cassette;
			FileCassettes = ((SaveData.Instance != null) ? SaveData.Instance.TotalCassettes : 0);
		}
	}
}
