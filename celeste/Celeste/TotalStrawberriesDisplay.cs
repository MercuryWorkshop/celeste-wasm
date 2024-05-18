using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class TotalStrawberriesDisplay : Entity
	{
		private const float NumberUpdateDelay = 0.4f;

		private const float ComboUpdateDelay = 0.3f;

		private const float AfterUpdateDelay = 2f;

		private const float LerpInSpeed = 1.2f;

		private const float LerpOutSpeed = 2f;

		public static readonly Color FlashColor = Calc.HexToColor("FF5E76");

		private MTexture bg;

		public float DrawLerp;

		private float strawberriesUpdateTimer;

		private float strawberriesWaitTimer;

		private StrawberriesCounter strawberries;

		public TotalStrawberriesDisplay()
		{
			base.Y = 96f;
			base.Depth = -101;
			base.Tag = (int)Tags.HUD | (int)Tags.Global | (int)Tags.PauseUpdate | (int)Tags.TransitionUpdate;
			bg = GFX.Gui["strawberryCountBG"];
			Add(strawberries = new StrawberriesCounter(centeredX: false, SaveData.Instance.TotalStrawberries));
		}

		public override void Update()
		{
			base.Update();
			Level level = base.Scene as Level;
			if (SaveData.Instance.TotalStrawberries > strawberries.Amount && strawberriesUpdateTimer <= 0f)
			{
				strawberriesUpdateTimer = 0.4f;
			}
			if (SaveData.Instance.TotalStrawberries > strawberries.Amount || strawberriesUpdateTimer > 0f || strawberriesWaitTimer > 0f || (level.Paused && level.PauseMainMenuOpen))
			{
				DrawLerp = Calc.Approach(DrawLerp, 1f, 1.2f * Engine.RawDeltaTime);
			}
			else
			{
				DrawLerp = Calc.Approach(DrawLerp, 0f, 2f * Engine.RawDeltaTime);
			}
			if (strawberriesWaitTimer > 0f)
			{
				strawberriesWaitTimer -= Engine.RawDeltaTime;
			}
			if (strawberriesUpdateTimer > 0f && DrawLerp == 1f)
			{
				strawberriesUpdateTimer -= Engine.RawDeltaTime;
				if (strawberriesUpdateTimer <= 0f)
				{
					if (strawberries.Amount < SaveData.Instance.TotalStrawberries)
					{
						strawberries.Amount++;
					}
					strawberriesWaitTimer = 2f;
					if (strawberries.Amount < SaveData.Instance.TotalStrawberries)
					{
						strawberriesUpdateTimer = 0.3f;
					}
				}
			}
			if (Visible)
			{
				float y = 96f;
				if (!level.TimerHidden)
				{
					if (Settings.Instance.SpeedrunClock == SpeedrunType.Chapter)
					{
						y += 58f;
					}
					else if (Settings.Instance.SpeedrunClock == SpeedrunType.File)
					{
						y += 78f;
					}
				}
				base.Y = Calc.Approach(base.Y, y, Engine.DeltaTime * 800f);
			}
			Visible = DrawLerp > 0f;
		}

		public override void Render()
		{
			Vector2 at = Vector2.Lerp(new Vector2(-bg.Width, base.Y), new Vector2(32f, base.Y), Ease.CubeOut(DrawLerp));
			at = at.Round();
			bg.DrawJustified(at + new Vector2(-96f, 12f), new Vector2(0f, 0.5f));
			strawberries.Position = at + new Vector2(0f, 0f - base.Y);
			strawberries.Render();
		}
	}
}
