using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class UnlockedPico8Message : Entity
	{
		private float alpha;

		private string text;

		private bool waitForKeyPress;

		private float timer;

		private Action callback;

		public UnlockedPico8Message(Action callback = null)
		{
			this.callback = callback;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			base.Tag = (int)Tags.HUD | (int)Tags.PauseUpdate;
			text = ActiveFont.FontSize.AutoNewline(Dialog.Clean("PICO8_UNLOCKED"), 900);
			base.Depth = -10000;
			Add(new Coroutine(Routine()));
		}

		private IEnumerator Routine()
		{
			Level level = base.Scene as Level;
			level.PauseLock = true;
			level.Paused = true;
			while ((alpha += Engine.DeltaTime / 0.5f) < 1f)
			{
				yield return null;
			}
			alpha = 1f;
			waitForKeyPress = true;
			while (!Input.MenuConfirm.Pressed)
			{
				yield return null;
			}
			waitForKeyPress = false;
			while ((alpha -= Engine.DeltaTime / 0.5f) > 0f)
			{
				yield return null;
			}
			alpha = 0f;
			level.PauseLock = false;
			level.Paused = false;
			RemoveSelf();
			if (callback != null)
			{
				callback();
			}
		}

		public override void Update()
		{
			timer += Engine.DeltaTime;
			base.Update();
		}

		public override void Render()
		{
			float e = Ease.CubeOut(alpha);
			Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * e * 0.8f);
			GFX.Gui["pico8"].DrawJustified(Celeste.TargetCenter + new Vector2(0f, -64f * (1f - e) - 16f), new Vector2(0.5f, 1f), Color.White * e);
			Vector2 pos = Celeste.TargetCenter + new Vector2(0f, 64f * (1f - e) + 16f);
			Vector2 fontsize = ActiveFont.Measure(text);
			ActiveFont.Draw(text, pos, new Vector2(0.5f, 0f), Vector2.One, Color.White * e);
			if (waitForKeyPress)
			{
				GFX.Gui["textboxbutton"].DrawCentered(Celeste.TargetCenter + new Vector2(fontsize.X / 2f + 32f, fontsize.Y + 48f + (float)((timer % 1f < 0.25f) ? 6 : 0)));
			}
		}
	}
}
