using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Celeste;
using SDL2;

namespace Monocle
{
	public static class ErrorLog
	{
		public static readonly string Filename = GetLogPath();

		public const string Marker = "==========================================";

		public static void Write(Exception e)
		{
			Write(e.ToString());
		}

		public static void Write(string str)
		{
			StringBuilder s = new StringBuilder();
			string content = "";
			if (Path.IsPathRooted(Filename))
			{
				string dir = Path.GetDirectoryName(Filename);
				if (!Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
				}
			}
			if (File.Exists(Filename))
			{
				StreamReader streamReader = new StreamReader(Filename);
				content = streamReader.ReadToEnd();
				streamReader.Close();
				if (!content.Contains("=========================================="))
				{
					content = "";
				}
			}
			if (Engine.Instance != null)
			{
				s.Append(Engine.Instance.Title);
			}
			else
			{
				s.Append("Monocle Engine");
			}
			s.AppendLine(" Error Log");
			s.AppendLine("==========================================");
			s.AppendLine();
			if (Engine.Instance != null && Engine.Instance.Version != null)
			{
				s.Append("Ver ");
				s.AppendLine(Engine.Instance.Version.ToString());
			}
			s.AppendLine(DateTime.Now.ToString());
			s.AppendLine(str);
			if (content != "")
			{
				int at = content.IndexOf("==========================================") + "==========================================".Length;
				string after = content.Substring(at);
				s.AppendLine(after);
			}
			StreamWriter streamWriter = new StreamWriter(Filename, append: false);
			streamWriter.Write(s.ToString());
			streamWriter.Close();
		}

		public static void Open()
		{
			if (!global::Celeste.Celeste.IsGGP && File.Exists(Filename))
			{
                Console.WriteLine("Monocle Error Log:");
				Console.WriteLine(File.ReadAllText(Filename));
			}
		}

		private static string GetLogPath()
		{
			string os = SDL.SDL_GetPlatform();
			if (os.Equals("Linux") || os.Equals("FreeBSD") || os.Equals("OpenBSD") || os.Equals("NetBSD"))
			{
				string result = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
				if (!string.IsNullOrEmpty(result))
				{
					return Path.Combine(result, "Celeste", "errorLog.txt");
				}
				result = Environment.GetEnvironmentVariable("HOME");
				if (!string.IsNullOrEmpty(result))
				{
					return Path.Combine(result, ".local/share/Celeste", "errorLog.txt");
				}
			}
			else if (os.Equals("Mac OS X"))
			{
				string result = Environment.GetEnvironmentVariable("HOME");
				if (!string.IsNullOrEmpty(result))
				{
					return Path.Combine(result, "Library/Application Support/Celeste", "errorLog.txt");
				}
			}
			else if (!os.Equals("Windows"))
			{
				return Path.Combine(SDL.SDL_GetPrefPath(null, "Celeste"), "errorLog.txt");
			}
			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "errorLog.txt");
		}
	}
}
