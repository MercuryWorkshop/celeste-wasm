using System;
using System.Collections;
using System.Threading;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class OverworldLoader : Scene
	{
		public Overworld.StartMode StartMode;

		public HiresSnow Snow;

		private bool loaded;

		private bool fadeIn;

		private Overworld overworld;

		private Postcard postcard;

		private bool showVariantPostcard;

		private bool showUnlockCSidePostcard;

		private Thread activeThread;

		public OverworldLoader(Overworld.StartMode startMode, HiresSnow snow = null)
		{
			StartMode = startMode;
			Snow = ((snow == null) ? new HiresSnow() : snow);
			fadeIn = snow == null;
		}

		public override void Begin()
		{
			Add(new HudRenderer());
			Add(Snow);
			if (fadeIn)
			{
				ScreenWipe.WipeColor = Color.Black;
				new FadeWipe(this, wipeIn: true);
			}
			base.RendererList.UpdateLists();
			Session session = null;
			if (SaveData.Instance != null)
			{
				session = SaveData.Instance.CurrentSession;
			}
			Entity e = new Entity();
			e.Add(new Coroutine(Routine(session)));
			Add(e);
			activeThread = Thread.CurrentThread;
			// activeThread.Priority = ThreadPriority.Lowest;
			// RunThread.Start(LoadThread, "OVERWORLD_LOADER", highPriority: true);
            LoadThread();
		}

		private void LoadThread()
		{
			if (!MTN.Loaded)
			{
				MTN.Load();
			}
			if (!MTN.DataLoaded)
			{
				MTN.LoadData();
			}
			CheckVariantsPostcardAtLaunch();
			overworld = new Overworld(this);
			overworld.Entities.UpdateLists();
			loaded = true;
			//activeThread.Priority = ThreadPriority.Normal;
		}

		private IEnumerator Routine(Session session)
		{
			if ((StartMode == Overworld.StartMode.AreaComplete || StartMode == Overworld.StartMode.AreaQuit) && session != null)
			{
				if (session.UnlockedCSide)
				{
					showUnlockCSidePostcard = true;
				}
				if (!Settings.Instance.VariantsUnlocked && SaveData.Instance != null && SaveData.Instance.TotalHeartGems >= 24)
				{
					showVariantPostcard = true;
				}
			}
			if (showUnlockCSidePostcard)
			{
				yield return 3f;
				Add(postcard = new Postcard(Dialog.Get("POSTCARD_CSIDES"), "event:/ui/main/postcard_csides_in", "event:/ui/main/postcard_csides_out"));
				yield return postcard.DisplayRoutine();
			}
			while (!loaded)
			{
				yield return null;
			}
			if (showVariantPostcard)
			{
				yield return 3f;
				Settings.Instance.VariantsUnlocked = true;
				Add(postcard = new Postcard(Dialog.Get("POSTCARD_VARIANTS"), "event:/new_content/ui/postcard_variants_in", "event:/new_content/ui/postcard_variants_out"));
				yield return postcard.DisplayRoutine();
				UserIO.SaveHandler(file: false, settings: true);
				while (UserIO.Saving)
				{
					yield return null;
				}
				while (SaveLoadIcon.Instance != null)
				{
					yield return null;
				}
			}
			Engine.Scene = overworld;
		}

		public override void BeforeRender()
		{
			base.BeforeRender();
			if (postcard != null)
			{
				postcard.BeforeRender();
			}
		}

		private void CheckVariantsPostcardAtLaunch()
		{
			if (StartMode != 0 || Settings.Instance.VariantsUnlocked || (Settings.LastVersion != null && !(new Version(Settings.LastVersion) <= new Version(1, 2, 4, 2))) || !UserIO.Open(UserIO.Mode.Read))
			{
				return;
			}
			for (int i = 0; i < 3; i++)
			{
				if (!UserIO.Exists(SaveData.GetFilename(i)))
				{
					continue;
				}
				SaveData savedata = UserIO.Load<SaveData>(SaveData.GetFilename(i));
				if (savedata != null)
				{
					savedata.AfterInitialize();
					if (savedata.TotalHeartGems >= 24)
					{
						showVariantPostcard = true;
						break;
					}
				}
			}
			UserIO.Close();
			SaveData.Instance = null;
		}
	}
}
