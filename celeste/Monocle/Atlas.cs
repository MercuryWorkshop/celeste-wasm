using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;

namespace Monocle
{
	public class Atlas
	{
		public enum AtlasDataFormat
		{
			TexturePacker_Sparrow,
			CrunchXml,
			CrunchBinary,
			CrunchXmlOrBinary,
			CrunchBinaryNoAtlas,
			Packer,
			PackerNoAtlas
		}

		public List<VirtualTexture> Sources;

		private Dictionary<string, MTexture> textures = new Dictionary<string, MTexture>(StringComparer.OrdinalIgnoreCase);

		private Dictionary<string, List<MTexture>> orderedTexturesCache = new Dictionary<string, List<MTexture>>();

		private Dictionary<string, string> links = new Dictionary<string, string>();

		public MTexture this[string id]
		{
			get
			{
				return textures[id];
			}
			set
			{
				textures[id] = value;
			}
		}

		public static Atlas FromAtlas(string path, AtlasDataFormat format)
		{
			Atlas obj = new Atlas
			{
				Sources = new List<VirtualTexture>()
			};
			ReadAtlasData(obj, path, format);
			return obj;
		}

		private static void ReadAtlasData(Atlas atlas, string path, AtlasDataFormat format)
		{
			switch (format)
			{
			case AtlasDataFormat.TexturePacker_Sparrow:
			{
				XmlElement xmlElement = Calc.LoadContentXML(path)["TextureAtlas"];
				VirtualTexture texture = VirtualContent.CreateTexture(Path.Combine(path2: xmlElement.Attr("imagePath", ""), path1: Path.GetDirectoryName(path)));
				MTexture mTexture = new MTexture(texture);
				atlas.Sources.Add(texture);
				{
					foreach (XmlElement sub in xmlElement.GetElementsByTagName("SubTexture"))
					{
						string name = sub.Attr("name");
						Rectangle clipRect = sub.Rect();
						if (sub.HasAttr("frameX"))
						{
							atlas.textures[name] = new MTexture(mTexture, name, clipRect, new Vector2(-sub.AttrInt("frameX"), -sub.AttrInt("frameY")), sub.AttrInt("frameWidth"), sub.AttrInt("frameHeight"));
						}
						else
						{
							atlas.textures[name] = new MTexture(mTexture, name, clipRect);
						}
					}
					break;
				}
			}
			case AtlasDataFormat.CrunchXml:
			{
				foreach (XmlElement item in Calc.LoadContentXML(path)["atlas"])
				{
					VirtualTexture texture2 = VirtualContent.CreateTexture(Path.Combine(path2: item.Attr("n", "") + ".png", path1: Path.GetDirectoryName(path)));
					MTexture mTexture2 = new MTexture(texture2);
					atlas.Sources.Add(texture2);
					foreach (XmlElement sub2 in item)
					{
						string name2 = sub2.Attr("n");
						Rectangle clipRect2 = new Rectangle(sub2.AttrInt("x"), sub2.AttrInt("y"), sub2.AttrInt("w"), sub2.AttrInt("h"));
						if (sub2.HasAttr("fx"))
						{
							atlas.textures[name2] = new MTexture(mTexture2, name2, clipRect2, new Vector2(-sub2.AttrInt("fx"), -sub2.AttrInt("fy")), sub2.AttrInt("fw"), sub2.AttrInt("fh"));
						}
						else
						{
							atlas.textures[name2] = new MTexture(mTexture2, name2, clipRect2);
						}
					}
				}
				break;
			}
			case AtlasDataFormat.CrunchBinary:
			{
				using FileStream input2 = File.OpenRead(Path.Combine(Engine.ContentDirectory, path));
				BinaryReader reader = new BinaryReader(input2);
				short textures = reader.ReadInt16();
				for (int i = 0; i < textures; i++)
				{
					string textureName = reader.ReadNullTerminatedString();
					VirtualTexture texture3 = VirtualContent.CreateTexture(Path.Combine(Path.GetDirectoryName(path), textureName + ".png"));
					atlas.Sources.Add(texture3);
					MTexture mTexture3 = new MTexture(texture3);
					short subtextures = reader.ReadInt16();
					for (int j2 = 0; j2 < subtextures; j2++)
					{
						string name3 = reader.ReadNullTerminatedString();
						short x = reader.ReadInt16();
						short y = reader.ReadInt16();
						short w = reader.ReadInt16();
						short h = reader.ReadInt16();
						short fx = reader.ReadInt16();
						short fy = reader.ReadInt16();
						short fw = reader.ReadInt16();
						short fh = reader.ReadInt16();
						atlas.textures[name3] = new MTexture(mTexture3, name3, new Rectangle(x, y, w, h), new Vector2(-fx, -fy), fw, fh);
					}
				}
				break;
			}
			case AtlasDataFormat.CrunchBinaryNoAtlas:
			{
				using FileStream input = File.OpenRead(Path.Combine(Engine.ContentDirectory, path + ".bin"));
				BinaryReader reader2 = new BinaryReader(input);
				short folders = reader2.ReadInt16();
				for (int j = 0; j < folders; j++)
				{
					string folderName = reader2.ReadNullTerminatedString();
					string folderPath = Path.Combine(Path.GetDirectoryName(path), folderName);
					short subtextures2 = reader2.ReadInt16();
					for (int j3 = 0; j3 < subtextures2; j3++)
					{
						string name4 = reader2.ReadNullTerminatedString();
						reader2.ReadInt16();
						reader2.ReadInt16();
						reader2.ReadInt16();
						reader2.ReadInt16();
						short fx2 = reader2.ReadInt16();
						short fy2 = reader2.ReadInt16();
						short fw2 = reader2.ReadInt16();
						short fh2 = reader2.ReadInt16();
						VirtualTexture texture4 = VirtualContent.CreateTexture(Path.Combine(folderPath, name4 + ".png"));
						atlas.Sources.Add(texture4);
						atlas.textures[name4] = new MTexture(texture4, new Vector2(-fx2, -fy2), fw2, fh2);
					}
				}
				break;
			}
			case AtlasDataFormat.Packer:
			{
				using FileStream fileStream = File.OpenRead(Path.Combine(Engine.ContentDirectory, path + ".meta"));
				BinaryReader reader3 = new BinaryReader(fileStream);
				reader3.ReadInt32();
				reader3.ReadString();
				reader3.ReadInt32();
				short textures2 = reader3.ReadInt16();
				for (int l = 0; l < textures2; l++)
				{
					string textureName2 = reader3.ReadString();
					VirtualTexture texture5 = VirtualContent.CreateTexture(Path.Combine(Path.GetDirectoryName(path), textureName2 + ".data"));
					atlas.Sources.Add(texture5);
					MTexture mTexture4 = new MTexture(texture5);
					short subtextures3 = reader3.ReadInt16();
					for (int j4 = 0; j4 < subtextures3; j4++)
					{
						string name5 = reader3.ReadString().Replace('\\', '/');
						short x2 = reader3.ReadInt16();
						short y2 = reader3.ReadInt16();
						short w2 = reader3.ReadInt16();
						short h2 = reader3.ReadInt16();
						short fx3 = reader3.ReadInt16();
						short fy3 = reader3.ReadInt16();
						short fw3 = reader3.ReadInt16();
						short fh3 = reader3.ReadInt16();
						atlas.textures[name5] = new MTexture(mTexture4, name5, new Rectangle(x2, y2, w2, h2), new Vector2(-fx3, -fy3), fw3, fh3);
					}
				}
				if (fileStream.Position < fileStream.Length && reader3.ReadString() == "LINKS")
				{
					short links = reader3.ReadInt16();
					for (int k = 0; k < links; k++)
					{
						string key = reader3.ReadString();
						string val = reader3.ReadString();
						atlas.links.Add(key, val);
					}
				}
				break;
			}
			case AtlasDataFormat.PackerNoAtlas:
			{
				using FileStream stream = File.OpenRead(Path.Combine(Engine.ContentDirectory, path + ".meta"));
				BinaryReader reader4 = new BinaryReader(stream);
				reader4.ReadInt32();
				reader4.ReadString();
				reader4.ReadInt32();
				short folders2 = reader4.ReadInt16();
				for (int n = 0; n < folders2; n++)
				{
					string folderName2 = reader4.ReadString();
					string folderPath2 = Path.Combine(Path.GetDirectoryName(path), folderName2);
					short subtextures4 = reader4.ReadInt16();
					for (int j5 = 0; j5 < subtextures4; j5++)
					{
						string name6 = reader4.ReadString().Replace('\\', '/');
						reader4.ReadInt16();
						reader4.ReadInt16();
						reader4.ReadInt16();
						reader4.ReadInt16();
						short fx4 = reader4.ReadInt16();
						short fy4 = reader4.ReadInt16();
						short fw4 = reader4.ReadInt16();
						short fh4 = reader4.ReadInt16();
						VirtualTexture texture6 = VirtualContent.CreateTexture(Path.Combine(folderPath2, name6 + ".data"));
						atlas.Sources.Add(texture6);
						atlas.textures[name6] = new MTexture(texture6, new Vector2(-fx4, -fy4), fw4, fh4);
						atlas.textures[name6].AtlasPath = name6;
					}
				}
				if (stream.Position < stream.Length && reader4.ReadString() == "LINKS")
				{
					short links2 = reader4.ReadInt16();
					for (int m = 0; m < links2; m++)
					{
						string key2 = reader4.ReadString();
						string val2 = reader4.ReadString();
						atlas.links.Add(key2, val2);
					}
				}
				break;
			}
			case AtlasDataFormat.CrunchXmlOrBinary:
				if (File.Exists(Path.Combine(Engine.ContentDirectory, path + ".bin")))
				{
					ReadAtlasData(atlas, path + ".bin", AtlasDataFormat.CrunchBinary);
				}
				else
				{
					ReadAtlasData(atlas, path + ".xml", AtlasDataFormat.CrunchXml);
				}
				break;
			default:
				throw new NotImplementedException();
			}
		}

