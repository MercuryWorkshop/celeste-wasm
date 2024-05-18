using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class FancyText
	{
		public class Node
		{
		}

		public class Char : Node
		{
			public int Index;

			public int Character;

			public float Position;

			public int Line;

			public int Page;

			public float Delay;

			public float LineWidth;

			public Color Color;

			public float Scale;

			public float Rotation;

			public float YOffset;

			public float Fade;

			public bool Shake;

			public bool Wave;

			public bool Impact;

			public bool IsPunctuation;

			public void Draw(PixelFont font, float baseSize, Vector2 position, Vector2 scale, float alpha)
			{
				float scaled = (Impact ? (2f - Fade) : 1f) * Scale;
				Vector2 offset = Vector2.Zero;
				Vector2 fullscale = scale * scaled;
				PixelFontSize size = font.Get(baseSize * Math.Max(fullscale.X, fullscale.Y));
				PixelFontCharacter character = size.Get(Character);
				fullscale *= baseSize / size.Size;
				position.X += Position * scale.X;
				offset += (Shake ? (new Vector2(-1 + Calc.Random.Next(3), -1 + Calc.Random.Next(3)) * 2f) : Vector2.Zero);
				offset += (Wave ? new Vector2(0f, (float)Math.Sin((float)Index * 0.25f + Engine.Scene.RawTimeActive * 8f) * 4f) : Vector2.Zero);
				offset.X += character.XOffset;
				offset.Y += (float)character.YOffset + (-8f * (1f - Fade) + YOffset * Fade);
				character.Texture.Draw(position + offset * fullscale, Vector2.Zero, Color * Fade * alpha, fullscale, Rotation);
			}
		}

		public class Portrait : Node
		{
			public int Side;

			public string Sprite;

			public string Animation;

			public bool UpsideDown;

			public bool Flipped;

			public bool Pop;

			public bool Glitchy;

			public string SfxEvent;

			public int SfxExpression = 1;

			public string SpriteId => "portrait_" + Sprite;

			public string BeginAnimation => "begin_" + Animation;

			public string IdleAnimation => "idle_" + Animation;

			public string TalkAnimation => "talk_" + Animation;
		}

		public class Wait : Node
		{
			public float Duration;
		}

		public class Trigger : Node
		{
			public int Index;

			public bool Silent;

			public string Label;
		}

		public class NewLine : Node
		{
		}

		public class NewPage : Node
		{
		}

		public enum Anchors
		{
			Top,
			Middle,
			Bottom
		}

		public class Anchor : Node
		{
			public Anchors Position;
		}

		public class Text
		{
			public List<Node> Nodes;

			public int Lines;

			public int Pages;

			public PixelFont Font;

			public float BaseSize;

			public int Count => Nodes.Count;

			public Node this[int index] => Nodes[index];

			public int GetCharactersOnPage(int start)
			{
				int characters = 0;
				for (int i = start; i < Count; i++)
				{
					if (Nodes[i] is Char)
					{
						characters++;
					}
					else if (Nodes[i] is NewPage)
					{
						break;
					}
				}
				return characters;
			}

			public int GetNextPageStart(int start)
			{
				for (int i = start; i < Count; i++)
				{
					if (Nodes[i] is NewPage)
					{
						return i + 1;
					}
				}
				return Nodes.Count;
			}

			public float WidestLine()
			{
				int widest = 0;
				for (int i = 0; i < Nodes.Count; i++)
				{
					if (Nodes[i] is Char)
					{
						widest = Math.Max(widest, (int)(Nodes[i] as Char).LineWidth);
					}
				}
				return widest;
			}

			public void Draw(Vector2 position, Vector2 justify, Vector2 scale, float alpha, int start = 0, int end = int.MaxValue)
			{
				int to = Math.Min(Nodes.Count, end);
				int widest = 0;
				float highest = 0f;
				float height = 0f;
				PixelFontSize size = Font.Get(BaseSize);
				for (int j = start; j < to; j++)
				{
					if (Nodes[j] is NewLine)
					{
						if (highest == 0f)
						{
							highest = 1f;
						}
						height += highest;
						highest = 0f;
					}
					else if (Nodes[j] is Char)
					{
						widest = Math.Max(widest, (int)(Nodes[j] as Char).LineWidth);
						highest = Math.Max(highest, (Nodes[j] as Char).Scale);
					}
					else if (Nodes[j] is NewPage)
					{
						break;
					}
				}
				height += highest;
				position -= justify * new Vector2(widest, height * (float)size.LineHeight) * scale;
				highest = 0f;
				for (int i = start; i < to && !(Nodes[i] is NewPage); i++)
				{
					if (Nodes[i] is NewLine)
					{
						if (highest == 0f)
						{
							highest = 1f;
						}
						position.Y += (float)size.LineHeight * highest * scale.Y;
						highest = 0f;
					}
					if (Nodes[i] is Char)
					{
						Char c = Nodes[i] as Char;
						c.Draw(Font, BaseSize, position, scale, alpha);
						highest = Math.Max(highest, c.Scale);
					}
				}
			}

			public void DrawJustifyPerLine(Vector2 position, Vector2 justify, Vector2 scale, float alpha, int start = 0, int end = int.MaxValue)
			{
				int to = Math.Min(Nodes.Count, end);
				float highest = 0f;
				float height = 0f;
				PixelFontSize size = Font.Get(BaseSize);
				for (int j = start; j < to; j++)
				{
					if (Nodes[j] is NewLine)
					{
						if (highest == 0f)
						{
							highest = 1f;
						}
						height += highest;
						highest = 0f;
					}
					else if (Nodes[j] is Char)
					{
						highest = Math.Max(highest, (Nodes[j] as Char).Scale);
					}
					else if (Nodes[j] is NewPage)
					{
						break;
					}
				}
				height += highest;
				highest = 0f;
				for (int i = start; i < to && !(Nodes[i] is NewPage); i++)
				{
					if (Nodes[i] is NewLine)
					{
						if (highest == 0f)
						{
							highest = 1f;
						}
						position.Y += highest * (float)size.LineHeight * scale.Y;
						highest = 0f;
					}
					if (Nodes[i] is Char)
					{
						Char c = Nodes[i] as Char;
						Vector2 offset = -justify * new Vector2(c.LineWidth, height * (float)size.LineHeight) * scale;
						c.Draw(Font, BaseSize, position + offset, scale, alpha);
						highest = Math.Max(highest, c.Scale);
					}
				}
			}
		}

		public static Color DefaultColor = Color.LightGray;

		public const float CharacterDelay = 0.01f;

		public const float PeriodDelay = 0.3f;

		public const float CommaDelay = 0.15f;

		public const float ShakeDistance = 2f;

		private Language language;

		private string text;

		private Text group = new Text();

		private int maxLineWidth;

		private int linesPerPage;

		private PixelFont font;

		private PixelFontSize size;

		private Color defaultColor;

		private float startFade;

		private int currentLine;

		private int currentPage;

		private float currentPosition;

		private Color currentColor;

		private float currentScale = 1f;

		private float currentDelay = 0.01f;

		private bool currentShake;

		private bool currentWave;

		private bool currentImpact;

		private bool currentMessedUp;

		private int currentCharIndex;

		public static Text Parse(string text, int maxLineWidth, int linesPerPage, float startFade = 1f, Color? defaultColor = null, Language language = null)
		{
			return new FancyText(text, maxLineWidth, linesPerPage, startFade, defaultColor.HasValue ? defaultColor.Value : DefaultColor, language).Parse();
		}

		private FancyText(string text, int maxLineWidth, int linesPerPage, float startFade, Color defaultColor, Language language)
		{
			this.text = text;
			this.maxLineWidth = maxLineWidth;
			this.linesPerPage = ((linesPerPage < 0) ? int.MaxValue : linesPerPage);
			this.startFade = startFade;
			this.defaultColor = (currentColor = defaultColor);
			if (language == null)
			{
				language = Dialog.Language;
			}
			this.language = language;
			group.Nodes = new List<Node>();
			group.Font = (font = Fonts.Get(language.FontFace));
			group.BaseSize = language.FontFaceSize;
			size = font.Get(group.BaseSize);
		}

		private Text Parse()
		{
			string[] splits = Regex.Split(text, language.SplitRegex);
			string[] words = new string[splits.Length];
			int length = 0;
			for (int j = 0; j < splits.Length; j++)
			{
				if (!string.IsNullOrEmpty(splits[j]))
				{
					words[length++] = splits[j];
				}
			}
			Stack<Color> colorStack = new Stack<Color>();
			Portrait[] lastPortrait = new Portrait[2];
			for (int i = 0; i < length; i++)
			{
				if (words[i] == "{")
				{
					i++;
					string command = words[i++];
					List<string> args = new List<string>();
					for (; i < words.Length && words[i] != "}"; i++)
					{
						if (!string.IsNullOrWhiteSpace(words[i]))
						{
							args.Add(words[i]);
						}
					}
					float delay = 0f;
					if (float.TryParse(command, NumberStyles.Float, CultureInfo.InvariantCulture, out delay))
					{
						group.Nodes.Add(new Wait
						{
							Duration = delay
						});
						continue;
					}
					if (command[0] == '#')
					{
						string color = "";
						if (command.Length > 1)
						{
							color = command.Substring(1);
						}
						else if (args.Count > 0)
						{
							color = args[0];
						}
						if (string.IsNullOrEmpty(color))
						{
							if (colorStack.Count > 0)
							{
								currentColor = colorStack.Pop();
							}
							else
							{
								currentColor = defaultColor;
							}
							continue;
						}
						colorStack.Push(currentColor);
						switch (color)
						{
						case "red":
							currentColor = Color.Red;
							break;
						case "green":
							currentColor = Color.Green;
							break;
						case "blue":
							currentColor = Color.Blue;
							break;
						default:
							currentColor = Calc.HexToColor(color);
							break;
						}
						continue;
					}
					switch (command)
					{
					case "break":
						CalcLineWidth();
						currentPage++;
						group.Pages++;
						currentLine = 0;
						currentPosition = 0f;
						group.Nodes.Add(new NewPage());
						continue;
					case "n":
						AddNewLine();
						continue;
					case ">>":
					{
						if (args.Count > 0 && float.TryParse(args[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var speed))
						{
							currentDelay = 0.01f / speed;
						}
						else
						{
							currentDelay = 0.01f;
						}
						continue;
					}
					}
					if (command.Equals("/>>"))
					{
						currentDelay = 0.01f;
						continue;
					}
					if (command.Equals("anchor"))
					{
						if (Enum.TryParse<Anchors>(args[0], ignoreCase: true, out var anchor))
						{
							group.Nodes.Add(new Anchor
							{
								Position = anchor
							});
						}
						continue;
					}
					if (command.Equals("portrait") || command.Equals("left") || command.Equals("right"))
					{
						Portrait nextPortrait;
						if (command.Equals("portrait") && args.Count > 0 && args[0].Equals("none"))
						{
							nextPortrait = new Portrait();
							group.Nodes.Add(nextPortrait);
							continue;
						}
						if (command.Equals("left"))
						{
							nextPortrait = lastPortrait[0];
						}
						else if (command.Equals("right"))
						{
							nextPortrait = lastPortrait[1];
						}
						else
						{
							nextPortrait = new Portrait();
							foreach (string arg in args)
							{
								if (arg.Equals("upsidedown"))
								{
									nextPortrait.UpsideDown = true;
								}
								else if (arg.Equals("flip"))
								{
									nextPortrait.Flipped = true;
								}
								else if (arg.Equals("left"))
								{
									nextPortrait.Side = -1;
								}
								else if (arg.Equals("right"))
								{
									nextPortrait.Side = 1;
								}
								else if (arg.Equals("pop"))
								{
									nextPortrait.Pop = true;
								}
								else if (nextPortrait.Sprite == null)
								{
									nextPortrait.Sprite = arg;
								}
								else
								{
									nextPortrait.Animation = arg;
								}
							}
						}
						if (GFX.PortraitsSpriteBank.Has(nextPortrait.SpriteId))
						{
							List<SpriteDataSource> sources = GFX.PortraitsSpriteBank.SpriteData[nextPortrait.SpriteId].Sources;
							for (int s = sources.Count - 1; s >= 0; s--)
							{
								XmlElement xml = sources[s].XML;
								if (xml != null)
								{
									if (nextPortrait.SfxEvent == null)
									{
										nextPortrait.SfxEvent = "event:/char/dialogue/" + xml.Attr("sfx", "");
									}
									if (xml.HasAttr("glitchy"))
									{
										nextPortrait.Glitchy = xml.AttrBool("glitchy", defaultValue: false);
									}
									if (xml.HasChild("sfxs") && nextPortrait.SfxExpression == 1)
									{
										foreach (object item in xml["sfxs"])
										{
											if (item is XmlElement element && element.Name.Equals(nextPortrait.Animation, StringComparison.InvariantCultureIgnoreCase))
											{
												nextPortrait.SfxExpression = element.AttrInt("index");
												break;
											}
										}
									}
								}
							}
						}
						group.Nodes.Add(nextPortrait);
						lastPortrait[(nextPortrait.Side > 0) ? 1u : 0u] = nextPortrait;
						continue;
					}
					if (command.Equals("trigger") || command.Equals("silent_trigger"))
					{
						string label = "";
						for (int k = 1; k < args.Count; k++)
						{
							label = label + args[k] + " ";
						}
						if (int.TryParse(args[0], out var trigger) && trigger >= 0)
						{
							group.Nodes.Add(new Trigger
							{
								Index = trigger,
								Silent = command.StartsWith("silent"),
								Label = label
							});
						}
						continue;
					}
					if (command.Equals("*"))
					{
						currentShake = true;
						continue;
					}
					if (command.Equals("/*"))
					{
						currentShake = false;
						continue;
					}
					if (command.Equals("~"))
					{
						currentWave = true;
						continue;
					}
					if (command.Equals("/~"))
					{
						currentWave = false;
						continue;
					}
					if (command.Equals("!"))
					{
						currentImpact = true;
						continue;
					}
					if (command.Equals("/!"))
					{
						currentImpact = false;
						continue;
					}
					if (command.Equals("%"))
					{
						currentMessedUp = true;
						continue;
					}
					if (command.Equals("/%"))
					{
						currentMessedUp = false;
						continue;
					}
					if (command.Equals("big"))
					{
						currentScale = 1.5f;
						continue;
					}
					if (command.Equals("/big"))
					{
						currentScale = 1f;
						continue;
					}
					if (command.Equals("s"))
					{
						int push = 1;
						if (args.Count > 0)
						{
							int.TryParse(args[0], out push);
						}
						currentPosition += 5 * push;
						continue;
					}
					if (!command.Equals("savedata"))
					{
						continue;
					}
					if (SaveData.Instance == null)
					{
						if (args[0].Equals("name", StringComparison.OrdinalIgnoreCase))
						{
							AddWord("Madeline");
						}
						else
						{
							AddWord("[SD:" + args[0] + "]");
						}
					}
					else if (args[0].Equals("name", StringComparison.OrdinalIgnoreCase))
					{
						if (!language.CanDisplay(SaveData.Instance.Name))
						{
							AddWord(Dialog.Clean("FILE_DEFAULT", language));
						}
						else
						{
							AddWord(SaveData.Instance.Name);
						}
					}
					else
					{
						FieldInfo field = typeof(SaveData).GetField(args[0]);
						AddWord(field.GetValue(SaveData.Instance).ToString());
					}
				}
				else
				{
					AddWord(words[i]);
				}
			}
			CalcLineWidth();
			return group;
		}

		private void CalcLineWidth()
		{
			Char last = null;
			int i = group.Nodes.Count - 1;
			while (i >= 0 && last == null)
			{
				if (group.Nodes[i] is Char)
				{
					last = group.Nodes[i] as Char;
				}
				else if (group.Nodes[i] is NewLine || group.Nodes[i] is NewPage)
				{
					return;
				}
				i--;
			}
			if (last == null)
			{
				return;
			}
			float width = (last.LineWidth = last.Position + (float)size.Get(last.Character).XAdvance * last.Scale);
			while (i >= 0 && !(group.Nodes[i] is NewLine) && !(group.Nodes[i] is NewPage))
			{
				if (group.Nodes[i] is Char)
				{
					(group.Nodes[i] as Char).LineWidth = width;
				}
				i--;
			}
		}

		private void AddNewLine()
		{
			CalcLineWidth();
			currentLine++;
			currentPosition = 0f;
			group.Lines++;
			if (currentLine > linesPerPage)
			{
				group.Pages++;
				currentPage++;
				currentLine = 0;
				group.Nodes.Add(new NewPage());
			}
			else
			{
				group.Nodes.Add(new NewLine());
			}
		}

		private void AddWord(string word)
		{
			float wordWidth = size.Measure(word).X * currentScale;
			if (currentPosition + wordWidth > (float)maxLineWidth)
			{
				AddNewLine();
			}
			for (int i = 0; i < word.Length; i++)
			{
				if ((currentPosition == 0f && word[i] == ' ') || word[i] == '\\')
				{
					continue;
				}
				PixelFontCharacter data = size.Get(word[i]);
				if (data == null)
				{
					continue;
				}
				float addDelay = 0f;
				if (i == word.Length - 1 && (i == 0 || word[i - 1] != '\\'))
				{
					if (Contains(language.CommaCharacters, word[i]))
					{
						addDelay = 0.15f;
					}
					else if (Contains(language.PeriodCharacters, word[i]))
					{
						addDelay = 0.3f;
					}
				}
				group.Nodes.Add(new Char
				{
					Index = currentCharIndex++,
					Character = word[i],
					Position = currentPosition,
					Line = currentLine,
					Page = currentPage,
					Delay = (currentImpact ? 0.0034999999f : (currentDelay + addDelay)),
					Color = currentColor,
					Scale = currentScale,
					Rotation = (currentMessedUp ? ((float)Calc.Random.Choose(-1, 1) * Calc.Random.Choose(0.17453292f, 0.34906584f)) : 0f),
					YOffset = (currentMessedUp ? ((float)Calc.Random.Choose(-3, -6, 3, 6)) : 0f),
					Fade = startFade,
					Shake = currentShake,
					Impact = currentImpact,
					Wave = currentWave,
					IsPunctuation = (Contains(language.CommaCharacters, word[i]) || Contains(language.PeriodCharacters, word[i]))
				});
				currentPosition += (float)data.XAdvance * currentScale;
				if (i < word.Length - 1 && data.Kerning.TryGetValue(word[i], out var kerning))
				{
					currentPosition += (float)kerning * currentScale;
				}
			}
		}

		private bool Contains(string str, char character)
		{
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] == character)
				{
					return true;
				}
			}
			return false;
		}
	}
}
