using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Celeste
{
	public static class BinaryPacker
	{
		public class Element
		{
			public string Package;

			public string Name;

			public Dictionary<string, object> Attributes;

			public List<Element> Children;

			public bool HasAttr(string name)
			{
				if (Attributes != null)
				{
					return Attributes.ContainsKey(name);
				}
				return false;
			}

			public string Attr(string name, string defaultValue = "")
			{
				if (Attributes == null || !Attributes.TryGetValue(name, out var result))
				{
					result = defaultValue;
				}
				return result.ToString();
			}

			public bool AttrBool(string name, bool defaultValue = false)
			{
				if (Attributes == null || !Attributes.TryGetValue(name, out var result))
				{
					result = defaultValue;
				}
				if (result is bool)
				{
					return (bool)result;
				}
				return bool.Parse(result.ToString());
			}

			public float AttrFloat(string name, float defaultValue = 0f)
			{
				if (Attributes == null || !Attributes.TryGetValue(name, out var result))
				{
					result = defaultValue;
				}
				if (result is float)
				{
					return (float)result;
				}
				return float.Parse(result.ToString(), CultureInfo.InvariantCulture);
			}
		}

		public static readonly HashSet<string> IgnoreAttributes = new HashSet<string> { "_eid" };

		public static string InnerTextAttributeName = "innerText";

		public static string OutputFileExtension = ".bin";

		private static Dictionary<string, short> stringValue = new Dictionary<string, short>();

		private static string[] stringLookup;

		private static short stringCounter;

		public static void ToBinary(string filename, string outdir = null)
		{
			string ext = Path.GetExtension(filename);
			if (outdir != null)
			{
				Path.Combine(outdir + Path.GetFileName(filename));
			}
			filename.Replace(ext, OutputFileExtension);
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(filename);
			XmlElement firstElemenet = null;
			foreach (object element in xmlDocument.ChildNodes)
			{
				if (element is XmlElement)
				{
					firstElemenet = element as XmlElement;
					break;
				}
			}
			ToBinary(firstElemenet, outdir);
		}

		public static void ToBinary(XmlElement rootElement, string outfilename)
		{
			stringValue.Clear();
			stringCounter = 0;
			CreateLookupTable(rootElement);
			AddLookupValue(InnerTextAttributeName);
			using FileStream stream = new FileStream(outfilename, FileMode.Create);
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write("CELESTE MAP");
			writer.Write(Path.GetFileNameWithoutExtension(outfilename));
			writer.Write((short)stringValue.Count);
			foreach (KeyValuePair<string, short> item in stringValue)
			{
				writer.Write(item.Key);
			}
			WriteElement(writer, rootElement);
			writer.Flush();
		}

		private static void CreateLookupTable(XmlElement element)
		{
			AddLookupValue(element.Name);
			foreach (XmlAttribute attribute in element.Attributes)
			{
				if (!IgnoreAttributes.Contains(attribute.Name))
				{
					AddLookupValue(attribute.Name);
					if (ParseValue(attribute.Value, out var type, out var _) && type == 5)
					{
						AddLookupValue(attribute.Value);
					}
				}
			}
			foreach (object node in element.ChildNodes)
			{
				if (node is XmlElement)
				{
					CreateLookupTable(node as XmlElement);
				}
			}
		}

		private static void AddLookupValue(string name)
		{
			if (!stringValue.ContainsKey(name))
			{
				stringValue.Add(name, stringCounter);
				stringCounter++;
			}
		}

		private static void WriteElement(BinaryWriter writer, XmlElement element)
		{
			int children = 0;
			foreach (object childNode in element.ChildNodes)
			{
				if (childNode is XmlElement)
				{
					children++;
				}
			}
			int attributes = 0;
			foreach (XmlAttribute attribute2 in element.Attributes)
			{
				if (!IgnoreAttributes.Contains(attribute2.Name))
				{
					attributes++;
				}
			}
			if (element.InnerText.Length > 0 && children == 0)
			{
				attributes++;
			}
			writer.Write(stringValue[element.Name]);
			writer.Write((byte)attributes);
			foreach (XmlAttribute attribute in element.Attributes)
			{
				if (!IgnoreAttributes.Contains(attribute.Name))
				{
					ParseValue(attribute.Value, out var type, out var value);
					writer.Write(stringValue[attribute.Name]);
					writer.Write(type);
					switch (type)
					{
					case 0:
						writer.Write((bool)value);
						break;
					case 1:
						writer.Write((byte)value);
						break;
					case 2:
						writer.Write((short)value);
						break;
					case 3:
						writer.Write((int)value);
						break;
					case 4:
						writer.Write((float)value);
						break;
					case 5:
						writer.Write(stringValue[(string)value]);
						break;
					}
				}
			}
			if (element.InnerText.Length > 0 && children == 0)
			{
				writer.Write(stringValue[InnerTextAttributeName]);
				if (element.Name == "solids" || element.Name == "bg")
				{
					byte[] bytes = RunLengthEncoding.Encode(element.InnerText);
					writer.Write((byte)7);
					writer.Write((short)bytes.Length);
					writer.Write(bytes);
				}
				else
				{
					writer.Write((byte)6);
					writer.Write(element.InnerText);
				}
			}
			writer.Write((short)children);
			foreach (object node in element.ChildNodes)
			{
				if (node is XmlElement)
				{
					WriteElement(writer, node as XmlElement);
				}
			}
		}

		private static bool ParseValue(string value, out byte type, out object result)
		{
			byte byteValue;
			short shortValue;
			int intValue;
			float floatValue;
			if (bool.TryParse(value, out var boolValue))
			{
				type = 0;
				result = boolValue;
			}
			else if (byte.TryParse(value, out byteValue))
			{
				type = 1;
				result = byteValue;
			}
			else if (short.TryParse(value, out shortValue))
			{
				type = 2;
				result = shortValue;
			}
			else if (int.TryParse(value, out intValue))
			{
				type = 3;
				result = intValue;
			}
			else if (float.TryParse(value, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out floatValue))
			{
				type = 4;
				result = floatValue;
			}
			else
			{
				type = 5;
				result = value;
			}
			return true;
		}

		public static Element FromBinary(string filename)
		{
			using FileStream stream = File.OpenRead(filename);
			BinaryReader reader = new BinaryReader(stream);
			reader.ReadString();
			string package = reader.ReadString();
			short stringCount = reader.ReadInt16();
			stringLookup = new string[stringCount];
			for (int i = 0; i < stringCount; i++)
			{
				stringLookup[i] = reader.ReadString();
			}
			Element element = ReadElement(reader);
			element.Package = package;
			return element;
		}

		private static Element ReadElement(BinaryReader reader)
		{
			Element element = new Element();
			element.Name = stringLookup[reader.ReadInt16()];
			byte attributes = reader.ReadByte();
			if (attributes > 0)
			{
				element.Attributes = new Dictionary<string, object>();
			}
			for (int j = 0; j < attributes; j++)
			{
				string key = stringLookup[reader.ReadInt16()];
				byte valueType = reader.ReadByte();
				object value = null;
				switch (valueType)
				{
				case 0:
					value = reader.ReadBoolean();
					break;
				case 1:
					value = Convert.ToInt32(reader.ReadByte());
					break;
				case 2:
					value = Convert.ToInt32(reader.ReadInt16());
					break;
				case 3:
					value = reader.ReadInt32();
					break;
				case 4:
					value = reader.ReadSingle();
					break;
				case 5:
					value = stringLookup[reader.ReadInt16()];
					break;
				case 6:
					value = reader.ReadString();
					break;
				case 7:
				{
					short length = reader.ReadInt16();
					value = RunLengthEncoding.Decode(reader.ReadBytes(length));
					break;
				}
				}
				element.Attributes.Add(key, value);
			}
			short children = reader.ReadInt16();
			if (children > 0)
			{
				element.Children = new List<Element>();
			}
			for (int i = 0; i < children; i++)
			{
				element.Children.Add(ReadElement(reader));
			}
			return element;
		}
	}
}
