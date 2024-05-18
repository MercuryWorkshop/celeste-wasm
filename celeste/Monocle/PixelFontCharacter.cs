using System.Collections.Generic;
using System.Xml;

namespace Monocle
{
	public class PixelFontCharacter
	{
		public int Character;

		public MTexture Texture;

		public int XOffset;

		public int YOffset;

		public int XAdvance;

		public Dictionary<int, int> Kerning = new Dictionary<int, int>();

		public PixelFontCharacter(int character, MTexture texture, XmlElement xml)
		{
			Character = character;
			Texture = texture.GetSubtexture(xml.AttrInt("x"), xml.AttrInt("y"), xml.AttrInt("width"), xml.AttrInt("height"));
			XOffset = xml.AttrInt("xoffset");
			YOffset = xml.AttrInt("yoffset");
			XAdvance = xml.AttrInt("xadvance");
		}
	}
}
