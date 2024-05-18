using System;
using System.Xml.Serialization;

namespace Celeste
{
	[Serializable]
	public class MEP
	{
		[XmlAttribute]
		public string Key;

		[XmlAttribute]
		public float Value;

		public MEP()
		{
		}

		public MEP(string key, float value)
		{
			Key = key;
			Value = value;
		}
	}
}
