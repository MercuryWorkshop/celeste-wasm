using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class PlayerSeeker : Actor
	{
		private Facings facing;

		private Sprite sprite;

		private Vector2 speed;

		private bool enabled;

		private float dashTimer;

		private Vector2 dashDirection;

		private float trailTimerA;

		private float trailTimerB;

		private Shaker shaker;

		public Vector2 CameraTarget
		{
			get
			{
				Rectangle bounds = (base.Scene as Level).Bounds;
				return (Position + new Vector2(-160f, -90f)).Clamp(bounds.Left, bounds.Top, bounds.Right - 320, bounds.Bottom - 180);
			}
		}

		public PlayerSeeker(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			Add(sprite = GFX.SpriteBank.Create("seeker"));
			sprite.Play("statue");
			sprite.OnLastFrame = delegate(string a)
			{
				if (a == "flipMouth" || a == "flipEyes")
				{
					facing = (Facings)(0 - facing);
				}
			};
			base.Collider = new Hitbox(10f, 10f, -5f, -5f);
			Add(new MirrorReflection());
			Add(new PlayerCollider(OnPlayer));
			Add(new VertexLight(Color.White, 1f, 32, 64));
			facing = Facings.Right;
			Add(shaker = new Shaker(on: false));
			Add(new Coroutine(IntroSequence()));
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			Level obj = scene as Level;
			obj.Session.ColorGrade = "templevoid";
			obj.ScreenPadding = 32f;
			obj.CanRetry = false;
		}

		private IEnumerator IntroSequence()
		{
			Level level = base.Scene as Level;
			yield return null;
			Glitch.Value = 0.05f;
			level.Tracker.GetEntity<Player>()?.StartTempleMirrorVoidSleep();
			yield return 3f;
			Vector2 from = level.Camera.Position;
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 2f, start: true);
			tween.OnUpdate = delegate(Tween f)
			{
				Vector2 cameraTarget = CameraTarget;
				level.Camera.Position = from + (cameraTarget - from) * f.Eased;
			};
			Add(tween);
			yield return 2f;
			shaker.ShakeFor(0.5f, removeOnFinish: false);
			BreakOutParticles();
			Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
			yield return 1f;
			shaker.ShakeFor(0.5f, removeOnFinish: false);
			BreakOutParticles();
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Long);
			yield return 1f;
			BreakOutParticles();
			Audio.Play("event:/game/05_mirror_temple/seeker_statue_break", Position);
			shaker.ShakeFor(1f, removeOnFinish: false);
			sprite.Play("hatch");
			Input.Rumble(RumbleStrength.Strong, RumbleLength.FullSecond);
			enabled = true;
			yield return 0.8f;
			BreakOutParticles();
			yield return 0.7f;
		}

		private void BreakOutParticles()
		{
			Level level = SceneAs<Level>();
			for (float i = 0f; i < (float)Math.PI * 2f; i += 0.17453292f)
			{
				Vector2 at = base.Center + Calc.AngleToVector(i + Calc.Random.Range(-(float)Math.PI / 90f, (float)Math.PI / 90f), Calc.Random.Range(12, 20));
				level.Particles.Emit(Seeker.P_BreakOut, at, i);
			}
		}

		private void OnPlayer(Player player)
		{
			if (!player.Dead)
			{
				Leader.StoreStrawberries(player.Leader);
				PlayerDeadBody playerDeadBody = player.Die((player.Position - Position).SafeNormalize(), evenIfInvincible: true, registerDeathInStats: false);
				playerDeadBody.DeathAction = End;
				playerDeadBody.ActionDelay = 0.3f;
				Engine.TimeRate = 0.25f;
			}
		}

		private void End()
		{
			Level level = base.Scene as Level;
			level.OnEndOfFrame += delegate
			{
				Glitch.Value = 0f;
				Distort.Anxiety = 0f;
				Engine.TimeRate = 1f;
				level.Session.ColorGrade = null;
				level.UnloadLevel();
				level.CanRetry = true;
				level.Session.Level = "c-00";
				level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));
				level.LoadLevel(Player.IntroTypes.WakeUp);
				Leader.RestoreStrawberries(level.Tracker.GetEntity<Player>().Leader);
			};
		}

		public override void Update()
		{
			foreach (Entity entity in base.Scene.Tracker.GetEntities<SeekerBarrier>())
			{
				entity.Collidable = true;
			}
			Level level = base.Scene as Level;
			base.Update();
			sprite.Scale.X = Calc.Approach(sprite.Scale.X, 1f, 2f * Engine.DeltaTime);
			sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 1f, 2f * Engine.DeltaTime);
			if (enabled && sprite.CurrentAnimationID != "hatch")
			{
				if (dashTimer > 0f)
				{
					speed = Calc.Approach(speed, Vector2.Zero, 800f * Engine.DeltaTime);
					dashTimer -= Engine.DeltaTime;
					if (dashTimer <= 0f)
					{
						sprite.Play("spotted");
					}
					if (trailTimerA > 0f)
					{
						trailTimerA -= Engine.DeltaTime;
						if (trailTimerA <= 0f)
						{
							CreateTrail();
						}
					}
					if (trailTimerB > 0f)
					{
						trailTimerB -= Engine.DeltaTime;
						if (trailTimerB <= 0f)
						{
							CreateTrail();
						}
					}
					if (base.Scene.OnInterval(0.04f))
					{
						Vector2 normal = speed.SafeNormalize();
						SceneAs<Level>().Particles.Emit(Seeker.P_Attack, 2, Position + normal * 4f, Vector2.One * 4f, normal.Angle());
					}
				}
				else
				{
					Vector2 aim = Input.Aim.Value.SafeNormalize();
					speed += aim * 600f * Engine.DeltaTime;
					float length = speed.Length();
					if (length > 120f)
					{
						length = Calc.Approach(length, 120f, Engine.DeltaTime * 700f);
						speed = speed.SafeNormalize(length);
					}
					if (aim.Y == 0f)
					{
						speed.Y = Calc.Approach(speed.Y, 0f, 400f * Engine.DeltaTime);
					}
					if (aim.X == 0f)
					{
						speed.X = Calc.Approach(speed.X, 0f, 400f * Engine.DeltaTime);
					}
					if (aim.Length() > 0f && sprite.CurrentAnimationID == "idle")
					{
						level.Displacement.AddBurst(Position, 0.5f, 8f, 32f);
						sprite.Play("spotted");
						Audio.Play("event:/game/05_mirror_temple/seeker_playercontrolstart");
					}
					int last = Math.Sign((int)facing);
					int next = Math.Sign(speed.X);
					if (next != 0 && last != next && Math.Sign(Input.Aim.Value.X) == Math.Sign(speed.X) && Math.Abs(speed.X) > 20f && sprite.CurrentAnimationID != "flipMouth" && sprite.CurrentAnimationID != "flipEyes")
					{
						sprite.Play("flipMouth");
					}
					if (Input.Dash.Pressed)
					{
						Dash(Input.Aim.Value.EightWayNormal());
					}
				}
				MoveH(speed.X * Engine.DeltaTime, OnCollide);
				MoveV(speed.Y * Engine.DeltaTime, OnCollide);
				Position = Position.Clamp(level.Bounds.X, level.Bounds.Y, level.Bounds.Right, level.Bounds.Bottom);
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null)
				{
					float dist = (Position - player.Position).Length();
					if (dist < 200f && player.Sprite.CurrentAnimationID == "asleep")
					{
						player.Sprite.Rate = 2f;
						player.Sprite.Play("wakeUp");
					}
					else if (dist < 100f && player.Sprite.CurrentAnimationID != "wakeUp")
					{
						player.Sprite.Rate = 1f;
						player.Sprite.Play("runFast");
						player.Facing = ((!(base.X > player.X)) ? Facings.Right : Facings.Left);
					}
					if (dist < 50f && dashTimer <= 0f)
					{
						Dash((player.Center - base.Center).SafeNormalize());
					}
					Engine.TimeRate = Calc.ClampedMap(dist, 60f, 220f, 0.5f);
					Camera camera = level.Camera;
					Vector2 target = CameraTarget;
					camera.Position += (target - camera.Position) * (1f - (float)Math.Pow(0.009999999776482582, Engine.DeltaTime));
					Distort.Anxiety = Calc.ClampedMap(dist, 0f, 200f, 0.25f, 0f) + Calc.Random.NextFloat(0.05f);
					Distort.AnxietyOrigin = (new Vector2(player.X, level.Camera.Top) - level.Camera.Position) / new Vector2(320f, 180f);
				}
				else
				{
					Engine.TimeRate = Calc.Approach(Engine.TimeRate, 1f, 1f * Engine.DeltaTime);
				}
			}
			foreach (Entity entity2 in base.Scene.Tracker.GetEntities<SeekerBarrier>())
			{
				entity2.Collidable = false;
			}
		}

		private void CreateTrail()
		{
			Vector2 wasScale = sprite.Scale;
			sprite.Scale.X *= (float)facing;
			TrailManager.Add(this, Seeker.TrailColor);
			sprite.Scale = wasScale;
		}

		private void OnCollide(CollisionData data)
		{
			if (dashTimer <= 0f)
			{
				if (data.Direction.X != 0f)
				{
					speed.X = 0f;
				}
				if (data.Direction.Y != 0f)
				{
					speed.Y = 0f;
				}
				return;
			}
			float dir;
			Vector2 at;
			Vector2 range;
			if (data.Direction.X > 0f)
			{
				dir = (float)Math.PI;
				at = new Vector2(base.Right, base.Y);
				range = Vector2.UnitY * 4f;
			}
			else if (data.Direction.X < 0f)
			{
				dir = 0f;
				at = new Vector2(base.Left, base.Y);
				range = Vector2.UnitY * 4f;
			}
			else if (data.Direction.Y > 0f)
			{
				dir = -(float)Math.PI / 2f;
				at = new Vector2(base.X, base.Bottom);
				range = Vector2.UnitX * 4f;
			}
			else
			{
				dir = (float)Math.PI / 2f;
				at = new Vector2(base.X, base.Top);
				range = Vector2.UnitX * 4f;
			}
			SceneAs<Level>().Particles.Emit(Seeker.P_HitWall, 12, at, range, dir);
			if (data.Hit is SeekerBarrier)
			{
				(data.Hit as SeekerBarrier).OnReflectSeeker();
				Audio.Play("event:/game/05_mirror_temple/seeker_hit_lightwall", Position);
			}
			else
			{
				Audio.Play("event:/game/05_mirror_temple/seeker_hit_normal", Position);
			}
			if (data.Direction.X != 0f)
			{
				speed.X *= -0.8f;
				sprite.Scale = new Vector2(0.6f, 1.4f);
			}
			else if (data.Direction.Y != 0f)
			{
				speed.Y *= -0.8f;
				sprite.Scale = new Vector2(1.4f, 0.6f);
			}
			if (data.Hit is TempleCrackedBlock)
			{
				Celeste.Freeze(0.15f);
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
				(data.Hit as TempleCrackedBlock).Break(Position);
			}
		}

		private void Dash(Vector2 dir)
		{
			if (dashTimer <= 0f)
			{
				CreateTrail();
				trailTimerA = 0.1f;
				trailTimerB = 0.25f;
			}
			dashTimer = 0.3f;
			dashDirection = dir;
			if (dashDirection == Vector2.Zero)
			{
				dashDirection.X = Math.Sign((int)facing);
			}
			if (dashDirection.X != 0f)
			{
				facing = (Facings)Math.Sign(dashDirection.X);
			}
			speed = dashDirection * 400f;
			sprite.Play("attacking");
			SceneAs<Level>().DirectionalShake(dashDirection);
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			Audio.Play("event:/game/05_mirror_temple/seeker_dash", Position);
			if (dashDirection.X == 0f)
			{
				sprite.Scale = new Vector2(0.6f, 1.4f);
			}
			else
			{
				sprite.Scale = new Vector2(1.4f, 0.6f);
			}
		}

		public override void Render()
		{
			if (!SaveData.Instance.Assists.InvisibleMotion || !enabled || !(speed.LengthSquared() > 100f))
			{
				Vector2 wasPos = Position;
				Position += shaker.Value;
				Vector2 wasScale = sprite.Scale;
				sprite.Scale.X *= (float)facing;
				base.Render();
				Position = wasPos;
				sprite.Scale = wasScale;
			}
		}
	}
}
