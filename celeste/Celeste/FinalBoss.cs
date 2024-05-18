using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class FinalBoss : Entity
	{
		public static ParticleType P_Burst;

		public const float CameraXPastMax = 140f;

		private const float MoveSpeed = 600f;

		private const float AvoidRadius = 12f;

		public Sprite Sprite;

		public PlayerSprite NormalSprite;

		private PlayerHair normalHair;

		private Vector2 avoidPos;

		public float CameraYPastMax;

		public bool Moving;

		public bool Sitting;

		private int facing;

		private Level level;

		private Circle circle;

		private Vector2[] nodes;

		private int nodeIndex;

		private int patternIndex;

		private Coroutine attackCoroutine;

		private Coroutine triggerBlocksCoroutine;

		private List<Entity> fallingBlocks;

		private List<Entity> movingBlocks;

		private bool playerHasMoved;

		private SineWave floatSine;

		private bool dialog;

		private bool startHit;

		private VertexLight light;

		private Wiggler scaleWiggler;

		private FinalBossStarfield bossBg;

		private SoundSource chargeSfx;

		private SoundSource laserSfx;

		public Vector2 BeamOrigin => base.Center + Sprite.Position + new Vector2(0f, -14f);

		public Vector2 ShotOrigin => base.Center + Sprite.Position + new Vector2(6f * Sprite.Scale.X, 2f);

		public FinalBoss(Vector2 position, Vector2[] nodes, int patternIndex, float cameraYPastMax, bool dialog, bool startHit, bool cameraLockY)
			: base(position)
		{
			this.patternIndex = patternIndex;
			CameraYPastMax = cameraYPastMax;
			this.dialog = dialog;
			this.startHit = startHit;
			Add(light = new VertexLight(Color.White, 1f, 32, 64));
			base.Collider = (circle = new Circle(14f, 0f, -6f));
			Add(new PlayerCollider(OnPlayer));
			this.nodes = new Vector2[nodes.Length + 1];
			this.nodes[0] = Position;
			for (int i = 0; i < nodes.Length; i++)
			{
				this.nodes[i + 1] = nodes[i];
			}
			attackCoroutine = new Coroutine(removeOnComplete: false);
			Add(attackCoroutine);
			triggerBlocksCoroutine = new Coroutine(removeOnComplete: false);
			Add(triggerBlocksCoroutine);
			Add(new CameraLocker(cameraLockY ? Level.CameraLockModes.FinalBoss : Level.CameraLockModes.FinalBossNoY, 140f, cameraYPastMax));
			Add(floatSine = new SineWave(0.6f));
			Add(scaleWiggler = Wiggler.Create(0.6f, 3f));
			Add(chargeSfx = new SoundSource());
			Add(laserSfx = new SoundSource());
		}

		public FinalBoss(EntityData e, Vector2 offset)
			: this(e.Position + offset, e.NodesOffset(offset), e.Int("patternIndex"), e.Float("cameraPastY", 120f), e.Bool("dialog"), e.Bool("startHit"), e.Bool("cameraLockY", defaultValue: true))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			level = SceneAs<Level>();
			if (patternIndex == 0)
			{
				NormalSprite = new PlayerSprite(PlayerSpriteMode.Badeline);
				NormalSprite.Scale.X = -1f;
				NormalSprite.Play("laugh");
				normalHair = new PlayerHair(NormalSprite);
				normalHair.Color = BadelineOldsite.HairColor;
				normalHair.Border = Color.Black;
				normalHair.Facing = Facings.Left;
				Add(normalHair);
				Add(NormalSprite);
			}
			else
			{
				CreateBossSprite();
			}
			bossBg = level.Background.Get<FinalBossStarfield>();
			if (patternIndex == 0 && !level.Session.GetFlag("boss_intro") && level.Session.Level.Equals("boss-00"))
			{
				level.Session.Audio.Music.Event = "event:/music/lvl2/phone_loop";
				level.Session.Audio.Apply();
				if (bossBg != null)
				{
					bossBg.Alpha = 0f;
				}
				Sitting = true;
				Position.Y += 16f;
				NormalSprite.Play("pretendDead");
				NormalSprite.Scale.X = 1f;
			}
			else if (patternIndex == 0 && !level.Session.GetFlag("boss_mid") && level.Session.Level.Equals("boss-14"))
			{
				level.Add(new CS06_BossMid());
			}
			else if (startHit)
			{
				Alarm.Set(this, 0.5f, delegate
				{
					OnPlayer(null);
				});
			}
			light.Position = ((Sprite != null) ? Sprite : NormalSprite).Position + new Vector2(0f, -10f);
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			fallingBlocks = base.Scene.Tracker.GetEntitiesCopy<FallingBlock>();
			fallingBlocks.Sort((Entity a, Entity b) => (int)(a.X - b.X));
			movingBlocks = base.Scene.Tracker.GetEntitiesCopy<FinalBossMovingBlock>();
			movingBlocks.Sort((Entity a, Entity b) => (int)(a.X - b.X));
		}

		private void CreateBossSprite()
		{
			Add(Sprite = GFX.SpriteBank.Create("badeline_boss"));
			Sprite.OnFrameChange = delegate(string anim)
			{
				if (anim == "idle" && Sprite.CurrentAnimationFrame == 18)
				{
					Audio.Play("event:/char/badeline/boss_idle_air", Position);
				}
			};
			facing = -1;
			if (NormalSprite != null)
			{
				Sprite.Position = NormalSprite.Position;
				Remove(NormalSprite);
			}
			if (normalHair != null)
			{
				Remove(normalHair);
			}
			NormalSprite = null;
			normalHair = null;
		}

		public override void Update()
		{
			base.Update();
			Sprite sprite = ((Sprite != null) ? Sprite : NormalSprite);
			if (!Sitting)
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (!Moving && player != null)
				{
					if (facing == -1 && player.X > base.X + 20f)
					{
						facing = 1;
						scaleWiggler.Start();
					}
					else if (facing == 1 && player.X < base.X - 20f)
					{
						facing = -1;
						scaleWiggler.Start();
					}
				}
				if (!playerHasMoved && player != null && player.Speed != Vector2.Zero)
				{
					playerHasMoved = true;
					if (patternIndex != 0)
					{
						StartAttacking();
					}
					TriggerMovingBlocks(0);
				}
				if (!Moving)
				{
					sprite.Position = avoidPos + new Vector2(floatSine.Value * 3f, floatSine.ValueOverTwo * 4f);
				}
				else
				{
					sprite.Position = Calc.Approach(sprite.Position, Vector2.Zero, 12f * Engine.DeltaTime);
				}
				float radius = circle.Radius;
				circle.Radius = 6f;
				CollideFirst<DashBlock>()?.Break(base.Center, -Vector2.UnitY);
				circle.Radius = radius;
				if (!level.IsInBounds(Position, 24f))
				{
					Active = (Visible = (Collidable = false));
					return;
				}
				Vector2 target;
				if (!Moving && player != null)
				{
					float dist = (base.Center - player.Center).Length();
					dist = Calc.ClampedMap(dist, 32f, 88f, 12f, 0f);
					target = ((!(dist <= 0f)) ? (base.Center - player.Center).SafeNormalize(dist) : Vector2.Zero);
				}
				else
				{
					target = Vector2.Zero;
				}
				avoidPos = Calc.Approach(avoidPos, target, 40f * Engine.DeltaTime);
			}
			light.Position = sprite.Position + new Vector2(0f, -10f);
		}

		public override void Render()
		{
			if (Sprite != null)
			{
				Sprite.Scale.X = facing;
				Sprite.Scale.Y = 1f;
				Sprite.Scale *= 1f + scaleWiggler.Value * 0.2f;
			}
			if (NormalSprite != null)
			{
				Vector2 was = NormalSprite.Position;
				NormalSprite.Position = NormalSprite.Position.Floor();
				base.Render();
				NormalSprite.Position = was;
			}
			else
			{
				base.Render();
			}
		}

		public void OnPlayer(Player player)
		{
			if (Sprite == null)
			{
				CreateBossSprite();
			}
			Sprite.Play("getHit");
			Audio.Play("event:/char/badeline/boss_hug", Position);
			chargeSfx.Stop();
			if (laserSfx.EventName == "event:/char/badeline/boss_laser_charge" && laserSfx.Playing)
			{
				laserSfx.Stop();
			}
			Collidable = false;
			avoidPos = Vector2.Zero;
			nodeIndex++;
			if (dialog)
			{
				if (nodeIndex == 1)
				{
					base.Scene.Add(new MiniTextbox("ch6_boss_tired_a"));
				}
				else if (nodeIndex == 2)
				{
					base.Scene.Add(new MiniTextbox("ch6_boss_tired_b"));
				}
				else if (nodeIndex == 3)
				{
					base.Scene.Add(new MiniTextbox("ch6_boss_tired_c"));
				}
			}
			foreach (FinalBossShot entity in level.Tracker.GetEntities<FinalBossShot>())
			{
				entity.Destroy();
			}
			foreach (FinalBossBeam entity2 in level.Tracker.GetEntities<FinalBossBeam>())
			{
				entity2.Destroy();
			}
			TriggerFallingBlocks(base.X);
			TriggerMovingBlocks(nodeIndex);
			attackCoroutine.Active = false;
			Moving = true;
			bool lastHit = nodeIndex == nodes.Length - 1;
			if (level.Session.Area.Mode == AreaMode.Normal)
			{
				if (lastHit && level.Session.Level.Equals("boss-19"))
				{
					Alarm.Set(this, 0.25f, delegate
					{
						Audio.Play("event:/game/06_reflection/boss_spikes_burst");
						foreach (CrystalStaticSpinner entity3 in base.Scene.Tracker.GetEntities<CrystalStaticSpinner>())
						{
							entity3.Destroy(boss: true);
						}
					});
					Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "postboss", 1f);
					Audio.SetMusic(null);
				}
				else if (startHit && level.Session.Audio.Music.Event != "event:/music/lvl6/badeline_glitch")
				{
					level.Session.Audio.Music.Event = "event:/music/lvl6/badeline_glitch";
					level.Session.Audio.Apply();
				}
				else if (level.Session.Audio.Music.Event != "event:/music/lvl6/badeline_fight" && level.Session.Audio.Music.Event != "event:/music/lvl6/badeline_glitch")
				{
					level.Session.Audio.Music.Event = "event:/music/lvl6/badeline_fight";
					level.Session.Audio.Apply();
				}
			}
			Add(new Coroutine(MoveSequence(player, lastHit)));
		}

		private IEnumerator MoveSequence(Player player, bool lastHit)
		{
			if (lastHit)
			{
				Audio.SetMusicParam("boss_pitch", 1f);
				Tween tween4 = Tween.Create(Tween.TweenMode.Oneshot, null, 0.3f, start: true);
				tween4.OnUpdate = delegate(Tween t)
				{
					Glitch.Value = 0.6f * t.Eased;
				};
				Add(tween4);
			}
			else
			{
				Tween tween3 = Tween.Create(Tween.TweenMode.Oneshot, null, 0.3f, start: true);
				tween3.OnUpdate = delegate(Tween t)
				{
					Glitch.Value = 0.5f * (1f - t.Eased);
				};
				Add(tween3);
			}
			if (player != null && !player.Dead)
			{
				player.StartAttract(base.Center + Vector2.UnitY * 4f);
			}
			float timer = 0.15f;
			while (player != null && !player.Dead && !player.AtAttractTarget)
			{
				yield return null;
				timer -= Engine.DeltaTime;
			}
			if (timer > 0f)
			{
				yield return timer;
			}
			foreach (ReflectionTentacles entity2 in base.Scene.Tracker.GetEntities<ReflectionTentacles>())
			{
				entity2.Retreat();
			}
			if (player != null)
			{
				Celeste.Freeze(0.1f);
				if (lastHit)
				{
					Engine.TimeRate = 0.5f;
				}
				else
				{
					Engine.TimeRate = 0.75f;
				}
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			}
			PushPlayer(player);
			level.Shake();
			yield return 0.05f;
			for (float i = 0f; i < (float)Math.PI * 2f; i += 0.17453292f)
			{
				Vector2 at = base.Center + Sprite.Position + Calc.AngleToVector(i + Calc.Random.Range(-(float)Math.PI / 90f, (float)Math.PI / 90f), Calc.Random.Range(16, 20));
				level.Particles.Emit(P_Burst, at, i);
			}
			yield return 0.05f;
			Audio.SetMusicParam("boss_pitch", 0f);
			float from2 = Engine.TimeRate;
			Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, null, 0.35f / Engine.TimeRateB, start: true);
			tween2.UseRawDeltaTime = true;
			tween2.OnUpdate = delegate(Tween t)
			{
				if (bossBg != null && bossBg.Alpha < t.Eased)
				{
					bossBg.Alpha = t.Eased;
				}
				Engine.TimeRate = MathHelper.Lerp(from2, 1f, t.Eased);
				if (lastHit)
				{
					Glitch.Value = 0.6f * (1f - t.Eased);
				}
			};
			Add(tween2);
			yield return 0.2f;
			Vector2 from = Position;
			Vector2 to = nodes[nodeIndex];
			float time = Vector2.Distance(from, to) / 600f;
			float dir = (to - from).Angle();
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, time, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				Position = Vector2.Lerp(from, to, t.Eased);
				if (t.Eased >= 0.1f && t.Eased <= 0.9f && base.Scene.OnInterval(0.02f))
				{
					TrailManager.Add(this, Player.NormalHairColor, 0.5f);
					level.Particles.Emit(Player.P_DashB, 2, base.Center, Vector2.One * 3f, dir);
				}
			};
			tween.OnComplete = delegate
			{
				Sprite.Play("recoverHit");
				Moving = false;
				Collidable = true;
				Player entity = base.Scene.Tracker.GetEntity<Player>();
				if (entity != null)
				{
					facing = Math.Sign(entity.X - base.X);
					if (facing == 0)
					{
						facing = -1;
					}
				}
				StartAttacking();
				floatSine.Reset();
			};
			Add(tween);
		}

		private void PushPlayer(Player player)
		{
			if (player != null && !player.Dead)
			{
				int dir = Math.Sign(base.X - nodes[nodeIndex].X);
				if (dir == 0)
				{
					dir = -1;
				}
				player.FinalBossPushLaunch(dir);
			}
			SceneAs<Level>().Displacement.AddBurst(Position, 0.4f, 12f, 36f, 0.5f);
			SceneAs<Level>().Displacement.AddBurst(Position, 0.4f, 24f, 48f, 0.5f);
			SceneAs<Level>().Displacement.AddBurst(Position, 0.4f, 36f, 60f, 0.5f);
		}

		private void TriggerFallingBlocks(float leftOfX)
		{
			while (fallingBlocks.Count > 0 && fallingBlocks[0].Scene == null)
			{
				fallingBlocks.RemoveAt(0);
			}
			int num = 0;
			while (fallingBlocks.Count > 0 && fallingBlocks[0].X < leftOfX)
			{
				FallingBlock obj = fallingBlocks[0] as FallingBlock;
				obj.StartShaking();
				obj.Triggered = true;
				obj.FallDelay = 0.4f * (float)num;
				num++;
				fallingBlocks.RemoveAt(0);
			}
		}

		private void TriggerMovingBlocks(int nodeIndex)
		{
			if (nodeIndex > 0)
			{
				DestroyMovingBlocks(nodeIndex - 1);
			}
			float delay = 0f;
			foreach (FinalBossMovingBlock block in movingBlocks)
			{
				if (block.BossNodeIndex == nodeIndex)
				{
					block.StartMoving(delay);
					delay += 0.15f;
				}
			}
		}

		private void DestroyMovingBlocks(int nodeIndex)
		{
			float delay = 0f;
			foreach (FinalBossMovingBlock block in movingBlocks)
			{
				if (block.BossNodeIndex == nodeIndex)
				{
					block.Destroy(delay);
					delay += 0.05f;
				}
			}
		}

		private void StartAttacking()
		{
			switch (patternIndex)
			{
			case 0:
			case 1:
				attackCoroutine.Replace(Attack01Sequence());
				break;
			case 2:
				attackCoroutine.Replace(Attack02Sequence());
				break;
			case 3:
				attackCoroutine.Replace(Attack03Sequence());
				break;
			case 4:
				attackCoroutine.Replace(Attack04Sequence());
				break;
			case 5:
				attackCoroutine.Replace(Attack05Sequence());
				break;
			case 6:
				attackCoroutine.Replace(Attack06Sequence());
				break;
			case 7:
				attackCoroutine.Replace(Attack07Sequence());
				break;
			case 8:
				attackCoroutine.Replace(Attack08Sequence());
				break;
			case 9:
				attackCoroutine.Replace(Attack09Sequence());
				break;
			case 10:
				attackCoroutine.Replace(Attack10Sequence());
				break;
			case 11:
				attackCoroutine.Replace(Attack11Sequence());
				break;
			case 13:
				attackCoroutine.Replace(Attack13Sequence());
				break;
			case 14:
				attackCoroutine.Replace(Attack14Sequence());
				break;
			case 15:
				attackCoroutine.Replace(Attack15Sequence());
				break;
			case 12:
				break;
			}
		}

		private void StartShootCharge()
		{
			Sprite.Play("attack1Begin");
			chargeSfx.Play("event:/char/badeline/boss_bullet");
		}

		private IEnumerator Attack01Sequence()
		{
			StartShootCharge();
			while (true)
			{
				yield return 0.5f;
				Shoot();
				yield return 1f;
				StartShootCharge();
				yield return 0.15f;
				yield return 0.3f;
			}
		}

		private IEnumerator Attack02Sequence()
		{
			while (true)
			{
				yield return 0.5f;
				yield return Beam();
				yield return 0.4f;
				StartShootCharge();
				yield return 0.3f;
				Shoot();
				yield return 0.5f;
				yield return 0.3f;
			}
		}

		private IEnumerator Attack03Sequence()
		{
			StartShootCharge();
			yield return 0.1f;
			while (true)
			{
				for (int i = 0; i < 5; i++)
				{
					Player player = level.Tracker.GetEntity<Player>();
					if (player != null)
					{
						Vector2 at = player.Center;
						for (int j = 0; j < 2; j++)
						{
							ShootAt(at);
							yield return 0.15f;
						}
					}
					if (i < 4)
					{
						StartShootCharge();
						yield return 0.5f;
					}
				}
				yield return 2f;
				StartShootCharge();
				yield return 0.7f;
			}
		}

		private IEnumerator Attack04Sequence()
		{
			StartShootCharge();
			yield return 0.1f;
			while (true)
			{
				for (int i = 0; i < 5; i++)
				{
					Player player = level.Tracker.GetEntity<Player>();
					if (player != null)
					{
						Vector2 at = player.Center;
						for (int j = 0; j < 2; j++)
						{
							ShootAt(at);
							yield return 0.15f;
						}
					}
					if (i < 4)
					{
						StartShootCharge();
						yield return 0.5f;
					}
				}
				yield return 1.5f;
				yield return Beam();
				yield return 1.5f;
				StartShootCharge();
			}
		}

		private IEnumerator Attack05Sequence()
		{
			yield return 0.2f;
			while (true)
			{
				yield return Beam();
				yield return 0.6f;
				StartShootCharge();
				yield return 0.3f;
				for (int i = 0; i < 3; i++)
				{
					Player player = level.Tracker.GetEntity<Player>();
					if (player != null)
					{
						Vector2 at = player.Center;
						for (int j = 0; j < 2; j++)
						{
							ShootAt(at);
							yield return 0.15f;
						}
					}
					if (i < 2)
					{
						StartShootCharge();
						yield return 0.5f;
					}
				}
				yield return 0.8f;
			}
		}

		private IEnumerator Attack06Sequence()
		{
			while (true)
			{
				yield return Beam();
				yield return 0.7f;
			}
		}

		private IEnumerator Attack07Sequence()
		{
			while (true)
			{
				Shoot();
				yield return 0.8f;
				StartShootCharge();
				yield return 0.8f;
			}
		}

		private IEnumerator Attack08Sequence()
		{
			while (true)
			{
				yield return 0.1f;
				yield return Beam();
				yield return 0.8f;
			}
		}

		private IEnumerator Attack09Sequence()
		{
			StartShootCharge();
			while (true)
			{
				yield return 0.5f;
				Shoot();
				yield return 0.15f;
				StartShootCharge();
				Shoot();
				yield return 0.4f;
				StartShootCharge();
				yield return 0.1f;
			}
		}

		private IEnumerator Attack10Sequence()
		{
			yield break;
		}

		private IEnumerator Attack11Sequence()
		{
			if (nodeIndex == 0)
			{
				StartShootCharge();
				yield return 0.6f;
			}
			while (true)
			{
				Shoot();
				yield return 1.9f;
				StartShootCharge();
				yield return 0.6f;
			}
		}

		private IEnumerator Attack13Sequence()
		{
			if (nodeIndex != 0)
			{
				yield return Attack01Sequence();
			}
		}

		private IEnumerator Attack14Sequence()
		{
			while (true)
			{
				yield return 0.2f;
				yield return Beam();
				yield return 0.3f;
			}
		}

		private IEnumerator Attack15Sequence()
		{
			while (true)
			{
				yield return 0.2f;
				yield return Beam();
				yield return 1.2f;
			}
		}

		private void Shoot(float angleOffset = 0f)
		{
			if (!chargeSfx.Playing)
			{
				chargeSfx.Play("event:/char/badeline/boss_bullet", "end", 1f);
			}
			else
			{
				chargeSfx.Param("end", 1f);
			}
			Sprite.Play("attack1Recoil", restart: true);
			Player player = level.Tracker.GetEntity<Player>();
			if (player != null)
			{
				level.Add(Engine.Pooler.Create<FinalBossShot>().Init(this, player, angleOffset));
			}
		}

		private void ShootAt(Vector2 at)
		{
			if (!chargeSfx.Playing)
			{
				chargeSfx.Play("event:/char/badeline/boss_bullet", "end", 1f);
			}
			else
			{
				chargeSfx.Param("end", 1f);
			}
			Sprite.Play("attack1Recoil", restart: true);
			level.Add(Engine.Pooler.Create<FinalBossShot>().Init(this, at));
		}

		private IEnumerator Beam()
		{
			laserSfx.Play("event:/char/badeline/boss_laser_charge");
			Sprite.Play("attack2Begin", restart: true);
			yield return 0.1f;
			Player player = level.Tracker.GetEntity<Player>();
			if (player != null)
			{
				level.Add(Engine.Pooler.Create<FinalBossBeam>().Init(this, player));
			}
			yield return 0.9f;
			Sprite.Play("attack2Lock", restart: true);
			yield return 0.5f;
			laserSfx.Stop();
			Audio.Play("event:/char/badeline/boss_laser_fire", Position);
			Sprite.Play("attack2Recoil");
		}

		public override void Removed(Scene scene)
		{
			if (bossBg != null && patternIndex == 0)
			{
				bossBg.Alpha = 1f;
			}
			base.Removed(scene);
		}
	}
}
