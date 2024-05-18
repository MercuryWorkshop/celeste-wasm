using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class MoveBlock : Solid
	{
		public enum Directions
		{
			Left,
			Right,
			Up,
			Down
		}

		private enum MovementState
		{
			Idling,
			Moving,
			Breaking
		}

		private class Border : Entity
		{
			public MoveBlock Parent;

			public Border(MoveBlock parent)
			{
				Parent = parent;
				base.Depth = 1;
			}

			public override void Update()
			{
				if (Parent.Scene != base.Scene)
				{
					RemoveSelf();
				}
				base.Update();
			}

			public override void Render()
			{
				Draw.Rect(Parent.X + Parent.Shake.X - 1f, Parent.Y + Parent.Shake.Y - 1f, Parent.Width + 2f, Parent.Height + 2f, Color.Black);
			}
		}

		[Pooled]
		private class Debris : Actor
		{
			private Image sprite;

			private Vector2 home;

			private Vector2 speed;

			private bool shaking;

			private bool returning;

			private float returnEase;

			private float returnDuration;

			private SimpleCurve returnCurve;

			private bool firstHit;

			private float alpha;

			private Collision onCollideH;

			private Collision onCollideV;

			private float spin;

			public Debris()
				: base(Vector2.Zero)
			{
				base.Tag = Tags.TransitionUpdate;
				base.Collider = new Hitbox(4f, 4f, -2f, -2f);
				Add(sprite = new Image(Calc.Random.Choose(GFX.Game.GetAtlasSubtextures("objects/moveblock/debris"))));
				sprite.CenterOrigin();
				sprite.FlipX = Calc.Random.Chance(0.5f);
				onCollideH = delegate
				{
					speed.X = (0f - speed.X) * 0.5f;
				};
				onCollideV = delegate
				{
					if (firstHit || speed.Y > 50f)
					{
						Audio.Play("event:/game/general/debris_stone", Position, "debris_velocity", Calc.ClampedMap(speed.Y, 0f, 600f));
					}
					if (speed.Y > 0f && speed.Y < 40f)
					{
						speed.Y = 0f;
					}
					else
					{
						speed.Y = (0f - speed.Y) * 0.25f;
					}
					firstHit = false;
				};
			}

			protected override void OnSquish(CollisionData data)
			{
			}

			public Debris Init(Vector2 position, Vector2 center, Vector2 returnTo)
			{
				Collidable = true;
				Position = position;
				speed = (position - center).SafeNormalize(60f + Calc.Random.NextFloat(60f));
				home = returnTo;
				sprite.Position = Vector2.Zero;
				sprite.Rotation = Calc.Random.NextAngle();
				returning = false;
				shaking = false;
				sprite.Scale.X = 1f;
				sprite.Scale.Y = 1f;
				sprite.Color = Color.White;
				alpha = 1f;
				firstHit = false;
				spin = Calc.Random.Range(3.4906585f, 10.471975f) * (float)Calc.Random.Choose(1, -1);
				return this;
			}

			public override void Update()
			{
				base.Update();
				if (!returning)
				{
					if (Collidable)
					{
						speed.X = Calc.Approach(speed.X, 0f, Engine.DeltaTime * 100f);
						if (!OnGround())
						{
							speed.Y += 400f * Engine.DeltaTime;
						}
						MoveH(speed.X * Engine.DeltaTime, onCollideH);
						MoveV(speed.Y * Engine.DeltaTime, onCollideV);
					}
					if (shaking && base.Scene.OnInterval(0.05f))
					{
						sprite.X = -1 + Calc.Random.Next(3);
						sprite.Y = -1 + Calc.Random.Next(3);
					}
				}
				else
				{
					Position = returnCurve.GetPoint(Ease.CubeOut(returnEase));
					returnEase = Calc.Approach(returnEase, 1f, Engine.DeltaTime / returnDuration);
					sprite.Scale = Vector2.One * (1f + returnEase * 0.5f);
				}
				if ((base.Scene as Level).Transitioning)
				{
					alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime * 4f);
					sprite.Color = Color.White * alpha;
				}
				sprite.Rotation += spin * Calc.ClampedMap(Math.Abs(speed.Y), 50f, 150f) * Engine.DeltaTime;
			}

			public void StopMoving()
			{
				Collidable = false;
			}

			public void StartShaking()
			{
				shaking = true;
			}

			public void ReturnHome(float duration)
			{
				if (base.Scene != null)
				{
					Camera camera = (base.Scene as Level).Camera;
					if (base.X < camera.X)
					{
						base.X = camera.X - 8f;
					}
					if (base.Y < camera.Y)
					{
						base.Y = camera.Y - 8f;
					}
					if (base.X > camera.X + 320f)
					{
						base.X = camera.X + 320f + 8f;
					}
					if (base.Y > camera.Y + 180f)
					{
						base.Y = camera.Y + 180f + 8f;
					}
				}
				returning = true;
				returnEase = 0f;
				returnDuration = duration;
				Vector2 dir = (home - Position).SafeNormalize();
				Vector2 control = (Position + home) / 2f + new Vector2(dir.Y, 0f - dir.X) * (Calc.Random.NextFloat(16f) + 16f) * Calc.Random.Facing();
				returnCurve = new SimpleCurve(Position, home, control);
			}
		}

		public static ParticleType P_Activate;

		public static ParticleType P_Break;

		public static ParticleType P_Move;

		private const float Accel = 300f;

		private const float MoveSpeed = 60f;

		private const float FastMoveSpeed = 75f;

		private const float SteerSpeed = (float)Math.PI * 16f;

		private const float MaxAngle = (float)Math.PI / 4f;

		private const float NoSteerTime = 0.2f;

		private const float CrashTime = 0.15f;

		private const float CrashResetTime = 0.1f;

		private const float RegenTime = 3f;

		private bool canSteer;

		private bool fast;

		private Directions direction;

		private float homeAngle;

		private int angleSteerSign;

		private Vector2 startPosition;

		private MovementState state;

		private bool leftPressed;

		private bool rightPressed;

		private bool topPressed;

		private float speed;

		private float targetSpeed;

		private float angle;

		private float targetAngle;

		private Player noSquish;

		private List<Image> body = new List<Image>();

		private List<Image> topButton = new List<Image>();

		private List<Image> leftButton = new List<Image>();

		private List<Image> rightButton = new List<Image>();

		private List<MTexture> arrows = new List<MTexture>();

		private Border border;

		private Color fillColor = idleBgFill;

		private float flash;

		private SoundSource moveSfx;

		private bool triggered;

		private static readonly Color idleBgFill = Calc.HexToColor("474070");

		private static readonly Color pressedBgFill = Calc.HexToColor("30b335");

		private static readonly Color breakingBgFill = Calc.HexToColor("cc2541");

		private float particleRemainder;

		public MoveBlock(Vector2 position, int width, int height, Directions direction, bool canSteer, bool fast)
			: base(position, width, height, safe: false)
		{
			base.Depth = -1;
			startPosition = position;
			this.canSteer = canSteer;
			this.direction = direction;
			this.fast = fast;
			switch (direction)
			{
			default:
				homeAngle = (targetAngle = (angle = 0f));
				angleSteerSign = 1;
				break;
			case Directions.Left:
				homeAngle = (targetAngle = (angle = (float)Math.PI));
				angleSteerSign = -1;
				break;
			case Directions.Up:
				homeAngle = (targetAngle = (angle = -(float)Math.PI / 2f));
				angleSteerSign = 1;
				break;
			case Directions.Down:
				homeAngle = (targetAngle = (angle = (float)Math.PI / 2f));
				angleSteerSign = -1;
				break;
			}
			int columns = width / 8;
			int rows = height / 8;
			MTexture tex = GFX.Game["objects/moveBlock/base"];
			MTexture btn = GFX.Game["objects/moveBlock/button"];
			if (canSteer && (direction == Directions.Left || direction == Directions.Right))
			{
				for (int x2 = 0; x2 < columns; x2++)
				{
					int tx2 = ((x2 != 0) ? ((x2 < columns - 1) ? 1 : 2) : 0);
					AddImage(btn.GetSubtexture(tx2 * 8, 0, 8, 8), new Vector2(x2 * 8, -4f), 0f, new Vector2(1f, 1f), topButton);
				}
				tex = GFX.Game["objects/moveBlock/base_h"];
			}
			else if (canSteer && (direction == Directions.Up || direction == Directions.Down))
			{
				for (int y2 = 0; y2 < rows; y2++)
				{
					int ty2 = ((y2 != 0) ? ((y2 < rows - 1) ? 1 : 2) : 0);
					AddImage(btn.GetSubtexture(ty2 * 8, 0, 8, 8), new Vector2(-4f, y2 * 8), (float)Math.PI / 2f, new Vector2(1f, -1f), leftButton);
					AddImage(btn.GetSubtexture(ty2 * 8, 0, 8, 8), new Vector2((columns - 1) * 8 + 4, y2 * 8), (float)Math.PI / 2f, new Vector2(1f, 1f), rightButton);
				}
				tex = GFX.Game["objects/moveBlock/base_v"];
			}
			for (int x = 0; x < columns; x++)
			{
				for (int y = 0; y < rows; y++)
				{
					int tx = ((x != 0) ? ((x < columns - 1) ? 1 : 2) : 0);
					int ty = ((y != 0) ? ((y < rows - 1) ? 1 : 2) : 0);
					AddImage(tex.GetSubtexture(tx * 8, ty * 8, 8, 8), new Vector2(x, y) * 8f, 0f, new Vector2(1f, 1f), body);
				}
			}
			arrows = GFX.Game.GetAtlasSubtextures("objects/moveBlock/arrow");
			Add(moveSfx = new SoundSource());
			Add(new Coroutine(Controller()));
			UpdateColors();
			Add(new LightOcclude(0.5f));
		}

		public MoveBlock(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.Enum("direction", Directions.Left), data.Bool("canSteer", defaultValue: true), data.Bool("fast"))
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			scene.Add(border = new Border(this));
		}

		private IEnumerator Controller()
		{
			while (true)
			{
				triggered = false;
				state = MovementState.Idling;
				while (!triggered && !HasPlayerRider())
				{
					yield return null;
				}
				Audio.Play("event:/game/04_cliffside/arrowblock_activate", Position);
				state = MovementState.Moving;
				StartShaking(0.2f);
				ActivateParticles();
				yield return 0.2f;
				targetSpeed = (fast ? 75f : 60f);
				moveSfx.Play("event:/game/04_cliffside/arrowblock_move");
				moveSfx.Param("arrow_stop", 0f);
				StopPlayerRunIntoAnimation = false;
				float crashTimer = 0.15f;
				float crashResetTimer = 0.1f;
				float noSteerTimer = 0.2f;
				while (true)
				{
					if (canSteer)
					{
						targetAngle = homeAngle;
						bool hasPlayer = ((direction != Directions.Right && direction != 0) ? HasPlayerClimbing() : HasPlayerOnTop());
						if (hasPlayer && noSteerTimer > 0f)
						{
							noSteerTimer -= Engine.DeltaTime;
						}
						if (hasPlayer)
						{
							if (noSteerTimer <= 0f)
							{
								if (direction == Directions.Right || direction == Directions.Left)
								{
									targetAngle = homeAngle + (float)Math.PI / 4f * (float)angleSteerSign * (float)Input.MoveY.Value;
								}
								else
								{
									targetAngle = homeAngle + (float)Math.PI / 4f * (float)angleSteerSign * (float)Input.MoveX.Value;
								}
							}
						}
						else
						{
							noSteerTimer = 0.2f;
						}
					}
					if (base.Scene.OnInterval(0.02f))
					{
						MoveParticles();
					}
					speed = Calc.Approach(speed, targetSpeed, 300f * Engine.DeltaTime);
					angle = Calc.Approach(angle, targetAngle, (float)Math.PI * 16f * Engine.DeltaTime);
					Vector2 moveSpeed = Calc.AngleToVector(angle, speed);
					Vector2 move = moveSpeed * Engine.DeltaTime;
					bool hit;
					if (direction == Directions.Right || direction == Directions.Left)
					{
						hit = MoveCheck(move.XComp());
						noSquish = base.Scene.Tracker.GetEntity<Player>();
						MoveVCollideSolids(move.Y, thruDashBlocks: false);
						noSquish = null;
						LiftSpeed = moveSpeed;
						if (base.Scene.OnInterval(0.03f))
						{
							if (move.Y > 0f)
							{
								ScrapeParticles(Vector2.UnitY);
							}
							else if (move.Y < 0f)
							{
								ScrapeParticles(-Vector2.UnitY);
							}
						}
					}
					else
					{
						hit = MoveCheck(move.YComp());
						noSquish = base.Scene.Tracker.GetEntity<Player>();
						MoveHCollideSolids(move.X, thruDashBlocks: false);
						noSquish = null;
						LiftSpeed = moveSpeed;
						if (base.Scene.OnInterval(0.03f))
						{
							if (move.X > 0f)
							{
								ScrapeParticles(Vector2.UnitX);
							}
							else if (move.X < 0f)
							{
								ScrapeParticles(-Vector2.UnitX);
							}
						}
						if (direction == Directions.Down && base.Top > (float)(SceneAs<Level>().Bounds.Bottom + 32))
						{
							hit = true;
						}
					}
					if (hit)
					{
						moveSfx.Param("arrow_stop", 1f);
						crashResetTimer = 0.1f;
						if (!(crashTimer > 0f))
						{
							break;
						}
						crashTimer -= Engine.DeltaTime;
					}
					else
					{
						moveSfx.Param("arrow_stop", 0f);
						if (crashResetTimer > 0f)
						{
							crashResetTimer -= Engine.DeltaTime;
						}
						else
						{
							crashTimer = 0.15f;
						}
					}
					Level level = base.Scene as Level;
					if (base.Left < (float)level.Bounds.Left || base.Top < (float)level.Bounds.Top || base.Right > (float)level.Bounds.Right)
					{
						break;
					}
					yield return null;
				}
				Audio.Play("event:/game/04_cliffside/arrowblock_break", Position);
				moveSfx.Stop();
				state = MovementState.Breaking;
				speed = (targetSpeed = 0f);
				angle = (targetAngle = homeAngle);
				StartShaking(0.2f);
				StopPlayerRunIntoAnimation = true;
				yield return 0.2f;
				BreakParticles();
				List<Debris> debris = new List<Debris>();
				for (int x = 0; (float)x < base.Width; x += 8)
				{
					for (int y = 0; (float)y < base.Height; y += 8)
					{
						Vector2 offset = new Vector2((float)x + 4f, (float)y + 4f);
						Debris d = Engine.Pooler.Create<Debris>().Init(Position + offset, base.Center, startPosition + offset);
						debris.Add(d);
						base.Scene.Add(d);
					}
				}
				MoveStaticMovers(startPosition - Position);
				DisableStaticMovers();
				Position = startPosition;
				Visible = (Collidable = false);
				yield return 2.2f;
				foreach (Debris item in debris)
				{
					item.StopMoving();
				}
				while (CollideCheck<Actor>() || CollideCheck<Solid>())
				{
					yield return null;
				}
				Collidable = true;
				EventInstance sound = Audio.Play("event:/game/04_cliffside/arrowblock_reform_begin", debris[0].Position);
				MoveBlock moveBlock = this;
				Coroutine component;
				Coroutine routine = (component = new Coroutine(SoundFollowsDebrisCenter(sound, debris)));
				moveBlock.Add(component);
				foreach (Debris item2 in debris)
				{
					item2.StartShaking();
				}
				yield return 0.2f;
				foreach (Debris item3 in debris)
				{
					item3.ReturnHome(0.65f);
				}
				yield return 0.6f;
				routine.RemoveSelf();
				foreach (Debris item4 in debris)
				{
					item4.RemoveSelf();
				}
				Audio.Play("event:/game/04_cliffside/arrowblock_reappear", Position);
				Visible = true;
				EnableStaticMovers();
				speed = (targetSpeed = 0f);
				angle = (targetAngle = homeAngle);
				noSquish = null;
				fillColor = idleBgFill;
				UpdateColors();
				flash = 1f;
			}
		}

		private IEnumerator SoundFollowsDebrisCenter(EventInstance instance, List<Debris> debris)
		{
			while (true)
			{
				instance.getPlaybackState(out var state);
				if (state == PLAYBACK_STATE.STOPPED)
				{
					break;
				}
				Vector2 center = Vector2.Zero;
				foreach (Debris d in debris)
				{
					center += d.Position;
				}
				center /= (float)debris.Count;
				Audio.Position(instance, center);
				yield return null;
			}
		}

		public override void Update()
		{
			base.Update();
			if (canSteer)
			{
				bool leftIsPressed = (direction == Directions.Up || direction == Directions.Down) && CollideCheck<Player>(Position + new Vector2(-1f, 0f));
				bool rightIsPressed = (direction == Directions.Up || direction == Directions.Down) && CollideCheck<Player>(Position + new Vector2(1f, 0f));
				bool topIsPressed = (direction == Directions.Left || direction == Directions.Right) && CollideCheck<Player>(Position + new Vector2(0f, -1f));
				foreach (Image item in topButton)
				{
					item.Y = (topIsPressed ? 2 : 0);
				}
				foreach (Image item2 in leftButton)
				{
					item2.X = (leftIsPressed ? 2 : 0);
				}
				foreach (Image item3 in rightButton)
				{
					item3.X = base.Width + (float)(rightIsPressed ? (-2) : 0);
				}
				if ((leftIsPressed && !leftPressed) || (topIsPressed && !topPressed) || (rightIsPressed && !rightPressed))
				{
					Audio.Play("event:/game/04_cliffside/arrowblock_side_depress", Position);
				}
				if ((!leftIsPressed && leftPressed) || (!topIsPressed && topPressed) || (!rightIsPressed && rightPressed))
				{
					Audio.Play("event:/game/04_cliffside/arrowblock_side_release", Position);
				}
				leftPressed = leftIsPressed;
				rightPressed = rightIsPressed;
				topPressed = topIsPressed;
			}
			if (moveSfx != null && moveSfx.Playing)
			{
				int cardinalAngle = (int)Math.Floor((0f - (Calc.AngleToVector(angle, 1f) * new Vector2(-1f, 1f)).Angle() + (float)Math.PI * 2f) % ((float)Math.PI * 2f) / ((float)Math.PI * 2f) * 8f + 0.5f);
				moveSfx.Param("arrow_influence", cardinalAngle + 1);
			}
			border.Visible = Visible;
			flash = Calc.Approach(flash, 0f, Engine.DeltaTime * 5f);
			UpdateColors();
		}

		public override void OnStaticMoverTrigger(StaticMover sm)
		{
			triggered = true;
		}

		public override void MoveHExact(int move)
		{
			if (noSquish != null && ((move < 0 && noSquish.X < base.X) || (move > 0 && noSquish.X > base.X)))
			{
				while (move != 0 && noSquish.CollideCheck<Solid>(noSquish.Position + Vector2.UnitX * move))
				{
					move -= Math.Sign(move);
				}
			}
			base.MoveHExact(move);
		}

		public override void MoveVExact(int move)
		{
			if (noSquish != null && move < 0 && noSquish.Y <= base.Y)
			{
				while (move != 0 && noSquish.CollideCheck<Solid>(noSquish.Position + Vector2.UnitY * move))
				{
					move -= Math.Sign(move);
				}
			}
			base.MoveVExact(move);
		}

		private bool MoveCheck(Vector2 speed)
		{
			if (speed.X != 0f)
			{
				if (MoveHCollideSolids(speed.X, thruDashBlocks: false))
				{
					for (int j = 1; j <= 3; j++)
					{
						for (int s2 = 1; s2 >= -1; s2 -= 2)
						{
							Vector2 add2 = new Vector2(Math.Sign(speed.X), j * s2);
							if (!CollideCheck<Solid>(Position + add2))
							{
								MoveVExact(j * s2);
								MoveHExact(Math.Sign(speed.X));
								return false;
							}
						}
					}
					return true;
				}
				return false;
			}
			if (speed.Y != 0f)
			{
				if (MoveVCollideSolids(speed.Y, thruDashBlocks: false))
				{
					for (int i = 1; i <= 3; i++)
					{
						for (int s = 1; s >= -1; s -= 2)
						{
							Vector2 add = new Vector2(i * s, Math.Sign(speed.Y));
							if (!CollideCheck<Solid>(Position + add))
							{
								MoveHExact(i * s);
								MoveVExact(Math.Sign(speed.Y));
								return false;
							}
						}
					}
					return true;
				}
				return false;
			}
			return false;
		}

		private void UpdateColors()
		{
			Color targetFill = idleBgFill;
			if (state == MovementState.Moving)
			{
				targetFill = pressedBgFill;
			}
			else if (state == MovementState.Breaking)
			{
				targetFill = breakingBgFill;
			}
			fillColor = Color.Lerp(fillColor, targetFill, 10f * Engine.DeltaTime);
			foreach (Image item in topButton)
			{
				item.Color = fillColor;
			}
			foreach (Image item2 in leftButton)
			{
				item2.Color = fillColor;
			}
			foreach (Image item3 in rightButton)
			{
				item3.Color = fillColor;
			}
		}

		private void AddImage(MTexture tex, Vector2 position, float rotation, Vector2 scale, List<Image> addTo)
		{
			Image img = new Image(tex);
			img.Position = position + new Vector2(4f, 4f);
			img.CenterOrigin();
			img.Rotation = rotation;
			img.Scale = scale;
			Add(img);
			addTo?.Add(img);
		}

		private void SetVisible(List<Image> images, bool visible)
		{
			foreach (Image image in images)
			{
				image.Visible = visible;
			}
		}

		public override void Render()
		{
			Vector2 was = Position;
			Position += base.Shake;
			foreach (Image item in leftButton)
			{
				item.Render();
			}
			foreach (Image item2 in rightButton)
			{
				item2.Render();
			}
			foreach (Image item3 in topButton)
			{
				item3.Render();
			}
			Draw.Rect(base.X + 3f, base.Y + 3f, base.Width - 6f, base.Height - 6f, fillColor);
			foreach (Image item4 in body)
			{
				item4.Render();
			}
			Draw.Rect(base.Center.X - 4f, base.Center.Y - 4f, 8f, 8f, fillColor);
			if (state != MovementState.Breaking)
			{
				int cardinalAngle = (int)Math.Floor((0f - angle + (float)Math.PI * 2f) % ((float)Math.PI * 2f) / ((float)Math.PI * 2f) * 8f + 0.5f);
				arrows[Calc.Clamp(cardinalAngle, 0, 7)].DrawCentered(base.Center);
			}
			else
			{
				GFX.Game["objects/moveBlock/x"].DrawCentered(base.Center);
			}
			float expand = flash * 4f;
			Draw.Rect(base.X - expand, base.Y - expand, base.Width + expand * 2f, base.Height + expand * 2f, Color.White * flash);
			Position = was;
		}

		private void ActivateParticles()
		{
			bool vertical = direction == Directions.Down || direction == Directions.Up;
			bool num = (!canSteer || !vertical) && !CollideCheck<Player>(Position - Vector2.UnitX);
			bool right = (!canSteer || !vertical) && !CollideCheck<Player>(Position + Vector2.UnitX);
			bool up = (!canSteer || vertical) && !CollideCheck<Player>(Position - Vector2.UnitY);
			if (num)
			{
				SceneAs<Level>().ParticlesBG.Emit(P_Activate, (int)(base.Height / 2f), base.CenterLeft, Vector2.UnitY * (base.Height - 4f) * 0.5f, (float)Math.PI);
			}
			if (right)
			{
				SceneAs<Level>().ParticlesBG.Emit(P_Activate, (int)(base.Height / 2f), base.CenterRight, Vector2.UnitY * (base.Height - 4f) * 0.5f, 0f);
			}
			if (up)
			{
				SceneAs<Level>().ParticlesBG.Emit(P_Activate, (int)(base.Width / 2f), base.TopCenter, Vector2.UnitX * (base.Width - 4f) * 0.5f, -(float)Math.PI / 2f);
			}
			SceneAs<Level>().ParticlesBG.Emit(P_Activate, (int)(base.Width / 2f), base.BottomCenter, Vector2.UnitX * (base.Width - 4f) * 0.5f, (float)Math.PI / 2f);
		}

		private void BreakParticles()
		{
			Vector2 from = base.Center;
			for (int x = 0; (float)x < base.Width; x += 4)
			{
				for (int y = 0; (float)y < base.Height; y += 4)
				{
					Vector2 at = Position + new Vector2(2 + x, 2 + y);
					SceneAs<Level>().Particles.Emit(P_Break, 1, at, Vector2.One * 2f, (at - from).Angle());
				}
			}
		}

		private void MoveParticles()
		{
			Vector2 at;
			Vector2 range;
			float dir;
			float add;
			if (direction == Directions.Right)
			{
				at = base.CenterLeft + Vector2.UnitX;
				range = Vector2.UnitY * (base.Height - 4f);
				dir = (float)Math.PI;
				add = base.Height / 32f;
			}
			else if (direction == Directions.Left)
			{
				at = base.CenterRight;
				range = Vector2.UnitY * (base.Height - 4f);
				dir = 0f;
				add = base.Height / 32f;
			}
			else if (direction == Directions.Down)
			{
				at = base.TopCenter + Vector2.UnitY;
				range = Vector2.UnitX * (base.Width - 4f);
				dir = -(float)Math.PI / 2f;
				add = base.Width / 32f;
			}
			else
			{
				at = base.BottomCenter;
				range = Vector2.UnitX * (base.Width - 4f);
				dir = (float)Math.PI / 2f;
				add = base.Width / 32f;
			}
			particleRemainder += add;
			int amount = (int)particleRemainder;
			particleRemainder -= amount;
			range *= 0.5f;
			if (amount > 0)
			{
				SceneAs<Level>().ParticlesBG.Emit(P_Move, amount, at, range, dir);
			}
		}

		private void ScrapeParticles(Vector2 dir)
		{
			_ = Collidable;
			Collidable = false;
			if (dir.X != 0f)
			{
				float xCheck = ((!(dir.X > 0f)) ? (base.Left - 1f) : base.Right);
				for (int y = 0; (float)y < base.Height; y += 8)
				{
					Vector2 at2 = new Vector2(xCheck, base.Top + 4f + (float)y);
					if (base.Scene.CollideCheck<Solid>(at2))
					{
						SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, at2);
					}
				}
			}
			else
			{
				float yCheck = ((!(dir.Y > 0f)) ? (base.Top - 1f) : base.Bottom);
				for (int x = 0; (float)x < base.Width; x += 8)
				{
					Vector2 at = new Vector2(base.Left + 4f + (float)x, yCheck);
					if (base.Scene.CollideCheck<Solid>(at))
					{
						SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, at);
					}
				}
			}
			Collidable = true;
		}
	}
}
