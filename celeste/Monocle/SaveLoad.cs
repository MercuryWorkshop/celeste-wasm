using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace Monocle
{
	public static class SaveLoad
	{
		public enum SerializeModes
		{
			Binary,
			XML
		}

		public static void SerializeToFile<T>(T obj, string filepath, SerializeModes mode)
		{
			using FileStream fileStream = new FileStream(filepath, FileMode.Create);
			switch (mode)
			{
			case SerializeModes.Binary:
				new BinaryFormatter().Serialize(fileStream, obj);
				break;
			case SerializeModes.XML:
				new XmlSerializer(typeof(T)).Serialize(fileStream, obj);
				break;
			}
		}

		public static bool SafeSerializeToFile<T>(T obj, string filepath, SerializeModes mode)
		{
			try
			{
				SerializeToFile(obj, filepath, mode);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static T DeserializeFromFile<T>(string filepath, SerializeModes mode)
		{
			using FileStream fileStream = File.OpenRead(filepath);
			if (mode == SerializeModes.Binary)
			{
				return (T)new BinaryFormatter().Deserialize(fileStream);
			}
			return (T)new XmlSerializer(typeof(T)).Deserialize(fileStream);
		}

		public static T SafeDeserializeFromFile<T>(string filepath, SerializeModes mode, bool debugUnsafe = false)
		{
			if (File.Exists(filepath))
			{
				if (debugUnsafe)
				{
					return DeserializeFromFile<T>(filepath, mode);
				}
				try
				{
					return DeserializeFromFile<T>(filepath, mode);
				}
				catch
				{
					return default(T);
				}
			}
			return default(T);
		}

		public static T SafeDeserializeFromFile<T>(string filepath, SerializeModes mode, out bool loadError, bool debugUnsafe = false)
		{
			if (File.Exists(filepath))
			{
				if (debugUnsafe)
				{
					loadError = false;
					return DeserializeFromFile<T>(filepath, mode);
				}
				try
				{
					loadError = false;
					return DeserializeFromFile<T>(filepath, mode);
				}
				catch
				{
					loadError = true;
					return default(T);
				}
			}
			loadError = false;
			return default(T);
		}
	}
}
