using System.Collections;
using Monocle;

namespace Celeste
{
	public class FileErrorOverlay : Overlay
	{
		public enum Error
		{
			Load,
			Save
		}

		private Error mode;

		private TextMenu menu;

		public bool Open { get; private set; }

		public bool TryAgain { get; private set; }

		public bool Ignore { get; private set; }

		public FileErrorOverlay(Error mode)
		{
			Open = true;
			this.mode = mode;
			Add(new Coroutine(Routine()));
			Engine.Scene.Add(this);
		}

		private IEnumerator Routine()
		{
			yield return FadeIn();
			bool waiting = true;
			int option = 0;
			Audio.Play("event:/ui/main/message_confirm");
			menu = new TextMenu();
			menu.Add(new TextMenu.Header(Dialog.Clean("savefailed_title")));
			menu.Add(new TextMenu.Button(Dialog.Clean((mode == Error.Save) ? "savefailed_retry" : "loadfailed_goback")).Pressed(delegate
			{
				option = 0;
				waiting = false;
			}));
			menu.Add(new TextMenu.Button(Dialog.Clean("savefailed_ignore")).Pressed(delegate
			{
				option = 1;
				waiting = false;
			}));
			while (waiting)
			{
				yield return null;
			}
			menu = null;
			Ignore = option == 1;
			TryAgain = option == 0;
			yield return FadeOut();
			Open = false;
			RemoveSelf();
		}

		public override void Update()
		{
			base.Update();
			if (menu != null)
			{
				menu.Update();
			}
			if (SaveLoadIcon.Instance != null && SaveLoadIcon.Instance.Scene == base.Scene)
			{
				SaveLoadIcon.Instance.Update();
			}
		}

		public override void Render()
		{
			RenderFade();
			if (menu != null)
			{
				menu.Render();
			}
			base.Render();
		}
	}
}
