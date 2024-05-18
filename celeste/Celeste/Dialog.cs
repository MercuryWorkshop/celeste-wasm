using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Monocle;

namespace Celeste
{
	public static class Dialog
	{
		public static Language Language = null;

		public static Dictionary<string, Language> Languages;

		public static List<Language> OrderedLanguages;

		private static string[] LanguageDataVariables = new string[7] { "language", "icon", "order", "split_regex", "commas", "periods", "font" };

		private const string path = "Dialog";

		public static void Load()
		{
			Language = null;
			Languages = new Dictionary<string, Language>();
			string[] files = Directory.GetFiles(Path.Combine(Engine.ContentDirectory, "Dialog"), "*.txt", SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				LoadLanguage(files[i]);
			}
			if (Settings.Instance != null && Settings.Instance.Language != null && Languages.ContainsKey(Settings.Instance.Language))
			{
				Language = Languages[Settings.Instance.Language];
			}
			else if (Languages.ContainsKey("english"))
			{
				Language = Languages["english"];
			}
			else
			{
				if (Languages.Count <= 0)
				{
					throw new Exception("Missing Language Files");
				}
				Language = Languages.ElementAt(0).Value;
			}
			Settings.Instance.Language = Language.Id;
			OrderedLanguages = new List<Language>();
			foreach (KeyValuePair<string, Language> language in Languages)
			{
				OrderedLanguages.Add(language.Value);
			}
			OrderedLanguages.Sort((Language a, Language b) => (a.Order != b.Order) ? (a.Order - b.Order) : a.Id.CompareTo(b.Id));
		}

		public static Language LoadLanguage(string filename)
		{
			Language language = null;
			language = ((!File.Exists(filename + ".export")) ? Language.FromTxt(filename) : Language.FromExport(filename + ".export"));
			if (language != null)
			{
				Languages[language.Id] = language;
			}
			return language;
		}

		public static void Unload()
		{
			foreach (KeyValuePair<string, Language> language in Languages)
			{
				language.Value.Dispose();
			}
			Languages.Clear();
			Language = null;
			OrderedLanguages.Clear();
			OrderedLanguages = null;
		}

		public static bool Has(string name, Language language = null)
		{
			if (language == null)
			{
				language = Language;
			}
			return language.Dialog.ContainsKey(name);
		}

		public static string Get(string name, Language language = null)
		{
			if (language == null)
			{
				language = Language;
			}
			string result = "";
			if (language.Dialog.TryGetValue(name, out result))
			{
				return result;
			}
			return "XXX";
		}

		public static string Clean(string name, Language language = null)
		{
			if (language == null)
			{
				language = Language;
			}
			string result = "";
			if (language.Cleaned.TryGetValue(name, out result))
			{
				return result;
			}
			return "XXX";
		}

		public static string Time(long ticks)
		{
			TimeSpan time = TimeSpan.FromTicks(ticks);
			if ((int)time.TotalHours > 0)
			{
				return (int)time.TotalHours + time.ToString("\\:mm\\:ss\\.fff");
			}
			return time.Minutes + time.ToString("\\:ss\\.fff");
		}

		public static string FileTime(long ticks)
		{
			TimeSpan time = TimeSpan.FromTicks(ticks);
			if (time.TotalHours >= 1.0)
			{
				return (int)time.TotalHours + time.ToString("\\:mm\\:ss\\.fff");
			}
			return time.ToString("mm\\:ss\\.fff");
		}

		public static string Deaths(int deaths)
		{
			if (deaths > 999999)
			{
				return ((float)deaths / 1000000f).ToString("0.00") + "m";
			}
			if (deaths > 9999)
			{
				return ((float)deaths / 1000f).ToString("0.0") + "k";
			}
			return deaths.ToString();
		}

		public static void CheckCharacters()
		{
			string[] files = Directory.GetFiles(Path.Combine(Engine.ContentDirectory, "Dialog"), "*.txt", SearchOption.AllDirectories);
			foreach (string file in files)
			{
				HashSet<int> characters = new HashSet<int>();
				foreach (string line in File.ReadLines(file, Encoding.UTF8))
				{
					for (int j = 0; j < line.Length; j++)
					{
						if (!characters.Contains(line[j]))
						{
							characters.Add(line[j]);
						}
					}
				}
				List<int> listed = new List<int>();
				foreach (int c in characters)
				{
					listed.Add(c);
				}
				listed.Sort();
				StringBuilder str = new StringBuilder();
				str.Append("chars=");
				int counter = 0;
				Console.WriteLine("Characters of : " + file);
				int i;
				for (i = 0; i < listed.Count; i++)
				{
					bool isList = false;
					int k;
					for (k = i + 1; k < listed.Count && listed[k] == listed[k - 1] + 1; k++)
					{
						isList = true;
					}
					if (isList)
					{
						str.Append(listed[i] + "-" + listed[k - 1] + ",");
					}
					else
					{
						str.Append(listed[i] + ",");
					}
					i = k - 1;
					counter++;
					if (counter >= 10)
					{
						counter = 0;
						str.Remove(str.Length - 1, 1);
						Console.WriteLine(str.ToString());
						str.Clear();
						str.Append("chars=");
					}
				}
				str.Remove(str.Length - 1, 1);
				Console.WriteLine(str.ToString());
				Console.WriteLine();
			}
		}

