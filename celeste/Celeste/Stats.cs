using System.Collections.Generic;

namespace Celeste
{
	public static class Stats
	{
		private static Dictionary<Stat, string> statToString = new Dictionary<Stat, string>();

		private static bool ready;

		public static void MakeRequest()
		{
		}

		public static bool Has()
		{
			return false;
		}

		public static void Increment(Stat stat, int increment = 1)
		{
		}

		public static int Local(Stat stat)
		{
			return 0;
		}

		public static long Global(Stat stat)
		{
			return 0L;
		}

		public static void Store()
		{
		}

		public static string Name(Stat stat)
		{
			return Dialog.Clean("STAT_" + stat);
		}
	}
}
