using System.Collections;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class LevelExit : Scene
	{
		public enum Mode
		{
			SaveAndQuit,
			GiveUp,
			Restart,
			GoldenBerryRestart,
			Completed,
			CompletedInterlude
		}

		private Mode mode;

		private Session session;

		private float timer;

		private XmlElement completeXml;

		private Atlas completeAtlas;

		private bool completeLoaded;

		private HiresSnow snow;

		private OverworldLoader overworldLoader;

		public string GoldenStrawberryEntryLevel;

		private const float MinTimeForCompleteScreen = 3.3f;

		public LevelExit(Mode mode, Session session, HiresSnow snow = null)
		{
			Add(new HudRenderer());
			this.session = session;
			this.mode = mode;
			this.snow = snow;
		}

		public override void Begin()
		{
			base.Begin();
			if (mode != Mode.GoldenBerryRestart)
			{
				SaveLoadIcon.Show(this);
			}
			bool fade = snow == null;
			if (fade)
			{
				snow = new HiresSnow();
			}
			if (mode == Mode.Completed)
			{
				snow.Direction = new Vector2(0f, 16f);
				if (fade)
				{
					snow.Reset();
				}
                LoadCompleteThread();
				//RunThread.Start(LoadCompleteThread, "COMPLETE_LEVEL");
				if (session.Area.Mode != 0)
				{
					Audio.SetMusic("event:/music/menu/complete_bside");
				}
				else if (session.Area.ID == 7)
				{
					Audio.SetMusic("event:/music/menu/complete_summit");
				}
				else
				{
					Audio.SetMusic("event:/music/menu/complete_area");
				}
				Audio.SetAmbience(null);
			}
			if (mode == Mode.GiveUp)
			{
				overworldLoader = new OverworldLoader(Overworld.StartMode.AreaQuit, snow);
			}
			else if (mode == Mode.SaveAndQuit)
			{
				overworldLoader = new OverworldLoader(Overworld.StartMode.MainMenu, snow);
			}
			else if (mode == Mode.CompletedInterlude)
			{
				overworldLoader = new OverworldLoader(Overworld.StartMode.AreaComplete, snow);
			}
			Entity handler;
			Add(handler = new Entity());
			handler.Add(new Coroutine(Routine()));
			if (mode != Mode.Restart && mode != Mode.GoldenBerryRestart)
			{
				Add(snow);
				if (fade)
				{
					new FadeWipe(this, wipeIn: true);
				}
			}
			Stats.Store();
			base.RendererList.UpdateLists();
		}

		private void LoadCompleteThread()
		{
			completeXml = AreaData.Get(session).CompleteScreenXml;
			if (completeXml != null && completeXml.HasAttr("atlas"))
			{
				string path = Path.Combine("Graphics", "Atlases", completeXml.Attr("atlas"));
				completeAtlas = Atlas.FromAtlas(path, Atlas.AtlasDataFormat.PackerNoAtlas);
			}
			completeLoaded = true;
		}

		private IEnumerator Routine()
		{
			if (mode != Mode.GoldenBerryRestart)
			{
				UserIO.SaveHandler(file: true, settings: true);
				while (UserIO.Saving)
				{
					yield return null;
				}
				if (mode == Mode.Completed)
				{
					while (!completeLoaded)
					{
						yield return null;
					}
				}
				while (SaveLoadIcon.OnScreen)
				{
					yield return null;
				}
			}
			if (mode == Mode.Completed)
			{
				while (timer < 3.3f)
				{
					yield return null;
				}
				Audio.SetMusicParam("end", 1f);
				Engine.Scene = new AreaComplete(session, completeXml, completeAtlas, snow);
			}
			else if (mode == Mode.GiveUp || mode == Mode.SaveAndQuit || mode == Mode.CompletedInterlude)
			{
				Engine.Scene = overworldLoader;
			}
			else
			{
				if (mode != Mode.Restart && mode != Mode.GoldenBerryRestart)
				{
					yield break;
				}
				Session restartSession;
				if (mode == Mode.GoldenBerryRestart)
				{
					if ((session.Audio.Music.Event == "event:/music/lvl7/main" || session.Audio.Music.Event == "event:/music/lvl7/final_ascent") && session.Audio.Music.Progress > 0)
					{
						Audio.SetMusic(null);
					}
					restartSession = session.Restart(GoldenStrawberryEntryLevel);
				}
				else
				{
					restartSession = session.Restart();
				}
				LevelLoader loader = new LevelLoader(restartSession);
				if (mode == Mode.GoldenBerryRestart)
				{
					loader.PlayerIntroTypeOverride = Player.IntroTypes.Respawn;
				}
				Engine.Scene = loader;
			}
		}

		public override void Update()
		{
			timer += Engine.DeltaTime;
			base.Update();
		}
	}
}
