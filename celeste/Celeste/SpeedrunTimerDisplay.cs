using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class SpeedrunTimerDisplay : Entity
	{
		public float CompleteTimer;

		public const int GuiChapterHeight = 58;

		public const int GuiFileHeight = 78;

		private static float numberWidth;

		private static float spacerWidth;

		private MTexture bg = GFX.Gui["strawberryCountBG"];

		public float DrawLerp;

		private Wiggler wiggler;

		public SpeedrunTimerDisplay()
		{
			base.Tag = (int)Tags.HUD | (int)Tags.Global | (int)Tags.PauseUpdate | (int)Tags.TransitionUpdate;
			base.Depth = -100;
			base.Y = 60f;
			CalculateBaseSizes();
			Add(wiggler = Wiggler.Create(0.5f, 4f));
		}

		public static void CalculateBaseSizes()
		{
			PixelFont font = Dialog.Languages["english"].Font;
			float fontFaceSize = Dialog.Languages["english"].FontFaceSize;
			PixelFontSize fontSize = font.Get(fontFaceSize);
			for (int i = 0; i < 10; i++)
			{
				float w = fontSize.Measure(i.ToString()).X;
				if (w > numberWidth)
				{
					numberWidth = w;
				}
			}
			spacerWidth = fontSize.Measure('.').X;
		}

		public override void Update()
		{
			Level level = base.Scene as Level;
			if (level.Completed)
			{
				if (CompleteTimer == 0f)
				{
					wiggler.Start();
				}
				CompleteTimer += Engine.DeltaTime;
			}
			bool display = false;
			if (level.Session.Area.ID != 8 && !level.TimerHidden)
			{
				if (Settings.Instance.SpeedrunClock == SpeedrunType.Chapter)
				{
					if (CompleteTimer < 3f)
					{
						display = true;
					}
				}
				else if (Settings.Instance.SpeedrunClock == SpeedrunType.File)
				{
					display = true;
				}
			}
			DrawLerp = Calc.Approach(DrawLerp, display ? 1 : 0, Engine.DeltaTime * 4f);
			base.Update();
		}

		public override void Render()
		{
			if (!(DrawLerp <= 0f))
			{
				float x = -300f * Ease.CubeIn(1f - DrawLerp);
				Level level = base.Scene as Level;
				Session session = level.Session;
				if (Settings.Instance.SpeedrunClock == SpeedrunType.Chapter)
				{
					string chapterTime2 = TimeSpan.FromTicks(session.Time).ShortGameplayFormat();
					bg.Draw(new Vector2(x, base.Y));
					DrawTime(new Vector2(x + 32f, base.Y + 44f), chapterTime2, 1f + wiggler.Value * 0.15f, session.StartedFromBeginning, level.Completed, session.BeatBestTime);
				}
				else if (Settings.Instance.SpeedrunClock == SpeedrunType.File)
				{
					TimeSpan chapterTimeSpan = TimeSpan.FromTicks(session.Time);
					string chapterTime = "";
					chapterTime = ((!(chapterTimeSpan.TotalHours >= 1.0)) ? chapterTimeSpan.ToString("mm\\:ss") : ((int)chapterTimeSpan.TotalHours + ":" + chapterTimeSpan.ToString("mm\\:ss")));
					TimeSpan time = TimeSpan.FromTicks(SaveData.Instance.Time);
					int hours = (int)time.TotalHours;
					string gameTime = hours + time.ToString("\\:mm\\:ss\\.fff");
					int w = ((hours < 10) ? 64 : ((hours < 100) ? 96 : 128));
					Draw.Rect(x, base.Y, w + 2, 38f, Color.Black);
					bg.Draw(new Vector2(x + (float)w, base.Y));
					DrawTime(new Vector2(x + 32f, base.Y + 44f), gameTime);
					bg.Draw(new Vector2(x, base.Y + 38f), Vector2.Zero, Color.White, 0.6f);
					DrawTime(new Vector2(x + 32f, base.Y + 40f + 26.400002f), chapterTime, (1f + wiggler.Value * 0.15f) * 0.6f, session.StartedFromBeginning, level.Completed, session.BeatBestTime, 0.6f);
				}
			}
		}

		public static void DrawTime(Vector2 position, string timeString, float scale = 1f, bool valid = true, bool finished = false, bool bestTime = false, float alpha = 1f)
		{
			PixelFont font = Dialog.Languages["english"].Font;
			float fontSize = Dialog.Languages["english"].FontFaceSize;
			float s = scale;
			float x = position.X;
			float y = position.Y;
			Color baseColor = Color.White * alpha;
			Color smallColor = Color.LightGray * alpha;
			if (!valid)
			{
				baseColor = Calc.HexToColor("918988") * alpha;
				smallColor = Calc.HexToColor("7a6f6d") * alpha;
			}
			else if (bestTime)
			{
				baseColor = Calc.HexToColor("fad768") * alpha;
				smallColor = Calc.HexToColor("cfa727") * alpha;
			}
			else if (finished)
			{
				baseColor = Calc.HexToColor("6ded87") * alpha;
				smallColor = Calc.HexToColor("43d14c") * alpha;
			}
			for (int i = 0; i < timeString.Length; i++)
			{
				char c = timeString[i];
				if (c == '.')
				{
					s = scale * 0.7f;
					y -= 5f * scale;
				}
				Color color = ((c == ':' || c == '.' || s < scale) ? smallColor : baseColor);
				float advance = (((c == ':' || c == '.') ? spacerWidth : numberWidth) + 4f) * s;
				font.DrawOutline(fontSize, c.ToString(), new Vector2(x + advance / 2f, y), new Vector2(0.5f, 1f), Vector2.One * s, color, 2f, Color.Black);
				x += advance;
			}
		}

		public static float GetTimeWidth(string timeString, float scale = 1f)
		{
			float s = scale;
			float x = 0f;
			foreach (char c in timeString)
			{
				if (c == '.')
				{
					s = scale * 0.7f;
				}
				float advance = (((c == ':' || c == '.') ? spacerWidth : numberWidth) + 4f) * s;
				x += advance;
			}
			return x;
		}
	}
}
