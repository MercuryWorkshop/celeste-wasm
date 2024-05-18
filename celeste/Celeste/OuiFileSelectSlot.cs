using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class OuiFileSelectSlot : Entity
	{
		private class Button
		{
			public string Label;

			public Action Action;

			public float Scale = 1f;
		}

		public SaveData SaveData;

		public int FileSlot;

		public string Name;

		public bool AssistModeEnabled;

		public bool VariantModeEnabled;

		public bool Exists;

		public bool Corrupted;

		public string Time;

		public int FurthestArea;

		public Sprite Portrait;

		public bool HasBlackgems;

		public StrawberriesCounter Strawberries;

		public DeathsCounter Deaths;

		public List<bool> Cassettes = new List<bool>();

		public List<bool[]> HeartGems = new List<bool[]>();

		private const int height = 300;

		private const int spacing = 10;

		private const float portraitSize = 200f;

		public bool StartingGame;

		public bool Renaming;

		public bool Assisting;

		private OuiFileSelect fileSelect;

		private bool deleting;

		private float highlightEase;

		private float highlightEaseDelay;

		private float selectedEase;

		private float deletingEase;

		private Tween tween;

		private int buttonIndex;

		private int deleteIndex;

		private Wiggler wiggler;

		private float failedToDeleteEase;

		private float failedToDeleteTimer;

		private float screenFlash;

		private float inputDelay;

		private float newgameFade;

		private float timeScale = 1f;

		private Button assistButton;

		private Button variantButton;

		private Sprite normalCard;

		private Sprite goldCard;

		private Sprite normalTicket;

		private Sprite goldTicket;

		private List<Button> buttons = new List<Button>();

		public Vector2 IdlePosition => new Vector2(960f, 540 + 310 * (FileSlot - 1));

		public Vector2 SelectedPosition => new Vector2(960f, 440f);

		private bool highlighted => fileSelect.SlotIndex == FileSlot;

		private bool selected
		{
			get
			{
				if (fileSelect.SlotSelected)
				{
					return highlighted;
				}
				return false;
			}
		}

		private bool Golden
		{
			get
			{
				if (!Corrupted && Exists)
				{
					return SaveData.TotalStrawberries >= 202;
				}
				return false;
			}
		}

		private Sprite Card
		{
			get
			{
				if (!Golden)
				{
					return normalCard;
				}
				return goldCard;
			}
		}

		private Sprite Ticket
		{
			get
			{
				if (!Golden)
				{
					return normalTicket;
				}
				return goldTicket;
			}
		}

		private OuiFileSelectSlot(int index, OuiFileSelect fileSelect)
		{
			FileSlot = index;
			this.fileSelect = fileSelect;
			base.Tag |= (int)Tags.HUD | (int)Tags.PauseUpdate;
			Visible = false;
			Add(wiggler = Wiggler.Create(0.4f, 4f));
			normalTicket = new Sprite(MTN.FileSelect, "ticket");
			normalTicket.AddLoop("idle", "", 0.1f);
			normalTicket.Add("shine", "", 0.1f, "idle");
			normalTicket.CenterOrigin();
			normalTicket.Play("idle");
			normalCard = new Sprite(MTN.FileSelect, "card");
			normalCard.AddLoop("idle", "", 0.1f);
			normalCard.Add("shine", "", 0.1f, "idle");
			normalCard.CenterOrigin();
			normalCard.Play("idle");
			goldTicket = new Sprite(MTN.FileSelect, "ticketShine");
			goldTicket.AddLoop("idle", "", 0.1f, default(int));
			goldTicket.Add("shine", "", 0.05f, "idle", 0, 0, 0, 0, 0, 1, 2, 3, 4, 5);
			goldTicket.CenterOrigin();
			goldTicket.Play("idle");
			goldCard = new Sprite(MTN.FileSelect, "cardShine");
			goldCard.AddLoop("idle", "", 0.1f, 5);
			goldCard.Add("shine", "", 0.05f, "idle");
			goldCard.CenterOrigin();
			goldCard.Play("idle");
		}

		public OuiFileSelectSlot(int index, OuiFileSelect fileSelect, bool corrupted)
			: this(index, fileSelect)
		{
			Corrupted = corrupted;
			Exists = corrupted;
			Setup();
		}

		public OuiFileSelectSlot(int index, OuiFileSelect fileSelect, SaveData data)
			: this(index, fileSelect)
		{
			Exists = true;
			SaveData = data;
			Name = data.Name;
			if (!Dialog.Language.CanDisplay(Name))
			{
				Name = Dialog.Clean("FILE_DEFAULT");
			}
			if (!Settings.Instance.VariantsUnlocked && data.TotalHeartGems >= 24)
			{
				Settings.Instance.VariantsUnlocked = true;
			}
			AssistModeEnabled = data.AssistMode;
			VariantModeEnabled = data.VariantMode;
			Add(Deaths = new DeathsCounter(AreaMode.Normal, centeredX: false, data.TotalDeaths));
			Add(Strawberries = new StrawberriesCounter(centeredX: true, data.TotalStrawberries));
			Time = Dialog.FileTime(data.Time);
			if (TimeSpan.FromTicks(data.Time).TotalHours > 0.0)
			{
				timeScale = 0.725f;
			}
			FurthestArea = data.UnlockedAreas;
			foreach (AreaStats area in data.Areas)
			{
				if (area.ID > data.UnlockedAreas)
				{
					break;
				}
				if (!AreaData.Areas[area.ID].Interlude && AreaData.Areas[area.ID].CanFullClear)
				{
					bool[] gems = new bool[3];
					for (int i = 0; i < gems.Length; i++)
					{
						gems[i] = area.Modes[i].HeartGem;
					}
					Cassettes.Add(area.Cassette);
					HeartGems.Add(gems);
				}
			}
			Setup();
		}

		private void Setup()
		{
			string portrait = "portrait_madeline";
			string portraitAnim = "idle_normal";
			Portrait = GFX.PortraitsSpriteBank.Create(portrait);
			Portrait.Play(portraitAnim);
			Portrait.Scale = Vector2.One * (200f / (float)GFX.PortraitsSpriteBank.SpriteData[portrait].Sources[0].XML.AttrInt("size", 160));
			Add(Portrait);
		}

		public void CreateButtons()
		{
			buttons.Clear();
			if (Exists)
			{
				if (!Corrupted)
				{
					buttons.Add(new Button
					{
						Label = Dialog.Clean("file_continue"),
						Action = OnContinueSelected
					});
					if (SaveData != null)
					{
						List<Button> list = buttons;
						Button obj = new Button
						{
							Label = Dialog.Clean("FILE_ASSIST_" + (AssistModeEnabled ? "ON" : "OFF")),
							Action = OnAssistSelected,
							Scale = 0.7f
						};
						Button item = obj;
						assistButton = obj;
						list.Add(item);
						if (Settings.Instance.VariantsUnlocked || SaveData.CheatMode)
						{
							List<Button> list2 = buttons;
							Button obj2 = new Button
							{
								Label = Dialog.Clean("FILE_VARIANT_" + (VariantModeEnabled ? "ON" : "OFF")),
								Action = OnVariantSelected,
								Scale = 0.7f
							};
							item = obj2;
							variantButton = obj2;
							list2.Add(item);
						}
					}
				}
				buttons.Add(new Button
				{
					Label = Dialog.Clean("file_delete"),
					Action = OnDeleteSelected,
					Scale = 0.7f
				});
			}
			else
			{
				buttons.Add(new Button
				{
					Label = Dialog.Clean("file_begin"),
					Action = OnNewGameSelected
				});
				buttons.Add(new Button
				{
					Label = Dialog.Clean("file_rename"),
					Action = OnRenameSelected,
					Scale = 0.7f
				});
				List<Button> list3 = buttons;
				Button obj3 = new Button
				{
					Label = Dialog.Clean("FILE_ASSIST_" + (AssistModeEnabled ? "ON" : "OFF")),
					Action = OnAssistSelected,
					Scale = 0.7f
				};
				Button item = obj3;
				assistButton = obj3;
				list3.Add(item);
				if (Settings.Instance.VariantsUnlocked)
				{
					List<Button> list4 = buttons;
					Button obj4 = new Button
					{
						Label = Dialog.Clean("FILE_VARIANT_" + (VariantModeEnabled ? "ON" : "OFF")),
						Action = OnVariantSelected,
						Scale = 0.7f
					};
					item = obj4;
					variantButton = obj4;
					list4.Add(item);
				}
			}
		}

		private void OnContinueSelected()
		{
			StartingGame = true;
			Audio.Play("event:/ui/main/savefile_begin");
			SaveData.Start(SaveData, FileSlot);
			SaveData.Instance.AssistMode = AssistModeEnabled;
			SaveData.Instance.VariantMode = VariantModeEnabled;
			SaveData.Instance.AssistModeChecks();
			if (SaveData.Instance.CurrentSession != null && SaveData.Instance.CurrentSession.InArea)
			{
				Audio.SetMusic(null);
				Audio.SetAmbience(null);
				fileSelect.Overworld.ShowInputUI = false;
				new FadeWipe(base.Scene, wipeIn: false, delegate
				{
					LevelEnter.Go(SaveData.Instance.CurrentSession, fromSaveData: true);
				});
			}
			else if (SaveData.Instance.Areas[0].Modes[0].Completed || SaveData.Instance.CheatMode)
			{
				if (SaveData.Instance.CurrentSession != null && SaveData.Instance.CurrentSession.ShouldAdvance)
				{
					SaveData.Instance.LastArea.ID = SaveData.Instance.UnlockedAreas;
				}
				SaveData.Instance.CurrentSession = null;
				(base.Scene as Overworld).Goto<OuiChapterSelect>();
			}
			else
			{
				Audio.SetMusic(null);
				Audio.SetAmbience(null);
				EnterFirstArea();
			}
		}

		private void OnDeleteSelected()
		{
			deleting = true;
			wiggler.Start();
			Audio.Play("event:/ui/main/message_confirm");
		}

		private void OnNewGameSelected()
		{
			Audio.SetMusic(null);
			Audio.SetAmbience(null);
			Audio.Play("event:/ui/main/savefile_begin");
			SaveData.Start(new SaveData
			{
				Name = Name,
				AssistMode = AssistModeEnabled,
				VariantMode = VariantModeEnabled
			}, FileSlot);
			StartingGame = true;
			EnterFirstArea();
		}

		private void EnterFirstArea()
		{
			fileSelect.Overworld.Maddy.Disabled = true;
			fileSelect.Overworld.ShowInputUI = false;
			Add(new Coroutine(EnterFirstAreaRoutine()));
		}

		private IEnumerator EnterFirstAreaRoutine()
		{
			Overworld overworld = fileSelect.Overworld;
			yield return fileSelect.Leave(null);
			yield return overworld.Mountain.EaseCamera(0, AreaData.Areas[0].MountainIdle);
			yield return 0.3f;
			overworld.Mountain.EaseCamera(0, AreaData.Areas[0].MountainZoom, 1f);
			yield return 0.4f;
			AreaData.Areas[0].Wipe(overworld, arg2: false, null);
			overworld.RendererList.UpdateLists();
			overworld.RendererList.MoveToFront(overworld.Snow);
			yield return 0.5f;
			LevelEnter.Go(new Session(new AreaKey(0)), fromSaveData: false);
		}

		private void OnRenameSelected()
		{
			Renaming = true;
			OuiFileNaming ouiFileNaming = fileSelect.Overworld.Goto<OuiFileNaming>();
			ouiFileNaming.FileSlot = this;
			ouiFileNaming.StartingName = Name;
			Audio.Play("event:/ui/main/savefile_rename_start");
		}

		private void OnAssistSelected()
		{
			Assisting = true;
			fileSelect.Overworld.Goto<OuiAssistMode>().FileSlot = this;
			Audio.Play("event:/ui/main/assist_button_info");
		}

		private void OnVariantSelected()
		{
			if (Settings.Instance.VariantsUnlocked || (SaveData != null && SaveData.CheatMode))
			{
				VariantModeEnabled = !VariantModeEnabled;
				if (VariantModeEnabled)
				{
					AssistModeEnabled = false;
					Audio.Play("event:/ui/main/button_toggle_on");
				}
				else
				{
					Audio.Play("event:/ui/main/button_toggle_off");
				}
				assistButton.Label = Dialog.Clean("FILE_ASSIST_" + (AssistModeEnabled ? "ON" : "OFF"));
				variantButton.Label = Dialog.Clean("FILE_VARIANT_" + (VariantModeEnabled ? "ON" : "OFF"));
			}
		}

		public Vector2 HiddenPosition(int x, int y)
		{
			if (!selected)
			{
				return new Vector2(960f, base.Y) + new Vector2(x, y) * new Vector2(1920f, 1080f);
			}
			return new Vector2(1920f, 1080f) / 2f + new Vector2(x, y) * new Vector2(1920f, 1080f);
		}

		public void Show()
		{
			Visible = true;
			deleting = false;
			StartingGame = false;
			Renaming = false;
			Assisting = false;
			selectedEase = 0f;
			highlightEase = 0f;
			highlightEaseDelay = 0.35f;
			Vector2 from = Position;
			StartTween(0.25f, delegate(Tween f)
			{
				Position = Vector2.Lerp(from, IdlePosition, f.Eased);
			});
		}

		public void Select(bool resetButtonIndex)
		{
			Visible = true;
			deleting = false;
			StartingGame = false;
			Renaming = false;
			Assisting = false;
			CreateButtons();
			Card.Play("shine");
			Ticket.Play("shine");
			Vector2 from = Position;
			wiggler.Start();
			if (resetButtonIndex)
			{
				buttonIndex = 0;
			}
			deleteIndex = 1;
			inputDelay = 0.1f;
			StartTween(0.25f, delegate(Tween f)
			{
				Position = Vector2.Lerp(from, SelectedPosition, selectedEase = f.Eased);
				newgameFade = Math.Max(newgameFade, f.Eased);
			});
		}

		public void Unselect()
		{
			Vector2 from = Position;
			buttonIndex = 0;
			StartTween(0.25f, delegate(Tween f)
			{
				selectedEase = 1f - f.Eased;
				newgameFade = 1f - f.Eased;
				Position = Vector2.Lerp(from, IdlePosition, f.Eased);
			});
		}

		public void MoveTo(float x, float y)
		{
			Vector2 from = Position;
			Vector2 to = new Vector2(x, y);
			StartTween(0.25f, delegate(Tween f)
			{
				Position = Vector2.Lerp(from, to, f.Eased);
			});
		}

		public void Hide(int x, int y)
		{
			Vector2 from = Position;
			Vector2 to = HiddenPosition(x, y);
			StartTween(0.25f, delegate(Tween f)
			{
				Position = Vector2.Lerp(from, to, f.Eased);
			}, hide: true);
		}

		private void StartTween(float duration, Action<Tween> callback, bool hide = false)
		{
			if (tween != null && tween.Entity == this)
			{
				tween.RemoveSelf();
			}
			Add(tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, duration));
			tween.OnUpdate = callback;
			tween.OnComplete = delegate
			{
				if (hide)
				{
					Visible = false;
				}
				tween = null;
			};
			tween.Start();
		}

		public override void Update()
		{
			inputDelay -= Engine.DeltaTime;
			Ticket.Update();
			Card.Update();
			if (selected && fileSelect.Selected && fileSelect.Focused && !StartingGame && tween == null && inputDelay <= 0f && !StartingGame)
			{
				if (deleting)
				{
					if (Input.MenuCancel.Pressed)
					{
						deleting = false;
						wiggler.Start();
						Audio.Play("event:/ui/main/button_back");
					}
					else if (Input.MenuUp.Pressed && deleteIndex > 0)
					{
						deleteIndex = 0;
						wiggler.Start();
						Audio.Play("event:/ui/main/rollover_up");
					}
					else if (Input.MenuDown.Pressed && deleteIndex < 1)
					{
						deleteIndex = 1;
						wiggler.Start();
						Audio.Play("event:/ui/main/rollover_down");
					}
					else if (Input.MenuConfirm.Pressed)
					{
						if (deleteIndex == 1)
						{
							deleting = false;
							wiggler.Start();
							Audio.Play("event:/ui/main/button_back");
						}
						else if (SaveData.TryDelete(FileSlot))
						{
							Exists = false;
							Corrupted = false;
							deleting = false;
							deletingEase = 0f;
							fileSelect.UnselectHighlighted();
							Audio.Play("event:/ui/main/savefile_delete");
							if (!Settings.Instance.DisableFlashes)
							{
								screenFlash = 1f;
							}
							CreateButtons();
						}
						else
						{
							failedToDeleteEase = 0f;
							failedToDeleteTimer = 3f;
							Audio.Play("event:/ui/main/button_invalid");
						}
					}
				}
				else if (Input.MenuCancel.Pressed)
				{
					if (fileSelect.HasSlots)
					{
						fileSelect.UnselectHighlighted();
						Audio.Play("event:/ui/main/whoosh_savefile_in");
						Audio.Play("event:/ui/main/button_back");
					}
				}
				else if (Input.MenuUp.Pressed && buttonIndex > 0)
				{
					buttonIndex--;
					wiggler.Start();
					Audio.Play("event:/ui/main/rollover_up");
				}
				else if (Input.MenuDown.Pressed && buttonIndex < buttons.Count - 1)
				{
					buttonIndex++;
					wiggler.Start();
					Audio.Play("event:/ui/main/rollover_down");
				}
				else if (Input.MenuConfirm.Pressed)
				{
					buttons[buttonIndex].Action();
				}
			}
			if (highlightEaseDelay <= 0f)
			{
				highlightEase = Calc.Approach(highlightEase, (highlighted && (Exists || !selected)) ? 1f : 0f, Engine.DeltaTime * 4f);
			}
			else
			{
				highlightEaseDelay -= Engine.DeltaTime;
			}
			base.Depth = (highlighted ? (-10) : 0);
			if (Renaming || Assisting)
			{
				selectedEase = Calc.Approach(selectedEase, 0f, Engine.DeltaTime * 4f);
			}
			deletingEase = Calc.Approach(deletingEase, deleting ? 1f : 0f, Engine.DeltaTime * 4f);
			failedToDeleteEase = Calc.Approach(failedToDeleteEase, (failedToDeleteTimer > 0f) ? 1f : 0f, Engine.DeltaTime * 4f);
			failedToDeleteTimer -= Engine.DeltaTime;
			screenFlash = Calc.Approach(screenFlash, 0f, Engine.DeltaTime * 4f);
			base.Update();
		}

		public override void Render()
		{
			float ease = Ease.CubeInOut(highlightEase);
			float w = wiggler.Value * 8f;
			if (selectedEase > 0f)
			{
				Vector2 buttonPositions = Position + new Vector2(0f, -150f + 350f * selectedEase);
				float lh2 = ActiveFont.LineHeight;
				for (int j = 0; j < buttons.Count; j++)
				{
					Button button = buttons[j];
					Vector2 wiggle = Vector2.UnitX * ((buttonIndex == j && !deleting) ? w : 0f);
					Color selectedColor = SelectionColor(buttonIndex == j && !deleting);
					ActiveFont.DrawOutline(button.Label, buttonPositions + wiggle, new Vector2(0.5f, 0f), Vector2.One * button.Scale, selectedColor, 2f, Color.Black);
					buttonPositions.Y += lh2 * button.Scale + 15f;
				}
			}
			Vector2 slide2 = Position + Vector2.UnitX * ease * 360f;
			Ticket.RenderPosition = slide2;
			Ticket.Render();
			if (highlightEase > 0f && Exists && !Corrupted)
			{
				int left = -280;
				int width = 600;
				for (int i = 0; i < Cassettes.Count; i++)
				{
					MTN.FileSelect[Cassettes[i] ? "cassette" : "dot"].DrawCentered(slide2 + new Vector2((float)left + ((float)i + 0.5f) * 75f, -75f));
					bool[] gems = HeartGems[i];
					int has = 0;
					for (int l = 0; l < gems.Length; l++)
					{
						if (gems[l])
						{
							has++;
						}
					}
					Vector2 pos = slide2 + new Vector2((float)left + ((float)i + 0.5f) * 75f, -12f);
					if (has == 0)
					{
						MTN.FileSelect["dot"].DrawCentered(pos);
						continue;
					}
					pos.Y -= (float)(has - 1) * 0.5f * 14f;
					int k = 0;
					int o = 0;
					for (; k < gems.Length; k++)
					{
						if (gems[k])
						{
							MTN.FileSelect["heartgem" + k].DrawCentered(pos + new Vector2(0f, o * 14));
							o++;
						}
					}
				}
				Deaths.Position = slide2 + new Vector2(left, 68f) - Position;
				Deaths.Render();
				ActiveFont.Draw(Time, slide2 + new Vector2(left + width, 68f), new Vector2(1f, 0.5f), Vector2.One * timeScale, Color.Black * 0.6f);
			}
			else if (Corrupted)
			{
				ActiveFont.Draw(Dialog.Clean("file_corrupted"), slide2, new Vector2(0.5f, 0.5f), Vector2.One, Color.Black * 0.8f);
			}
			else if (!Exists)
			{
				ActiveFont.Draw(Dialog.Clean("file_newgame"), slide2, new Vector2(0.5f, 0.5f), Vector2.One, Color.Black * 0.8f);
			}
			Vector2 slide = Position - Vector2.UnitX * ease * 360f;
			int padding = 64;
			int portraitPadding = 16;
			float remainingSize = Card.Width - (float)(padding * 2) - 200f - (float)portraitPadding;
			float center2 = (0f - Card.Width) / 2f + (float)padding + 200f + (float)portraitPadding + remainingSize / 2f;
			float fade = (Exists ? 1f : newgameFade);
			if (!Corrupted)
			{
				if (newgameFade > 0f || Exists)
				{
					if (AssistModeEnabled)
					{
						MTN.FileSelect["assist"].DrawCentered(slide, Color.White * fade);
					}
					else if (VariantModeEnabled)
					{
						MTN.FileSelect["variants"].DrawCentered(slide, Color.White * fade);
					}
				}
				if (Exists && SaveData.CheatMode)
				{
					MTN.FileSelect["cheatmode"].DrawCentered(slide, Color.White * fade);
				}
			}
			Card.RenderPosition = slide;
			Card.Render();
			if (!Corrupted)
			{
				if (Exists)
				{
					if (SaveData.TotalStrawberries >= 175)
					{
						MTN.FileSelect["strawberry"].DrawCentered(slide, Color.White * fade);
					}
					if (SaveData.Areas.Count > 7 && SaveData.Areas[7].Modes[0].Completed)
					{
						MTN.FileSelect["flag"].DrawCentered(slide, Color.White * fade);
					}
					if (SaveData.TotalCassettes >= 8)
					{
						MTN.FileSelect["cassettes"].DrawCentered(slide, Color.White * fade);
					}
					if (SaveData.TotalHeartGems >= 16)
					{
						MTN.FileSelect["heart"].DrawCentered(slide, Color.White * fade);
					}
					if (SaveData.TotalGoldenStrawberries >= 25)
					{
						MTN.FileSelect["goldberry"].DrawCentered(slide, Color.White * fade);
					}
					if (SaveData.TotalHeartGems >= 24)
					{
						MTN.FileSelect["goldheart"].DrawCentered(slide, Color.White * fade);
					}
					if (SaveData.Areas.Count > 10 && SaveData.Areas[10].Modes[0].Completed)
					{
						MTN.FileSelect["farewell"].DrawCentered(slide, Color.White * fade);
					}
				}
				if (Exists || Renaming || newgameFade > 0f)
				{
					Portrait.RenderPosition = slide + new Vector2((0f - Card.Width) / 2f + (float)padding + 100f, 0f);
					Portrait.Color = Color.White * fade;
					Portrait.Render();
					MTN.FileSelect[(!Golden) ? "portraitOverlay" : "portraitOverlayGold"].DrawCentered(Portrait.RenderPosition, Color.White * fade);
					string name = Name;
					Vector2 namePosition = slide + new Vector2(center2, -32 + ((!Exists) ? 64 : 0));
					float nameSize = Math.Min(1f, 440f / ActiveFont.Measure(name).X);
					ActiveFont.Draw(name, namePosition, new Vector2(0.5f, 1f), Vector2.One * nameSize, Color.Black * 0.8f * fade);
					if (Renaming && base.Scene.BetweenInterval(0.3f))
					{
						ActiveFont.Draw("|", new Vector2(namePosition.X + ActiveFont.Measure(name).X * nameSize * 0.5f, namePosition.Y), new Vector2(0f, 1f), Vector2.One * nameSize, Color.Black * 0.8f * fade);
					}
				}
				if (Exists)
				{
					if (FurthestArea < AreaData.Areas.Count)
					{
						ActiveFont.Draw(Dialog.Clean(AreaData.Areas[FurthestArea].Name), slide + new Vector2(center2, -10f), new Vector2(0.5f, 0.5f), Vector2.One * 0.8f, Color.Black * 0.6f);
					}
					Strawberries.Position = slide + new Vector2(center2, 55f) - Position;
					Strawberries.Render();
				}
			}
			else
			{
				ActiveFont.Draw(Dialog.Clean("file_failedtoload"), slide, new Vector2(0.5f, 0.5f), Vector2.One, Color.Black * 0.8f);
			}
			if (deletingEase > 0f)
			{
				float e = Ease.CubeOut(deletingEase);
				Vector2 center = new Vector2(960f, 540f);
				float lh = ActiveFont.LineHeight;
				Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * e * 0.9f);
				ActiveFont.Draw(Dialog.Clean("file_delete_really"), center + new Vector2(0f, -16f - 64f * (1f - e)), new Vector2(0.5f, 1f), Vector2.One, Color.White * e);
				ActiveFont.DrawOutline(Dialog.Clean("file_delete_yes"), center + new Vector2(((deleting && deleteIndex == 0) ? w : 0f) * 1.2f * e, 16f + 64f * (1f - e)), new Vector2(0.5f, 0f), Vector2.One * 0.8f, deleting ? SelectionColor(deleteIndex == 0) : Color.Gray, 2f, Color.Black * e);
				ActiveFont.DrawOutline(Dialog.Clean("file_delete_no"), center + new Vector2(((deleting && deleteIndex == 1) ? w : 0f) * 1.2f * e, 16f + lh + 64f * (1f - e)), new Vector2(0.5f, 0f), Vector2.One * 0.8f, deleting ? SelectionColor(deleteIndex == 1) : Color.Gray, 2f, Color.Black * e);
				if (failedToDeleteEase > 0f)
				{
					Vector2 failedPos = new Vector2(960f, 980f - 100f * deletingEase);
					Vector2 failedScale = Vector2.One * 0.8f;
					if (failedToDeleteEase < 1f && failedToDeleteTimer > 0f)
					{
						failedPos += new Vector2(-5 + Calc.Random.Next(10), -5 + Calc.Random.Next(10));
						failedScale = Vector2.One * (0.8f + 0.2f * (1f - failedToDeleteEase));
					}
					ActiveFont.DrawOutline(Dialog.Clean("file_delete_failed"), failedPos, new Vector2(0.5f, 0f), failedScale, Color.PaleVioletRed * deletingEase, 2f, Color.Black * deletingEase);
				}
			}
			if (screenFlash > 0f)
			{
				Draw.Rect(-10f, -10f, 1940f, 1100f, Color.White * Ease.CubeOut(screenFlash));
			}
		}

		public Color SelectionColor(bool selected)
		{
			if (selected)
			{
				if (!Settings.Instance.DisableFlashes && !base.Scene.BetweenInterval(0.1f))
				{
					return TextMenu.HighlightColorB;
				}
				return TextMenu.HighlightColorA;
			}
			return Color.White;
		}
	}
}
