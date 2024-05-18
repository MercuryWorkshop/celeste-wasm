using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Monocle;

namespace Celeste
{
	public class Language
	{
		public string FilePath;

		public string Id;

		public string Label;

		public string IconPath;

		public MTexture Icon;

		public int Order = 100;

		public string FontFace;

		public float FontFaceSize;

		public string SplitRegex = "(\\s|\\{|\\})";

		public string CommaCharacters = ",";

		public string PeriodCharacters = ".!?";

		public int Lines;

		public int Words;

		public Dictionary<string, string> Dialog = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public Dictionary<string, string> Cleaned = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		private static readonly Regex command = new Regex("\\{(.*?)\\}", RegexOptions.RightToLeft);

		private static readonly Regex insert = new Regex("\\{\\+\\s*(.*?)\\}");

		private static readonly Regex variable = new Regex("^\\w+\\=.*");

		private static readonly Regex portrait = new Regex("\\[(?<content>[^\\[\\\\]*(?:\\\\.[^\\]\\\\]*)*)\\]", RegexOptions.IgnoreCase);

		public PixelFont Font => Fonts.Get(FontFace);

		public PixelFontSize FontSize => Font.Get(FontFaceSize);

		public string this[string name] => Dialog[name];

		public bool CanDisplay(string text)
		{
			PixelFontSize fnt = FontSize;
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] != ' ' && fnt.Get(text[i]) == null)
				{
					return false;
				}
			}
			return true;
		}

		public void Export(string path)
		{
			using BinaryWriter writer = new BinaryWriter(File.OpenWrite(path));
			writer.Write(Id);
			writer.Write(Label);
			writer.Write(IconPath);
			writer.Write(Order);
			writer.Write(FontFace);
			writer.Write(FontFaceSize);
			writer.Write(SplitRegex);
			writer.Write(CommaCharacters);
			writer.Write(PeriodCharacters);
			writer.Write(Lines);
			writer.Write(Words);
			writer.Write(Dialog.Count);
			foreach (KeyValuePair<string, string> kv in Dialog)
			{
				writer.Write(kv.Key);
				writer.Write(kv.Value);
				writer.Write(Cleaned[kv.Key]);
			}
		}

		public static Language FromExport(string path)
		{
			Language language = new Language();
			using BinaryReader reader = new BinaryReader(File.OpenRead(path));
			language.Id = reader.ReadString();
			language.Label = reader.ReadString();
			language.IconPath = reader.ReadString();
			language.Icon = new MTexture(VirtualContent.CreateTexture(Path.Combine("Dialog", language.IconPath)));
			language.Order = reader.ReadInt32();
			language.FontFace = reader.ReadString();
			language.FontFaceSize = reader.ReadSingle();
			language.SplitRegex = reader.ReadString();
			language.CommaCharacters = reader.ReadString();
			language.PeriodCharacters = reader.ReadString();
			language.Lines = reader.ReadInt32();
			language.Words = reader.ReadInt32();
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = reader.ReadString();
				language.Dialog[key] = reader.ReadString();
				language.Cleaned[key] = reader.ReadString();
			}
			return language;
		}

		public static Language FromTxt(string path)
		{
			Language language = null;
			string lastKey = "";
			StringBuilder lastValue = new StringBuilder();
			string lastLine = "";
			foreach (string item in File.ReadLines(path, Encoding.UTF8))
			{
				string line = item.Trim();
				if (line.Length <= 0 || line[0] == '#')
				{
					continue;
				}
				if (line.IndexOf('[') >= 0)
				{
					line = portrait.Replace(line, "{portrait ${content}}");
				}
				line = line.Replace("\\#", "#");
				if (line.Length <= 0)
				{
					continue;
				}
				if (variable.IsMatch(line))
				{
					if (!string.IsNullOrEmpty(lastKey))
					{
						language.Dialog[lastKey] = lastValue.ToString();
					}
					string[] split = line.Split('=');
					string key2 = split[0].Trim();
					string value = ((split.Length > 1) ? split[1].Trim() : "");
					if (key2.Equals("language", StringComparison.OrdinalIgnoreCase))
					{
						string[] data = value.Split(',');
						language = new Language();
						language.FontFace = null;
						language.Id = data[0];
						language.FilePath = Path.GetFileName(path);
						if (data.Length > 1)
						{
							language.Label = data[1];
						}
					}
					else if (key2.Equals("icon", StringComparison.OrdinalIgnoreCase))
					{
						VirtualTexture tex = VirtualContent.CreateTexture(Path.Combine("Dialog", value));
						language.IconPath = value;
						language.Icon = new MTexture(tex);
					}
					else if (key2.Equals("order", StringComparison.OrdinalIgnoreCase))
					{
						language.Order = int.Parse(value);
					}
					else if (key2.Equals("font", StringComparison.OrdinalIgnoreCase))
					{
						string[] args = value.Split(',');
						language.FontFace = args[0];
						language.FontFaceSize = float.Parse(args[1], CultureInfo.InvariantCulture);
					}
					else if (key2.Equals("SPLIT_REGEX", StringComparison.OrdinalIgnoreCase))
					{
						language.SplitRegex = value;
					}
					else if (key2.Equals("commas", StringComparison.OrdinalIgnoreCase))
					{
						language.CommaCharacters = value;
					}
					else if (key2.Equals("periods", StringComparison.OrdinalIgnoreCase))
					{
						language.PeriodCharacters = value;
					}
					else
					{
						lastKey = key2;
						lastValue.Clear();
						lastValue.Append(value);
					}
				}
				else
				{
					if (lastValue.Length > 0)
					{
						string str = lastValue.ToString();
						if (!str.EndsWith("{break}") && !str.EndsWith("{n}") && command.Replace(lastLine, "").Length > 0)
						{
							lastValue.Append("{break}");
						}
					}
					lastValue.Append(line);
				}
				lastLine = line;
			}
			if (!string.IsNullOrEmpty(lastKey))
			{
				language.Dialog[lastKey] = lastValue.ToString();
			}
			List<string> ids = new List<string>();
			foreach (KeyValuePair<string, string> item2 in language.Dialog)
			{
				ids.Add(item2.Key);
			}
			foreach (string id2 in ids)
			{
				string content2 = language.Dialog[id2];
				MatchCollection matches = null;
				while (matches == null || matches.Count > 0)
				{
					matches = insert.Matches(content2);
					for (int i = 0; i < matches.Count; i++)
					{
						Match match = matches[i];
						string key = match.Groups[1].Value;
						content2 = ((!language.Dialog.TryGetValue(key, out var currentInsert)) ? content2.Replace(match.Value, "[XXX]") : content2.Replace(match.Value, currentInsert));
					}
				}
				language.Dialog[id2] = content2;
			}
			language.Lines = 0;
			language.Words = 0;
			foreach (string id in ids)
			{
				string content = language.Dialog[id];
				if (content.IndexOf('{') >= 0)
				{
					content = content.Replace("{n}", "\n");
					content = content.Replace("{break}", "\n");
					content = command.Replace(content, "");
				}
				language.Cleaned.Add(id, content);
			}
			return language;
		}

		public void Dispose()
		{
			if (Icon.Texture != null && !Icon.Texture.IsDisposed)
			{
				Icon.Texture.Dispose();
			}
		}
	}
}
