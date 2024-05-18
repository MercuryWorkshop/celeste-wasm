using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class GameLoader : Scene
	{
		public HiresSnow Snow;

		private Atlas opening;

		private bool loaded;

		private bool audioLoaded;

		private bool audioStarted;

		private bool dialogLoaded;

		private Entity handler;

		private Thread activeThread;

		private bool skipped;

		private bool ready;

		private List<MTexture> loadingTextures;

		private float loadingFrame;

		private float loadingAlpha;

		public GameLoader()
		{
			Console.WriteLine("GAME DISPLAYED (in " + Celeste.LoadTimer.ElapsedMilliseconds + "ms)");
			Snow = new HiresSnow();
			opening = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Opening"), Atlas.AtlasDataFormat.PackerNoAtlas);
		}

		public override void Begin()
		{
			Add(new HudRenderer());
			Add(Snow);
			new FadeWipe(this, wipeIn: true);
			base.RendererList.UpdateLists();
			Add(handler = new Entity());
			handler.Tag = Tags.HUD;
			handler.Add(new Coroutine(IntroRoutine()));
			activeThread = Thread.CurrentThread;
            // Unsupported in WASM
			// activeThread.Priority = ThreadPriority.Lowest;
			//RunThread.Start(LoadThread, "GAME_LOADER", highPriority: true);
            LoadThread();
		}

		private void LoadThread()
		{
			MInput.Disabled = true;
			Stopwatch t3 = Stopwatch.StartNew();
			Audio.Init();
			Audio.Banks.Master = Audio.Banks.Load("Master Bank", loadStrings: true);
			Audio.Banks.Music = Audio.Banks.Load("music", loadStrings: false);
			Audio.Banks.Sfxs = Audio.Banks.Load("sfx", loadStrings: false);
			Audio.Banks.UI = Audio.Banks.Load("ui", loadStrings: false);
			Audio.Banks.DlcMusic = Audio.Banks.Load("dlc_music", loadStrings: false);
			Audio.Banks.DlcSfxs = Audio.Banks.Load("dlc_sfx", loadStrings: false);
			Settings.Instance.ApplyVolumes();
			audioLoaded = true;
			Console.WriteLine(" - AUDIO LOAD: " + t3.ElapsedMilliseconds + "ms");
			GFX.Load();
			MTN.Load();
			GFX.LoadData();
			MTN.LoadData();
			Stopwatch t2 = Stopwatch.StartNew();
			Fonts.Prepare();
			Dialog.Load();
			Fonts.Load(Dialog.Languages["english"].FontFace);
			Fonts.Load(Dialog.Languages[Settings.Instance.Language].FontFace);
			dialogLoaded = true;
			Console.WriteLine(" - DIA/FONT LOAD: " + t2.ElapsedMilliseconds + "ms");
			MInput.Disabled = false;
			Stopwatch t = Stopwatch.StartNew();
			AreaData.Load();
			Console.WriteLine(" - LEVELS LOAD: " + t.ElapsedMilliseconds + "ms");
			Console.WriteLine("DONE LOADING (in " + Celeste.LoadTimer.ElapsedMilliseconds + "ms)");
			Celeste.LoadTimer.Stop();
			Celeste.LoadTimer = null;
			loaded = true;
		}

		public IEnumerator IntroRoutine()
		{
			if (Celeste.PlayMode != Celeste.PlayModes.Debug)
			{
				for (float p2 = 0f; p2 > 1f; p2 += Engine.DeltaTime)
				{
					if (skipped)
					{
						break;
					}
					yield return null;
				}
				if (!skipped)
				{
					Image img3 = new Image(opening["presentedby"]);
					yield return FadeInOut(img3);
				}
				if (!skipped)
				{
					Image img2 = new Image(opening["gameby"]);
					yield return FadeInOut(img2);
				}
				bool showAutoSaving = !Celeste.IsGGP;
				if (!skipped && showAutoSaving)
				{
					while (!dialogLoaded)
					{
						yield return null;
					}
					AutoSavingNotice notice = new AutoSavingNotice();
					Add(notice);
					for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime)
					{
						if (skipped)
						{
							break;
						}
						yield return null;
					}
					notice.Display = false;
					while (notice.StillVisible)
					{
						notice.ForceClose = skipped;
						yield return null;
					}
					Remove(notice);
				}
			}
			ready = true;
			if (!loaded)
			{
				loadingTextures = OVR.Atlas.GetAtlasSubtextures("loading/");
				Image img = new Image(loadingTextures[0]);
				img.CenterOrigin();
				img.Scale = Vector2.One * 0.5f;
				handler.Add(img);
				while (!loaded || loadingAlpha > 0f)
				{
					loadingFrame += Engine.DeltaTime * 10f;
					loadingAlpha = Calc.Approach(loadingAlpha, (!loaded) ? 1 : 0, Engine.DeltaTime * 4f);
					img.Texture = loadingTextures[(int)(loadingFrame % (float)loadingTextures.Count)];
					img.Color = Color.White * Ease.CubeOut(loadingAlpha);
					img.Position = new Vector2(1792f, 1080f - 128f * Ease.CubeOut(loadingAlpha));
					yield return null;
				}
			}
			opening.Dispose();
			//activeThread.Priority = ThreadPriority.Normal;
			MInput.Disabled = false;
			Engine.Scene = new OverworldLoader(Overworld.StartMode.Titlescreen, Snow);
		}

		private IEnumerator FadeInOut(Image img)
		{
			float alpha = 0f;
			img.Color = Color.White * 0f;
			handler.Add(img);
			for (float i = 0f; i < 4.5f; i += Engine.DeltaTime)
			{
				if (skipped)
				{
					break;
				}
				alpha = Ease.CubeOut(Math.Min(i, 1f));
				img.Color = Color.White * alpha;
				yield return null;
			}
			while (alpha > 0f)
			{
				alpha -= Engine.DeltaTime * (float)((!skipped) ? 1 : 8);
				img.Color = Color.White * alpha;
				yield return null;
			}
		}

		public override void Update()
		{
			if (audioLoaded && !audioStarted)
			{
				Audio.SetAmbience("event:/env/amb/worldmap");
				audioStarted = true;
			}
			if (!ready)
			{
				bool disabled = MInput.Disabled;
				MInput.Disabled = false;
				if (Input.MenuConfirm.Pressed)
				{
					skipped = true;
				}
				MInput.Disabled = disabled;
			}
			base.Update();
		}
	}
}
