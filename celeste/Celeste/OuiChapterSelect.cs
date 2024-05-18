using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class OuiChapterSelect : Oui
	{
		private List<OuiChapterSelectIcon> icons = new List<OuiChapterSelectIcon>();

		private int indexToSnap = -1;

		private const int scarfSegmentSize = 2;

		private MTexture scarf = GFX.Gui["areas/hover"];

		private MTexture[] scarfSegments;

		private float ease;

		private float journalEase;

		private bool journalEnabled;

		private bool disableInput;

		private bool display;

		private float inputDelay;

		private bool autoAdvancing;

		private int area
		{
			get
			{
				return SaveData.Instance.LastArea.ID;
			}
			set
			{
				SaveData.Instance.LastArea.ID = value;
			}
		}

		public override bool IsStart(Overworld overworld, Overworld.StartMode start)
		{
			if (start == Overworld.StartMode.AreaComplete || start == Overworld.StartMode.AreaQuit)
			{
				indexToSnap = area;
			}
			return false;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			int count = AreaData.Areas.Count;
			for (int j = 0; j < count; j++)
			{
				MTexture front = GFX.Gui[AreaData.Areas[j].Icon];
				MTexture back = (GFX.Gui.Has(AreaData.Areas[j].Icon + "_back") ? GFX.Gui[AreaData.Areas[j].Icon + "_back"] : front);
				icons.Add(new OuiChapterSelectIcon(j, front, back));
				base.Scene.Add(icons[j]);
			}
			scarfSegments = new MTexture[scarf.Height / 2];
			for (int i = 0; i < scarfSegments.Length; i++)
			{
				scarfSegments[i] = scarf.GetSubtexture(0, i * 2, scarf.Width, 2);
			}
			if (indexToSnap >= 0)
			{
				area = indexToSnap;
				icons[indexToSnap].SnapToSelected();
			}
			base.Depth = -20;
		}

		public override IEnumerator Enter(Oui from)
		{
			Visible = true;
			EaseCamera();
			display = true;
			journalEnabled = Celeste.PlayMode == Celeste.PlayModes.Debug || SaveData.Instance.CheatMode;
			for (int i = 0; i <= SaveData.Instance.UnlockedAreas; i++)
			{
				if (journalEnabled)
				{
					break;
				}
				if (SaveData.Instance.Areas[i].Modes[0].TimePlayed > 0 && !AreaData.Get(i).Interlude)
				{
					journalEnabled = true;
				}
			}
			OuiChapterSelectIcon unselected = null;
			if (from is OuiChapterPanel)
			{
				OuiChapterSelectIcon ouiChapterSelectIcon;
				unselected = (ouiChapterSelectIcon = icons[area]);
				ouiChapterSelectIcon.Unselect();
			}
			foreach (OuiChapterSelectIcon icon in icons)
			{
				if (icon.Area <= SaveData.Instance.UnlockedAreas && icon != unselected)
				{
					icon.Position = icon.HiddenPosition;
					icon.Show();
					icon.AssistModeUnlockable = false;
				}
				else if (SaveData.Instance.AssistMode && icon.Area == SaveData.Instance.UnlockedAreas + 1 && icon.Area <= SaveData.Instance.MaxAssistArea)
				{
					icon.Position = icon.HiddenPosition;
					icon.Show();
					icon.AssistModeUnlockable = true;
				}
				yield return 0.01f;
			}
			if (!autoAdvancing && SaveData.Instance.UnlockedAreas == 10 && !SaveData.Instance.RevealedChapter9)
			{
				int ch = area;
				yield return SetupCh9Unlock();
				yield return PerformCh9Unlock(ch != 10);
			}
			if (from is OuiChapterPanel)
			{
				yield return 0.25f;
			}
		}

		public override IEnumerator Leave(Oui next)
		{
			display = false;
			if (next is OuiMainMenu)
			{
				while (area > SaveData.Instance.UnlockedAreas)
				{
					area--;
				}
				UserIO.SaveHandler(file: true, settings: false);
				yield return EaseOut(next);
				while (UserIO.Saving)
				{
					yield return null;
				}
			}
			else
			{
				yield return EaseOut(next);
			}
		}

		private IEnumerator EaseOut(Oui next)
		{
			OuiChapterSelectIcon selected = null;
			if (next is OuiChapterPanel)
			{
				OuiChapterSelectIcon ouiChapterSelectIcon;
				selected = (ouiChapterSelectIcon = icons[area]);
				ouiChapterSelectIcon.Select();
			}
			foreach (OuiChapterSelectIcon icon in icons)
			{
				if (selected != icon)
				{
					icon.Hide();
				}
				yield return 0.01f;
			}
			Visible = false;
		}

		public void AdvanceToNext()
		{
			autoAdvancing = true;
			base.Overworld.ShowInputUI = false;
			Focused = false;
			disableInput = true;
			Add(new Coroutine(AutoAdvanceRoutine()));
		}

		private IEnumerator AutoAdvanceRoutine()
		{
			if (area < SaveData.Instance.MaxArea)
			{
				int nextArea = area + 1;
				if (nextArea == 9 || nextArea == 10)
				{
					icons[nextArea].HideIcon = true;
				}
				while (!base.Selected)
				{
					yield return null;
				}
				yield return 1f;
				switch (nextArea)
				{
				case 10:
					yield return PerformCh9Unlock();
					break;
				case 9:
					yield return PerformCh8Unlock();
					break;
				default:
					Audio.Play("event:/ui/postgame/unlock_newchapter");
					Audio.Play("event:/ui/world_map/icon/roll_right");
					area = nextArea;
					EaseCamera();
					base.Overworld.Maddy.Hide();
					break;
				}
				yield return 0.25f;
			}
			autoAdvancing = false;
			disableInput = false;
			Focused = true;
			base.Overworld.ShowInputUI = true;
		}

		public override void Update()
		{
			if (Focused && !disableInput)
			{
				inputDelay -= Engine.DeltaTime;
				if (area >= 0 && area < AreaData.Areas.Count)
				{
					Input.SetLightbarColor(AreaData.Get(area).TitleBaseColor);
				}
				if (Input.MenuCancel.Pressed)
				{
					Audio.Play("event:/ui/main/button_back");
					base.Overworld.Goto<OuiMainMenu>();
					base.Overworld.Maddy.Hide();
				}
				else if (Input.MenuJournal.Pressed && journalEnabled)
				{
					Audio.Play("event:/ui/world_map/journal/select");
					base.Overworld.Goto<OuiJournal>();
				}
				else if (inputDelay <= 0f)
				{
					if (area > 0 && Input.MenuLeft.Pressed)
					{
						Audio.Play("event:/ui/world_map/icon/roll_left");
						inputDelay = 0.15f;
						area--;
						icons[area].Hovered(-1);
						EaseCamera();
						base.Overworld.Maddy.Hide();
					}
					else if (Input.MenuRight.Pressed)
					{
						bool assistModeCanMoveRight = SaveData.Instance.AssistMode && area == SaveData.Instance.UnlockedAreas && area < SaveData.Instance.MaxAssistArea;
						if (area < SaveData.Instance.UnlockedAreas || assistModeCanMoveRight)
						{
							Audio.Play("event:/ui/world_map/icon/roll_right");
							inputDelay = 0.15f;
							area++;
							icons[area].Hovered(1);
							if (area <= SaveData.Instance.UnlockedAreas)
							{
								EaseCamera();
							}
							base.Overworld.Maddy.Hide();
						}
					}
					else if (Input.MenuConfirm.Pressed)
					{
						if (icons[area].AssistModeUnlockable)
						{
							Audio.Play("event:/ui/world_map/icon/assist_skip");
							Focused = false;
							base.Overworld.ShowInputUI = false;
							icons[area].AssistModeUnlock(delegate
							{
								Focused = true;
								base.Overworld.ShowInputUI = true;
								EaseCamera();
								if (area == 10)
								{
									SaveData.Instance.RevealedChapter9 = true;
								}
								if (area < SaveData.Instance.MaxAssistArea)
								{
									OuiChapterSelectIcon ouiChapterSelectIcon = icons[area + 1];
									ouiChapterSelectIcon.AssistModeUnlockable = true;
									ouiChapterSelectIcon.Position = ouiChapterSelectIcon.HiddenPosition;
									ouiChapterSelectIcon.Show();
								}
							});
						}
						else
						{
							Audio.Play("event:/ui/world_map/icon/select");
							SaveData.Instance.LastArea.Mode = AreaMode.Normal;
							base.Overworld.Goto<OuiChapterPanel>();
						}
					}
				}
			}
			ease = Calc.Approach(ease, display ? 1f : 0f, Engine.DeltaTime * 3f);
			journalEase = Calc.Approach(journalEase, (display && !disableInput && Focused && journalEnabled) ? 1f : 0f, Engine.DeltaTime * 4f);
			base.Update();
		}

		public override void Render()
		{
			Vector2 pos2 = new Vector2(960f, (float)(-scarf.Height) * Ease.CubeInOut(1f - ease));
			for (int i = 0; i < scarfSegments.Length; i++)
			{
				float e = Ease.CubeIn((float)i / (float)scarfSegments.Length);
				float wave = e * (float)Math.Sin(base.Scene.RawTimeActive * 4f + (float)i * 0.05f) * 4f - e * 16f;
				scarfSegments[i].DrawJustified(pos2 + new Vector2(wave, i * 2), new Vector2(0.5f, 0f));
			}
			if (journalEase > 0f)
			{
				Vector2 pos = new Vector2(128f * Ease.CubeOut(journalEase), 952f);
				GFX.Gui["menu/journal"].DrawCentered(pos, Color.White * Ease.CubeOut(journalEase));
				Input.GuiButton(Input.MenuJournal).Draw(pos, Vector2.Zero, Color.White * Ease.CubeOut(journalEase));
			}
		}

		private void EaseCamera()
		{
			AreaData data = AreaData.Areas[area];
			base.Overworld.Mountain.EaseCamera(area, data.MountainIdle, null, nearTarget: true, area == 10);
			base.Overworld.Mountain.Model.EaseState(data.MountainState);
		}

		private IEnumerator PerformCh8Unlock()
		{
			Audio.Play("event:/ui/postgame/unlock_newchapter");
			Audio.Play("event:/ui/world_map/icon/roll_right");
			area = 9;
			EaseCamera();
			base.Overworld.Maddy.Hide();
			bool ready = false;
			icons[9].HighlightUnlock(delegate
			{
				ready = true;
			});
			while (!ready)
			{
				yield return null;
			}
		}

		private IEnumerator SetupCh9Unlock()
		{
			icons[10].HideIcon = true;
			yield return 0.25f;
			while (area < 9)
			{
				area++;
				yield return 0.1f;
			}
		}

		private IEnumerator PerformCh9Unlock(bool easeCamera = true)
		{
			Audio.Play("event:/ui/postgame/unlock_newchapter");
			Audio.Play("event:/ui/world_map/icon/roll_right");
			area = 10;
			yield return 0.25f;
			bool ready = false;
			icons[10].HighlightUnlock(delegate
			{
				ready = true;
			});
			while (!ready)
			{
				yield return null;
			}
			if (easeCamera)
			{
				EaseCamera();
			}
			base.Overworld.Maddy.Hide();
			SaveData.Instance.RevealedChapter9 = true;
		}
	}
}
