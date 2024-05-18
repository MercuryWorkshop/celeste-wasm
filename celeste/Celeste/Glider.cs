using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Glider : Actor
	{
		public static ParticleType P_Glide;

		public static ParticleType P_GlideUp;

		public static ParticleType P_Platform;

		public static ParticleType P_Glow;

		public static ParticleType P_Expand;

		private const float HighFrictionTime = 0.5f;

		public Vector2 Speed;

		public Holdable Hold;

		private Level level;

		private Collision onCollideH;

		private Collision onCollideV;

		private Vector2 prevLiftSpeed;

		private Vector2 startPos;

		private float noGravityTimer;

		private float highFrictionTimer;

		private bool bubble;

		private bool tutorial;

		private bool destroyed;

		private Sprite sprite;

		private Wiggler wiggler;

		private SineWave platformSine;

		private SoundSource fallingSfx;

		private BirdTutorialGui tutorialGui;

		public Glider(Vector2 position, bool bubble, bool tutorial)
			: base(position)
		{
			this.bubble = bubble;
			this.tutorial = tutorial;
			startPos = Position;
			base.Collider = new Hitbox(8f, 10f, -4f, -10f);
			onCollideH = OnCollideH;
			onCollideV = OnCollideV;
			Add(sprite = GFX.SpriteBank.Create("glider"));
			Add(wiggler = Wiggler.Create(0.25f, 4f));
			base.Depth = -5;
			Add(Hold = new Holdable(0.3f));
			Hold.PickupCollider = new Hitbox(20f, 22f, -10f, -16f);
			Hold.SlowFall = true;
			Hold.SlowRun = false;
			Hold.OnPickup = OnPickup;
			Hold.OnRelease = OnRelease;
			Hold.SpeedGetter = () => Speed;
			Hold.OnHitSpring = HitSpring;
			platformSine = new SineWave(0.3f);
			Add(platformSine);
			fallingSfx = new SoundSource();
			Add(fallingSfx);
			Add(new WindMover(WindMode));
		}

		public Glider(EntityData e, Vector2 offset)
			: this(e.Position + offset, e.Bool("bubble"), e.Bool("tutorial"))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			level = SceneAs<Level>();
			if (tutorial)
			{
				tutorialGui = new BirdTutorialGui(this, new Vector2(0f, -24f), Dialog.Clean("tutorial_carry"), Dialog.Clean("tutorial_hold"), BirdTutorialGui.ButtonPrompt.Grab);
				tutorialGui.Open = true;
				base.Scene.Add(tutorialGui);
			}
		}

		public override void Update()
		{
			if (base.Scene.OnInterval(0.05f))
			{
				level.Particles.Emit(P_Glow, 1, base.Center + Vector2.UnitY * -9f, new Vector2(10f, 4f));
			}
			float targetAngle = ((!Hold.IsHeld) ? 0f : ((!Hold.Holder.OnGround()) ? Calc.ClampedMap(Hold.Holder.Speed.X, -300f, 300f, (float)Math.PI / 3f, -(float)Math.PI / 3f) : Calc.ClampedMap(Hold.Holder.Speed.X, -300f, 300f, 0.6981317f, -0.6981317f)));
			sprite.Rotation = Calc.Approach(sprite.Rotation, targetAngle, (float)Math.PI * Engine.DeltaTime);
			if (Hold.IsHeld && !Hold.Holder.OnGround() && (sprite.CurrentAnimationID == "fall" || sprite.CurrentAnimationID == "fallLoop"))
			{
				if (!fallingSfx.Playing)
				{
					Audio.Play("event:/new_content/game/10_farewell/glider_engage", Position);
					fallingSfx.Play("event:/new_content/game/10_farewell/glider_movement");
				}
				Vector2 speed = Hold.Holder.Speed;
				Vector2 component = new Vector2(speed.X * 0.5f, (speed.Y < 0f) ? (speed.Y * 2f) : speed.Y);
				float value = Calc.Map(component.Length(), 0f, 120f, 0f, 0.7f);
				fallingSfx.Param("glider_speed", value);
			}
			else
			{
				fallingSfx.Stop();
			}
			base.Update();
			if (!destroyed)
			{
				foreach (SeekerBarrier sb in base.Scene.Tracker.GetEntities<SeekerBarrier>())
				{
					sb.Collidable = true;
					bool num = CollideCheck(sb);
					sb.Collidable = false;
					if (num)
					{
						destroyed = true;
						Collidable = false;
						if (Hold.IsHeld)
						{
							Vector2 spd = Hold.Holder.Speed;
							Hold.Holder.Drop();
							Speed = spd * 0.333f;
							Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
						}
						Add(new Coroutine(DestroyAnimationRoutine()));
						return;
					}
				}
				if (Hold.IsHeld)
				{
					prevLiftSpeed = Vector2.Zero;
				}
				else if (!bubble)
				{
					if (highFrictionTimer > 0f)
					{
						highFrictionTimer -= Engine.DeltaTime;
					}
					if (OnGround())
					{
						float targetX = ((!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f)));
						Speed.X = Calc.Approach(Speed.X, targetX, 800f * Engine.DeltaTime);
						Vector2 lift = base.LiftSpeed;
						if (lift == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
						{
							Speed = prevLiftSpeed;
							prevLiftSpeed = Vector2.Zero;
							Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
							if (Speed.X != 0f && Speed.Y == 0f)
							{
								Speed.Y = -60f;
							}
							if (Speed.Y < 0f)
							{
								noGravityTimer = 0.15f;
							}
						}
						else
						{
							prevLiftSpeed = lift;
							if (lift.Y < 0f && Speed.Y < 0f)
							{
								Speed.Y = 0f;
							}
						}
					}
					else if (Hold.ShouldHaveGravity)
					{
						float grav = 200f;
						if (Speed.Y >= -30f)
						{
							grav *= 0.5f;
						}
						float fric = ((Speed.Y < 0f) ? 40f : ((!(highFrictionTimer <= 0f)) ? 10f : 40f));
						Speed.X = Calc.Approach(Speed.X, 0f, fric * Engine.DeltaTime);
						if (noGravityTimer > 0f)
						{
							noGravityTimer -= Engine.DeltaTime;
						}
						else if (level.Wind.Y < 0f)
						{
							Speed.Y = Calc.Approach(Speed.Y, 0f, grav * Engine.DeltaTime);
						}
						else
						{
							Speed.Y = Calc.Approach(Speed.Y, 30f, grav * Engine.DeltaTime);
						}
					}
					MoveH(Speed.X * Engine.DeltaTime, onCollideH);
					MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
					if (base.Left < (float)level.Bounds.Left)
					{
						base.Left = level.Bounds.Left;
						OnCollideH(new CollisionData
						{
							Direction = -Vector2.UnitX
						});
					}
					else if (base.Right > (float)level.Bounds.Right)
					{
						base.Right = level.Bounds.Right;
						OnCollideH(new CollisionData
						{
							Direction = Vector2.UnitX
						});
					}
					if (base.Top < (float)level.Bounds.Top)
					{
						base.Top = level.Bounds.Top;
						OnCollideV(new CollisionData
						{
							Direction = -Vector2.UnitY
						});
					}
					else if (base.Top > (float)(level.Bounds.Bottom + 16))
					{
						RemoveSelf();
						return;
					}
					Hold.CheckAgainstColliders();
				}
				else
				{
					Position = startPos + Vector2.UnitY * platformSine.Value * 1f;
				}
				Vector2 targetScale = Vector2.One;
				if (!Hold.IsHeld)
				{
					if (level.Wind.Y < 0f)
					{
						PlayOpen();
					}
					else
					{
						sprite.Play("idle");
					}
				}
				else if (Hold.Holder.Speed.Y > 20f || level.Wind.Y < 0f)
				{
					if (level.OnInterval(0.04f))
					{
						if (level.Wind.Y < 0f)
						{
							level.ParticlesBG.Emit(P_GlideUp, 1, Position - Vector2.UnitY * 20f, new Vector2(6f, 4f));
						}
						else
						{
							level.ParticlesBG.Emit(P_Glide, 1, Position - Vector2.UnitY * 10f, new Vector2(6f, 4f));
						}
					}
					PlayOpen();
					if (Input.GliderMoveY.Value > 0)
					{
						targetScale.X = 0.7f;
						targetScale.Y = 1.4f;
					}
					else if (Input.GliderMoveY.Value < 0)
					{
						targetScale.X = 1.2f;
						targetScale.Y = 0.8f;
					}
					Input.Rumble(RumbleStrength.Climb, RumbleLength.Short);
				}
				else
				{
					sprite.Play("held");
				}
				sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, targetScale.Y, Engine.DeltaTime * 2f);
				sprite.Scale.X = Calc.Approach(sprite.Scale.X, (float)Math.Sign(sprite.Scale.X) * targetScale.X, Engine.DeltaTime * 2f);
				if (tutorialGui != null)
				{
					tutorialGui.Open = tutorial && !Hold.IsHeld && (OnGround(4) || bubble);
				}
			}
			else
			{
				Position += Speed * Engine.DeltaTime;
			}
		}

		private void PlayOpen()
		{
			if (sprite.CurrentAnimationID != "fall" && sprite.CurrentAnimationID != "fallLoop")
			{
				sprite.Play("fall");
				sprite.Scale = new Vector2(1.5f, 0.6f);
				level.Particles.Emit(P_Expand, 16, base.Center + (Vector2.UnitY * -12f).Rotate(sprite.Rotation), new Vector2(8f, 3f), -(float)Math.PI / 2f + sprite.Rotation);
				if (Hold.IsHeld)
				{
					Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
				}
			}
		}

		public override void Render()
		{
			if (!destroyed)
			{
				sprite.DrawSimpleOutline();
			}
			base.Render();
			if (bubble)
			{
				for (int i = 0; i < 24; i++)
				{
					Draw.Point(Position + PlatformAdd(i), PlatformColor(i));
				}
			}
		}

		private void WindMode(Vector2 wind)
		{
			if (!Hold.IsHeld)
			{
				if (wind.X != 0f)
				{
					MoveH(wind.X * 0.5f);
				}
				if (wind.Y != 0f)
				{
					MoveV(wind.Y);
				}
			}
		}

		private Vector2 PlatformAdd(int num)
		{
			return new Vector2(-12 + num, -5 + (int)Math.Round(Math.Sin(base.Scene.TimeActive + (float)num * 0.2f) * 1.7999999523162842));
		}

		private Color PlatformColor(int num)
		{
			if (num <= 1 || num >= 22)
			{
				return Color.White * 0.4f;
			}
			return Color.White * 0.8f;
		}

		private void OnCollideH(CollisionData data)
		{
			if (data.Hit is DashSwitch)
			{
				(data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
			}
			if (Speed.X < 0f)
			{
				Audio.Play("event:/new_content/game/10_farewell/glider_wallbounce_left", Position);
			}
			else
			{
				Audio.Play("event:/new_content/game/10_farewell/glider_wallbounce_right", Position);
			}
			Speed.X *= -1f;
			sprite.Scale = new Vector2(0.8f, 1.2f);
		}

		private void OnCollideV(CollisionData data)
		{
			if (Math.Abs(Speed.Y) > 8f)
			{
				sprite.Scale = new Vector2(1.2f, 0.8f);
				Audio.Play("event:/new_content/game/10_farewell/glider_land", Position);
			}
			if (Speed.Y < 0f)
			{
				Speed.Y *= -0.5f;
			}
			else
			{
				Speed.Y = 0f;
			}
		}

		private void OnPickup()
		{
			if (bubble)
			{
				for (int i = 0; i < 24; i++)
				{
					level.Particles.Emit(P_Platform, Position + PlatformAdd(i), PlatformColor(i));
				}
			}
			AllowPushing = false;
			Speed = Vector2.Zero;
			AddTag(Tags.Persistent);
			highFrictionTimer = 0.5f;
			bubble = false;
			wiggler.Start();
			tutorial = false;
		}

		private void OnRelease(Vector2 force)
		{
			if (force.X == 0f)
			{
				Audio.Play("event:/new_content/char/madeline/glider_drop", Position);
			}
			AllowPushing = true;
			RemoveTag(Tags.Persistent);
			force.Y *= 0.5f;
			if (force.X != 0f && force.Y == 0f)
			{
				force.Y = -0.4f;
			}
			Speed = force * 100f;
			wiggler.Start();
		}

		protected override void OnSquish(CollisionData data)
		{
			if (!TrySquishWiggle(data))
			{
				RemoveSelf();
			}
		}

		public bool HitSpring(Spring spring)
		{
			if (!Hold.IsHeld)
			{
				if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
				{
					Speed.X *= 0.5f;
					Speed.Y = -160f;
					noGravityTimer = 0.15f;
					wiggler.Start();
					return true;
				}
				if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
				{
					MoveTowardsY(spring.CenterY + 5f, 4f);
					Speed.X = 160f;
					Speed.Y = -80f;
					noGravityTimer = 0.1f;
					wiggler.Start();
					return true;
				}
				if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
				{
					MoveTowardsY(spring.CenterY + 5f, 4f);
					Speed.X = -160f;
					Speed.Y = -80f;
					noGravityTimer = 0.1f;
					wiggler.Start();
					return true;
				}
			}
			return false;
		}

		private IEnumerator DestroyAnimationRoutine()
		{
			Audio.Play("event:/new_content/game/10_farewell/glider_emancipate", Position);
			sprite.Play("death");
			yield return 1f;
			RemoveSelf();
		}
	}
}
