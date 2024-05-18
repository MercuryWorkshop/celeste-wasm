using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Editor;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste
{
	public class Level : Scene, IOverlayHandler
	{
		public enum CameraLockModes
		{
			None,
			BoostSequence,
			FinalBoss,
			FinalBossNoY,
			Lava
		}

		private enum ConditionBlockModes
		{
			Key,
			Button,
			Strawberry
		}

		public bool Completed;

		public bool NewLevel;

		public bool TimerStarted;

		public bool TimerStopped;

		public bool TimerHidden;

		public Session Session;

		public Vector2? StartPosition;

		public bool DarkRoom;

		public Player.IntroTypes LastIntroType;

		public bool InCredits;

		public bool AllowHudHide = true;

		public VirtualMap<char> SolidsData;

		public VirtualMap<char> BgData;

		public float NextTransitionDuration = 0.65f;

		public const float DefaultTransitionDuration = 0.65f;

		public ScreenWipe Wipe;

		private Coroutine transition;

		private Coroutine saving;

		public FormationBackdrop FormationBackdrop;

		public SolidTiles SolidTiles;

		public BackgroundTiles BgTiles;

		public Color BackgroundColor = Color.Black;

		public BackdropRenderer Background;

		public BackdropRenderer Foreground;

		public GameplayRenderer GameplayRenderer;

		public HudRenderer HudRenderer;

		public LightingRenderer Lighting;

		public DisplacementRenderer Displacement;

		public BloomRenderer Bloom;

		public TileGrid FgTilesLightMask;

		public ParticleSystem Particles;

		public ParticleSystem ParticlesBG;

		public ParticleSystem ParticlesFG;

		public HiresSnow HiresSnow;

		public TotalStrawberriesDisplay strawberriesDisplay;

		private WindController windController;

		public const float CameraOffsetXInterval = 48f;

		public const float CameraOffsetYInterval = 32f;

		public Camera Camera;

		public CameraLockModes CameraLockMode;

		public Vector2 CameraOffset;

		public float CameraUpwardMaxY;

		private Vector2 shakeDirection;

		private int lastDirectionalShake;

		private float shakeTimer;

		private Vector2 cameraPreShake;

		public float ScreenPadding;

		private float flash;

		private Color flashColor = Color.White;

		private bool doFlash;

		private bool flashDrawPlayer;

		private float glitchTimer;

		private float glitchSeed;

		public float Zoom = 1f;

		public float ZoomTarget = 1f;

		public Vector2 ZoomFocusPoint;

		private string lastColorGrade;

		private float colorGradeEase;

		private float colorGradeEaseSpeed = 1f;

		public Vector2 Wind;

		public float WindSine;

		public float WindSineTimer;

		public bool Frozen;

		public bool PauseLock;

		public bool CanRetry = true;

		public bool PauseMainMenuOpen;

		private bool wasPaused;

		private float wasPausedTimer;

		private float unpauseTimer;

		public bool SaveQuitDisabled;

		public bool InCutscene;

		public bool SkippingCutscene;

		private Coroutine skipCoroutine;

		private Action<Level> onCutsceneSkip;

		private bool onCutsceneSkipFadeIn;

		private bool onCutsceneSkipResetZoom;

		private bool endingChapterAfterCutscene;

		public static EventInstance DialogSnapshot;

		private static EventInstance PauseSnapshot;

		private static EventInstance AssistSpeedSnapshot;

		private static int AssistSpeedSnapshotValue = -1;

		public Pathfinder Pathfinder;

		public PlayerDeadBody RetryPlayerCorpse;

		public float BaseLightingAlpha;

		private bool updateHair = true;

		public bool InSpace;

		public bool HasCassetteBlocks;

		public float CassetteBlockTempo;

		public int CassetteBlockBeats;

		public Random HiccupRandom;

		public bool Raining;

		private Session.CoreModes coreMode;

		public Vector2 LevelOffset => new Vector2(Bounds.Left, Bounds.Top);

		public Point LevelSolidOffset => new Point(Bounds.Left / 8 - TileBounds.X, Bounds.Top / 8 - TileBounds.Y);

		public Rectangle TileBounds => Session.MapData.TileBounds;

		public bool Transitioning => transition != null;

		public Vector2 ShakeVector { get; private set; }

		public float VisualWind => Wind.X + WindSine;

		public bool FrozenOrPaused
		{
			get
			{
				if (!Frozen)
				{
					return Paused;
				}
				return true;
			}
		}

		public bool CanPause
		{
			get
			{
				Player player = base.Tracker.GetEntity<Player>();
				if (player != null && !player.Dead && !wasPaused && !Paused && !PauseLock && !SkippingCutscene && !Transitioning && Wipe == null && !UserIO.Saving)
				{
					if (player.LastBooster != null && player.LastBooster.Ch9HubTransition)
					{
						return !player.LastBooster.BoostingPlayer;
					}
					return true;
				}
				return false;
			}
		}

		public Overlay Overlay { get; set; }

		public bool ShowHud
		{
			get
			{
				if (Completed)
				{
					return false;
				}
				if (Paused)
				{
					return true;
				}
				if (base.Tracker.GetEntity<Textbox>() == null && base.Tracker.GetEntity<MiniTextbox>() == null && !Frozen)
				{
					return !InCutscene;
				}
				return false;
			}
		}

		private bool ShouldCreateCassetteManager
		{
			get
			{
				if (Session.Area.Mode == AreaMode.Normal)
				{
					return !Session.Cassette;
				}
				return true;
			}
		}

		public Vector2 DefaultSpawnPoint => GetSpawnPoint(new Vector2(Bounds.Left, Bounds.Bottom));

		public Rectangle Bounds => Session.LevelData.Bounds;

		public Rectangle? PreviousBounds { get; private set; }

		public Session.CoreModes CoreMode
		{
			get
			{
				return coreMode;
			}
			set
			{
				if (coreMode == value)
				{
					return;
				}
				coreMode = value;
				Session.SetFlag("cold", coreMode == Session.CoreModes.Cold);
				Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "room_state", (coreMode != Session.CoreModes.Hot) ? 1 : 0);
				if (Audio.CurrentMusic == "event:/music/lvl9/main")
				{
					Session.Audio.Music.Layer(1, coreMode == Session.CoreModes.Hot);
					Session.Audio.Music.Layer(2, coreMode == Session.CoreModes.Cold);
					Session.Audio.Apply();
				}
				foreach (CoreModeListener cml in base.Tracker.GetComponents<CoreModeListener>())
				{
					if (cml.OnChange != null)
					{
						cml.OnChange(value);
					}
				}
			}
		}

		public override void Begin()
		{
			ScreenWipe.WipeColor = Color.Black;
			GameplayBuffers.Create();
			Distort.WaterAlpha = 1f;
			Distort.WaterSineDirection = 1f;
			Audio.MusicUnderwater = false;
			Audio.EndSnapshot(DialogSnapshot);
			base.Begin();
		}

		public override void End()
		{
			base.End();
			Foreground.Ended(this);
			Background.Ended(this);
			EndPauseEffects();
			Audio.BusStopAll("bus:/gameplay_sfx");
			Audio.MusicUnderwater = false;
			Audio.SetAmbience(null);
			Audio.SetAltMusic(null);
			Audio.EndSnapshot(DialogSnapshot);
			Audio.ReleaseSnapshot(AssistSpeedSnapshot);
			AssistSpeedSnapshot = null;
			AssistSpeedSnapshotValue = -1;
			GameplayBuffers.Unload();
			ClutterBlockGenerator.Dispose();
			Engine.TimeRateB = 1f;
		}

		public void LoadLevel(Player.IntroTypes playerIntro, bool isFromLoader = false)
		{
			TimerHidden = false;
			TimerStopped = false;
			LastIntroType = playerIntro;
			Background.Fade = 0f;
			CanRetry = true;
			ScreenPadding = 0f;
			Displacement.Enabled = true;
			PauseLock = false;
			Frozen = false;
			CameraLockMode = CameraLockModes.None;
			RetryPlayerCorpse = null;
			FormationBackdrop.Display = false;
			SaveQuitDisabled = false;
			lastColorGrade = Session.ColorGrade;
			colorGradeEase = 0f;
			colorGradeEaseSpeed = 1f;
			HasCassetteBlocks = false;
			CassetteBlockTempo = 1f;
			CassetteBlockBeats = 2;
			Raining = false;
			bool createdCassetteBlockManager = false;
			bool hasLightning = false;
			if (HiccupRandom == null)
			{
				HiccupRandom = new Random(Session.Area.ID * 77 + (int)Session.Area.Mode * 999);
			}
			base.Entities.FindFirst<LightningRenderer>()?.Reset();
			Calc.PushRandom(Session.LevelData.LoadSeed);
			_ = Session.MapData;
			LevelData levelData = Session.LevelData;
			Vector2 levelPosition = new Vector2(levelData.Bounds.Left, levelData.Bounds.Top);
			bool doCheckpoint = playerIntro != Player.IntroTypes.Fall || levelData.Name == "0";
			DarkRoom = levelData.Dark && !Session.GetFlag("ignore_darkness_" + levelData.Name);
			Zoom = 1f;
			if (Session.Audio == null)
			{
				Session.Audio = AreaData.Get(Session).Mode[(int)Session.Area.Mode].AudioState.Clone();
			}
			if (!levelData.DelayAltMusic)
			{
				Audio.SetAltMusic(SFX.EventnameByHandle(levelData.AltMusic));
			}
			if (levelData.Music.Length > 0)
			{
				Session.Audio.Music.Event = SFX.EventnameByHandle(levelData.Music);
			}
			if (!AreaData.GetMode(Session.Area).IgnoreLevelAudioLayerData)
			{
				for (int j = 0; j < 4; j++)
				{
					Session.Audio.Music.Layer(j + 1, levelData.MusicLayers[j]);
				}
			}
			if (levelData.MusicProgress >= 0)
			{
				Session.Audio.Music.Progress = levelData.MusicProgress;
			}
			Session.Audio.Music.Layer(6, levelData.MusicWhispers);
			if (levelData.Ambience.Length > 0)
			{
				Session.Audio.Ambience.Event = SFX.EventnameByHandle(levelData.Ambience);
			}
			if (levelData.AmbienceProgress >= 0)
			{
				Session.Audio.Ambience.Progress = levelData.AmbienceProgress;
			}
			Session.Audio.Apply(isFromLoader);
			CoreMode = Session.CoreMode;
			NewLevel = !Session.LevelFlags.Contains(levelData.Name);
			if (doCheckpoint)
			{
				if (!Session.LevelFlags.Contains(levelData.Name))
				{
					Session.FurthestSeenLevel = levelData.Name;
				}
				Session.LevelFlags.Add(levelData.Name);
				Session.UpdateLevelStartDashes();
			}
			Vector2? checkpoint = null;
			CameraOffset = new Vector2(48f, 32f) * levelData.CameraOffset;
			base.Entities.FindFirst<WindController>()?.RemoveSelf();
			Add(windController = new WindController(levelData.WindPattern));
			if (playerIntro != 0)
			{
				windController.SetStartPattern();
			}
			if (levelData.Underwater)
			{
				Add(new Water(levelPosition, topSurface: false, bottomSurface: false, levelData.Bounds.Width, levelData.Bounds.Height));
			}
			InSpace = levelData.Space;
			if (InSpace)
			{
				Add(new SpaceController());
			}
			if (levelData.Name == "-1" && Session.Area.ID == 0 && !SaveData.Instance.CheatMode)
			{
				Add(new UnlockEverythingThingy());
			}
			int chaserIndex = 0;
			List<EntityID> followingPlayer = new List<EntityID>();
			Player player2 = base.Tracker.GetEntity<Player>();
			if (player2 != null)
			{
				foreach (Follower follower in player2.Leader.Followers)
				{
					followingPlayer.Add(follower.ParentEntityID);
				}
			}
			foreach (EntityData e2 in levelData.Entities)
			{
				int id2 = e2.ID;
				EntityID gid2 = new EntityID(levelData.Name, id2);
				if (Session.DoNotLoad.Contains(gid2) || followingPlayer.Contains(gid2))
				{
					continue;
				}
				switch (e2.Name)
				{
				case "checkpoint":
					if (doCheckpoint)
					{
						Checkpoint c = new Checkpoint(e2, levelPosition);
						Add(c);
						checkpoint = e2.Position + levelPosition + c.SpawnOffset;
					}
					break;
				case "jumpThru":
					Add(new JumpthruPlatform(e2, levelPosition));
					break;
				case "refill":
					Add(new Refill(e2, levelPosition));
					break;
				case "infiniteStar":
					Add(new FlyFeather(e2, levelPosition));
					break;
				case "strawberry":
					Add(new Strawberry(e2, levelPosition, gid2));
					break;
				case "memorialTextController":
					if (Session.Dashes == 0 && Session.StartedFromBeginning)
					{
						Add(new Strawberry(e2, levelPosition, gid2));
					}
					break;
				case "goldenBerry":
				{
					bool cheatMode = SaveData.Instance.CheatMode;
					bool noDeaths = Session.FurthestSeenLevel == Session.Level || Session.Deaths == 0;
					bool unlocked = SaveData.Instance.UnlockedModes >= 3 || SaveData.Instance.DebugMode;
					bool completed = SaveData.Instance.Areas[Session.Area.ID].Modes[(int)Session.Area.Mode].Completed;
					if ((cheatMode || (unlocked && completed)) && noDeaths)
					{
						Add(new Strawberry(e2, levelPosition, gid2));
					}
					break;
				}
				case "summitgem":
					Add(new SummitGem(e2, levelPosition, gid2));
					break;
				case "blackGem":
					if (!Session.HeartGem || Session.Area.Mode != 0)
					{
						Add(new HeartGem(e2, levelPosition));
					}
					break;
				case "dreamHeartGem":
					if (!Session.HeartGem)
					{
						Add(new DreamHeartGem(e2, levelPosition));
					}
					break;
				case "spring":
					Add(new Spring(e2, levelPosition, Spring.Orientations.Floor));
					break;
				case "wallSpringLeft":
					Add(new Spring(e2, levelPosition, Spring.Orientations.WallLeft));
					break;
				case "wallSpringRight":
					Add(new Spring(e2, levelPosition, Spring.Orientations.WallRight));
					break;
				case "fallingBlock":
					Add(new FallingBlock(e2, levelPosition));
					break;
				case "zipMover":
					Add(new ZipMover(e2, levelPosition));
					break;
				case "crumbleBlock":
					Add(new CrumblePlatform(e2, levelPosition));
					break;
				case "dreamBlock":
					Add(new DreamBlock(e2, levelPosition));
					break;
				case "touchSwitch":
					Add(new TouchSwitch(e2, levelPosition));
					break;
				case "switchGate":
					Add(new SwitchGate(e2, levelPosition));
					break;
				case "negaBlock":
					Add(new NegaBlock(e2, levelPosition));
					break;
				case "key":
					Add(new Key(e2, levelPosition, gid2));
					break;
				case "lockBlock":
					Add(new LockBlock(e2, levelPosition, gid2));
					break;
				case "movingPlatform":
					Add(new MovingPlatform(e2, levelPosition));
					break;
				case "rotatingPlatforms":
				{
					Vector2 vector = e2.Position + levelPosition;
					Vector2 center = e2.Nodes[0] + levelPosition;
					int width = e2.Width;
					int amount = e2.Int("platforms");
					bool clockwise = e2.Bool("clockwise");
					float dist = (vector - center).Length();
					float angle = (vector - center).Angle();
					float angleAdd = (float)Math.PI * 2f / (float)amount;
					for (int i = 0; i < amount; i++)
					{
						float o = angle + angleAdd * (float)i;
						o = Calc.WrapAngle(o);
						Vector2 at = center + Calc.AngleToVector(o, dist);
						Add(new RotatingPlatform(at, width, center, clockwise));
					}
					break;
				}
				case "blockField":
					Add(new BlockField(e2, levelPosition));
					break;
				case "cloud":
					Add(new Cloud(e2, levelPosition));
					break;
				case "booster":
					Add(new Booster(e2, levelPosition));
					break;
				case "moveBlock":
					Add(new MoveBlock(e2, levelPosition));
					break;
				case "light":
					Add(new PropLight(e2, levelPosition));
					break;
				case "switchBlock":
				case "swapBlock":
					Add(new SwapBlock(e2, levelPosition));
					break;
				case "dashSwitchH":
				case "dashSwitchV":
					Add(DashSwitch.Create(e2, levelPosition, gid2));
					break;
				case "templeGate":
					Add(new TempleGate(e2, levelPosition, levelData.Name));
					break;
				case "torch":
					Add(new Torch(e2, levelPosition, gid2));
					break;
				case "templeCrackedBlock":
					Add(new TempleCrackedBlock(gid2, e2, levelPosition));
					break;
				case "seekerBarrier":
					Add(new SeekerBarrier(e2, levelPosition));
					break;
				case "theoCrystal":
					Add(new TheoCrystal(e2, levelPosition));
					break;
				case "glider":
					Add(new Glider(e2, levelPosition));
					break;
				case "theoCrystalPedestal":
					Add(new TheoCrystalPedestal(e2, levelPosition));
					break;
				case "badelineBoost":
					Add(new BadelineBoost(e2, levelPosition));
					break;
				case "cassette":
					if (!Session.Cassette)
					{
						Add(new Cassette(e2, levelPosition));
					}
					break;
				case "cassetteBlock":
				{
					CassetteBlock cb = new CassetteBlock(e2, levelPosition, gid2);
					Add(cb);
					HasCassetteBlocks = true;
					if (CassetteBlockTempo == 1f)
					{
						CassetteBlockTempo = cb.Tempo;
					}
					CassetteBlockBeats = Math.Max(cb.Index + 1, CassetteBlockBeats);
					if (!createdCassetteBlockManager)
					{
						createdCassetteBlockManager = true;
						if (base.Tracker.GetEntity<CassetteBlockManager>() == null && ShouldCreateCassetteManager)
						{
							Add(new CassetteBlockManager());
						}
					}
					break;
				}
				case "wallBooster":
					Add(new WallBooster(e2, levelPosition));
					break;
				case "bounceBlock":
					Add(new BounceBlock(e2, levelPosition));
					break;
				case "coreModeToggle":
					Add(new CoreModeToggle(e2, levelPosition));
					break;
				case "iceBlock":
					Add(new IceBlock(e2, levelPosition));
					break;
				case "fireBarrier":
					Add(new FireBarrier(e2, levelPosition));
					break;
				case "eyebomb":
					Add(new Puffer(e2, levelPosition));
					break;
				case "flingBird":
					Add(new FlingBird(e2, levelPosition));
					break;
				case "flingBirdIntro":
					Add(new FlingBirdIntro(e2, levelPosition));
					break;
				case "birdPath":
					Add(new BirdPath(gid2, e2, levelPosition));
					break;
				case "lightningBlock":
					Add(new LightningBreakerBox(e2, levelPosition));
					break;
				case "spikesUp":
					Add(new Spikes(e2, levelPosition, Spikes.Directions.Up));
					break;
				case "spikesDown":
					Add(new Spikes(e2, levelPosition, Spikes.Directions.Down));
					break;
				case "spikesLeft":
					Add(new Spikes(e2, levelPosition, Spikes.Directions.Left));
					break;
				case "spikesRight":
					Add(new Spikes(e2, levelPosition, Spikes.Directions.Right));
					break;
				case "triggerSpikesUp":
					Add(new TriggerSpikes(e2, levelPosition, TriggerSpikes.Directions.Up));
					break;
				case "triggerSpikesDown":
					Add(new TriggerSpikes(e2, levelPosition, TriggerSpikes.Directions.Down));
					break;
				case "triggerSpikesRight":
					Add(new TriggerSpikes(e2, levelPosition, TriggerSpikes.Directions.Right));
					break;
				case "triggerSpikesLeft":
					Add(new TriggerSpikes(e2, levelPosition, TriggerSpikes.Directions.Left));
					break;
				case "darkChaser":
					Add(new BadelineOldsite(e2, levelPosition, chaserIndex));
					chaserIndex++;
					break;
				case "rotateSpinner":
					if (Session.Area.ID == 10)
					{
						Add(new StarRotateSpinner(e2, levelPosition));
					}
					else if (Session.Area.ID == 3 || (Session.Area.ID == 7 && Session.Level.StartsWith("d-")))
					{
						Add(new DustRotateSpinner(e2, levelPosition));
					}
					else
					{
						Add(new BladeRotateSpinner(e2, levelPosition));
					}
					break;
				case "trackSpinner":
					if (Session.Area.ID == 10)
					{
						Add(new StarTrackSpinner(e2, levelPosition));
					}
					else if (Session.Area.ID == 3 || (Session.Area.ID == 7 && Session.Level.StartsWith("d-")))
					{
						Add(new DustTrackSpinner(e2, levelPosition));
					}
					else
					{
						Add(new BladeTrackSpinner(e2, levelPosition));
					}
					break;
				case "spinner":
				{
					if (Session.Area.ID == 3 || (Session.Area.ID == 7 && Session.Level.StartsWith("d-")))
					{
						Add(new DustStaticSpinner(e2, levelPosition));
						break;
					}
					CrystalColor color = CrystalColor.Blue;
					if (Session.Area.ID == 5)
					{
						color = CrystalColor.Red;
					}
					else if (Session.Area.ID == 6)
					{
						color = CrystalColor.Purple;
					}
					else if (Session.Area.ID == 10)
					{
						color = CrystalColor.Rainbow;
					}
					Add(new CrystalStaticSpinner(e2, levelPosition, color));
					break;
				}
				case "sinkingPlatform":
					Add(new SinkingPlatform(e2, levelPosition));
					break;
				case "friendlyGhost":
					Add(new AngryOshiro(e2, levelPosition));
					break;
				case "seeker":
					Add(new Seeker(e2, levelPosition));
					break;
				case "seekerStatue":
					Add(new SeekerStatue(e2, levelPosition));
					break;
				case "slider":
					Add(new Slider(e2, levelPosition));
					break;
				case "templeBigEyeball":
					Add(new TempleBigEyeball(e2, levelPosition));
					break;
				case "crushBlock":
					Add(new CrushBlock(e2, levelPosition));
					break;
				case "bigSpinner":
					Add(new Bumper(e2, levelPosition));
					break;
				case "starJumpBlock":
					Add(new StarJumpBlock(e2, levelPosition));
					break;
				case "floatySpaceBlock":
					Add(new FloatySpaceBlock(e2, levelPosition));
					break;
				case "glassBlock":
					Add(new GlassBlock(e2, levelPosition));
					break;
				case "goldenBlock":
					Add(new GoldenBlock(e2, levelPosition));
					break;
				case "fireBall":
					Add(new FireBall(e2, levelPosition));
					break;
				case "risingLava":
					Add(new RisingLava(e2, levelPosition));
					break;
				case "sandwichLava":
					Add(new SandwichLava(e2, levelPosition));
					break;
				case "killbox":
					Add(new Killbox(e2, levelPosition));
					break;
				case "fakeHeart":
					Add(new FakeHeart(e2, levelPosition));
					break;
				case "lightning":
					if (e2.Bool("perLevel") || !Session.GetFlag("disable_lightning"))
					{
						Add(new Lightning(e2, levelPosition));
						hasLightning = true;
					}
					break;
				case "finalBoss":
					Add(new FinalBoss(e2, levelPosition));
					break;
				case "finalBossFallingBlock":
					Add(FallingBlock.CreateFinalBossBlock(e2, levelPosition));
					break;
				case "finalBossMovingBlock":
					Add(new FinalBossMovingBlock(e2, levelPosition));
					break;
				case "fakeWall":
					Add(new FakeWall(gid2, e2, levelPosition, FakeWall.Modes.Wall));
					break;
				case "fakeBlock":
					Add(new FakeWall(gid2, e2, levelPosition, FakeWall.Modes.Block));
					break;
				case "dashBlock":
					Add(new DashBlock(e2, levelPosition, gid2));
					break;
				case "invisibleBarrier":
					Add(new InvisibleBarrier(e2, levelPosition));
					break;
				case "exitBlock":
					Add(new ExitBlock(e2, levelPosition));
					break;
				case "conditionBlock":
				{
					ConditionBlockModes mode = e2.Enum("condition", ConditionBlockModes.Key);
					EntityID cid = EntityID.None;
					string[] spl = e2.Attr("conditionID").Split(':');
					cid.Level = spl[0];
					cid.ID = Convert.ToInt32(spl[1]);
					if (mode switch
					{
						ConditionBlockModes.Button => Session.GetFlag(DashSwitch.GetFlagName(cid)), 
						ConditionBlockModes.Key => Session.DoNotLoad.Contains(cid), 
						ConditionBlockModes.Strawberry => Session.Strawberries.Contains(cid), 
						_ => throw new Exception("Condition type not supported!"), 
					})
					{
						Add(new ExitBlock(e2, levelPosition));
					}
					break;
				}
				case "coverupWall":
					Add(new CoverupWall(e2, levelPosition));
					break;
				case "crumbleWallOnRumble":
					Add(new CrumbleWallOnRumble(e2, levelPosition, gid2));
					break;
				case "ridgeGate":
					if (GotCollectables(e2))
					{
						Add(new RidgeGate(e2, levelPosition));
					}
					break;
				case "tentacles":
					Add(new ReflectionTentacles(e2, levelPosition));
					break;
				case "starClimbController":
					Add(new StarJumpController());
					break;
				case "playerSeeker":
					Add(new PlayerSeeker(e2, levelPosition));
					break;
				case "chaserBarrier":
					Add(new ChaserBarrier(e2, levelPosition));
					break;
				case "introCrusher":
					Add(new IntroCrusher(e2, levelPosition));
					break;
				case "bridge":
					Add(new Bridge(e2, levelPosition));
					break;
				case "bridgeFixed":
					Add(new BridgeFixed(e2, levelPosition));
					break;
				case "bird":
					Add(new BirdNPC(e2, levelPosition));
					break;
				case "introCar":
					Add(new IntroCar(e2, levelPosition));
					break;
				case "memorial":
					Add(new Memorial(e2, levelPosition));
					break;
				case "wire":
					Add(new Wire(e2, levelPosition));
					break;
				case "cobweb":
					Add(new Cobweb(e2, levelPosition));
					break;
				case "lamp":
					Add(new Lamp(levelPosition + e2.Position, e2.Bool("broken")));
					break;
				case "hanginglamp":
					Add(new HangingLamp(e2, levelPosition + e2.Position));
					break;
				case "hahaha":
					Add(new Hahaha(e2, levelPosition));
					break;
				case "bonfire":
					Add(new Bonfire(e2, levelPosition));
					break;
				case "payphone":
					Add(new Payphone(levelPosition + e2.Position));
					break;
				case "colorSwitch":
					Add(new ClutterSwitch(e2, levelPosition));
					break;
				case "clutterDoor":
					Add(new ClutterDoor(e2, levelPosition, Session));
					break;
				case "dreammirror":
					Add(new DreamMirror(levelPosition + e2.Position));
					break;
				case "resortmirror":
					Add(new ResortMirror(e2, levelPosition));
					break;
				case "towerviewer":
					Add(new Lookout(e2, levelPosition));
					break;
				case "picoconsole":
					Add(new PicoConsole(e2, levelPosition));
					break;
				case "wavedashmachine":
					Add(new WaveDashTutorialMachine(e2, levelPosition));
					break;
				case "yellowBlocks":
					ClutterBlockGenerator.Init(this);
					ClutterBlockGenerator.Add((int)(e2.Position.X / 8f), (int)(e2.Position.Y / 8f), e2.Width / 8, e2.Height / 8, ClutterBlock.Colors.Yellow);
					break;
				case "redBlocks":
					ClutterBlockGenerator.Init(this);
					ClutterBlockGenerator.Add((int)(e2.Position.X / 8f), (int)(e2.Position.Y / 8f), e2.Width / 8, e2.Height / 8, ClutterBlock.Colors.Red);
					break;
				case "greenBlocks":
					ClutterBlockGenerator.Init(this);
					ClutterBlockGenerator.Add((int)(e2.Position.X / 8f), (int)(e2.Position.Y / 8f), e2.Width / 8, e2.Height / 8, ClutterBlock.Colors.Green);
					break;
				case "oshirodoor":
					Add(new MrOshiroDoor(e2, levelPosition));
					break;
				case "templeMirrorPortal":
					Add(new TempleMirrorPortal(e2, levelPosition));
					break;
				case "reflectionHeartStatue":
					Add(new ReflectionHeartStatue(e2, levelPosition));
					break;
				case "resortRoofEnding":
					Add(new ResortRoofEnding(e2, levelPosition));
					break;
				case "gondola":
					Add(new Gondola(e2, levelPosition));
					break;
				case "birdForsakenCityGem":
					Add(new ForsakenCitySatellite(e2, levelPosition));
					break;
				case "whiteblock":
					Add(new WhiteBlock(e2, levelPosition));
					break;
				case "plateau":
					Add(new Plateau(e2, levelPosition));
					break;
				case "soundSource":
					Add(new SoundSourceEntity(e2, levelPosition));
					break;
				case "templeMirror":
					Add(new TempleMirror(e2, levelPosition));
					break;
				case "templeEye":
					Add(new TempleEye(e2, levelPosition));
					break;
				case "clutterCabinet":
					Add(new ClutterCabinet(e2, levelPosition));
					break;
				case "floatingDebris":
					Add(new FloatingDebris(e2, levelPosition));
					break;
				case "foregroundDebris":
					Add(new ForegroundDebris(e2, levelPosition));
					break;
				case "moonCreature":
					Add(new MoonCreature(e2, levelPosition));
					break;
				case "lightbeam":
					Add(new LightBeam(e2, levelPosition));
					break;
				case "door":
					Add(new Door(e2, levelPosition));
					break;
				case "trapdoor":
					Add(new Trapdoor(e2, levelPosition));
					break;
				case "resortLantern":
					Add(new ResortLantern(e2, levelPosition));
					break;
				case "water":
					Add(new Water(e2, levelPosition));
					break;
				case "waterfall":
					Add(new WaterFall(e2, levelPosition));
					break;
				case "bigWaterfall":
					Add(new BigWaterfall(e2, levelPosition));
					break;
				case "clothesline":
					Add(new Clothesline(e2, levelPosition));
					break;
				case "cliffflag":
					Add(new CliffFlags(e2, levelPosition));
					break;
				case "cliffside_flag":
					Add(new CliffsideWindFlag(e2, levelPosition));
					break;
				case "flutterbird":
					Add(new FlutterBird(e2, levelPosition));
					break;
				case "SoundTest3d":
					Add(new _3dSoundTest(e2, levelPosition));
					break;
				case "SummitBackgroundManager":
					Add(new AscendManager(e2, levelPosition));
					break;
				case "summitGemManager":
					Add(new SummitGemManager(e2, levelPosition));
					break;
				case "heartGemDoor":
					Add(new HeartGemDoor(e2, levelPosition));
					break;
				case "summitcheckpoint":
					Add(new SummitCheckpoint(e2, levelPosition));
					break;
				case "summitcloud":
					Add(new SummitCloud(e2, levelPosition));
					break;
				case "coreMessage":
					Add(new CoreMessage(e2, levelPosition));
					break;
				case "playbackTutorial":
					Add(new PlayerPlayback(e2, levelPosition));
					break;
				case "playbackBillboard":
					Add(new PlaybackBillboard(e2, levelPosition));
					break;
				case "cutsceneNode":
					Add(new CutsceneNode(e2, levelPosition));
					break;
				case "kevins_pc":
					Add(new KevinsPC(e2, levelPosition));
					break;
				case "powerSourceNumber":
					Add(new PowerSourceNumber(e2.Position + levelPosition, e2.Int("number", 1), GotCollectables(e2)));
					break;
				case "npc":
				{
					string npc = e2.Attr("npc").ToLower();
					Vector2 npcPosition = e2.Position + levelPosition;
					switch (npc)
					{
					case "granny_00_house":
						Add(new NPC00_Granny(npcPosition));
						break;
					case "theo_01_campfire":
						Add(new NPC01_Theo(npcPosition));
						break;
					case "theo_02_campfire":
						Add(new NPC02_Theo(npcPosition));
						break;
					case "theo_03_escaping":
						if (!Session.GetFlag("resort_theo"))
						{
							Add(new NPC03_Theo_Escaping(npcPosition));
						}
						break;
					case "theo_03_vents":
						Add(new NPC03_Theo_Vents(npcPosition));
						break;
					case "oshiro_03_lobby":
						Add(new NPC03_Oshiro_Lobby(npcPosition));
						break;
					case "oshiro_03_hallway":
						Add(new NPC03_Oshiro_Hallway1(npcPosition));
						break;
					case "oshiro_03_hallway2":
						Add(new NPC03_Oshiro_Hallway2(npcPosition));
						break;
					case "oshiro_03_bigroom":
						Add(new NPC03_Oshiro_Cluttter(e2, levelPosition));
						break;
					case "oshiro_03_breakdown":
						Add(new NPC03_Oshiro_Breakdown(npcPosition));
						break;
					case "oshiro_03_suite":
						Add(new NPC03_Oshiro_Suite(npcPosition));
						break;
					case "oshiro_03_rooftop":
						Add(new NPC03_Oshiro_Rooftop(npcPosition));
						break;
					case "granny_04_cliffside":
						Add(new NPC04_Granny(npcPosition));
						break;
					case "theo_04_cliffside":
						Add(new NPC04_Theo(npcPosition));
						break;
					case "theo_05_entrance":
						Add(new NPC05_Theo_Entrance(npcPosition));
						break;
					case "theo_05_inmirror":
						Add(new NPC05_Theo_Mirror(npcPosition));
						break;
					case "evil_05":
						Add(new NPC05_Badeline(e2, levelPosition));
						break;
					case "theo_06_plateau":
						Add(new NPC06_Theo_Plateau(e2, levelPosition));
						break;
					case "granny_06_intro":
						Add(new NPC06_Granny(e2, levelPosition));
						break;
					case "badeline_06_crying":
						Add(new NPC06_Badeline_Crying(e2, levelPosition));
						break;
					case "granny_06_ending":
						Add(new NPC06_Granny_Ending(e2, levelPosition));
						break;
					case "theo_06_ending":
						Add(new NPC06_Theo_Ending(e2, levelPosition));
						break;
					case "granny_07x":
						Add(new NPC07X_Granny_Ending(e2, levelPosition));
						break;
					case "theo_08_inside":
						Add(new NPC08_Theo(e2, levelPosition));
						break;
					case "granny_08_inside":
						Add(new NPC08_Granny(e2, levelPosition));
						break;
					case "granny_09_outside":
						Add(new NPC09_Granny_Outside(e2, levelPosition));
						break;
					case "granny_09_inside":
						Add(new NPC09_Granny_Inside(e2, levelPosition));
						break;
					case "gravestone_10":
						Add(new NPC10_Gravestone(e2, levelPosition));
						break;
					case "granny_10_never":
						Add(new NPC07X_Granny_Ending(e2, levelPosition, ch9EasterEgg: true));
						break;
					}
					break;
				}
				}
			}
			ClutterBlockGenerator.Generate();
			foreach (EntityData e in levelData.Triggers)
			{
				int id = e.ID + 10000000;
				EntityID gid = new EntityID(levelData.Name, id);
				if (Session.DoNotLoad.Contains(gid))
				{
					continue;
				}
				switch (e.Name)
				{
				case "eventTrigger":
					Add(new EventTrigger(e, levelPosition));
					break;
				case "musicFadeTrigger":
					Add(new MusicFadeTrigger(e, levelPosition));
					break;
				case "musicTrigger":
					Add(new MusicTrigger(e, levelPosition));
					break;
				case "altMusicTrigger":
					Add(new AltMusicTrigger(e, levelPosition));
					break;
				case "cameraOffsetTrigger":
					Add(new CameraOffsetTrigger(e, levelPosition));
					break;
				case "lightFadeTrigger":
					Add(new LightFadeTrigger(e, levelPosition));
					break;
				case "bloomFadeTrigger":
					Add(new BloomFadeTrigger(e, levelPosition));
					break;
				case "cameraTargetTrigger":
				{
					string flag = e.Attr("deleteFlag");
					if (string.IsNullOrEmpty(flag) || !Session.GetFlag(flag))
					{
						Add(new CameraTargetTrigger(e, levelPosition));
					}
					break;
				}
				case "cameraAdvanceTargetTrigger":
					Add(new CameraAdvanceTargetTrigger(e, levelPosition));
					break;
				case "respawnTargetTrigger":
					Add(new RespawnTargetTrigger(e, levelPosition));
					break;
				case "changeRespawnTrigger":
					Add(new ChangeRespawnTrigger(e, levelPosition));
					break;
				case "windTrigger":
					Add(new WindTrigger(e, levelPosition));
					break;
				case "windAttackTrigger":
					Add(new WindAttackTrigger(e, levelPosition));
					break;
				case "minitextboxTrigger":
					Add(new MiniTextboxTrigger(e, levelPosition, gid));
					break;
				case "oshiroTrigger":
					Add(new OshiroTrigger(e, levelPosition));
					break;
				case "interactTrigger":
					Add(new InteractTrigger(e, levelPosition));
					break;
				case "checkpointBlockerTrigger":
					Add(new CheckpointBlockerTrigger(e, levelPosition));
					break;
				case "lookoutBlocker":
					Add(new LookoutBlocker(e, levelPosition));
					break;
				case "stopBoostTrigger":
					Add(new StopBoostTrigger(e, levelPosition));
					break;
				case "noRefillTrigger":
					Add(new NoRefillTrigger(e, levelPosition));
					break;
				case "ambienceParamTrigger":
					Add(new AmbienceParamTrigger(e, levelPosition));
					break;
				case "creditsTrigger":
					Add(new CreditsTrigger(e, levelPosition));
					break;
				case "goldenBerryCollectTrigger":
					Add(new GoldBerryCollectTrigger(e, levelPosition));
					break;
				case "moonGlitchBackgroundTrigger":
					Add(new MoonGlitchBackgroundTrigger(e, levelPosition));
					break;
				case "blackholeStrength":
					Add(new BlackholeStrengthTrigger(e, levelPosition));
					break;
				case "rumbleTrigger":
					Add(new RumbleTrigger(e, levelPosition, gid));
					break;
				case "birdPathTrigger":
					Add(new BirdPathTrigger(e, levelPosition));
					break;
				case "spawnFacingTrigger":
					Add(new SpawnFacingTrigger(e, levelPosition));
					break;
				case "detachFollowersTrigger":
					Add(new DetachStrawberryTrigger(e, levelPosition));
					break;
				}
			}
			foreach (DecalData decal2 in levelData.FgDecals)
			{
				Add(new Decal(decal2.Texture, levelPosition + decal2.Position, decal2.Scale, -10500));
			}
			foreach (DecalData decal in levelData.BgDecals)
			{
				Add(new Decal(decal.Texture, levelPosition + decal.Position, decal.Scale, 9000));
			}
			if (playerIntro != 0)
			{
				if (Session.JustStarted && !Session.StartedFromBeginning && checkpoint.HasValue && !StartPosition.HasValue)
				{
					StartPosition = checkpoint;
				}
				if (!Session.RespawnPoint.HasValue)
				{
					if (StartPosition.HasValue)
					{
						Session.RespawnPoint = GetSpawnPoint(StartPosition.Value);
					}
					else
					{
						Session.RespawnPoint = DefaultSpawnPoint;
					}
				}
				PlayerSpriteMode spriteMode = ((!Session.Inventory.Backpack) ? PlayerSpriteMode.MadelineNoBackpack : PlayerSpriteMode.Madeline);
				Player player = new Player(Session.RespawnPoint.Value, spriteMode);
				player.IntroType = playerIntro;
				Add(player);
				base.Entities.UpdateLists();
				CameraLockModes finalBossCamera = CameraLockMode;
				CameraLockMode = CameraLockModes.None;
				Camera.Position = GetFullCameraTargetAt(player, player.Position);
				CameraLockMode = finalBossCamera;
				CameraUpwardMaxY = Camera.Y + 180f;
				foreach (EntityID key in Session.Keys)
				{
					Add(new Key(player, key));
				}
				SpotlightWipe.FocusPoint = Session.RespawnPoint.Value - Camera.Position;
				if (playerIntro != Player.IntroTypes.Respawn && playerIntro != Player.IntroTypes.Fall)
				{
					new SpotlightWipe(this, wipeIn: true);
				}
				else
				{
					DoScreenWipe(wipeIn: true);
				}
				if (isFromLoader)
				{
					base.RendererList.UpdateLists();
				}
				if (DarkRoom)
				{
					Lighting.Alpha = Session.DarkRoomAlpha;
				}
				else
				{
					Lighting.Alpha = BaseLightingAlpha + Session.LightingAlphaAdd;
				}
				Bloom.Base = AreaData.Get(Session).BloomBase + Session.BloomBaseAdd;
			}
			else
			{
				base.Entities.UpdateLists();
			}
			if (HasCassetteBlocks && ShouldCreateCassetteManager)
			{
				base.Tracker.GetEntity<CassetteBlockManager>()?.OnLevelStart();
			}
			if (!string.IsNullOrEmpty(levelData.ObjTiles))
			{
				Tileset tileset = new Tileset(GFX.Game["tilesets/scenery"], 8, 8);
				int[,] grid = Calc.ReadCSVIntGrid(levelData.ObjTiles, Bounds.Width / 8, Bounds.Height / 8);
				for (int x = 0; x < grid.GetLength(0); x++)
				{
					for (int y = 0; y < grid.GetLength(1); y++)
					{
						if (grid[x, y] != -1)
						{
							TileInterceptor.TileCheck(this, tileset[grid[x, y]], new Vector2(x * 8, y * 8) + LevelOffset);
						}
					}
				}
			}
			LightningRenderer lightning = base.Tracker.GetEntity<LightningRenderer>();
			if (lightning != null)
			{
				if (hasLightning)
				{
					lightning.StartAmbience();
				}
				else
				{
					lightning.StopAmbience();
				}
			}
			Calc.PopRandom();
		}

		public void UnloadLevel()
		{
			List<Entity> toRemove = GetEntitiesExcludingTagMask(Tags.Global);
			foreach (Entity textbox in base.Tracker.GetEntities<Textbox>())
			{
				toRemove.Add(textbox);
			}
			UnloadEntities(toRemove);
			base.Entities.UpdateLists();
		}

		public void Reload()
		{
			if (!Completed)
			{
				if (Session.FirstLevel && Session.Strawberries.Count <= 0 && !Session.Cassette && !Session.HeartGem && !Session.HitCheckpoint)
				{
					Session.Time = 0L;
					Session.Deaths = 0;
					TimerStarted = false;
				}
				Session.Dashes = Session.DashesAtLevelStart;
				Glitch.Value = 0f;
				Engine.TimeRate = 1f;
				Distort.Anxiety = 0f;
				Distort.GameRate = 1f;
				Audio.SetMusicParam("fade", 1f);
				ParticlesBG.Clear();
				Particles.Clear();
				ParticlesFG.Clear();
				TrailManager.Clear();
				UnloadLevel();
				GC.Collect();
				GC.WaitForPendingFinalizers();
				LoadLevel(Player.IntroTypes.Respawn);
				strawberriesDisplay.DrawLerp = 0f;
				WindController wc = base.Entities.FindFirst<WindController>();
				if (wc != null)
				{
					wc.SnapWind();
				}
				else
				{
					Wind = Vector2.Zero;
				}
			}
		}

		private bool GotCollectables(EntityData e)
		{
			bool gotAllStrawberries = true;
			bool gotAllKeys = true;
			List<EntityID> berryIds = new List<EntityID>();
			if (e.Attr("strawberries").Length > 0)
			{
				string[] array = e.Attr("strawberries").Split(',');
				foreach (string obj in array)
				{
					EntityID cid2 = EntityID.None;
					string[] spl2 = obj.Split(':');
					cid2.Level = spl2[0];
					cid2.ID = Convert.ToInt32(spl2[1]);
					berryIds.Add(cid2);
				}
			}
			foreach (EntityID sid2 in berryIds)
			{
				if (!Session.Strawberries.Contains(sid2))
				{
					gotAllStrawberries = false;
					break;
				}
			}
			List<EntityID> keyIds = new List<EntityID>();
			if (e.Attr("keys").Length > 0)
			{
				string[] array = e.Attr("keys").Split(',');
				foreach (string obj2 in array)
				{
					EntityID cid = EntityID.None;
					string[] spl = obj2.Split(':');
					cid.Level = spl[0];
					cid.ID = Convert.ToInt32(spl[1]);
					keyIds.Add(cid);
				}
			}
			foreach (EntityID sid in keyIds)
			{
				if (!Session.DoNotLoad.Contains(sid))
				{
					gotAllKeys = false;
					break;
				}
			}
			return gotAllKeys && gotAllStrawberries;
		}

		public void TransitionTo(LevelData next, Vector2 direction)
		{
			Session.CoreMode = CoreMode;
			transition = new Coroutine(TransitionRoutine(next, direction));
		}

		private IEnumerator TransitionRoutine(LevelData next, Vector2 direction)
		{
			Player player = base.Tracker.GetEntity<Player>();
			List<Entity> toRemove = GetEntitiesExcludingTagMask((int)Tags.Persistent | (int)Tags.Global);
			List<Component> transitionOut = base.Tracker.GetComponentsCopy<TransitionListener>();
			player.CleanUpTriggers();
			foreach (SoundSource sfx in base.Tracker.GetComponents<SoundSource>())
			{
				if (sfx.DisposeOnTransition)
				{
					sfx.Stop();
				}
			}
			PreviousBounds = Bounds;
			Session.Level = next.Name;
			Session.FirstLevel = false;
			Session.DeathsInCurrentLevel = 0;
			LoadLevel(Player.IntroTypes.Transition);
			Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "has_conveyors", (base.Tracker.GetEntities<WallBooster>().Count > 0) ? 1 : 0);
			List<Component> transitionIn = base.Tracker.GetComponentsCopy<TransitionListener>();
			transitionIn.RemoveAll((Component c) => transitionOut.Contains(c));
			GC.Collect();
			float cameraAt = 0f;
			Vector2 cameraFrom = Camera.Position;
			Vector2 inside = direction * 4f;
			if (direction == Vector2.UnitY)
			{
				inside = direction * 12f;
			}
			Vector2 playerTo = player.Position;
			while (direction.X != 0f && playerTo.Y >= (float)Bounds.Bottom)
			{
				playerTo.Y -= 1f;
			}
			for (; !IsInBounds(playerTo, inside); playerTo += direction)
			{
			}
			Vector2 cameraTo = GetFullCameraTargetAt(player, playerTo);
			Vector2 was = player.Position;
			player.Position = playerTo;
			foreach (Entity trigger in player.CollideAll<WindTrigger>())
			{
				if (!toRemove.Contains(trigger))
				{
					windController.SetPattern((trigger as WindTrigger).Pattern);
					break;
				}
			}
			windController.SetStartPattern();
			player.Position = was;
			foreach (TransitionListener tl5 in transitionOut)
			{
				if (tl5.OnOutBegin != null)
				{
					tl5.OnOutBegin();
				}
			}
			foreach (TransitionListener tl4 in transitionIn)
			{
				if (tl4.OnInBegin != null)
				{
					tl4.OnInBegin();
				}
			}
			float lightingStart = Lighting.Alpha;
			float lightingEnd = (DarkRoom ? Session.DarkRoomAlpha : (BaseLightingAlpha + Session.LightingAlphaAdd));
			bool lightingWait = lightingStart >= Session.DarkRoomAlpha || lightingEnd >= Session.DarkRoomAlpha;
			if (lightingEnd > lightingStart && lightingWait)
			{
				Audio.Play("event:/game/05_mirror_temple/room_lightlevel_down");
				while (Lighting.Alpha != lightingEnd)
				{
					yield return null;
					Lighting.Alpha = Calc.Approach(Lighting.Alpha, lightingEnd, 2f * Engine.DeltaTime);
				}
			}
			bool cameraFinished = false;
			while (!player.TransitionTo(playerTo, direction) || cameraAt < 1f)
			{
				yield return null;
				if (cameraFinished)
				{
					continue;
				}
				cameraAt = Calc.Approach(cameraAt, 1f, Engine.DeltaTime / NextTransitionDuration);
				if (cameraAt > 0.9f)
				{
					Camera.Position = cameraTo;
				}
				else
				{
					Camera.Position = Vector2.Lerp(cameraFrom, cameraTo, Ease.CubeOut(cameraAt));
				}
				if (!lightingWait && lightingStart < lightingEnd)
				{
					Lighting.Alpha = lightingStart + (lightingEnd - lightingStart) * cameraAt;
				}
				foreach (TransitionListener tl2 in transitionOut)
				{
					if (tl2.OnOut != null)
					{
						tl2.OnOut(cameraAt);
					}
				}
				foreach (TransitionListener tl in transitionIn)
				{
					if (tl.OnIn != null)
					{
						tl.OnIn(cameraAt);
					}
				}
				if (cameraAt >= 1f)
				{
					cameraFinished = true;
				}
			}
			if (lightingEnd < lightingStart && lightingWait)
			{
				Audio.Play("event:/game/05_mirror_temple/room_lightlevel_up");
				while (Lighting.Alpha != lightingEnd)
				{
					yield return null;
					Lighting.Alpha = Calc.Approach(Lighting.Alpha, lightingEnd, 2f * Engine.DeltaTime);
				}
			}
			UnloadEntities(toRemove);
			base.Entities.UpdateLists();
			Rectangle clearOutside = Bounds;
			clearOutside.Inflate(16, 16);
			Particles.ClearRect(clearOutside, inside: false);
			ParticlesBG.ClearRect(clearOutside, inside: false);
			ParticlesFG.ClearRect(clearOutside, inside: false);
			Vector2 from = player.CollideFirst<RespawnTargetTrigger>()?.Target ?? player.Position;
			Session.RespawnPoint = Session.LevelData.Spawns.ClosestTo(from);
			player.OnTransition();
			foreach (TransitionListener tl3 in transitionIn)
			{
				if (tl3.OnInEnd != null)
				{
					tl3.OnInEnd();
				}
			}
			if (Session.LevelData.DelayAltMusic)
			{
				Audio.SetAltMusic(SFX.EventnameByHandle(Session.LevelData.AltMusic));
			}
			NextTransitionDuration = 0.65f;
			transition = null;
		}

		public void UnloadEntities(List<Entity> entities)
		{
			foreach (Entity entity in entities)
			{
				Remove(entity);
			}
		}

		public Vector2 GetSpawnPoint(Vector2 from)
		{
			return Session.GetSpawnPoint(from);
		}

		public Vector2 GetFullCameraTargetAt(Player player, Vector2 at)
		{
			Vector2 was = player.Position;
			player.Position = at;
			foreach (Entity trigger in base.Tracker.GetEntities<Trigger>())
			{
				if (trigger is CameraTargetTrigger && player.CollideCheck(trigger))
				{
					(trigger as CameraTargetTrigger).OnStay(player);
				}
				else if (trigger is CameraOffsetTrigger && player.CollideCheck(trigger))
				{
					(trigger as CameraOffsetTrigger).OnEnter(player);
				}
			}
			Vector2 cameraTarget = player.CameraTarget;
			player.Position = was;
			return cameraTarget;
		}

		public void TeleportTo(Player player, string nextLevel, Player.IntroTypes introType, Vector2? nearestSpawn = null)
		{
			Leader.StoreStrawberries(player.Leader);
			Vector2 oldPosition = player.Position;
			Remove(player);
			UnloadLevel();
			Session.Level = nextLevel;
			Session.RespawnPoint = GetSpawnPoint(new Vector2(Bounds.Left, Bounds.Top) + (nearestSpawn.HasValue ? nearestSpawn.Value : Vector2.Zero));
			if (introType == Player.IntroTypes.Transition)
			{
				player.Position = Session.RespawnPoint.Value;
				player.Hair.MoveHairBy(player.Position - oldPosition);
				player.MuffleLanding = true;
				Add(player);
				LoadLevel(Player.IntroTypes.Transition);
				base.Entities.UpdateLists();
			}
			else
			{
				LoadLevel(introType);
				base.Entities.UpdateLists();
				player = base.Tracker.GetEntity<Player>();
			}
			Camera.Position = player.CameraTarget;
			Update();
			Leader.RestoreStrawberries(player.Leader);
		}

		public void AutoSave()
		{
			if (saving == null)
			{
				saving = new Coroutine(SavingRoutine());
			}
		}

		public bool IsAutoSaving()
		{
			return saving != null;
		}

		private IEnumerator SavingRoutine()
		{
			UserIO.SaveHandler(file: true, settings: false);
			while (UserIO.Saving)
			{
				yield return null;
			}
			saving = null;
		}

		public void UpdateTime()
		{
			if (InCredits || Session.Area.ID == 8 || TimerStopped)
			{
				return;
			}
			long time = TimeSpan.FromSeconds(Engine.RawDeltaTime).Ticks;
			SaveData.Instance.AddTime(Session.Area, time);
			if (!TimerStarted && !InCutscene)
			{
				Player player = base.Tracker.GetEntity<Player>();
				if (player != null && !player.TimePaused)
				{
					TimerStarted = true;
				}
			}
			if (!Completed && TimerStarted)
			{
				Session.Time += time;
			}
		}

		public override void Update()
		{
			if (unpauseTimer > 0f)
			{
				unpauseTimer -= Engine.RawDeltaTime;
				UpdateTime();
				return;
			}
			if (Overlay != null)
			{
				Overlay.Update();
				base.Entities.UpdateLists();
				return;
			}
			int assistSpeed = 10;
			if (!InCutscene && base.Tracker.GetEntity<Player>() != null && Wipe == null && !Frozen)
			{
				assistSpeed = SaveData.Instance.Assists.GameSpeed;
			}
			Engine.TimeRateB = (float)assistSpeed / 10f;
			if (assistSpeed != 10)
			{
				if (AssistSpeedSnapshot == null || AssistSpeedSnapshotValue != assistSpeed)
				{
					Audio.ReleaseSnapshot(AssistSpeedSnapshot);
					AssistSpeedSnapshot = null;
					AssistSpeedSnapshotValue = assistSpeed;
					if (AssistSpeedSnapshotValue < 10)
					{
						AssistSpeedSnapshot = Audio.CreateSnapshot("snapshot:/assist_game_speed/assist_speed_" + AssistSpeedSnapshotValue * 10);
					}
					else if (AssistSpeedSnapshotValue <= 16)
					{
						AssistSpeedSnapshot = Audio.CreateSnapshot("snapshot:/variant_speed/variant_speed_" + AssistSpeedSnapshotValue * 10);
					}
				}
			}
			else if (AssistSpeedSnapshot != null)
			{
				Audio.ReleaseSnapshot(AssistSpeedSnapshot);
				AssistSpeedSnapshot = null;
				AssistSpeedSnapshotValue = -1;
			}
			if (wasPaused && !Paused)
			{
				EndPauseEffects();
			}
			if (CanPause && Input.QuickRestart.Pressed)
			{
				Input.QuickRestart.ConsumeBuffer();
				Pause(0, minimal: false, quickReset: true);
			}
			else if (CanPause && (Input.Pause.Pressed || Input.ESC.Pressed))
			{
				Input.Pause.ConsumeBuffer();
				Input.ESC.ConsumeBuffer();
				Pause();
			}
			if (wasPaused && !Paused)
			{
				wasPaused = false;
			}
			if (Paused)
			{
				wasPausedTimer = 0f;
			}
			else
			{
				wasPausedTimer += Engine.DeltaTime;
			}
			UpdateTime();
			if (saving != null)
			{
				saving.Update();
			}
			if (!Paused)
			{
				glitchTimer += Engine.DeltaTime;
				glitchSeed = Calc.Random.NextFloat();
			}
			if (SkippingCutscene)
			{
				if (skipCoroutine != null)
				{
					skipCoroutine.Update();
				}
				base.RendererList.Update();
			}
			else if (FrozenOrPaused)
			{
				bool was = MInput.Disabled;
				MInput.Disabled = false;
				if (!Paused)
				{
					foreach (Entity e3 in base[Tags.FrozenUpdate])
					{
						if (e3.Active)
						{
							e3.Update();
						}
					}
				}
				foreach (Entity e2 in base[Tags.PauseUpdate])
				{
					if (e2.Active)
					{
						e2.Update();
					}
				}
				MInput.Disabled = was;
				if (Wipe != null)
				{
					Wipe.Update(this);
				}
				if (HiresSnow != null)
				{
					HiresSnow.Update(this);
				}
				base.Entities.UpdateLists();
			}
			else if (!Transitioning)
			{
				if (RetryPlayerCorpse == null)
				{
					base.Update();
				}
				else
				{
					RetryPlayerCorpse.Update();
					base.RendererList.Update();
					foreach (Entity e in base[Tags.PauseUpdate])
					{
						if (e.Active)
						{
							e.Update();
						}
					}
				}
			}
			else
			{
				foreach (Entity item in base[Tags.TransitionUpdate])
				{
					item.Update();
				}
				transition.Update();
				base.RendererList.Update();
			}
			HudRenderer.BackgroundFade = Calc.Approach(HudRenderer.BackgroundFade, Paused ? 1f : 0f, 8f * Engine.RawDeltaTime);
			if (!FrozenOrPaused)
			{
				WindSineTimer += Engine.DeltaTime;
				WindSine = (float)(Math.Sin(WindSineTimer) + 1.0) / 2f;
			}
			foreach (PostUpdateHook hook in base.Tracker.GetComponents<PostUpdateHook>())
			{
				if (hook.Entity.Active)
				{
					hook.OnPostUpdate();
				}
			}
			if (updateHair)
			{
				foreach (Component h in base.Tracker.GetComponents<PlayerHair>())
				{
					if (h.Active && h.Entity.Active)
					{
						(h as PlayerHair).AfterUpdate();
					}
				}
				if (FrozenOrPaused)
				{
					updateHair = false;
				}
			}
			else if (!FrozenOrPaused)
			{
				updateHair = true;
			}
			if (shakeTimer > 0f)
			{
				if (OnRawInterval(0.04f))
				{
					int value = (int)Math.Ceiling(shakeTimer * 10f);
					if (shakeDirection == Vector2.Zero)
					{
						ShakeVector = new Vector2(-value + Calc.Random.Next(value * 2 + 1), -value + Calc.Random.Next(value * 2 + 1));
					}
					else
					{
						if (lastDirectionalShake == 0)
						{
							lastDirectionalShake = 1;
						}
						else
						{
							lastDirectionalShake *= -1;
						}
						ShakeVector = -shakeDirection * lastDirectionalShake * value;
					}
					if (Settings.Instance.ScreenShake == ScreenshakeAmount.Half)
					{
						float x = Math.Sign(ShakeVector.X);
						float y = Math.Sign(ShakeVector.Y);
						ShakeVector = new Vector2(x, y);
					}
				}
				float decrease = ((Settings.Instance.ScreenShake == ScreenshakeAmount.Half) ? 1.5f : 1f);
				shakeTimer -= Engine.RawDeltaTime * decrease;
			}
			else
			{
				ShakeVector = Vector2.Zero;
			}
			if (doFlash)
			{
				flash = Calc.Approach(flash, 1f, Engine.DeltaTime * 10f);
				if (flash >= 1f)
				{
					doFlash = false;
				}
			}
			else if (flash > 0f)
			{
				flash = Calc.Approach(flash, 0f, Engine.DeltaTime * 3f);
			}
			if (lastColorGrade != Session.ColorGrade)
			{
				if (colorGradeEase >= 1f)
				{
					colorGradeEase = 0f;
					lastColorGrade = Session.ColorGrade;
				}
				else
				{
					colorGradeEase = Calc.Approach(colorGradeEase, 1f, Engine.DeltaTime * colorGradeEaseSpeed);
				}
			}
			if (Celeste.PlayMode == Celeste.PlayModes.Debug)
			{
				if (MInput.Keyboard.Pressed(Keys.Tab) && Engine.Scene.Tracker.GetEntity<KeyboardConfigUI>() == null && Engine.Scene.Tracker.GetEntity<ButtonConfigUI>() == null)
				{
					Engine.Scene = new MapEditor(Session.Area);
				}
				if (MInput.Keyboard.Pressed(Keys.F1))
				{
					Celeste.ReloadAssets(levels: true, graphics: false, hires: false, Session.Area);
					Engine.Scene = new LevelLoader(Session);
				}
				else if (MInput.Keyboard.Pressed(Keys.F2))
				{
					Celeste.ReloadAssets(levels: true, graphics: true, hires: false, Session.Area);
					Engine.Scene = new LevelLoader(Session);
				}
				else if (MInput.Keyboard.Pressed(Keys.F3))
				{
					Celeste.ReloadAssets(levels: true, graphics: true, hires: true, Session.Area);
					Engine.Scene = new LevelLoader(Session);
				}
			}
		}

		public override void BeforeRender()
		{
			cameraPreShake = Camera.Position;
			Camera.Position += ShakeVector;
			Camera.Position = Camera.Position.Floor();
			foreach (BeforeRenderHook beforeRenderHook in base.Tracker.GetComponents<BeforeRenderHook>())
			{
				if (beforeRenderHook.Visible)
				{
					beforeRenderHook.Callback();
				}
			}
			SpeedRing.DrawToBuffer(this);
			base.BeforeRender();
		}

		public override void Render()
		{
			Engine.Instance.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);
			Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
			GameplayRenderer.Render(this);
			Lighting.Render(this);
			Engine.Instance.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level);
			Engine.Instance.GraphicsDevice.Clear(BackgroundColor);
			Background.Render(this);
			Distort.Render((RenderTarget2D)GameplayBuffers.Gameplay, (RenderTarget2D)GameplayBuffers.Displacement, Displacement.HasDisplacement(this));
			Bloom.Apply(GameplayBuffers.Level, this);
			Foreground.Render(this);
			Glitch.Apply(GameplayBuffers.Level, glitchTimer * 2f, glitchSeed, (float)Math.PI * 2f);
			if (Engine.DashAssistFreeze)
			{
				PlayerDashAssist dasher = base.Tracker.GetEntity<PlayerDashAssist>();
				if (dasher != null)
				{
					Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Camera.Matrix);
					dasher.Render();
					Draw.SpriteBatch.End();
				}
			}
			if (flash > 0f)
			{
				Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
				Draw.Rect(-1f, -1f, 322f, 182f, flashColor * flash);
				Draw.SpriteBatch.End();
				if (flashDrawPlayer)
				{
					Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Camera.Matrix);
					Player player = base.Tracker.GetEntity<Player>();
					if (player != null && player.Visible)
					{
						player.Render();
					}
					Draw.SpriteBatch.End();
				}
			}
			Engine.Instance.GraphicsDevice.SetRenderTarget(null);
			Engine.Instance.GraphicsDevice.Clear(Color.Black);
			Engine.Instance.GraphicsDevice.Viewport = Engine.Viewport;
			Matrix gameMatrix = Matrix.CreateScale(6f) * Engine.ScreenMatrix;
			Vector2 fullsize = new Vector2(320f, 180f);
			Vector2 size = fullsize / ZoomTarget;
			Vector2 orig = ((ZoomTarget != 1f) ? ((ZoomFocusPoint - size / 2f) / (fullsize - size) * fullsize) : Vector2.Zero);
			MTexture lastColorTex = GFX.ColorGrades.GetOrDefault(lastColorGrade, GFX.ColorGrades["none"]);
			MTexture nextColorTex = GFX.ColorGrades.GetOrDefault(Session.ColorGrade, GFX.ColorGrades["none"]);
			if (colorGradeEase > 0f && lastColorTex != nextColorTex)
			{
				ColorGrade.Set(lastColorTex, nextColorTex, colorGradeEase);
			}
			else
			{
				ColorGrade.Set(nextColorTex);
			}
			float scale = Zoom * ((320f - ScreenPadding * 2f) / 320f);
			Vector2 padding = new Vector2(ScreenPadding, ScreenPadding * 0.5625f);
			if (SaveData.Instance.Assists.MirrorMode)
			{
				padding.X = 0f - padding.X;
				orig.X = 160f - (orig.X - 160f);
			}
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGrade.Effect, gameMatrix);
			Draw.SpriteBatch.Draw((RenderTarget2D)GameplayBuffers.Level, orig + padding, GameplayBuffers.Level.Bounds, Color.White, 0f, orig, scale, SaveData.Instance.Assists.MirrorMode ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
			Draw.SpriteBatch.End();
			if (Pathfinder != null && Pathfinder.DebugRenderEnabled)
			{
				Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Camera.Matrix * gameMatrix);
				Pathfinder.Render();
				Draw.SpriteBatch.End();
			}
			if (((!Paused || !PauseMainMenuOpen) && !(wasPausedTimer < 1f)) || !Input.MenuJournal.Check || !AllowHudHide)
			{
				HudRenderer.Render(this);
			}
			if (Wipe != null)
			{
				Wipe.Render(this);
			}
			if (HiresSnow != null)
			{
				HiresSnow.Render(this);
			}
		}

		public override void AfterRender()
		{
			base.AfterRender();
			Camera.Position = cameraPreShake;
		}

		private void StartPauseEffects()
		{
			if (Audio.CurrentMusic == "event:/music/lvl0/bridge")
			{
				Audio.PauseMusic = true;
			}
			Audio.PauseGameplaySfx = true;
			Audio.Play("event:/ui/game/pause");
			if (PauseSnapshot == null)
			{
				PauseSnapshot = Audio.CreateSnapshot("snapshot:/pause_menu");
			}
		}

		private void EndPauseEffects()
		{
			Audio.PauseMusic = false;
			Audio.PauseGameplaySfx = false;
			Audio.ReleaseSnapshot(PauseSnapshot);
			PauseSnapshot = null;
		}

		public void Pause(int startIndex = 0, bool minimal = false, bool quickReset = false)
		{
			wasPaused = true;
			Player player = base.Tracker.GetEntity<Player>();
			if (!Paused)
			{
				StartPauseEffects();
			}
			Paused = true;
			if (quickReset)
			{
				Audio.Play("event:/ui/main/message_confirm");
				PauseMainMenuOpen = false;
				GiveUp(0, restartArea: true, minimal, showHint: false);
				return;
			}
			PauseMainMenuOpen = true;
			TextMenu menu = new TextMenu();
			if (!minimal)
			{
				menu.Add(new TextMenu.Header(Dialog.Clean("menu_pause_title")));
			}
			menu.Add(new TextMenu.Button(Dialog.Clean("menu_pause_resume")).Pressed(delegate
			{
				menu.OnCancel();
			}));
			if (InCutscene && !SkippingCutscene)
			{
				menu.Add(new TextMenu.Button(Dialog.Clean("menu_pause_skip_cutscene")).Pressed(delegate
				{
					SkipCutscene();
					Paused = false;
					PauseMainMenuOpen = false;
					menu.RemoveSelf();
				}));
			}
			if (!minimal && !InCutscene && !SkippingCutscene)
			{
				TextMenu.Item retry;
				menu.Add(retry = new TextMenu.Button(Dialog.Clean("menu_pause_retry")).Pressed(delegate
				{
					if (player != null && !player.Dead)
					{
						Engine.TimeRate = 1f;
						Distort.GameRate = 1f;
						Distort.Anxiety = 0f;
						InCutscene = (SkippingCutscene = false);
						RetryPlayerCorpse = player.Die(Vector2.Zero, evenIfInvincible: true);
						foreach (LevelEndingHook levelEndingHook2 in base.Tracker.GetComponents<LevelEndingHook>())
						{
							if (levelEndingHook2.OnEnd != null)
							{
								levelEndingHook2.OnEnd();
							}
						}
					}
					Paused = false;
					PauseMainMenuOpen = false;
					EndPauseEffects();
					menu.RemoveSelf();
				}));
				retry.Disabled = !CanRetry || (player != null && !player.CanRetry) || Frozen || Completed;
			}
			if (!minimal && SaveData.Instance.AssistMode)
			{
				TextMenu.Item item6 = null;
				menu.Add(item6 = new TextMenu.Button(Dialog.Clean("menu_pause_assist")).Pressed(delegate
				{
					menu.RemoveSelf();
					PauseMainMenuOpen = false;
					AssistMode(menu.IndexOf(item6), minimal);
				}));
			}
			if (!minimal && SaveData.Instance.VariantMode)
			{
				TextMenu.Item item4 = null;
				menu.Add(item4 = new TextMenu.Button(Dialog.Clean("menu_pause_variant")).Pressed(delegate
				{
					menu.RemoveSelf();
					PauseMainMenuOpen = false;
					VariantMode(menu.IndexOf(item4), minimal);
				}));
			}
			TextMenu.Item item3 = null;
			menu.Add(item3 = new TextMenu.Button(Dialog.Clean("menu_pause_options")).Pressed(delegate
			{
				menu.RemoveSelf();
				PauseMainMenuOpen = false;
				Options(menu.IndexOf(item3), minimal);
			}));
			if (!minimal && Celeste.PlayMode != Celeste.PlayModes.Event)
			{
				TextMenu.Item item5 = null;
				menu.Add(item5 = new TextMenu.Button(Dialog.Clean("menu_pause_savequit")).Pressed(delegate
				{
					menu.Focused = false;
					Engine.TimeRate = 1f;
					Audio.SetMusic(null);
					Audio.BusStopAll("bus:/gameplay_sfx", immediate: true);
					Session.InArea = true;
					Session.Deaths++;
					Session.DeathsInCurrentLevel++;
					SaveData.Instance.AddDeath(Session.Area);
					DoScreenWipe(wipeIn: false, delegate
					{
						Engine.Scene = new LevelExit(LevelExit.Mode.SaveAndQuit, Session, HiresSnow);
					}, hiresSnow: true);
					foreach (LevelEndingHook levelEndingHook in base.Tracker.GetComponents<LevelEndingHook>())
					{
						if (levelEndingHook.OnEnd != null)
						{
							levelEndingHook.OnEnd();
						}
					}
				}));
				if (SaveQuitDisabled || (player != null && player.StateMachine.State == 18))
				{
					item5.Disabled = true;
				}
			}
			if (!minimal)
			{
				menu.Add(new TextMenu.SubHeader(""));
				TextMenu.Item item2 = null;
				menu.Add(item2 = new TextMenu.Button(Dialog.Clean("menu_pause_restartarea")).Pressed(delegate
				{
					PauseMainMenuOpen = false;
					menu.RemoveSelf();
					GiveUp(menu.IndexOf(item2), restartArea: true, minimal, showHint: true);
				}));
				(item2 as TextMenu.Button).ConfirmSfx = "event:/ui/main/message_confirm";
				if (SaveData.Instance.Areas[0].Modes[0].Completed || SaveData.Instance.DebugMode || SaveData.Instance.CheatMode)
				{
					TextMenu.Item item = null;
					menu.Add(item = new TextMenu.Button(Dialog.Clean("menu_pause_return")).Pressed(delegate
					{
						PauseMainMenuOpen = false;
						menu.RemoveSelf();
						GiveUp(menu.IndexOf(item), restartArea: false, minimal, showHint: false);
					}));
					(item as TextMenu.Button).ConfirmSfx = "event:/ui/main/message_confirm";
				}
				if (Celeste.PlayMode == Celeste.PlayModes.Event)
				{
					menu.Add(new TextMenu.Button(Dialog.Clean("menu_pause_restartdemo")).Pressed(delegate
					{
						EndPauseEffects();
						Audio.SetMusic(null);
						menu.Focused = false;
						DoScreenWipe(wipeIn: false, delegate
						{
							LevelEnter.Go(new Session(new AreaKey(0)), fromSaveData: false);
						});
					}));
				}
			}
			menu.OnESC = (menu.OnCancel = (menu.OnPause = delegate
			{
				PauseMainMenuOpen = false;
				menu.RemoveSelf();
				Paused = false;
				Audio.Play("event:/ui/game/unpause");
				unpauseTimer = 0.15f;
			}));
			if (startIndex > 0)
			{
				menu.Selection = startIndex;
			}
			Add(menu);
		}

		private void GiveUp(int returnIndex, bool restartArea, bool minimal, bool showHint)
		{
			Paused = true;
			QuickResetHint quickHint = null;
			ReturnMapHint returnHint = null;
			if (!restartArea)
			{
				Add(returnHint = new ReturnMapHint());
			}
			TextMenu menu = new TextMenu();
			menu.AutoScroll = false;
			menu.Position = new Vector2((float)Engine.Width / 2f, (float)Engine.Height / 2f - 100f);
			menu.Add(new TextMenu.Header(Dialog.Clean(restartArea ? "menu_restart_title" : "menu_return_title")));
			menu.Add(new TextMenu.Button(Dialog.Clean(restartArea ? "menu_restart_continue" : "menu_return_continue")).Pressed(delegate
			{
				Engine.TimeRate = 1f;
				menu.Focused = false;
				Session.InArea = false;
				Audio.SetMusic(null);
				Audio.BusStopAll("bus:/gameplay_sfx", immediate: true);
				if (restartArea)
				{
					DoScreenWipe(wipeIn: false, delegate
					{
						Engine.Scene = new LevelExit(LevelExit.Mode.Restart, Session);
					});
				}
				else
				{
					DoScreenWipe(wipeIn: false, delegate
					{
						Engine.Scene = new LevelExit(LevelExit.Mode.GiveUp, Session, HiresSnow);
					}, hiresSnow: true);
				}
				foreach (LevelEndingHook levelEndingHook in base.Tracker.GetComponents<LevelEndingHook>())
				{
					if (levelEndingHook.OnEnd != null)
					{
						levelEndingHook.OnEnd();
					}
				}
			}));
			menu.Add(new TextMenu.Button(Dialog.Clean(restartArea ? "menu_restart_cancel" : "menu_return_cancel")).Pressed(delegate
			{
				menu.OnCancel();
			}));
			menu.OnPause = (menu.OnESC = delegate
			{
				menu.RemoveSelf();
				if (quickHint != null)
				{
					quickHint.RemoveSelf();
				}
				if (returnHint != null)
				{
					returnHint.RemoveSelf();
				}
				Paused = false;
				unpauseTimer = 0.15f;
				Audio.Play("event:/ui/game/unpause");
			});
			menu.OnCancel = delegate
			{
				Audio.Play("event:/ui/main/button_back");
				menu.RemoveSelf();
				if (quickHint != null)
				{
					quickHint.RemoveSelf();
				}
				if (returnHint != null)
				{
					returnHint.RemoveSelf();
				}
				Pause(returnIndex, minimal);
			};
			Add(menu);
		}

		private void Options(int returnIndex, bool minimal)
		{
			Paused = true;
			bool oldAllowHudHide = AllowHudHide;
			AllowHudHide = false;
			TextMenu options = MenuOptions.Create(inGame: true, PauseSnapshot);
			options.OnESC = (options.OnCancel = delegate
			{
				Audio.Play("event:/ui/main/button_back");
				AllowHudHide = oldAllowHudHide;
				options.CloseAndRun(SaveFromOptions(), delegate
				{
					Pause(returnIndex, minimal);
				});
			});
			options.OnPause = delegate
			{
				Audio.Play("event:/ui/main/button_back");
				options.CloseAndRun(SaveFromOptions(), delegate
				{
					AllowHudHide = oldAllowHudHide;
					Paused = false;
					unpauseTimer = 0.15f;
				});
			};
			Add(options);
		}

		private IEnumerator SaveFromOptions()
		{
			UserIO.SaveHandler(file: false, settings: true);
			while (UserIO.Saving)
			{
				yield return null;
			}
		}

		private void AssistMode(int returnIndex, bool minimal)
		{
			Paused = true;
			TextMenu menu = new TextMenu();
			menu.Add(new TextMenu.Header(Dialog.Clean("MENU_ASSIST_TITLE")));
			menu.Add(new TextMenu.Slider(Dialog.Clean("MENU_ASSIST_GAMESPEED"), (int i) => i * 10 + "%", 5, 10, SaveData.Instance.Assists.GameSpeed).Change(delegate(int i)
			{
				SaveData.Instance.Assists.GameSpeed = i;
				Engine.TimeRateB = (float)SaveData.Instance.Assists.GameSpeed / 10f;
			}));
			menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_ASSIST_INFINITE_STAMINA"), SaveData.Instance.Assists.InfiniteStamina).Change(delegate(bool on)
			{
				SaveData.Instance.Assists.InfiniteStamina = on;
			}));
			TextMenu.Option<int> dashes;
			menu.Add(dashes = new TextMenu.Slider(Dialog.Clean("MENU_ASSIST_AIR_DASHES"), (int i) => i switch
			{
				0 => Dialog.Clean("MENU_ASSIST_AIR_DASHES_NORMAL"), 
				1 => Dialog.Clean("MENU_ASSIST_AIR_DASHES_TWO"), 
				_ => Dialog.Clean("MENU_ASSIST_AIR_DASHES_INFINITE"), 
			}, 0, 2, (int)SaveData.Instance.Assists.DashMode).Change(delegate(int on)
			{
				SaveData.Instance.Assists.DashMode = (Assists.DashModes)on;
				Player entity = base.Tracker.GetEntity<Player>();
				if (entity != null)
				{
					entity.Dashes = Math.Min(entity.Dashes, entity.MaxDashes);
				}
			}));
			if (Session.Area.ID == 0)
			{
				dashes.Disabled = true;
			}
			menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_ASSIST_DASH_ASSIST"), SaveData.Instance.Assists.DashAssist).Change(delegate(bool on)
			{
				SaveData.Instance.Assists.DashAssist = on;
			}));
			menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_ASSIST_INVINCIBLE"), SaveData.Instance.Assists.Invincible).Change(delegate(bool on)
			{
				SaveData.Instance.Assists.Invincible = on;
			}));
			menu.OnESC = (menu.OnCancel = delegate
			{
				Audio.Play("event:/ui/main/button_back");
				Pause(returnIndex, minimal);
				menu.Close();
			});
			menu.OnPause = delegate
			{
				Audio.Play("event:/ui/main/button_back");
				Paused = false;
				unpauseTimer = 0.15f;
				menu.Close();
			};
			Add(menu);
		}

		private void VariantMode(int returnIndex, bool minimal)
		{
			Paused = true;
			TextMenu menu = new TextMenu();
			menu.Add(new TextMenu.Header(Dialog.Clean("MENU_VARIANT_TITLE")));
			menu.Add(new TextMenu.SubHeader(Dialog.Clean("MENU_VARIANT_SUBTITLE")));
			TextMenu.Slider speed;
			menu.Add(speed = new TextMenu.Slider(Dialog.Clean("MENU_ASSIST_GAMESPEED"), (int i) => i * 10 + "%", 5, 16, SaveData.Instance.Assists.GameSpeed));
			speed.Change(delegate(int i)
			{
				if (i > 10)
				{
					i = ((speed.Values[speed.PreviousIndex].Item2 <= i) ? (i + 1) : (i - 1));
				}
				speed.Index = i - 5;
				SaveData.Instance.Assists.GameSpeed = i;
				Engine.TimeRateB = (float)SaveData.Instance.Assists.GameSpeed / 10f;
			});
			menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_VARIANT_MIRROR"), SaveData.Instance.Assists.MirrorMode).Change(delegate(bool on)
			{
				SaveData.Instance.Assists.MirrorMode = on;
				Input.MoveX.Inverted = (Input.Aim.InvertedX = (Input.Feather.InvertedX = on));
			}));
			menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_VARIANT_360DASHING"), SaveData.Instance.Assists.ThreeSixtyDashing).Change(delegate(bool on)
			{
				SaveData.Instance.Assists.ThreeSixtyDashing = on;
			}));
			menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_VARIANT_INVISMOTION"), SaveData.Instance.Assists.InvisibleMotion).Change(delegate(bool on)
			{
				SaveData.Instance.Assists.InvisibleMotion = on;
			}));
			menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_VARIANT_NOGRABBING"), SaveData.Instance.Assists.NoGrabbing).Change(delegate(bool on)
			{
				SaveData.Instance.Assists.NoGrabbing = on;
			}));
			menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_VARIANT_LOWFRICTION"), SaveData.Instance.Assists.LowFriction).Change(delegate(bool on)
			{
				SaveData.Instance.Assists.LowFriction = on;
			}));
			menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_VARIANT_SUPERDASHING"), SaveData.Instance.Assists.SuperDashing).Change(delegate(bool on)
			{
				SaveData.Instance.Assists.SuperDashing = on;
			}));
			menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_VARIANT_HICCUPS"), SaveData.Instance.Assists.Hiccups).Change(delegate(bool on)
			{
				SaveData.Instance.Assists.Hiccups = on;
			}));
			menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_VARIANT_PLAYASBADELINE"), SaveData.Instance.Assists.PlayAsBadeline).Change(delegate(bool on)
			{
				SaveData.Instance.Assists.PlayAsBadeline = on;
				Player entity2 = base.Tracker.GetEntity<Player>();
				if (entity2 != null)
				{
					PlayerSpriteMode mode = (SaveData.Instance.Assists.PlayAsBadeline ? PlayerSpriteMode.MadelineAsBadeline : entity2.DefaultSpriteMode);
					if (entity2.Active)
					{
						entity2.ResetSpriteNextFrame(mode);
					}
					else
					{
						entity2.ResetSprite(mode);
					}
				}
			}));
			menu.Add(new TextMenu.SubHeader(Dialog.Clean("MENU_ASSIST_SUBTITLE")));
			menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_ASSIST_INFINITE_STAMINA"), SaveData.Instance.Assists.InfiniteStamina).Change(delegate(bool on)
			{
				SaveData.Instance.Assists.InfiniteStamina = on;
			}));
			TextMenu.Option<int> dashes;
			menu.Add(dashes = new TextMenu.Slider(Dialog.Clean("MENU_ASSIST_AIR_DASHES"), (int i) => i switch
			{
				0 => Dialog.Clean("MENU_ASSIST_AIR_DASHES_NORMAL"), 
				1 => Dialog.Clean("MENU_ASSIST_AIR_DASHES_TWO"), 
				_ => Dialog.Clean("MENU_ASSIST_AIR_DASHES_INFINITE"), 
			}, 0, 2, (int)SaveData.Instance.Assists.DashMode).Change(delegate(int on)
			{
				SaveData.Instance.Assists.DashMode = (Assists.DashModes)on;
				Player entity = base.Tracker.GetEntity<Player>();
				if (entity != null)
				{
					entity.Dashes = Math.Min(entity.Dashes, entity.MaxDashes);
				}
			}));
			if (Session.Area.ID == 0)
			{
				dashes.Disabled = true;
			}
			menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_ASSIST_DASH_ASSIST"), SaveData.Instance.Assists.DashAssist).Change(delegate(bool on)
			{
				SaveData.Instance.Assists.DashAssist = on;
			}));
			menu.Add(new TextMenu.OnOff(Dialog.Clean("MENU_ASSIST_INVINCIBLE"), SaveData.Instance.Assists.Invincible).Change(delegate(bool on)
			{
				SaveData.Instance.Assists.Invincible = on;
			}));
			menu.OnESC = (menu.OnCancel = delegate
			{
				Audio.Play("event:/ui/main/button_back");
				Pause(returnIndex, minimal);
				menu.Close();
			});
			menu.OnPause = delegate
			{
				Audio.Play("event:/ui/main/button_back");
				Paused = false;
				unpauseTimer = 0.15f;
				menu.Close();
			};
			Add(menu);
		}

		public void SnapColorGrade(string next)
		{
			if (Session.ColorGrade != next)
			{
				lastColorGrade = next;
				colorGradeEase = 0f;
				colorGradeEaseSpeed = 1f;
				Session.ColorGrade = next;
			}
		}

		public void NextColorGrade(string next, float speed = 1f)
		{
			if (Session.ColorGrade != next)
			{
				colorGradeEase = 0f;
				colorGradeEaseSpeed = speed;
				Session.ColorGrade = next;
			}
		}

		public void Shake(float time = 0.3f)
		{
			if (Settings.Instance.ScreenShake != 0)
			{
				shakeDirection = Vector2.Zero;
				shakeTimer = Math.Max(shakeTimer, time);
			}
		}

		public void StopShake()
		{
			shakeTimer = 0f;
		}

		public void DirectionalShake(Vector2 dir, float time = 0.3f)
		{
			if (Settings.Instance.ScreenShake != 0)
			{
				shakeDirection = dir.SafeNormalize();
				lastDirectionalShake = 0;
				shakeTimer = Math.Max(shakeTimer, time);
			}
		}

		public void Flash(Color color, bool drawPlayerOver = false)
		{
			if (!Settings.Instance.DisableFlashes)
			{
				doFlash = true;
				flashDrawPlayer = drawPlayerOver;
				flash = 1f;
				flashColor = color;
			}
		}

		public void ZoomSnap(Vector2 screenSpaceFocusPoint, float zoom)
		{
			ZoomFocusPoint = screenSpaceFocusPoint;
			ZoomTarget = (Zoom = zoom);
		}

		public IEnumerator ZoomTo(Vector2 screenSpaceFocusPoint, float zoom, float duration)
		{
			ZoomFocusPoint = screenSpaceFocusPoint;
			ZoomTarget = zoom;
			float from = Zoom;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
			{
				Zoom = MathHelper.Lerp(from, ZoomTarget, Ease.SineInOut(MathHelper.Clamp(p, 0f, 1f)));
				yield return null;
			}
			Zoom = ZoomTarget;
		}

		public IEnumerator ZoomAcross(Vector2 screenSpaceFocusPoint, float zoom, float duration)
		{
			float fromZoom = Zoom;
			Vector2 fromFocus = ZoomFocusPoint;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
			{
				float t = Ease.SineInOut(MathHelper.Clamp(p, 0f, 1f));
				Zoom = (ZoomTarget = MathHelper.Lerp(fromZoom, zoom, t));
				ZoomFocusPoint = Vector2.Lerp(fromFocus, screenSpaceFocusPoint, t);
				yield return null;
			}
			Zoom = ZoomTarget;
			ZoomFocusPoint = screenSpaceFocusPoint;
		}

		public IEnumerator ZoomBack(float duration)
		{
			float from = Zoom;
			float to = 1f;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
			{
				Zoom = MathHelper.Lerp(from, to, Ease.SineInOut(MathHelper.Clamp(p, 0f, 1f)));
				yield return null;
			}
			ResetZoom();
		}

		public void ResetZoom()
		{
			Zoom = 1f;
			ZoomTarget = 1f;
			ZoomFocusPoint = new Vector2(320f, 180f) / 2f;
		}

		public void DoScreenWipe(bool wipeIn, Action onComplete = null, bool hiresSnow = false)
		{
			AreaData.Get(Session).DoScreenWipe(this, wipeIn, onComplete);
			if (hiresSnow)
			{
				Add(HiresSnow = new HiresSnow());
				HiresSnow.Alpha = 0f;
				HiresSnow.AttachAlphaTo = Wipe;
			}
		}

		public bool InsideCamera(Vector2 position, float expand = 0f)
		{
			if (position.X >= Camera.Left - expand && position.X < Camera.Right + expand && position.Y >= Camera.Top - expand)
			{
				return position.Y < Camera.Bottom + expand;
			}
			return false;
		}

		public void EnforceBounds(Player player)
		{
			Rectangle bounds = Bounds;
			Rectangle camera = new Rectangle((int)Camera.Left, (int)Camera.Top, 320, 180);
			if (transition != null)
			{
				return;
			}
			if (CameraLockMode == CameraLockModes.FinalBoss && player.Left < (float)camera.Left)
			{
				player.Left = camera.Left;
				player.OnBoundsH();
			}
			else if (player.Left < (float)bounds.Left)
			{
				if (player.Top >= (float)bounds.Top && player.Bottom < (float)bounds.Bottom && Session.MapData.CanTransitionTo(this, player.Center + Vector2.UnitX * -8f))
				{
					player.BeforeSideTransition();
					NextLevel(player.Center + Vector2.UnitX * -8f, -Vector2.UnitX);
					return;
				}
				player.Left = bounds.Left;
				player.OnBoundsH();
			}
			TheoCrystal theo = base.Tracker.GetEntity<TheoCrystal>();
			if (CameraLockMode == CameraLockModes.FinalBoss && player.Right > (float)camera.Right && camera.Right < bounds.Right - 4)
			{
				player.Right = camera.Right;
				player.OnBoundsH();
			}
			else if (theo != null && (player.Holding == null || !player.Holding.IsHeld) && player.Right > (float)(bounds.Right - 1))
			{
				player.Right = bounds.Right - 1;
			}
			else if (player.Right > (float)bounds.Right)
			{
				if (player.Top >= (float)bounds.Top && player.Bottom < (float)bounds.Bottom && Session.MapData.CanTransitionTo(this, player.Center + Vector2.UnitX * 8f))
				{
					player.BeforeSideTransition();
					NextLevel(player.Center + Vector2.UnitX * 8f, Vector2.UnitX);
					return;
				}
				player.Right = bounds.Right;
				player.OnBoundsH();
			}
			if (CameraLockMode != 0 && player.Top < (float)camera.Top)
			{
				player.Top = camera.Top;
				player.OnBoundsV();
			}
			else if (player.CenterY < (float)bounds.Top)
			{
				if (Session.MapData.CanTransitionTo(this, player.Center - Vector2.UnitY * 12f))
				{
					player.BeforeUpTransition();
					NextLevel(player.Center - Vector2.UnitY * 12f, -Vector2.UnitY);
					return;
				}
				if (player.Top < (float)(bounds.Top - 24))
				{
					player.Top = bounds.Top - 24;
					player.OnBoundsV();
				}
			}
			if (CameraLockMode != 0 && camera.Bottom < bounds.Bottom - 4 && player.Top > (float)camera.Bottom)
			{
				if (SaveData.Instance.Assists.Invincible)
				{
					player.Play("event:/game/general/assist_screenbottom");
					player.Bounce(camera.Bottom);
				}
				else
				{
					player.Die(Vector2.Zero);
				}
			}
			else if (player.Bottom > (float)bounds.Bottom && Session.MapData.CanTransitionTo(this, player.Center + Vector2.UnitY * 12f) && !Session.LevelData.DisableDownTransition)
			{
				if (!player.CollideCheck<Solid>(player.Position + Vector2.UnitY * 4f))
				{
					player.BeforeDownTransition();
					NextLevel(player.Center + Vector2.UnitY * 12f, Vector2.UnitY);
				}
			}
			else if (player.Top > (float)bounds.Bottom && SaveData.Instance.Assists.Invincible)
			{
				player.Play("event:/game/general/assist_screenbottom");
				player.Bounce(bounds.Bottom);
			}
			else if (player.Top > (float)(bounds.Bottom + 4))
			{
				player.Die(Vector2.Zero);
			}
		}

		public bool IsInBounds(Entity entity)
		{
			Rectangle bounds = Bounds;
			if (entity.Right > (float)bounds.Left && entity.Bottom > (float)bounds.Top && entity.Left < (float)bounds.Right)
			{
				return entity.Top < (float)bounds.Bottom;
			}
			return false;
		}

		public bool IsInBounds(Vector2 position)
		{
			Rectangle bounds = Bounds;
			if (position.X >= (float)bounds.Left && position.Y >= (float)bounds.Top && position.X < (float)bounds.Right)
			{
				return position.Y < (float)bounds.Bottom;
			}
			return false;
		}

		public bool IsInBounds(Vector2 position, float pad)
		{
			Rectangle bounds = Bounds;
			if (position.X >= (float)bounds.Left - pad && position.Y >= (float)bounds.Top - pad && position.X < (float)bounds.Right + pad)
			{
				return position.Y < (float)bounds.Bottom + pad;
			}
			return false;
		}

		public bool IsInBounds(Vector2 position, Vector2 dirPad)
		{
			float padLeft = Math.Max(dirPad.X, 0f);
			float padRight = Math.Max(0f - dirPad.X, 0f);
			float padTop = Math.Max(dirPad.Y, 0f);
			float padBottom = Math.Max(0f - dirPad.Y, 0f);
			Rectangle bounds = Bounds;
			if (position.X >= (float)bounds.Left + padLeft && position.Y >= (float)bounds.Top + padTop && position.X < (float)bounds.Right - padRight)
			{
				return position.Y < (float)bounds.Bottom - padBottom;
			}
			return false;
		}

		public bool IsInCamera(Vector2 position, float pad)
		{
			Rectangle bounds = new Rectangle((int)Camera.X, (int)Camera.Y, 320, 180);
			if (position.X >= (float)bounds.Left - pad && position.Y >= (float)bounds.Top - pad && position.X < (float)bounds.Right + pad)
			{
				return position.Y < (float)bounds.Bottom + pad;
			}
			return false;
		}

		public void StartCutscene(Action<Level> onSkip, bool fadeInOnSkip = true, bool endingChapterAfterCutscene = false, bool resetZoomOnSkip = true)
		{
			this.endingChapterAfterCutscene = endingChapterAfterCutscene;
			InCutscene = true;
			onCutsceneSkip = onSkip;
			onCutsceneSkipFadeIn = fadeInOnSkip;
			onCutsceneSkipResetZoom = resetZoomOnSkip;
		}

		public void CancelCutscene()
		{
			InCutscene = false;
			SkippingCutscene = false;
		}

		public void SkipCutscene()
		{
			SkippingCutscene = true;
			Engine.TimeRate = 1f;
			Distort.Anxiety = 0f;
			Distort.GameRate = 1f;
			if (endingChapterAfterCutscene)
			{
				Audio.BusStopAll("bus:/gameplay_sfx", immediate: true);
			}
			List<Entity> textboxes = new List<Entity>();
			foreach (Entity textbox in base.Tracker.GetEntities<Textbox>())
			{
				textboxes.Add(textbox);
			}
			foreach (Entity item in textboxes)
			{
				item.RemoveSelf();
			}
			skipCoroutine = new Coroutine(SkipCutsceneRoutine());
		}

		private IEnumerator SkipCutsceneRoutine()
		{
			FadeWipe wipeIn = new FadeWipe(this, wipeIn: false);
			wipeIn.Duration = 0.25f;
			yield return wipeIn.Wait();
			onCutsceneSkip(this);
			strawberriesDisplay.DrawLerp = 0f;
			if (onCutsceneSkipResetZoom)
			{
				ResetZoom();
			}
			GameplayStats gameplayStates = base.Entities.FindFirst<GameplayStats>();
			if (gameplayStates != null)
			{
				gameplayStates.DrawLerp = 0f;
			}
			if (onCutsceneSkipFadeIn)
			{
				FadeWipe wipeOut = new FadeWipe(this, wipeIn: true);
				wipeOut.Duration = 0.25f;
				base.RendererList.UpdateLists();
				yield return wipeOut.Wait();
			}
			SkippingCutscene = false;
			EndCutscene();
		}

		public void EndCutscene()
		{
			if (!SkippingCutscene)
			{
				InCutscene = false;
			}
		}

		private void NextLevel(Vector2 at, Vector2 dir)
		{
			base.OnEndOfFrame += delegate
			{
				Engine.TimeRate = 1f;
				Distort.Anxiety = 0f;
				Distort.GameRate = 1f;
				TransitionTo(Session.MapData.GetAt(at), dir);
			};
		}

		public void RegisterAreaComplete()
		{
			if (Completed)
			{
				return;
			}
			Player player = base.Tracker.GetEntity<Player>();
			if (player != null)
			{
				List<Strawberry> strawbs = new List<Strawberry>();
				foreach (Follower follower in player.Leader.Followers)
				{
					if (follower.Entity is Strawberry)
					{
						strawbs.Add(follower.Entity as Strawberry);
					}
				}
				foreach (Strawberry item in strawbs)
				{
					item.OnCollect();
				}
			}
			Completed = true;
			SaveData.Instance.RegisterCompletion(Session);
		}

		public ScreenWipe CompleteArea(bool spotlightWipe = true, bool skipScreenWipe = false, bool skipCompleteScreen = false)
		{
			RegisterAreaComplete();
			PauseLock = true;
			Action onComplete = ((!(AreaData.Get(Session).Interlude || skipCompleteScreen)) ? ((Action)delegate
			{
				Engine.Scene = new LevelExit(LevelExit.Mode.Completed, Session);
			}) : ((Action)delegate
			{
				Engine.Scene = new LevelExit(LevelExit.Mode.CompletedInterlude, Session, HiresSnow);
			}));
			if (!SkippingCutscene && !skipScreenWipe)
			{
				if (spotlightWipe)
				{
					Player player = base.Tracker.GetEntity<Player>();
					if (player != null)
					{
						SpotlightWipe.FocusPoint = player.Position - Camera.Position - new Vector2(0f, 8f);
					}
					return new SpotlightWipe(this, wipeIn: false, onComplete);
				}
				return new FadeWipe(this, wipeIn: false, onComplete);
			}
			Audio.BusStopAll("bus:/gameplay_sfx", immediate: true);
			onComplete();
			return null;
		}
	}
}
