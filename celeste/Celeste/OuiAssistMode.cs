using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class OuiAssistMode : Oui
	{
		private class Page
		{
			public FancyText.Text Text;

			public float Ease;

			public float Direction;
		}

		public OuiFileSelectSlot FileSlot;

		private float fade;

		private List<Page> pages = new List<Page>();

		private int pageIndex;

		private int questionIndex = 1;

		private float questionEase;

		private Wiggler wiggler;

		private float dot;

		private FancyText.Text questionText;

		private Color iconColor = Calc.HexToColor("44adf7");

		private float leftArrowEase;

		private float rightArrowEase;

		private EventInstance mainSfx;

		private const float textScale = 0.8f;

		public OuiAssistMode()
		{
			Visible = false;
			Add(wiggler = Wiggler.Create(0.4f, 4f));
		}

		public override IEnumerator Enter(Oui from)
		{
			Focused = false;
			Visible = true;
			pageIndex = 0;
			questionIndex = 1;
			questionEase = 0f;
			dot = 0f;
			questionText = FancyText.Parse(Dialog.Get("ASSIST_ASK"), 1600, -1, 1f, Color.White);
			if (!FileSlot.AssistModeEnabled)
			{
				for (int i = 0; Dialog.Has("ASSIST_MODE_" + i); i++)
				{
					Page page = new Page();
					page.Text = FancyText.Parse(Dialog.Get("ASSIST_MODE_" + i), 2000, -1, 1f, Color.White * 0.9f);
					page.Ease = 0f;
					pages.Add(page);
				}
				pages[0].Ease = 1f;
				mainSfx = Audio.Play("event:/ui/main/assist_info_whistle");
			}
			else
			{
				questionEase = 1f;
			}
			while (fade < 1f)
			{
				fade += Engine.DeltaTime * 4f;
				yield return null;
			}
			Focused = true;
			Add(new Coroutine(InputRoutine()));
		}

		public override IEnumerator Leave(Oui next)
		{
			Focused = false;
			while (fade > 0f)
			{
				fade -= Engine.DeltaTime * 4f;
				yield return null;
			}
			if (mainSfx != null)
			{
				mainSfx.release();
			}
			pages.Clear();
			Visible = false;
		}

		private IEnumerator InputRoutine()
		{
			while (true)
			{
				if (Input.MenuCancel.Pressed)
				{
					Focused = false;
					base.Overworld.Goto<OuiFileSelect>();
					Audio.Play("event:/ui/main/button_back");
					Audio.SetParameter(mainSfx, "assist_progress", 6f);
					yield break;
				}
				int was = pageIndex;
				if ((Input.MenuConfirm.Pressed || Input.MenuRight.Pressed) && pageIndex < pages.Count)
				{
					pageIndex++;
					Audio.Play("event:/ui/main/rollover_down");
					Audio.SetParameter(mainSfx, "assist_progress", pageIndex);
				}
				else if (Input.MenuLeft.Pressed && pageIndex > 0)
				{
					Audio.Play("event:/ui/main/rollover_up");
					pageIndex--;
				}
				if (was != pageIndex)
				{
					if (was < pages.Count)
					{
						pages[was].Direction = Math.Sign(was - pageIndex);
						while ((pages[was].Ease = Calc.Approach(pages[was].Ease, 0f, Engine.DeltaTime * 8f)) != 0f)
						{
							yield return null;
						}
					}
					else
					{
						while ((questionEase = Calc.Approach(questionEase, 0f, Engine.DeltaTime * 8f)) != 0f)
						{
							yield return null;
						}
					}
					if (pageIndex < pages.Count)
					{
						pages[pageIndex].Direction = Math.Sign(pageIndex - was);
						while ((pages[pageIndex].Ease = Calc.Approach(pages[pageIndex].Ease, 1f, Engine.DeltaTime * 8f)) != 1f)
						{
							yield return null;
						}
					}
					else
					{
						while ((questionEase = Calc.Approach(questionEase, 1f, Engine.DeltaTime * 8f)) != 1f)
						{
							yield return null;
						}
					}
				}
				if (pageIndex >= pages.Count)
				{
					if (Input.MenuConfirm.Pressed)
					{
						break;
					}
					if (Input.MenuUp.Pressed && questionIndex > 0)
					{
						Audio.Play("event:/ui/main/rollover_up");
						questionIndex--;
						wiggler.Start();
					}
					else if (Input.MenuDown.Pressed && questionIndex < 1)
					{
						Audio.Play("event:/ui/main/rollover_down");
						questionIndex++;
						wiggler.Start();
					}
				}
				yield return null;
			}
			FileSlot.AssistModeEnabled = questionIndex == 0;
			if (FileSlot.AssistModeEnabled)
			{
				FileSlot.VariantModeEnabled = false;
			}
			FileSlot.CreateButtons();
			Focused = false;
			base.Overworld.Goto<OuiFileSelect>();
			Audio.Play((questionIndex == 0) ? "event:/ui/main/assist_button_yes" : "event:/ui/main/assist_button_no");
			Audio.SetParameter(mainSfx, "assist_progress", (questionIndex == 0) ? 4 : 5);
		}

		public override void Update()
		{
			dot = Calc.Approach(dot, pageIndex, Engine.DeltaTime * 8f);
			leftArrowEase = Calc.Approach(leftArrowEase, (pageIndex > 0) ? 1 : 0, Engine.DeltaTime * 4f);
			rightArrowEase = Calc.Approach(rightArrowEase, (pageIndex < pages.Count) ? 1 : 0, Engine.DeltaTime * 4f);
			base.Update();
		}

		public override void Render()
		{
			if (!Visible)
			{
				return;
			}
			Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * fade * 0.9f);
			for (int j = 0; j < pages.Count; j++)
			{
				Page page = pages[j];
				float e = Ease.CubeOut(page.Ease);
				if (e > 0f)
				{
					Vector2 pos = new Vector2(960f, 620f);
					pos.X += page.Direction * (1f - e) * 256f;
					page.Text.DrawJustifyPerLine(pos, new Vector2(0.5f, 0f), Vector2.One * 0.8f, e * fade);
				}
			}
			if (questionEase > 0f)
			{
				float e2 = Ease.CubeOut(questionEase);
				float w = wiggler.Value * 8f;
				Vector2 center = new Vector2(960f + (1f - e2) * 256f, 620f);
				float ln = ActiveFont.LineHeight;
				questionText.DrawJustifyPerLine(center, new Vector2(0.5f, 0f), Vector2.One, e2 * fade);
				ActiveFont.DrawOutline(Dialog.Clean("ASSIST_YES"), center + new Vector2(((questionIndex == 0) ? w : 0f) * 1.2f * e2, ln * 1.4f + 10f), new Vector2(0.5f, 0f), Vector2.One * 0.8f, SelectionColor(questionIndex == 0), 2f, Color.Black * e2 * fade);
				ActiveFont.DrawOutline(Dialog.Clean("ASSIST_NO"), center + new Vector2(((questionIndex == 1) ? w : 0f) * 1.2f * e2, ln * 2.2f + 20f), new Vector2(0.5f, 0f), Vector2.One * 0.8f, SelectionColor(questionIndex == 1), 2f, Color.Black * e2 * fade);
			}
			if (pages.Count > 0)
			{
				int dots = pages.Count + 1;
				MTexture tex = GFX.Gui["dot"];
				int width = tex.Width * dots;
				Vector2 pos2 = new Vector2(960f, 960f - 40f * Ease.CubeOut(fade));
				for (int i = 0; i < dots; i++)
				{
					tex.DrawCentered(pos2 + new Vector2((float)(-width / 2) + (float)tex.Width * ((float)i + 0.5f), 0f), Color.White * 0.25f);
				}
				float scale = 1f + Calc.YoYo(dot % 1f) * 4f;
				tex.DrawCentered(pos2 + new Vector2((float)(-width / 2) + (float)tex.Width * (dot + 0.5f), 0f), iconColor, new Vector2(scale, 1f));
				GFX.Gui["dotarrow"].DrawCentered(pos2 + new Vector2(-width / 2 - 50, 32f * (1f - Ease.CubeOut(leftArrowEase))), iconColor * leftArrowEase, new Vector2(-1f, 1f));
				GFX.Gui["dotarrow"].DrawCentered(pos2 + new Vector2(width / 2 + 50, 32f * (1f - Ease.CubeOut(rightArrowEase))), iconColor * rightArrowEase);
			}
			GFX.Gui["assistmode"].DrawJustified(new Vector2(960f, 540f + 64f * Ease.CubeOut(fade)), new Vector2(0.5f, 1f), iconColor * fade);
		}

		private Color SelectionColor(bool selected)
		{
			if (selected)
			{
				return ((Settings.Instance.DisableFlashes || base.Scene.BetweenInterval(0.1f)) ? TextMenu.HighlightColorA : TextMenu.HighlightColorB) * fade;
			}
			return Color.White * fade;
		}
	}
}
