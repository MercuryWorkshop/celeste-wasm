using System;
using System.Collections;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste
{
	public class OuiFileNaming : Oui
	{
		public string StartingName;

		public OuiFileSelectSlot FileSlot;

		public const int MinNameLength = 1;

		public const int MaxNameLengthNormal = 12;

		public const int MaxNameLengthJP = 8;

		private string[] letters;

		private int index;

		private int line;

		private float widestLetter;

		private float widestLine;

		private int widestLineCount;

		private bool selectingOptions = true;

		private int optionsIndex;

		private bool hiragana = true;

		private float lineHeight;

		private float lineSpacing;

		private float boxPadding;

		private float optionsScale;

		private string cancel;

		private string space;

		private string backspace;

		private string accept;

		private float cancelWidth;

		private float spaceWidth;

		private float backspaceWidth;

		private float beginWidth;

		private float optionsWidth;

		private float boxWidth;

		private float boxHeight;

		private float pressedTimer;

		private float timer;

		private float ease;

		private Wiggler wiggler;

		private static int[] dakuten_able = new int[40]
		{
			12363, 12365, 12367, 12369, 12371, 12373, 12375, 12377, 12379, 12381,
			12383, 12385, 12388, 12390, 12392, 12399, 12402, 12405, 12408, 12411,
			12459, 12461, 12463, 12465, 12467, 12469, 12471, 12473, 12475, 12477,
			12479, 12481, 12484, 12486, 12488, 12495, 12498, 12501, 12504, 12507
		};

		private static int[] handakuten_able = new int[10] { 12400, 12403, 12406, 12409, 12412, 12496, 12499, 12502, 12505, 12508 };

		private Color unselectColor = Color.LightGray;

		private Color selectColorA = Calc.HexToColor("84FF54");

		private Color selectColorB = Calc.HexToColor("FCFF59");

		private Color disableColor = Color.DarkSlateBlue;

		public string Name
		{
			get
			{
				return FileSlot.Name;
			}
			set
			{
				FileSlot.Name = value;
			}
		}

		public int MaxNameLength
		{
			get
			{
				if (!Japanese)
				{
					return 12;
				}
				return 8;
			}
		}

		public bool Japanese => Settings.Instance.Language == "japanese";

		private Vector2 boxtopleft => Position + new Vector2((1920f - boxWidth) / 2f, 360f + (680f - boxHeight) / 2f);

		public OuiFileNaming()
		{
			wiggler = Wiggler.Create(0.25f, 4f);
			Position = new Vector2(0f, 1080f);
			Visible = false;
		}

		public override IEnumerator Enter(Oui from)
		{
			if (Name == Dialog.Clean("FILE_DEFAULT") || (Settings.Instance != null && Name == Settings.Instance.DefaultFileName))
			{
				Name = "";
			}
			base.Overworld.ShowInputUI = false;
			selectingOptions = false;
			optionsIndex = 0;
			index = 0;
			line = 0;
			ReloadLetters(Dialog.Clean("name_letters"));
			optionsScale = 0.75f;
			cancel = Dialog.Clean("name_back");
			space = Dialog.Clean("name_space");
			backspace = Dialog.Clean("name_backspace");
			accept = Dialog.Clean("name_accept");
			cancelWidth = ActiveFont.Measure(cancel).X * optionsScale;
			spaceWidth = ActiveFont.Measure(space).X * optionsScale;
			backspaceWidth = ActiveFont.Measure(backspace).X * optionsScale;
			beginWidth = ActiveFont.Measure(accept).X * optionsScale * 1.25f;
			optionsWidth = cancelWidth + spaceWidth + backspaceWidth + beginWidth + widestLetter * 3f;
			Visible = true;
			Vector2 posFrom = Position;
			Vector2 posTo = Vector2.Zero;
			for (float t = 0f; t < 1f; t += Engine.DeltaTime * 3f)
			{
				ease = Ease.CubeIn(t);
				Position = posFrom + (posTo - posFrom) * Ease.CubeInOut(t);
				yield return null;
			}
			ease = 1f;
			yield return 0.05f;
			Focused = true;
			yield return 0.05f;
			wiggler.Start();
		}

		private void ReloadLetters(string chars)
		{
			letters = chars.Split('\n');
			widestLetter = 0f;
			for (int j = 0; j < chars.Length; j++)
			{
				float size = ActiveFont.Measure(chars[j]).X;
				if (size > widestLetter)
				{
					widestLetter = size;
				}
			}
			if (Japanese)
			{
				widestLetter *= 1.5f;
			}
			widestLineCount = 0;
			string[] array = letters;
			foreach (string i in array)
			{
				if (i.Length > widestLineCount)
				{
					widestLineCount = i.Length;
				}
			}
			widestLine = (float)widestLineCount * widestLetter;
			lineHeight = ActiveFont.LineHeight;
			lineSpacing = ActiveFont.LineHeight * 0.1f;
			boxPadding = widestLetter;
			boxWidth = Math.Max(widestLine, optionsWidth) + boxPadding * 2f;
			boxHeight = (float)(letters.Length + 1) * lineHeight + (float)letters.Length * lineSpacing + boxPadding * 3f;
		}

		public override IEnumerator Leave(Oui next)
		{
			base.Overworld.ShowInputUI = true;
			Focused = false;
			Vector2 posFrom = Position;
			Vector2 posTo = new Vector2(0f, 1080f);
			for (float t = 0f; t < 1f; t += Engine.DeltaTime * 2f)
			{
				ease = 1f - Ease.CubeIn(t);
				Position = posFrom + (posTo - posFrom) * Ease.CubeInOut(t);
				yield return null;
			}
			Visible = false;
		}

		public override void Update()
		{
			base.Update();
			if (base.Selected && Focused)
			{
				if (!string.IsNullOrWhiteSpace(Name) && MInput.Keyboard.Check(Keys.LeftControl) && MInput.Keyboard.Pressed(Keys.S))
				{
					ResetDefaultName();
				}
				if (Input.MenuJournal.Pressed && Japanese)
				{
					SwapType();
				}
				if (Input.MenuRight.Pressed && (optionsIndex < 3 || !selectingOptions) && (Name.Length > 0 || !selectingOptions))
				{
					if (selectingOptions)
					{
						optionsIndex = Math.Min(optionsIndex + 1, 3);
					}
					else
					{
						do
						{
							index = (index + 1) % letters[line].Length;
						}
						while (letters[line][index] == ' ');
					}
					wiggler.Start();
					Audio.Play("event:/ui/main/rename_entry_rollover");
				}
				else if (Input.MenuLeft.Pressed && (optionsIndex > 0 || !selectingOptions))
				{
					if (selectingOptions)
					{
						optionsIndex = Math.Max(optionsIndex - 1, 0);
					}
					else
					{
						do
						{
							index = (index + letters[line].Length - 1) % letters[line].Length;
						}
						while (letters[line][index] == ' ');
					}
					wiggler.Start();
					Audio.Play("event:/ui/main/rename_entry_rollover");
				}
				else if (Input.MenuDown.Pressed && !selectingOptions)
				{
					int nextLine = line + 1;
					while (true)
					{
						if (nextLine >= letters.Length)
						{
							selectingOptions = true;
							break;
						}
						if (index < letters[nextLine].Length && letters[nextLine][index] != ' ')
						{
							line = nextLine;
							break;
						}
						nextLine++;
					}
					if (selectingOptions)
					{
						float realX = (float)index * widestLetter;
						float innerWidth2 = boxWidth - boxPadding * 2f;
						if (Name.Length == 0 || realX < cancelWidth + (innerWidth2 - cancelWidth - beginWidth - backspaceWidth - spaceWidth - widestLetter * 3f) / 2f)
						{
							optionsIndex = 0;
						}
						else if (realX < innerWidth2 - beginWidth - backspaceWidth - widestLetter * 2f)
						{
							optionsIndex = 1;
						}
						else if (realX < innerWidth2 - beginWidth - widestLetter)
						{
							optionsIndex = 2;
						}
						else
						{
							optionsIndex = 3;
						}
					}
					wiggler.Start();
					Audio.Play("event:/ui/main/rename_entry_rollover");
				}
				else if ((Input.MenuUp.Pressed || (selectingOptions && Name.Length <= 0 && optionsIndex > 0)) && (line > 0 || selectingOptions))
				{
					if (selectingOptions)
					{
						line = letters.Length;
						selectingOptions = false;
						float innerWidth = boxWidth - boxPadding * 2f;
						if (optionsIndex == 0)
						{
							index = (int)(cancelWidth / 2f / widestLetter);
						}
						else if (optionsIndex == 1)
						{
							index = (int)((innerWidth - beginWidth - backspaceWidth - spaceWidth / 2f - widestLetter * 2f) / widestLetter);
						}
						else if (optionsIndex == 2)
						{
							index = (int)((innerWidth - beginWidth - backspaceWidth / 2f - widestLetter) / widestLetter);
						}
						else if (optionsIndex == 3)
						{
							index = (int)((innerWidth - beginWidth / 2f) / widestLetter);
						}
					}
					line--;
					while (line > 0 && (index >= letters[line].Length || letters[line][index] == ' '))
					{
						line--;
					}
					while (index >= letters[line].Length || letters[line][index] == ' ')
					{
						index--;
					}
					wiggler.Start();
					Audio.Play("event:/ui/main/rename_entry_rollover");
				}
				else if (Input.MenuConfirm.Pressed)
				{
					if (selectingOptions)
					{
						if (optionsIndex == 0)
						{
							Cancel();
						}
						else if (optionsIndex == 1 && Name.Length > 0)
						{
							Space();
						}
						else if (optionsIndex == 2)
						{
							Backspace();
						}
						else if (optionsIndex == 3)
						{
							Finish();
						}
					}
					else if (Japanese && letters[line][index] == '\u309b' && Name.Length > 0 && dakuten_able.Contains(Name.Last()))
					{
						int last2 = Name[Name.Length - 1];
						last2++;
						Name = Name.Substring(0, Name.Length - 1);
						Name += (char)last2;
						wiggler.Start();
						Audio.Play("event:/ui/main/rename_entry_char");
					}
					else if (Japanese && letters[line][index] == '\u309c' && Name.Length > 0 && (handakuten_able.Contains(Name.Last()) || handakuten_able.Contains(Name.Last() + 1)))
					{
						int last = Name[Name.Length - 1];
						last = ((!handakuten_able.Contains(last)) ? (last + 2) : (last + 1));
						Name = Name.Substring(0, Name.Length - 1);
						Name += (char)last;
						wiggler.Start();
						Audio.Play("event:/ui/main/rename_entry_char");
					}
					else if (Name.Length < MaxNameLength)
					{
						Name += letters[line][index];
						wiggler.Start();
						Audio.Play("event:/ui/main/rename_entry_char");
					}
					else
					{
						Audio.Play("event:/ui/main/button_invalid");
					}
				}
				else if (Input.MenuCancel.Pressed)
				{
					if (Celeste.IsGGP && MInput.GamePads[Input.Gamepad].Pressed(Buttons.B))
					{
						Cancel();
					}
					else if (Name.Length > 0)
					{
						Backspace();
					}
					else
					{
						Cancel();
					}
				}
				else if (Input.Pause.Pressed)
				{
					Finish();
				}
			}
			pressedTimer -= Engine.DeltaTime;
			timer += Engine.DeltaTime;
			wiggler.Update();
		}

		private void ResetDefaultName()
		{
			if (StartingName == Settings.Instance.DefaultFileName || StartingName == Dialog.Clean("FILE_DEFAULT"))
			{
				StartingName = Name;
			}
			Settings.Instance.DefaultFileName = Name;
			Audio.Play("event:/new_content/ui/rename_entry_accept_locked");
		}

		private void Space()
		{
			if (Name.Length < MaxNameLength && Name.Length > 0)
			{
				Name += " ";
				wiggler.Start();
				Audio.Play("event:/ui/main/rename_entry_char");
			}
			else
			{
				Audio.Play("event:/ui/main/button_invalid");
			}
		}

		private void Backspace()
		{
			if (Name.Length > 0)
			{
				Name = Name.Substring(0, Name.Length - 1);
				Audio.Play("event:/ui/main/rename_entry_backspace");
			}
			else
			{
				Audio.Play("event:/ui/main/button_invalid");
			}
		}

		private void Finish()
		{
			if (Name.Length >= 1)
			{
				if (MInput.GamePads.Length != 0 && MInput.GamePads[0] != null && (MInput.GamePads[0].Check(Buttons.LeftTrigger) || MInput.GamePads[0].Check(Buttons.LeftShoulder)) && (MInput.GamePads[0].Check(Buttons.RightTrigger) || MInput.GamePads[0].Check(Buttons.RightShoulder)))
				{
					ResetDefaultName();
				}
				Focused = false;
				base.Overworld.Goto<OuiFileSelect>();
				Audio.Play("event:/ui/main/rename_entry_accept");
			}
			else
			{
				Audio.Play("event:/ui/main/button_invalid");
			}
		}

		private void SwapType()
		{
			hiragana = !hiragana;
			if (hiragana)
			{
				ReloadLetters(Dialog.Clean("name_letters"));
			}
			else
			{
				ReloadLetters(Dialog.Clean("name_letters_katakana"));
			}
		}

		private void Cancel()
		{
			FileSlot.Name = StartingName;
			Focused = false;
			base.Overworld.Goto<OuiFileSelect>();
			Audio.Play("event:/ui/main/button_back");
		}

		public override void Render()
		{
			Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * 0.8f * ease);
			Vector2 pos = boxtopleft + new Vector2(boxPadding, boxPadding);
			int lineIndex = 0;
			string[] array = letters;
			foreach (string j in array)
			{
				for (int i = 0; i < j.Length; i++)
				{
					bool selected = lineIndex == line && i == index && !selectingOptions;
					Vector2 scale = Vector2.One * (selected ? 1.2f : 1f);
					Vector2 p = pos + new Vector2(widestLetter, lineHeight) / 2f;
					if (selected)
					{
						p += new Vector2(0f, wiggler.Value) * 8f;
					}
					DrawOptionText(j[i].ToString(), p, new Vector2(0.5f, 0.5f), scale, selected);
					pos.X += widestLetter;
				}
				pos.X = boxtopleft.X + boxPadding;
				pos.Y += lineHeight + lineSpacing;
				lineIndex++;
			}
			float w = wiggler.Value * 8f;
			pos.Y = boxtopleft.Y + boxHeight - lineHeight - boxPadding;
			Draw.Rect(pos.X, pos.Y - boxPadding * 0.5f, boxWidth - boxPadding * 2f, 4f, Color.White);
			DrawOptionText(cancel, pos + new Vector2(0f, lineHeight + ((selectingOptions && optionsIndex == 0) ? w : 0f)), new Vector2(0f, 1f), Vector2.One * optionsScale, selectingOptions && optionsIndex == 0);
			pos.X = boxtopleft.X + boxWidth - backspaceWidth - widestLetter - spaceWidth - widestLetter - beginWidth - boxPadding;
			DrawOptionText(space, pos + new Vector2(0f, lineHeight + ((selectingOptions && optionsIndex == 1) ? w : 0f)), new Vector2(0f, 1f), Vector2.One * optionsScale, selectingOptions && optionsIndex == 1, Name.Length == 0 || !Focused);
			pos.X += spaceWidth + widestLetter;
			DrawOptionText(backspace, pos + new Vector2(0f, lineHeight + ((selectingOptions && optionsIndex == 2) ? w : 0f)), new Vector2(0f, 1f), Vector2.One * optionsScale, selectingOptions && optionsIndex == 2, Name.Length <= 0 || !Focused);
			pos.X += backspaceWidth + widestLetter;
			DrawOptionText(accept, pos + new Vector2(0f, lineHeight + ((selectingOptions && optionsIndex == 3) ? w : 0f)), new Vector2(0f, 1f), Vector2.One * optionsScale * 1.25f, selectingOptions && optionsIndex == 3, Name.Length < 1 || !Focused);
			if (Japanese)
			{
				float scale2 = 1f;
				string text = Dialog.Clean(hiragana ? "NAME_LETTERS_SWAP_KATAKANA" : "NAME_LETTERS_SWAP_HIRAGANA");
				MTexture mTexture = Input.GuiButton(Input.MenuJournal);
				ActiveFont.Measure(text);
				float buttonWidth = (float)mTexture.Width * scale2;
				Vector2 center = new Vector2(70f, 1144f - 154f * ease);
				mTexture.DrawJustified(center, new Vector2(0f, 0.5f), Color.White, scale2, 0f);
				ActiveFont.DrawOutline(text, center + new Vector2(16f + buttonWidth, 0f), new Vector2(0f, 0.5f), Vector2.One * scale2, Color.White, 2f, Color.Black);
			}
		}

		private void DrawOptionText(string text, Vector2 at, Vector2 justify, Vector2 scale, bool selected, bool disabled = false)
		{
			bool num = selected && pressedTimer > 0f;
			Color color = (disabled ? disableColor : GetTextColor(selected));
			Color shadow = (disabled ? Color.Lerp(disableColor, Color.Black, 0.7f) : Color.Gray);
			if (num)
			{
				ActiveFont.Draw(text, at + Vector2.UnitY, justify, scale, color);
			}
			else
			{
				ActiveFont.DrawEdgeOutline(text, at, justify, scale, color, 4f, shadow);
			}
		}

		private Color GetTextColor(bool selected)
		{
			if (selected)
			{
				if (Settings.Instance.DisableFlashes)
				{
					return selectColorA;
				}
				if (!Calc.BetweenInterval(timer, 0.1f))
				{
					return selectColorB;
				}
				return selectColorA;
			}
			return unselectColor;
		}
	}
}
