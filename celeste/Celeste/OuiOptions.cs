using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class OuiOptions : Oui
	{
		private TextMenu menu;

		private const float onScreenX = 960f;

		private const float offScreenX = 2880f;

		private string startLanguage;

		private string currentLanguage;

		private float alpha;

		public override void Added(Scene scene)
		{
			base.Added(scene);
		}

		private void ReloadMenu()
		{
			Vector2 pos = Vector2.Zero;
			int index = -1;
			if (menu != null)
			{
				pos = menu.Position;
				index = menu.Selection;
				base.Scene.Remove(menu);
			}
			menu = MenuOptions.Create();
			if (index >= 0)
			{
				menu.Selection = index;
				menu.Position = pos;
			}
			base.Scene.Add(menu);
		}

		public override IEnumerator Enter(Oui from)
		{
			ReloadMenu();
			menu.Visible = (Visible = true);
			menu.Focused = false;
			currentLanguage = (startLanguage = Settings.Instance.Language);
			for (float p = 0f; p < 1f; p += Engine.DeltaTime * 4f)
			{
				menu.X = 2880f + -1920f * Ease.CubeOut(p);
				alpha = Ease.CubeOut(p);
				yield return null;
			}
			menu.Focused = true;
		}

		public override IEnumerator Leave(Oui next)
		{
			Audio.Play("event:/ui/main/whoosh_large_out");
			menu.Focused = false;
			UserIO.SaveHandler(file: false, settings: true);
			while (UserIO.Saving)
			{
				yield return null;
			}
			for (float p = 0f; p < 1f; p += Engine.DeltaTime * 4f)
			{
				menu.X = 960f + 1920f * Ease.CubeIn(p);
				alpha = 1f - Ease.CubeIn(p);
				yield return null;
			}
			if (startLanguage != Settings.Instance.Language)
			{
				base.Overworld.ReloadMenus(Overworld.StartMode.ReturnFromOptions);
				yield return null;
			}
			menu.Visible = (Visible = false);
			menu.RemoveSelf();
			menu = null;
		}

		public override void Update()
		{
			if (menu != null && menu.Focused && base.Selected && Input.MenuCancel.Pressed)
			{
				Audio.Play("event:/ui/main/button_back");
				base.Overworld.Goto<OuiMainMenu>();
			}
			if (base.Selected && currentLanguage != Settings.Instance.Language)
			{
				currentLanguage = Settings.Instance.Language;
				ReloadMenu();
			}
			base.Update();
		}

		public override void Render()
		{
			if (alpha > 0f)
			{
				Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * alpha * 0.4f);
			}
			base.Render();
		}
	}
}