		public static Atlas FromMultiAtlas(string rootPath, string[] dataPath, AtlasDataFormat format)
		{
			Atlas atlas = new Atlas();
			atlas.Sources = new List<VirtualTexture>();
			for (int i = 0; i < dataPath.Length; i++)
			{
				ReadAtlasData(atlas, Path.Combine(rootPath, dataPath[i]), format);
			}
			return atlas;
		}

		public static Atlas FromMultiAtlas(string rootPath, string filename, AtlasDataFormat format)
		{
			Atlas atlas = new Atlas();
			atlas.Sources = new List<VirtualTexture>();
			int index = 0;
			while (true)
			{
				string dataPath = Path.Combine(rootPath, filename + index + ".xml");
				if (!File.Exists(Path.Combine(Engine.ContentDirectory, dataPath)))
				{
					break;
				}
				ReadAtlasData(atlas, dataPath, format);
				index++;
			}
			return atlas;
		}

		public static Atlas FromDirectory(string path)
		{
			Atlas atlas = new Atlas();
			atlas.Sources = new List<VirtualTexture>();
			string contentDirectory = Engine.ContentDirectory;
			int contentDirectoryLength = contentDirectory.Length;
			string text = Path.Combine(contentDirectory, path);
			int contentPathLength = text.Length;
			string[] files = Directory.GetFiles(text, "*", SearchOption.AllDirectories);
			foreach (string file in files)
			{
				string ext = Path.GetExtension(file);
				if (!(ext != ".png") || !(ext != ".xnb"))
				{
					VirtualTexture texture = VirtualContent.CreateTexture(file.Substring(contentDirectoryLength + 1));
					atlas.Sources.Add(texture);
					string filepath = file.Substring(contentPathLength + 1);
					filepath = filepath.Substring(0, filepath.Length - 4);
					filepath = filepath.Replace('\\', '/');
					atlas.textures.Add(filepath, new MTexture(texture));
				}
			}
			return atlas;
		}

