using System;
using System.Collections.Generic;

namespace Monocle
{
	public class BitTag
	{
		internal static int TotalTags = 0;

		internal static BitTag[] byID = new BitTag[32];

		private static Dictionary<string, BitTag> byName = new Dictionary<string, BitTag>(StringComparer.OrdinalIgnoreCase);

		public int ID;

		public int Value;

		public string Name;

		public static BitTag Get(string name)
		{
			return byName[name];
		}

		public BitTag(string name)
		{
			ID = TotalTags;
			Value = 1 << TotalTags;
			Name = name;
			byID[ID] = this;
			byName[name] = this;
			TotalTags++;
		}

		public static implicit operator int(BitTag tag)
		{
			return tag.Value;
		}
	}
}
