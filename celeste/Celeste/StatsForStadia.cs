using System;
using System.Collections.Generic;

namespace Celeste
{
	public static class StatsForStadia
	{
		private static Dictionary<StadiaStat, string> statToString = new Dictionary<StadiaStat, string>();

		private static bool ready;

		public static void MakeRequest()
		{
		}

		public static void Increment(StadiaStat stat, int increment = 1)
		{
		}

		public static void SetIfLarger(StadiaStat stat, int value)
		{
		}

		public static void BeginFrame(IntPtr handle)
		{
		}
	}
}
