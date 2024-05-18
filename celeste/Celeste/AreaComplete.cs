using System;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste
{
	public class AreaComplete : Scene
	{
		public Session Session;

		private bool finishedSlide;

		private bool canConfirm = true;

		private HiresSnow snow;

		private float speedrunTimerDelay = 1.1f;

		private float speedrunTimerEase;

		private string speedrunTimerChapterString;

		private string speedrunTimerFileString;

		private string chapterSpeedrunText = Dialog.Get("OPTIONS_SPEEDRUN_CHAPTER") + ":";

		private AreaCompleteTitle title;

		private CompleteRenderer complete;

		private string version;

		public AreaComplete(Session session, XmlElement xml, Atlas atlas, HiresSnow snow)
		{
			Session = session;
			version = Celeste.Instance.Version.ToString();
			if (session.Area.ID != 7)
			{
				string titleText = Dialog.Clean(string.Concat("areacomplete_", session.Area.Mode, session.FullClear ? "_fullclear" : ""));
				Vector2 titleOrigin = new Vector2(960f, 200f);
				float size = Math.Min(1600f / ActiveFont.Measure(titleText).X, 3f);
				title = new AreaCompleteTitle(titleOrigin, titleText, size);
			}
			Add(complete = new CompleteRenderer(xml, atlas, 1f, delegate
			{
				finishedSlide = true;
			}));
			if (title != null)
			{
				complete.RenderUI = delegate
				{
					title.DrawLineUI();
				};
			}
			complete.RenderPostUI = RenderUI;
			speedrunTimerChapterString = TimeSpan.FromTicks(Session.Time).ShortGameplayFormat();
			speedrunTimerFileString = Dialog.FileTime(SaveData.Instance.Time);
			SpeedrunTimerDisplay.CalculateBaseSizes();
			Add(this.snow = snow);
			base.RendererList.UpdateLists();
			AreaKey area = session.Area;
			if (area.Mode == AreaMode.Normal)
			{
				if (area.ID == 1)
				{
					Achievements.Register(Achievement.CH1);
				}
				else if (area.ID == 2)
				{
					Achievements.Register(Achievement.CH2);
				}
				else if (area.ID == 3)
				{
					Achievements.Register(Achievement.CH3);
				}
				else if (area.ID == 4)
				{
					Achievements.Register(Achievement.CH4);
				}
				else if (area.ID == 5)
				{
					Achievements.Register(Achievement.CH5);
				}
				else if (area.ID == 6)
				{
					Achievements.Register(Achievement.CH6);
				}
				else if (area.ID == 7)
				{
					Achievements.Register(Achievement.CH7);
				}
			}
		}

		public override void End()
		{
			base.End();
			complete.Dispose();
		}

		public override void Update()
		{
			base.Update();
			if (Input.MenuConfirm.Pressed && finishedSlide && canConfirm)
			{
				canConfirm = false;
				if (Session.Area.ID == 7 && Session.Area.Mode == AreaMode.Normal)
				{
					new FadeWipe(this, wipeIn: false, delegate
					{
						Session.RespawnPoint = null;
						Session.FirstLevel = false;
						Session.Level = "credits-summit";
						Session.Audio.Music.Event = "event:/music/lvl8/main";
						Session.Audio.Apply();
						Engine.Scene = new LevelLoader(Session)
						{
							PlayerIntroTypeOverride = Player.IntroTypes.None,
							Level = 
							{
								new CS07_Credits()
							}
						};
					});
				}
				else
				{
					HiresSnow outSnow = new HiresSnow();
					outSnow.Alpha = 0f;
					outSnow.AttachAlphaTo = new FadeWipe(this, wipeIn: false, delegate
					{
						Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaComplete, outSnow);
					});
					Add(outSnow);
				}
			}
			snow.Alpha = Calc.Approach(snow.Alpha, 0f, Engine.DeltaTime * 0.5f);
			snow.Direction.Y = Calc.Approach(snow.Direction.Y, 1f, Engine.DeltaTime * 24f);
			speedrunTimerDelay -= Engine.DeltaTime;
			if (speedrunTimerDelay <= 0f)
			{
				speedrunTimerEase = Calc.Approach(speedrunTimerEase, 1f, Engine.DeltaTime * 2f);
			}
			if (title != null)
			{
				title.Update();
			}
			if (Celeste.PlayMode == Celeste.PlayModes.Debug)
			{
				if (MInput.Keyboard.Pressed(Keys.F2))
				{
					Celeste.ReloadAssets(levels: false, graphics: true, hires: false);
					Engine.Scene = new LevelExit(LevelExit.Mode.Completed, Session);
				}
				else if (MInput.Keyboard.Pressed(Keys.F3))
				{
					Celeste.ReloadAssets(levels: false, graphics: true, hires: true);
					Engine.Scene = new LevelExit(LevelExit.Mode.Completed, Session);
				}
			}
		}

		private void RenderUI()
		{
			base.Entities.Render();
			Info(speedrunTimerEase, speedrunTimerChapterString, speedrunTimerFileString, chapterSpeedrunText, version);
			if (complete.HasUI && title != null)
			{
				title.Render();
			}
		}

		public static void Info(float ease, string speedrunTimerChapterString, string speedrunTimerFileString, string chapterSpeedrunText, string versionText)
		{
			if (ease > 0f && Settings.Instance.SpeedrunClock != 0)
			{
				Vector2 pos = new Vector2(80f - 300f * (1f - Ease.CubeOut(ease)), 1000f);
				if (Settings.Instance.SpeedrunClock == SpeedrunType.Chapter)
				{
					SpeedrunTimerDisplay.DrawTime(pos, speedrunTimerChapterString);
				}
				else
				{
					pos.Y -= 16f;
					SpeedrunTimerDisplay.DrawTime(pos, speedrunTimerFileString);
					ActiveFont.DrawOutline(chapterSpeedrunText, pos + new Vector2(0f, 40f), new Vector2(0f, 1f), Vector2.One * 0.6f, Color.White, 2f, Color.Black);
					SpeedrunTimerDisplay.DrawTime(pos + new Vector2(ActiveFont.Measure(chapterSpeedrunText).X * 0.6f + 8f, 40f), speedrunTimerChapterString, 0.6f);
				}
				VersionNumberAndVariants(versionText, ease, 1f);
			}
		}

		public static void VersionNumberAndVariants(string version, float ease, float alpha)
		{
			Vector2 pos = new Vector2(1820f + 300f * (1f - Ease.CubeOut(ease)), 1020f);
			if (SaveData.Instance.AssistMode || SaveData.Instance.VariantMode)
			{
				MTexture mTexture = GFX.Gui[SaveData.Instance.AssistMode ? "cs_assistmode" : "cs_variantmode"];
				pos.Y -= 32f;
				mTexture.DrawJustified(pos + new Vector2(0f, -8f), new Vector2(0.5f, 1f), Color.White, 0.6f);
				ActiveFont.DrawOutline(version, pos, new Vector2(0.5f, 0f), Vector2.One * 0.5f, Color.White, 2f, Color.Black);
			}
			else
			{
				ActiveFont.DrawOutline(version, pos, new Vector2(0.5f, 0.5f), Vector2.One * 0.5f, Color.White, 2f, Color.Black);
			}
		}
	}
}
