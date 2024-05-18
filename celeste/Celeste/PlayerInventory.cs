using System;

namespace Celeste
{
	[Serializable]
	public struct PlayerInventory
	{
		public static readonly PlayerInventory Prologue = new PlayerInventory(0, dreamDash: false);

		public static readonly PlayerInventory Default = new PlayerInventory(1, dreamDash: true, backpack: true, noRefills: false);

		public static readonly PlayerInventory OldSite = new PlayerInventory(1, dreamDash: false);

		public static readonly PlayerInventory CH6End = new PlayerInventory(2);

		public static readonly PlayerInventory TheSummit = new PlayerInventory(2, dreamDash: true, backpack: false);

		public static readonly PlayerInventory Core = new PlayerInventory(2, dreamDash: true, backpack: true, noRefills: true);

		public static readonly PlayerInventory Farewell = new PlayerInventory(1, dreamDash: true, backpack: false);

		public int Dashes;

		public bool DreamDash;

		public bool Backpack;

		public bool NoRefills;

		public PlayerInventory(int dashes = 1, bool dreamDash = true, bool backpack = true, bool noRefills = false)
		{
			Dashes = dashes;
			DreamDash = dreamDash;
			Backpack = backpack;
			NoRefills = noRefills;
		}
	}
}
