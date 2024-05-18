using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class AngryOshiro : Entity
	{
		private const int StChase = 0;

		private const int StChargeUp = 1;

		private const int StAttack = 2;

		private const int StDummy = 3;

		private const int StWaiting = 4;

		private const int StHurt = 5;

		private const float HitboxBackRange = 4f;

		public Sprite Sprite;

		private Sprite lightning;

		private bool lightningVisible;

		private VertexLight light;

		private Level level;

		private SineWave sine;

		private float cameraXOffset;

		private StateMachine state;

		private int attackIndex;

		private float targetAnxiety;

		private float anxietySpeed;

		private bool easeBackFromRightEdge;

		private bool fromCutscene;

		private bool doRespawnAnim;

		private bool leaving;

		private Shaker shaker;

		private PlayerCollider bounceCollider;

		private Vector2 colliderTargetPosition;

		private bool canControlTimeRate = true;

		private SoundSource prechargeSfx;

		private SoundSource chargeSfx;

		private bool hasEnteredSfx;

		private const float minCameraOffsetX = -48f;

		private const float yApproachTargetSpeed = 100f;

		private float yApproachSpeed = 100f;

		private static readonly float[] ChaseWaitTimes = new float[5] { 1f, 2f, 3f, 2f, 3f };

		private float attackSpeed;

		private const float HurtXSpeed = 100f;

		private const float HurtYSpeed = 200f;

		private float TargetY
		{
			get
			{
				Player player = level.Tracker.GetEntity<Player>();
				if (player != null)
				{
					return MathHelper.Clamp(player.CenterY, level.Bounds.Top + 8, level.Bounds.Bottom - 8);
				}
				return base.Y;
			}
		}

		public bool DummyMode => state.State == 3;

		public AngryOshiro(Vector2 position, bool fromCutscene)
			: base(position)
		{
			Add(Sprite = GFX.SpriteBank.Create("oshiro_boss"));
			Sprite.Play("idle");
			Add(lightning = GFX.SpriteBank.Create("oshiro_boss_lightning"));
			lightning.Visible = false;
			lightning.OnFinish = delegate
			{
				lightningVisible = false;
			};
			base.Collider = new Circle(14f);
			base.Collider.Position = (colliderTargetPosition = new Vector2(3f, 4f));
			Add(sine = new SineWave(0.5f));
			Add(bounceCollider = new PlayerCollider(OnPlayerBounce, new Hitbox(28f, 6f, -11f, -11f)));
			Add(new PlayerCollider(OnPlayer));
			base.Depth = -12500;
			Visible = false;
			Add(light = new VertexLight(Color.White, 1f, 32, 64));
			Add(shaker = new Shaker(on: false));
			state = new StateMachine();
			state.SetCallbacks(0, ChaseUpdate, ChaseCoroutine, ChaseBegin);
			state.SetCallbacks(1, ChargeUpUpdate, ChargeUpCoroutine, null, ChargeUpEnd);
			state.SetCallbacks(2, AttackUpdate, AttackCoroutine, AttackBegin, AttackEnd);
			state.SetCallbacks(3, null);
			state.SetCallbacks(4, WaitingUpdate);
			state.SetCallbacks(5, HurtUpdate, null, HurtBegin);
			Add(state);
			if (fromCutscene)
			{
				yApproachSpeed = 0f;
			}
			this.fromCutscene = fromCutscene;
			Add(new TransitionListener
			{
				OnOutBegin = delegate
				{
					if (base.X > (float)level.Bounds.Left + Sprite.Width / 2f)
					{
						Visible = false;
					}
					else
					{
						easeBackFromRightEdge = true;
					}
				},
				OnOut = delegate
				{
					lightning.Update();
					if (easeBackFromRightEdge)
					{
						base.X -= 128f * Engine.RawDeltaTime;
					}
				}
			});
			Add(prechargeSfx = new SoundSource());
			Add(chargeSfx = new SoundSource());
			Distort.AnxietyOrigin = new Vector2(1f, 0.5f);
		}

		public AngryOshiro(EntityData data, Vector2 offset)
			: this(data.Position + offset, fromCutscene: false)
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			level = SceneAs<Level>();
			if (level.Session.GetFlag("oshiroEnding") || (!level.Session.GetFlag("oshiro_resort_roof") && level.Session.Level.Equals("roof00")))
			{
				RemoveSelf();
			}
			if (state.State != 3 && !fromCutscene)
			{
				state.State = 4;
			}
			if (!fromCutscene)
			{
				base.Y = TargetY;
				cameraXOffset = -48f;
			}
			else
			{
				cameraXOffset = base.X - level.Camera.Left;
			}
		}

		private void OnPlayer(Player player)
		{
			if (state.State != 5 && (base.CenterX < player.CenterX + 4f || Sprite.CurrentAnimationID != "respawn"))
			{
				player.Die((player.Center - base.Center).SafeNormalize(Vector2.UnitX));
			}
		}

		private void OnPlayerBounce(Player player)
		{
			if (state.State == 2 && player.Bottom <= base.Top + 6f)
			{
				Audio.Play("event:/game/general/thing_booped", Position);
				Celeste.Freeze(0.2f);
				player.Bounce(base.Top + 2f);
				state.State = 5;
				prechargeSfx.Stop();
				chargeSfx.Stop();
			}
		}

		public override void Update()
		{
			base.Update();
			Sprite.Scale.X = Calc.Approach(Sprite.Scale.X, 1f, 0.6f * Engine.DeltaTime);
			Sprite.Scale.Y = Calc.Approach(Sprite.Scale.Y, 1f, 0.6f * Engine.DeltaTime);
			if (!doRespawnAnim)
			{
				Visible = base.X > (float)level.Bounds.Left - base.Width / 2f;
			}
			yApproachSpeed = Calc.Approach(yApproachSpeed, 100f, 300f * Engine.DeltaTime);
			if (state.State != 3 && canControlTimeRate)
			{
				if (state.State == 2 && attackSpeed > 200f)
				{
					Player player = base.Scene.Tracker.GetEntity<Player>();
					if (player != null && !player.Dead && base.CenterX < player.CenterX + 4f)
					{
						Engine.TimeRate = MathHelper.Lerp(Calc.ClampedMap(player.CenterX - base.CenterX, 30f, 80f, 0.5f), 1f, Calc.ClampedMap(Math.Abs(player.CenterY - base.CenterY), 32f, 48f));
					}
					else
					{
						Engine.TimeRate = 1f;
					}
				}
				else
				{
					Engine.TimeRate = 1f;
				}
				Distort.GameRate = Calc.Approach(Distort.GameRate, Calc.Map(Engine.TimeRate, 0.5f, 1f), Engine.DeltaTime * 8f);
				Distort.Anxiety = Calc.Approach(Distort.Anxiety, targetAnxiety, anxietySpeed * Engine.DeltaTime);
			}
			else
			{
				Distort.GameRate = 1f;
				Distort.Anxiety = 0f;
			}
		}

		public void StopControllingTime()
		{
			canControlTimeRate = false;
		}

		public override void Render()
		{
			if (lightningVisible)
			{
				lightning.RenderPosition = new Vector2(level.Camera.Left - 2f, base.Top + 16f);
				lightning.Render();
			}
			Sprite.Position = shaker.Value * 2f;
			base.Render();
		}

		public void Leave()
		{
			leaving = true;
		}

		public void Squish()
		{
			Sprite.Scale = new Vector2(1.3f, 0.5f);
			shaker.ShakeFor(0.5f, removeOnFinish: false);
		}

		private void ChaseBegin()
		{
			Sprite.Play("idle");
		}

		private int ChaseUpdate()
		{
			if (!hasEnteredSfx && cameraXOffset >= -16f && !doRespawnAnim)
			{
				Audio.Play("event:/char/oshiro/boss_enter_screen", Position);
				hasEnteredSfx = true;
			}
			if (doRespawnAnim && cameraXOffset >= 0f)
			{
				base.Collider.Position.X = -48f;
				Visible = true;
				Sprite.Play("respawn");
				doRespawnAnim = false;
				if (base.Scene.Tracker.GetEntity<Player>() != null)
				{
					Audio.Play("event:/char/oshiro/boss_reform", Position);
				}
			}
			cameraXOffset = Calc.Approach(cameraXOffset, 20f, 80f * Engine.DeltaTime);
			base.X = level.Camera.Left + cameraXOffset;
			base.Collider.Position.X = Calc.Approach(base.Collider.Position.X, colliderTargetPosition.X, Engine.DeltaTime * 128f);
			Collidable = Visible;
			if (level.Tracker.GetEntity<Player>() != null && Sprite.CurrentAnimationID != "respawn")
			{
				base.CenterY = Calc.Approach(base.CenterY, TargetY, yApproachSpeed * Engine.DeltaTime);
			}
			return 0;
		}

		private IEnumerator ChaseCoroutine()
		{
			if (level.Session.Area.Mode != 0)
			{
				yield return 1f;
			}
			else
			{
				yield return ChaseWaitTimes[attackIndex];
				attackIndex++;
				attackIndex %= ChaseWaitTimes.Length;
			}
			prechargeSfx.Play("event:/char/oshiro/boss_precharge");
			Sprite.Play("charge");
			yield return 0.7f;
			if (base.Scene.Tracker.GetEntity<Player>() != null)
			{
				Alarm.Set(this, 0.216f, delegate
				{
					chargeSfx.Play("event:/char/oshiro/boss_charge");
				});
				state.State = 1;
			}
			else
			{
				Sprite.Play("idle");
			}
		}

		private int ChargeUpUpdate()
		{
			if (level.OnInterval(0.05f))
			{
				Sprite.Position = Calc.Random.ShakeVector();
			}
			cameraXOffset = Calc.Approach(cameraXOffset, 0f, 40f * Engine.DeltaTime);
			base.X = level.Camera.Left + cameraXOffset;
			Player player = level.Tracker.GetEntity<Player>();
			if (player != null)
			{
				base.CenterY = Calc.Approach(base.CenterY, MathHelper.Clamp(player.CenterY, level.Bounds.Top + 8, level.Bounds.Bottom - 8), 30f * Engine.DeltaTime);
			}
			return 1;
		}

		private void ChargeUpEnd()
		{
			Sprite.Position = Vector2.Zero;
		}

		private IEnumerator ChargeUpCoroutine()
		{
			Celeste.Freeze(0.05f);
			Distort.Anxiety = 0.3f;
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			lightningVisible = true;
			lightning.Play("once", restart: true);
			yield return 0.3f;
			if (base.Scene.Tracker.GetEntity<Player>() != null)
			{
				state.State = 2;
			}
			else
			{
				state.State = 0;
			}
		}

		private void AttackBegin()
		{
			attackSpeed = 0f;
			targetAnxiety = 0.3f;
			anxietySpeed = 4f;
			level.DirectionalShake(Vector2.UnitX);
		}

		private void AttackEnd()
		{
			targetAnxiety = 0f;
			anxietySpeed = 0.5f;
		}

		private int AttackUpdate()
		{
			base.X += attackSpeed * Engine.DeltaTime;
			attackSpeed = Calc.Approach(attackSpeed, 500f, 2000f * Engine.DeltaTime);
			if (base.X >= level.Camera.Right + 48f)
			{
				if (leaving)
				{
					RemoveSelf();
					return 2;
				}
				base.X = level.Camera.Left + -48f;
				cameraXOffset = -48f;
				doRespawnAnim = true;
				Visible = false;
				return 0;
			}
			Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
			if (base.Scene.OnInterval(0.05f))
			{
				TrailManager.Add(this, Color.Red * 0.6f, 0.5f);
			}
			return 2;
		}

		private IEnumerator AttackCoroutine()
		{
			yield return 0.1f;
			targetAnxiety = 0f;
			anxietySpeed = 0.5f;
		}

		public void EnterDummyMode()
		{
			state.State = 3;
		}

		public void LeaveDummyMode()
		{
			state.State = 0;
		}

		private int WaitingUpdate()
		{
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null && player.Speed != Vector2.Zero && player.X > (float)(level.Bounds.Left + 48))
			{
				return 0;
			}
			return 4;
		}

		private void HurtBegin()
		{
			Sprite.Play("hurt", restart: true);
		}

		private int HurtUpdate()
		{
			base.X += 100f * Engine.DeltaTime;
			base.Y += 200f * Engine.DeltaTime;
			if (base.Top > (float)(level.Bounds.Bottom + 20))
			{
				if (leaving)
				{
					RemoveSelf();
					return 5;
				}
				base.X = level.Camera.Left + -48f;
				cameraXOffset = -48f;
				doRespawnAnim = true;
				Visible = false;
				return 0;
			}
			return 5;
		}
	}
}
