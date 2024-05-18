using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste
{
	public class OuiChapterPanel : Oui
	{
		private class Option
		{
			public string Label;

			public string ID;

			public MTexture Icon;

			public MTexture Bg = GFX.Gui["areaselect/tab"];

			public Color BgColor = Calc.HexToColor("3c6180");

			public float Pop;

			public bool Large = true;

			public int Siblings;

			public float Slide;

			public float Appear = 1f;

			public float IconEase = 1f;

			public bool Appeared;

			public float Faded;

			public float CheckpointSlideOut;

			public string CheckpointLevelName;

			public float CheckpointRotation;

			public Vector2 CheckpointOffset;

			public float Scale
			{
				get
				{
					if (Siblings < 5)
					{
						return 1f;
					}
					return 0.8f;
				}
			}

			public bool OnTopOfUI => Pop > 0.5f;

			public void SlideTowards(int i, int count, bool snap)
			{
				float halfcount = (float)count / 2f - 0.5f;
				float target = (float)i - halfcount;
				if (snap)
				{
					Slide = target;
				}
				else
				{
					Slide = Calc.Approach(Slide, target, Engine.DeltaTime * 4f);
				}
			}

			public Vector2 GetRenderPosition(Vector2 center)
			{
				float size = (float)(Large ? 170 : 130) * Scale;
				if (Siblings > 0 && size * (float)Siblings > 750f)
				{
					size = 750 / Siblings;
				}
				Vector2 pos = center + new Vector2(Slide * size, (float)Math.Sin(Pop * (float)Math.PI) * 70f - Pop * 12f);
				pos.Y += (1f - Ease.CubeOut(Appear)) * -200f;
				pos.Y -= (1f - Scale) * 80f;
				return pos;
			}

			public void Render(Vector2 center, bool selected, Wiggler wiggler, Wiggler appearWiggler)
			{
				float s = Scale + (selected ? (wiggler.Value * 0.25f) : 0f) + (Appeared ? (appearWiggler.Value * 0.25f) : 0f);
				Vector2 pos = GetRenderPosition(center);
				Color bgColor = Color.Lerp(BgColor, Color.Black, (1f - Pop) * 0.6f);
				Bg.DrawCentered(pos + new Vector2(0f, 10f), bgColor, (Appeared ? Scale : s) * new Vector2(Large ? 1f : 0.9f, 1f));
				if (IconEase > 0f)
				{
					float ease = Ease.CubeIn(IconEase);
					Color iconColor = Color.Lerp(Color.White, Color.Black, Faded * 0.6f) * ease;
					Icon.DrawCentered(pos, iconColor, (float)(Bg.Width - 50) / (float)Icon.Width * s * (2.5f - ease * 1.5f));
				}
			}
		}

		public AreaKey Area;

		public AreaStats RealStats;

		public AreaStats DisplayedStats;

		public AreaData Data;

		public Overworld.StartMode OverworldStartMode;

		public bool EnteringChapter;

		public const int ContentOffsetX = 440;

		public const int NoStatsHeight = 300;

		public const int StatsHeight = 540;

		public const int CheckpointsHeight = 730;

		private bool initialized;

		private string chapter = "";

		private bool selectingMode = true;

		private float height;

		private bool resizing;

		private Wiggler wiggler;

		private Wiggler modeAppearWiggler;

		private MTexture card = new MTexture();

		private Vector2 contentOffset;

		private float spotlightRadius;

		private float spotlightAlpha;

		private Vector2 spotlightPosition;

		private AreaCompleteTitle remixUnlockText;

		private StrawberriesCounter strawberries = new StrawberriesCounter(centeredX: true, 0, 0, showOutOf: true);

		private Vector2 strawberriesOffset;

		private DeathsCounter deaths = new DeathsCounter(AreaMode.Normal, centeredX: true, 0);

		private Vector2 deathsOffset;

		private HeartGemDisplay heart = new HeartGemDisplay(0, hasGem: false);

		private Vector2 heartOffset;

		private int checkpoint;

		private List<Option> modes = new List<Option>();

		private List<Option> checkpoints = new List<Option>();

		private EventInstance bSideUnlockSfx;

		public Vector2 OpenPosition => new Vector2(1070f, 100f);

		public Vector2 ClosePosition => new Vector2(2220f, 100f);

		public Vector2 IconOffset => new Vector2(690f, 86f);

		private Vector2 OptionsRenderPosition => Position + new Vector2(contentOffset.X, 128f + height);

		private int option
		{
			get
			{
				if (!selectingMode)
				{
					return checkpoint;
				}
				return (int)Area.Mode;
			}
			set
			{
				if (selectingMode)
				{
					Area.Mode = (AreaMode)value;
				}
				else
				{
					checkpoint = value;
				}
			}
		}

		private List<Option> options
		{
			get
			{
				if (!selectingMode)
				{
					return checkpoints;
				}
				return modes;
			}
		}

		public OuiChapterPanel()
		{
			Add(strawberries);
			Add(deaths);
			Add(heart);
			deaths.CanWiggle = false;
			strawberries.CanWiggle = false;
			strawberries.OverworldSfx = true;
			Add(wiggler = Wiggler.Create(0.4f, 4f));
			Add(modeAppearWiggler = Wiggler.Create(0.4f, 4f));
		}

		public override bool IsStart(Overworld overworld, Overworld.StartMode start)
		{
			if (start == Overworld.StartMode.AreaComplete || start == Overworld.StartMode.AreaQuit)
			{
				bool shouldAutoAdvance = start == Overworld.StartMode.AreaComplete && (Celeste.PlayMode == Celeste.PlayModes.Event || (SaveData.Instance.CurrentSession != null && SaveData.Instance.CurrentSession.ShouldAdvance));
				Position = OpenPosition;
				Reset();
				Add(new Coroutine(IncrementStats(shouldAutoAdvance)));
				overworld.ShowInputUI = false;
				overworld.Mountain.SnapState(Data.MountainState);
				overworld.Mountain.SnapCamera(Area.ID, Data.MountainZoom);
				overworld.Mountain.EaseCamera(Area.ID, Data.MountainSelect, 1f);
				OverworldStartMode = start;
				return true;
			}
			Position = ClosePosition;
			return false;
		}

		public override IEnumerator Enter(Oui from)
		{
			Visible = true;
			Area.Mode = AreaMode.Normal;
			Reset();
			base.Overworld.Mountain.EaseCamera(Area.ID, Data.MountainSelect);
			for (float p = 0f; p < 1f; p += Engine.DeltaTime * 4f)
			{
				yield return null;
				Position = ClosePosition + (OpenPosition - ClosePosition) * Ease.CubeOut(p);
			}
			Position = OpenPosition;
		}

		private void Reset()
		{
			Area = SaveData.Instance.LastArea;
			Data = AreaData.Areas[Area.ID];
			RealStats = SaveData.Instance.Areas[Area.ID];
			if (SaveData.Instance.CurrentSession != null && SaveData.Instance.CurrentSession.OldStats != null && SaveData.Instance.CurrentSession.Area.ID == Area.ID)
			{
				DisplayedStats = SaveData.Instance.CurrentSession.OldStats;
				SaveData.Instance.CurrentSession = null;
			}
			else
			{
				DisplayedStats = RealStats;
			}
			height = GetModeHeight();
			modes.Clear();
			bool hasRemixMode = false;
			if (!Data.Interlude && Data.HasMode(AreaMode.BSide) && (DisplayedStats.Cassette || ((SaveData.Instance.DebugMode || SaveData.Instance.CheatMode) && DisplayedStats.Cassette == RealStats.Cassette)))
			{
				hasRemixMode = true;
			}
			bool num = !Data.Interlude && Data.HasMode(AreaMode.CSide) && SaveData.Instance.UnlockedModes >= 3 && Celeste.PlayMode != Celeste.PlayModes.Event;
			modes.Add(new Option
			{
				Label = Dialog.Clean(Data.Interlude ? "FILE_BEGIN" : "overworld_normal").ToUpper(),
				Icon = GFX.Gui["menu/play"],
				ID = "A"
			});
			if (hasRemixMode)
			{
				AddRemixButton();
			}
			if (num)
			{
				modes.Add(new Option
				{
					Label = Dialog.Clean("overworld_remix2"),
					Icon = GFX.Gui["menu/rmx2"],
					ID = "C"
				});
			}
			selectingMode = true;
			UpdateStats(wiggle: false);
			SetStatsPosition(approach: false);
			for (int i = 0; i < options.Count; i++)
			{
				options[i].SlideTowards(i, options.Count, snap: true);
			}
			chapter = Dialog.Get("area_chapter").Replace("{x}", Area.ChapterIndex.ToString().PadLeft(2));
			contentOffset = new Vector2(440f, 120f);
			initialized = true;
		}

		private int GetModeHeight()
		{
			AreaModeStats mode = RealStats.Modes[(int)Area.Mode];
			bool nostats = mode.Strawberries.Count <= 0;
			if (!Data.Interlude && ((mode.Deaths > 0 && Area.Mode != 0) || mode.Completed || mode.HeartGem))
			{
				nostats = false;
			}
			if (!nostats)
			{
				return 540;
			}
			return 300;
		}

		private Option AddRemixButton()
		{
			Option o = new Option
			{
				Label = Dialog.Clean("overworld_remix"),
				Icon = GFX.Gui["menu/remix"],
				ID = "B"
			};
			modes.Insert(1, o);
			return o;
		}

		public override IEnumerator Leave(Oui next)
		{
			base.Overworld.Mountain.EaseCamera(Area.ID, Data.MountainIdle);
			Add(new Coroutine(EaseOut()));
			yield break;
		}

		public IEnumerator EaseOut(bool removeChildren = true)
		{
			for (float p = 0f; p < 1f; p += Engine.DeltaTime * 4f)
			{
				Position = OpenPosition + (ClosePosition - OpenPosition) * Ease.CubeIn(p);
				yield return null;
			}
			if (!base.Selected)
			{
				Visible = false;
			}
		}

		public void Start(string checkpoint = null)
		{
			Focused = false;
			Audio.Play("event:/ui/world_map/chapter/checkpoint_start");
			Add(new Coroutine(StartRoutine(checkpoint)));
		}

		private IEnumerator StartRoutine(string checkpoint = null)
		{
			EnteringChapter = true;
			base.Overworld.Maddy.Hide(down: false);
			base.Overworld.Mountain.EaseCamera(Area.ID, Data.MountainZoom, 1f);
			Add(new Coroutine(EaseOut(removeChildren: false)));
			yield return 0.2f;
			ScreenWipe.WipeColor = Color.Black;
			AreaData.Get(Area).Wipe(base.Overworld, arg2: false, null);
			Audio.SetMusic(null);
			Audio.SetAmbience(null);
			if ((Area.ID == 0 || Area.ID == 9) && checkpoint == null && Area.Mode == AreaMode.Normal)
			{
				base.Overworld.RendererList.UpdateLists();
				base.Overworld.RendererList.MoveToFront(base.Overworld.Snow);
			}
			yield return 0.5f;
			LevelEnter.Go(new Session(Area, checkpoint), fromSaveData: false);
		}

		private void Swap()
		{
			Focused = false;
			base.Overworld.ShowInputUI = !selectingMode;
			Add(new Coroutine(SwapRoutine()));
		}

		private IEnumerator SwapRoutine()
		{
			float fromHeight = height;
			int toHeight = (selectingMode ? 730 : GetModeHeight());
			resizing = true;
			PlayExpandSfx(fromHeight, toHeight);
			float offset = 800f;
			for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime * 4f)
			{
				yield return null;
				contentOffset.X = 440f + offset * Ease.CubeIn(p2);
				height = MathHelper.Lerp(fromHeight, toHeight, Ease.CubeOut(p2 * 0.5f));
			}
			selectingMode = !selectingMode;
			if (!selectingMode)
			{
				HashSet<string> hashSet = SaveData.Instance.GetCheckpoints(Area);
				int siblings = hashSet.Count + 1;
				checkpoints.Clear();
				checkpoints.Add(new Option
				{
					Label = Dialog.Clean("overworld_start"),
					BgColor = Calc.HexToColor("eabe26"),
					Icon = GFX.Gui["areaselect/startpoint"],
					CheckpointLevelName = null,
					CheckpointRotation = (float)Calc.Random.Choose(-1, 1) * Calc.Random.Range(0.05f, 0.2f),
					CheckpointOffset = new Vector2(Calc.Random.Range(-16, 16), Calc.Random.Range(-16, 16)),
					Large = false,
					Siblings = siblings
				});
				foreach (string level in hashSet)
				{
					checkpoints.Add(new Option
					{
						Label = AreaData.GetCheckpointName(Area, level),
						Icon = GFX.Gui["areaselect/checkpoint"],
						CheckpointLevelName = level,
						CheckpointRotation = (float)Calc.Random.Choose(-1, 1) * Calc.Random.Range(0.05f, 0.2f),
						CheckpointOffset = new Vector2(Calc.Random.Range(-16, 16), Calc.Random.Range(-16, 16)),
						Large = false,
						Siblings = siblings
					});
				}
				if (!RealStats.Modes[(int)Area.Mode].Completed && !SaveData.Instance.DebugMode && !SaveData.Instance.CheatMode)
				{
					option = checkpoints.Count - 1;
					for (int j = 0; j < checkpoints.Count - 1; j++)
					{
						options[j].CheckpointSlideOut = 1f;
					}
				}
				else
				{
					option = 0;
				}
				for (int i = 0; i < options.Count; i++)
				{
					options[i].SlideTowards(i, options.Count, snap: true);
				}
			}
			options[option].Pop = 1f;
			for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime * 4f)
			{
				yield return null;
				height = MathHelper.Lerp(fromHeight, toHeight, Ease.CubeOut(Math.Min(1f, 0.5f + p2 * 0.5f)));
				contentOffset.X = 440f + offset * (1f - Ease.CubeOut(p2));
			}
			contentOffset.X = 440f;
			height = toHeight;
			Focused = true;
			resizing = false;
		}

		public override void Update()
		{
			if (!initialized)
			{
				return;
			}
			base.Update();
			for (int i = 0; i < options.Count; i++)
			{
				Option o = options[i];
				o.Pop = Calc.Approach(o.Pop, (option == i) ? 1f : 0f, Engine.DeltaTime * 4f);
				o.Appear = Calc.Approach(o.Appear, 1f, Engine.DeltaTime * 3f);
				o.CheckpointSlideOut = Calc.Approach(o.CheckpointSlideOut, (option > i) ? 1 : 0, Engine.DeltaTime * 4f);
				o.Faded = Calc.Approach(o.Faded, (option != i && !o.Appeared) ? 1 : 0, Engine.DeltaTime * 4f);
				o.SlideTowards(i, options.Count, snap: false);
			}
			if (selectingMode && !resizing)
			{
				height = Calc.Approach(height, GetModeHeight(), Engine.DeltaTime * 1600f);
			}
			if (base.Selected && Focused)
			{
				if (Input.MenuLeft.Pressed && option > 0)
				{
					Audio.Play("event:/ui/world_map/chapter/tab_roll_left");
					option--;
					wiggler.Start();
					if (selectingMode)
					{
						UpdateStats();
						PlayExpandSfx(height, GetModeHeight());
					}
					else
					{
						Audio.Play("event:/ui/world_map/chapter/checkpoint_photo_add");
					}
				}
				else if (Input.MenuRight.Pressed && option + 1 < options.Count)
				{
					Audio.Play("event:/ui/world_map/chapter/tab_roll_right");
					option++;
					wiggler.Start();
					if (selectingMode)
					{
						UpdateStats();
						PlayExpandSfx(height, GetModeHeight());
					}
					else
					{
						Audio.Play("event:/ui/world_map/chapter/checkpoint_photo_remove");
					}
				}
				else if (Input.MenuConfirm.Pressed)
				{
					if (selectingMode)
					{
						if (!SaveData.Instance.FoundAnyCheckpoints(Area))
						{
							Start();
						}
						else
						{
							Audio.Play("event:/ui/world_map/chapter/level_select");
							Swap();
						}
					}
					else
					{
						Start(options[option].CheckpointLevelName);
					}
				}
				else if (Input.MenuCancel.Pressed)
				{
					if (selectingMode)
					{
						Audio.Play("event:/ui/world_map/chapter/back");
						base.Overworld.Goto<OuiChapterSelect>();
					}
					else
					{
						Audio.Play("event:/ui/world_map/chapter/checkpoint_back");
						Swap();
					}
				}
			}
			SetStatsPosition(approach: true);
		}

		public override void Render()
		{
			if (!initialized)
			{
				return;
			}
			Vector2 optionPosition = OptionsRenderPosition;
			for (int k = 0; k < options.Count; k++)
			{
				if (!options[k].OnTopOfUI)
				{
					options[k].Render(optionPosition, option == k, wiggler, modeAppearWiggler);
				}
			}
			bool golden = false;
			if (RealStats.Modes[(int)Area.Mode].Completed)
			{
				int mode = (int)Area.Mode;
				foreach (EntityData strawb in AreaData.Areas[Area.ID].Mode[mode].MapData.Goldenberries)
				{
					EntityID entityID = new EntityID(strawb.Level.Name, strawb.ID);
					if (RealStats.Modes[mode].Strawberries.Contains(entityID))
					{
						golden = true;
						break;
					}
				}
			}
			MTexture top = GFX.Gui[(!golden) ? "areaselect/cardtop" : "areaselect/cardtop_golden"];
			top.Draw(Position + new Vector2(0f, -32f));
			MTexture full = GFX.Gui[(!golden) ? "areaselect/card" : "areaselect/card_golden"];
			card = full.GetSubtexture(0, full.Height - (int)height, full.Width, (int)height, card);
			card.Draw(Position + new Vector2(0f, -32 + top.Height));
			for (int j = 0; j < options.Count; j++)
			{
				if (options[j].OnTopOfUI)
				{
					options[j].Render(optionPosition, option == j, wiggler, modeAppearWiggler);
				}
			}
			ActiveFont.Draw(options[option].Label, optionPosition + new Vector2(0f, -140f), Vector2.One * 0.5f, Vector2.One * (1f + wiggler.Value * 0.1f), Color.Black * 0.8f);
			if (selectingMode)
			{
				strawberries.Position = contentOffset + new Vector2(0f, 170f) + strawberriesOffset;
				deaths.Position = contentOffset + new Vector2(0f, 170f) + deathsOffset;
				heart.Position = contentOffset + new Vector2(0f, 170f) + heartOffset;
				base.Render();
			}
			if (!selectingMode)
			{
				Vector2 checkpointPos = Position + new Vector2(contentOffset.X, 340f);
				for (int i = options.Count - 1; i >= 0; i--)
				{
					DrawCheckpoint(checkpointPos, options[i], i);
				}
			}
			GFX.Gui["areaselect/title"].Draw(Position + new Vector2(-60f, 0f), Vector2.Zero, Data.TitleBaseColor);
			GFX.Gui["areaselect/accent"].Draw(Position + new Vector2(-60f, 0f), Vector2.Zero, Data.TitleAccentColor);
			string name = Dialog.Clean(AreaData.Get(Area).Name);
			if (Data.Interlude)
			{
				ActiveFont.Draw(name, Position + IconOffset + new Vector2(-100f, 0f), new Vector2(1f, 0.5f), Vector2.One * 1f, Data.TitleTextColor * 0.8f);
			}
			else
			{
				ActiveFont.Draw(chapter, Position + IconOffset + new Vector2(-100f, -2f), new Vector2(1f, 1f), Vector2.One * 0.6f, Data.TitleAccentColor * 0.8f);
				ActiveFont.Draw(name, Position + IconOffset + new Vector2(-100f, -18f), new Vector2(1f, 0f), Vector2.One * 1f, Data.TitleTextColor * 0.8f);
			}
			if (spotlightAlpha > 0f)
			{
				HiresRenderer.EndRender();
				SpotlightWipe.DrawSpotlight(spotlightPosition, spotlightRadius, Color.Black * spotlightAlpha);
				HiresRenderer.BeginRender();
			}
		}

		private void DrawCheckpoint(Vector2 center, Option option, int checkpointIndex)
		{
			MTexture preview = GetCheckpointPreview(Area, option.CheckpointLevelName);
			MTexture mTexture = MTN.Checkpoints["polaroid"];
			float rot = option.CheckpointRotation;
			Vector2 pos = center + option.CheckpointOffset;
			pos += Vector2.UnitX * 800f * Ease.CubeIn(option.CheckpointSlideOut);
			mTexture.DrawCentered(pos, Color.White, 0.75f, rot);
			MTexture tex = GFX.Gui["collectables/strawberry"];
			if (preview != null)
			{
				Vector2 scale = Vector2.One * 0.75f;
				if (SaveData.Instance.Assists.MirrorMode)
				{
					scale.X = 0f - scale.X;
				}
				scale *= 720f / (float)preview.Width;
				HiresRenderer.EndRender();
				HiresRenderer.BeginRender(BlendState.AlphaBlend, SamplerState.PointClamp);
				preview.DrawCentered(pos, Color.White, scale, rot);
				HiresRenderer.EndRender();
				HiresRenderer.BeginRender();
			}
			int mode = (int)Area.Mode;
			if (!RealStats.Modes[mode].Completed && !SaveData.Instance.CheatMode && !SaveData.Instance.DebugMode)
			{
				return;
			}
			Vector2 bottomRight = new Vector2(300f, 220f);
			bottomRight = pos + bottomRight.Rotate(rot);
			int total = 0;
			total = ((checkpointIndex != 0) ? Data.Mode[mode].Checkpoints[checkpointIndex - 1].Strawberries : Data.Mode[mode].StartStrawberries);
			bool[] has = new bool[total];
			foreach (EntityID berry in RealStats.Modes[mode].Strawberries)
			{
				for (int j = 0; j < total; j++)
				{
					EntityData at = Data.Mode[mode].StrawberriesByCheckpoint[checkpointIndex, j];
					if (at != null && at.Level.Name == berry.Level && at.ID == berry.ID)
					{
						has[j] = true;
					}
				}
			}
			Vector2 normal = Calc.AngleToVector(rot, 1f);
			Vector2 offset = bottomRight - normal * total * 44f;
			if (Area.Mode == AreaMode.Normal && Data.CassetteCheckpointIndex == checkpointIndex)
			{
				Vector2 cassettePos = offset - normal * 60f;
				if (RealStats.Cassette)
				{
					MTN.Journal["cassette"].DrawCentered(cassettePos, Color.White, 1f, rot);
				}
				else
				{
					MTN.Journal["cassette_outline"].DrawCentered(cassettePos, Color.DarkGray, 1f, rot);
				}
			}
			for (int i = 0; i < total; i++)
			{
				tex.DrawCentered(offset, has[i] ? Color.White : (Color.Black * 0.3f), 0.5f, rot);
				offset += normal * 44f;
			}
		}

		private void UpdateStats(bool wiggle = true, bool? overrideStrawberryWiggle = null, bool? overrideDeathWiggle = null, bool? overrideHeartWiggle = null)
		{
			AreaModeStats stats = DisplayedStats.Modes[(int)Area.Mode];
			AreaData info = AreaData.Get(Area);
			deaths.Visible = stats.Deaths > 0 && (Area.Mode != 0 || RealStats.Modes[(int)Area.Mode].Completed) && !AreaData.Get(Area).Interlude;
			deaths.Amount = stats.Deaths;
			deaths.SetMode(info.IsFinal ? AreaMode.CSide : Area.Mode);
			heart.Visible = stats.HeartGem && !info.Interlude && info.CanFullClear;
			heart.SetCurrentMode(Area.Mode, stats.HeartGem);
			strawberries.Visible = (stats.TotalStrawberries > 0 || (stats.Completed && Area.Mode == AreaMode.Normal && AreaData.Get(Area).Mode[0].TotalStrawberries > 0)) && !AreaData.Get(Area).Interlude;
			strawberries.Amount = stats.TotalStrawberries;
			strawberries.OutOf = Data.Mode[0].TotalStrawberries;
			strawberries.ShowOutOf = stats.Completed && Area.Mode == AreaMode.Normal;
			strawberries.Golden = Area.Mode != AreaMode.Normal;
			if (wiggle)
			{
				if (strawberries.Visible && (!overrideStrawberryWiggle.HasValue || overrideStrawberryWiggle.Value))
				{
					strawberries.Wiggle();
				}
				if (heart.Visible && (!overrideHeartWiggle.HasValue || overrideHeartWiggle.Value))
				{
					heart.Wiggle();
				}
				if (deaths.Visible && (!overrideDeathWiggle.HasValue || overrideDeathWiggle.Value))
				{
					deaths.Wiggle();
				}
			}
		}

		private void SetStatsPosition(bool approach)
		{
			if (heart.Visible && (strawberries.Visible || deaths.Visible))
			{
				heartOffset = Approach(heartOffset, new Vector2(-120f, 0f), !approach);
				strawberriesOffset = Approach(strawberriesOffset, new Vector2(120f, deaths.Visible ? (-40) : 0), !approach);
				deathsOffset = Approach(deathsOffset, new Vector2(120f, strawberries.Visible ? 40 : 0), !approach);
			}
			else if (heart.Visible)
			{
				heartOffset = Approach(heartOffset, Vector2.Zero, !approach);
			}
			else
			{
				strawberriesOffset = Approach(strawberriesOffset, new Vector2(0f, deaths.Visible ? (-40) : 0), !approach);
				deathsOffset = Approach(deathsOffset, new Vector2(0f, strawberries.Visible ? 40 : 0), !approach);
			}
		}

		private Vector2 Approach(Vector2 from, Vector2 to, bool snap)
		{
			if (snap)
			{
				return to;
			}
			return from += (to - from) * (1f - (float)Math.Pow(0.0010000000474974513, Engine.DeltaTime));
		}

		private IEnumerator IncrementStatsDisplay(AreaModeStats modeStats, AreaModeStats newModeStats, bool doHeartGem, bool doStrawberries, bool doDeaths, bool doRemixUnlock)
		{
			if (doHeartGem)
			{
				Audio.Play("event:/ui/postgame/crystal_heart");
				heart.Visible = true;
				heart.SetCurrentMode(Area.Mode, has: true);
				heart.Appear(Area.Mode);
				yield return 1.8f;
			}
			if (doStrawberries)
			{
				strawberries.CanWiggle = true;
				strawberries.Visible = true;
				while (newModeStats.TotalStrawberries > modeStats.TotalStrawberries)
				{
					int diff = newModeStats.TotalStrawberries - modeStats.TotalStrawberries;
					if (diff < 3)
					{
						yield return 0.3f;
					}
					else if (diff < 8)
					{
						yield return 0.2f;
					}
					else
					{
						yield return 0.1f;
						modeStats.TotalStrawberries++;
					}
					modeStats.TotalStrawberries++;
					strawberries.Amount = modeStats.TotalStrawberries;
					Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
				}
				strawberries.CanWiggle = false;
				yield return 0.5f;
				if (newModeStats.Completed && !modeStats.Completed && Area.Mode == AreaMode.Normal)
				{
					yield return 0.25f;
					Audio.Play((strawberries.Amount >= Data.Mode[0].TotalStrawberries) ? "event:/ui/postgame/strawberry_total_all" : "event:/ui/postgame/strawberry_total");
					strawberries.OutOf = Data.Mode[0].TotalStrawberries;
					strawberries.ShowOutOf = true;
					strawberries.Wiggle();
					modeStats.Completed = true;
					Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
					yield return 0.6f;
				}
			}
			if (doDeaths)
			{
				Audio.Play("event:/ui/postgame/death_appear");
				deaths.CanWiggle = true;
				deaths.Visible = true;
				while (newModeStats.Deaths > modeStats.Deaths)
				{
					int add;
					yield return HandleDeathTick(modeStats.Deaths, newModeStats.Deaths, out add);
					modeStats.Deaths += add;
					deaths.Amount = modeStats.Deaths;
					if (modeStats.Deaths >= newModeStats.Deaths)
					{
						Audio.Play("event:/ui/postgame/death_final");
					}
					else
					{
						Audio.Play("event:/ui/postgame/death_count");
					}
					Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
				}
				yield return 0.8f;
				deaths.CanWiggle = false;
			}
			if (doRemixUnlock)
			{
				bSideUnlockSfx = Audio.Play("event:/ui/postgame/unlock_bside");
				Option o = AddRemixButton();
				o.Appear = 0f;
				o.IconEase = 0f;
				o.Appeared = true;
				yield return 0.5f;
				spotlightPosition = o.GetRenderPosition(OptionsRenderPosition);
				for (float t2 = 0f; t2 < 1f; t2 += Engine.DeltaTime / 0.5f)
				{
					spotlightAlpha = t2 * 0.9f;
					spotlightRadius = 128f * Ease.CubeOut(t2);
					yield return null;
				}
				yield return 0.3f;
				while ((o.IconEase += Engine.DeltaTime * 2f) < 1f)
				{
					yield return null;
				}
				o.IconEase = 1f;
				modeAppearWiggler.Start();
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
				remixUnlockText = new AreaCompleteTitle(spotlightPosition + new Vector2(0f, 80f), Dialog.Clean("OVERWORLD_REMIX_UNLOCKED"), 1f);
				remixUnlockText.Tag = Tags.HUD;
				base.Overworld.Add(remixUnlockText);
				yield return 1.5f;
				for (float t2 = 0f; t2 < 1f; t2 += Engine.DeltaTime / 0.5f)
				{
					spotlightAlpha = (1f - t2) * 0.5f;
					spotlightRadius = 128f + 128f * Ease.CubeOut(t2);
					remixUnlockText.Alpha = 1f - Ease.CubeOut(t2);
					yield return null;
				}
				remixUnlockText.RemoveSelf();
				remixUnlockText = null;
				o.Appeared = false;
			}
		}

		public IEnumerator IncrementStats(bool shouldAdvance)
		{
			Focused = false;
			base.Overworld.ShowInputUI = false;
			if (Data.Interlude)
			{
				if (shouldAdvance && OverworldStartMode == Overworld.StartMode.AreaComplete)
				{
					yield return 1.2f;
					base.Overworld.Goto<OuiChapterSelect>().AdvanceToNext();
				}
				else
				{
					Focused = true;
				}
				yield return null;
				yield break;
			}
			AreaData data = Data;
			AreaStats stats = DisplayedStats;
			AreaStats newStats = SaveData.Instance.Areas[data.ID];
			AreaModeStats modeStats = stats.Modes[(int)Area.Mode];
			AreaModeStats newModeStats = newStats.Modes[(int)Area.Mode];
			bool doStrawberries = newModeStats.TotalStrawberries > modeStats.TotalStrawberries;
			bool doHeartGem = newModeStats.HeartGem && !modeStats.HeartGem;
			bool doDeaths = newModeStats.Deaths > modeStats.Deaths && (Area.Mode != 0 || newModeStats.Completed);
			bool doRemixUnlock = Area.Mode == AreaMode.Normal && data.HasMode(AreaMode.BSide) && newStats.Cassette && !stats.Cassette;
			if (doStrawberries || doHeartGem || doDeaths || doRemixUnlock)
			{
				yield return 0.8f;
			}
			bool skipped = false;
			Coroutine routine = new Coroutine(IncrementStatsDisplay(modeStats, newModeStats, doHeartGem, doStrawberries, doDeaths, doRemixUnlock));
			Add(routine);
			yield return null;
			while (!routine.Finished)
			{
				if (MInput.GamePads[0].Pressed(Buttons.Start) || MInput.Keyboard.Pressed(Keys.Enter))
				{
					routine.Active = false;
					routine.RemoveSelf();
					skipped = true;
					Audio.Stop(bSideUnlockSfx);
					Audio.Play("event:/new_content/ui/skip_all");
					break;
				}
				yield return null;
			}
			if (skipped && doRemixUnlock)
			{
				spotlightAlpha = 0f;
				spotlightRadius = 0f;
				if (remixUnlockText != null)
				{
					remixUnlockText.RemoveSelf();
					remixUnlockText = null;
				}
				if (modes.Count <= 1 || modes[1].ID != "B")
				{
					AddRemixButton();
				}
				else
				{
					Option obj = modes[1];
					obj.IconEase = 1f;
					obj.Appear = 1f;
					obj.Appeared = false;
				}
			}
			DisplayedStats = RealStats;
			if (skipped)
			{
				doStrawberries = doStrawberries && modeStats.TotalStrawberries != newModeStats.TotalStrawberries;
				doDeaths &= modeStats.Deaths != newModeStats.Deaths;
				doHeartGem = doHeartGem && !heart.Visible;
				UpdateStats(wiggle: true, doStrawberries, doDeaths, doHeartGem);
			}
			yield return null;
			if (shouldAdvance && OverworldStartMode == Overworld.StartMode.AreaComplete)
			{
				if ((!doDeaths && !doStrawberries && !doHeartGem) || Settings.Instance.SpeedrunClock != 0)
				{
					yield return 1.2f;
				}
				base.Overworld.Goto<OuiChapterSelect>().AdvanceToNext();
			}
			else
			{
				Focused = true;
				base.Overworld.ShowInputUI = true;
			}
		}

		private float HandleDeathTick(int oldDeaths, int newDeaths, out int add)
		{
			int diff = newDeaths - oldDeaths;
			if (diff < 3)
			{
				add = 1;
				return 0.3f;
			}
			if (diff < 8)
			{
				add = 2;
				return 0.2f;
			}
			if (diff < 30)
			{
				add = 5;
				return 0.1f;
			}
			if (diff < 100)
			{
				add = 10;
				return 0.1f;
			}
			if (diff < 1000)
			{
				add = 25;
				return 0.1f;
			}
			add = 100;
			return 0.1f;
		}

		private void PlayExpandSfx(float currentHeight, float nextHeight)
		{
			if (nextHeight > currentHeight)
			{
				Audio.Play("event:/ui/world_map/chapter/pane_expand");
			}
			else if (nextHeight < currentHeight)
			{
				Audio.Play("event:/ui/world_map/chapter/pane_contract");
			}
		}

		public static string GetCheckpointPreviewName(AreaKey area, string level)
		{
			if (level == null)
			{
				return area.ToString();
			}
			return area.ToString() + "_" + level;
		}

		private MTexture GetCheckpointPreview(AreaKey area, string level)
		{
			string name = GetCheckpointPreviewName(area, level);
			if (MTN.Checkpoints.Has(name))
			{
				return MTN.Checkpoints[name];
			}
			return null;
		}
	}
}
