using System;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using Monocle;
using SDL2;

namespace Celeste
{
	public static class UserIO
	{
		public enum Mode
		{
			Read,
			Write
		}

		public const string SaveDataTitle = "Celeste Save Data";

		private static readonly string SavePath = GetSavePath("Saves");

		private static readonly string BackupPath = GetSavePath("Backups");

		private const string Extension = ".celeste";

		private static bool savingInternal;

		private static bool savingFile;

		private static bool savingSettings;

		private static byte[] savingFileData;

		private static byte[] savingSettingsData;

		public static bool Saving { get; private set; }

		public static bool SavingResult { get; private set; }

		private static string GetSavePath(string dir)
		{
			string os = SDL.SDL_GetPlatform();
			if (os.Equals("Linux") || os.Equals("FreeBSD") || os.Equals("OpenBSD") || os.Equals("NetBSD"))
			{
				string result = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
				if (!string.IsNullOrEmpty(result))
				{
					return Path.Combine(result, "Celeste/" + dir);
				}
				result = Environment.GetEnvironmentVariable("HOME");
				if (!string.IsNullOrEmpty(result))
				{
					return Path.Combine(result, ".local/share/Celeste/" + dir);
				}
			}
			else if (os.Equals("Mac OS X"))
			{
				string result = Environment.GetEnvironmentVariable("HOME");
				if (!string.IsNullOrEmpty(result))
				{
					return Path.Combine(result, "Library/Application Support/Celeste/" + dir);
				}
			}
			else if (!os.Equals("Windows"))
			{
				return Path.Combine(SDL.SDL_GetPrefPath(null, "Celeste"), dir);
			}
			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dir);
		}

		private static string GetHandle(string name)
		{
			return Path.Combine(SavePath, name + ".celeste");
		}

		private static string GetBackupHandle(string name)
		{
			return Path.Combine(BackupPath, name + ".celeste");
		}

		public static bool Open(Mode mode)
		{
			return true;
		}

		public static bool Save<T>(string path, byte[] data) where T : class
		{
			string handle = GetHandle(path);
			bool success = false;
			try
			{
				if (Celeste.IsGGP)
				{
					DirectoryInfo dir2 = new FileInfo(handle).Directory;
					if (!dir2.Exists)
					{
						dir2.Create();
					}
					File.WriteAllBytes(handle, data);
					byte[] diskData = File.ReadAllBytes(handle);
					if (data.Length != diskData.Length)
					{
						throw new InvalidOperationException("INVALID FILE LENGTH");
					}
					for (int i = 0; i < data.Length; i++)
					{
						if (data[i] != diskData[i])
						{
							throw new InvalidOperationException("INVALID FILE DATA");
						}
					}
					return true;
				}
				string backupHandle = GetBackupHandle(path);
				DirectoryInfo dir = new FileInfo(handle).Directory;
				if (!dir.Exists)
				{
					dir.Create();
				}
				dir = new FileInfo(backupHandle).Directory;
				if (!dir.Exists)
				{
					dir.Create();
				}
				using (FileStream stream = File.Open(backupHandle, FileMode.Create, FileAccess.Write))
				{
					stream.Write(data, 0, data.Length);
				}
				if (Load<T>(path, backup: true) != null)
				{
					File.Copy(backupHandle, handle, overwrite: true);
					success = true;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("ERROR: " + e.ToString());
				ErrorLog.Write(e);
			}
			if (!success)
			{
				Console.WriteLine("Save Failed");
			}
			return success;
		}

		public static T Load<T>(string path, bool backup = false) where T : class
		{
			string handle = ((!backup) ? GetHandle(path) : GetBackupHandle(path));
			T instance = null;
			try
			{
				if (File.Exists(handle))
				{
					using (FileStream stream = File.OpenRead(handle))
					{
						instance = Deserialize<T>(stream);
						return instance;
					}
				}
				return instance;
			}
			catch (Exception e)
			{
				Console.WriteLine("ERROR: " + e.ToString());
				ErrorLog.Write(e);
				return instance;
			}
		}

		private static T Deserialize<T>(Stream stream) where T : class
		{
			return (T)new XmlSerializer(typeof(T)).Deserialize(stream);
		}

		public static bool Exists(string path)
		{
			return File.Exists(GetHandle(path));
		}

		public static bool Delete(string path)
		{
			string handle = GetHandle(path);
			if (File.Exists(handle))
			{
				File.Delete(handle);
				return true;
			}
			return false;
		}

		public static void Close()
		{
		}

		public static byte[] Serialize<T>(T instance)
		{
			using MemoryStream ms = new MemoryStream();
			new XmlSerializer(typeof(T)).Serialize(ms, instance);
			return ms.ToArray();
		}

		public static void SaveHandler(bool file, bool settings)
		{
			if (!Saving)
			{
				Saving = true;
				Celeste.SaveRoutine = new Coroutine(SaveRoutine(file, settings));
			}
		}

		private static IEnumerator SaveRoutine(bool file, bool settings)
		{
			savingFile = file;
			savingSettings = settings;
			FileErrorOverlay menu;
			do
			{
				if (savingFile)
				{
					SaveData.Instance.BeforeSave();
					savingFileData = Serialize(SaveData.Instance);
				}
				if (savingSettings)
				{
					savingSettingsData = Serialize(Settings.Instance);
				}
				savingInternal = true;
				SavingResult = false;
                SaveThread();
				//RunThread.Start(SaveThread, "USER_IO");
				SaveLoadIcon.Show(Engine.Scene);
				while (savingInternal)
				{
					yield return null;
				}
				SaveLoadIcon.Hide();
				if (SavingResult)
				{
					break;
				}
				menu = new FileErrorOverlay(FileErrorOverlay.Error.Save);
				while (menu.Open)
				{
					yield return null;
				}
			}
			while (menu.TryAgain);
			Saving = false;
			Celeste.SaveRoutine = null;
		}

		private static void SaveThread()
		{
			SavingResult = false;
			if (Open(Mode.Write))
			{
				SavingResult = true;
				if (savingFile)
				{
					SavingResult &= Save<SaveData>(SaveData.GetFilename(), savingFileData);
				}
				if (savingSettings)
				{
					SavingResult &= Save<Settings>("settings", savingSettingsData);
				}
				Close();
			}
			savingInternal = false;
		}
	}
}
