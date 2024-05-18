using System.Collections.Generic;
using System.IO;
using System.Xml;
using Monocle;

namespace Celeste
{
	public static class Fonts
	{
		private static Dictionary<string, List<string>> paths = new Dictionary<string, List<string>>();

		private static Dictionary<string, PixelFont> loadedFonts = new Dictionary<string, PixelFont>();

		public static PixelFont Load(string face)
		{
			if (!loadedFonts.TryGetValue(face, out var font) && paths.TryGetValue(face, out var files))
			{
				loadedFonts.Add(face, font = new PixelFont(face));
				{
					foreach (string file in files)
					{
						font.AddFontSize(file, GFX.Gui);
					}
					return font;
				}
			}
			return font;
		}

		public static PixelFont Get(string face)
		{
			if (loadedFonts.TryGetValue(face, out var font))
			{
				return font;
			}
			return null;
		}

		public static void Unload(string face)
		{
			if (loadedFonts.TryGetValue(face, out var font))
			{
				font.Dispose();
				loadedFonts.Remove(face);
			}
		}

		public static void Reload()
		{
			List<string> has = new List<string>();
			foreach (string loaded2 in loadedFonts.Keys)
			{
				has.Add(loaded2);
			}
			foreach (string loaded in has)
			{
				loadedFonts[loaded].Dispose();
				Load(loaded);
			}
		}

		public static void Prepare()
		{
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.CloseInput = true;
			string[] files2 = Directory.GetFiles(Path.Combine(Engine.ContentDirectory, "Dialog"), "*.fnt", SearchOption.AllDirectories);
			foreach (string fontFile in files2)
			{
				string fontFace = null;
				using (XmlReader reader = XmlReader.Create(File.OpenRead(fontFile), settings))
				{
					while (reader.Read())
					{
						if (reader.NodeType == XmlNodeType.Element && reader.Name == "info")
						{
							fontFace = reader.GetAttribute("face");
						}
					}
				}
				if (fontFace != null)
				{
					if (!paths.TryGetValue(fontFace, out var files))
					{
						paths.Add(fontFace, files = new List<string>());
					}
					files.Add(fontFile);
				}
			}
		}

		public static void Log()
		{
			Engine.Commands.Log("EXISTING FONTS:");
			foreach (KeyValuePair<string, List<string>> kv in paths)
			{
				Engine.Commands.Log(" - " + kv.Key);
				foreach (string path in kv.Value)
				{
					Engine.Commands.Log(" - > " + path);
				}
			}
			Engine.Commands.Log("LOADED:");
			foreach (KeyValuePair<string, PixelFont> fn in loadedFonts)
			{
				Engine.Commands.Log(" - " + fn.Key);
			}
		}
	}
}
