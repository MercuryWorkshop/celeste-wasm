using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class Seeker : Actor
	{
		private struct PatrolPoint
		{
			public Vector2 Point;

			public float Distance;
		}

		[Pooled]
		private class RecoverBlast : Entity
		{
			private Sprite sprite;

			public override void Added(Scene scene)
			{
				base.Added(scene);
				base.Depth = -199;
				if (sprite == null)
				{
					Add(sprite = GFX.SpriteBank.Create("seekerShockWave"));
					sprite.OnLastFrame = delegate
					{
						RemoveSelf();
					};
				}
				sprite.Play("shockwave", restart: true);
			}

			public static void Spawn(Vector2 position)
			{
				RecoverBlast blast = Engine.Pooler.Create<RecoverBlast>();
				blast.Position = position;
				Engine.Scene.Add(blast);
			}
		}

		public static ParticleType P_Attack;

		public static ParticleType P_HitWall;

		public static ParticleType P_Stomp;

		public static ParticleType P_Regen;

		public static ParticleType P_BreakOut;

		public static readonly Color TrailColor = Calc.HexToColor("99e550");

		private const int StIdle = 0;

		private const int StPatrol = 1;

		private const int StSpotted = 2;

		private const int StAttack = 3;

		private const int StStunned = 4;

		private const int StSkidding = 5;

		private const int StRegenerate = 6;

		private const int StReturned = 7;

		private const int size = 12;

		private const int bounceWidth = 16;

		private const int bounceHeight = 4;

		private const float Accel = 600f;

		private const float WallCollideStunThreshold = 100f;

		private const float StunXSpeed = 100f;

		private const float BounceSpeed = 200f;

		private const float SightDistSq = 25600f;

		private const float ExplodeRadius = 40f;

		private Hitbox physicsHitbox;

		private Hitbox breakWallsHitbox;

		private Hitbox attackHitbox;

		private Hitbox bounceHitbox;

		private Circle pushRadius;

		private Circle breakWallsRadius;

		private StateMachine State;

		private Vector2 lastSpottedAt;

		private Vector2 lastPathTo;

		private bool spotted;

		private bool canSeePlayer;

		private Collision onCollideH;

		private Collision onCollideV;

		private Random random;

		private Vector2 lastPosition;

		private Shaker shaker;

		private Wiggler scaleWiggler;

		private bool lastPathFound;

		private List<Vector2> path;

		private int pathIndex;

		private Vector2[] patrolPoints;

		private SineWave idleSineX;

		private SineWave idleSineY;

		public VertexLight Light;

		private bool dead;

		private SoundSource boopedSfx;

		private SoundSource aggroSfx;

		private SoundSource reviveSfx;

		private Sprite sprite;

		private int facing = 1;

		private int spriteFacing = 1;

		private string nextSprite;

		private HoldableCollider theo;

		private HashSet<string> flipAnimations = new HashSet<string> { "flipMouth", "flipEyes", "skid" };

		public Vector2 Speed;

		private const float FarDistSq = 12544f;

		private const float IdleAccel = 200f;

		private const float IdleSpeed = 50f;

		private const float PatrolSpeed = 25f;

		private const int PatrolChoices = 3;

		private const float PatrolWaitTime = 0.4f;

		private static PatrolPoint[] patrolChoices = new PatrolPoint[3];

		private float patrolWaitTimer;

		private const float SpottedTargetSpeed = 60f;

		private const float SpottedFarSpeed = 90f;

		private const float SpottedMaxYDist = 24f;

		private const float AttackMinXDist = 16f;

		private const float SpottedLosePlayerTime = 0.6f;

		private const float SpottedMinAttackTime = 0.2f;

		private float spottedLosePlayerTimer;

		private float spottedTurnDelay;

		private const float AttackWindUpSpeed = -60f;

		private const float AttackWindUpTime = 0.3f;

		private const float AttackStartSpeed = 180f;

		private const float AttackTargetSpeed = 260f;

		private const float AttackAccel = 300f;

		private const float DirectionDotThreshold = 0.4f;

		private const int AttackTargetUpShift = 2;

		private const float AttackMaxRotateRadians = 0.61086524f;

		private float attackSpeed;

		private bool attackWindUp;

		private const float StunnedAccel = 150f;

		private const float StunTime = 0.8f;

		private const float SkiddingAccel = 200f;

		private const float StrongSkiddingAccel = 400f;

		private const float StrongSkiddingTime = 0.08f;

		private bool strongSkid;

		public bool Attacking
		{
			get
			{
				if (State.State == 3)
				{
					return !attackWindUp;
				}
				return false;
			}
		}

		public bool Spotted
		{
			get
			{
				if (State.State != 3)
				{
					return State.State == 2;
				}
				return true;
			}
		}

		public bool Regenerating => State.State == 6;

		private Vector2 FollowTarget => lastSpottedAt - Vector2.UnitY * 2f;

		public Seeker(Vector2 position, Vector2[] patrolPoints)
			: base(position)
		{
			base.Depth = -200;
			this.patrolPoints = patrolPoints;
			lastPosition = position;
			base.Collider = (physicsHitbox = new Hitbox(6f, 6f, -3f, -3f));
			breakWallsHitbox = new Hitbox(6f, 14f, -3f, -7f);
			attackHitbox = new Hitbox(12f, 8f, -6f, -2f);
			bounceHitbox = new Hitbox(16f, 6f, -8f, -8f);
			pushRadius = new Circle(40f);
			breakWallsRadius = new Circle(16f);
			Add(new PlayerCollider(OnAttackPlayer, attackHitbox));
			Add(new PlayerCollider(OnBouncePlayer, bounceHitbox));
			Add(shaker = new Shaker(on: false));
			Add(State = new StateMachine());
			State.SetCallbacks(0, IdleUpdate, IdleCoroutine);
			State.SetCallbacks(1, PatrolUpdate, null, PatrolBegin);
			State.SetCallbacks(2, SpottedUpdate, SpottedCoroutine, SpottedBegin);
			State.SetCallbacks(3, AttackUpdate, AttackCoroutine, AttackBegin);
			State.SetCallbacks(4, StunnedUpdate, StunnedCoroutine);
			State.SetCallbacks(5, SkiddingUpdate, SkiddingCoroutine, SkiddingBegin, SkiddingEnd);
			State.SetCallbacks(6, RegenerateUpdate, RegenerateCoroutine, RegenerateBegin, RegenerateEnd);
			State.SetCallbacks(7, null, ReturnedCoroutine);
			onCollideH = OnCollideH;
			onCollideV = OnCollideV;
			Add(idleSineX = new SineWave(0.5f));
			Add(idleSineY = new SineWave(0.7f));
			Add(Light = new VertexLight(Color.White, 1f, 32, 64));
			Add(theo = new HoldableCollider(OnHoldable, attackHitbox));
			Add(new MirrorReflection());
			path = new List<Vector2>();
			IgnoreJumpThrus = true;
			Add(sprite = GFX.SpriteBank.Create("seeker"));
			sprite.OnLastFrame = delegate(string f)
			{
				if (flipAnimations.Contains(f) && spriteFacing != facing)
				{
					spriteFacing = facing;
					if (nextSprite != null)
					{
						sprite.Play(nextSprite);
						nextSprite = null;
					}
				}
			};
			sprite.OnChange = delegate(string last, string next)
			{
				nextSprite = null;
				sprite.OnLastFrame(last);
			};
			SquishCallback = delegate(CollisionData d)
			{
				if (!dead && !TrySquishWiggle(d))
				{
					Entity entity = new Entity(Position);
					DeathEffect component = new DeathEffect(Color.HotPink, base.Center - Position)
					{
						OnEnd = delegate
						{
							entity.RemoveSelf();
						}
					};
					entity.Add(component);
					entity.Depth = -1000000;
					base.Scene.Add(entity);
					Audio.Play("event:/game/05_mirror_temple/seeker_death", Position);
					RemoveSelf();
					dead = true;
				}
			};
			scaleWiggler = Wiggler.Create(0.8f, 2f);
			Add(scaleWiggler);
			Add(boopedSfx = new SoundSource());
			Add(aggroSfx = new SoundSource());
			Add(reviveSfx = new SoundSource());
		}

		public Seeker(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.NodesOffset(offset))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			random = new Random(SceneAs<Level>().Session.LevelData.LoadSeed);
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player == null || base.X == player.X)
			{
				SnapFacing(1f);
			}
			else
			{
				SnapFacing(Math.Sign(player.X - base.X));
			}
		}

		public override bool IsRiding(JumpThru jumpThru)
		{
			return false;
		}

		public override bool IsRiding(Solid solid)
		{
			return false;
		}

		private void OnAttackPlayer(Player player)
		{
			if (State.State != 4)
			{
				player.Die((player.Center - Position).SafeNormalize());
				return;
			}
			Collider was = base.Collider;
			base.Collider = bounceHitbox;
			player.PointBounce(base.Center);
			Speed = (base.Center - player.Center).SafeNormalize(100f);
			scaleWiggler.Start();
			base.Collider = was;
		}

		private void OnBouncePlayer(Player player)
		{
			Collider was = base.Collider;
			base.Collider = attackHitbox;
			if (CollideCheck(player))
			{
				OnAttackPlayer(player);
			}
			else
			{
				player.Bounce(base.Top);
				GotBouncedOn(player);
			}
			base.Collider = was;
		}

		private void GotBouncedOn(Entity entity)
		{
			Celeste.Freeze(0.15f);
			Speed = (base.Center - entity.Center).SafeNormalize(200f);
			State.State = 6;
			sprite.Scale = new Vector2(1.4f, 0.6f);
			SceneAs<Level>().Particles.Emit(P_Stomp, 8, base.Center - Vector2.UnitY * 5f, new Vector2(6f, 3f));
		}

		public void HitSpring()
		{
			Speed.Y = -150f;
		}

		private bool CanSeePlayer(Player player)
		{
			if (player == null)
			{
				return false;
			}
			if (State.State != 2 && !SceneAs<Level>().InsideCamera(base.Center) && Vector2.DistanceSquared(base.Center, player.Center) > 25600f)
			{
				return false;
			}
			Vector2 perp = (player.Center - base.Center).Perpendicular().SafeNormalize(2f);
			if (!base.Scene.CollideCheck<Solid>(base.Center + perp, player.Center + perp))
			{
				return !base.Scene.CollideCheck<Solid>(base.Center - perp, player.Center - perp);
			}
			return false;
		}

		public override void Update()
		{
			Light.Alpha = Calc.Approach(Light.Alpha, 1f, Engine.DeltaTime * 2f);
			foreach (Entity entity in base.Scene.Tracker.GetEntities<SeekerBarrier>())
			{
				entity.Collidable = true;
			}
			sprite.Scale.X = Calc.Approach(sprite.Scale.X, 1f, 2f * Engine.DeltaTime);
			sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 1f, 2f * Engine.DeltaTime);
			if (State.State == 6)
			{
				canSeePlayer = false;
			}
			else
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				canSeePlayer = CanSeePlayer(player);
				if (canSeePlayer)
				{
					spotted = true;
					lastSpottedAt = player.Center;
				}
			}
			if (lastPathTo != lastSpottedAt)
			{
				lastPathTo = lastSpottedAt;
				pathIndex = 0;
				lastPathFound = SceneAs<Level>().Pathfinder.Find(ref path, base.Center, FollowTarget);
			}
			base.Update();
			lastPosition = Position;
			MoveH(Speed.X * Engine.DeltaTime, onCollideH);
			MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
			Level level = SceneAs<Level>();
			if (base.Left < (float)level.Bounds.Left && Speed.X < 0f)
			{
				base.Left = level.Bounds.Left;
				onCollideH(CollisionData.Empty);
			}
			else if (base.Right > (float)level.Bounds.Right && Speed.X > 0f)
			{
				base.Right = level.Bounds.Right;
				onCollideH(CollisionData.Empty);
			}
			if (base.Top < (float)(level.Bounds.Top + -8) && Speed.Y < 0f)
			{
				base.Top = level.Bounds.Top + -8;
				onCollideV(CollisionData.Empty);
			}
			else if (base.Bottom > (float)level.Bounds.Bottom && Speed.Y > 0f)
			{
				base.Bottom = level.Bounds.Bottom;
				onCollideV(CollisionData.Empty);
			}
			foreach (SeekerCollider component in base.Scene.Tracker.GetComponents<SeekerCollider>())
			{
				component.Check(this);
			}
			if (State.State == 3 && Speed.X > 0f)
			{
				bounceHitbox.Width = 16f;
				bounceHitbox.Position.X = -10f;
			}
			else if (State.State == 3 && Speed.Y < 0f)
			{
				bounceHitbox.Width = 16f;
				bounceHitbox.Position.X = -6f;
			}
			else
			{
				bounceHitbox.Width = 12f;
				bounceHitbox.Position.X = -6f;
			}
			foreach (Entity entity2 in base.Scene.Tracker.GetEntities<SeekerBarrier>())
			{
				entity2.Collidable = false;
			}
		}

		private void TurnFacing(float dir, string gotoSprite = null)
		{
			if (dir != 0f)
			{
				facing = Math.Sign(dir);
			}
			if (spriteFacing != facing)
			{
				if (State.State == 5)
				{
					sprite.Play("skid");
				}
				else if (State.State == 3 || State.State == 2)
				{
					sprite.Play("flipMouth");
				}
				else
				{
					sprite.Play("flipEyes");
				}
				nextSprite = gotoSprite;
			}
			else if (gotoSprite != null)
			{
				sprite.Play(gotoSprite);
			}
		}

		private void SnapFacing(float dir)
		{
			if (dir != 0f)
			{
				spriteFacing = (facing = Math.Sign(dir));
			}
		}

		private void OnHoldable(Holdable holdable)
		{
			if (State.State != 6 && holdable.Dangerous(theo))
			{
				holdable.HitSeeker(this);
				State.State = 4;
				Speed = (base.Center - holdable.Entity.Center).SafeNormalize(120f);
				scaleWiggler.Start();
			}
			else if ((State.State == 3 || State.State == 5) && holdable.IsHeld)
			{
				holdable.Swat(theo, Math.Sign(Speed.X));
				State.State = 4;
				Speed = (base.Center - holdable.Entity.Center).SafeNormalize(120f);
				scaleWiggler.Start();
			}
		}

		public override void Render()
		{
			Vector2 wasPos = Position;
			Position += shaker.Value;
			Vector2 wasScale = sprite.Scale;
			sprite.Scale *= 1f - 0.3f * scaleWiggler.Value;
			sprite.Scale.X *= spriteFacing;
			base.Render();
			Position = wasPos;
			sprite.Scale = wasScale;
		}

		public override void DebugRender(Camera camera)
		{
			Collider was = base.Collider;
			base.Collider = attackHitbox;
			attackHitbox.Render(camera, Color.Red);
			base.Collider = bounceHitbox;
			bounceHitbox.Render(camera, Color.Aqua);
			base.Collider = was;
		}

		private void SlammedIntoWall(CollisionData data)
		{
			float dir;
			float atX;
			if (data.Direction.X > 0f)
			{
				dir = (float)Math.PI;
				atX = base.Right;
			}
			else
			{
				dir = 0f;
				atX = base.Left;
			}
			SceneAs<Level>().Particles.Emit(P_HitWall, 12, new Vector2(atX, base.Y), Vector2.UnitY * 4f, dir);
			if (data.Hit is DashSwitch)
			{
				(data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
			}
			base.Collider = breakWallsHitbox;
			foreach (TempleCrackedBlock block in base.Scene.Tracker.GetEntities<TempleCrackedBlock>())
			{
				if (CollideCheck(block, Position + Vector2.UnitX * Math.Sign(Speed.X)))
				{
					block.Break(base.Center);
				}
			}
			base.Collider = physicsHitbox;
			SceneAs<Level>().DirectionalShake(Vector2.UnitX * Math.Sign(Speed.X));
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			Speed.X = (float)Math.Sign(Speed.X) * -100f;
			Speed.Y *= 0.4f;
			sprite.Scale.X = 0.6f;
			sprite.Scale.Y = 1.4f;
			shaker.ShakeFor(0.5f, removeOnFinish: false);
			scaleWiggler.Start();
			State.State = 4;
			if (data.Hit is SeekerBarrier)
			{
				(data.Hit as SeekerBarrier).OnReflectSeeker();
				Audio.Play("event:/game/05_mirror_temple/seeker_hit_lightwall", Position);
			}
			else
			{
				Audio.Play("event:/game/05_mirror_temple/seeker_hit_normal", Position);
			}
		}

		private void OnCollideH(CollisionData data)
		{
			if (State.State == 3 && data.Hit != null)
			{
				int xDir = Math.Sign(Speed.X);
				if ((!CollideCheck<Solid>(Position + new Vector2(xDir, 4f)) && !MoveVExact(4)) || (!CollideCheck<Solid>(Position + new Vector2(xDir, -4f)) && !MoveVExact(-4)))
				{
					return;
				}
			}
			if ((State.State == 3 || State.State == 5) && Math.Abs(Speed.X) >= 100f)
			{
				SlammedIntoWall(data);
			}
			else
			{
				Speed.X *= -0.2f;
			}
		}

		private void OnCollideV(CollisionData data)
		{
			if (State.State == 3)
			{
				Speed.Y *= -0.6f;
			}
			else
			{
				Speed.Y *= -0.2f;
			}
		}

		private void CreateTrail()
		{
			Vector2 wasScale = sprite.Scale;
			sprite.Scale *= 1f - 0.3f * scaleWiggler.Value;
			sprite.Scale.X *= spriteFacing;
			TrailManager.Add(this, TrailColor, 0.5f);
			sprite.Scale = wasScale;
		}

		private int IdleUpdate()
		{
			if (canSeePlayer)
			{
				return 2;
			}
			Vector2 targetSpeed = Vector2.Zero;
			if (spotted && Vector2.DistanceSquared(base.Center, FollowTarget) > 64f)
			{
				float speed = GetSpeedMagnitude(50f);
				targetSpeed = ((!lastPathFound) ? (FollowTarget - base.Center).SafeNormalize(speed) : GetPathSpeed(speed));
			}
			if (targetSpeed == Vector2.Zero)
			{
				targetSpeed.X = idleSineX.Value * 6f;
				targetSpeed.Y = idleSineY.Value * 6f;
			}
			Speed = Calc.Approach(Speed, targetSpeed, 200f * Engine.DeltaTime);
			if (Speed.LengthSquared() > 400f)
			{
				TurnFacing(Speed.X);
			}
			if (spriteFacing == facing)
			{
				sprite.Play("idle");
			}
			return 0;
		}

		private IEnumerator IdleCoroutine()
		{
			if (patrolPoints != null && patrolPoints.Length != 0 && spotted)
			{
				while (Vector2.DistanceSquared(base.Center, FollowTarget) > 64f)
				{
					yield return null;
				}
				yield return 0.3f;
				State.State = 1;
			}
		}

		private Vector2 GetPathSpeed(float magnitude)
		{
			if (pathIndex >= path.Count)
			{
				return Vector2.Zero;
			}
			if (Vector2.DistanceSquared(base.Center, path[pathIndex]) < 36f)
			{
				pathIndex++;
				return GetPathSpeed(magnitude);
			}
			return (path[pathIndex] - base.Center).SafeNormalize(magnitude);
		}

		private float GetSpeedMagnitude(float baseMagnitude)
		{
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				if (Vector2.DistanceSquared(base.Center, player.Center) > 12544f)
				{
					return baseMagnitude * 3f;
				}
				return baseMagnitude * 1.5f;
			}
			return baseMagnitude;
		}

		private void PatrolBegin()
		{
			State.State = ChoosePatrolTarget();
			patrolWaitTimer = 0f;
		}

		private int PatrolUpdate()
		{
			if (canSeePlayer)
			{
				return 2;
			}
			if (patrolWaitTimer > 0f)
			{
				patrolWaitTimer -= Engine.DeltaTime;
				if (patrolWaitTimer <= 0f)
				{
					return ChoosePatrolTarget();
				}
			}
			else if (Vector2.DistanceSquared(base.Center, lastSpottedAt) < 144f)
			{
				patrolWaitTimer = 0.4f;
			}
			float mag = GetSpeedMagnitude(25f);
			Speed = Calc.Approach(target: (!lastPathFound) ? (FollowTarget - base.Center).SafeNormalize(mag) : GetPathSpeed(mag), val: Speed, maxMove: 600f * Engine.DeltaTime);
			if (Speed.LengthSquared() > 100f)
			{
				TurnFacing(Speed.X);
			}
			if (spriteFacing == facing)
			{
				sprite.Play("search");
			}
			return 1;
		}

		private int ChoosePatrolTarget()
		{
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player == null)
			{
				return 0;
			}
			for (int j = 0; j < 3; j++)
			{
				patrolChoices[j].Distance = 0f;
			}
			int found = 0;
			Vector2[] array = patrolPoints;
			foreach (Vector2 pt in array)
			{
				if (Vector2.DistanceSquared(base.Center, pt) < 576f)
				{
					continue;
				}
				float dist = Vector2.DistanceSquared(pt, player.Center);
				for (int i = 0; i < 3; i++)
				{
					if (dist < patrolChoices[i].Distance || patrolChoices[i].Distance <= 0f)
					{
						found++;
						for (int k = 2; k > i; k--)
						{
							patrolChoices[k].Distance = patrolChoices[k - 1].Distance;
							patrolChoices[k].Point = patrolChoices[k - 1].Point;
						}
						patrolChoices[i].Distance = dist;
						patrolChoices[i].Point = pt;
						break;
					}
				}
			}
			if (found <= 0)
			{
				return 0;
			}
			lastSpottedAt = patrolChoices[random.Next(Math.Min(3, found))].Point;
			lastPathTo = lastSpottedAt;
			pathIndex = 0;
			lastPathFound = SceneAs<Level>().Pathfinder.Find(ref path, base.Center, FollowTarget);
			return 1;
		}

		private void SpottedBegin()
		{
			aggroSfx.Play("event:/game/05_mirror_temple/seeker_aggro");
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				TurnFacing(player.X - base.X, "spot");
			}
			spottedLosePlayerTimer = 0.6f;
			spottedTurnDelay = 1f;
		}

		private int SpottedUpdate()
		{
			if (!canSeePlayer)
			{
				spottedLosePlayerTimer -= Engine.DeltaTime;
				if (spottedLosePlayerTimer < 0f)
				{
					return 0;
				}
			}
			else
			{
				spottedLosePlayerTimer = 0.6f;
			}
			float mag = GetSpeedMagnitude(60f);
			Vector2 targetSpeed = ((!lastPathFound) ? (FollowTarget - base.Center).SafeNormalize(mag) : GetPathSpeed(mag));
			if (Vector2.DistanceSquared(base.Center, FollowTarget) < 2500f && base.Y < FollowTarget.Y)
			{
				float angle = targetSpeed.Angle();
				if (base.Y < FollowTarget.Y - 2f)
				{
					angle = Calc.AngleLerp(angle, (float)Math.PI / 2f, 0.5f);
				}
				else if (base.Y > FollowTarget.Y + 2f)
				{
					angle = Calc.AngleLerp(angle, -(float)Math.PI / 2f, 0.5f);
				}
				targetSpeed = Calc.AngleToVector(angle, 60f);
				Vector2 add = Vector2.UnitX * Math.Sign(base.X - lastSpottedAt.X) * 48f;
				if (Math.Abs(base.X - lastSpottedAt.X) < 36f && !CollideCheck<Solid>(Position + add) && !CollideCheck<Solid>(lastSpottedAt + add))
				{
					targetSpeed.X = Math.Sign(base.X - lastSpottedAt.X) * 60;
				}
			}
			Speed = Calc.Approach(Speed, targetSpeed, 600f * Engine.DeltaTime);
			spottedTurnDelay -= Engine.DeltaTime;
			if (spottedTurnDelay <= 0f)
			{
				TurnFacing(Speed.X, "spotted");
			}
			return 2;
		}

		private IEnumerator SpottedCoroutine()
		{
			yield return 0.2f;
			while (!CanAttack())
			{
				yield return null;
			}
			State.State = 3;
		}

		private bool CanAttack()
		{
			if (Math.Abs(base.Y - lastSpottedAt.Y) > 24f)
			{
				return false;
			}
			if (Math.Abs(base.X - lastSpottedAt.X) < 16f)
			{
				return false;
			}
			Vector2 aim = (FollowTarget - base.Center).SafeNormalize();
			if (Vector2.Dot(-Vector2.UnitY, aim) > 0.5f || Vector2.Dot(Vector2.UnitY, aim) > 0.5f)
			{
				return false;
			}
			if (CollideCheck<Solid>(Position + Vector2.UnitX * Math.Sign(lastSpottedAt.X - base.X) * 24f))
			{
				return false;
			}
			return true;
		}

		private void AttackBegin()
		{
			Audio.Play("event:/game/05_mirror_temple/seeker_dash", Position);
			attackWindUp = true;
			attackSpeed = -60f;
			Speed = (FollowTarget - base.Center).SafeNormalize(-60f);
		}

		private int AttackUpdate()
		{
			if (!attackWindUp)
			{
				Vector2 targetDir = (FollowTarget - base.Center).SafeNormalize();
				if (Vector2.Dot(Speed.SafeNormalize(), targetDir) < 0.4f)
				{
					return 5;
				}
				attackSpeed = Calc.Approach(attackSpeed, 260f, 300f * Engine.DeltaTime);
				Speed = Speed.RotateTowards(targetDir.Angle(), 0.61086524f * Engine.DeltaTime).SafeNormalize(attackSpeed);
				if (base.Scene.OnInterval(0.04f))
				{
					Vector2 normal = (-Speed).SafeNormalize();
					SceneAs<Level>().Particles.Emit(P_Attack, 2, Position + normal * 4f, Vector2.One * 4f, normal.Angle());
				}
				if (base.Scene.OnInterval(0.06f))
				{
					CreateTrail();
				}
			}
			return 3;
		}

		private IEnumerator AttackCoroutine()
		{
			TurnFacing(lastSpottedAt.X - base.X, "windUp");
			yield return 0.3f;
			attackWindUp = false;
			attackSpeed = 180f;
			Speed = (lastSpottedAt - Vector2.UnitY * 2f - base.Center).SafeNormalize(180f);
			SnapFacing(Speed.X);
		}

		private int StunnedUpdate()
		{
			Speed = Calc.Approach(Speed, Vector2.Zero, 150f * Engine.DeltaTime);
			return 4;
		}

		private IEnumerator StunnedCoroutine()
		{
			yield return 0.8f;
			State.State = 0;
		}

		private void SkiddingBegin()
		{
			Audio.Play("event:/game/05_mirror_temple/seeker_dash_turn", Position);
			strongSkid = false;
			TurnFacing(-facing);
		}

		private int SkiddingUpdate()
		{
			Speed = Calc.Approach(Speed, Vector2.Zero, (strongSkid ? 400f : 200f) * Engine.DeltaTime);
			if (Speed.LengthSquared() < 400f)
			{
				if (canSeePlayer)
				{
					return 2;
				}
				return 0;
			}
			return 5;
		}

		private IEnumerator SkiddingCoroutine()
		{
			yield return 0.08f;
			strongSkid = true;
		}

		private void SkiddingEnd()
		{
			spriteFacing = facing;
		}

		private void RegenerateBegin()
		{
			Audio.Play("event:/game/general/thing_booped", Position);
			boopedSfx.Play("event:/game/05_mirror_temple/seeker_booped");
			sprite.Play("takeHit");
			Collidable = false;
			State.Locked = true;
			Light.StartRadius = 16f;
			Light.EndRadius = 32f;
		}

		private void RegenerateEnd()
		{
			reviveSfx.Play("event:/game/05_mirror_temple/seeker_revive");
			Collidable = true;
			Light.StartRadius = 32f;
			Light.EndRadius = 64f;
		}

		private int RegenerateUpdate()
		{
			Speed.X = Calc.Approach(Speed.X, 0f, 150f * Engine.DeltaTime);
			Speed = Calc.Approach(Speed, Vector2.Zero, 150f * Engine.DeltaTime);
			return 6;
		}

		private IEnumerator RegenerateCoroutine()
		{
			yield return 1f;
			shaker.On = true;
			yield return 0.2f;
			sprite.Play("pulse");
			yield return 0.5f;
			sprite.Play("recover");
			RecoverBlast.Spawn(Position);
			yield return 0.15f;
			base.Collider = pushRadius;
			Player player = CollideFirst<Player>();
			if (player != null && !base.Scene.CollideCheck<Solid>(Position, player.Center))
			{
				player.ExplodeLaunch(Position);
			}
			TheoCrystal theo = CollideFirst<TheoCrystal>();
			if (theo != null && !base.Scene.CollideCheck<Solid>(Position, theo.Center))
			{
				theo.ExplodeLaunch(Position);
			}
			foreach (TempleCrackedBlock wall in base.Scene.Tracker.GetEntities<TempleCrackedBlock>())
			{
				if (CollideCheck(wall))
				{
					wall.Break(Position);
				}
			}
			foreach (TouchSwitch sw in base.Scene.Tracker.GetEntities<TouchSwitch>())
			{
				if (CollideCheck(sw))
				{
					sw.TurnOn();
				}
			}
			base.Collider = physicsHitbox;
			Level level = SceneAs<Level>();
			level.Displacement.AddBurst(Position, 0.4f, 12f, 36f, 0.5f);
			level.Displacement.AddBurst(Position, 0.4f, 24f, 48f, 0.5f);
			level.Displacement.AddBurst(Position, 0.4f, 36f, 60f, 0.5f);
			for (float i = 0f; i < (float)Math.PI * 2f; i += 0.17453292f)
			{
				Vector2 at = base.Center + Calc.AngleToVector(i + Calc.Random.Range(-(float)Math.PI / 90f, (float)Math.PI / 90f), Calc.Random.Range(12, 18));
				level.Particles.Emit(P_Regen, at, i);
			}
			shaker.On = false;
			State.Locked = false;
			State.State = 7;
		}

		private IEnumerator ReturnedCoroutine()
		{
			yield return 0.3f;
			State.State = 0;
		}
	}
}
