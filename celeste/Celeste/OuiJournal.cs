using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class OuiJournal : Oui
	{
		private const float onScreenX = 0f;

		private const float offScreenX = -1920f;

		public bool PageTurningLocked;

		public List<OuiJournalPage> Pages = new List<OuiJournalPage>();

		public int PageIndex;

		public VirtualRenderTarget CurrentPageBuffer;

		public VirtualRenderTarget NextPageBuffer;

		private bool turningPage;

		private float turningScale;

		private Color backColor = Color.Lerp(Color.White, Color.Black, 0.2f);

		private bool fromAreaInspect;

		private float rotation;

		private MountainCamera cameraStart;

		private MountainCamera cameraEnd;

		private MTexture arrow = MTN.Journal["pageArrow"];

		private float dot;

		private float dotTarget;

		private float dotEase;

		private float leftArrowEase;

		private float rightArrowEase;

		public OuiJournalPage Page => Pages[PageIndex];

		public OuiJournalPage NextPage => Pages[PageIndex + 1];

		public OuiJournalPage PrevPage => Pages[PageIndex - 1];

		public override IEnumerator Enter(Oui from)
		{
			Stats.MakeRequest();
			base.Overworld.ShowConfirmUI = false;
			fromAreaInspect = from is OuiChapterPanel;
			PageIndex = 0;
			Visible = true;
			base.X = -1920f;
			turningPage = false;
			turningScale = 1f;
			rotation = 0f;
			dot = 0f;
			dotTarget = 0f;
			dotEase = 0f;
			leftArrowEase = 0f;
			rightArrowEase = 0f;
			NextPageBuffer = VirtualContent.CreateRenderTarget("journal-a", 1610, 1000);
			CurrentPageBuffer = VirtualContent.CreateRenderTarget("journal-b", 1610, 1000);
			Pages.Add(new OuiJournalCover(this));
			Pages.Add(new OuiJournalProgress(this));
			Pages.Add(new OuiJournalSpeedrun(this));
			Pages.Add(new OuiJournalDeaths(this));
			Pages.Add(new OuiJournalPoem(this));
			if (Stats.Has())
			{
				Pages.Add(new OuiJournalGlobal(this));
			}
			int i = 0;
			foreach (OuiJournalPage page in Pages)
			{
				page.PageIndex = i++;
			}
			Pages[0].Redraw(CurrentPageBuffer);
			cameraStart = base.Overworld.Mountain.UntiltedCamera;
			cameraEnd = cameraStart;
			cameraEnd.Position += -cameraStart.Rotation.Forward() * 1f;
			base.Overworld.Mountain.EaseCamera(base.Overworld.Mountain.Area, cameraEnd, 2f);
			base.Overworld.Mountain.AllowUserRotation = false;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.4f)
			{
				rotation = -0.025f * Ease.BackOut(p);
				base.X = -1920f + 1920f * Ease.CubeInOut(p);
				dotEase = p;
				yield return null;
			}
			dotEase = 1f;
		}

		public override void HandleGraphicsReset()
		{
			base.HandleGraphicsReset();
			if (Pages.Count > 0)
			{
				Page.Redraw(CurrentPageBuffer);
			}
		}

		public IEnumerator TurnPage(int direction)
		{
			turningPage = true;
			if (direction < 0)
			{
				PageIndex--;
				turningScale = -1f;
				dotTarget -= 1f;
				Page.Redraw(CurrentPageBuffer);
				NextPage.Redraw(NextPageBuffer);
				while ((turningScale = Calc.Approach(turningScale, 1f, Engine.DeltaTime * 8f)) < 1f)
				{
					yield return null;
				}
			}
			else
			{
				NextPage.Redraw(NextPageBuffer);
				turningScale = 1f;
				dotTarget += 1f;
				while ((turningScale = Calc.Approach(turningScale, -1f, Engine.DeltaTime * 8f)) > -1f)
				{
					yield return null;
				}
				PageIndex++;
				Page.Redraw(CurrentPageBuffer);
			}
			turningScale = 1f;
			turningPage = false;
		}

		public override IEnumerator Leave(Oui next)
		{
			Audio.Play("event:/ui/world_map/journal/back");
			base.Overworld.Mountain.EaseCamera(base.Overworld.Mountain.Area, cameraStart, 0.4f);
			UserIO.SaveHandler(file: false, settings: true);
			yield return EaseOut(0.4f);
			while (UserIO.Saving)
			{
				yield return null;
			}
			CurrentPageBuffer.Dispose();
			NextPageBuffer.Dispose();
			base.Overworld.ShowConfirmUI = true;
			Pages.Clear();
			Visible = false;
			base.Overworld.Mountain.AllowUserRotation = true;
		}

		private IEnumerator EaseOut(float duration)
		{
			float rotFrom = rotation;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
			{
				rotation = rotFrom * (1f - Ease.BackOut(p));
				base.X = 0f + -1920f * Ease.CubeInOut(p);
				dotEase = 1f - p;
				yield return null;
			}
			dotEase = 0f;
		}

		public override void Update()
		{
			base.Update();
			dot = Calc.Approach(dot, dotTarget, Engine.DeltaTime * 8f);
			leftArrowEase = Calc.Approach(leftArrowEase, (dotTarget > 0f) ? 1 : 0, Engine.DeltaTime * 5f) * dotEase;
			rightArrowEase = Calc.Approach(rightArrowEase, (dotTarget < (float)(Pages.Count - 1)) ? 1 : 0, Engine.DeltaTime * 5f) * dotEase;
			if (!Focused || turningPage)
			{
				return;
			}
			Page.Update();
			if (!PageTurningLocked)
			{
				if (Input.MenuLeft.Pressed && PageIndex > 0)
				{
					if (PageIndex == 1)
					{
						Audio.Play("event:/ui/world_map/journal/page_cover_back");
					}
					else
					{
						Audio.Play("event:/ui/world_map/journal/page_main_back");
					}
					Add(new Coroutine(TurnPage(-1)));
				}
				else if (Input.MenuRight.Pressed && PageIndex < Pages.Count - 1)
				{
					if (PageIndex == 0)
					{
						Audio.Play("event:/ui/world_map/journal/page_cover_forward");
					}
					else
					{
						Audio.Play("event:/ui/world_map/journal/page_main_forward");
					}
					Add(new Coroutine(TurnPage(1)));
				}
			}
			if (!PageTurningLocked && (Input.MenuJournal.Pressed || Input.MenuCancel.Pressed))
			{
				Close();
			}
		}

		private void Close()
		{
			if (fromAreaInspect)
			{
				base.Overworld.Goto<OuiChapterPanel>();
			}
			else
			{
				base.Overworld.Goto<OuiChapterSelect>();
			}
		}

		public override void Render()
		{
			Vector2 pos = Position + new Vector2(128f, 120f);
			float easeFront = Ease.CubeInOut(Math.Max(0f, turningScale));
			float easeBack = Ease.CubeInOut(Math.Abs(Math.Min(0f, turningScale)));
			if (SaveData.Instance.CheatMode)
			{
				MTN.FileSelect["cheatmode"].DrawCentered(pos + new Vector2(80f, 360f), Color.White, 1f, (float)Math.PI / 2f);
			}
			if (SaveData.Instance.AssistMode)
			{
				MTN.FileSelect["assist"].DrawCentered(pos + new Vector2(100f, 370f), Color.White, 1f, (float)Math.PI / 2f);
			}
			MTexture edge = MTN.Journal["edge"];
			edge.Draw(pos + new Vector2(-edge.Width, 0f), Vector2.Zero, Color.White, 1f, rotation);
			if (PageIndex > 0)
			{
				MTN.Journal[PrevPage.PageTexture].Draw(pos, Vector2.Zero, backColor, new Vector2(-1f, 1f), rotation);
			}
			if (turningPage)
			{
				MTN.Journal[NextPage.PageTexture].Draw(pos, Vector2.Zero, Color.White, 1f, rotation);
				Draw.SpriteBatch.Draw((RenderTarget2D)NextPageBuffer, pos, NextPageBuffer.Bounds, Color.White, rotation, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
			}
			if (turningPage && easeBack > 0f)
			{
				MTN.Journal[Page.PageTexture].Draw(pos, Vector2.Zero, backColor, new Vector2(-1f * easeBack, 1f), rotation);
			}
			if (easeFront > 0f)
			{
				MTN.Journal[Page.PageTexture].Draw(pos, Vector2.Zero, Color.White, new Vector2(easeFront, 1f), rotation);
				Draw.SpriteBatch.Draw((RenderTarget2D)CurrentPageBuffer, pos, CurrentPageBuffer.Bounds, Color.White, rotation, Vector2.Zero, new Vector2(easeFront, 1f), SpriteEffects.None, 0f);
			}
			if (Pages.Count > 0)
			{
				int dots = Pages.Count;
				MTexture tex = GFX.Gui["dot_outline"];
				int width = tex.Width * dots;
				Vector2 p = new Vector2(960f, 1040f - 40f * Ease.CubeOut(dotEase));
				for (int i = 0; i < dots; i++)
				{
					tex.DrawCentered(p + new Vector2((float)(-width / 2) + (float)tex.Width * ((float)i + 0.5f), 0f), Color.White * 0.25f);
				}
				float scale = 1f + Calc.YoYo(dot % 1f) * 4f;
				tex.DrawCentered(p + new Vector2((float)(-width / 2) + (float)tex.Width * (dot + 0.5f), 0f), Color.White, new Vector2(scale, 1f));
				GFX.Gui["dotarrow_outline"].DrawCentered(p + new Vector2(-width / 2 - 50, 32f * (1f - Ease.CubeOut(leftArrowEase))), Color.White * leftArrowEase, new Vector2(-1f, 1f));
				GFX.Gui["dotarrow_outline"].DrawCentered(p + new Vector2(width / 2 + 50, 32f * (1f - Ease.CubeOut(rightArrowEase))), Color.White * rightArrowEase);
			}
		}
	}
}
