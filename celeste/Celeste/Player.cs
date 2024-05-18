using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class Player : Actor
	{
		public enum IntroTypes
		{
			Transition,
			Respawn,
			WalkInRight,
			WalkInLeft,
			Jump,
			WakeUp,
			Fall,
			TempleMirrorVoid,
			None,
			ThinkForABit
		}

		public struct ChaserStateSound
		{
			public enum Actions
			{
				Oneshot,
				Loop,
				Stop
			}

			public string Event;

			public string Parameter;

			public float ParameterValue;

			public Actions Action;
		}

		public struct ChaserState
		{
			public Vector2 Position;

			public float TimeStamp;

			public string Animation;

			public Facings Facing;

			public bool OnGround;

			public Color HairColor;

			public int Depth;

			public Vector2 Scale;

			public Vector2 DashDirection;

			private ChaserStateSound sound0;

			private ChaserStateSound sound1;

			private ChaserStateSound sound2;

			private ChaserStateSound sound3;

			private ChaserStateSound sound4;

			public int Sounds;

			public ChaserStateSound this[int index] => index switch
			{
				0 => sound0, 
				1 => sound1, 
				2 => sound2, 
				3 => sound3, 
				4 => sound4, 
				_ => default(ChaserStateSound), 
			};

			public ChaserState(Player player)
			{
				Position = player.Position;
				TimeStamp = player.Scene.TimeActive;
				Animation = player.Sprite.CurrentAnimationID;
				Facing = player.Facing;
				OnGround = player.onGround;
				HairColor = player.Hair.Color;
				Depth = player.Depth;
				Scale = new Vector2(Math.Abs(player.Sprite.Scale.X) * (float)player.Facing, player.Sprite.Scale.Y);
				DashDirection = player.DashDir;
				List<ChaserStateSound> sounds = player.activeSounds;
				Sounds = Math.Min(5, sounds.Count);
				sound0 = ((Sounds > 0) ? sounds[0] : default(ChaserStateSound));
				sound1 = ((Sounds > 1) ? sounds[1] : default(ChaserStateSound));
				sound2 = ((Sounds > 2) ? sounds[2] : default(ChaserStateSound));
				sound3 = ((Sounds > 3) ? sounds[3] : default(ChaserStateSound));
				sound4 = ((Sounds > 4) ? sounds[4] : default(ChaserStateSound));
			}
		}

		public static ParticleType P_DashA;

		public static ParticleType P_DashB;

		public static ParticleType P_DashBadB;

		public static ParticleType P_CassetteFly;

		public static ParticleType P_Split;

		public static ParticleType P_SummitLandA;

		public static ParticleType P_SummitLandB;

		public static ParticleType P_SummitLandC;

		public const float MaxFall = 160f;

		private const float Gravity = 900f;

		private const float HalfGravThreshold = 40f;

		private const float FastMaxFall = 240f;

		private const float FastMaxAccel = 300f;

		public const float MaxRun = 90f;

		public const float RunAccel = 1000f;

		private const float RunReduce = 400f;

		private const float AirMult = 0.65f;

		private const float HoldingMaxRun = 70f;

		private const float HoldMinTime = 0.35f;

		private const float BounceAutoJumpTime = 0.1f;

		private const float DuckFriction = 500f;

		private const int DuckCorrectCheck = 4;

		private const float DuckCorrectSlide = 50f;

		private const float DodgeSlideSpeedMult = 1.2f;

		private const float DuckSuperJumpXMult = 1.25f;

		private const float DuckSuperJumpYMult = 0.5f;

		private const float JumpGraceTime = 0.1f;

		private const float JumpSpeed = -105f;

		private const float JumpHBoost = 40f;

		private const float VarJumpTime = 0.2f;

		private const float CeilingVarJumpGrace = 0.05f;

		private const int UpwardCornerCorrection = 4;

		private const int DashingUpwardCornerCorrection = 5;

		private const float WallSpeedRetentionTime = 0.06f;

		private const int WallJumpCheckDist = 3;

		private const int SuperWallJumpCheckDist = 5;

		private const float WallJumpForceTime = 0.16f;

		private const float WallJumpHSpeed = 130f;

		public const float WallSlideStartMax = 20f;

		private const float WallSlideTime = 1.2f;

		private const float BounceVarJumpTime = 0.2f;

		private const float BounceSpeed = -140f;

		private const float SuperBounceVarJumpTime = 0.2f;

		private const float SuperBounceSpeed = -185f;

		private const float SuperJumpSpeed = -105f;

		private const float SuperJumpH = 260f;

		private const float SuperWallJumpSpeed = -160f;

		private const float SuperWallJumpVarTime = 0.25f;

		private const float SuperWallJumpForceTime = 0.2f;

		private const float SuperWallJumpH = 170f;

		private const float DashSpeed = 240f;

		private const float EndDashSpeed = 160f;

		private const float EndDashUpMult = 0.75f;

		private const float DashTime = 0.15f;

		private const float SuperDashTime = 0.3f;

		private const float DashCooldown = 0.2f;

		private const float DashRefillCooldown = 0.1f;

		private const int DashHJumpThruNudge = 6;

		private const int DashCornerCorrection = 4;

		private const int DashVFloorSnapDist = 3;

		private const float DashAttackTime = 0.3f;

		private const float BoostMoveSpeed = 80f;

		public const float BoostTime = 0.25f;

		private const float DuckWindMult = 0f;

		private const int WindWallDistance = 3;

		private const float ReboundSpeedX = 120f;

		private const float ReboundSpeedY = -120f;

		private const float ReboundVarJumpTime = 0.15f;

		private const float ReflectBoundSpeed = 220f;

		private const float DreamDashSpeed = 240f;

		private const int DreamDashEndWiggle = 5;

		private const float DreamDashMinTime = 0.1f;

		public const float ClimbMaxStamina = 110f;

		private const float ClimbUpCost = 45.454544f;

		private const float ClimbStillCost = 10f;

		private const float ClimbJumpCost = 27.5f;

		private const int ClimbCheckDist = 2;

		private const int ClimbUpCheckDist = 2;

		private const float ClimbNoMoveTime = 0.1f;

		public const float ClimbTiredThreshold = 20f;

		private const float ClimbUpSpeed = -45f;

		private const float ClimbDownSpeed = 80f;

		private const float ClimbSlipSpeed = 30f;

		private const float ClimbAccel = 900f;

		private const float ClimbGrabYMult = 0.2f;

		private const float ClimbHopY = -120f;

		private const float ClimbHopX = 100f;

		private const float ClimbHopForceTime = 0.2f;

		private const float ClimbJumpBoostTime = 0.2f;

		private const float ClimbHopNoWindTime = 0.3f;

		private const float LaunchSpeed = 280f;

		private const float LaunchCancelThreshold = 220f;

		private const float LiftYCap = -130f;

		private const float LiftXCap = 250f;

		private const float JumpThruAssistSpeed = -40f;

		private const float FlyPowerFlashTime = 0.5f;

		private const float ThrowRecoil = 80f;

		private static readonly Vector2 CarryOffsetTarget = new Vector2(0f, -12f);

		private const float ChaserStateMaxTime = 4f;

		public const float WalkSpeed = 64f;

		private const float LowFrictionMult = 0.35f;

		private const float LowFrictionAirMult = 0.5f;

		private const float LowFrictionStopTime = 0.15f;

		private const float HiccupTimeMin = 1.2f;

		private const float HiccupTimeMax = 1.8f;

		private const float HiccupDuckMult = 0.5f;

		private const float HiccupAirBoost = -60f;

		private const float HiccupAirVarTime = 0.15f;

		private const float GliderMaxFall = 40f;

		private const float GliderWindMaxFall = 0f;

		private const float GliderWindUpFall = -32f;

		public const float GliderFastFall = 120f;

		private const float GliderSlowFall = 24f;

		private const float GliderGravMult = 0.5f;

		private const float GliderMaxRun = 108.00001f;

		private const float GliderRunMult = 0.5f;

		private const float GliderUpMinPickupSpeed = -105f;

		private const float GliderDashMinPickupSpeed = -240f;

		private const float GliderWallJumpForceTime = 0.26f;

		private const float DashGliderBoostTime = 0.55f;

		public const int StNormal = 0;

		public const int StClimb = 1;

		public const int StDash = 2;

		public const int StSwim = 3;

		public const int StBoost = 4;

		public const int StRedDash = 5;

		public const int StHitSquash = 6;

		public const int StLaunch = 7;

		public const int StPickup = 8;

		public const int StDreamDash = 9;

		public const int StSummitLaunch = 10;

		public const int StDummy = 11;

		public const int StIntroWalk = 12;

		public const int StIntroJump = 13;

		public const int StIntroRespawn = 14;

		public const int StIntroWakeUp = 15;

		public const int StBirdDashTutorial = 16;

		public const int StFrozen = 17;

		public const int StReflectionFall = 18;

		public const int StStarFly = 19;

		public const int StTempleFall = 20;

		public const int StCassetteFly = 21;

		public const int StAttract = 22;

		public const int StIntroMoonJump = 23;

		public const int StFlingBird = 24;

		public const int StIntroThinkForABit = 25;

		public const string TalkSfx = "player_talk";

		public Vector2 Speed;

		public Facings Facing;

		public PlayerSprite Sprite;

		public PlayerHair Hair;

		public StateMachine StateMachine;

		public Vector2 CameraAnchor;

		public bool CameraAnchorIgnoreX;

		public bool CameraAnchorIgnoreY;

		public Vector2 CameraAnchorLerp;

		public bool ForceCameraUpdate;

		public Leader Leader;

		public VertexLight Light;

		public int Dashes;

		public float Stamina = 110f;

		public bool StrawberriesBlocked;

		public Vector2 PreviousPosition;

		public bool DummyAutoAnimate = true;

		public Vector2 ForceStrongWindHair;

		public Vector2? OverrideDashDirection;

		public bool FlipInReflection;

		public bool JustRespawned;

		public bool EnforceLevelBounds = true;

		private Level level;

		private Collision onCollideH;

		private Collision onCollideV;

		private bool onGround;

		private bool wasOnGround;

		private int moveX;

		private bool flash;

		private bool wasDucking;

		private int climbTriggerDir;

		private bool holdCannotDuck;

		private bool windMovedUp;

		private float idleTimer;

		private static Chooser<string> idleColdOptions = new Chooser<string>().Add("idleA", 5f).Add("idleB", 3f).Add("idleC", 1f);

		private static Chooser<string> idleNoBackpackOptions = new Chooser<string>().Add("idleA", 1f).Add("idleB", 3f).Add("idleC", 3f);

		private static Chooser<string> idleWarmOptions = new Chooser<string>().Add("idleA", 5f).Add("idleB", 3f);

		public int StrawberryCollectIndex;

		public float StrawberryCollectResetTimer;

		private Hitbox hurtbox;

		private float jumpGraceTimer;

		public bool AutoJump;

		public float AutoJumpTimer;

		private float varJumpSpeed;

		private float varJumpTimer;

		private int forceMoveX;

		private float forceMoveXTimer;

		private int hopWaitX;

		private float hopWaitXSpeed;

		private Vector2 lastAim;

		private float dashCooldownTimer;

		private float dashRefillCooldownTimer;

		public Vector2 DashDir;

		private float wallSlideTimer = 1.2f;

		private int wallSlideDir;

		private float climbNoMoveTimer;

		private Vector2 carryOffset;

		private Vector2 deadOffset;

		private float introEase;

		private float wallSpeedRetentionTimer;

		private float wallSpeedRetained;

		private int wallBoostDir;

		private float wallBoostTimer;

		private float maxFall;

		private float dashAttackTimer;

		private float gliderBoostTimer;

		public List<ChaserState> ChaserStates;

		private bool wasTired;

		private HashSet<Trigger> triggersInside;

		private float highestAirY;

		private bool dashStartedOnGround;

		private bool fastJump;

		private int lastClimbMove;

		private float noWindTimer;

		private float dreamDashCanEndTimer;

		private Solid climbHopSolid;

		private Vector2 climbHopSolidPosition;

		private SoundSource wallSlideSfx;

		private SoundSource swimSurfaceLoopSfx;

		private float playFootstepOnLand;

		private float minHoldTimer;

		public Booster CurrentBooster;

		public Booster LastBooster;

		private bool calledDashEvents;

		private int lastDashes;

		private Sprite sweatSprite;

		private int startHairCount;

		private bool launched;

		private float launchedTimer;

		private float dashTrailTimer;

		private int dashTrailCounter;

		private bool canCurveDash;

		private float lowFrictionStopTimer;

		private float hiccupTimer;

		private List<ChaserStateSound> activeSounds = new List<ChaserStateSound>();

		private EventInstance? idleSfx;

		public bool MuffleLanding;

		private Vector2 gliderBoostDir;

		private float explodeLaunchBoostTimer;

		private float explodeLaunchBoostSpeed;

		private bool demoDashed;

		private readonly Hitbox normalHitbox = new Hitbox(8f, 11f, -4f, -11f);

		private readonly Hitbox duckHitbox = new Hitbox(8f, 6f, -4f, -6f);

		private readonly Hitbox normalHurtbox = new Hitbox(8f, 9f, -4f, -11f);

		private readonly Hitbox duckHurtbox = new Hitbox(8f, 4f, -4f, -6f);

		private readonly Hitbox starFlyHitbox = new Hitbox(8f, 8f, -4f, -10f);

		private readonly Hitbox starFlyHurtbox = new Hitbox(6f, 6f, -3f, -9f);

		private Vector2 normalLightOffset = new Vector2(0f, -8f);

		private Vector2 duckingLightOffset = new Vector2(0f, -3f);

		private List<Entity> temp = new List<Entity>();

		public static readonly Color NormalHairColor = Calc.HexToColor("AC3232");

		public static readonly Color FlyPowerHairColor = Calc.HexToColor("F2EB6D");

		public static readonly Color UsedHairColor = Calc.HexToColor("44B7FF");

		public static readonly Color FlashHairColor = Color.White;

		public static readonly Color TwoDashesHairColor = Calc.HexToColor("ff6def");

		public static readonly Color NormalBadelineHairColor = BadelineOldsite.HairColor;

		public static readonly Color UsedBadelineHairColor = UsedHairColor;

		public static readonly Color TwoDashesBadelineHairColor = TwoDashesHairColor;

		private float hairFlashTimer;

		private bool startHairCalled;

		public Color? OverrideHairColor;

		private Vector2 windDirection;

		private float windTimeout;

		private float windHairTimer;

		public IntroTypes IntroType;

		private MirrorReflection reflection;

		public PlayerSpriteMode DefaultSpriteMode;

		private PlayerSpriteMode? nextSpriteMode;

		private const float LaunchedBoostCheckSpeedSq = 10000f;

		private const float LaunchedJumpCheckSpeedSq = 48400f;

		private const float LaunchedMinSpeedSq = 19600f;

		private const float LaunchedDoubleSpeedSq = 22500f;

		private const float SideBounceSpeed = 240f;

		private const float SideBounceThreshold = 240f;

		private const float SideBounceForceMoveXTime = 0.3f;

		private const float SpacePhysicsMult = 0.6f;

		private EventInstance? conveyorLoopSfx;

		private const float WallBoosterSpeed = -160f;

		private const float WallBoosterLiftSpeed = -80f;

		private const float WallBoosterAccel = 600f;

		private const float WallBoostingHopHSpeed = 100f;

		private const float WallBoosterOverTopSpeed = -180f;

		private const float IceBoosterSpeed = 40f;

		private const float IceBoosterAccel = 300f;

		private bool wallBoosting;

		private Vector2 beforeDashSpeed;

		private bool wasDashB;

		private const float SwimYSpeedMult = 0.5f;

		private const float SwimMaxRise = -60f;

		private const float SwimVDeccel = 600f;

		private const float SwimMax = 80f;

		private const float SwimUnderwaterMax = 60f;

		private const float SwimAccel = 600f;

		private const float SwimReduce = 400f;

		private const float SwimDashSpeedMult = 0.75f;

		private Vector2 boostTarget;

		private bool boostRed;

		private const float HitSquashNoMoveTime = 0.1f;

		private const float HitSquashFriction = 800f;

		private float hitSquashNoMoveTimer;

		private float? launchApproachX;

		private float summitLaunchTargetX;

		private float summitLaunchParticleTimer;

		private DreamBlock dreamBlock;

		private SoundSource dreamSfxLoop;

		private bool dreamJump;

		private const float StarFlyTransformDeccel = 1000f;

		private const float StarFlyTime = 2f;

		private const float StarFlyStartSpeed = 250f;

		private const float StarFlyTargetSpeed = 140f;

		private const float StarFlyMaxSpeed = 190f;

		private const float StarFlyMaxLerpTime = 1f;

		private const float StarFlySlowSpeed = 91f;

		private const float StarFlyAccel = 1000f;

		private const float StarFlyRotateSpeed = 5.5850534f;

		private const float StarFlyEndX = 160f;

		private const float StarFlyEndXVarJumpTime = 0.1f;

		private const float StarFlyEndFlashDuration = 0.5f;

		private const float StarFlyEndNoBounceTime = 0.2f;

		private const float StarFlyWallBounce = -0.5f;

		private const float StarFlyMaxExitY = 0f;

		private const float StarFlyMaxExitX = 140f;

		private const float StarFlyExitUp = -100f;

		private Color starFlyColor = Calc.HexToColor("ffd65c");

		private BloomPoint starFlyBloom;

		private float starFlyTimer;

		private bool starFlyTransforming;

		private float starFlySpeedLerp;

		private Vector2 starFlyLastDir;

		private SoundSource starFlyLoopSfx;

		private SoundSource starFlyWarningSfx;

		private FlingBird flingBird;

		private SimpleCurve cassetteFlyCurve;

		private float cassetteFlyLerp;

		private Vector2 attractTo;

		public bool DummyMoving;

		public bool DummyGravity = true;

		public bool DummyFriction = true;

		public bool DummyMaxspeed = true;

		private Facings IntroWalkDirection;

		private Tween respawnTween;

		public bool Dead { get; private set; }

		public Vector2 CameraTarget
		{
			get
			{
				Vector2 at = default(Vector2);
				Vector2 target = new Vector2(base.X - 160f, base.Y - 90f);
				if (StateMachine.State != 18)
				{
					target += new Vector2(level.CameraOffset.X, level.CameraOffset.Y);
				}
				if (StateMachine.State == 19)
				{
					target.X += 0.2f * Speed.X;
					target.Y += 0.2f * Speed.Y;
				}
				else if (StateMachine.State == 5)
				{
					target.X += 48 * Math.Sign(Speed.X);
					target.Y += 48 * Math.Sign(Speed.Y);
				}
				else if (StateMachine.State == 10)
				{
					target.Y -= 64f;
				}
				else if (StateMachine.State == 18)
				{
					target.Y += 32f;
				}
				if (CameraAnchorLerp.Length() > 0f)
				{
					if (CameraAnchorIgnoreX && !CameraAnchorIgnoreY)
					{
						target.Y = MathHelper.Lerp(target.Y, CameraAnchor.Y, CameraAnchorLerp.Y);
					}
					else if (!CameraAnchorIgnoreX && CameraAnchorIgnoreY)
					{
						target.X = MathHelper.Lerp(target.X, CameraAnchor.X, CameraAnchorLerp.X);
					}
					else if (CameraAnchorLerp.X == CameraAnchorLerp.Y)
					{
						target = Vector2.Lerp(target, CameraAnchor, CameraAnchorLerp.X);
					}
					else
					{
						target.X = MathHelper.Lerp(target.X, CameraAnchor.X, CameraAnchorLerp.X);
						target.Y = MathHelper.Lerp(target.Y, CameraAnchor.Y, CameraAnchorLerp.Y);
					}
				}
				if (EnforceLevelBounds)
				{
					at.X = MathHelper.Clamp(target.X, level.Bounds.Left, level.Bounds.Right - 320);
					at.Y = MathHelper.Clamp(target.Y, level.Bounds.Top, level.Bounds.Bottom - 180);
				}
				else
				{
					at = target;
				}
				if (level.CameraLockMode != 0)
				{
					CameraLocker locker = base.Scene.Tracker.GetComponent<CameraLocker>();
					if (level.CameraLockMode != Level.CameraLockModes.BoostSequence)
					{
						at.X = Math.Max(at.X, level.Camera.X);
						if (locker != null)
						{
							at.X = Math.Min(at.X, Math.Max(level.Bounds.Left, locker.Entity.X - locker.MaxXOffset));
						}
					}
					if (level.CameraLockMode == Level.CameraLockModes.FinalBoss)
					{
						at.Y = Math.Max(at.Y, level.Camera.Y);
						if (locker != null)
						{
							at.Y = Math.Min(at.Y, Math.Max(level.Bounds.Top, locker.Entity.Y - locker.MaxYOffset));
						}
					}
					else if (level.CameraLockMode == Level.CameraLockModes.BoostSequence)
					{
						level.CameraUpwardMaxY = Math.Min(level.Camera.Y + 180f, level.CameraUpwardMaxY);
						at.Y = Math.Min(at.Y, level.CameraUpwardMaxY);
						if (locker != null)
						{
							at.Y = Math.Max(at.Y, Math.Min(level.Bounds.Bottom - 180, locker.Entity.Y - locker.MaxYOffset));
						}
					}
				}
				foreach (Entity box in base.Scene.Tracker.GetEntities<Killbox>())
				{
					if (box.Collidable && base.Top < box.Bottom && base.Right > box.Left && base.Left < box.Right)
					{
						at.Y = Math.Min(at.Y, box.Top - 180f);
					}
				}
				return at;
			}
		}

		public bool CanRetry
		{
			get
			{
				int state = StateMachine.State;
				if ((uint)(state - 12) > 3u && state != 18 && state != 25)
				{
					return true;
				}
				return false;
			}
		}

		public bool TimePaused
		{
			get
			{
				if (Dead)
				{
					return true;
				}
				int state = StateMachine.State;
				if (state != 10 && (uint)(state - 12) > 3u && state != 25)
				{
					return false;
				}
				return true;
			}
		}

		public bool InControl
		{
			get
			{
				switch (StateMachine.State)
				{
				default:
					return true;
				case 11:
				case 12:
				case 13:
				case 14:
				case 15:
				case 16:
				case 17:
				case 23:
				case 25:
					return false;
				}
			}
		}

		public PlayerInventory Inventory
		{
			get
			{
				if (level != null && level.Session != null)
				{
					return level.Session.Inventory;
				}
				return PlayerInventory.Default;
			}
		}

		public bool OnSafeGround { get; private set; }

		public bool LoseShards => onGround;

		public int MaxDashes
		{
			get
			{
				if (SaveData.Instance.Assists.DashMode != 0 && !level.InCutscene)
				{
					return 2;
				}
				return Inventory.Dashes;
			}
		}

		private Vector2 LiftBoost
		{
			get
			{
				Vector2 val = base.LiftSpeed;
				if (Math.Abs(val.X) > 250f)
				{
					val.X = 250f * (float)Math.Sign(val.X);
				}
				if (val.Y > 0f)
				{
					val.Y = 0f;
				}
				else if (val.Y < -130f)
				{
					val.Y = -130f;
				}
				return val;
			}
		}

		public bool Ducking
		{
			get
			{
				if (base.Collider != duckHitbox)
				{
					return base.Collider == duckHurtbox;
				}
				return true;
			}
			set
			{
				if (value)
				{
					base.Collider = duckHitbox;
					hurtbox = duckHurtbox;
				}
				else
				{
					base.Collider = normalHitbox;
					hurtbox = normalHurtbox;
				}
			}
		}

		public bool CanUnDuck
		{
			get
			{
				if (!Ducking)
				{
					return true;
				}
				Collider was = base.Collider;
				base.Collider = normalHitbox;
				bool result = !CollideCheck<Solid>();
				base.Collider = was;
				return result;
			}
		}

		public Holdable Holding { get; set; }

		private bool IsTired => CheckStamina < 20f;

		private float CheckStamina
		{
			get
			{
				if (wallBoostTimer > 0f)
				{
					return Stamina + 27.5f;
				}
				return Stamina;
			}
		}

		public bool DashAttacking
		{
			get
			{
				if (!(dashAttackTimer > 0f))
				{
					return StateMachine.State == 5;
				}
				return true;
			}
		}

		public bool CanDash
		{
			get
			{
				if ((Input.CrouchDashPressed || Input.DashPressed) && dashCooldownTimer <= 0f && Dashes > 0 && (TalkComponent.PlayerOver == null || !Input.Talk.Pressed))
				{
					if (LastBooster != null && LastBooster.Ch9HubTransition)
					{
						return !LastBooster.BoostingPlayer;
					}
					return true;
				}
				return false;
			}
		}

		public bool StartedDashing { get; private set; }

		private bool SuperWallJumpAngleCheck
		{
			get
			{
				if (Math.Abs(DashDir.X) <= 0.2f)
				{
					return DashDir.Y <= -0.75f;
				}
				return false;
			}
		}

		public bool AtAttractTarget
		{
			get
			{
				if (StateMachine.State == 22)
				{
					return base.ExactPosition == attractTo;
				}
				return false;
			}
		}

		public Player(Vector2 position, PlayerSpriteMode spriteMode)
			: base(new Vector2((int)position.X, (int)position.Y))
		{
			Input.ResetGrab();
			DefaultSpriteMode = spriteMode;
			base.Depth = 0;
			base.Tag = Tags.Persistent;
			if (SaveData.Instance != null && SaveData.Instance.Assists.PlayAsBadeline)
			{
				spriteMode = PlayerSpriteMode.MadelineAsBadeline;
			}
			Sprite = new PlayerSprite(spriteMode);
			Add(Hair = new PlayerHair(Sprite));
			Add(Sprite);
			if (spriteMode == PlayerSpriteMode.MadelineAsBadeline)
			{
				Hair.Color = NormalBadelineHairColor;
			}
			else
			{
				Hair.Color = NormalHairColor;
			}
			startHairCount = Sprite.HairCount;
			sweatSprite = GFX.SpriteBank.Create("player_sweat");
			Add(sweatSprite);
			base.Collider = normalHitbox;
			hurtbox = normalHurtbox;
			onCollideH = OnCollideH;
			onCollideV = OnCollideV;
			StateMachine = new StateMachine(26);
			StateMachine.SetCallbacks(0, NormalUpdate, null, NormalBegin, NormalEnd);
			StateMachine.SetCallbacks(1, ClimbUpdate, null, ClimbBegin, ClimbEnd);
			StateMachine.SetCallbacks(2, DashUpdate, DashCoroutine, DashBegin, DashEnd);
			StateMachine.SetCallbacks(3, SwimUpdate, null, SwimBegin);
			StateMachine.SetCallbacks(4, BoostUpdate, BoostCoroutine, BoostBegin, BoostEnd);
			StateMachine.SetCallbacks(5, RedDashUpdate, RedDashCoroutine, RedDashBegin, RedDashEnd);
			StateMachine.SetCallbacks(6, HitSquashUpdate, null, HitSquashBegin);
			StateMachine.SetCallbacks(7, LaunchUpdate, null, LaunchBegin);
			StateMachine.SetCallbacks(8, null, PickupCoroutine);
			StateMachine.SetCallbacks(9, DreamDashUpdate, null, DreamDashBegin, DreamDashEnd);
			StateMachine.SetCallbacks(10, SummitLaunchUpdate, null, SummitLaunchBegin);
			StateMachine.SetCallbacks(11, DummyUpdate, null, DummyBegin);
			StateMachine.SetCallbacks(12, null, IntroWalkCoroutine);
			StateMachine.SetCallbacks(13, null, IntroJumpCoroutine);
			StateMachine.SetCallbacks(14, null, null, IntroRespawnBegin, IntroRespawnEnd);
			StateMachine.SetCallbacks(15, null, IntroWakeUpCoroutine);
			StateMachine.SetCallbacks(20, TempleFallUpdate, TempleFallCoroutine);
			StateMachine.SetCallbacks(18, ReflectionFallUpdate, ReflectionFallCoroutine, ReflectionFallBegin, ReflectionFallEnd);
			StateMachine.SetCallbacks(16, BirdDashTutorialUpdate, BirdDashTutorialCoroutine, BirdDashTutorialBegin);
			StateMachine.SetCallbacks(17, FrozenUpdate);
			StateMachine.SetCallbacks(19, StarFlyUpdate, StarFlyCoroutine, StarFlyBegin, StarFlyEnd);
			StateMachine.SetCallbacks(21, CassetteFlyUpdate, CassetteFlyCoroutine, CassetteFlyBegin, CassetteFlyEnd);
			StateMachine.SetCallbacks(22, AttractUpdate, null, AttractBegin, AttractEnd);
			StateMachine.SetCallbacks(23, null, IntroMoonJumpCoroutine);
			StateMachine.SetCallbacks(24, FlingBirdUpdate, FlingBirdCoroutine, FlingBirdBegin, FlingBirdEnd);
			StateMachine.SetCallbacks(25, null, IntroThinkForABitCoroutine);
			Add(StateMachine);
			Add(Leader = new Leader(new Vector2(0f, -8f)));
			lastAim = Vector2.UnitX;
			Facing = Facings.Right;
			ChaserStates = new List<ChaserState>();
			triggersInside = new HashSet<Trigger>();
			Add(Light = new VertexLight(normalLightOffset, Color.White, 1f, 32, 64));
			Add(new WaterInteraction(() => StateMachine.State == 2 || StateMachine.State == 18));
			Add(new WindMover(WindMove));
			Add(wallSlideSfx = new SoundSource());
			Add(swimSurfaceLoopSfx = new SoundSource());
			Sprite.OnFrameChange = delegate(string anim)
			{
				if (base.Scene != null && !Dead && Sprite.Visible)
				{
					int currentAnimationFrame = Sprite.CurrentAnimationFrame;
					if ((anim.Equals("runSlow_carry") && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (anim.Equals("runFast") && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (anim.Equals("runSlow") && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (anim.Equals("walk") && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (anim.Equals("runStumble") && currentAnimationFrame == 6) || (anim.Equals("flip") && currentAnimationFrame == 4) || (anim.Equals("runWind") && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (anim.Equals("idleC") && Sprite.Mode == PlayerSpriteMode.MadelineNoBackpack && (currentAnimationFrame == 3 || currentAnimationFrame == 6 || currentAnimationFrame == 8 || currentAnimationFrame == 11)) || (anim.Equals("carryTheoWalk") && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (anim.Equals("push") && (currentAnimationFrame == 8 || currentAnimationFrame == 15)))
					{
						Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position + Vector2.UnitY, temp));
						if (platformByPriority != null)
						{
							Play("event:/char/madeline/footstep", "surface_index", platformByPriority.GetStepSoundIndex(this));
						}
					}
					else if ((anim.Equals("climbUp") && currentAnimationFrame == 5) || (anim.Equals("climbDown") && currentAnimationFrame == 5))
					{
						Platform platformByPriority2 = SurfaceIndex.GetPlatformByPriority(CollideAll<Solid>(base.Center + Vector2.UnitX * (float)Facing, temp));
						if (platformByPriority2 != null)
						{
							Play("event:/char/madeline/handhold", "surface_index", platformByPriority2.GetWallSoundIndex(this, (int)Facing));
						}
					}
					else if (anim.Equals("wakeUp") && currentAnimationFrame == 19)
					{
						Play("event:/char/madeline/campfire_stand");
					}
					else if (anim.Equals("sitDown") && currentAnimationFrame == 12)
					{
						Play("event:/char/madeline/summit_sit");
					}
					if (anim.Equals("push") && (currentAnimationFrame == 8 || currentAnimationFrame == 15))
					{
						Dust.BurstFG(Position + new Vector2((0 - Facing) * 5, -1f), new Vector2(0 - Facing, -0.5f).Angle(), 1, 0f);
					}
				}
			};
			Sprite.OnLastFrame = delegate
			{
				if (base.Scene != null && !Dead && Sprite.CurrentAnimationID == "idle" && !level.InCutscene && idleTimer > 3f && Calc.Random.Chance(0.2f))
				{
					string text = "";
					text = ((Sprite.Mode != 0) ? idleNoBackpackOptions.Choose() : ((level.CoreMode == Session.CoreModes.Hot) ? idleWarmOptions : idleColdOptions).Choose());
					if (!string.IsNullOrEmpty(text) && Sprite.Has(text))
					{
						Sprite.Play(text);
						if (Sprite.Mode == PlayerSpriteMode.Madeline)
						{
							if (text == "idleB")
							{
								idleSfx = Play("event:/char/madeline/idle_scratch");
							}
							else if (text == "idleC")
							{
								idleSfx = Play("event:/char/madeline/idle_sneeze");
							}
						}
						else if (text == "idleA")
						{
							idleSfx = Play("event:/char/madeline/idle_crackknuckles");
						}
					}
				}
			};
			Sprite.OnChange = delegate(string last, string next)
			{
				if ((last == "idleB" || last == "idleC") && next != null && !next.StartsWith("idle") && idleSfx != null)
				{
					Audio.Stop(idleSfx);
				}
			};
			Add(reflection = new MirrorReflection());
		}

		public void ResetSpriteNextFrame(PlayerSpriteMode mode)
		{
			nextSpriteMode = mode;
		}

		public void ResetSprite(PlayerSpriteMode mode)
		{
			string anim = Sprite.CurrentAnimationID;
			int frame = Sprite.CurrentAnimationFrame;
			Sprite.RemoveSelf();
			Add(Sprite = new PlayerSprite(mode));
			if (Sprite.Has(anim))
			{
				Sprite.Play(anim);
				if (frame < Sprite.CurrentAnimationTotalFrames)
				{
					Sprite.SetAnimationFrame(frame);
				}
			}
			Hair.Sprite = Sprite;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			level = SceneAs<Level>();
			lastDashes = (Dashes = MaxDashes);
			SpawnFacingTrigger trigger = CollideFirst<SpawnFacingTrigger>();
			if (trigger != null)
			{
				Facing = trigger.Facing;
			}
			else if (base.X > (float)level.Bounds.Center.X && IntroType != IntroTypes.None)
			{
				Facing = Facings.Left;
			}
			switch (IntroType)
			{
			case IntroTypes.Respawn:
				StateMachine.State = 14;
				JustRespawned = true;
				break;
			case IntroTypes.WalkInRight:
				IntroWalkDirection = Facings.Right;
				StateMachine.State = 12;
				break;
			case IntroTypes.WalkInLeft:
				IntroWalkDirection = Facings.Left;
				StateMachine.State = 12;
				break;
			case IntroTypes.Jump:
				StateMachine.State = 13;
				break;
			case IntroTypes.WakeUp:
				Sprite.Play("asleep");
				Facing = Facings.Right;
				StateMachine.State = 15;
				break;
			case IntroTypes.None:
				StateMachine.State = 0;
				break;
			case IntroTypes.Fall:
				StateMachine.State = 18;
				break;
			case IntroTypes.TempleMirrorVoid:
				StartTempleMirrorVoidSleep();
				break;
			case IntroTypes.ThinkForABit:
				StateMachine.State = 25;
				break;
			}
			IntroType = IntroTypes.Transition;
			StartHair();
			PreviousPosition = Position;
		}

		public void StartTempleMirrorVoidSleep()
		{
			Sprite.Play("asleep");
			Facing = Facings.Right;
			StateMachine.State = 11;
			StateMachine.Locked = true;
			DummyAutoAnimate = false;
			DummyGravity = false;
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);
			level = null;
			Audio.Stop(conveyorLoopSfx);
			foreach (Trigger item in triggersInside)
			{
				item.Triggered = false;
				item.OnLeave(this);
			}
			triggersInside.Clear();
		}

		public override void SceneEnd(Scene scene)
		{
			base.SceneEnd(scene);
			Audio.Stop(conveyorLoopSfx);
		}

		public override void Render()
		{
			if (SaveData.Instance.Assists.InvisibleMotion && InControl && ((!onGround && StateMachine.State != 1 && StateMachine.State != 3) || Speed.LengthSquared() > 800f))
			{
				return;
			}
			Vector2 was = Sprite.RenderPosition;
			Sprite.RenderPosition = Sprite.RenderPosition.Floor();
			if (StateMachine.State == 14)
			{
				DeathEffect.Draw(base.Center + deadOffset, Hair.Color, introEase);
			}
			else
			{
				if (StateMachine.State != 19)
				{
					if (IsTired && flash)
					{
						Sprite.Color = Color.Red;
					}
					else
					{
						Sprite.Color = Color.White;
					}
				}
				if (reflection.IsRendering && FlipInReflection)
				{
					Facing = (Facings)(0 - Facing);
					Hair.Facing = Facing;
				}
				Sprite.Scale.X *= (float)Facing;
				if (sweatSprite.LastAnimationID == "idle")
				{
					sweatSprite.Scale = Sprite.Scale;
				}
				else
				{
					sweatSprite.Scale.Y = Sprite.Scale.Y;
					sweatSprite.Scale.X = Math.Abs(Sprite.Scale.X) * (float)Math.Sign(sweatSprite.Scale.X);
				}
				base.Render();
				if (Sprite.CurrentAnimationID == "startStarFly")
				{
					float p = (float)Sprite.CurrentAnimationFrame / (float)Sprite.CurrentAnimationTotalFrames;
					GFX.Game.GetAtlasSubtexturesAt("characters/player/startStarFlyWhite", Sprite.CurrentAnimationFrame).Draw(Sprite.RenderPosition, Sprite.Origin, starFlyColor * p, Sprite.Scale, Sprite.Rotation, SpriteEffects.None);
				}
				Sprite.Scale.X *= (float)Facing;
				if (reflection.IsRendering && FlipInReflection)
				{
					Facing = (Facings)(0 - Facing);
					Hair.Facing = Facing;
				}
			}
			Sprite.RenderPosition = was;
		}

		public override void DebugRender(Camera camera)
		{
			base.DebugRender(camera);
			Collider was = base.Collider;
			base.Collider = hurtbox;
			Draw.HollowRect(base.Collider, Color.Lime);
			base.Collider = was;
		}

		public override void Update()
		{
			if (SaveData.Instance.Assists.InfiniteStamina)
			{
				Stamina = 110f;
			}
			PreviousPosition = Position;
			if (nextSpriteMode.HasValue)
			{
				ResetSprite(nextSpriteMode.Value);
				nextSpriteMode = null;
			}
			climbTriggerDir = 0;
			if (SaveData.Instance.Assists.Hiccups)
			{
				if (hiccupTimer <= 0f)
				{
					hiccupTimer = level.HiccupRandom.Range(1.2f, 1.8f);
				}
				if (Ducking)
				{
					hiccupTimer -= Engine.DeltaTime * 0.5f;
				}
				else
				{
					hiccupTimer -= Engine.DeltaTime;
				}
				if (hiccupTimer <= 0f)
				{
					HiccupJump();
				}
			}
			if (gliderBoostTimer > 0f)
			{
				gliderBoostTimer -= Engine.DeltaTime;
			}
			if (lowFrictionStopTimer > 0f)
			{
				lowFrictionStopTimer -= Engine.DeltaTime;
			}
			if (explodeLaunchBoostTimer > 0f)
			{
				if (Input.MoveX.Value == Math.Sign(explodeLaunchBoostSpeed))
				{
					Speed.X = explodeLaunchBoostSpeed;
					explodeLaunchBoostTimer = 0f;
				}
				else
				{
					explodeLaunchBoostTimer -= Engine.DeltaTime;
				}
			}
			StrawberryCollectResetTimer -= Engine.DeltaTime;
			if (StrawberryCollectResetTimer <= 0f)
			{
				StrawberryCollectIndex = 0;
			}
			idleTimer += Engine.DeltaTime;
			if (level != null && level.InCutscene)
			{
				idleTimer = -5f;
			}
			else if (Speed.X != 0f || Speed.Y != 0f)
			{
				idleTimer = 0f;
			}
			if (!Dead)
			{
				Audio.MusicUnderwater = UnderwaterMusicCheck();
			}
			if (JustRespawned && Speed != Vector2.Zero)
			{
				JustRespawned = false;
			}
			if (StateMachine.State == 9)
			{
				bool flag2 = (OnSafeGround = false);
				onGround = flag2;
			}
			else if (Speed.Y >= 0f)
			{
				Platform first = CollideFirst<Solid>(Position + Vector2.UnitY);
				if (first == null)
				{
					first = CollideFirstOutside<JumpThru>(Position + Vector2.UnitY);
				}
				if (first != null)
				{
					onGround = true;
					OnSafeGround = first.Safe;
				}
				else
				{
					bool flag2 = (OnSafeGround = false);
					onGround = flag2;
				}
			}
			else
			{
				bool flag2 = (OnSafeGround = false);
				onGround = flag2;
			}
			if (StateMachine.State == 3)
			{
				OnSafeGround = true;
			}
			if (OnSafeGround)
			{
				foreach (SafeGroundBlocker component in base.Scene.Tracker.GetComponents<SafeGroundBlocker>())
				{
					if (component.Check(this))
					{
						OnSafeGround = false;
						break;
					}
				}
			}
			playFootstepOnLand -= Engine.DeltaTime;
			if (onGround)
			{
				highestAirY = base.Y;
			}
			else
			{
				highestAirY = Math.Min(base.Y, highestAirY);
			}
			if (base.Scene.OnInterval(0.05f))
			{
				flash = !flash;
			}
			if (wallSlideDir != 0)
			{
				wallSlideTimer = Math.Max(wallSlideTimer - Engine.DeltaTime, 0f);
				wallSlideDir = 0;
			}
			if (wallBoostTimer > 0f)
			{
				wallBoostTimer -= Engine.DeltaTime;
				if (moveX == wallBoostDir)
				{
					Speed.X = 130f * (float)moveX;
					Stamina += 27.5f;
					wallBoostTimer = 0f;
					sweatSprite.Play("idle");
				}
			}
			if (onGround && StateMachine.State != 1)
			{
				AutoJump = false;
				Stamina = 110f;
				wallSlideTimer = 1.2f;
			}
			if (dashAttackTimer > 0f)
			{
				dashAttackTimer -= Engine.DeltaTime;
			}
			if (onGround)
			{
				dreamJump = false;
				jumpGraceTimer = 0.1f;
			}
			else if (jumpGraceTimer > 0f)
			{
				jumpGraceTimer -= Engine.DeltaTime;
			}
			if (dashCooldownTimer > 0f)
			{
				dashCooldownTimer -= Engine.DeltaTime;
			}
			if (dashRefillCooldownTimer > 0f)
			{
				dashRefillCooldownTimer -= Engine.DeltaTime;
			}
			else if (SaveData.Instance.Assists.DashMode == Assists.DashModes.Infinite && !level.InCutscene)
			{
				RefillDash();
			}
			else if (!Inventory.NoRefills)
			{
				if (StateMachine.State == 3)
				{
					RefillDash();
				}
				else if (onGround && (CollideCheck<Solid, NegaBlock>(Position + Vector2.UnitY) || CollideCheckOutside<JumpThru>(Position + Vector2.UnitY)) && (!CollideCheck<Spikes>(Position) || SaveData.Instance.Assists.Invincible))
				{
					RefillDash();
				}
			}
			if (varJumpTimer > 0f)
			{
				varJumpTimer -= Engine.DeltaTime;
			}
			if (AutoJumpTimer > 0f)
			{
				if (AutoJump)
				{
					AutoJumpTimer -= Engine.DeltaTime;
					if (AutoJumpTimer <= 0f)
					{
						AutoJump = false;
					}
				}
				else
				{
					AutoJumpTimer = 0f;
				}
			}
			if (forceMoveXTimer > 0f)
			{
				forceMoveXTimer -= Engine.DeltaTime;
				moveX = forceMoveX;
			}
			else
			{
				moveX = Input.MoveX.Value;
				climbHopSolid = null;
			}
			if (climbHopSolid != null && !climbHopSolid.Collidable)
			{
				climbHopSolid = null;
			}
			else if (climbHopSolid != null && climbHopSolid.Position != climbHopSolidPosition)
			{
				Vector2 move = climbHopSolid.Position - climbHopSolidPosition;
				climbHopSolidPosition = climbHopSolid.Position;
				MoveHExact((int)move.X);
				MoveVExact((int)move.Y);
			}
			if (noWindTimer > 0f)
			{
				noWindTimer -= Engine.DeltaTime;
			}
			if (moveX != 0 && InControl && StateMachine.State != 1 && StateMachine.State != 8 && StateMachine.State != 5 && StateMachine.State != 6)
			{
				Facings to = (Facings)moveX;
				if (to != Facing && Ducking)
				{
					Sprite.Scale = new Vector2(0.8f, 1.2f);
				}
				Facing = to;
			}
			lastAim = Input.GetAimVector(Facing);
			if (wallSpeedRetentionTimer > 0f)
			{
				if (Math.Sign(Speed.X) == -Math.Sign(wallSpeedRetained))
				{
					wallSpeedRetentionTimer = 0f;
				}
				else if (!CollideCheck<Solid>(Position + Vector2.UnitX * Math.Sign(wallSpeedRetained)))
				{
					Speed.X = wallSpeedRetained;
					wallSpeedRetentionTimer = 0f;
				}
				else
				{
					wallSpeedRetentionTimer -= Engine.DeltaTime;
				}
			}
			if (hopWaitX != 0)
			{
				if (Math.Sign(Speed.X) == -hopWaitX || Speed.Y > 0f)
				{
					hopWaitX = 0;
				}
				else if (!CollideCheck<Solid>(Position + Vector2.UnitX * hopWaitX))
				{
					lowFrictionStopTimer = 0.15f;
					Speed.X = hopWaitXSpeed;
					hopWaitX = 0;
				}
			}
			if (windTimeout > 0f)
			{
				windTimeout -= Engine.DeltaTime;
			}
			Vector2 windDir = windDirection;
			if (ForceStrongWindHair.Length() > 0f)
			{
				windDir = ForceStrongWindHair;
			}
			if (windTimeout > 0f && windDir.X != 0f)
			{
				windHairTimer += Engine.DeltaTime * 8f;
				Hair.StepPerSegment = new Vector2(windDir.X * 5f, (float)Math.Sin(windHairTimer));
				Hair.StepInFacingPerSegment = 0f;
				Hair.StepApproach = 128f;
				Hair.StepYSinePerSegment = 0f;
			}
			else if (Dashes > 1)
			{
				Hair.StepPerSegment = new Vector2((float)Math.Sin(base.Scene.TimeActive * 2f) * 0.7f - (float)((int)Facing * 3), (float)Math.Sin(base.Scene.TimeActive * 1f));
				Hair.StepInFacingPerSegment = 0f;
				Hair.StepApproach = 90f;
				Hair.StepYSinePerSegment = 1f;
				Hair.StepPerSegment.Y += windDir.Y * 2f;
			}
			else
			{
				Hair.StepPerSegment = new Vector2(0f, 2f);
				Hair.StepInFacingPerSegment = 0.5f;
				Hair.StepApproach = 64f;
				Hair.StepYSinePerSegment = 0f;
				Hair.StepPerSegment.Y += windDir.Y * 0.5f;
			}
			if (StateMachine.State == 5)
			{
				Sprite.HairCount = 1;
			}
			else if (StateMachine.State != 19)
			{
				Sprite.HairCount = ((Dashes > 1) ? 5 : startHairCount);
			}
			if (minHoldTimer > 0f)
			{
				minHoldTimer -= Engine.DeltaTime;
			}
			if (launched)
			{
				if (Speed.LengthSquared() < 19600f)
				{
					launched = false;
				}
				else
				{
					float was2 = launchedTimer;
					launchedTimer += Engine.DeltaTime;
					if (launchedTimer >= 0.5f)
					{
						launched = false;
						launchedTimer = 0f;
					}
					else if (Calc.OnInterval(launchedTimer, was2, 0.15f))
					{
						level.Add(Engine.Pooler.Create<SpeedRing>().Init(base.Center, Speed.Angle(), Color.White));
					}
				}
			}
			else
			{
				launchedTimer = 0f;
			}
			if (IsTired)
			{
				Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
				if (!wasTired)
				{
					wasTired = true;
				}
			}
			else
			{
				wasTired = false;
			}
			base.Update();
			if (Ducking)
			{
				Light.Position = duckingLightOffset;
			}
			else
			{
				Light.Position = normalLightOffset;
			}
			if (!onGround && Speed.Y <= 0f && (StateMachine.State != 1 || lastClimbMove == -1) && CollideCheck<JumpThru>() && !JumpThruBoostBlockedCheck())
			{
				MoveV(-40f * Engine.DeltaTime);
			}
			if (!onGround && DashAttacking && DashDir.Y == 0f && (CollideCheck<Solid>(Position + Vector2.UnitY * 3f) || CollideCheckOutside<JumpThru>(Position + Vector2.UnitY * 3f)) && !DashCorrectCheck(Vector2.UnitY * 3f))
			{
				MoveVExact(3);
			}
			if (Speed.Y > 0f && CanUnDuck && base.Collider != starFlyHitbox && !onGround && jumpGraceTimer <= 0f)
			{
				Ducking = false;
			}
			if (StateMachine.State != 9 && StateMachine.State != 22)
			{
				MoveH(Speed.X * Engine.DeltaTime, onCollideH);
			}
			if (StateMachine.State != 9 && StateMachine.State != 22)
			{
				MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
			}
			if (StateMachine.State == 3)
			{
				if (Speed.Y < 0f && Speed.Y >= -60f)
				{
					while (!SwimCheck())
					{
						Speed.Y = 0f;
						if (MoveVExact(1))
						{
							break;
						}
					}
				}
			}
			else if (StateMachine.State == 0 && SwimCheck())
			{
				StateMachine.State = 3;
			}
			else if (StateMachine.State == 1 && SwimCheck())
			{
				Water water = CollideFirst<Water>(Position);
				if (water != null && base.Center.Y < water.Center.Y)
				{
					while (SwimCheck() && !MoveVExact(-1))
					{
					}
					if (SwimCheck())
					{
						StateMachine.State = 3;
					}
				}
				else
				{
					StateMachine.State = 3;
				}
			}
			if (Sprite.CurrentAnimationID != null && Sprite.CurrentAnimationID.Equals("wallslide") && Speed.Y > 0f)
			{
				if (!wallSlideSfx.Playing)
				{
					Loop(wallSlideSfx, "event:/char/madeline/wallslide");
				}
				Platform platform = SurfaceIndex.GetPlatformByPriority(CollideAll<Solid>(base.Center + Vector2.UnitX * (float)Facing, temp));
				if (platform != null)
				{
					wallSlideSfx.Param("surface_index", platform.GetWallSoundIndex(this, (int)Facing));
				}
			}
			else
			{
				Stop(wallSlideSfx);
			}
			UpdateSprite();
			UpdateCarry();
			if (StateMachine.State != 18)
			{
				foreach (Trigger trigger in base.Scene.Tracker.GetEntities<Trigger>())
				{
					if (CollideCheck(trigger))
					{
						if (!trigger.Triggered)
						{
							trigger.Triggered = true;
							triggersInside.Add(trigger);
							trigger.OnEnter(this);
						}
						trigger.OnStay(this);
					}
					else if (trigger.Triggered)
					{
						triggersInside.Remove(trigger);
						trigger.Triggered = false;
						trigger.OnLeave(this);
					}
				}
			}
			StrawberriesBlocked = CollideCheck<BlockField>();
			if (InControl || ForceCameraUpdate)
			{
				if (StateMachine.State == 18)
				{
					level.Camera.Position = CameraTarget;
				}
				else
				{
					Vector2 from = level.Camera.Position;
					Vector2 target = CameraTarget;
					float multiplier = ((StateMachine.State == 20) ? 8f : 1f);
					level.Camera.Position = from + (target - from) * (1f - (float)Math.Pow(0.01f / multiplier, Engine.DeltaTime));
				}
			}
			if (!Dead && StateMachine.State != 21)
			{
				Collider was = base.Collider;
				base.Collider = hurtbox;
				foreach (PlayerCollider component2 in base.Scene.Tracker.GetComponents<PlayerCollider>())
				{
					if (component2.Check(this) && Dead)
					{
						base.Collider = was;
						return;
					}
				}
				if (base.Collider == hurtbox)
				{
					base.Collider = was;
				}
			}
			if (InControl && !Dead && StateMachine.State != 9 && EnforceLevelBounds)
			{
				level.EnforceBounds(this);
			}
			UpdateChaserStates();
			UpdateHair(applyGravity: true);
			if (wasDucking != Ducking)
			{
				wasDucking = Ducking;
				if (wasDucking)
				{
					Play("event:/char/madeline/duck");
				}
				else if (onGround)
				{
					Play("event:/char/madeline/stand");
				}
			}
			if (Speed.X != 0f && ((StateMachine.State == 3 && !SwimUnderwaterCheck()) || (StateMachine.State == 0 && CollideCheck<Water>(Position))))
			{
				if (!swimSurfaceLoopSfx.Playing)
				{
					swimSurfaceLoopSfx.Play("event:/char/madeline/water_move_shallow");
				}
			}
			else
			{
				swimSurfaceLoopSfx.Stop();
			}
			wasOnGround = onGround;
			windMovedUp = false;
		}

		private void CreateTrail()
		{
			Vector2 scale = new Vector2(Math.Abs(Sprite.Scale.X) * (float)Facing, Sprite.Scale.Y);
			if (Sprite.Mode == PlayerSpriteMode.MadelineAsBadeline)
			{
				TrailManager.Add(this, scale, wasDashB ? NormalBadelineHairColor : UsedBadelineHairColor);
			}
			else
			{
				TrailManager.Add(this, scale, wasDashB ? NormalHairColor : UsedHairColor);
			}
		}

		public void CleanUpTriggers()
		{
			if (triggersInside.Count <= 0)
			{
				return;
			}
			foreach (Trigger item in triggersInside)
			{
				item.OnLeave(this);
				item.Triggered = false;
			}
			triggersInside.Clear();
		}

		private void UpdateChaserStates()
		{
			while (ChaserStates.Count > 0 && base.Scene.TimeActive - ChaserStates[0].TimeStamp > 4f)
			{
				ChaserStates.RemoveAt(0);
			}
			ChaserStates.Add(new ChaserState(this));
			activeSounds.Clear();
		}

		private void StartHair()
		{
			if (!startHairCalled)
			{
				startHairCalled = true;
				Hair.Facing = Facing;
				Hair.Start();
				UpdateHair(applyGravity: true);
			}
		}

		public void UpdateHair(bool applyGravity)
		{
			if (StateMachine.State == 19)
			{
				Hair.Color = Sprite.Color;
				applyGravity = false;
			}
			else if (Dashes == 0 && Dashes < MaxDashes)
			{
				if (Sprite.Mode == PlayerSpriteMode.MadelineAsBadeline)
				{
					Hair.Color = Color.Lerp(Hair.Color, UsedBadelineHairColor, 6f * Engine.DeltaTime);
				}
				else
				{
					Hair.Color = Color.Lerp(Hair.Color, UsedHairColor, 6f * Engine.DeltaTime);
				}
			}
			else
			{
				Color color;
				if (lastDashes != Dashes)
				{
					color = FlashHairColor;
					hairFlashTimer = 0.12f;
				}
				else if (!(hairFlashTimer > 0f))
				{
					color = ((Sprite.Mode == PlayerSpriteMode.MadelineAsBadeline) ? ((Dashes != 2) ? NormalBadelineHairColor : TwoDashesBadelineHairColor) : ((Dashes != 2) ? NormalHairColor : TwoDashesHairColor));
				}
				else
				{
					color = FlashHairColor;
					hairFlashTimer -= Engine.DeltaTime;
				}
				Hair.Color = color;
			}
			if (OverrideHairColor.HasValue)
			{
				Hair.Color = OverrideHairColor.Value;
			}
			Hair.Facing = Facing;
			Hair.SimulateMotion = applyGravity;
			lastDashes = Dashes;
		}

		private void UpdateSprite()
		{
			Sprite.Scale.X = Calc.Approach(Sprite.Scale.X, 1f, 1.75f * Engine.DeltaTime);
			Sprite.Scale.Y = Calc.Approach(Sprite.Scale.Y, 1f, 1.75f * Engine.DeltaTime);
			if (InControl && Sprite.CurrentAnimationID != "throw" && StateMachine.State != 20 && StateMachine.State != 18 && StateMachine.State != 19 && StateMachine.State != 21)
			{
				if (StateMachine.State == 22)
				{
					Sprite.Play("fallFast");
				}
				else if (StateMachine.State == 10)
				{
					Sprite.Play("launch");
				}
				else if (StateMachine.State == 8)
				{
					Sprite.Play("pickup");
				}
				else if (StateMachine.State == 3)
				{
					if (Input.MoveY.Value > 0)
					{
						Sprite.Play("swimDown");
					}
					else if (Input.MoveY.Value < 0)
					{
						Sprite.Play("swimUp");
					}
					else
					{
						Sprite.Play("swimIdle");
					}
				}
				else if (StateMachine.State == 9)
				{
					if (Sprite.CurrentAnimationID != "dreamDashIn" && Sprite.CurrentAnimationID != "dreamDashLoop")
					{
						Sprite.Play("dreamDashIn");
					}
				}
				else if (Sprite.DreamDashing && Sprite.LastAnimationID != "dreamDashOut")
				{
					Sprite.Play("dreamDashOut");
				}
				else if (Sprite.CurrentAnimationID != "dreamDashOut")
				{
					if (DashAttacking)
					{
						if (onGround && DashDir.Y == 0f && !Ducking && Speed.X != 0f && moveX == -Math.Sign(Speed.X))
						{
							if (base.Scene.OnInterval(0.02f))
							{
								Dust.Burst(Position, -(float)Math.PI / 2f);
							}
							Sprite.Play("skid");
						}
						else if (Ducking)
						{
							Sprite.Play("duck");
						}
						else
						{
							Sprite.Play("dash");
						}
					}
					else if (StateMachine.State == 1)
					{
						if (lastClimbMove < 0)
						{
							Sprite.Play("climbUp");
						}
						else if (lastClimbMove > 0)
						{
							Sprite.Play("wallslide");
						}
						else if (!CollideCheck<Solid>(Position + new Vector2((float)Facing, 6f)))
						{
							Sprite.Play("dangling");
						}
						else if ((float)Input.MoveX == (float)(0 - Facing))
						{
							if (Sprite.CurrentAnimationID != "climbLookBack")
							{
								Sprite.Play("climbLookBackStart");
							}
						}
						else
						{
							Sprite.Play("wallslide");
						}
					}
					else if (Ducking && StateMachine.State == 0)
					{
						Sprite.Play("duck");
					}
					else if (onGround)
					{
						fastJump = false;
						if (Holding == null && moveX != 0 && CollideCheck<Solid>(Position + Vector2.UnitX * moveX) && !ClimbBlocker.EdgeCheck(level, this, moveX))
						{
							Sprite.Play("push");
						}
						else if (Math.Abs(Speed.X) <= 25f && moveX == 0)
						{
							if (Holding != null)
							{
								Sprite.Play("idle_carry");
							}
							else if (!base.Scene.CollideCheck<Solid>(Position + new Vector2((float)Facing, 2f)) && !base.Scene.CollideCheck<Solid>(Position + new Vector2((int)Facing * 4, 2f)) && !CollideCheck<JumpThru>(Position + new Vector2((int)Facing * 4, 2f)))
							{
								Sprite.Play("edge");
							}
							else if (!base.Scene.CollideCheck<Solid>(Position + new Vector2(0 - Facing, 2f)) && !base.Scene.CollideCheck<Solid>(Position + new Vector2((0 - Facing) * 4, 2f)) && !CollideCheck<JumpThru>(Position + new Vector2((0 - Facing) * 4, 2f)))
							{
								Sprite.Play("edgeBack");
							}
							else if (Input.MoveY.Value == -1)
							{
								if (Sprite.LastAnimationID != "lookUp")
								{
									Sprite.Play("lookUp");
								}
							}
							else if (Sprite.CurrentAnimationID != null && (!Sprite.CurrentAnimationID.Contains("idle") || (Sprite.CurrentAnimationID == "idle_carry" && Holding == null)))
							{
								Sprite.Play("idle");
							}
						}
						else if (Holding != null)
						{
							Sprite.Play("runSlow_carry");
						}
						else if (Math.Sign(Speed.X) == -moveX && moveX != 0)
						{
							if (Math.Abs(Speed.X) > 90f)
							{
								Sprite.Play("skid");
							}
							else if (Sprite.CurrentAnimationID != "skid")
							{
								Sprite.Play("flip");
							}
						}
						else if (windDirection.X != 0f && windTimeout > 0f && Facing == (Facings)(-Math.Sign(windDirection.X)))
						{
							Sprite.Play("runWind");
						}
						else if (!Sprite.Running || Sprite.CurrentAnimationID == "runWind" || (Sprite.CurrentAnimationID == "runSlow_carry" && Holding == null))
						{
							if (Math.Abs(Speed.X) < 45f)
							{
								Sprite.Play("runSlow");
							}
							else
							{
								Sprite.Play("runFast");
							}
						}
					}
					else if (wallSlideDir != 0 && Holding == null)
					{
						Sprite.Play("wallslide");
					}
					else if (Speed.Y < 0f)
					{
						if (Holding != null)
						{
							Sprite.Play("jumpSlow_carry");
						}
						else if (fastJump || Math.Abs(Speed.X) > 90f)
						{
							fastJump = true;
							Sprite.Play("jumpFast");
						}
						else
						{
							Sprite.Play("jumpSlow");
						}
					}
					else if (Holding != null)
					{
						Sprite.Play("fallSlow_carry");
					}
					else if (fastJump || Speed.Y >= 160f || level.InSpace)
					{
						fastJump = true;
						if (Sprite.LastAnimationID != "fallFast")
						{
							Sprite.Play("fallFast");
						}
					}
					else
					{
						Sprite.Play("fallSlow");
					}
				}
			}
			if (StateMachine.State != 11)
			{
				if (level.InSpace)
				{
					Sprite.Rate = 0.5f;
				}
				else
				{
					Sprite.Rate = 1f;
				}
			}
		}

		public void CreateSplitParticles()
		{
			level.Particles.Emit(P_Split, 16, base.Center, Vector2.One * 6f);
		}

		public bool GetChasePosition(float sceneTime, float timeAgo, out ChaserState chaseState)
		{
			if (!Dead)
			{
				bool tooLongAgoFound = false;
				foreach (ChaserState state in ChaserStates)
				{
					float time = sceneTime - state.TimeStamp;
					if (time <= timeAgo)
					{
						if (tooLongAgoFound || timeAgo - time < 0.02f)
						{
							chaseState = state;
							return true;
						}
						chaseState = default(ChaserState);
						return false;
					}
					tooLongAgoFound = true;
				}
			}
			chaseState = default(ChaserState);
			return false;
		}

		public void OnTransition()
		{
			wallSlideTimer = 1.2f;
			jumpGraceTimer = 0f;
			forceMoveXTimer = 0f;
			ChaserStates.Clear();
			RefillDash();
			RefillStamina();
			Leader.TransferFollowers();
		}

		public bool TransitionTo(Vector2 target, Vector2 direction)
		{
			MoveTowardsX(target.X, 60f * Engine.DeltaTime);
			MoveTowardsY(target.Y, 60f * Engine.DeltaTime);
			UpdateHair(applyGravity: false);
			UpdateCarry();
			if (Position == target)
			{
				ZeroRemainderX();
				ZeroRemainderY();
				Speed.X = (int)Math.Round(Speed.X);
				Speed.Y = (int)Math.Round(Speed.Y);
				return true;
			}
			return false;
		}

		public void BeforeSideTransition()
		{
		}

		public void BeforeDownTransition()
		{
			if (StateMachine.State != 5 && StateMachine.State != 18 && StateMachine.State != 19)
			{
				StateMachine.State = 0;
				Speed.Y = Math.Max(0f, Speed.Y);
				AutoJump = false;
				varJumpTimer = 0f;
			}
			foreach (Entity platform in base.Scene.Tracker.GetEntities<Platform>())
			{
				if (!(platform is SolidTiles) && CollideCheckOutside(platform, Position + Vector2.UnitY * base.Height))
				{
					platform.Collidable = false;
				}
			}
		}

		public void BeforeUpTransition()
		{
			Speed.X = 0f;
			if (StateMachine.State != 5 && StateMachine.State != 18 && StateMachine.State != 19)
			{
				varJumpSpeed = (Speed.Y = -105f);
				if (StateMachine.State == 10)
				{
					StateMachine.State = 13;
				}
				else
				{
					StateMachine.State = 0;
				}
				AutoJump = true;
				AutoJumpTimer = 0f;
				varJumpTimer = 0.2f;
			}
			dashCooldownTimer = 0.2f;
		}

		private bool LaunchedBoostCheck()
		{
			if (LiftBoost.LengthSquared() >= 10000f && Speed.LengthSquared() >= 48400f)
			{
				launched = true;
				return true;
			}
			launched = false;
			return false;
		}

		public void HiccupJump()
		{
			switch (StateMachine.State)
			{
			default:
				StateMachine.State = 0;
				Speed.X = Calc.Approach(Speed.X, 0f, 40f);
				if (Speed.Y > -60f)
				{
					varJumpSpeed = (Speed.Y = -60f);
					varJumpTimer = 0.15f;
					AutoJump = true;
					AutoJumpTimer = 0f;
					if (jumpGraceTimer > 0f)
					{
						jumpGraceTimer = 0.6f;
					}
				}
				sweatSprite.Play("jump", restart: true);
				break;
			case 1:
				StateMachine.State = 0;
				varJumpSpeed = (Speed.Y = -60f);
				varJumpTimer = 0.15f;
				Speed.X = 130f * (float)(0 - Facing);
				AutoJump = true;
				AutoJumpTimer = 0f;
				sweatSprite.Play("jump", restart: true);
				break;
			case 19:
				if (Speed.X > 0f)
				{
					Speed = Speed.Rotate(0.6981317f);
				}
				else
				{
					Speed = Speed.Rotate(-0.6981317f);
				}
				break;
			case 5:
			case 9:
				if (Speed.X < 0f || (Speed.X == 0f && Speed.Y < 0f))
				{
					Speed = Speed.Rotate(0.17453292f);
				}
				else
				{
					Speed = Speed.Rotate(-0.17453292f);
				}
				break;
			case 4:
			case 7:
			case 22:
				sweatSprite.Play("jump", restart: true);
				break;
			case 10:
			case 11:
			case 12:
			case 13:
			case 14:
			case 15:
			case 16:
			case 17:
			case 18:
			case 21:
			case 24:
				return;
			}
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
			Play(Ducking ? "event:/new_content/char/madeline/hiccup_ducking" : "event:/new_content/char/madeline/hiccup_standing");
		}

		public void Jump(bool particles = true, bool playSfx = true)
		{
			Input.Jump.ConsumeBuffer();
			jumpGraceTimer = 0f;
			varJumpTimer = 0.2f;
			AutoJump = false;
			dashAttackTimer = 0f;
			gliderBoostTimer = 0f;
			wallSlideTimer = 1.2f;
			wallBoostTimer = 0f;
			Speed.X += 40f * (float)moveX;
			Speed.Y = -105f;
			Speed += LiftBoost;
			varJumpSpeed = Speed.Y;
			LaunchedBoostCheck();
			if (playSfx)
			{
				if (launched)
				{
					Play("event:/char/madeline/jump_assisted");
				}
				if (dreamJump)
				{
					Play("event:/char/madeline/jump_dreamblock");
				}
				else
				{
					Play("event:/char/madeline/jump");
				}
			}
			Sprite.Scale = new Vector2(0.6f, 1.4f);
			if (particles)
			{
				int soundIndex = -1;
				Platform jumpOff = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position + Vector2.UnitY, temp));
				if (jumpOff != null)
				{
					soundIndex = jumpOff.GetLandSoundIndex(this);
				}
				Dust.Burst(base.BottomCenter, -(float)Math.PI / 2f, 4, DustParticleFromSurfaceIndex(soundIndex));
			}
			SaveData.Instance.TotalJumps++;
		}

		private void SuperJump()
		{
			Input.Jump.ConsumeBuffer();
			jumpGraceTimer = 0f;
			varJumpTimer = 0.2f;
			AutoJump = false;
			dashAttackTimer = 0f;
			gliderBoostTimer = 0f;
			wallSlideTimer = 1.2f;
			wallBoostTimer = 0f;
			Speed.X = 260f * (float)Facing;
			Speed.Y = -105f;
			Speed += LiftBoost;
			gliderBoostTimer = 0.55f;
			Play("event:/char/madeline/jump");
			if (Ducking)
			{
				Ducking = false;
				Speed.X *= 1.25f;
				Speed.Y *= 0.5f;
				Play("event:/char/madeline/jump_superslide");
				gliderBoostDir = Calc.AngleToVector((float)Math.PI * -3f / 16f, 1f);
			}
			else
			{
				gliderBoostDir = Calc.AngleToVector(-(float)Math.PI / 4f, 1f);
				Play("event:/char/madeline/jump_super");
			}
			varJumpSpeed = Speed.Y;
			launched = true;
			Sprite.Scale = new Vector2(0.6f, 1.4f);
			int surfaceIndex = -1;
			Platform jumpOff = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position + Vector2.UnitY, temp));
			if (jumpOff != null)
			{
				surfaceIndex = jumpOff.GetLandSoundIndex(this);
			}
			Dust.Burst(base.BottomCenter, -(float)Math.PI / 2f, 4, DustParticleFromSurfaceIndex(surfaceIndex));
			SaveData.Instance.TotalJumps++;
		}

		private bool WallJumpCheck(int dir)
		{
			int dist = 3;
			bool farCheck = DashAttacking && DashDir.X == 0f && DashDir.Y == -1f;
			if (farCheck)
			{
				Spikes.Directions checkDir = ((dir <= 0) ? Spikes.Directions.Right : Spikes.Directions.Left);
				foreach (Spikes spikes in level.Tracker.GetEntities<Spikes>())
				{
					if (spikes.Direction == checkDir && CollideCheck(spikes, Position + Vector2.UnitX * dir * 5f))
					{
						farCheck = false;
						break;
					}
				}
			}
			if (farCheck)
			{
				dist = 5;
			}
			if (ClimbBoundsCheck(dir) && !ClimbBlocker.EdgeCheck(level, this, dir * dist))
			{
				return CollideCheck<Solid>(Position + Vector2.UnitX * dir * dist);
			}
			return false;
		}

		private void WallJump(int dir)
		{
			Ducking = false;
			Input.Jump.ConsumeBuffer();
			jumpGraceTimer = 0f;
			varJumpTimer = 0.2f;
			AutoJump = false;
			dashAttackTimer = 0f;
			gliderBoostTimer = 0f;
			wallSlideTimer = 1.2f;
			wallBoostTimer = 0f;
			lowFrictionStopTimer = 0.15f;
			if (Holding != null && Holding.SlowFall)
			{
				forceMoveX = dir;
				forceMoveXTimer = 0.26f;
			}
			else if (moveX != 0)
			{
				forceMoveX = dir;
				forceMoveXTimer = 0.16f;
			}
			if (base.LiftSpeed == Vector2.Zero)
			{
				Solid wall = CollideFirst<Solid>(Position + Vector2.UnitX * 3f * -dir);
				if (wall != null)
				{
					base.LiftSpeed = wall.LiftSpeed;
				}
			}
			Speed.X = 130f * (float)dir;
			Speed.Y = -105f;
			Speed += LiftBoost;
			varJumpSpeed = Speed.Y;
			LaunchedBoostCheck();
			int surfaceIndex = -1;
			Platform pushOff = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position - Vector2.UnitX * dir * 4f, temp));
			if (pushOff != null)
			{
				surfaceIndex = pushOff.GetWallSoundIndex(this, -dir);
				Play("event:/char/madeline/landing", "surface_index", surfaceIndex);
				if (pushOff is DreamBlock)
				{
					(pushOff as DreamBlock).FootstepRipple(Position + new Vector2(dir * 3, -4f));
				}
			}
			Play((dir < 0) ? "event:/char/madeline/jump_wall_right" : "event:/char/madeline/jump_wall_left");
			Sprite.Scale = new Vector2(0.6f, 1.4f);
			if (dir == -1)
			{
				Dust.Burst(base.Center + Vector2.UnitX * 2f, (float)Math.PI * -3f / 4f, 4, DustParticleFromSurfaceIndex(surfaceIndex));
			}
			else
			{
				Dust.Burst(base.Center + Vector2.UnitX * -2f, -(float)Math.PI / 4f, 4, DustParticleFromSurfaceIndex(surfaceIndex));
			}
			SaveData.Instance.TotalWallJumps++;
		}

		private void SuperWallJump(int dir)
		{
			Ducking = false;
			Input.Jump.ConsumeBuffer();
			jumpGraceTimer = 0f;
			varJumpTimer = 0.25f;
			AutoJump = false;
			dashAttackTimer = 0f;
			gliderBoostTimer = 0.55f;
			gliderBoostDir = -Vector2.UnitY;
			wallSlideTimer = 1.2f;
			wallBoostTimer = 0f;
			Speed.X = 170f * (float)dir;
			Speed.Y = -160f;
			Speed += LiftBoost;
			varJumpSpeed = Speed.Y;
			launched = true;
			Play((dir < 0) ? "event:/char/madeline/jump_wall_right" : "event:/char/madeline/jump_wall_left");
			Play("event:/char/madeline/jump_superwall");
			Sprite.Scale = new Vector2(0.6f, 1.4f);
			int surfaceIndex = -1;
			Platform pushOff = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position - Vector2.UnitX * dir * 4f, temp));
			if (pushOff != null)
			{
				surfaceIndex = pushOff.GetWallSoundIndex(this, dir);
			}
			if (dir == -1)
			{
				Dust.Burst(base.Center + Vector2.UnitX * 2f, (float)Math.PI * -3f / 4f, 4, DustParticleFromSurfaceIndex(surfaceIndex));
			}
			else
			{
				Dust.Burst(base.Center + Vector2.UnitX * -2f, -(float)Math.PI / 4f, 4, DustParticleFromSurfaceIndex(surfaceIndex));
			}
			SaveData.Instance.TotalWallJumps++;
		}

		private void ClimbJump()
		{
			if (!onGround)
			{
				Stamina -= 27.5f;
				sweatSprite.Play("jump", restart: true);
				Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
			}
			dreamJump = false;
			Jump(particles: false, playSfx: false);
			if (moveX == 0)
			{
				wallBoostDir = 0 - Facing;
				wallBoostTimer = 0.2f;
			}
			int surfaceIndex = -1;
			Platform pushOff = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position - Vector2.UnitX * (float)Facing * 4f, temp));
			if (pushOff != null)
			{
				surfaceIndex = pushOff.GetWallSoundIndex(this, (int)Facing);
			}
			if (Facing == Facings.Right)
			{
				Play("event:/char/madeline/jump_climb_right");
				Dust.Burst(base.Center + Vector2.UnitX * 2f, (float)Math.PI * -3f / 4f, 4, DustParticleFromSurfaceIndex(surfaceIndex));
			}
			else
			{
				Play("event:/char/madeline/jump_climb_left");
				Dust.Burst(base.Center + Vector2.UnitX * -2f, -(float)Math.PI / 4f, 4, DustParticleFromSurfaceIndex(surfaceIndex));
			}
		}

		public void Bounce(float fromY)
		{
			if (StateMachine.State == 4 && CurrentBooster != null)
			{
				CurrentBooster.PlayerReleased();
				CurrentBooster = null;
			}
			Collider was = base.Collider;
			base.Collider = normalHitbox;
			MoveVExact((int)(fromY - base.Bottom));
			if (!Inventory.NoRefills)
			{
				RefillDash();
			}
			RefillStamina();
			StateMachine.State = 0;
			jumpGraceTimer = 0f;
			varJumpTimer = 0.2f;
			AutoJump = true;
			AutoJumpTimer = 0.1f;
			dashAttackTimer = 0f;
			gliderBoostTimer = 0f;
			wallSlideTimer = 1.2f;
			wallBoostTimer = 0f;
			varJumpSpeed = (Speed.Y = -140f);
			launched = false;
			Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
			Sprite.Scale = new Vector2(0.6f, 1.4f);
			base.Collider = was;
		}

		public void SuperBounce(float fromY)
		{
			if (StateMachine.State == 4 && CurrentBooster != null)
			{
				CurrentBooster.PlayerReleased();
				CurrentBooster = null;
			}
			Collider was = base.Collider;
			base.Collider = normalHitbox;
			MoveV(fromY - base.Bottom);
			if (!Inventory.NoRefills)
			{
				RefillDash();
			}
			RefillStamina();
			StateMachine.State = 0;
			jumpGraceTimer = 0f;
			varJumpTimer = 0.2f;
			AutoJump = true;
			AutoJumpTimer = 0f;
			dashAttackTimer = 0f;
			gliderBoostTimer = 0f;
			wallSlideTimer = 1.2f;
			wallBoostTimer = 0f;
			Speed.X = 0f;
			varJumpSpeed = (Speed.Y = -185f);
			launched = false;
			level.DirectionalShake(-Vector2.UnitY, 0.1f);
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			Sprite.Scale = new Vector2(0.5f, 1.5f);
			base.Collider = was;
		}

		public bool SideBounce(int dir, float fromX, float fromY)
		{
			if (Math.Abs(Speed.X) > 240f && Math.Sign(Speed.X) == dir)
			{
				return false;
			}
			Collider was = base.Collider;
			base.Collider = normalHitbox;
			MoveV(Calc.Clamp(fromY - base.Bottom, -4f, 4f));
			if (dir > 0)
			{
				MoveH(fromX - base.Left);
			}
			else if (dir < 0)
			{
				MoveH(fromX - base.Right);
			}
			if (!Inventory.NoRefills)
			{
				RefillDash();
			}
			RefillStamina();
			StateMachine.State = 0;
			jumpGraceTimer = 0f;
			varJumpTimer = 0.2f;
			AutoJump = true;
			AutoJumpTimer = 0f;
			dashAttackTimer = 0f;
			gliderBoostTimer = 0f;
			wallSlideTimer = 1.2f;
			forceMoveX = dir;
			forceMoveXTimer = 0.3f;
			wallBoostTimer = 0f;
			launched = false;
			Speed.X = 240f * (float)dir;
			varJumpSpeed = (Speed.Y = -140f);
			level.DirectionalShake(Vector2.UnitX * dir, 0.1f);
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			Sprite.Scale = new Vector2(1.5f, 0.5f);
			base.Collider = was;
			return true;
		}

		public void Rebound(int direction = 0)
		{
			Speed.X = (float)direction * 120f;
			Speed.Y = -120f;
			varJumpSpeed = Speed.Y;
			varJumpTimer = 0.15f;
			AutoJump = true;
			AutoJumpTimer = 0f;
			dashAttackTimer = 0f;
			gliderBoostTimer = 0f;
			wallSlideTimer = 1.2f;
			wallBoostTimer = 0f;
			launched = false;
			lowFrictionStopTimer = 0.15f;
			forceMoveXTimer = 0f;
			StateMachine.State = 0;
		}

		public void ReflectBounce(Vector2 direction)
		{
			if (direction.X != 0f)
			{
				Speed.X = direction.X * 220f;
			}
			if (direction.Y != 0f)
			{
				Speed.Y = direction.Y * 220f;
			}
			AutoJumpTimer = 0f;
			dashAttackTimer = 0f;
			gliderBoostTimer = 0f;
			wallSlideTimer = 1.2f;
			wallBoostTimer = 0f;
			launched = false;
			dashAttackTimer = 0f;
			gliderBoostTimer = 0f;
			forceMoveXTimer = 0f;
			StateMachine.State = 0;
		}

		public bool RefillDash()
		{
			if (Dashes < MaxDashes)
			{
				Dashes = MaxDashes;
				return true;
			}
			return false;
		}

		public bool UseRefill(bool twoDashes)
		{
			int amount = MaxDashes;
			if (twoDashes)
			{
				amount = 2;
			}
			if (Dashes < amount || Stamina < 20f)
			{
				Dashes = amount;
				RefillStamina();
				return true;
			}
			return false;
		}

		public void RefillStamina()
		{
			Stamina = 110f;
		}

		public PlayerDeadBody Die(Vector2 direction, bool evenIfInvincible = false, bool registerDeathInStats = true)
		{
			Session session = level.Session;
			bool invincible = !evenIfInvincible && SaveData.Instance.Assists.Invincible;
			if (!Dead && !invincible && StateMachine.State != 18)
			{
				Stop(wallSlideSfx);
				if (registerDeathInStats)
				{
					session.Deaths++;
					session.DeathsInCurrentLevel++;
					SaveData.Instance.AddDeath(session.Area);
				}
				Strawberry goldenStrawb = null;
				foreach (Follower strawb in Leader.Followers)
				{
					if (strawb.Entity is Strawberry && (strawb.Entity as Strawberry).Golden && !(strawb.Entity as Strawberry).Winged)
					{
						goldenStrawb = strawb.Entity as Strawberry;
					}
				}
				Dead = true;
				Leader.LoseFollowers();
				base.Depth = -1000000;
				Speed = Vector2.Zero;
				StateMachine.Locked = true;
				Collidable = false;
				Drop();
				if (LastBooster != null)
				{
					LastBooster.PlayerDied();
				}
				level.InCutscene = false;
				level.Shake();
				Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
				PlayerDeadBody body = new PlayerDeadBody(this, direction);
				if (goldenStrawb != null)
				{
					body.HasGolden = true;
					body.DeathAction = delegate
					{
						Engine.Scene = new LevelExit(LevelExit.Mode.GoldenBerryRestart, session)
						{
							GoldenStrawberryEntryLevel = goldenStrawb.ID.Level
						};
					};
				}
				base.Scene.Add(body);
				base.Scene.Remove(this);
				base.Scene.Tracker.GetEntity<Lookout>()?.StopInteracting();
				return body;
			}
			return null;
		}

		public bool CanUnDuckAt(Vector2 at)
		{
			Vector2 was = Position;
			Position = at;
			bool canUnDuck = CanUnDuck;
			Position = was;
			return canUnDuck;
		}

		public bool DuckFreeAt(Vector2 at)
		{
			Vector2 oldP = Position;
			Collider oldC = base.Collider;
			Position = at;
			base.Collider = duckHitbox;
			bool result = !CollideCheck<Solid>();
			Position = oldP;
			base.Collider = oldC;
			return result;
		}

		private void Duck()
		{
			base.Collider = duckHitbox;
		}

		private void UnDuck()
		{
			base.Collider = normalHitbox;
		}

		public void UpdateCarry()
		{
			if (Holding != null)
			{
				if (Holding.Scene == null)
				{
					Holding = null;
				}
				else
				{
					Holding.Carry(Position + carryOffset + Vector2.UnitY * Sprite.CarryYOffset);
				}
			}
		}

		public void Swat(int dir)
		{
			if (Holding != null)
			{
				Holding.Release(new Vector2(0.8f * (float)dir, -0.25f));
				Holding = null;
			}
		}

		private bool Pickup(Holdable pickup)
		{
			if (pickup.Pickup(this))
			{
				Ducking = false;
				Holding = pickup;
				minHoldTimer = 0.35f;
				return true;
			}
			return false;
		}

		public void Throw()
		{
			if (Holding != null)
			{
				if (Input.MoveY.Value == 1)
				{
					Drop();
				}
				else
				{
					Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
					Holding.Release(Vector2.UnitX * (float)Facing);
					Speed.X += 80f * (float)(0 - Facing);
					Play("event:/char/madeline/crystaltheo_throw");
					Sprite.Play("throw");
				}
				Holding = null;
			}
		}

		public void Drop()
		{
			if (Holding != null)
			{
				Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
				Holding.Release(Vector2.Zero);
				Holding = null;
			}
		}

		public void StartJumpGraceTime()
		{
			jumpGraceTimer = 0.1f;
		}

		public override bool IsRiding(Solid solid)
		{
			if (StateMachine.State == 23)
			{
				return false;
			}
			if (StateMachine.State == 9)
			{
				return CollideCheck(solid);
			}
			if (StateMachine.State == 1 || StateMachine.State == 6)
			{
				return CollideCheck(solid, Position + Vector2.UnitX * (float)Facing);
			}
			if (climbTriggerDir != 0)
			{
				return CollideCheck(solid, Position + Vector2.UnitX * climbTriggerDir);
			}
			return base.IsRiding(solid);
		}

		public override bool IsRiding(JumpThru jumpThru)
		{
			if (StateMachine.State == 9)
			{
				return false;
			}
			if (StateMachine.State != 1 && Speed.Y >= 0f)
			{
				return base.IsRiding(jumpThru);
			}
			return false;
		}

		public bool BounceCheck(float y)
		{
			return base.Bottom <= y + 3f;
		}

		public void PointBounce(Vector2 from)
		{
			if (StateMachine.State == 2)
			{
				StateMachine.State = 0;
			}
			if (StateMachine.State == 4 && CurrentBooster != null)
			{
				CurrentBooster.PlayerReleased();
			}
			RefillDash();
			RefillStamina();
			Vector2 vec = (base.Center - from).SafeNormalize();
			if (vec.Y > -0.2f && vec.Y <= 0.4f)
			{
				vec.Y = -0.2f;
			}
			Speed = vec * 220f;
			Speed.X *= 1.5f;
			if (Math.Abs(Speed.X) < 100f)
			{
				if (Speed.X == 0f)
				{
					Speed.X = (float)(0 - Facing) * 100f;
				}
				else
				{
					Speed.X = (float)Math.Sign(Speed.X) * 100f;
				}
			}
		}

		private void WindMove(Vector2 move)
		{
			if (JustRespawned || !(noWindTimer <= 0f) || !InControl || StateMachine.State == 4 || StateMachine.State == 2 || StateMachine.State == 10)
			{
				return;
			}
			if (move.X != 0f && StateMachine.State != 1)
			{
				windTimeout = 0.2f;
				windDirection.X = Math.Sign(move.X);
				if (!CollideCheck<Solid>(Position + Vector2.UnitX * -Math.Sign(move.X) * 3f))
				{
					if (Ducking && onGround)
					{
						move.X *= 0f;
					}
					if (move.X < 0f)
					{
						move.X = Math.Max(move.X, (float)level.Bounds.Left - (base.ExactPosition.X + base.Collider.Left));
					}
					else
					{
						move.X = Math.Min(move.X, (float)level.Bounds.Right - (base.ExactPosition.X + base.Collider.Right));
					}
					MoveH(move.X);
				}
			}
			if (move.Y == 0f)
			{
				return;
			}
			windTimeout = 0.2f;
			windDirection.Y = Math.Sign(move.Y);
			if (!(base.Bottom > (float)level.Bounds.Top) || (!(Speed.Y < 0f) && OnGround()))
			{
				return;
			}
			if (StateMachine.State == 1)
			{
				if (!(move.Y > 0f) || !(climbNoMoveTimer <= 0f))
				{
					return;
				}
				move.Y *= 0.4f;
			}
			if (move.Y < 0f)
			{
				windMovedUp = true;
			}
			MoveV(move.Y);
		}

		private void OnCollideH(CollisionData data)
		{
			canCurveDash = false;
			if (StateMachine.State == 19)
			{
				if (starFlyTimer < 0.2f)
				{
					Speed.X = 0f;
					return;
				}
				Play("event:/game/06_reflection/feather_state_bump");
				Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
				Speed.X *= -0.5f;
			}
			else
			{
				if (StateMachine.State == 9)
				{
					return;
				}
				if (DashAttacking && data.Hit != null && data.Hit.OnDashCollide != null && data.Direction.X == (float)Math.Sign(DashDir.X))
				{
					DashCollisionResults result = data.Hit.OnDashCollide(this, data.Direction);
					if (result == DashCollisionResults.NormalOverride)
					{
						result = DashCollisionResults.NormalCollision;
					}
					else if (StateMachine.State == 5)
					{
						result = DashCollisionResults.Ignore;
					}
					switch (result)
					{
					case DashCollisionResults.Rebound:
						Rebound(-Math.Sign(Speed.X));
						return;
					case DashCollisionResults.Bounce:
						ReflectBounce(new Vector2(-Math.Sign(Speed.X), 0f));
						return;
					case DashCollisionResults.Ignore:
						return;
					}
				}
				if (StateMachine.State == 2 || StateMachine.State == 5)
				{
					if (onGround && DuckFreeAt(Position + Vector2.UnitX * Math.Sign(Speed.X)))
					{
						Ducking = true;
						return;
					}
					if (Speed.Y == 0f && Speed.X != 0f)
					{
						for (int i = 1; i <= 4; i++)
						{
							for (int j = 1; j >= -1; j -= 2)
							{
								Vector2 correct_add = new Vector2(Math.Sign(Speed.X), i * j);
								Vector2 pos = Position + correct_add;
								if (!CollideCheck<Solid>(pos) && CollideCheck<Solid>(pos - Vector2.UnitY * j) && !DashCorrectCheck(correct_add))
								{
									MoveVExact(i * j);
									MoveHExact(Math.Sign(Speed.X));
									return;
								}
							}
						}
					}
				}
				if (DreamDashCheck(Vector2.UnitX * Math.Sign(Speed.X)))
				{
					StateMachine.State = 9;
					dashAttackTimer = 0f;
					gliderBoostTimer = 0f;
					return;
				}
				if (wallSpeedRetentionTimer <= 0f)
				{
					wallSpeedRetained = Speed.X;
					wallSpeedRetentionTimer = 0.06f;
				}
				if (data.Hit != null && data.Hit.OnCollide != null)
				{
					data.Hit.OnCollide(data.Direction);
				}
				Speed.X = 0f;
				dashAttackTimer = 0f;
				gliderBoostTimer = 0f;
				if (StateMachine.State == 5)
				{
					Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
					level.Displacement.AddBurst(base.Center, 0.5f, 8f, 48f, 0.4f, Ease.QuadOut, Ease.QuadOut);
					StateMachine.State = 6;
				}
			}
		}

		private void OnCollideV(CollisionData data)
		{
			canCurveDash = false;
			if (StateMachine.State == 19)
			{
				if (starFlyTimer < 0.2f)
				{
					Speed.Y = 0f;
					return;
				}
				Play("event:/game/06_reflection/feather_state_bump");
				Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
				Speed.Y *= -0.5f;
			}
			else if (StateMachine.State == 3)
			{
				Speed.Y = 0f;
			}
			else
			{
				if (StateMachine.State == 9)
				{
					return;
				}
				if (data.Hit != null && data.Hit.OnDashCollide != null)
				{
					if (DashAttacking && data.Direction.Y == (float)Math.Sign(DashDir.Y))
					{
						DashCollisionResults result = data.Hit.OnDashCollide(this, data.Direction);
						if (StateMachine.State == 5)
						{
							result = DashCollisionResults.Ignore;
						}
						switch (result)
						{
						case DashCollisionResults.Rebound:
							Rebound();
							return;
						case DashCollisionResults.Bounce:
							ReflectBounce(new Vector2(0f, -Math.Sign(Speed.Y)));
							return;
						case DashCollisionResults.Ignore:
							return;
						}
					}
					else if (StateMachine.State == 10)
					{
						data.Hit.OnDashCollide(this, data.Direction);
						return;
					}
				}
				if (Speed.Y > 0f)
				{
					if ((StateMachine.State == 2 || StateMachine.State == 5) && !dashStartedOnGround)
					{
						if (Speed.X <= 0.01f)
						{
							for (int l = -1; l >= -4; l--)
							{
								if (!OnGround(Position + new Vector2(l, 0f)))
								{
									MoveHExact(l);
									MoveVExact(1);
									return;
								}
							}
						}
						if (Speed.X >= -0.01f)
						{
							for (int k = 1; k <= 4; k++)
							{
								if (!OnGround(Position + new Vector2(k, 0f)))
								{
									MoveHExact(k);
									MoveVExact(1);
									return;
								}
							}
						}
					}
					if (DreamDashCheck(Vector2.UnitY * Math.Sign(Speed.Y)))
					{
						StateMachine.State = 9;
						dashAttackTimer = 0f;
						gliderBoostTimer = 0f;
						return;
					}
					if (DashDir.X != 0f && DashDir.Y > 0f && Speed.Y > 0f)
					{
						DashDir.X = Math.Sign(DashDir.X);
						DashDir.Y = 0f;
						Speed.Y = 0f;
						Speed.X *= 1.2f;
						Ducking = true;
					}
					if (StateMachine.State != 1)
					{
						float squish = Math.Min(Speed.Y / 240f, 1f);
						Sprite.Scale.X = MathHelper.Lerp(1f, 1.6f, squish);
						Sprite.Scale.Y = MathHelper.Lerp(1f, 0.4f, squish);
						if (highestAirY < base.Y - 50f && Speed.Y >= 160f && Math.Abs(Speed.X) >= 90f)
						{
							Sprite.Play("runStumble");
						}
						Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
						Platform platform = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position + new Vector2(0f, 1f), temp));
						int surfaceIndex = -1;
						if (platform != null)
						{
							surfaceIndex = platform.GetLandSoundIndex(this);
							if (surfaceIndex >= 0 && !MuffleLanding)
							{
								Play((playFootstepOnLand > 0f) ? "event:/char/madeline/footstep" : "event:/char/madeline/landing", "surface_index", surfaceIndex);
							}
							if (platform is DreamBlock)
							{
								(platform as DreamBlock).FootstepRipple(Position);
							}
							MuffleLanding = false;
						}
						if (Speed.Y >= 80f)
						{
							Dust.Burst(Position, new Vector2(0f, -1f).Angle(), 8, DustParticleFromSurfaceIndex(surfaceIndex));
						}
						playFootstepOnLand = 0f;
					}
				}
				else
				{
					if (Speed.Y < 0f)
					{
						int maxCorrect = 4;
						if (DashAttacking && Math.Abs(Speed.X) < 0.01f)
						{
							maxCorrect = 5;
						}
						if (Speed.X <= 0.01f)
						{
							for (int j = 1; j <= maxCorrect; j++)
							{
								if (!CollideCheck<Solid>(Position + new Vector2(-j, -1f)))
								{
									Position += new Vector2(-j, -1f);
									return;
								}
							}
						}
						if (Speed.X >= -0.01f)
						{
							for (int i = 1; i <= maxCorrect; i++)
							{
								if (!CollideCheck<Solid>(Position + new Vector2(i, -1f)))
								{
									Position += new Vector2(i, -1f);
									return;
								}
							}
						}
						if (varJumpTimer < 0.15f)
						{
							varJumpTimer = 0f;
						}
					}
					if (DreamDashCheck(Vector2.UnitY * Math.Sign(Speed.Y)))
					{
						StateMachine.State = 9;
						dashAttackTimer = 0f;
						gliderBoostTimer = 0f;
						return;
					}
				}
				if (data.Hit != null && data.Hit.OnCollide != null)
				{
					data.Hit.OnCollide(data.Direction);
				}
				dashAttackTimer = 0f;
				gliderBoostTimer = 0f;
				Speed.Y = 0f;
				if (StateMachine.State == 5)
				{
					Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
					level.Displacement.AddBurst(base.Center, 0.5f, 8f, 48f, 0.4f, Ease.QuadOut, Ease.QuadOut);
					StateMachine.State = 6;
				}
			}
		}

		private bool DreamDashCheck(Vector2 dir)
		{
			if (Inventory.DreamDash && DashAttacking && (dir.X == (float)Math.Sign(DashDir.X) || dir.Y == (float)Math.Sign(DashDir.Y)))
			{
				DreamBlock block = CollideFirst<DreamBlock>(Position + dir);
				if (block != null)
				{
					if (CollideCheck<Solid, DreamBlock>(Position + dir))
					{
						Vector2 side = new Vector2(Math.Abs(dir.Y), Math.Abs(dir.X));
						bool checkNegative;
						bool checkPositive;
						if (dir.X != 0f)
						{
							checkNegative = Speed.Y <= 0f;
							checkPositive = Speed.Y >= 0f;
						}
						else
						{
							checkNegative = Speed.X <= 0f;
							checkPositive = Speed.X >= 0f;
						}
						if (checkNegative)
						{
							for (int j = -1; j >= -4; j--)
							{
								Vector2 at2 = Position + dir + side * j;
								if (!CollideCheck<Solid, DreamBlock>(at2))
								{
									Position += side * j;
									dreamBlock = block;
									return true;
								}
							}
						}
						if (checkPositive)
						{
							for (int i = 1; i <= 4; i++)
							{
								Vector2 at = Position + dir + side * i;
								if (!CollideCheck<Solid, DreamBlock>(at))
								{
									Position += side * i;
									dreamBlock = block;
									return true;
								}
							}
						}
						return false;
					}
					dreamBlock = block;
					return true;
				}
			}
			return false;
		}

		public void OnBoundsH()
		{
			Speed.X = 0f;
			if (StateMachine.State == 5)
			{
				StateMachine.State = 0;
			}
		}

		public void OnBoundsV()
		{
			Speed.Y = 0f;
			if (StateMachine.State == 5)
			{
				StateMachine.State = 0;
			}
		}

		protected override void OnSquish(CollisionData data)
		{
			bool ducked = false;
			if (!Ducking && StateMachine.State != 1)
			{
				ducked = true;
				Ducking = true;
				data.Pusher.Collidable = true;
				if (!CollideCheck<Solid>())
				{
					data.Pusher.Collidable = false;
					return;
				}
				Vector2 was = Position;
				Position = data.TargetPosition;
				if (!CollideCheck<Solid>())
				{
					data.Pusher.Collidable = false;
					return;
				}
				Position = was;
				data.Pusher.Collidable = false;
			}
			if (!TrySquishWiggle(data, 3, 5))
			{
				bool forceKill = false;
				if (data.Pusher != null && data.Pusher.SquishEvenInAssistMode)
				{
					forceKill = true;
				}
				Die(Vector2.Zero, forceKill);
			}
			else if (ducked && CanUnDuck)
			{
				Ducking = false;
			}
		}

		private void NormalBegin()
		{
			maxFall = 160f;
		}

		private void NormalEnd()
		{
			wallBoostTimer = 0f;
			wallSpeedRetentionTimer = 0f;
			hopWaitX = 0;
		}

		public bool ClimbBoundsCheck(int dir)
		{
			if (base.Left + (float)(dir * 2) >= (float)level.Bounds.Left)
			{
				return base.Right + (float)(dir * 2) < (float)level.Bounds.Right;
			}
			return false;
		}

		public void ClimbTrigger(int dir)
		{
			climbTriggerDir = dir;
		}

		public bool ClimbCheck(int dir, int yAdd = 0)
		{
			if (ClimbBoundsCheck(dir) && !ClimbBlocker.Check(base.Scene, this, Position + Vector2.UnitY * yAdd + Vector2.UnitX * 2f * (float)Facing))
			{
				return CollideCheck<Solid>(Position + new Vector2(dir * 2, yAdd));
			}
			return false;
		}

		private int NormalUpdate()
		{
			if (LiftBoost.Y < 0f && wasOnGround && !onGround && Speed.Y >= 0f)
			{
				Speed.Y = LiftBoost.Y;
			}
			if (Holding == null)
			{
				if (Input.GrabCheck && !IsTired && !Ducking)
				{
					foreach (Holdable hold in base.Scene.Tracker.GetComponents<Holdable>())
					{
						if (hold.Check(this) && Pickup(hold))
						{
							return 8;
						}
					}
					if (Speed.Y >= 0f && Math.Sign(Speed.X) != 0 - Facing)
					{
						if (ClimbCheck((int)Facing))
						{
							Ducking = false;
							if (!SaveData.Instance.Assists.NoGrabbing)
							{
								return 1;
							}
							ClimbTrigger((int)Facing);
						}
						if (!SaveData.Instance.Assists.NoGrabbing && (float)Input.MoveY < 1f && level.Wind.Y <= 0f)
						{
							for (int j = 1; j <= 2; j++)
							{
								if (!CollideCheck<Solid>(Position + Vector2.UnitY * -j) && ClimbCheck((int)Facing, -j))
								{
									MoveVExact(-j);
									Ducking = false;
									return 1;
								}
							}
						}
					}
				}
				if (CanDash)
				{
					Speed += LiftBoost;
					return StartDash();
				}
				if (Ducking)
				{
					if (onGround && (float)Input.MoveY != 1f)
					{
						if (CanUnDuck)
						{
							Ducking = false;
							Sprite.Scale = new Vector2(0.8f, 1.2f);
						}
						else if (Speed.X == 0f)
						{
							for (int i = 4; i > 0; i--)
							{
								if (CanUnDuckAt(Position + Vector2.UnitX * i))
								{
									MoveH(50f * Engine.DeltaTime);
									break;
								}
								if (CanUnDuckAt(Position - Vector2.UnitX * i))
								{
									MoveH(-50f * Engine.DeltaTime);
									break;
								}
							}
						}
					}
				}
				else if (onGround && (float)Input.MoveY == 1f && Speed.Y >= 0f)
				{
					Ducking = true;
					Sprite.Scale = new Vector2(1.4f, 0.6f);
				}
			}
			else
			{
				if (!Input.GrabCheck && minHoldTimer <= 0f)
				{
					Throw();
				}
				if (!Ducking && onGround && (float)Input.MoveY == 1f && Speed.Y >= 0f && !holdCannotDuck)
				{
					Drop();
					Ducking = true;
					Sprite.Scale = new Vector2(1.4f, 0.6f);
				}
				else if (onGround && Ducking && Speed.Y >= 0f)
				{
					if (CanUnDuck)
					{
						Ducking = false;
					}
					else
					{
						Drop();
					}
				}
				else if (onGround && (float)Input.MoveY != 1f && holdCannotDuck)
				{
					holdCannotDuck = false;
				}
			}
			if (Ducking && onGround)
			{
				Speed.X = Calc.Approach(Speed.X, 0f, 500f * Engine.DeltaTime);
			}
			else
			{
				float mult2 = (onGround ? 1f : 0.65f);
				if (onGround && level.CoreMode == Session.CoreModes.Cold)
				{
					mult2 *= 0.3f;
				}
				if (SaveData.Instance.Assists.LowFriction && lowFrictionStopTimer <= 0f)
				{
					mult2 *= (onGround ? 0.35f : 0.5f);
				}
				float max2;
				if (Holding != null && Holding.SlowRun)
				{
					max2 = 70f;
				}
				else if (Holding != null && Holding.SlowFall && !onGround)
				{
					max2 = 108.00001f;
					mult2 *= 0.5f;
				}
				else
				{
					max2 = 90f;
				}
				if (level.InSpace)
				{
					max2 *= 0.6f;
				}
				if (Math.Abs(Speed.X) > max2 && Math.Sign(Speed.X) == moveX)
				{
					Speed.X = Calc.Approach(Speed.X, max2 * (float)moveX, 400f * mult2 * Engine.DeltaTime);
				}
				else
				{
					Speed.X = Calc.Approach(Speed.X, max2 * (float)moveX, 1000f * mult2 * Engine.DeltaTime);
				}
			}
			float mf = 160f;
			float fmf = 240f;
			if (level.InSpace)
			{
				mf *= 0.6f;
				fmf *= 0.6f;
			}
			if (Holding != null && Holding.SlowFall && forceMoveXTimer <= 0f)
			{
				maxFall = Calc.Approach(target: ((float)Input.GliderMoveY == 1f) ? 120f : ((windMovedUp && (float)Input.GliderMoveY == -1f) ? (-32f) : (((float)Input.GliderMoveY == -1f) ? 24f : ((!windMovedUp) ? 40f : 0f))), val: maxFall, maxMove: 300f * Engine.DeltaTime);
			}
			else if ((float)Input.MoveY == 1f && Speed.Y >= mf)
			{
				maxFall = Calc.Approach(maxFall, fmf, 300f * Engine.DeltaTime);
				float half = mf + (fmf - mf) * 0.5f;
				if (Speed.Y >= half)
				{
					float spriteLerp = Math.Min(1f, (Speed.Y - half) / (fmf - half));
					Sprite.Scale.X = MathHelper.Lerp(1f, 0.5f, spriteLerp);
					Sprite.Scale.Y = MathHelper.Lerp(1f, 1.5f, spriteLerp);
				}
			}
			else
			{
				maxFall = Calc.Approach(maxFall, mf, 300f * Engine.DeltaTime);
			}
			if (!onGround)
			{
				float max = maxFall;
				if (Holding != null && Holding.SlowFall)
				{
					holdCannotDuck = (float)Input.MoveY == 1f;
				}
				if ((moveX == (int)Facing || (moveX == 0 && Input.GrabCheck)) && Input.MoveY.Value != 1)
				{
					if (Speed.Y >= 0f && wallSlideTimer > 0f && Holding == null && ClimbBoundsCheck((int)Facing) && CollideCheck<Solid>(Position + Vector2.UnitX * (float)Facing) && !ClimbBlocker.EdgeCheck(level, this, (int)Facing) && CanUnDuck)
					{
						Ducking = false;
						wallSlideDir = (int)Facing;
					}
					if (wallSlideDir != 0)
					{
						if (Input.GrabCheck)
						{
							ClimbTrigger(wallSlideDir);
						}
						if (wallSlideTimer > 0.6f && ClimbBlocker.Check(level, this, Position + Vector2.UnitX * wallSlideDir))
						{
							wallSlideTimer = 0.6f;
						}
						max = MathHelper.Lerp(160f, 20f, wallSlideTimer / 1.2f);
						if (wallSlideTimer / 1.2f > 0.65f)
						{
							CreateWallSlideParticles(wallSlideDir);
						}
					}
				}
				float mult = ((Math.Abs(Speed.Y) < 40f && (Input.Jump.Check || AutoJump)) ? 0.5f : 1f);
				if (Holding != null && Holding.SlowFall && forceMoveXTimer <= 0f)
				{
					mult *= 0.5f;
				}
				if (level.InSpace)
				{
					mult *= 0.6f;
				}
				Speed.Y = Calc.Approach(Speed.Y, max, 900f * mult * Engine.DeltaTime);
			}
			if (varJumpTimer > 0f)
			{
				if (AutoJump || Input.Jump.Check)
				{
					Speed.Y = Math.Min(Speed.Y, varJumpSpeed);
				}
				else
				{
					varJumpTimer = 0f;
				}
			}
			if (Input.Jump.Pressed && (TalkComponent.PlayerOver == null || !Input.Talk.Pressed))
			{
				Water water = null;
				if (jumpGraceTimer > 0f)
				{
					Jump();
				}
				else if (CanUnDuck)
				{
					bool canUnduck = CanUnDuck;
					if (canUnduck && WallJumpCheck(1))
					{
						if (Facing == Facings.Right && Input.GrabCheck && !SaveData.Instance.Assists.NoGrabbing && Stamina > 0f && Holding == null && !ClimbBlocker.Check(base.Scene, this, Position + Vector2.UnitX * 3f))
						{
							ClimbJump();
						}
						else if (DashAttacking && SuperWallJumpAngleCheck)
						{
							SuperWallJump(-1);
						}
						else
						{
							WallJump(-1);
						}
					}
					else if (canUnduck && WallJumpCheck(-1))
					{
						if (Facing == Facings.Left && Input.GrabCheck && !SaveData.Instance.Assists.NoGrabbing && Stamina > 0f && Holding == null && !ClimbBlocker.Check(base.Scene, this, Position + Vector2.UnitX * -3f))
						{
							ClimbJump();
						}
						else if (DashAttacking && SuperWallJumpAngleCheck)
						{
							SuperWallJump(1);
						}
						else
						{
							WallJump(1);
						}
					}
					else if ((water = CollideFirst<Water>(Position + Vector2.UnitY * 2f)) != null)
					{
						Jump();
						water.TopSurface.DoRipple(Position, 1f);
					}
				}
			}
			return 0;
		}

		public void CreateWallSlideParticles(int dir)
		{
			if (base.Scene.OnInterval(0.01f))
			{
				int surfaceIndex = -1;
				Platform pushOff = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position + Vector2.UnitX * dir * 4f, temp));
				if (pushOff != null)
				{
					surfaceIndex = pushOff.GetWallSoundIndex(this, dir);
				}
				ParticleType particle = DustParticleFromSurfaceIndex(surfaceIndex);
				float push = ((particle == ParticleTypes.Dust) ? 5f : 2f);
				Vector2 at = base.Center;
				if (dir == 1)
				{
					at += new Vector2(push, 4f);
				}
				else
				{
					at += new Vector2(0f - push, 4f);
				}
				Dust.Burst(at, -(float)Math.PI / 2f, 1, particle);
			}
		}

		private void PlaySweatEffectDangerOverride(string state)
		{
			if (Stamina <= 20f)
			{
				sweatSprite.Play("danger");
			}
			else
			{
				sweatSprite.Play(state);
			}
		}

		private void ClimbBegin()
		{
			AutoJump = false;
			Speed.X = 0f;
			Speed.Y *= 0.2f;
			wallSlideTimer = 1.2f;
			climbNoMoveTimer = 0.1f;
			wallBoostTimer = 0f;
			lastClimbMove = 0;
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
			for (int i = 0; i < 2; i++)
			{
				if (CollideCheck<Solid>(Position + Vector2.UnitX * (float)Facing))
				{
					break;
				}
				Position += Vector2.UnitX * (float)Facing;
			}
			Platform platform = SurfaceIndex.GetPlatformByPriority(CollideAll<Solid>(Position + Vector2.UnitX * (float)Facing, temp));
			if (platform != null)
			{
				Play("event:/char/madeline/grab", "surface_index", platform.GetWallSoundIndex(this, (int)Facing));
				if (platform is DreamBlock)
				{
					(platform as DreamBlock).FootstepRipple(Position + new Vector2((int)Facing * 3, -4f));
				}
			}
		}

		private void ClimbEnd()
		{
			if (conveyorLoopSfx != null)
			{
				conveyorLoopSfx.Value.setParameterByName("end", 1f);
				conveyorLoopSfx.Value.release();
				conveyorLoopSfx = null;
			}
			wallSpeedRetentionTimer = 0f;
			if (sweatSprite != null && sweatSprite.CurrentAnimationID != "jump")
			{
				sweatSprite.Play("idle");
			}
		}

		private int ClimbUpdate()
		{
			climbNoMoveTimer -= Engine.DeltaTime;
			if (onGround)
			{
				Stamina = 110f;
			}
			if (Input.Jump.Pressed && (!Ducking || CanUnDuck))
			{
				if (moveX == 0 - Facing)
				{
					WallJump(0 - Facing);
				}
				else
				{
					ClimbJump();
				}
				return 0;
			}
			if (CanDash)
			{
				Speed += LiftBoost;
				return StartDash();
			}
			if (!Input.GrabCheck)
			{
				Speed += LiftBoost;
				Play("event:/char/madeline/grab_letgo");
				return 0;
			}
			if (!CollideCheck<Solid>(Position + Vector2.UnitX * (float)Facing))
			{
				if (Speed.Y < 0f)
				{
					if (wallBoosting)
					{
						Speed += LiftBoost;
						Play("event:/char/madeline/grab_letgo");
					}
					else
					{
						ClimbHop();
					}
				}
				return 0;
			}
			WallBooster booster = WallBoosterCheck();
			if (climbNoMoveTimer <= 0f && booster != null)
			{
				wallBoosting = true;
				if (conveyorLoopSfx == null)
				{
					conveyorLoopSfx = Audio.Play("event:/game/09_core/conveyor_activate", Position, "end", 0f);
				}
				Audio.Position(conveyorLoopSfx, Position);
				Speed.Y = Calc.Approach(Speed.Y, -160f, 600f * Engine.DeltaTime);
				base.LiftSpeed = Vector2.UnitY * Math.Max(Speed.Y, -80f);
				Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
			}
			else
			{
				wallBoosting = false;
				if (conveyorLoopSfx != null)
				{
					conveyorLoopSfx.Value.setParameterByName("end", 1f);
					conveyorLoopSfx.Value.release();
					conveyorLoopSfx = null;
				}
				float target = 0f;
				bool trySlip = false;
				if (climbNoMoveTimer <= 0f)
				{
					if (ClimbBlocker.Check(base.Scene, this, Position + Vector2.UnitX * (float)Facing))
					{
						trySlip = true;
					}
					else if (Input.MoveY.Value == -1)
					{
						target = -45f;
						if (CollideCheck<Solid>(Position - Vector2.UnitY) || (ClimbHopBlockedCheck() && SlipCheck(-1f)))
						{
							if (Speed.Y < 0f)
							{
								Speed.Y = 0f;
							}
							target = 0f;
							trySlip = true;
						}
						else if (SlipCheck())
						{
							ClimbHop();
							return 0;
						}
					}
					else if (Input.MoveY.Value == 1)
					{
						target = 80f;
						if (onGround)
						{
							if (Speed.Y > 0f)
							{
								Speed.Y = 0f;
							}
							target = 0f;
						}
						else
						{
							CreateWallSlideParticles((int)Facing);
						}
					}
					else
					{
						trySlip = true;
					}
				}
				else
				{
					trySlip = true;
				}
				lastClimbMove = Math.Sign(target);
				if (trySlip && SlipCheck())
				{
					target = 30f;
				}
				Speed.Y = Calc.Approach(Speed.Y, target, 900f * Engine.DeltaTime);
			}
			if (Input.MoveY.Value != 1 && Speed.Y > 0f && !CollideCheck<Solid>(Position + new Vector2((float)Facing, 1f)))
			{
				Speed.Y = 0f;
			}
			if (climbNoMoveTimer <= 0f)
			{
				if (lastClimbMove == -1)
				{
					Stamina -= 45.454544f * Engine.DeltaTime;
					if (Stamina <= 20f)
					{
						sweatSprite.Play("danger");
					}
					else if (sweatSprite.CurrentAnimationID != "climbLoop")
					{
						sweatSprite.Play("climb");
					}
					if (base.Scene.OnInterval(0.2f))
					{
						Input.Rumble(RumbleStrength.Climb, RumbleLength.Short);
					}
				}
				else
				{
					if (lastClimbMove == 0)
					{
						Stamina -= 10f * Engine.DeltaTime;
					}
					if (!onGround)
					{
						PlaySweatEffectDangerOverride("still");
						if (base.Scene.OnInterval(0.8f))
						{
							Input.Rumble(RumbleStrength.Climb, RumbleLength.Short);
						}
					}
					else
					{
						PlaySweatEffectDangerOverride("idle");
					}
				}
			}
			else
			{
				PlaySweatEffectDangerOverride("idle");
			}
			if (Stamina <= 0f)
			{
				Speed += LiftBoost;
				return 0;
			}
			return 1;
		}

		private WallBooster WallBoosterCheck()
		{
			if (ClimbBlocker.Check(base.Scene, this, Position + Vector2.UnitX * (float)Facing))
			{
				return null;
			}
			foreach (WallBooster booster in base.Scene.Tracker.GetEntities<WallBooster>())
			{
				if (booster.Facing == Facing && CollideCheck(booster))
				{
					return booster;
				}
			}
			return null;
		}

		private void ClimbHop()
		{
			climbHopSolid = CollideFirst<Solid>(Position + Vector2.UnitX * (float)Facing);
			playFootstepOnLand = 0.5f;
			if (climbHopSolid != null)
			{
				climbHopSolidPosition = climbHopSolid.Position;
				hopWaitX = (int)Facing;
				hopWaitXSpeed = (float)Facing * 100f;
			}
			else
			{
				hopWaitX = 0;
				Speed.X = (float)Facing * 100f;
			}
			lowFrictionStopTimer = 0.15f;
			Speed.Y = Math.Min(Speed.Y, -120f);
			forceMoveX = 0;
			forceMoveXTimer = 0.2f;
			fastJump = false;
			noWindTimer = 0.3f;
			Play("event:/char/madeline/climb_ledge");
		}

		private bool SlipCheck(float addY = 0f)
		{
			Vector2 at = ((Facing != Facings.Right) ? (base.TopLeft - Vector2.UnitX + Vector2.UnitY * (4f + addY)) : (base.TopRight + Vector2.UnitY * (4f + addY)));
			if (!base.Scene.CollideCheck<Solid>(at))
			{
				return !base.Scene.CollideCheck<Solid>(at + Vector2.UnitY * (-4f + addY));
			}
			return false;
		}

		private bool ClimbHopBlockedCheck()
		{
			foreach (Follower follower in Leader.Followers)
			{
				if (follower.Entity is StrawberrySeed)
				{
					return true;
				}
			}
			foreach (LedgeBlocker component in base.Scene.Tracker.GetComponents<LedgeBlocker>())
			{
				if (component.HopBlockCheck(this))
				{
					return true;
				}
			}
			if (CollideCheck<Solid>(Position - Vector2.UnitY * 6f))
			{
				return true;
			}
			return false;
		}

		private bool JumpThruBoostBlockedCheck()
		{
			foreach (LedgeBlocker component in base.Scene.Tracker.GetComponents<LedgeBlocker>())
			{
				if (component.JumpThruBoostCheck(this))
				{
					return true;
				}
			}
			return false;
		}

		private bool DashCorrectCheck(Vector2 add)
		{
			Vector2 posWas = Position;
			Collider colWas = base.Collider;
			Position += add;
			base.Collider = hurtbox;
			foreach (LedgeBlocker component in base.Scene.Tracker.GetComponents<LedgeBlocker>())
			{
				if (component.DashCorrectCheck(this))
				{
					Position = posWas;
					base.Collider = colWas;
					return true;
				}
			}
			Position = posWas;
			base.Collider = colWas;
			return false;
		}

		public int StartDash()
		{
			wasDashB = Dashes == 2;
			Dashes = Math.Max(0, Dashes - 1);
			demoDashed = Input.CrouchDashPressed;
			Input.Dash.ConsumeBuffer();
			Input.CrouchDash.ConsumeBuffer();
			return 2;
		}

		private void CallDashEvents()
		{
			if (calledDashEvents)
			{
				return;
			}
			calledDashEvents = true;
			if (CurrentBooster == null)
			{
				SaveData.Instance.TotalDashes++;
				level.Session.Dashes++;
				Stats.Increment(Stat.DASHES);
				bool rightDashSound = DashDir.Y < 0f || (DashDir.Y == 0f && DashDir.X > 0f);
				if (DashDir == Vector2.Zero)
				{
					rightDashSound = Facing == Facings.Right;
				}
				if (rightDashSound)
				{
					if (wasDashB)
					{
						Play("event:/char/madeline/dash_pink_right");
					}
					else
					{
						Play("event:/char/madeline/dash_red_right");
					}
				}
				else if (wasDashB)
				{
					Play("event:/char/madeline/dash_pink_left");
				}
				else
				{
					Play("event:/char/madeline/dash_red_left");
				}
				if (SwimCheck())
				{
					Play("event:/char/madeline/water_dash_gen");
				}
				{
					foreach (DashListener dl in base.Scene.Tracker.GetComponents<DashListener>())
					{
						if (dl.OnDash != null)
						{
							dl.OnDash(DashDir);
						}
					}
					return;
				}
			}
			CurrentBooster.PlayerBoosted(this, DashDir);
			CurrentBooster = null;
		}

		private void DashBegin()
		{
			calledDashEvents = false;
			dashStartedOnGround = onGround;
			launched = false;
			canCurveDash = true;
			if (Engine.TimeRate > 0.25f)
			{
				Celeste.Freeze(0.05f);
			}
			dashCooldownTimer = 0.2f;
			dashRefillCooldownTimer = 0.1f;
			StartedDashing = true;
			wallSlideTimer = 1.2f;
			dashTrailTimer = 0f;
			dashTrailCounter = 0;
			if (!SaveData.Instance.Assists.DashAssist)
			{
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			}
			dashAttackTimer = 0.3f;
			gliderBoostTimer = 0.55f;
			if (SaveData.Instance.Assists.SuperDashing)
			{
				dashAttackTimer += 0.15f;
			}
			beforeDashSpeed = Speed;
			Speed = Vector2.Zero;
			DashDir = Vector2.Zero;
			if (!onGround && Ducking && CanUnDuck)
			{
				Ducking = false;
			}
			else if (!Ducking && (demoDashed || Input.MoveY.Value == 1))
			{
				Ducking = true;
			}
			DashAssistInit();
		}

		private void DashAssistInit()
		{
			if (SaveData.Instance.Assists.DashAssist && !demoDashed)
			{
				Input.LastAim = Vector2.UnitX * (float)Facing;
				Engine.DashAssistFreeze = true;
				Engine.DashAssistFreezePress = false;
				PlayerDashAssist dashAssist = base.Scene.Tracker.GetEntity<PlayerDashAssist>();
				if (dashAssist == null)
				{
					base.Scene.Add(dashAssist = new PlayerDashAssist());
				}
				dashAssist.Direction = Input.GetAimVector(Facing).Angle();
				dashAssist.Scale = 0f;
				dashAssist.Offset = ((CurrentBooster == null && StateMachine.PreviousState != 5) ? Vector2.Zero : new Vector2(0f, -4f));
			}
		}

		private void DashEnd()
		{
			CallDashEvents();
			demoDashed = false;
		}

		private int DashUpdate()
		{
			StartedDashing = false;
			if (dashTrailTimer > 0f)
			{
				dashTrailTimer -= Engine.DeltaTime;
				if (dashTrailTimer <= 0f)
				{
					CreateTrail();
					dashTrailCounter--;
					if (dashTrailCounter > 0)
					{
						dashTrailTimer = 0.1f;
					}
				}
			}
			if (SaveData.Instance.Assists.SuperDashing && canCurveDash && Input.Aim.Value != Vector2.Zero && Speed != Vector2.Zero)
			{
				Vector2 aim = Input.GetAimVector();
				aim = CorrectDashPrecision(aim);
				float dot = Vector2.Dot(aim, Speed.SafeNormalize());
				if (dot >= -0.1f && dot < 0.99f)
				{
					Speed = Speed.RotateTowards(aim.Angle(), 4.1887903f * Engine.DeltaTime);
					DashDir = Speed.SafeNormalize();
					DashDir = CorrectDashPrecision(DashDir);
				}
			}
			if (SaveData.Instance.Assists.SuperDashing && CanDash)
			{
				StartDash();
				StateMachine.ForceState(2);
				return 2;
			}
			if (Holding == null && DashDir != Vector2.Zero && Input.GrabCheck && !IsTired && CanUnDuck)
			{
				foreach (Holdable hold in base.Scene.Tracker.GetComponents<Holdable>())
				{
					if (hold.Check(this) && Pickup(hold))
					{
						return 8;
					}
				}
			}
			if (Math.Abs(DashDir.Y) < 0.1f)
			{
				foreach (JumpThru jt in base.Scene.Tracker.GetEntities<JumpThru>())
				{
					if (CollideCheck(jt) && base.Bottom - jt.Top <= 6f && !DashCorrectCheck(Vector2.UnitY * (jt.Top - base.Bottom)))
					{
						MoveVExact((int)(jt.Top - base.Bottom));
					}
				}
				if (CanUnDuck && Input.Jump.Pressed && jumpGraceTimer > 0f)
				{
					SuperJump();
					return 0;
				}
			}
			if (SuperWallJumpAngleCheck)
			{
				if (Input.Jump.Pressed && CanUnDuck)
				{
					if (WallJumpCheck(1))
					{
						SuperWallJump(-1);
						return 0;
					}
					if (WallJumpCheck(-1))
					{
						SuperWallJump(1);
						return 0;
					}
				}
			}
			else if (Input.Jump.Pressed && CanUnDuck)
			{
				if (WallJumpCheck(1))
				{
					if (Facing == Facings.Right && Input.GrabCheck && Stamina > 0f && Holding == null && !ClimbBlocker.Check(base.Scene, this, Position + Vector2.UnitX * 3f))
					{
						ClimbJump();
					}
					else
					{
						WallJump(-1);
					}
					return 0;
				}
				if (WallJumpCheck(-1))
				{
					if (Facing == Facings.Left && Input.GrabCheck && Stamina > 0f && Holding == null && !ClimbBlocker.Check(base.Scene, this, Position + Vector2.UnitX * -3f))
					{
						ClimbJump();
					}
					else
					{
						WallJump(1);
					}
					return 0;
				}
			}
			if (Speed != Vector2.Zero && level.OnInterval(0.02f))
			{
				ParticleType type = ((!wasDashB) ? P_DashA : ((Sprite.Mode != PlayerSpriteMode.MadelineAsBadeline) ? P_DashB : P_DashBadB));
				level.ParticlesFG.Emit(type, base.Center + Calc.Random.Range(Vector2.One * -2f, Vector2.One * 2f), DashDir.Angle());
			}
			return 2;
		}

		private Vector2 CorrectDashPrecision(Vector2 dir)
		{
			if (dir.X != 0f && Math.Abs(dir.X) < 0.001f)
			{
				dir.X = 0f;
				dir.Y = Math.Sign(dir.Y);
			}
			else if (dir.Y != 0f && Math.Abs(dir.Y) < 0.001f)
			{
				dir.Y = 0f;
				dir.X = Math.Sign(dir.X);
			}
			return dir;
		}

		private IEnumerator DashCoroutine()
		{
			yield return null;
			if (SaveData.Instance.Assists.DashAssist)
			{
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			}
			level.Displacement.AddBurst(base.Center, 0.4f, 8f, 64f, 0.5f, Ease.QuadOut, Ease.QuadOut);
			Vector2 dir = lastAim;
			if (OverrideDashDirection.HasValue)
			{
				dir = OverrideDashDirection.Value;
			}
			dir = CorrectDashPrecision(dir);
			Vector2 newSpeed = dir * 240f;
			if (Math.Sign(beforeDashSpeed.X) == Math.Sign(newSpeed.X) && Math.Abs(beforeDashSpeed.X) > Math.Abs(newSpeed.X))
			{
				newSpeed.X = beforeDashSpeed.X;
			}
			Speed = newSpeed;
			if (CollideCheck<Water>())
			{
				Speed *= 0.75f;
			}
			gliderBoostDir = (DashDir = dir);
			SceneAs<Level>().DirectionalShake(DashDir, 0.2f);
			if (DashDir.X != 0f)
			{
				Facing = (Facings)Math.Sign(DashDir.X);
			}
			CallDashEvents();
			if (StateMachine.PreviousState == 19)
			{
				level.Particles.Emit(FlyFeather.P_Boost, 12, base.Center, Vector2.One * 4f, (-dir).Angle());
			}
			if (onGround && DashDir.X != 0f && DashDir.Y > 0f && Speed.Y > 0f && (!Inventory.DreamDash || !CollideCheck<DreamBlock>(Position + Vector2.UnitY)))
			{
				DashDir.X = Math.Sign(DashDir.X);
				DashDir.Y = 0f;
				Speed.Y = 0f;
				Speed.X *= 1.2f;
				Ducking = true;
			}
			SlashFx.Burst(base.Center, DashDir.Angle());
			CreateTrail();
			if (SaveData.Instance.Assists.SuperDashing)
			{
				dashTrailTimer = 0.1f;
				dashTrailCounter = 2;
			}
			else
			{
				dashTrailTimer = 0.08f;
				dashTrailCounter = 1;
			}
			if (DashDir.X != 0f && Input.GrabCheck)
			{
				SwapBlock swapBlock = CollideFirst<SwapBlock>(Position + Vector2.UnitX * Math.Sign(DashDir.X));
				if (swapBlock != null && swapBlock.Direction.X == (float)Math.Sign(DashDir.X))
				{
					StateMachine.State = 1;
					Speed = Vector2.Zero;
					yield break;
				}
			}
			Vector2 swapCancel = Vector2.One;
			foreach (SwapBlock swapBlock2 in base.Scene.Tracker.GetEntities<SwapBlock>())
			{
				if (CollideCheck(swapBlock2, Position + Vector2.UnitY) && swapBlock2 != null && swapBlock2.Swapping)
				{
					if (DashDir.X != 0f && swapBlock2.Direction.X == (float)Math.Sign(DashDir.X))
					{
						Speed.X = (swapCancel.X = 0f);
					}
					if (DashDir.Y != 0f && swapBlock2.Direction.Y == (float)Math.Sign(DashDir.Y))
					{
						Speed.Y = (swapCancel.Y = 0f);
					}
				}
			}
			if (SaveData.Instance.Assists.SuperDashing)
			{
				yield return 0.3f;
			}
			else
			{
				yield return 0.15f;
			}
			CreateTrail();
			AutoJump = true;
			AutoJumpTimer = 0f;
			if (DashDir.Y <= 0f)
			{
				Speed = DashDir * 160f;
				Speed.X *= swapCancel.X;
				Speed.Y *= swapCancel.Y;
			}
			if (Speed.Y < 0f)
			{
				Speed.Y *= 0.75f;
			}
			StateMachine.State = 0;
		}

		private bool SwimCheck()
		{
			if (CollideCheck<Water>(Position + Vector2.UnitY * -8f))
			{
				return CollideCheck<Water>(Position);
			}
			return false;
		}

		private bool SwimUnderwaterCheck()
		{
			return CollideCheck<Water>(Position + Vector2.UnitY * -9f);
		}

		private bool SwimJumpCheck()
		{
			return !CollideCheck<Water>(Position + Vector2.UnitY * -14f);
		}

		private bool SwimRiseCheck()
		{
			return !CollideCheck<Water>(Position + Vector2.UnitY * -18f);
		}

		private bool UnderwaterMusicCheck()
		{
			if (CollideCheck<Water>(Position))
			{
				return CollideCheck<Water>(Position + Vector2.UnitY * -12f);
			}
			return false;
		}

		private void SwimBegin()
		{
			if (Speed.Y > 0f)
			{
				Speed.Y *= 0.5f;
			}
			Stamina = 110f;
		}

		private int SwimUpdate()
		{
			if (!SwimCheck())
			{
				return 0;
			}
			if (CanUnDuck)
			{
				Ducking = false;
			}
			if (CanDash)
			{
				demoDashed = Input.CrouchDashPressed;
				Input.Dash.ConsumeBuffer();
				Input.CrouchDash.ConsumeBuffer();
				return 2;
			}
			bool underwater = SwimUnderwaterCheck();
			if (!underwater && Speed.Y >= 0f && Input.GrabCheck && !IsTired && CanUnDuck && Math.Sign(Speed.X) != 0 - Facing && ClimbCheck((int)Facing))
			{
				if (SaveData.Instance.Assists.NoGrabbing)
				{
					ClimbTrigger((int)Facing);
				}
				else if (!MoveVExact(-1))
				{
					Ducking = false;
					return 1;
				}
			}
			Vector2 move = Input.Feather.Value;
			move = move.SafeNormalize();
			float maxX = (underwater ? 60f : 80f);
			float maxY = 80f;
			if (Math.Abs(Speed.X) > 80f && Math.Sign(Speed.X) == Math.Sign(move.X))
			{
				Speed.X = Calc.Approach(Speed.X, maxX * move.X, 400f * Engine.DeltaTime);
			}
			else
			{
				Speed.X = Calc.Approach(Speed.X, maxX * move.X, 600f * Engine.DeltaTime);
			}
			if (move.Y == 0f && SwimRiseCheck())
			{
				Speed.Y = Calc.Approach(Speed.Y, -60f, 600f * Engine.DeltaTime);
			}
			else if (move.Y >= 0f || SwimUnderwaterCheck())
			{
				if (Math.Abs(Speed.Y) > 80f && Math.Sign(Speed.Y) == Math.Sign(move.Y))
				{
					Speed.Y = Calc.Approach(Speed.Y, maxY * move.Y, 400f * Engine.DeltaTime);
				}
				else
				{
					Speed.Y = Calc.Approach(Speed.Y, maxY * move.Y, 600f * Engine.DeltaTime);
				}
			}
			if (!underwater && moveX != 0 && CollideCheck<Solid>(Position + Vector2.UnitX * moveX) && !CollideCheck<Solid>(Position + new Vector2(moveX, -3f)))
			{
				ClimbHop();
			}
			if (Input.Jump.Pressed && SwimJumpCheck())
			{
				Jump();
				return 0;
			}
			return 3;
		}

		public void Boost(Booster booster)
		{
			StateMachine.State = 4;
			Speed = Vector2.Zero;
			boostTarget = booster.Center;
			boostRed = false;
			LastBooster = (CurrentBooster = booster);
		}

		public void RedBoost(Booster booster)
		{
			StateMachine.State = 4;
			Speed = Vector2.Zero;
			boostTarget = booster.Center;
			boostRed = true;
			LastBooster = (CurrentBooster = booster);
		}

		private void BoostBegin()
		{
			RefillDash();
			RefillStamina();
			if (Holding != null)
			{
				Drop();
			}
		}

		private void BoostEnd()
		{
			Vector2 to = (boostTarget - base.Collider.Center).Floor();
			MoveToX(to.X);
			MoveToY(to.Y);
		}

		private int BoostUpdate()
		{
			Vector2 targetAdd = Input.Aim.Value * 3f;
			Vector2 to = Calc.Approach(base.ExactPosition, boostTarget - base.Collider.Center + targetAdd, 80f * Engine.DeltaTime);
			MoveToX(to.X);
			MoveToY(to.Y);
			if (Input.DashPressed || Input.CrouchDashPressed)
			{
				demoDashed = Input.CrouchDashPressed;
				Input.Dash.ConsumePress();
				Input.CrouchDash.ConsumeBuffer();
				if (boostRed)
				{
					return 5;
				}
				return 2;
			}
			return 4;
		}

		private IEnumerator BoostCoroutine()
		{
			yield return 0.25f;
			if (boostRed)
			{
				StateMachine.State = 5;
			}
			else
			{
				StateMachine.State = 2;
			}
		}

		private void RedDashBegin()
		{
			calledDashEvents = false;
			dashStartedOnGround = false;
			Celeste.Freeze(0.05f);
			Dust.Burst(Position, (-DashDir).Angle(), 8);
			dashCooldownTimer = 0.2f;
			dashRefillCooldownTimer = 0.1f;
			StartedDashing = true;
			level.Displacement.AddBurst(base.Center, 0.5f, 0f, 80f, 0.666f, Ease.QuadOut, Ease.QuadOut);
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			dashAttackTimer = 0.3f;
			gliderBoostTimer = 0.55f;
			DashDir = (Speed = Vector2.Zero);
			if (!onGround && CanUnDuck)
			{
				Ducking = false;
			}
			DashAssistInit();
		}

		private void RedDashEnd()
		{
			CallDashEvents();
		}

		private int RedDashUpdate()
		{
			StartedDashing = false;
			bool ch9Boost = LastBooster != null && LastBooster.Ch9HubTransition;
			gliderBoostTimer = 0.05f;
			if (CanDash)
			{
				return StartDash();
			}
			if (DashDir.Y == 0f)
			{
				foreach (JumpThru jt in base.Scene.Tracker.GetEntities<JumpThru>())
				{
					if (CollideCheck(jt) && base.Bottom - jt.Top <= 6f)
					{
						MoveVExact((int)(jt.Top - base.Bottom));
					}
				}
				if (CanUnDuck && Input.Jump.Pressed && jumpGraceTimer > 0f && !ch9Boost)
				{
					SuperJump();
					return 0;
				}
			}
			if (!ch9Boost)
			{
				if (SuperWallJumpAngleCheck)
				{
					if (Input.Jump.Pressed && CanUnDuck)
					{
						if (WallJumpCheck(1))
						{
							SuperWallJump(-1);
							return 0;
						}
						if (WallJumpCheck(-1))
						{
							SuperWallJump(1);
							return 0;
						}
					}
				}
				else if (Input.Jump.Pressed && CanUnDuck)
				{
					if (WallJumpCheck(1))
					{
						if (Facing == Facings.Right && Input.GrabCheck && Stamina > 0f && Holding == null && !ClimbBlocker.Check(base.Scene, this, Position + Vector2.UnitX * 3f))
						{
							ClimbJump();
						}
						else
						{
							WallJump(-1);
						}
						return 0;
					}
					if (WallJumpCheck(-1))
					{
						if (Facing == Facings.Left && Input.GrabCheck && Stamina > 0f && Holding == null && !ClimbBlocker.Check(base.Scene, this, Position + Vector2.UnitX * -3f))
						{
							ClimbJump();
						}
						else
						{
							WallJump(1);
						}
						return 0;
					}
				}
			}
			return 5;
		}

		private IEnumerator RedDashCoroutine()
		{
			yield return null;
			Speed = CorrectDashPrecision(lastAim) * 240f;
			gliderBoostDir = (DashDir = lastAim);
			SceneAs<Level>().DirectionalShake(DashDir, 0.2f);
			if (DashDir.X != 0f)
			{
				Facing = (Facings)Math.Sign(DashDir.X);
			}
			CallDashEvents();
		}

		private void HitSquashBegin()
		{
			hitSquashNoMoveTimer = 0.1f;
		}

		private int HitSquashUpdate()
		{
			Speed.X = Calc.Approach(Speed.X, 0f, 800f * Engine.DeltaTime);
			Speed.Y = Calc.Approach(Speed.Y, 0f, 800f * Engine.DeltaTime);
			if (Input.Jump.Pressed)
			{
				if (onGround)
				{
					Jump();
				}
				else if (WallJumpCheck(1))
				{
					if (Facing == Facings.Right && Input.GrabCheck && Stamina > 0f && Holding == null && !ClimbBlocker.Check(base.Scene, this, Position + Vector2.UnitX * 3f))
					{
						ClimbJump();
					}
					else
					{
						WallJump(-1);
					}
				}
				else if (WallJumpCheck(-1))
				{
					if (Facing == Facings.Left && Input.GrabCheck && Stamina > 0f && Holding == null && !ClimbBlocker.Check(base.Scene, this, Position + Vector2.UnitX * -3f))
					{
						ClimbJump();
					}
					else
					{
						WallJump(1);
					}
				}
				else
				{
					Input.Jump.ConsumeBuffer();
				}
				return 0;
			}
			if (CanDash)
			{
				return StartDash();
			}
			if (Input.GrabCheck && ClimbCheck((int)Facing))
			{
				return 1;
			}
			if (hitSquashNoMoveTimer > 0f)
			{
				hitSquashNoMoveTimer -= Engine.DeltaTime;
				return 6;
			}
			return 0;
		}

		public Vector2 ExplodeLaunch(Vector2 from, bool snapUp = true, bool sidesOnly = false)
		{
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			Celeste.Freeze(0.1f);
			launchApproachX = null;
			Vector2 normal = (base.Center - from).SafeNormalize(-Vector2.UnitY);
			float dot = Vector2.Dot(normal, Vector2.UnitY);
			if (snapUp && dot <= -0.7f)
			{
				normal.X = 0f;
				normal.Y = -1f;
			}
			else if (dot <= 0.65f && dot >= -0.55f)
			{
				normal.Y = 0f;
				normal.X = Math.Sign(normal.X);
			}
			if (sidesOnly && normal.X != 0f)
			{
				normal.Y = 0f;
				normal.X = Math.Sign(normal.X);
			}
			Speed = 280f * normal;
			if (Speed.Y <= 50f)
			{
				Speed.Y = Math.Min(-150f, Speed.Y);
				AutoJump = true;
			}
			if (Speed.X != 0f)
			{
				if (Input.MoveX.Value == Math.Sign(Speed.X))
				{
					explodeLaunchBoostTimer = 0f;
					Speed.X *= 1.2f;
				}
				else
				{
					explodeLaunchBoostTimer = 0.01f;
					explodeLaunchBoostSpeed = Speed.X * 1.2f;
				}
			}
			SlashFx.Burst(base.Center, Speed.Angle());
			if (!Inventory.NoRefills)
			{
				RefillDash();
			}
			RefillStamina();
			dashCooldownTimer = 0.2f;
			StateMachine.State = 7;
			return normal;
		}

		public void FinalBossPushLaunch(int dir)
		{
			launchApproachX = null;
			Speed.X = 0.9f * (float)dir * 280f;
			Speed.Y = -150f;
			AutoJump = true;
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			SlashFx.Burst(base.Center, Speed.Angle());
			RefillDash();
			RefillStamina();
			dashCooldownTimer = 0.28f;
			StateMachine.State = 7;
		}

		public void BadelineBoostLaunch(float atX)
		{
			launchApproachX = atX;
			Speed.X = 0f;
			Speed.Y = -330f;
			AutoJump = true;
			if (Holding != null)
			{
				Drop();
			}
			SlashFx.Burst(base.Center, Speed.Angle());
			RefillDash();
			RefillStamina();
			dashCooldownTimer = 0.2f;
			StateMachine.State = 7;
		}

		private void LaunchBegin()
		{
			launched = true;
		}

		private int LaunchUpdate()
		{
			if (launchApproachX.HasValue)
			{
				MoveTowardsX(launchApproachX.Value, 60f * Engine.DeltaTime);
			}
			if (CanDash)
			{
				return StartDash();
			}
			if (Input.GrabCheck && !IsTired && !Ducking)
			{
				foreach (Holdable hold in base.Scene.Tracker.GetComponents<Holdable>())
				{
					if (hold.Check(this) && Pickup(hold))
					{
						return 8;
					}
				}
			}
			if (Speed.Y < 0f)
			{
				Speed.Y = Calc.Approach(Speed.Y, 160f, 450f * Engine.DeltaTime);
			}
			else
			{
				Speed.Y = Calc.Approach(Speed.Y, 160f, 225f * Engine.DeltaTime);
			}
			Speed.X = Calc.Approach(Speed.X, 0f, 200f * Engine.DeltaTime);
			if (Speed.Length() < 220f)
			{
				return 0;
			}
			return 7;
		}

		public void SummitLaunch(float targetX)
		{
			summitLaunchTargetX = targetX;
			StateMachine.State = 10;
		}

		private void SummitLaunchBegin()
		{
			wallBoostTimer = 0f;
			Sprite.Play("launch");
			Speed = -Vector2.UnitY * 240f;
			summitLaunchParticleTimer = 0.4f;
		}

		private int SummitLaunchUpdate()
		{
			summitLaunchParticleTimer -= Engine.DeltaTime;
			if (summitLaunchParticleTimer > 0f && base.Scene.OnInterval(0.03f))
			{
				level.ParticlesFG.Emit(BadelineBoost.P_Move, 1, base.Center, Vector2.One * 4f);
			}
			Facing = Facings.Right;
			MoveTowardsX(summitLaunchTargetX, 20f * Engine.DeltaTime);
			Speed = -Vector2.UnitY * 240f;
			if (level.OnInterval(0.2f))
			{
				level.Add(Engine.Pooler.Create<SpeedRing>().Init(base.Center, (float)Math.PI / 2f, Color.White));
			}
			CrystalStaticSpinner hit = base.Scene.CollideFirst<CrystalStaticSpinner>(new Rectangle((int)(base.X - 4f), (int)(base.Y - 40f), 8, 12));
			if (hit != null)
			{
				hit.Destroy();
				level.Shake();
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
				Celeste.Freeze(0.01f);
			}
			return 10;
		}

		public void StopSummitLaunch()
		{
			StateMachine.State = 0;
			Speed.Y = -140f;
			AutoJump = true;
			varJumpSpeed = Speed.Y;
		}

		private IEnumerator PickupCoroutine()
		{
			Play("event:/char/madeline/crystaltheo_lift");
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
			if (Holding != null && Holding.SlowFall && ((gliderBoostTimer - 0.16f > 0f && gliderBoostDir.Y < 0f) || (Speed.Length() > 180f && Speed.Y <= 0f)))
			{
				Audio.Play("event:/new_content/game/10_farewell/glider_platform_dissipate", Position);
			}
			Vector2 oldSpeed = Speed;
			float varJump = varJumpTimer;
			Speed = Vector2.Zero;
			Vector2 begin = Holding.Entity.Position - Position;
			Vector2 end = CarryOffsetTarget;
			SimpleCurve curve = new SimpleCurve(control: new Vector2(begin.X + (float)(Math.Sign(begin.X) * 2), CarryOffsetTarget.Y - 2f), begin: begin, end: end);
			carryOffset = begin;
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, 0.16f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				carryOffset = curve.GetPoint(t.Eased);
			};
			Add(tween);
			yield return tween.Wait();
			Speed = oldSpeed;
			Speed.Y = Math.Min(Speed.Y, 0f);
			varJumpTimer = varJump;
			StateMachine.State = 0;
			if (Holding != null && Holding.SlowFall)
			{
				if (gliderBoostTimer > 0f && gliderBoostDir.Y < 0f)
				{
					Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
					gliderBoostTimer = 0f;
					Speed.Y = Math.Min(Speed.Y, -240f * Math.Abs(gliderBoostDir.Y));
				}
				else if (Speed.Y < 0f)
				{
					Speed.Y = Math.Min(Speed.Y, -105f);
				}
				if (onGround && (float)Input.MoveY == 1f)
				{
					holdCannotDuck = true;
				}
			}
		}

		private void DreamDashBegin()
		{
			if (dreamSfxLoop == null)
			{
				Add(dreamSfxLoop = new SoundSource());
			}
			Speed = DashDir * 240f;
			TreatNaive = true;
			base.Depth = -12000;
			dreamDashCanEndTimer = 0.1f;
			Stamina = 110f;
			dreamJump = false;
			Play("event:/char/madeline/dreamblock_enter");
			Loop(dreamSfxLoop, "event:/char/madeline/dreamblock_travel");
		}

		private void DreamDashEnd()
		{
			base.Depth = 0;
			if (!dreamJump)
			{
				AutoJump = true;
				AutoJumpTimer = 0f;
			}
			if (!Inventory.NoRefills)
			{
				RefillDash();
			}
			RefillStamina();
			TreatNaive = false;
			if (dreamBlock != null)
			{
				if (DashDir.X != 0f)
				{
					jumpGraceTimer = 0.1f;
					dreamJump = true;
				}
				else
				{
					jumpGraceTimer = 0f;
				}
				dreamBlock.OnPlayerExit(this);
				dreamBlock = null;
			}
			Stop(dreamSfxLoop);
			Play("event:/char/madeline/dreamblock_exit");
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
		}

		private int DreamDashUpdate()
		{
			Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
			Vector2 oldPos = Position;
			NaiveMove(Speed * Engine.DeltaTime);
			if (dreamDashCanEndTimer > 0f)
			{
				dreamDashCanEndTimer -= Engine.DeltaTime;
			}
			DreamBlock block = CollideFirst<DreamBlock>();
			if (block == null)
			{
				if (DreamDashedIntoSolid())
				{
					if (SaveData.Instance.Assists.Invincible)
					{
						Position = oldPos;
						Speed *= -1f;
						Play("event:/game/general/assist_dreamblockbounce");
					}
					else
					{
						Die(Vector2.Zero);
					}
				}
				else if (dreamDashCanEndTimer <= 0f)
				{
					Celeste.Freeze(0.05f);
					if (Input.Jump.Pressed && DashDir.X != 0f)
					{
						dreamJump = true;
						Jump();
					}
					else if (DashDir.Y >= 0f || DashDir.X != 0f)
					{
						if (DashDir.X > 0f && CollideCheck<Solid>(Position - Vector2.UnitX * 5f))
						{
							MoveHExact(-5);
						}
						else if (DashDir.X < 0f && CollideCheck<Solid>(Position + Vector2.UnitX * 5f))
						{
							MoveHExact(5);
						}
						bool left = ClimbCheck(-1);
						bool right = ClimbCheck(1);
						if (Input.GrabCheck && ((moveX == 1 && right) || (moveX == -1 && left)))
						{
							Facing = (Facings)moveX;
							if (!SaveData.Instance.Assists.NoGrabbing)
							{
								return 1;
							}
							ClimbTrigger(moveX);
							Speed.X = 0f;
						}
					}
					return 0;
				}
			}
			else
			{
				dreamBlock = block;
				if (base.Scene.OnInterval(0.1f))
				{
					CreateTrail();
				}
				if (level.OnInterval(0.04f))
				{
					DisplacementRenderer.Burst burst = level.Displacement.AddBurst(base.Center, 0.3f, 0f, 40f);
					burst.WorldClipCollider = dreamBlock.Collider;
					burst.WorldClipPadding = 2;
				}
			}
			return 9;
		}

		private bool DreamDashedIntoSolid()
		{
			if (CollideCheck<Solid>())
			{
				for (int x = 1; x <= 5; x++)
				{
					for (int xm = -1; xm <= 1; xm += 2)
					{
						for (int y = 1; y <= 5; y++)
						{
							for (int ym = -1; ym <= 1; ym += 2)
							{
								Vector2 add = new Vector2(x * xm, y * ym);
								if (!CollideCheck<Solid>(Position + add))
								{
									Position += add;
									return false;
								}
							}
						}
					}
				}
				return true;
			}
			return false;
		}

		public bool StartStarFly()
		{
			RefillStamina();
			if (StateMachine.State == 18)
			{
				return false;
			}
			if (StateMachine.State == 19)
			{
				starFlyTimer = 2f;
				Sprite.Color = starFlyColor;
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			}
			else
			{
				StateMachine.State = 19;
			}
			return true;
		}

		private void StarFlyBegin()
		{
			Sprite.Play("startStarFly");
			starFlyTransforming = true;
			starFlyTimer = 2f;
			starFlySpeedLerp = 0f;
			jumpGraceTimer = 0f;
			if (starFlyBloom == null)
			{
				Add(starFlyBloom = new BloomPoint(new Vector2(0f, -6f), 0f, 16f));
			}
			starFlyBloom.Visible = true;
			starFlyBloom.Alpha = 0f;
			base.Collider = starFlyHitbox;
			hurtbox = starFlyHurtbox;
			if (starFlyLoopSfx == null)
			{
				Add(starFlyLoopSfx = new SoundSource());
				starFlyLoopSfx.DisposeOnTransition = false;
				Add(starFlyWarningSfx = new SoundSource());
				starFlyWarningSfx.DisposeOnTransition = false;
			}
			starFlyLoopSfx.Play("event:/game/06_reflection/feather_state_loop", "feather_speed", 1f);
			starFlyWarningSfx.Stop();
		}

		private void StarFlyEnd()
		{
			Play("event:/game/06_reflection/feather_state_end");
			starFlyWarningSfx.Stop();
			starFlyLoopSfx.Stop();
			Hair.DrawPlayerSpriteOutline = false;
			Sprite.Color = Color.White;
			level.Displacement.AddBurst(base.Center, 0.25f, 8f, 32f);
			starFlyBloom.Visible = false;
			Sprite.HairCount = startHairCount;
			StarFlyReturnToNormalHitbox();
			if (StateMachine.State != 2)
			{
				level.Particles.Emit(FlyFeather.P_Boost, 12, base.Center, Vector2.One * 4f, (-Speed).Angle());
			}
		}

		private void StarFlyReturnToNormalHitbox()
		{
			base.Collider = normalHitbox;
			hurtbox = normalHurtbox;
			if (!CollideCheck<Solid>())
			{
				return;
			}
			Vector2 start = Position;
			base.Y -= normalHitbox.Bottom - starFlyHitbox.Bottom;
			if (CollideCheck<Solid>())
			{
				Position = start;
				Ducking = true;
				base.Y -= duckHitbox.Bottom - starFlyHitbox.Bottom;
				if (CollideCheck<Solid>())
				{
					Position = start;
					throw new Exception("Could not get out of solids when exiting Star Fly State!");
				}
			}
		}

		private IEnumerator StarFlyCoroutine()
		{
			while (Sprite.CurrentAnimationID == "startStarFly")
			{
				yield return null;
			}
			while (Speed != Vector2.Zero)
			{
				yield return null;
			}
			yield return 0.1f;
			Sprite.Color = starFlyColor;
			Sprite.HairCount = 7;
			Hair.DrawPlayerSpriteOutline = true;
			level.Displacement.AddBurst(base.Center, 0.25f, 8f, 32f);
			starFlyTransforming = false;
			starFlyTimer = 2f;
			RefillDash();
			RefillStamina();
			Vector2 dir = Input.Feather.Value;
			if (dir == Vector2.Zero)
			{
				dir = Vector2.UnitX * (float)Facing;
			}
			Speed = dir * 250f;
			starFlyLastDir = dir;
			level.Particles.Emit(FlyFeather.P_Boost, 12, base.Center, Vector2.One * 4f, (-dir).Angle());
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			level.DirectionalShake(starFlyLastDir);
			while (starFlyTimer > 0.5f)
			{
				yield return null;
			}
			starFlyWarningSfx.Play("event:/game/06_reflection/feather_state_warning");
		}

		private int StarFlyUpdate()
		{
			starFlyBloom.Alpha = Calc.Approach(starFlyBloom.Alpha, 0.7f, Engine.DeltaTime * 2f);
			Input.Rumble(RumbleStrength.Climb, RumbleLength.Short);
			if (starFlyTransforming)
			{
				Speed = Calc.Approach(Speed, Vector2.Zero, 1000f * Engine.DeltaTime);
			}
			else
			{
				Vector2 aim = Input.Feather.Value;
				bool slow = false;
				if (aim == Vector2.Zero)
				{
					slow = true;
					aim = starFlyLastDir;
				}
				Vector2 currentDir = Speed.SafeNormalize(Vector2.Zero);
				currentDir = (starFlyLastDir = ((!(currentDir == Vector2.Zero)) ? currentDir.RotateTowards(aim.Angle(), 5.5850534f * Engine.DeltaTime) : aim));
				float maxSpeed;
				if (slow)
				{
					starFlySpeedLerp = 0f;
					maxSpeed = 91f;
				}
				else if (currentDir != Vector2.Zero && Vector2.Dot(currentDir, aim) >= 0.45f)
				{
					starFlySpeedLerp = Calc.Approach(starFlySpeedLerp, 1f, Engine.DeltaTime / 1f);
					maxSpeed = MathHelper.Lerp(140f, 190f, starFlySpeedLerp);
				}
				else
				{
					starFlySpeedLerp = 0f;
					maxSpeed = 140f;
				}
				starFlyLoopSfx.Param("feather_speed", (!slow) ? 1 : 0);
				float currentSpeed = Speed.Length();
				currentSpeed = Calc.Approach(currentSpeed, maxSpeed, 1000f * Engine.DeltaTime);
				Speed = currentDir * currentSpeed;
				if (level.OnInterval(0.02f))
				{
					level.Particles.Emit(FlyFeather.P_Flying, 1, base.Center, Vector2.One * 2f, (-Speed).Angle());
				}
				if (Input.Jump.Pressed)
				{
					if (OnGround(3))
					{
						Jump();
						return 0;
					}
					if (WallJumpCheck(-1))
					{
						WallJump(1);
						return 0;
					}
					if (WallJumpCheck(1))
					{
						WallJump(-1);
						return 0;
					}
				}
				if (Input.GrabCheck)
				{
					bool cancel = false;
					int dir = 0;
					if (Input.MoveX.Value != -1 && ClimbCheck(1))
					{
						Facing = Facings.Right;
						dir = 1;
						cancel = true;
					}
					else if (Input.MoveX.Value != 1 && ClimbCheck(-1))
					{
						Facing = Facings.Left;
						dir = -1;
						cancel = true;
					}
					if (cancel)
					{
						if (SaveData.Instance.Assists.NoGrabbing)
						{
							Speed = Vector2.Zero;
							ClimbTrigger(dir);
							return 0;
						}
						return 1;
					}
				}
				if (CanDash)
				{
					return StartDash();
				}
				starFlyTimer -= Engine.DeltaTime;
				if (starFlyTimer <= 0f)
				{
					if (Input.MoveY.Value == -1)
					{
						Speed.Y = -100f;
					}
					if (Input.MoveY.Value < 1)
					{
						varJumpSpeed = Speed.Y;
						AutoJump = true;
						AutoJumpTimer = 0f;
						varJumpTimer = 0.2f;
					}
					if (Speed.Y > 0f)
					{
						Speed.Y = 0f;
					}
					if (Math.Abs(Speed.X) > 140f)
					{
						Speed.X = 140f * (float)Math.Sign(Speed.X);
					}
					Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
					return 0;
				}
				if (starFlyTimer < 0.5f && base.Scene.OnInterval(0.05f))
				{
					if (Sprite.Color == starFlyColor)
					{
						Sprite.Color = NormalHairColor;
					}
					else
					{
						Sprite.Color = starFlyColor;
					}
				}
			}
			return 19;
		}

		public bool DoFlingBird(FlingBird bird)
		{
			if (!Dead && StateMachine.State != 24)
			{
				flingBird = bird;
				StateMachine.State = 24;
				if (Holding != null)
				{
					Drop();
				}
				return true;
			}
			return false;
		}

		public void FinishFlingBird()
		{
			StateMachine.State = 0;
			AutoJump = true;
			forceMoveX = 1;
			forceMoveXTimer = 0.2f;
			Speed = FlingBird.FlingSpeed;
			varJumpTimer = 0.2f;
			varJumpSpeed = Speed.Y;
			launched = true;
		}

		private void FlingBirdBegin()
		{
			RefillDash();
			RefillStamina();
		}

		private void FlingBirdEnd()
		{
		}

		private int FlingBirdUpdate()
		{
			MoveTowardsX(flingBird.X, 250f * Engine.DeltaTime);
			MoveTowardsY(flingBird.Y + 8f + base.Collider.Height, 250f * Engine.DeltaTime);
			return 24;
		}

		private IEnumerator FlingBirdCoroutine()
		{
			yield break;
		}

		public void StartCassetteFly(Vector2 targetPosition, Vector2 control)
		{
			StateMachine.State = 21;
			cassetteFlyCurve = new SimpleCurve(Position, targetPosition, control);
			cassetteFlyLerp = 0f;
			Speed = Vector2.Zero;
			if (Holding != null)
			{
				Drop();
			}
		}

		private void CassetteFlyBegin()
		{
			Sprite.Play("bubble");
			Sprite.Y += 5f;
		}

		private void CassetteFlyEnd()
		{
		}

		private int CassetteFlyUpdate()
		{
			return 21;
		}

		private IEnumerator CassetteFlyCoroutine()
		{
			level.CanRetry = false;
			level.FormationBackdrop.Display = true;
			level.FormationBackdrop.Alpha = 0.5f;
			Sprite.Scale = Vector2.One * 1.25f;
			base.Depth = -2000000;
			yield return 0.4f;
			while (cassetteFlyLerp < 1f)
			{
				if (level.OnInterval(0.03f))
				{
					level.Particles.Emit(P_CassetteFly, 2, base.Center, Vector2.One * 4f);
				}
				cassetteFlyLerp = Calc.Approach(cassetteFlyLerp, 1f, 1.6f * Engine.DeltaTime);
				Position = cassetteFlyCurve.GetPoint(Ease.SineInOut(cassetteFlyLerp));
				level.Camera.Position = CameraTarget;
				yield return null;
			}
			Position = cassetteFlyCurve.End;
			Sprite.Scale = Vector2.One * 1.25f;
			Sprite.Y -= 5f;
			Sprite.Play("fallFast");
			yield return 0.2f;
			level.CanRetry = true;
			level.FormationBackdrop.Display = false;
			level.FormationBackdrop.Alpha = 0.5f;
			StateMachine.State = 0;
			base.Depth = 0;
		}

		public void StartAttract(Vector2 attractTo)
		{
			this.attractTo = attractTo.Round();
			StateMachine.State = 22;
		}

		private void AttractBegin()
		{
			Speed = Vector2.Zero;
		}

		private void AttractEnd()
		{
		}

		private int AttractUpdate()
		{
			if (Vector2.Distance(attractTo, base.ExactPosition) <= 1.5f)
			{
				Position = attractTo;
				ZeroRemainderX();
				ZeroRemainderY();
			}
			else
			{
				Vector2 at = Calc.Approach(base.ExactPosition, attractTo, 200f * Engine.DeltaTime);
				MoveToX(at.X);
				MoveToY(at.Y);
			}
			return 22;
		}

		private void DummyBegin()
		{
			DummyMoving = false;
			DummyGravity = true;
			DummyAutoAnimate = true;
		}

		private int DummyUpdate()
		{
			if (CanUnDuck)
			{
				Ducking = false;
			}
			if (!onGround && DummyGravity)
			{
				float mult = ((Math.Abs(Speed.Y) < 40f && (Input.Jump.Check || AutoJump)) ? 0.5f : 1f);
				if (level.InSpace)
				{
					mult *= 0.6f;
				}
				Speed.Y = Calc.Approach(Speed.Y, 160f, 900f * mult * Engine.DeltaTime);
			}
			if (varJumpTimer > 0f)
			{
				if (AutoJump || Input.Jump.Check)
				{
					Speed.Y = Math.Min(Speed.Y, varJumpSpeed);
				}
				else
				{
					varJumpTimer = 0f;
				}
			}
			if (!DummyMoving)
			{
				if (Math.Abs(Speed.X) > 90f && DummyMaxspeed)
				{
					Speed.X = Calc.Approach(Speed.X, 90f * (float)Math.Sign(Speed.X), 2500f * Engine.DeltaTime);
				}
				if (DummyFriction)
				{
					Speed.X = Calc.Approach(Speed.X, 0f, 1000f * Engine.DeltaTime);
				}
			}
			if (DummyAutoAnimate)
			{
				if (onGround)
				{
					if (Speed.X == 0f)
					{
						Sprite.Play("idle");
					}
					else
					{
						Sprite.Play("walk");
					}
				}
				else if (Speed.Y < 0f)
				{
					Sprite.Play("jumpSlow");
				}
				else
				{
					Sprite.Play("fallSlow");
				}
			}
			return 11;
		}

		public IEnumerator DummyWalkTo(float x, bool walkBackwards = false, float speedMultiplier = 1f, bool keepWalkingIntoWalls = false)
		{
			StateMachine.State = 11;
			if (Math.Abs(base.X - x) > 4f && !Dead)
			{
				DummyMoving = true;
				if (walkBackwards)
				{
					Sprite.Rate = -1f;
					Facing = (Facings)Math.Sign(base.X - x);
				}
				else
				{
					Facing = (Facings)Math.Sign(x - base.X);
				}
				while (Math.Abs(x - base.X) > 4f && base.Scene != null && (keepWalkingIntoWalls || !CollideCheck<Solid>(Position + Vector2.UnitX * Math.Sign(x - base.X))))
				{
					Speed.X = Calc.Approach(Speed.X, (float)Math.Sign(x - base.X) * 64f * speedMultiplier, 1000f * Engine.DeltaTime);
					yield return null;
				}
				Sprite.Rate = 1f;
				Sprite.Play("idle");
				DummyMoving = false;
			}
		}

		public IEnumerator DummyWalkToExact(int x, bool walkBackwards = false, float speedMultiplier = 1f, bool cancelOnFall = false)
		{
			StateMachine.State = 11;
			if (base.X == (float)x)
			{
				yield break;
			}
			DummyMoving = true;
			if (walkBackwards)
			{
				Sprite.Rate = -1f;
				Facing = (Facings)Math.Sign(base.X - (float)x);
			}
			else
			{
				Facing = (Facings)Math.Sign((float)x - base.X);
			}
			int last = Math.Sign(base.X - (float)x);
			while (!Dead && base.X != (float)x && !CollideCheck<Solid>(Position + new Vector2((float)Facing, 0f)) && (!cancelOnFall || OnGround()))
			{
				Speed.X = Calc.Approach(Speed.X, (float)Math.Sign((float)x - base.X) * 64f * speedMultiplier, 1000f * Engine.DeltaTime);
				int next = Math.Sign(base.X - (float)x);
				if (next != last)
				{
					base.X = x;
					break;
				}
				last = next;
				yield return null;
			}
			Speed.X = 0f;
			Sprite.Rate = 1f;
			Sprite.Play("idle");
			DummyMoving = false;
		}

		public IEnumerator DummyRunTo(float x, bool fastAnim = false)
		{
			StateMachine.State = 11;
			if (Math.Abs(base.X - x) > 4f)
			{
				DummyMoving = true;
				if (fastAnim)
				{
					Sprite.Play("runFast");
				}
				else if (!Sprite.LastAnimationID.StartsWith("run"))
				{
					Sprite.Play("runSlow");
				}
				Facing = (Facings)Math.Sign(x - base.X);
				while (Math.Abs(base.X - x) > 4f)
				{
					Speed.X = Calc.Approach(Speed.X, (float)Math.Sign(x - base.X) * 90f, 1000f * Engine.DeltaTime);
					yield return null;
				}
				Sprite.Play("idle");
				DummyMoving = false;
			}
		}

		private int FrozenUpdate()
		{
			return 17;
		}

		private int TempleFallUpdate()
		{
			Facing = Facings.Right;
			if (!onGround)
			{
				int center = level.Bounds.Left + 160;
				int mX = ((Math.Abs((float)center - base.X) > 4f) ? Math.Sign((float)center - base.X) : 0);
				Speed.X = Calc.Approach(Speed.X, 54.000004f * (float)mX, 325f * Engine.DeltaTime);
			}
			if (!onGround && DummyGravity)
			{
				Speed.Y = Calc.Approach(Speed.Y, 320f, 225f * Engine.DeltaTime);
			}
			return 20;
		}

		private IEnumerator TempleFallCoroutine()
		{
			Sprite.Play("fallFast");
			while (!onGround)
			{
				yield return null;
			}
			Play("event:/char/madeline/mirrortemple_big_landing");
			if (Dashes <= 1)
			{
				Sprite.Play("fallPose");
			}
			else
			{
				Sprite.Play("idle");
			}
			Sprite.Scale.Y = 0.7f;
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			level.DirectionalShake(new Vector2(0f, 1f), 0.5f);
			Speed.X = 0f;
			level.Particles.Emit(P_SummitLandA, 12, base.BottomCenter, Vector2.UnitX * 3f, -(float)Math.PI / 2f);
			level.Particles.Emit(P_SummitLandB, 8, base.BottomCenter - Vector2.UnitX * 2f, Vector2.UnitX * 2f, 3.403392f);
			level.Particles.Emit(P_SummitLandB, 8, base.BottomCenter + Vector2.UnitX * 2f, Vector2.UnitX * 2f, -(float)Math.PI / 12f);
			for (float p = 0f; p < 1f; p += Engine.DeltaTime)
			{
				yield return null;
			}
			StateMachine.State = 0;
		}

		private void ReflectionFallBegin()
		{
			IgnoreJumpThrus = true;
		}

		private void ReflectionFallEnd()
		{
			FallEffects.Show(visible: false);
			IgnoreJumpThrus = false;
		}

		private int ReflectionFallUpdate()
		{
			Facing = Facings.Right;
			if (base.Scene.OnInterval(0.05f))
			{
				wasDashB = true;
				CreateTrail();
			}
			if (CollideCheck<Water>())
			{
				Speed.Y = Calc.Approach(Speed.Y, -20f, 400f * Engine.DeltaTime);
			}
			else
			{
				Speed.Y = Calc.Approach(Speed.Y, 320f, 225f * Engine.DeltaTime);
			}
			foreach (Entity entity in base.Scene.Tracker.GetEntities<FlyFeather>())
			{
				entity.RemoveSelf();
			}
			CrystalStaticSpinner hit = base.Scene.CollideFirst<CrystalStaticSpinner>(new Rectangle((int)(base.X - 6f), (int)(base.Y - 6f), 12, 12));
			if (hit != null)
			{
				hit.Destroy();
				level.Shake();
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
				Celeste.Freeze(0.01f);
			}
			return 18;
		}

		private IEnumerator ReflectionFallCoroutine()
		{
			Sprite.Play("bigFall");
			level.StartCutscene(OnReflectionFallSkip);
			for (float t = 0f; t < 2f; t += Engine.DeltaTime)
			{
				Speed.Y = 0f;
				yield return null;
			}
			FallEffects.Show(visible: true);
			Speed.Y = 320f;
			while (!CollideCheck<Water>())
			{
				yield return null;
			}
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			FallEffects.Show(visible: false);
			Sprite.Play("bigFallRecover");
			level.Session.Audio.Music.Event = "event:/music/lvl6/main";
			level.Session.Audio.Apply();
			level.EndCutscene();
			yield return 1.2f;
			StateMachine.State = 0;
		}

		private void OnReflectionFallSkip(Level level)
		{
			level.OnEndOfFrame += delegate
			{
				level.Remove(this);
				level.UnloadLevel();
				level.Session.Level = "00";
				level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Bottom));
				level.LoadLevel(IntroTypes.None);
				FallEffects.Show(visible: false);
				level.Session.Audio.Music.Event = "event:/music/lvl6/main";
				level.Session.Audio.Apply();
			};
		}

		public IEnumerator IntroWalkCoroutine()
		{
			Vector2 start = Position;
			if (IntroWalkDirection == Facings.Right)
			{
				base.X = level.Bounds.Left - 16;
				Facing = Facings.Right;
			}
			else
			{
				base.X = level.Bounds.Right + 16;
				Facing = Facings.Left;
			}
			yield return 0.3f;
			Sprite.Play("runSlow");
			while (Math.Abs(base.X - start.X) > 2f && !CollideCheck<Solid>(Position + new Vector2((float)Facing, 0f)))
			{
				MoveTowardsX(start.X, 64f * Engine.DeltaTime);
				yield return null;
			}
			Position = start;
			Sprite.Play("idle");
			yield return 0.2f;
			StateMachine.State = 0;
		}

		private IEnumerator IntroJumpCoroutine()
		{
			Vector2 start = Position;
			bool wasSummitJump = StateMachine.PreviousState == 10;
			base.Depth = -1000000;
			Facing = Facings.Right;
			if (!wasSummitJump)
			{
				base.Y = level.Bounds.Bottom + 16;
				yield return 0.5f;
			}
			else
			{
				start.Y = level.Bounds.Bottom - 24;
				MoveToX((int)Math.Round(base.X / 8f) * 8);
			}
			if (!wasSummitJump)
			{
				Sprite.Play("jumpSlow");
			}
			while (base.Y > start.Y - 8f)
			{
				base.Y += -120f * Engine.DeltaTime;
				yield return null;
			}
			base.Y = (float)Math.Round(base.Y);
			Speed.Y = -100f;
			while (Speed.Y < 0f)
			{
				Speed.Y += Engine.DeltaTime * 800f;
				yield return null;
			}
			Speed.Y = 0f;
			if (wasSummitJump)
			{
				yield return 0.2f;
				Play("event:/char/madeline/summit_areastart");
				Sprite.Play("launchRecover");
				yield return 0.1f;
			}
			else
			{
				yield return 0.1f;
			}
			if (!wasSummitJump)
			{
				Sprite.Play("fallSlow");
			}
			while (!onGround)
			{
				Speed.Y += Engine.DeltaTime * 800f;
				yield return null;
			}
			if (StateMachine.PreviousState != 10)
			{
				Position = start;
			}
			base.Depth = 0;
			level.DirectionalShake(Vector2.UnitY);
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			if (wasSummitJump)
			{
				level.Particles.Emit(P_SummitLandA, 12, base.BottomCenter, Vector2.UnitX * 3f, -(float)Math.PI / 2f);
				level.Particles.Emit(P_SummitLandB, 8, base.BottomCenter - Vector2.UnitX * 2f, Vector2.UnitX * 2f, 3.403392f);
				level.Particles.Emit(P_SummitLandB, 8, base.BottomCenter + Vector2.UnitX * 2f, Vector2.UnitX * 2f, -(float)Math.PI / 12f);
				level.ParticlesBG.Emit(P_SummitLandC, 30, base.BottomCenter, Vector2.UnitX * 5f);
				yield return 0.35f;
				for (int i = 0; i < Hair.Nodes.Count; i++)
				{
					Hair.Nodes[i] = new Vector2(0f, 2 + i);
				}
			}
			StateMachine.State = 0;
		}

		private IEnumerator IntroMoonJumpCoroutine()
		{
			Vector2 start = Position;
			Facing = Facings.Right;
			Speed = Vector2.Zero;
			Visible = false;
			base.Y = level.Bounds.Bottom + 16;
			yield return 0.5f;
			yield return MoonLanding(start);
			StateMachine.State = 0;
		}

		public IEnumerator MoonLanding(Vector2 groundPosition)
		{
			base.Depth = -1000000;
			Speed = Vector2.Zero;
			Visible = true;
			Sprite.Play("jumpSlow");
			while (base.Y > groundPosition.Y - 8f)
			{
				MoveV(-200f * Engine.DeltaTime);
				yield return null;
			}
			Speed.Y = -200f;
			while (Speed.Y < 0f)
			{
				Speed.Y += Engine.DeltaTime * 400f;
				yield return null;
			}
			Speed.Y = 0f;
			yield return 0.2f;
			Sprite.Play("fallSlow");
			float s = 100f;
			while (!OnGround())
			{
				Speed.Y += Engine.DeltaTime * s;
				s = Calc.Approach(s, 2f, Engine.DeltaTime * 50f);
				yield return null;
			}
			base.Depth = 0;
		}

		private IEnumerator IntroWakeUpCoroutine()
		{
			Sprite.Play("asleep");
			yield return 0.5f;
			yield return Sprite.PlayRoutine("wakeUp");
			yield return 0.2f;
			StateMachine.State = 0;
		}

		private void IntroRespawnBegin()
		{
			Play("event:/char/madeline/revive");
			base.Depth = -1000000;
			introEase = 1f;
			Vector2 from = Position;
			from.X = MathHelper.Clamp(from.X, (float)level.Bounds.Left + 40f, (float)level.Bounds.Right - 40f);
			from.Y = MathHelper.Clamp(from.Y, (float)level.Bounds.Top + 40f, (float)level.Bounds.Bottom - 40f);
			deadOffset = from;
			from -= Position;
			respawnTween = Tween.Create(Tween.TweenMode.Oneshot, null, 0.6f, start: true);
			respawnTween.OnUpdate = delegate(Tween t)
			{
				deadOffset = Vector2.Lerp(from, Vector2.Zero, t.Eased);
				introEase = 1f - t.Eased;
			};
			respawnTween.OnComplete = delegate
			{
				if (StateMachine.State == 14)
				{
					StateMachine.State = 0;
					Sprite.Scale = new Vector2(1.5f, 0.5f);
				}
			};
			Add(respawnTween);
		}

		private void IntroRespawnEnd()
		{
			base.Depth = 0;
			deadOffset = Vector2.Zero;
			Remove(respawnTween);
			respawnTween = null;
		}

		public IEnumerator IntroThinkForABitCoroutine()
		{
			(base.Scene as Level).Camera.X += 8f;
			yield return 0.1f;
			Sprite.Play("walk");
			float target = base.X + 8f;
			while (base.X < target)
			{
				MoveH(32f * Engine.DeltaTime);
				yield return null;
			}
			Sprite.Play("idle");
			yield return 0.3f;
			Facing = Facings.Left;
			yield return 0.8f;
			Facing = Facings.Right;
			yield return 0.1f;
			StateMachine.State = 0;
		}

		private void BirdDashTutorialBegin()
		{
			DashBegin();
			Play("event:/char/madeline/dash_red_right");
			Sprite.Play("dash");
		}

		private int BirdDashTutorialUpdate()
		{
			return 16;
		}

		private IEnumerator BirdDashTutorialCoroutine()
		{
			yield return null;
			CreateTrail();
			Add(Alarm.Create(Alarm.AlarmMode.Oneshot, CreateTrail, 0.08f, start: true));
			Add(Alarm.Create(Alarm.AlarmMode.Oneshot, CreateTrail, 0.15f, start: true));
			Vector2 aim = new Vector2(1f, -1f).SafeNormalize();
			Facing = Facings.Right;
			Speed = aim * 240f;
			DashDir = aim;
			SceneAs<Level>().DirectionalShake(DashDir, 0.2f);
			SlashFx.Burst(base.Center, DashDir.Angle());
			for (float time2 = 0f; time2 < 0.15f; time2 += Engine.DeltaTime)
			{
				if (Speed != Vector2.Zero && level.OnInterval(0.02f))
				{
					level.ParticlesFG.Emit(P_DashA, base.Center + Calc.Random.Range(Vector2.One * -2f, Vector2.One * 2f), DashDir.Angle());
				}
				yield return null;
			}
			AutoJump = true;
			AutoJumpTimer = 0f;
			if (DashDir.Y <= 0f)
			{
				Speed = DashDir * 160f;
			}
			if (Speed.Y < 0f)
			{
				Speed.Y *= 0.75f;
			}
			Sprite.Play("fallFast");
			bool climbing = false;
			while (!OnGround() && !climbing)
			{
				Speed.Y = Calc.Approach(Speed.Y, 160f, 900f * Engine.DeltaTime);
				if (CollideCheck<Solid>(Position + new Vector2(1f, 0f)))
				{
					climbing = true;
				}
				if (base.Top > (float)level.Bounds.Bottom)
				{
					level.CancelCutscene();
					Die(Vector2.Zero);
				}
				yield return null;
			}
			if (climbing)
			{
				Sprite.Play("wallslide");
				Dust.Burst(Position + new Vector2(4f, -6f), new Vector2(-4f, 0f).Angle());
				Speed.Y = 0f;
				yield return 0.2f;
				Sprite.Play("climbUp");
				while (CollideCheck<Solid>(Position + new Vector2(1f, 0f)))
				{
					base.Y += -45f * Engine.DeltaTime;
					yield return null;
				}
				base.Y = (float)Math.Round(base.Y);
				Play("event:/char/madeline/climb_ledge");
				Sprite.Play("jumpFast");
				Speed.Y = -105f;
				while (!OnGround())
				{
					Speed.Y = Calc.Approach(Speed.Y, 160f, 900f * Engine.DeltaTime);
					Speed.X = 20f;
					yield return null;
				}
				Speed.X = 0f;
				Speed.Y = 0f;
				Sprite.Play("walk");
				for (float time2 = 0f; time2 < 0.5f; time2 += Engine.DeltaTime)
				{
					base.X += 32f * Engine.DeltaTime;
					yield return null;
				}
				Sprite.Play("tired");
				yield break;
			}
			Sprite.Play("tired");
			Speed.Y = 0f;
			while (Speed.X != 0f)
			{
				Speed.X = Calc.Approach(Speed.X, 0f, 240f * Engine.DeltaTime);
				if (base.Scene.OnInterval(0.04f))
				{
					Dust.Burst(base.BottomCenter + new Vector2(0f, -2f), (float)Math.PI * -3f / 4f);
				}
				yield return null;
			}
		}

		public EventInstance? Play(string sound, string param = null, float value = 0f)
		{
			float raining = 0f;
			if (base.Scene is Level level && level.Raining)
			{
				raining = 1f;
			}
			AddChaserStateSound(sound, param, value);
			return Audio.Play(sound, base.Center, param, value, "raining", raining);
		}

		public void Loop(SoundSource sfx, string sound)
		{
			AddChaserStateSound(sound, null, 0f, ChaserStateSound.Actions.Loop);
			sfx.Play(sound);
		}

		public void Stop(SoundSource sfx)
		{
			if (sfx.Playing)
			{
				AddChaserStateSound(sfx.EventName, null, 0f, ChaserStateSound.Actions.Stop);
				sfx.Stop();
			}
		}

		private void AddChaserStateSound(string sound, ChaserStateSound.Actions action)
		{
			AddChaserStateSound(sound, null, 0f, action);
		}

		private void AddChaserStateSound(string sound, string param = null, float value = 0f, ChaserStateSound.Actions action = ChaserStateSound.Actions.Oneshot)
		{
			string eventName = null;
			SFX.MadelineToBadelineSound.TryGetValue(sound, out eventName);
			if (eventName != null)
			{
				activeSounds.Add(new ChaserStateSound
				{
					Event = eventName,
					Parameter = param,
					ParameterValue = value,
					Action = action
				});
			}
		}

		private ParticleType DustParticleFromSurfaceIndex(int index)
		{
			if (index == 40)
			{
				return ParticleTypes.SparkyDust;
			}
			return ParticleTypes.Dust;
		}
	}
}
