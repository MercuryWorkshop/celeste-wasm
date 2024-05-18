using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Puffer : Actor
	{
		private enum States
		{
			Idle,
			Hit,
			Gone
		}

		private const float RespawnTime = 2.5f;

		private const float RespawnMoveTime = 0.5f;

		private const float BounceSpeed = 200f;

		private const float ExplodeRadius = 40f;

		private const float DetectRadius = 32f;

		private const float StunnedAccel = 320f;

		private const float AlertedRadius = 60f;

		private const float CantExplodeTime = 0.5f;

		private Sprite sprite;

		private States state;

		private Vector2 startPosition;

		private Vector2 anchorPosition;

		private Vector2 lastSpeedPosition;

		private Vector2 lastSinePosition;

		private Circle pushRadius;

		private Circle breakWallsRadius;

		private Circle detectRadius;

		private SineWave idleSine;

		private Vector2 hitSpeed;

		private float goneTimer;

		private float cannotHitTimer;

		private Collision onCollideV;

		private Collision onCollideH;

		private float alertTimer;

		private Wiggler bounceWiggler;

		private Wiggler inflateWiggler;

		private Vector2 scale;

		private SimpleCurve returnCurve;

		private float cantExplodeTimer;

		private Vector2 lastPlayerPos;

		private float playerAliveFade;

		private Vector2 facing = Vector2.One;

		private float eyeSpin;

		public Puffer(Vector2 position, bool faceRight)
			: base(position)
		{
			base.Collider = new Hitbox(12f, 10f, -6f, -5f);
			Add(new PlayerCollider(OnPlayer, new Hitbox(14f, 12f, -7f, -7f)));
			Add(sprite = GFX.SpriteBank.Create("pufferFish"));
			sprite.Play("idle");
			if (!faceRight)
			{
				facing.X = -1f;
			}
			idleSine = new SineWave(0.5f);
			idleSine.Randomize();
			Add(idleSine);
			anchorPosition = Position;
			Position += new Vector2(idleSine.Value * 3f, idleSine.ValueOverTwo * 2f);
			state = States.Idle;
			startPosition = (lastSinePosition = (lastSpeedPosition = Position));
			pushRadius = new Circle(40f);
			detectRadius = new Circle(32f);
			breakWallsRadius = new Circle(16f);
			onCollideV = OnCollideV;
			onCollideH = OnCollideH;
			scale = Vector2.One;
			bounceWiggler = Wiggler.Create(0.6f, 2.5f, delegate(float v)
			{
				sprite.Rotation = v * 20f * ((float)Math.PI / 180f);
			});
			Add(bounceWiggler);
			inflateWiggler = Wiggler.Create(0.6f, 2f);
			Add(inflateWiggler);
		}

		public Puffer(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Bool("right"))
		{
		}

		public override bool IsRiding(JumpThru jumpThru)
		{
			return false;
		}

		public override bool IsRiding(Solid solid)
		{
			return false;
		}

		protected override void OnSquish(CollisionData data)
		{
			Explode();
			GotoGone();
		}

		private void OnCollideH(CollisionData data)
		{
			hitSpeed.X *= -0.8f;
		}

		private void OnCollideV(CollisionData data)
		{
			if (!(data.Direction.Y > 0f))
			{
				return;
			}
			for (int j = -1; j <= 1; j += 2)
			{
				for (int i = 1; i <= 2; i++)
				{
					Vector2 at = Position + Vector2.UnitX * i * j;
					if (!CollideCheck<Solid>(at) && !OnGround(at))
					{
						Position = at;
						return;
					}
				}
			}
			hitSpeed.Y *= -0.2f;
		}

		private void GotoIdle()
		{
			if (state == States.Gone)
			{
				Position = startPosition;
				cantExplodeTimer = 0.5f;
				sprite.Play("recover");
				Audio.Play("event:/new_content/game/10_farewell/puffer_reform", Position);
			}
			lastSinePosition = (lastSpeedPosition = (anchorPosition = Position));
			hitSpeed = Vector2.Zero;
			idleSine.Reset();
			state = States.Idle;
		}

		private void GotoHit(Vector2 from)
		{
			scale = new Vector2(1.2f, 0.8f);
			hitSpeed = Vector2.UnitY * 200f;
			state = States.Hit;
			bounceWiggler.Start();
			Alert(restart: true, playSfx: false);
			Audio.Play("event:/new_content/game/10_farewell/puffer_boop", Position);
		}

		private void GotoHitSpeed(Vector2 speed)
		{
			hitSpeed = speed;
			state = States.Hit;
		}

		private void GotoGone()
		{
			Vector2 control = Position + (startPosition - Position) * 0.5f;
			if ((startPosition - Position).LengthSquared() > 100f)
			{
				if (Math.Abs(Position.Y - startPosition.Y) > Math.Abs(Position.X - startPosition.X))
				{
					if (Position.X > startPosition.X)
					{
						control += Vector2.UnitX * -24f;
					}
					else
					{
						control += Vector2.UnitX * 24f;
					}
				}
				else if (Position.Y > startPosition.Y)
				{
					control += Vector2.UnitY * -24f;
				}
				else
				{
					control += Vector2.UnitY * 24f;
				}
			}
			returnCurve = new SimpleCurve(Position, startPosition, control);
			Collidable = false;
			goneTimer = 2.5f;
			state = States.Gone;
		}

		private void Explode()
		{
			Collider was = base.Collider;
			base.Collider = pushRadius;
			Audio.Play("event:/new_content/game/10_farewell/puffer_splode", Position);
			sprite.Play("explode");
			Player player = CollideFirst<Player>();
			if (player != null && !base.Scene.CollideCheck<Solid>(Position, player.Center))
			{
				player.ExplodeLaunch(Position, snapUp: false, sidesOnly: true);
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
			foreach (FloatingDebris fd in base.Scene.Tracker.GetEntities<FloatingDebris>())
			{
				if (CollideCheck(fd))
				{
					fd.OnExplode(Position);
				}
			}
			base.Collider = was;
			Level level = SceneAs<Level>();
			level.Shake();
			level.Displacement.AddBurst(Position, 0.4f, 12f, 36f, 0.5f);
			level.Displacement.AddBurst(Position, 0.4f, 24f, 48f, 0.5f);
			level.Displacement.AddBurst(Position, 0.4f, 36f, 60f, 0.5f);
			for (float i = 0f; i < (float)Math.PI * 2f; i += 0.17453292f)
			{
				Vector2 at = base.Center + Calc.AngleToVector(i + Calc.Random.Range(-(float)Math.PI / 90f, (float)Math.PI / 90f), Calc.Random.Range(12, 18));
				level.Particles.Emit(Seeker.P_Regen, at, i);
			}
		}

		public override void Render()
		{
			sprite.Scale = scale * (1f + inflateWiggler.Value * 0.4f);
			sprite.Scale *= facing;
			bool drawOutline = false;
			if (sprite.CurrentAnimationID != "hidden" && sprite.CurrentAnimationID != "explode" && sprite.CurrentAnimationID != "recover")
			{
				drawOutline = true;
			}
			else if (sprite.CurrentAnimationID == "explode" && sprite.CurrentAnimationFrame <= 1)
			{
				drawOutline = true;
			}
			else if (sprite.CurrentAnimationID == "recover" && sprite.CurrentAnimationFrame >= 4)
			{
				drawOutline = true;
			}
			if (drawOutline)
			{
				sprite.DrawSimpleOutline();
			}
			float masterFade = playerAliveFade * Calc.ClampedMap((Position - lastPlayerPos).Length(), 128f, 96f);
			if (masterFade > 0f && state != States.Gone)
			{
				bool playerAbove = false;
				Vector2 p = lastPlayerPos;
				if (p.Y < base.Y)
				{
					p.Y = base.Y - (p.Y - base.Y) * 0.5f;
					p.X += p.X - base.X;
					playerAbove = true;
				}
				float pAngle = (p - Position).Angle();
				for (int i = 0; i < 28; i++)
				{
					float offset2 = (float)Math.Sin(base.Scene.TimeActive * 0.5f) * 0.02f;
					float rad = Calc.Map((float)i / 28f + offset2, 0f, 1f, -(float)Math.PI / 30f, 3.2463126f);
					rad += bounceWiggler.Value * 20f * ((float)Math.PI / 180f);
					Vector2 angle = Calc.AngleToVector(rad, 1f);
					Vector2 pos = Position + angle * 32f;
					float alpha = Calc.ClampedMap(Calc.AbsAngleDiff(rad, pAngle), (float)Math.PI / 2f, 0.17453292f);
					alpha = Ease.CubeOut(alpha) * 0.8f * masterFade;
					if (!(alpha > 0f))
					{
						continue;
					}
					if (i == 0 || i == 27)
					{
						Draw.Line(pos, pos - angle * 10f, Color.White * alpha);
						continue;
					}
					Vector2 add = angle * (float)Math.Sin(base.Scene.TimeActive * 2f + (float)i * 0.6f);
					if (i % 2 == 0)
					{
						add *= -1f;
					}
					pos += add;
					if (!playerAbove && Calc.AbsAngleDiff(rad, pAngle) <= 0.17453292f)
					{
						Draw.Line(pos, pos - angle * 3f, Color.White * alpha);
					}
					else
					{
						Draw.Point(pos, Color.White * alpha);
					}
				}
			}
			base.Render();
			if (sprite.CurrentAnimationID == "alerted")
			{
				Vector2 vector = Position + new Vector2(3f, (facing.X < 0f) ? (-5) : (-4)) * sprite.Scale;
				Vector2 lookAt = lastPlayerPos + new Vector2(0f, -4f);
				Vector2 offset = Calc.AngleToVector(Calc.Angle(vector, lookAt) + eyeSpin * ((float)Math.PI * 2f) * 2f, 1f);
				Vector2 pupil = vector + new Vector2((float)Math.Round(offset.X), (float)Math.Round(Calc.ClampedMap(offset.Y, -1f, 1f, -1f, 2f)));
				Draw.Rect(pupil.X, pupil.Y, 1f, 1f, Color.Black);
			}
			sprite.Scale /= facing;
		}

		public override void Update()
		{
			base.Update();
			eyeSpin = Calc.Approach(eyeSpin, 0f, Engine.DeltaTime * 1.5f);
			scale = Calc.Approach(scale, Vector2.One, 1f * Engine.DeltaTime);
			if (cannotHitTimer > 0f)
			{
				cannotHitTimer -= Engine.DeltaTime;
			}
			if (state != States.Gone && cantExplodeTimer > 0f)
			{
				cantExplodeTimer -= Engine.DeltaTime;
			}
			if (alertTimer > 0f)
			{
				alertTimer -= Engine.DeltaTime;
			}
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player == null)
			{
				playerAliveFade = Calc.Approach(playerAliveFade, 0f, 1f * Engine.DeltaTime);
			}
			else
			{
				playerAliveFade = Calc.Approach(playerAliveFade, 1f, 1f * Engine.DeltaTime);
				lastPlayerPos = player.Center;
			}
			switch (state)
			{
			case States.Idle:
			{
				if (Position != lastSinePosition)
				{
					anchorPosition += Position - lastSinePosition;
				}
				Vector2 at = anchorPosition + new Vector2(idleSine.Value * 3f, idleSine.ValueOverTwo * 2f);
				MoveToX(at.X);
				MoveToY(at.Y);
				lastSinePosition = Position;
				if (ProximityExplodeCheck())
				{
					Explode();
					GotoGone();
					break;
				}
				if (AlertedCheck())
				{
					Alert(restart: false, playSfx: true);
				}
				else if (sprite.CurrentAnimationID == "alerted" && alertTimer <= 0f)
				{
					Audio.Play("event:/new_content/game/10_farewell/puffer_shrink", Position);
					sprite.Play("unalert");
				}
				{
					foreach (PufferCollider component in base.Scene.Tracker.GetComponents<PufferCollider>())
					{
						component.Check(this);
					}
					break;
				}
			}
			case States.Hit:
				lastSpeedPosition = Position;
				MoveH(hitSpeed.X * Engine.DeltaTime, onCollideH);
				MoveV(hitSpeed.Y * Engine.DeltaTime, OnCollideV);
				anchorPosition = Position;
				hitSpeed.X = Calc.Approach(hitSpeed.X, 0f, 150f * Engine.DeltaTime);
				hitSpeed = Calc.Approach(hitSpeed, Vector2.Zero, 320f * Engine.DeltaTime);
				if (ProximityExplodeCheck())
				{
					Explode();
					GotoGone();
					break;
				}
				if (base.Top >= (float)(SceneAs<Level>().Bounds.Bottom + 5))
				{
					sprite.Play("hidden");
					GotoGone();
					break;
				}
				foreach (PufferCollider component2 in base.Scene.Tracker.GetComponents<PufferCollider>())
				{
					component2.Check(this);
				}
				if (hitSpeed == Vector2.Zero)
				{
					ZeroRemainderX();
					ZeroRemainderY();
					GotoIdle();
				}
				break;
			case States.Gone:
			{
				float was = goneTimer;
				goneTimer -= Engine.DeltaTime;
				if (goneTimer <= 0.5f)
				{
					if (was > 0.5f && returnCurve.GetLengthParametric(8) > 8f)
					{
						Audio.Play("event:/new_content/game/10_farewell/puffer_return", Position);
					}
					Position = returnCurve.GetPoint(Ease.CubeInOut(Calc.ClampedMap(goneTimer, 0.5f, 0f)));
				}
				if (goneTimer <= 0f)
				{
					Visible = (Collidable = true);
					GotoIdle();
				}
				break;
			}
			}
		}

		public bool HitSpring(Spring spring)
		{
			switch (spring.Orientation)
			{
			default:
				if (hitSpeed.Y >= 0f)
				{
					GotoHitSpeed(224f * -Vector2.UnitY);
					MoveTowardsX(spring.CenterX, 4f);
					bounceWiggler.Start();
					Alert(restart: true, playSfx: false);
					return true;
				}
				return false;
			case Spring.Orientations.WallLeft:
				if (hitSpeed.X <= 60f)
				{
					facing.X = 1f;
					GotoHitSpeed(280f * Vector2.UnitX);
					MoveTowardsY(spring.CenterY, 4f);
					bounceWiggler.Start();
					Alert(restart: true, playSfx: false);
					return true;
				}
				return false;
			case Spring.Orientations.WallRight:
				if (hitSpeed.X >= -60f)
				{
					facing.X = -1f;
					GotoHitSpeed(280f * -Vector2.UnitX);
					MoveTowardsY(spring.CenterY, 4f);
					bounceWiggler.Start();
					Alert(restart: true, playSfx: false);
					return true;
				}
				return false;
			}
		}

		private bool ProximityExplodeCheck()
		{
			if (cantExplodeTimer > 0f)
			{
				return false;
			}
			bool ret = false;
			Collider was = base.Collider;
			base.Collider = detectRadius;
			Player hit;
			if ((hit = CollideFirst<Player>()) != null && hit.CenterY >= base.Y + was.Bottom - 4f && !base.Scene.CollideCheck<Solid>(Position, hit.Center))
			{
				ret = true;
			}
			base.Collider = was;
			return ret;
		}

		private bool AlertedCheck()
		{
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				return (player.Center - base.Center).Length() < 60f;
			}
			return false;
		}

		private void Alert(bool restart, bool playSfx)
		{
			if (sprite.CurrentAnimationID == "idle")
			{
				if (playSfx)
				{
					Audio.Play("event:/new_content/game/10_farewell/puffer_expand", Position);
				}
				sprite.Play("alert");
				inflateWiggler.Start();
			}
			else if (restart && playSfx)
			{
				Audio.Play("event:/new_content/game/10_farewell/puffer_expand", Position);
			}
			alertTimer = 2f;
		}

		private void OnPlayer(Player player)
		{
			if (state == States.Gone || !(cantExplodeTimer <= 0f))
			{
				return;
			}
			if (cannotHitTimer <= 0f)
			{
				if (player.Bottom > lastSpeedPosition.Y + 3f)
				{
					Explode();
					GotoGone();
				}
				else
				{
					player.Bounce(base.Top);
					GotoHit(player.Center);
					MoveToX(anchorPosition.X);
					idleSine.Reset();
					anchorPosition = (lastSinePosition = Position);
					eyeSpin = 1f;
				}
			}
			cannotHitTimer = 0.1f;
		}
	}
}