		public static bool CheckLanguageFontCharacters(string a)
		{
			Language lang = Languages[a];
			bool result = true;
			HashSet<int> missing = new HashSet<int>();
			foreach (KeyValuePair<string, string> msg in lang.Dialog)
			{
				for (int i = 0; i < msg.Value.Length; i++)
				{
					int c2 = msg.Value[i];
					if (!missing.Contains(c2) && !lang.FontSize.Characters.ContainsKey(c2))
					{
						missing.Add(c2);
						result = false;
					}
				}
			}
			Console.WriteLine("FONT: " + a);
			if (missing.Count > 0)
			{
				Console.WriteLine(" - Missing Characters: " + string.Join(",", missing));
			}
			Console.WriteLine(" - OK: " + result);
			Console.WriteLine();
			if (missing.Count > 0)
			{
				string txt = "";
				foreach (int c in missing)
				{
					txt += (char)c;
				}
				File.WriteAllText(a + "-missing-debug.txt", txt);
			}
			return result;
		}

		public static bool CompareLanguages(string a, string b, bool compareContent)
		{
			Console.WriteLine("COMPARE: " + a + " -> " + b);
			Language langA = Languages[a];
			Language langB = Languages[b];
			bool result = true;
			List<string> missingFromA = new List<string>();
			List<string> missingFromB = new List<string>();
			List<string> contentNotEqual = new List<string>();
			foreach (KeyValuePair<string, string> id3 in langA.Dialog)
			{
				if (!langB.Dialog.ContainsKey(id3.Key))
				{
					missingFromB.Add(id3.Key);
					result = false;
				}
				else if (compareContent && langB.Dialog[id3.Key] != langA.Dialog[id3.Key])
				{
					contentNotEqual.Add(id3.Key);
					result = false;
				}
			}
			foreach (KeyValuePair<string, string> id2 in langB.Dialog)
			{
				if (!langA.Dialog.ContainsKey(id2.Key))
				{
					missingFromA.Add(id2.Key);
					result = false;
				}
			}
			if (missingFromA.Count > 0)
			{
				Console.WriteLine(" - Missing from " + a + ": " + string.Join(", ", missingFromA));
			}
			if (missingFromB.Count > 0)
			{
				Console.WriteLine(" - Missing from " + b + ": " + string.Join(", ", missingFromB));
			}
			if (contentNotEqual.Count > 0)
			{
				Console.WriteLine(" - Diff. Content: " + string.Join(", ", contentNotEqual));
			}
			Func<string, List<List<string>>> GetEvents = delegate(string text)
			{
				List<List<string>> list = new List<List<string>>();
				foreach (Match item in Regex.Matches(text, "\\{([^}]*)\\}"))
				{
					string[] array = Regex.Split(item.Value, "(\\{|\\}|\\s)");
					List<string> list2 = new List<string>();
					string[] array2 = array;
					foreach (string text2 in array2)
					{
						if (!string.IsNullOrWhiteSpace(text2) && text2.Length > 0 && text2 != "{" && text2 != "}")
						{
							list2.Add(text2);
						}
					}
					list.Add(list2);
				}
				return list;
			};
			foreach (KeyValuePair<string, string> id in langA.Dialog)
			{
				if (!langB.Dialog.ContainsKey(id.Key))
				{
					continue;
				}
				List<List<string>> matchesA = GetEvents(id.Value);
				List<List<string>> matchesB = GetEvents(langB.Dialog[id.Key]);
				int i = 0;
				int j = 0;
				for (; i < matchesA.Count; i++)
				{
					string e = matchesA[i][0];
					if (!(e == "portrait") && !(e == "trigger"))
					{
						continue;
					}
					for (; j < matchesB.Count && matchesB[j][0] != e; j++)
					{
					}
					if (j >= matchesB.Count)
					{
						Console.WriteLine(" - Command number mismatch in " + id.Key + " in " + b);
						result = false;
						i = matchesA.Count;
						continue;
					}
					if (e == "portrait")
					{
						for (int k = 0; k < matchesA[i].Count; k++)
						{
							if (matchesA[i][k] != matchesB[j][k])
							{
								Console.WriteLine(" - Portrait in " + id.Key + " is incorrect in " + b + " ({" + string.Join(" ", matchesA[i]) + "} vs {" + string.Join(" ", matchesB[j]) + "})");
								result = false;
							}
						}
					}
					else if (e == "trigger" && matchesA[i][1] != matchesB[j][1])
					{
						Console.WriteLine(" - Trigger in " + id.Key + " is incorrect in " + b + " (" + matchesA[i][1] + " vs " + matchesB[j][1] + ")");
						result = false;
					}
					j++;
				}
			}
			Console.WriteLine(" - OK: " + result);
			Console.WriteLine();
			return result;
		}
	}
}
