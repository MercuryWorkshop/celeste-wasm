using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class SaveLoadIcon : Entity
	{
		public static SaveLoadIcon Instance;

		private bool display = true;

		private Sprite icon;

		private Wiggler wiggler;

		public static bool OnScreen => Instance != null;

		public static void Show(Scene scene)
		{
			if (Instance != null)
			{
				Instance.RemoveSelf();
			}
			scene.Add(Instance = new SaveLoadIcon());
		}

		public static void Hide()
		{
			if (Instance != null)
			{
				Instance.display = false;
			}
		}

		public SaveLoadIcon()
		{
			base.Tag = (int)Tags.HUD | (int)Tags.FrozenUpdate | (int)Tags.PauseUpdate | (int)Tags.Global;
			base.Depth = -1000000;
			Add(icon = GFX.GuiSpriteBank.Create("save"));
			icon.UseRawDeltaTime = true;
			Add(wiggler = Wiggler.Create(0.4f, 4f, delegate(float f)
			{
				icon.Rotation = f * 0.1f;
			}));
			wiggler.UseRawDeltaTime = true;
			Add(new Coroutine(Routine())
			{
				UseRawDeltaTime = true
			});
			icon.Visible = false;
		}

		private IEnumerator Routine()
		{
			icon.Play("start", restart: true);
			icon.Visible = true;
			yield return 0.25f;
			float timer = 1f;
			while (display)
			{
				timer -= Engine.DeltaTime;
				if (timer <= 0f)
				{
					wiggler.Start();
					timer = 1f;
				}
				yield return null;
			}
			icon.Play("end");
			yield return 0.5f;
			icon.Visible = false;
			yield return null;
			Instance = null;
			RemoveSelf();
		}

		public override void Render()
		{
			Position = new Vector2(1760f, 920f);
			base.Render();
		}
	}
}
