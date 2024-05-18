using System;
using System.Collections;
using System.Diagnostics;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class BadelineBoost : Entity
	{
		public static ParticleType P_Ambience;

		public static ParticleType P_Move;

		private const float MoveSpeed = 320f;

		private Sprite sprite;

		private Image stretch;

		private Wiggler wiggler;

		private VertexLight light;

		private BloomPoint bloom;

		private bool canSkip;

		private bool finalCh9Boost;

		private bool finalCh9GoldenBoost;

		private bool finalCh9Dialog;

		private Vector2[] nodes;

		private int nodeIndex;

		private bool travelling;

		private Player holding;

		private SoundSource relocateSfx;

		public EventInstance? Ch9FinalBoostSfx;

		public BadelineBoost(Vector2[] nodes, bool lockCamera, bool canSkip = false, bool finalCh9Boost = false, bool finalCh9GoldenBoost = false, bool finalCh9Dialog = false)
			: base(nodes[0])
		{
			base.Depth = -1000000;
			this.nodes = nodes;
			this.canSkip = canSkip;
			this.finalCh9Boost = finalCh9Boost;
			this.finalCh9GoldenBoost = finalCh9GoldenBoost;
			this.finalCh9Dialog = finalCh9Dialog;
			base.Collider = new Circle(16f);
			Add(new PlayerCollider(OnPlayer));
			Add(sprite = GFX.SpriteBank.Create("badelineBoost"));
			Add(stretch = new Image(GFX.Game["objects/badelineboost/stretch"]));
			stretch.Visible = false;
			stretch.CenterOrigin();
			Add(light = new VertexLight(Color.White, 0.7f, 12, 20));
			Add(bloom = new BloomPoint(0.5f, 12f));
			Add(wiggler = Wiggler.Create(0.4f, 3f, delegate
			{
				sprite.Scale = Vector2.One * (1f + wiggler.Value * 0.4f);
			}));
			if (lockCamera)
			{
				Add(new CameraLocker(Level.CameraLockModes.BoostSequence, 0f, 160f));
			}
			Add(relocateSfx = new SoundSource());
		}

		public BadelineBoost(EntityData data, Vector2 offset)
			: this(data.NodesWithPosition(offset), data.Bool("lockCamera", defaultValue: true), data.Bool("canSkip"), data.Bool("finalCh9Boost"), data.Bool("finalCh9GoldenBoost"), data.Bool("finalCh9Dialog"))
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (CollideCheck<FakeWall>())
			{
				base.Depth = -12500;
			}
		}

		private void OnPlayer(Player player)
		{
			Add(new Coroutine(BoostRoutine(player)));
		}

		private IEnumerator BoostRoutine(Player player)
		{
			holding = player;
			travelling = true;
			nodeIndex++;
			sprite.Visible = false;
			sprite.Position = Vector2.Zero;
			Collidable = false;
			bool finalBoost = nodeIndex >= nodes.Length;
			Level level = base.Scene as Level;
			bool endLevel;
			if (finalBoost && finalCh9GoldenBoost)
			{
				endLevel = true;
			}
			else
			{
				bool hasGolden = false;
				foreach (Follower follower in player.Leader.Followers)
				{
					if (follower.Entity is Strawberry strawb && strawb.Golden)
					{
						hasGolden = true;
						break;
					}
				}
				endLevel = finalBoost && finalCh9Boost && !hasGolden;
			}
			Stopwatch sw = new Stopwatch();
			sw.Start();
			if (finalCh9Boost)
			{
				Audio.Play("event:/new_content/char/badeline/booster_finalfinal_part1", Position);
			}
			else if (!finalBoost)
			{
				Audio.Play("event:/char/badeline/booster_begin", Position);
			}
			else
			{
				Audio.Play("event:/char/badeline/booster_final", Position);
			}
			if (player.Holding != null)
			{
				player.Drop();
			}
			player.StateMachine.State = 11;
			player.DummyAutoAnimate = false;
			player.DummyGravity = false;
			if (player.Inventory.Dashes > 1)
			{
				player.Dashes = 1;
			}
			else
			{
				player.RefillDash();
			}
			player.RefillStamina();
			player.Speed = Vector2.Zero;
			int side = Math.Sign(player.X - base.X);
			if (side == 0)
			{
				side = -1;
			}
			BadelineDummy badeline = new BadelineDummy(Position);
			base.Scene.Add(badeline);
			player.Facing = (Facings)(-side);
			badeline.Sprite.Scale.X = side;
			Vector2 playerFrom = player.Position;
			Vector2 playerTo = Position + new Vector2(side * 4, -3f);
			Vector2 badelineFrom = badeline.Position;
			Vector2 badelineTo = Position + new Vector2(-side * 4, 3f);
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.2f)
			{
				Vector2 target = Vector2.Lerp(playerFrom, playerTo, p);
				if (player.Scene != null)
				{
					player.MoveToX(target.X);
				}
				if (player.Scene != null)
				{
					player.MoveToY(target.Y);
				}
				badeline.Position = Vector2.Lerp(badelineFrom, badelineTo, p);
				yield return null;
			}
			if (finalBoost)
			{
				Vector2 center = new Vector2(Calc.Clamp(player.X - level.Camera.X, 120f, 200f), Calc.Clamp(player.Y - level.Camera.Y, 60f, 120f));
				Add(new Coroutine(level.ZoomTo(center, 1.5f, 0.18f)));
				Engine.TimeRate = 0.5f;
			}
			else
			{
				Audio.Play("event:/char/badeline/booster_throw", Position);
			}
			badeline.Sprite.Play("boost");
			yield return 0.1f;
			if (!player.Dead)
			{
				player.MoveV(5f);
			}
			yield return 0.1f;
			if (endLevel)
			{
				level.TimerStopped = true;
				level.RegisterAreaComplete();
			}
			if (finalBoost && finalCh9Boost)
			{
				base.Scene.Add(new CS10_FinalLaunch(player, this, finalCh9Dialog));
				player.Active = false;
				badeline.Active = false;
				Active = false;
				yield return null;
				player.Active = true;
				badeline.Active = true;
			}
			Add(Alarm.Create(Alarm.AlarmMode.Oneshot, delegate
			{
				if (player.Dashes < player.Inventory.Dashes)
				{
					player.Dashes++;
				}
				base.Scene.Remove(badeline);
				(base.Scene as Level).Displacement.AddBurst(badeline.Position, 0.25f, 8f, 32f, 0.5f);
			}, 0.15f, start: true));
			(base.Scene as Level).Shake();
			holding = null;
			if (!finalBoost)
			{
				player.BadelineBoostLaunch(base.CenterX);
				Vector2 from = Position;
				Vector2 to = nodes[nodeIndex];
				float time = Vector2.Distance(from, to) / 320f;
				time = Math.Min(3f, time);
				stretch.Visible = true;
				stretch.Rotation = (to - from).Angle();
				Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, time, start: true);
				tween.OnUpdate = delegate(Tween t)
				{
					Position = Vector2.Lerp(from, to, t.Eased);
					stretch.Scale.X = 1f + Calc.YoYo(t.Eased) * 2f;
					stretch.Scale.Y = 1f - Calc.YoYo(t.Eased) * 0.75f;
					if (t.Eased < 0.9f && base.Scene.OnInterval(0.03f))
					{
						TrailManager.Add(this, Player.TwoDashesHairColor, 0.5f);
						level.ParticlesFG.Emit(P_Move, 1, base.Center, Vector2.One * 4f);
					}
				};
				tween.OnComplete = delegate
				{
					if (base.X >= (float)level.Bounds.Right)
					{
						RemoveSelf();
					}
					else
					{
						travelling = false;
						stretch.Visible = false;
						sprite.Visible = true;
						Collidable = true;
						Audio.Play("event:/char/badeline/booster_reappear", Position);
					}
				};
				Add(tween);
				relocateSfx.Play("event:/char/badeline/booster_relocate");
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
				level.DirectionalShake(-Vector2.UnitY);
				level.Displacement.AddBurst(base.Center, 0.4f, 8f, 32f, 0.5f);
			}
			else
			{
				if (finalCh9Boost)
				{
					Ch9FinalBoostSfx = Audio.Play("event:/new_content/char/badeline/booster_finalfinal_part2", Position);
				}
				Console.WriteLine("TIME: " + sw.ElapsedMilliseconds);
				Engine.FreezeTimer = 0.1f;
				yield return null;
				if (endLevel)
				{
					level.TimerHidden = true;
				}
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
				level.Flash(Color.White * 0.5f, drawPlayerOver: true);
				level.DirectionalShake(-Vector2.UnitY, 0.6f);
				level.Displacement.AddBurst(base.Center, 0.6f, 8f, 64f, 0.5f);
				level.ResetZoom();
				player.SummitLaunch(base.X);
				Engine.TimeRate = 1f;
				Finish();
			}
		}

		private void Skip()
		{
			travelling = true;
			nodeIndex++;
			Collidable = false;
			Level level = SceneAs<Level>();
			Vector2 from = Position;
			Vector2 to = nodes[nodeIndex];
			float time = Vector2.Distance(from, to) / 320f;
			time = Math.Min(3f, time);
			stretch.Visible = true;
			stretch.Rotation = (to - from).Angle();
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, time, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				Position = Vector2.Lerp(from, to, t.Eased);
				stretch.Scale.X = 1f + Calc.YoYo(t.Eased) * 2f;
				stretch.Scale.Y = 1f - Calc.YoYo(t.Eased) * 0.75f;
				if (t.Eased < 0.9f && base.Scene.OnInterval(0.03f))
				{
					TrailManager.Add(this, Player.TwoDashesHairColor, 0.5f);
					level.ParticlesFG.Emit(P_Move, 1, base.Center, Vector2.One * 4f);
				}
			};
			tween.OnComplete = delegate
			{
				if (base.X >= (float)level.Bounds.Right)
				{
					RemoveSelf();
				}
				else
				{
					travelling = false;
					stretch.Visible = false;
					sprite.Visible = true;
					Collidable = true;
					Audio.Play("event:/char/badeline/booster_reappear", Position);
				}
			};
			Add(tween);
			relocateSfx.Play("event:/char/badeline/booster_relocate");
			level.Displacement.AddBurst(base.Center, 0.4f, 8f, 32f, 0.5f);
		}

		public void Wiggle()
		{
			wiggler.Start();
			(base.Scene as Level).Displacement.AddBurst(Position, 0.3f, 4f, 16f, 0.25f);
			Audio.Play("event:/game/general/crystalheart_pulse", Position);
		}

		public override void Update()
		{
			if (sprite.Visible && base.Scene.OnInterval(0.05f))
			{
				SceneAs<Level>().ParticlesBG.Emit(P_Ambience, 1, base.Center, Vector2.One * 3f);
			}
			if (holding != null)
			{
				holding.Speed = Vector2.Zero;
			}
			if (!travelling)
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null)
				{
					float dist = Calc.ClampedMap((player.Center - Position).Length(), 16f, 64f, 12f, 0f);
					Vector2 dir = (player.Center - Position).SafeNormalize();
					sprite.Position = Calc.Approach(sprite.Position, dir * dist, 32f * Engine.DeltaTime);
					if (canSkip && player.Position.X - base.X >= 100f && nodeIndex + 1 < nodes.Length)
					{
						Skip();
					}
				}
			}
			light.Visible = (bloom.Visible = sprite.Visible || stretch.Visible);
			base.Update();
		}

		private void Finish()
		{
			SceneAs<Level>().Displacement.AddBurst(base.Center, 0.5f, 24f, 96f, 0.4f);
			SceneAs<Level>().Particles.Emit(BadelineOldsite.P_Vanish, 12, base.Center, Vector2.One * 6f);
			SceneAs<Level>().CameraLockMode = Level.CameraLockModes.None;
			SceneAs<Level>().CameraOffset = new Vector2(0f, -16f);
			RemoveSelf();
		}
	}
}
