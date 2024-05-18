using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;

namespace Monocle
{
	public class SpriteData
	{
		public List<SpriteDataSource> Sources = new List<SpriteDataSource>();

		public Sprite Sprite;

		public Atlas Atlas;

		public SpriteData(Atlas atlas)
		{
			Sprite = new Sprite(atlas, "");
			Atlas = atlas;
		}

		public void Add(XmlElement xml, string overridePath = null)
		{
			SpriteDataSource source = new SpriteDataSource();
			source.XML = xml;
			source.Path = source.XML.Attr("path");
			source.OverridePath = overridePath;
			string prefix = "Sprite '" + source.XML.Name + "': ";
			if (!source.XML.HasAttr("path") && string.IsNullOrEmpty(overridePath))
			{
				throw new Exception(prefix + "'path' is missing!");
			}
			HashSet<string> ids = new HashSet<string>();
			foreach (XmlElement anim2 in source.XML.GetElementsByTagName("Anim"))
			{
				CheckAnimXML(anim2, prefix, ids);
			}
			foreach (XmlElement loop2 in source.XML.GetElementsByTagName("Loop"))
			{
				CheckAnimXML(loop2, prefix, ids);
			}
			if (source.XML.HasAttr("start") && !ids.Contains(source.XML.Attr("start")))
			{
				throw new Exception(prefix + "starting animation '" + source.XML.Attr("start") + "' is missing!");
			}
			if (source.XML.HasChild("Justify") && source.XML.HasChild("Origin"))
			{
				throw new Exception(prefix + "has both Origin and Justify tags!");
			}
			string normalPath = source.XML.Attr("path", "");
			float masterDelay = source.XML.AttrFloat("delay", 0f);
			foreach (XmlElement anim in source.XML.GetElementsByTagName("Anim"))
			{
				Chooser<string> into = ((!anim.HasAttr("goto")) ? null : Chooser<string>.FromString<string>(anim.Attr("goto")));
				string id2 = anim.Attr("id");
				string path2 = anim.Attr("path", "");
				int[] frames2 = Calc.ReadCSVIntWithTricks(anim.Attr("frames", ""));
				path2 = ((string.IsNullOrEmpty(overridePath) || !HasFrames(Atlas, overridePath + path2, frames2)) ? (normalPath + path2) : (overridePath + path2));
				Sprite.Add(id2, path2, anim.AttrFloat("delay", masterDelay), into, frames2);
			}
			foreach (XmlElement loop in source.XML.GetElementsByTagName("Loop"))
			{
				string id = loop.Attr("id");
				string path = loop.Attr("path", "");
				int[] frames = Calc.ReadCSVIntWithTricks(loop.Attr("frames", ""));
				path = ((string.IsNullOrEmpty(overridePath) || !HasFrames(Atlas, overridePath + path, frames)) ? (normalPath + path) : (overridePath + path));
				Sprite.AddLoop(id, path, loop.AttrFloat("delay", masterDelay), frames);
			}
			if (source.XML.HasChild("Center"))
			{
				Sprite.CenterOrigin();
				Sprite.Justify = new Vector2(0.5f, 0.5f);
			}
			else if (source.XML.HasChild("Justify"))
			{
				Sprite.JustifyOrigin(source.XML.ChildPosition("Justify"));
				Sprite.Justify = source.XML.ChildPosition("Justify");
			}
			else if (source.XML.HasChild("Origin"))
			{
				Sprite.Origin = source.XML.ChildPosition("Origin");
			}
			if (source.XML.HasChild("Position"))
			{
				Sprite.Position = source.XML.ChildPosition("Position");
			}
			if (source.XML.HasAttr("start"))
			{
				Sprite.Play(source.XML.Attr("start"));
			}
			Sources.Add(source);
		}

		private bool HasFrames(Atlas atlas, string path, int[] frames = null)
		{
			if (frames == null || frames.Length == 0)
			{
				return atlas.GetAtlasSubtexturesAt(path, 0) != null;
			}
			for (int i = 0; i < frames.Length; i++)
			{
				if (atlas.GetAtlasSubtexturesAt(path, frames[i]) == null)
				{
					return false;
				}
			}
			return true;
		}

		private void CheckAnimXML(XmlElement xml, string prefix, HashSet<string> ids)
		{
			if (!xml.HasAttr("id"))
			{
				throw new Exception(prefix + "'id' is missing on " + xml.Name + "!");
			}
			if (ids.Contains(xml.Attr("id")))
			{
				throw new Exception(prefix + "multiple animations with id '" + xml.Attr("id") + "'!");
			}
			ids.Add(xml.Attr("id"));
		}

		public Sprite Create()
		{
			return Sprite.CreateClone();
		}

		public Sprite CreateOn(Sprite sprite)
		{
			return Sprite.CloneInto(sprite);
		}
	}
}
