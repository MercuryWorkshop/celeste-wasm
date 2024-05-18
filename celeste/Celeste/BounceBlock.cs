using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class BounceBlock : Solid
	{
		private enum States
		{
			Waiting,
			WindingUp,
			Bouncing,
			BounceEnd,
			Broken
		}

		[Pooled]
		private class RespawnDebris : Entity
		{
			private Image sprite;

			private Vector2 from;

			private Vector2 to;

			private float percent;

			private float duration;

			public RespawnDebris Init(Vector2 from, Vector2 to, bool ice, float duration)
			{
				List<MTexture> textures = GFX.Game.GetAtlasSubtextures(ice ? "objects/bumpblocknew/ice_rubble" : "objects/bumpblocknew/fire_rubble");
				MTexture tex = Calc.Random.Choose(textures);
				if (sprite == null)
				{
					Add(sprite = new Image(tex));
					sprite.CenterOrigin();
				}
				else
				{
					sprite.Texture = tex;
				}
				Position = (this.from = from);
				percent = 0f;
				this.to = to;
				this.duration = duration;
				return this;
			}

			public override void Update()
			{
				if (percent > 1f)
				{
					RemoveSelf();
					return;
				}
				percent += Engine.DeltaTime / duration;
				Position = Vector2.Lerp(from, to, Ease.CubeIn(percent));
				sprite.Color = Color.White * percent;
			}

			public override void Render()
			{
				sprite.DrawOutline(Color.Black);
				base.Render();
			}
		}

		[Pooled]
		private class BreakDebris : Entity
		{
			private Image sprite;

			private Vector2 speed;

			private float percent;

			private float duration;

			public BreakDebris Init(Vector2 position, Vector2 direction, bool ice)
			{
				List<MTexture> textures = GFX.Game.GetAtlasSubtextures(ice ? "objects/bumpblocknew/ice_rubble" : "objects/bumpblocknew/fire_rubble");
				MTexture tex = Calc.Random.Choose(textures);
				if (sprite == null)
				{
					Add(sprite = new Image(tex));
					sprite.CenterOrigin();
				}
				else
				{
					sprite.Texture = tex;
				}
				Position = position;
				direction = Calc.AngleToVector(direction.Angle() + Calc.Random.Range(-0.1f, 0.1f), 1f);
				speed = direction * (ice ? Calc.Random.Range(20, 40) : Calc.Random.Range(120, 200));
				percent = 0f;
				duration = Calc.Random.Range(2, 3);
				return this;
			}

			public override void Update()
			{
				base.Update();
				if (percent >= 1f)
				{
					RemoveSelf();
					return;
				}
				Position += speed * Engine.DeltaTime;
				speed.X = Calc.Approach(speed.X, 0f, 180f * Engine.DeltaTime);
				speed.Y += 200f * Engine.DeltaTime;
				percent += Engine.DeltaTime / duration;
				sprite.Color = Color.White * (1f - percent);
			}

			public override void Render()
			{
				sprite.DrawOutline(Color.Black);
				base.Render();
			}
		}

		public static ParticleType P_Reform;

		public static ParticleType P_FireBreak;

		public static ParticleType P_IceBreak;

		private const float WindUpDelay = 0f;

		private const float WindUpDist = 10f;

		private const float IceWindUpDist = 16f;

		private const float BounceDist = 24f;

		private const float LiftSpeedXMult = 0.75f;

		private const float RespawnTime = 1.6f;

		private const float WallPushTime = 0.1f;

		private const float BounceEndTime = 0.05f;

		private Vector2 bounceDir;

		private States state;

		private Vector2 startPos;

		private float moveSpeed;

		private float windUpStartTimer;

		private float windUpProgress;

		private bool iceMode;

		private bool iceModeNext;

		private float respawnTimer;

		private float bounceEndTimer;

		private Vector2 bounceLift;

		private float reappearFlash;

		private bool reformed = true;

		private Vector2 debrisDirection;

		private List<Image> hotImages;

		private List<Image> coldImages;

		private Sprite hotCenterSprite;

		private Sprite coldCenterSprite;

		public BounceBlock(Vector2 position, float width, float height)
			: base(position, width, height, safe: false)
		{
			state = States.Waiting;
			startPos = Position;
			hotImages = BuildSprite(GFX.Game["objects/bumpblocknew/fire00"]);
			hotCenterSprite = GFX.SpriteBank.Create("bumpBlockCenterFire");
			hotCenterSprite.Position = new Vector2(base.Width, base.Height) / 2f;
			hotCenterSprite.Visible = false;
			Add(hotCenterSprite);
			coldImages = BuildSprite(GFX.Game["objects/bumpblocknew/ice00"]);
			coldCenterSprite = GFX.SpriteBank.Create("bumpBlockCenterIce");
			coldCenterSprite.Position = new Vector2(base.Width, base.Height) / 2f;
			coldCenterSprite.Visible = false;
			Add(coldCenterSprite);
			Add(new CoreModeListener(OnChangeMode));
		}

		public BounceBlock(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height)
		{
		}

		private List<Image> BuildSprite(MTexture source)
		{
			List<Image> imgs = new List<Image>();
			int tc = source.Width / 8;
			int tr = source.Height / 8;
			for (int x = 0; (float)x < base.Width; x += 8)
			{
				for (int y = 0; (float)y < base.Height; y += 8)
				{
					int tx = ((x != 0) ? ((!((float)x >= base.Width - 8f)) ? Calc.Random.Next(1, tc - 1) : (tc - 1)) : 0);
					int ty = ((y != 0) ? ((!((float)y >= base.Height - 8f)) ? Calc.Random.Next(1, tr - 1) : (tr - 1)) : 0);
					Image img = new Image(source.GetSubtexture(tx * 8, ty * 8, 8, 8));
					img.Position = new Vector2(x, y);
					imgs.Add(img);
					Add(img);
				}
			}
			return imgs;
		}

		private void ToggleSprite()
		{
			hotCenterSprite.Visible = !iceMode;
			coldCenterSprite.Visible = iceMode;
			foreach (Image hotImage in hotImages)
			{
				hotImage.Visible = !iceMode;
			}
			foreach (Image coldImage in coldImages)
			{
				coldImage.Visible = iceMode;
			}
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			iceModeNext = (iceMode = SceneAs<Level>().CoreMode == Session.CoreModes.Cold);
			ToggleSprite();
		}

		private void OnChangeMode(Session.CoreModes coreMode)
		{
			iceModeNext = coreMode == Session.CoreModes.Cold;
		}

		private void CheckModeChange()
		{
			if (iceModeNext != iceMode)
			{
				iceMode = iceModeNext;
				ToggleSprite();
			}
		}

		public override void Render()
		{
			Vector2 was = Position;
			Position += base.Shake;
			if (state != States.Broken && reformed)
			{
				base.Render();
			}
			if (reappearFlash > 0f)
			{
				float e = Ease.CubeOut(reappearFlash);
				float s = e * 2f;
				Draw.Rect(base.X - s, base.Y - s, base.Width + s * 2f, base.Height + s * 2f, Color.White * e);
			}
			Position = was;
		}

		public override void Update()
		{
			base.Update();
			reappearFlash = Calc.Approach(reappearFlash, 0f, Engine.DeltaTime * 8f);
			if (state == States.Waiting)
			{
				CheckModeChange();
				moveSpeed = Calc.Approach(moveSpeed, 100f, 400f * Engine.DeltaTime);
				Vector2 at = Calc.Approach(base.ExactPosition, startPos, moveSpeed * Engine.DeltaTime);
				Vector2 lift2 = (at - base.ExactPosition).SafeNormalize(moveSpeed);
				lift2.X *= 0.75f;
				MoveTo(at, lift2);
				windUpProgress = Calc.Approach(windUpProgress, 0f, 1f * Engine.DeltaTime);
				Player player2 = WindUpPlayerCheck();
				if (player2 != null)
				{
					moveSpeed = 80f;
					windUpStartTimer = 0f;
					if (iceMode)
					{
						bounceDir = -Vector2.UnitY;
					}
					else
					{
						bounceDir = (player2.Center - base.Center).SafeNormalize();
					}
					state = States.WindingUp;
					Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
					if (iceMode)
					{
						StartShaking(0.2f);
						Audio.Play("event:/game/09_core/iceblock_touch", base.Center);
					}
					else
					{
						Audio.Play("event:/game/09_core/bounceblock_touch", base.Center);
					}
				}
			}
			else if (state == States.WindingUp)
			{
				Player player = WindUpPlayerCheck();
				if (player != null)
				{
					if (iceMode)
					{
						bounceDir = -Vector2.UnitY;
					}
					else
					{
						bounceDir = (player.Center - base.Center).SafeNormalize();
					}
				}
				if (windUpStartTimer > 0f)
				{
					windUpStartTimer -= Engine.DeltaTime;
					windUpProgress = Calc.Approach(windUpProgress, 0f, 1f * Engine.DeltaTime);
					return;
				}
				moveSpeed = Calc.Approach(moveSpeed, iceMode ? 35f : 40f, 600f * Engine.DeltaTime);
				float mult = (iceMode ? 0.333f : 1f);
				Vector2 target2 = startPos - bounceDir * (iceMode ? 16f : 10f);
				Vector2 at3 = Calc.Approach(base.ExactPosition, target2, moveSpeed * mult * Engine.DeltaTime);
				Vector2 lift = (at3 - base.ExactPosition).SafeNormalize(moveSpeed * mult);
				lift.X *= 0.75f;
				MoveTo(at3, lift);
				windUpProgress = Calc.ClampedMap(Vector2.Distance(base.ExactPosition, target2), 16f, 2f);
				if (iceMode && Vector2.DistanceSquared(base.ExactPosition, target2) <= 12f)
				{
					StartShaking(0.1f);
				}
				else if (!iceMode && windUpProgress >= 0.5f)
				{
					StartShaking(0.1f);
				}
				if (Vector2.DistanceSquared(base.ExactPosition, target2) <= 2f)
				{
					if (iceMode)
					{
						Break();
					}
					else
					{
						state = States.Bouncing;
					}
					moveSpeed = 0f;
				}
			}
			else if (state == States.Bouncing)
			{
				moveSpeed = Calc.Approach(moveSpeed, 140f, 800f * Engine.DeltaTime);
				Vector2 target = startPos + bounceDir * 24f;
				Vector2 at2 = Calc.Approach(base.ExactPosition, target, moveSpeed * Engine.DeltaTime);
				bounceLift = (at2 - base.ExactPosition).SafeNormalize(Math.Min(moveSpeed * 3f, 200f));
				bounceLift.X *= 0.75f;
				MoveTo(at2, bounceLift);
				windUpProgress = 1f;
				if (base.ExactPosition == target || (!iceMode && WindUpPlayerCheck() == null))
				{
					debrisDirection = (target - startPos).SafeNormalize();
					state = States.BounceEnd;
					Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
					moveSpeed = 0f;
					bounceEndTimer = 0.05f;
					ShakeOffPlayer(bounceLift);
				}
			}
			else if (state == States.BounceEnd)
			{
				bounceEndTimer -= Engine.DeltaTime;
				if (bounceEndTimer <= 0f)
				{
					Break();
				}
			}
			else
			{
				if (state != States.Broken)
				{
					return;
				}
				base.Depth = 8990;
				reformed = false;
				if (respawnTimer > 0f)
				{
					respawnTimer -= Engine.DeltaTime;
					return;
				}
				Vector2 was = Position;
				Position = startPos;
				if (!CollideCheck<Actor>() && !CollideCheck<Solid>())
				{
					CheckModeChange();
					Audio.Play(iceMode ? "event:/game/09_core/iceblock_reappear" : "event:/game/09_core/bounceblock_reappear", base.Center);
					float duration = 0.35f;
					for (int x = 0; (float)x < base.Width; x += 8)
					{
						for (int y = 0; (float)y < base.Height; y += 8)
						{
							Vector2 pos = new Vector2(base.X + (float)x + 4f, base.Y + (float)y + 4f);
							base.Scene.Add(Engine.Pooler.Create<RespawnDebris>().Init(pos + (pos - base.Center).SafeNormalize() * 12f, pos, iceMode, duration));
						}
					}
					Alarm.Set(this, duration, delegate
					{
						reformed = true;
						reappearFlash = 0.6f;
						EnableStaticMovers();
						ReformParticles();
					});
					base.Depth = -9000;
					MoveStaticMovers(Position - was);
					Collidable = true;
					state = States.Waiting;
				}
				else
				{
					Position = was;
				}
			}
		}

		private void ReformParticles()
		{
			Level level = SceneAs<Level>();
			for (int x = 0; (float)x < base.Width; x += 4)
			{
				level.Particles.Emit(P_Reform, new Vector2(base.X + 2f + (float)x + (float)Calc.Random.Range(-1, 1), base.Y), -(float)Math.PI / 2f);
				level.Particles.Emit(P_Reform, new Vector2(base.X + 2f + (float)x + (float)Calc.Random.Range(-1, 1), base.Bottom - 1f), (float)Math.PI / 2f);
			}
			for (int y = 0; (float)y < base.Height; y += 4)
			{
				level.Particles.Emit(P_Reform, new Vector2(base.X, base.Y + 2f + (float)y + (float)Calc.Random.Range(-1, 1)), (float)Math.PI);
				level.Particles.Emit(P_Reform, new Vector2(base.Right - 1f, base.Y + 2f + (float)y + (float)Calc.Random.Range(-1, 1)), 0f);
			}
		}

		private Player WindUpPlayerCheck()
		{
			Player player = CollideFirst<Player>(Position - Vector2.UnitY);
			if (player != null && player.Speed.Y < 0f)
			{
				player = null;
			}
			if (player == null)
			{
				player = CollideFirst<Player>(Position + Vector2.UnitX);
				if (player == null || player.StateMachine.State != 1 || player.Facing != Facings.Left)
				{
					player = CollideFirst<Player>(Position - Vector2.UnitX);
					if (player == null || player.StateMachine.State != 1 || player.Facing != Facings.Right)
					{
						player = null;
					}
				}
			}
			return player;
		}

		private void ShakeOffPlayer(Vector2 liftSpeed)
		{
			Player player = WindUpPlayerCheck();
			if (player != null)
			{
				player.StateMachine.State = 0;
				player.Speed = liftSpeed;
				player.StartJumpGraceTime();
			}
		}

		private void Break()
		{
			if (!iceMode)
			{
				Audio.Play("event:/game/09_core/bounceblock_break", base.Center);
			}
			Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
			state = States.Broken;
			Collidable = false;
			DisableStaticMovers();
			respawnTimer = 1.6f;
			Vector2 direction = new Vector2(0f, 1f);
			if (!iceMode)
			{
				direction = debrisDirection;
			}
			Vector2 center = base.Center;
			for (int x2 = 0; (float)x2 < base.Width; x2 += 8)
			{
				for (int y = 0; (float)y < base.Height; y += 8)
				{
					if (iceMode)
					{
						direction = (new Vector2(base.X + (float)x2 + 4f, base.Y + (float)y + 4f) - center).SafeNormalize();
					}
					base.Scene.Add(Engine.Pooler.Create<BreakDebris>().Init(new Vector2(base.X + (float)x2 + 4f, base.Y + (float)y + 4f), direction, iceMode));
				}
			}
			float debrisDir = debrisDirection.Angle();
			Level level = SceneAs<Level>();
			for (int x = 0; (float)x < base.Width; x += 4)
			{
				for (int y2 = 0; (float)y2 < base.Height; y2 += 4)
				{
					Vector2 at = Position + new Vector2(2 + x, 2 + y2) + Calc.Random.Range(-Vector2.One, Vector2.One);
					float dir = (iceMode ? (at - center).Angle() : debrisDir);
					level.Particles.Emit(iceMode ? P_IceBreak : P_FireBreak, at, dir);
				}
			}
		}
	}
}
