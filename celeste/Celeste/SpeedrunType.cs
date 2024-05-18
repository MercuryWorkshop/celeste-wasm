using System.Xml.Serialization;

namespace Celeste
{
	public enum SpeedrunType
	{
		[XmlEnum("false")]
		Off,
		[XmlEnum("true")]
		Chapter,
		File
	}
}
