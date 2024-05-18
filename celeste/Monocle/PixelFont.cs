using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;

namespace Monocle
{
	public class PixelFont
	{
		public string Face;

		public List<PixelFontSize> Sizes = new List<PixelFontSize>();

		private List<VirtualTexture> managedTextures = new List<VirtualTexture>();

		public PixelFont(string face)
		{
			Face = face;
		}

		public PixelFontSize AddFontSize(string path, Atlas atlas = null, bool outline = false)
		{
			XmlElement data = Calc.LoadXML(path)["font"];
			return AddFontSize(path, data, atlas, outline);
		}

		public PixelFontSize AddFontSize(string path, XmlElement data, Atlas atlas = null, bool outline = false)
		{
			float size = data["info"].AttrFloat("size");
			foreach (PixelFontSize fs in Sizes)
			{
				if (fs.Size == size)
				{
					return fs;
				}
			}
			List<MTexture> textures = new List<MTexture>();
			foreach (XmlElement item in data["pages"])
			{
				string file = item.Attr("file");
				string atlasPath = Path.GetFileNameWithoutExtension(file);
				if (atlas != null && atlas.Has(atlasPath))
				{
					textures.Add(atlas[atlasPath]);
					continue;
				}
				VirtualTexture tex = VirtualContent.CreateTexture(Path.Combine(Path.GetDirectoryName(path).Substring(Engine.ContentDirectory.Length + 1), file));
				textures.Add(new MTexture(tex));
				managedTextures.Add(tex);
			}
			PixelFontSize fontSize = new PixelFontSize
			{
				Textures = textures,
				Characters = new Dictionary<int, PixelFontCharacter>(),
				LineHeight = data["common"].AttrInt("lineHeight"),
				Size = size,
				Outline = outline
			};
			foreach (XmlElement character in data["chars"])
			{
				int id = character.AttrInt("id");
				int page = character.AttrInt("page", 0);
				fontSize.Characters.Add(id, new PixelFontCharacter(id, textures[page], character));
			}
			if (data["kernings"] != null)
			{
				foreach (XmlElement item2 in data["kernings"])
				{
					int from = item2.AttrInt("first");
					int to = item2.AttrInt("second");
					int push = item2.AttrInt("amount");
					PixelFontCharacter c = null;
					if (fontSize.Characters.TryGetValue(from, out c))
					{
						c.Kerning.Add(to, push);
					}
				}
			}
			Sizes.Add(fontSize);
			Sizes.Sort((PixelFontSize a, PixelFontSize b) => Math.Sign(a.Size - b.Size));
			return fontSize;
		}

		public PixelFontSize Get(float size)
		{
			int i = 0;
			for (int j = Sizes.Count - 1; i < j; i++)
			{
				if (Sizes[i].Size >= size)
				{
					return Sizes[i];
				}
			}
			return Sizes[Sizes.Count - 1];
		}

		public bool Has(float size)
		{
			int i = 0;
			for (int j = Sizes.Count - 1; i < j; i++)
			{
				if (Sizes[i].Size == size)
				{
					return true;
				}
			}
			return false;
		}

		public void Draw(float baseSize, char character, Vector2 position, Vector2 justify, Vector2 scale, Color color)
		{
			PixelFontSize fontSize = Get(baseSize * Math.Max(scale.X, scale.Y));
			scale *= baseSize / fontSize.Size;
			fontSize.Draw(character, position, justify, scale, color);
		}

		public void Draw(float baseSize, string text, Vector2 position, Vector2 justify, Vector2 scale, Color color, float edgeDepth, Color edgeColor, float stroke, Color strokeColor)
		{
			PixelFontSize fontSize = Get(baseSize * Math.Max(scale.X, scale.Y));
			scale *= baseSize / fontSize.Size;
			fontSize.Draw(text, position, justify, scale, color, edgeDepth, edgeColor, stroke, strokeColor);
		}

		public void Draw(float baseSize, string text, Vector2 position, Color color)
		{
			Vector2 scale = Vector2.One;
			PixelFontSize fontSize = Get(baseSize * Math.Max(scale.X, scale.Y));
			scale *= baseSize / fontSize.Size;
			fontSize.Draw(text, position, Vector2.Zero, Vector2.One, color, 0f, Color.Transparent, 0f, Color.Transparent);
		}

		public void Draw(float baseSize, string text, Vector2 position, Vector2 justify, Vector2 scale, Color color)
		{
			PixelFontSize fontSize = Get(baseSize * Math.Max(scale.X, scale.Y));
			scale *= baseSize / fontSize.Size;
			fontSize.Draw(text, position, justify, scale, color, 0f, Color.Transparent, 0f, Color.Transparent);
		}

		public void DrawOutline(float baseSize, string text, Vector2 position, Vector2 justify, Vector2 scale, Color color, float stroke, Color strokeColor)
		{
			PixelFontSize fontSize = Get(baseSize * Math.Max(scale.X, scale.Y));
			scale *= baseSize / fontSize.Size;
			fontSize.Draw(text, position, justify, scale, color, 0f, Color.Transparent, stroke, strokeColor);
		}

		public void DrawEdgeOutline(float baseSize, string text, Vector2 position, Vector2 justify, Vector2 scale, Color color, float edgeDepth, Color edgeColor, float stroke = 0f, Color strokeColor = default(Color))
		{
			PixelFontSize fontSize = Get(baseSize * Math.Max(scale.X, scale.Y));
			scale *= baseSize / fontSize.Size;
			fontSize.Draw(text, position, justify, scale, color, edgeDepth, edgeColor, stroke, strokeColor);
		}

		public void Dispose()
		{
			foreach (VirtualTexture managedTexture in managedTextures)
			{
				managedTexture.Dispose();
			}
			Sizes.Clear();
		}
	}
}
