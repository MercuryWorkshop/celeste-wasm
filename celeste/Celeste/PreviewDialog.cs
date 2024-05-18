using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste
{
	public class PreviewDialog : Scene
	{
		private class Renderer : HiresRenderer
		{
			public PreviewDialog previewer;

			public Renderer(PreviewDialog previewer)
			{
				this.previewer = previewer;
			}

			public override void RenderContent(Scene scene)
			{
				HiresRenderer.BeginRender();
				previewer.RenderContent();
				HiresRenderer.EndRender();
			}
		}

		private Language language;

		private List<string> list = new List<string>();

		private Vector2 listScroll = new Vector2(64f, 64f);

		private const float scale = 0.6f;

		private string current;

		private List<object> elements = new List<object>();

		private Vector2 textboxScroll = new Vector2(0f, 0f);

		private float delay;

		public Vector2 Mouse => Vector2.Transform(new Vector2(MInput.Mouse.CurrentState.X, MInput.Mouse.CurrentState.Y), Matrix.Invert(Engine.ScreenMatrix));

		public PreviewDialog(Language language = null, float listScroll = 64f, float textboxScroll = 0f, string dialog = null)
		{
			this.listScroll.Y = listScroll;
			this.textboxScroll.Y = textboxScroll;
			if (language != null)
			{
				SetLanguage(language);
			}
			if (dialog != null)
			{
				SetCurrent(dialog);
			}
			Add(new Renderer(this));
		}

		public override void End()
		{
			base.End();
			UnsetLanguage();
		}

		public override void Update()
		{
			if (!Engine.Instance.IsActive)
			{
				delay = 0.1f;
				return;
			}
			if (delay > 0f)
			{
				delay -= Engine.DeltaTime;
				return;
			}
			if (current != null)
			{
				float y = 1f;
				foreach (object element in elements)
				{
					if (element is Textbox textbox)
					{
						textbox.RenderOffset = textboxScroll + Vector2.UnitY * y;
						y += 300f;
						if (textbox.Scene != null)
						{
							textbox.Update();
						}
					}
					else
					{
						y += (float)(language.FontSize.LineHeight + 50);
					}
				}
				textboxScroll.Y += (float)MInput.Mouse.WheelDelta * Engine.DeltaTime * ActiveFont.LineHeight;
				textboxScroll.Y -= Input.Aim.Value.Y * Engine.DeltaTime * ActiveFont.LineHeight * 20f;
				textboxScroll.Y = Calc.Clamp(textboxScroll.Y, 716f - y, 64f);
				if (MInput.Keyboard.Pressed(Keys.Escape) || Input.MenuConfirm.Pressed)
				{
					ClearTextboxes();
				}
				else if (MInput.Keyboard.Pressed(Keys.Space))
				{
					string last = current;
					ClearTextboxes();
					int next = list.IndexOf(last) + 1;
					if (next < list.Count)
					{
						SetCurrent(list[next]);
					}
				}
			}
			else
			{
				listScroll.Y += (float)MInput.Mouse.WheelDelta * Engine.DeltaTime * ActiveFont.LineHeight;
				listScroll.Y -= Input.Aim.Value.Y * Engine.DeltaTime * ActiveFont.LineHeight * 20f;
				listScroll.Y = Calc.Clamp(listScroll.Y, 1016f - (float)list.Count * ActiveFont.LineHeight * 0.6f, 64f);
				if (language != null)
				{
					if (MInput.Mouse.PressedLeftButton)
					{
						for (int j = 0; j < list.Count; j++)
						{
							if (MouseOverOption(j))
							{
								SetCurrent(list[j]);
								break;
							}
						}
					}
					if (MInput.Keyboard.Pressed(Keys.Escape) || Input.MenuConfirm.Pressed)
					{
						listScroll = new Vector2(64f, 64f);
						UnsetLanguage();
					}
				}
				else if (MInput.Mouse.PressedLeftButton)
				{
					int i = 0;
					foreach (KeyValuePair<string, Language> kv in Dialog.Languages)
					{
						if (MouseOverOption(i))
						{
							SetLanguage(kv.Value);
							listScroll = new Vector2(64f, 64f);
							break;
						}
						i++;
					}
				}
			}
			if (MInput.Keyboard.Pressed(Keys.F2))
			{
				Celeste.ReloadPortraits();
				Engine.Scene = new PreviewDialog(language, listScroll.Y, textboxScroll.Y, current);
			}
			if (MInput.Keyboard.Pressed(Keys.F1) && language != null)
			{
				Celeste.ReloadDialog();
				Engine.Scene = new PreviewDialog(Dialog.Languages[language.Id], listScroll.Y, textboxScroll.Y, current);
			}
		}

		private void ClearTextboxes()
		{
			foreach (object element in elements)
			{
				if (element is Textbox)
				{
					Remove(element as Textbox);
				}
			}
			current = null;
			textboxScroll = Vector2.Zero;
		}

		private void SetCurrent(string id)
		{
			current = id;
			elements.Clear();
			Textbox last = null;
			int page = 0;
			while (true)
			{
				Textbox box = new Textbox(id, language);
				if (!box.SkipToPage(page))
				{
					break;
				}
				if (last != null)
				{
					for (int i = last.Start + 1; i <= box.Start && i < last.Nodes.Count; i++)
					{
						if (last.Nodes[i] is FancyText.Trigger trigger)
						{
							elements.Add((trigger.Silent ? "Silent " : "") + "Trigger [" + trigger.Index + "] " + trigger.Label);
						}
					}
				}
				Add(box);
				elements.Add(box);
				box.RenderOffset = textboxScroll + Vector2.UnitY * (1 + page * 300);
				last = box;
				page++;
			}
		}

		private void SetLanguage(Language lan)
		{
			Fonts.Load(lan.FontFace);
			language = lan;
			list.Clear();
			bool foundFirst = false;
			foreach (KeyValuePair<string, string> kv in language.Dialog)
			{
				if (!foundFirst && kv.Key.StartsWith("CH0", StringComparison.OrdinalIgnoreCase))
				{
					foundFirst = true;
				}
				if (foundFirst && !kv.Key.StartsWith("poem_", StringComparison.OrdinalIgnoreCase) && !kv.Key.StartsWith("journal_", StringComparison.OrdinalIgnoreCase))
				{
					list.Add(kv.Key);
				}
			}
		}

		private void UnsetLanguage()
		{
			if (language != null && language.Id != Settings.Instance.Language && language.FontFace != Dialog.Languages["english"].FontFace)
			{
				Fonts.Unload(language.FontFace);
			}
			language = null;
		}

		private void RenderContent()
		{
			Draw.Rect(0f, 0f, 960f, 1080f, Color.DarkSlateGray * 0.25f);
			if (current != null)
			{
				int page = 1;
				int y = 0;
				foreach (object element in elements)
				{
					if (element is Textbox textbox)
					{
						if (textbox.Opened && language.Font.Sizes.Count > 0)
						{
							textbox.Render();
							language.Font.DrawOutline(language.FontFaceSize, "#" + page, textbox.RenderOffset + new Vector2(32f, 64f), Vector2.Zero, Vector2.One * 0.5f, Color.White, 2f, Color.Black);
							page++;
							y += 300;
						}
					}
					else
					{
						language.Font.DrawOutline(language.FontFaceSize, element.ToString(), textboxScroll + new Vector2(128f, y + 50 + language.FontSize.LineHeight), new Vector2(0f, 0.5f), Vector2.One * 0.5f, Color.White, 2f, Color.Black);
						y += language.FontSize.LineHeight + 50;
					}
				}
				ActiveFont.DrawOutline(current, new Vector2(1888f, 32f), new Vector2(1f, 0f), Vector2.One * 0.5f, Color.Red, 2f, Color.Black);
			}
			else if (language != null)
			{
				int j = 0;
				foreach (string option in list)
				{
					if (language.Font.Sizes.Count > 0)
					{
						language.Font.Draw(language.FontFaceSize, option, listScroll + new Vector2(0f, (float)j * ActiveFont.LineHeight * 0.6f), Vector2.Zero, Vector2.One * 0.6f, MouseOverOption(j) ? Color.White : Color.Gray);
					}
					j++;
				}
			}
			else
			{
				int i = 0;
				foreach (KeyValuePair<string, Language> language in Dialog.Languages)
				{
					ActiveFont.Draw(language.Value.Id, listScroll + new Vector2(0f, (float)i * ActiveFont.LineHeight * 0.6f), Vector2.Zero, Vector2.One * 0.6f, MouseOverOption(i) ? Color.White : Color.Gray);
					i++;
				}
			}
			Draw.Rect(Mouse.X - 12f, Mouse.Y - 4f, 24f, 8f, Color.Red);
			Draw.Rect(Mouse.X - 4f, Mouse.Y - 12f, 8f, 24f, Color.Red);
		}

		private bool MouseOverOption(int i)
		{
			if (Mouse.X > listScroll.X && Mouse.Y > listScroll.Y + (float)i * ActiveFont.LineHeight * 0.6f && MInput.Mouse.X < 960f)
			{
				return Mouse.Y < listScroll.Y + (float)(i + 1) * ActiveFont.LineHeight * 0.6f;
			}
			return false;
		}
	}
}
