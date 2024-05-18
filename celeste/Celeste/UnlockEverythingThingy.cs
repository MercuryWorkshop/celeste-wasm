using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class UnlockEverythingThingy : CheatListener
	{
		public UnlockEverythingThingy()
		{
			AddInput('u', () => Input.MenuUp.Pressed && !Input.MenuUp.Repeating);
			AddInput('d', () => Input.MenuDown.Pressed && !Input.MenuDown.Repeating);
			AddInput('r', () => Input.MenuRight.Pressed && !Input.MenuRight.Repeating);
			AddInput('l', () => Input.MenuLeft.Pressed && !Input.MenuLeft.Repeating);
			AddInput('A', () => Input.MenuConfirm.Pressed);
			AddInput('L', () => Input.MenuJournal.Pressed);
			AddInput('R', () => Input.Grab.Pressed && !Input.MenuJournal.Pressed);
			AddCheat("lrLRuudlRA", EnteredCheat);
			Logging = true;
		}

		public void EnteredCheat()
		{
			Level level = SceneAs<Level>();
			level.PauseLock = true;
			level.Frozen = true;
			level.Flash(Color.White);
			Audio.Play("event:/game/06_reflection/feather_bubble_get", (base.Scene as Level).Camera.Position + new Vector2(160f, 90f));
			new FadeWipe(base.Scene, wipeIn: false, delegate
			{
				UnlockEverything(level);
			}).Duration = 2f;
			RemoveSelf();
		}

		public void UnlockEverything(Level level)
		{
			SaveData.Instance.RevealedChapter9 = true;
			SaveData.Instance.UnlockedAreas = SaveData.Instance.MaxArea;
			SaveData.Instance.CheatMode = true;
			Settings.Instance.Pico8OnMainMenu = true;
			Settings.Instance.VariantsUnlocked = true;
			level.Session.InArea = false;
			Engine.Scene = new LevelExit(LevelExit.Mode.GiveUp, level.Session);
		}
	}
}