		public bool Has(string id)
		{
			return textures.ContainsKey(id);
		}

		public MTexture GetOrDefault(string id, MTexture defaultTexture)
		{
			if (string.IsNullOrEmpty(id) || !Has(id))
			{
				return defaultTexture;
			}
			return textures[id];
		}

		public List<MTexture> GetAtlasSubtextures(string key)
		{
			if (!orderedTexturesCache.TryGetValue(key, out var list))
			{
				list = new List<MTexture>();
				int index = 0;
				while (true)
				{
					MTexture texture = GetAtlasSubtextureFromAtlasAt(key, index);
					if (texture == null)
					{
						break;
					}
					list.Add(texture);
					index++;
				}
				orderedTexturesCache.Add(key, list);
			}
			return list;
		}

		private MTexture GetAtlasSubtextureFromCacheAt(string key, int index)
		{
			return orderedTexturesCache[key][index];
		}

		private MTexture GetAtlasSubtextureFromAtlasAt(string key, int index)
		{
			if (index == 0 && textures.ContainsKey(key))
			{
				return textures[key];
			}
			string indexString = index.ToString();
			int startLength = indexString.Length;
			while (indexString.Length < startLength + 6)
			{
				if (textures.TryGetValue(key + indexString, out var result))
				{
					return result;
				}
				indexString = "0" + indexString;
			}
			return null;
		}

		public MTexture GetAtlasSubtexturesAt(string key, int index)
		{
			if (orderedTexturesCache.TryGetValue(key, out var list))
			{
				return list[index];
			}
			return GetAtlasSubtextureFromAtlasAt(key, index);
		}

		public MTexture GetLinkedTexture(string key)
		{
			if (key != null && links.TryGetValue(key, out var other) && textures.TryGetValue(other, out var texture))
			{
				return texture;
			}
			return null;
		}

		public void Dispose()
		{
			foreach (VirtualTexture source in Sources)
			{
				source.Dispose();
			}
			Sources.Clear();
			textures.Clear();
		}
	}
}
