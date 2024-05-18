using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class LevelLoader : Scene
	{
		private Session session;

		private Vector2? startPosition;

		private bool started;

		public Player.IntroTypes? PlayerIntroTypeOverride;

		public Level Level { get; private set; }

		public bool Loaded { get; private set; }

		public LevelLoader(Session session, Vector2? startPosition = null)
		{
			this.session = session;
			if (!startPosition.HasValue)
			{
				this.startPosition = session.RespawnPoint;
			}
			else
			{
				this.startPosition = startPosition;
			}
			Level = new Level();
            LoadingThread();
			//RunThread.Start(LoadingThread, "LEVEL_LOADER");
		}

		private void LoadingThread()
		{
			MapData mapData = session.MapData;
			AreaData overworldData = AreaData.Get(session);
			if (session.Area.ID == 0)
			{
				SaveData.Instance.Assists.DashMode = Assists.DashModes.Normal;
			}
			Level.Add(Level.GameplayRenderer = new GameplayRenderer());
			Level.Add(Level.Lighting = new LightingRenderer());
			Level.Add(Level.Bloom = new BloomRenderer());
			Level.Add(Level.Displacement = new DisplacementRenderer());
			Level.Add(Level.Background = new BackdropRenderer());
			Level.Add(Level.Foreground = new BackdropRenderer());
			Level.Add(new DustEdges());
			Level.Add(new WaterSurface());
			Level.Add(new MirrorSurfaces());
			Level.Add(new GlassBlockBg());
			Level.Add(new LightningRenderer());
			Level.Add(new SeekerBarrierRenderer());
			Level.Add(Level.HudRenderer = new HudRenderer());
			if (session.Area.ID == 9)
			{
				Level.Add(new IceTileOverlay());
			}
			Level.BaseLightingAlpha = (Level.Lighting.Alpha = overworldData.DarknessAlpha);
			Level.Bloom.Base = overworldData.BloomBase;
			Level.Bloom.Strength = overworldData.BloomStrength;
			Level.BackgroundColor = mapData.BackgroundColor;
			Level.Background.Backdrops = mapData.CreateBackdrops(mapData.Background);
			foreach (Backdrop backdrop in Level.Background.Backdrops)
			{
				backdrop.Renderer = Level.Background;
			}
			Level.Foreground.Backdrops = mapData.CreateBackdrops(mapData.Foreground);
			foreach (Backdrop backdrop2 in Level.Foreground.Backdrops)
			{
				backdrop2.Renderer = Level.Foreground;
			}
			Level.RendererList.UpdateLists();
			Level.Add(Level.FormationBackdrop = new FormationBackdrop());
			Level.Camera = Level.GameplayRenderer.Camera;
			Audio.SetCamera(Level.Camera);
			Level.Session = session;
			SaveData.Instance.StartSession(Level.Session);
			Level.Particles = new ParticleSystem(-8000, 400);
			Level.Particles.Tag = Tags.Global;
			Level.Add(Level.Particles);
			Level.ParticlesBG = new ParticleSystem(8000, 400);
			Level.ParticlesBG.Tag = Tags.Global;
			Level.Add(Level.ParticlesBG);
			Level.ParticlesFG = new ParticleSystem(-50000, 800);
			Level.ParticlesFG.Tag = Tags.Global;
			Level.ParticlesFG.Add(new MirrorReflection());
			Level.Add(Level.ParticlesFG);
			Level.Add(Level.strawberriesDisplay = new TotalStrawberriesDisplay());
			Level.Add(new SpeedrunTimerDisplay());
			Level.Add(new GameplayStats());
			Level.Add(new GrabbyIcon());
			Rectangle bounds = mapData.TileBounds;
			GFX.FGAutotiler.LevelBounds.Clear();
			VirtualMap<char> bgAutoTiles = new VirtualMap<char>(bounds.Width, bounds.Height, '0');
			VirtualMap<char> fgAutoTiles = new VirtualMap<char>(bounds.Width, bounds.Height, '0');
			VirtualMap<bool> insideLevel = new VirtualMap<bool>(bounds.Width, bounds.Height, emptyValue: false);
			Regex splitOn = new Regex("\\r\\n|\\n\\r|\\n|\\r");
			foreach (LevelData lvl2 in mapData.Levels)
			{
				int levelTileX2 = lvl2.TileBounds.Left;
				int levelTileY2 = lvl2.TileBounds.Top;
				string[] levelRows = splitOn.Split(lvl2.Bg);
				for (int y = levelTileY2; y < levelTileY2 + levelRows.Length; y++)
				{
					for (int x2 = levelTileX2; x2 < levelTileX2 + levelRows[y - levelTileY2].Length; x2++)
					{
						bgAutoTiles[x2 - bounds.X, y - bounds.Y] = levelRows[y - levelTileY2][x2 - levelTileX2];
					}
				}
				string[] levelRows2 = splitOn.Split(lvl2.Solids);
				for (int y2 = levelTileY2; y2 < levelTileY2 + levelRows2.Length; y2++)
				{
					for (int x = levelTileX2; x < levelTileX2 + levelRows2[y2 - levelTileY2].Length; x++)
					{
						fgAutoTiles[x - bounds.X, y2 - bounds.Y] = levelRows2[y2 - levelTileY2][x - levelTileX2];
					}
				}
				for (int tx4 = lvl2.TileBounds.Left; tx4 < lvl2.TileBounds.Right; tx4++)
				{
					for (int ty5 = lvl2.TileBounds.Top; ty5 < lvl2.TileBounds.Bottom; ty5++)
					{
						insideLevel[tx4 - bounds.Left, ty5 - bounds.Top] = true;
					}
				}
				GFX.FGAutotiler.LevelBounds.Add(new Rectangle(lvl2.TileBounds.X - bounds.X, lvl2.TileBounds.Y - bounds.Y, lvl2.TileBounds.Width, lvl2.TileBounds.Height));
			}
			foreach (Rectangle filler in mapData.Filler)
			{
				for (int x3 = filler.Left; x3 < filler.Right; x3++)
				{
					for (int y3 = filler.Top; y3 < filler.Bottom; y3++)
					{
						char fill = '0';
						if (filler.Top - bounds.Y > 0)
						{
							char above = fgAutoTiles[x3 - bounds.X, filler.Top - bounds.Y - 1];
							if (above != '0')
							{
								fill = above;
							}
						}
						if (fill == '0' && filler.Left - bounds.X > 0)
						{
							char left = fgAutoTiles[filler.Left - bounds.X - 1, y3 - bounds.Y];
							if (left != '0')
							{
								fill = left;
							}
						}
						if (fill == '0' && filler.Right - bounds.X < bounds.Width - 1)
						{
							char right = fgAutoTiles[filler.Right - bounds.X, y3 - bounds.Y];
							if (right != '0')
							{
								fill = right;
							}
						}
						if (fill == '0' && filler.Bottom - bounds.Y < bounds.Height - 1)
						{
							char below = fgAutoTiles[x3 - bounds.X, filler.Bottom - bounds.Y];
							if (below != '0')
							{
								fill = below;
							}
						}
						if (fill == '0')
						{
							fill = '1';
						}
						fgAutoTiles[x3 - bounds.X, y3 - bounds.Y] = fill;
						insideLevel[x3 - bounds.X, y3 - bounds.Y] = true;
					}
				}
			}
			foreach (LevelData lvl5 in mapData.Levels)
			{
				for (int tx6 = lvl5.TileBounds.Left; tx6 < lvl5.TileBounds.Right; tx6++)
				{
					int ty4 = lvl5.TileBounds.Top;
					char bgup = bgAutoTiles[tx6 - bounds.X, ty4 - bounds.Y];
					for (int push8 = 1; push8 < 4 && !insideLevel[tx6 - bounds.X, ty4 - bounds.Y - push8]; push8++)
					{
						bgAutoTiles[tx6 - bounds.X, ty4 - bounds.Y - push8] = bgup;
					}
					ty4 = lvl5.TileBounds.Bottom - 1;
					char bgdown = bgAutoTiles[tx6 - bounds.X, ty4 - bounds.Y];
					for (int push7 = 1; push7 < 4 && !insideLevel[tx6 - bounds.X, ty4 - bounds.Y + push7]; push7++)
					{
						bgAutoTiles[tx6 - bounds.X, ty4 - bounds.Y + push7] = bgdown;
					}
				}
				for (int ty6 = lvl5.TileBounds.Top - 4; ty6 < lvl5.TileBounds.Bottom + 4; ty6++)
				{
					int tx5 = lvl5.TileBounds.Left;
					char bgleft = bgAutoTiles[tx5 - bounds.X, ty6 - bounds.Y];
					for (int push10 = 1; push10 < 4 && !insideLevel[tx5 - bounds.X - push10, ty6 - bounds.Y]; push10++)
					{
						bgAutoTiles[tx5 - bounds.X - push10, ty6 - bounds.Y] = bgleft;
					}
					tx5 = lvl5.TileBounds.Right - 1;
					char bgright = bgAutoTiles[tx5 - bounds.X, ty6 - bounds.Y];
					for (int push9 = 1; push9 < 4 && !insideLevel[tx5 - bounds.X + push9, ty6 - bounds.Y]; push9++)
					{
						bgAutoTiles[tx5 - bounds.X + push9, ty6 - bounds.Y] = bgright;
					}
				}
			}
			foreach (LevelData lvl4 in mapData.Levels)
			{
				for (int tx3 = lvl4.TileBounds.Left; tx3 < lvl4.TileBounds.Right; tx3++)
				{
					int ty3 = lvl4.TileBounds.Top;
					if (fgAutoTiles[tx3 - bounds.X, ty3 - bounds.Y] == '0')
					{
						for (int push6 = 1; push6 < 8; push6++)
						{
							insideLevel[tx3 - bounds.X, ty3 - bounds.Y - push6] = true;
						}
					}
					ty3 = lvl4.TileBounds.Bottom - 1;
					if (fgAutoTiles[tx3 - bounds.X, ty3 - bounds.Y] == '0')
					{
						for (int push5 = 1; push5 < 8; push5++)
						{
							insideLevel[tx3 - bounds.X, ty3 - bounds.Y + push5] = true;
						}
					}
				}
			}
			foreach (LevelData lvl3 in mapData.Levels)
			{
				for (int tx2 = lvl3.TileBounds.Left; tx2 < lvl3.TileBounds.Right; tx2++)
				{
					int ty = lvl3.TileBounds.Top;
					char fgup = fgAutoTiles[tx2 - bounds.X, ty - bounds.Y];
					for (int push2 = 1; push2 < 4 && !insideLevel[tx2 - bounds.X, ty - bounds.Y - push2]; push2++)
					{
						fgAutoTiles[tx2 - bounds.X, ty - bounds.Y - push2] = fgup;
					}
					ty = lvl3.TileBounds.Bottom - 1;
					char fgdown = fgAutoTiles[tx2 - bounds.X, ty - bounds.Y];
					for (int push = 1; push < 4 && !insideLevel[tx2 - bounds.X, ty - bounds.Y + push]; push++)
					{
						fgAutoTiles[tx2 - bounds.X, ty - bounds.Y + push] = fgdown;
					}
				}
				for (int ty2 = lvl3.TileBounds.Top - 4; ty2 < lvl3.TileBounds.Bottom + 4; ty2++)
				{
					int tx = lvl3.TileBounds.Left;
					char fgleft = fgAutoTiles[tx - bounds.X, ty2 - bounds.Y];
					for (int push4 = 1; push4 < 4 && !insideLevel[tx - bounds.X - push4, ty2 - bounds.Y]; push4++)
					{
						fgAutoTiles[tx - bounds.X - push4, ty2 - bounds.Y] = fgleft;
					}
					tx = lvl3.TileBounds.Right - 1;
					char fgright = fgAutoTiles[tx - bounds.X, ty2 - bounds.Y];
					for (int push3 = 1; push3 < 4 && !insideLevel[tx - bounds.X + push3, ty2 - bounds.Y]; push3++)
					{
						fgAutoTiles[tx - bounds.X + push3, ty2 - bounds.Y] = fgright;
					}
				}
			}
			Vector2 tl = new Vector2(bounds.X, bounds.Y) * 8f;
			Calc.PushRandom(mapData.LoadSeed);
			BackgroundTiles bgs = null;
			SolidTiles fgs = null;
			Level.Add(Level.BgTiles = (bgs = new BackgroundTiles(tl, bgAutoTiles)));
			Level.Add(Level.SolidTiles = (fgs = new SolidTiles(tl, fgAutoTiles)));
			Level.BgData = bgAutoTiles;
			Level.SolidsData = fgAutoTiles;
			Calc.PopRandom();
			new Entity(tl).Add(Level.FgTilesLightMask = new TileGrid(8, 8, bounds.Width, bounds.Height));
			Level.FgTilesLightMask.Color = Color.Black;
			foreach (LevelData lvl in mapData.Levels)
			{
				int levelTileX = lvl.TileBounds.Left;
				int levelTileY = lvl.TileBounds.Top;
				int levelTileW = lvl.TileBounds.Width;
				int levelTileH = lvl.TileBounds.Height;
				if (!string.IsNullOrEmpty(lvl.BgTiles))
				{
					int[,] tiles2 = Calc.ReadCSVIntGrid(lvl.BgTiles, levelTileW, levelTileH);
					bgs.Tiles.Overlay(GFX.SceneryTiles, tiles2, levelTileX - bounds.X, levelTileY - bounds.Y);
				}
				if (!string.IsNullOrEmpty(lvl.FgTiles))
				{
					int[,] tiles = Calc.ReadCSVIntGrid(lvl.FgTiles, levelTileW, levelTileH);
					fgs.Tiles.Overlay(GFX.SceneryTiles, tiles, levelTileX - bounds.X, levelTileY - bounds.Y);
					Level.FgTilesLightMask.Overlay(GFX.SceneryTiles, tiles, levelTileX - bounds.X, levelTileY - bounds.Y);
				}
			}
			if (overworldData.OnLevelBegin != null)
			{
				overworldData.OnLevelBegin(Level);
			}
			Level.StartPosition = startPosition;
			Level.Pathfinder = new Pathfinder(Level);
			Loaded = true;
		}

		private void StartLevel()
		{
			started = true;
			Session session = Level.Session;
			Player.IntroTypes introType = (PlayerIntroTypeOverride.HasValue ? PlayerIntroTypeOverride.Value : ((!session.FirstLevel || !session.StartedFromBeginning || !session.JustStarted) ? Player.IntroTypes.Respawn : ((session.Area.Mode != AreaMode.CSide) ? AreaData.Get(Level).IntroType : Player.IntroTypes.WalkInRight)));
			Level.LoadLevel(introType, isFromLoader: true);
			Level.Session.JustStarted = false;
			if (Engine.Scene == this)
			{
				Engine.Scene = Level;
			}
		}

		public override void Update()
		{
			base.Update();
			if (Loaded && !started)
			{
				StartLevel();
			}
		}
	}
}
