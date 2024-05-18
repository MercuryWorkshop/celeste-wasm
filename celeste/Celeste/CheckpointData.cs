using System.Collections.Generic;

namespace Celeste
{
	public class CheckpointData
	{
		public string Level;

		public string Name;

		public bool Dreaming;

		public int Strawberries;

		public string ColorGrade;

		public PlayerInventory? Inventory;

		public AudioState AudioState;

		public HashSet<string> Flags;

		public Session.CoreModes? CoreMode;

		public CheckpointData(string level, string name, PlayerInventory? inventory = null, bool dreaming = false, AudioState audioState = null)
		{
			Level = level;
			Name = name;
			Inventory = inventory;
			Dreaming = dreaming;
			AudioState = audioState;
			CoreMode = null;
			ColorGrade = null;
		}
	}
}
