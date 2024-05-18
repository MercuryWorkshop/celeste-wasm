using System;
using System.Xml.Serialization;

namespace Celeste
{
	[Serializable]
	public struct AreaKey
	{
		public static readonly AreaKey None = new AreaKey(-1);

		public static readonly AreaKey Default = new AreaKey(0);

		[XmlAttribute]
		public int ID;

		[XmlAttribute]
		public AreaMode Mode;

		public int ChapterIndex
		{
			get
			{
				if (AreaData.Areas[ID].Interlude)
				{
					return -1;
				}
				int chapter = 0;
				for (int i = 0; i <= ID; i++)
				{
					if (!AreaData.Areas[i].Interlude)
					{
						chapter++;
					}
				}
				return chapter;
			}
		}

		public AreaKey(int id, AreaMode mode = AreaMode.Normal)
		{
			ID = id;
			Mode = mode;
		}

		public static bool operator ==(AreaKey a, AreaKey b)
		{
			if (a.ID == b.ID)
			{
				return a.Mode == b.Mode;
			}
			return false;
		}

		public static bool operator !=(AreaKey a, AreaKey b)
		{
			if (a.ID == b.ID)
			{
				return a.Mode != b.Mode;
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			return false;
		}

		public override int GetHashCode()
		{
			return (int)(ID * 3 + Mode);
		}

		public override string ToString()
		{
			string str = ID.ToString();
			if (Mode == AreaMode.BSide)
			{
				str += "H";
			}
			else if (Mode == AreaMode.CSide)
			{
				str += "HH";
			}
			return str;
		}
	}
}
