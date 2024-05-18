using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Booster : Entity
	{
		private const float RespawnTime = 1f;

		public static ParticleType P_Burst;

		public static ParticleType P_BurstRed;

		public static ParticleType P_Appear;

		public static ParticleType P_RedAppear;

		public static readonly Vector2 playerOffset = new Vector2(0f, -2f);

		private Sprite sprite;

		private Entity outline;

		private Wiggler wiggler;

		private BloomPoint bloom;

		private VertexLight light;

		private Coroutine dashRoutine;

		private DashListener dashListener;

		private ParticleType particleType;

		private float respawnTimer;

		private float cannotUseTimer;

		private bool red;

		private SoundSource loopingSfx;

		public bool Ch9HubBooster;

		public bool Ch9HubTransition;

		public bool BoostingPlayer { get; private set; }

		public Booster(Vector2 position, bool red)
			: base(position)
		{
			base.Depth = -8500;
			base.Collider = new Circle(10f, 0f, 2f);
			this.red = red;
			Add(sprite = GFX.SpriteBank.Create(red ? "boosterRed" : "booster"));
			Add(new PlayerCollider(OnPlayer));
			Add(light = new VertexLight(Color.White, 1f, 16, 32));
			Add(bloom = new BloomPoint(0.1f, 16f));
			Add(wiggler = Wiggler.Create(0.5f, 4f, delegate(float f)
			{
				sprite.Scale = Vector2.One * (1f + f * 0.25f);
			}));
			Add(dashRoutine = new Coroutine(removeOnComplete: false));
			Add(dashListener = new DashListener());
			Add(new MirrorReflection());
			Add(loopingSfx = new SoundSource());
			dashListener.OnDash = OnPlayerDashed;
			particleType = (red ? P_BurstRed : P_Burst);
		}

		public Booster(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Bool("red"))
		{
			Ch9HubBooster = data.Bool("ch9_hub_booster");
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Image img = new Image(GFX.Game["objects/booster/outline"]);
			img.CenterOrigin();
			img.Color = Color.White * 0.75f;
			outline = new Entity(Position);
			outline.Depth = 8999;
			outline.Visible = false;
			outline.Add(img);
			outline.Add(new MirrorReflection());
			scene.Add(outline);
		}

		public void Appear()
		{
			Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_reappear" : "event:/game/04_cliffside/greenbooster_reappear", Position);
			sprite.Play("appear");
			wiggler.Start();
			Visible = true;
			AppearParticles();
		}

		private void AppearParticles()
		{
			ParticleSystem p = SceneAs<Level>().ParticlesBG;
			for (int i = 0; i < 360; i += 30)
			{
				p.Emit(red ? P_RedAppear : P_Appear, 1, base.Center, Vector2.One * 2f, (float)i * ((float)Math.PI / 180f));
			}
		}

		private void OnPlayer(Player player)
		{
			if (respawnTimer <= 0f && cannotUseTimer <= 0f && !BoostingPlayer)
			{
				cannotUseTimer = 0.45f;
				if (red)
				{
					player.RedBoost(this);
				}
				else
				{
					player.Boost(this);
				}
				Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_enter" : "event:/game/04_cliffside/greenbooster_enter", Position);
				wiggler.Start();
				sprite.Play("inside");
				sprite.FlipX = player.Facing == Facings.Left;
			}
		}

		public void PlayerBoosted(Player player, Vector2 direction)
		{
			Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_dash" : "event:/game/04_cliffside/greenbooster_dash", Position);
			if (red)
			{
				loopingSfx.Play("event:/game/05_mirror_temple/redbooster_move");
				loopingSfx.DisposeOnTransition = false;
			}
			if (Ch9HubBooster && direction.Y < 0f)
			{
				bool doCh9Boost = true;
				List<LockBlock> locks = base.Scene.Entities.FindAll<LockBlock>();
				if (locks.Count > 0)
				{
					foreach (LockBlock item in locks)
					{
						if (!item.UnlockingRegistered)
						{
							doCh9Boost = false;
							break;
						}
					}
				}
				if (doCh9Boost)
				{
					Ch9HubTransition = true;
					Add(Alarm.Create(Alarm.AlarmMode.Oneshot, delegate
					{
						Add(new SoundSource("event:/new_content/timeline_bubble_to_remembered")
						{
							DisposeOnTransition = false
						});
					}, 2f, start: true));
				}
			}
			BoostingPlayer = true;
			base.Tag = (int)Tags.Persistent | (int)Tags.TransitionUpdate;
			sprite.Play("spin");
			sprite.FlipX = player.Facing == Facings.Left;
			outline.Visible = true;
			wiggler.Start();
			dashRoutine.Replace(BoostRoutine(player, direction));
		}

		private IEnumerator BoostRoutine(Player player, Vector2 dir)
		{
			float angle = (-dir).Angle();
			while ((player.StateMachine.State == 2 || player.StateMachine.State == 5) && BoostingPlayer)
			{
				sprite.RenderPosition = player.Center + playerOffset;
				loopingSfx.Position = sprite.Position;
				if (base.Scene.OnInterval(0.02f))
				{
					(base.Scene as Level).ParticlesBG.Emit(particleType, 2, player.Center - dir * 3f + new Vector2(0f, -2f), new Vector2(3f, 3f), angle);
				}
				yield return null;
			}
			PlayerReleased();
			if (player.StateMachine.State == 4)
			{
				sprite.Visible = false;
			}
			while (SceneAs<Level>().Transitioning)
			{
				yield return null;
			}
			base.Tag = 0;
		}

		public void OnPlayerDashed(Vector2 direction)
		{
			if (BoostingPlayer)
			{
				BoostingPlayer = false;
			}
		}

		public void PlayerReleased()
		{
			Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_end" : "event:/game/04_cliffside/greenbooster_end", sprite.RenderPosition);
			sprite.Play("pop");
			cannotUseTimer = 0f;
			respawnTimer = 1f;
			BoostingPlayer = false;
			wiggler.Stop();
			loopingSfx.Stop();
		}

		public void PlayerDied()
		{
			if (BoostingPlayer)
			{
				PlayerReleased();
				dashRoutine.Active = false;
				base.Tag = 0;
			}
		}

		public void Respawn()
		{
			Audio.Play(red ? "event:/game/05_mirror_temple/redbooster_reappear" : "event:/game/04_cliffside/greenbooster_reappear", Position);
			sprite.Position = Vector2.Zero;
			sprite.Play("loop", restart: true);
			wiggler.Start();
			sprite.Visible = true;
			outline.Visible = false;
			AppearParticles();
		}

		public override void Update()
		{
			base.Update();
			if (cannotUseTimer > 0f)
			{
				cannotUseTimer -= Engine.DeltaTime;
			}
			if (respawnTimer > 0f)
			{
				respawnTimer -= Engine.DeltaTime;
				if (respawnTimer <= 0f)
				{
					Respawn();
				}
			}
			if (!dashRoutine.Active && respawnTimer <= 0f)
			{
				Vector2 target = Vector2.Zero;
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null && CollideCheck(player))
				{
					target = player.Center + playerOffset - Position;
				}
				sprite.Position = Calc.Approach(sprite.Position, target, 80f * Engine.DeltaTime);
			}
			if (sprite.CurrentAnimationID == "inside" && !BoostingPlayer && !CollideCheck<Player>())
			{
				sprite.Play("loop");
			}
		}

		public override void Render()
		{
			Vector2 was = sprite.Position;
			sprite.Position = was.Floor();
			if (sprite.CurrentAnimationID != "pop" && sprite.Visible)
			{
				sprite.DrawOutline();
			}
			base.Render();
			sprite.Position = was;
		}

		public override void Removed(Scene scene)
		{
			if (Ch9HubTransition)
			{
				Level level = scene as Level;
				foreach (Backdrop item in level.Background.GetEach<Backdrop>("bright"))
				{
					item.ForceVisible = false;
					item.FadeAlphaMultiplier = 1f;
				}
				level.Bloom.Base = AreaData.Get(level).BloomBase + 0.25f;
				level.Session.BloomBaseAdd = 0.25f;
			}
			base.Removed(scene);
		}
	}
}
