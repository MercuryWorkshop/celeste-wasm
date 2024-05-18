using System;
using System.Collections.Generic;
using System.Xml;

namespace Monocle
{
	public class SpriteBank
	{
		public Atlas Atlas;

		public XmlDocument XML;

		public Dictionary<string, SpriteData> SpriteData;

		public SpriteBank(Atlas atlas, XmlDocument xml)
		{
			Atlas = atlas;
			XML = xml;
			SpriteData = new Dictionary<string, SpriteData>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, XmlElement> elements = new Dictionary<string, XmlElement>();
			foreach (object e in XML["Sprites"].ChildNodes)
			{
				if (e is XmlElement)
				{
					XmlElement element = e as XmlElement;
					elements.Add(element.Name, element);
					if (SpriteData.ContainsKey(element.Name))
					{
						throw new Exception("Duplicate sprite name in SpriteData: '" + element.Name + "'!");
					}
					SpriteData spriteData2 = (SpriteData[element.Name] = new SpriteData(Atlas));
					SpriteData data = spriteData2;
					if (element.HasAttr("copy"))
					{
						data.Add(elements[element.Attr("copy")], element.Attr("path"));
					}
					data.Add(element);
				}
			}
		}

		public SpriteBank(Atlas atlas, string xmlPath)
			: this(atlas, Calc.LoadContentXML(xmlPath))
		{
		}

		public bool Has(string id)
		{
			return SpriteData.ContainsKey(id);
		}

		public Sprite Create(string id)
		{
			if (SpriteData.ContainsKey(id))
			{
				return SpriteData[id].Create();
			}
			throw new Exception("Missing animation name in SpriteData: '" + id + "'!");
		}

		public Sprite CreateOn(Sprite sprite, string id)
		{
			if (SpriteData.ContainsKey(id))
			{
				return SpriteData[id].CreateOn(sprite);
			}
			throw new Exception("Missing animation name in SpriteData: '" + id + "'!");
		}
	}
}
