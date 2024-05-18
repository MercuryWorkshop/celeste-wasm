using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class FakeHeart : Entity
	{
		private const float RespawnTime = 3f;

		private Sprite sprite;

		private ParticleType shineParticle;

		public Wiggler ScaleWiggler;

		private Wiggler moveWiggler;

		private Vector2 moveWiggleDir;

		private BloomPoint bloom;

		private VertexLight light;

		private HoldableCollider crystalCollider;

		private float timer;

		private float bounceSfxDelay;

		private float respawnTimer;

		public FakeHeart(Vector2 position)
			: base(position)
		{
			Add(crystalCollider = new HoldableCollider(OnHoldable));
			Add(new MirrorReflection());
		}

		public FakeHeart(EntityData data, Vector2 offset)
			: this(data.Position + offset)
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			AreaMode mode = Calc.Random.Choose(AreaMode.Normal, AreaMode.BSide, AreaMode.CSide);
			Add(sprite = GFX.SpriteBank.Create("heartgem" + (int)mode));
			sprite.Play("spin");
			sprite.OnLoop = delegate(string anim)
			{
				if (Visible && anim == "spin")
				{
					Audio.Play("event:/game/general/crystalheart_pulse", Position);
					ScaleWiggler.Start();
					(base.Scene as Level).Displacement.AddBurst(Position, 0.35f, 8f, 48f, 0.25f);
				}
			};
			base.Collider = new Hitbox(16f, 16f, -8f, -8f);
			Add(new PlayerCollider(OnPlayer));
			Add(ScaleWiggler = Wiggler.Create(0.5f, 4f, delegate(float f)
			{
				sprite.Scale = Vector2.One * (1f + f * 0.25f);
			}));
			Add(bloom = new BloomPoint(0.75f, 16f));
			Color color;
			switch (mode)
			{
			case AreaMode.Normal:
				color = Color.Aqua;
				shineParticle = HeartGem.P_BlueShine;
				break;
			case AreaMode.BSide:
				color = Color.Red;
				shineParticle = HeartGem.P_RedShine;
				break;
			default:
				color = Color.Gold;
				shineParticle = HeartGem.P_GoldShine;
				break;
			}
			color = Color.Lerp(color, Color.White, 0.5f);
			Add(light = new VertexLight(color, 1f, 32, 64));
			moveWiggler = Wiggler.Create(0.8f, 2f);
			moveWiggler.StartZero = true;
			Add(moveWiggler);
		}

		public override void Update()
		{
			bounceSfxDelay -= Engine.DeltaTime;
			timer += Engine.DeltaTime;
			sprite.Position = Vector2.UnitY * (float)Math.Sin(timer * 2f) * 2f + moveWiggleDir * moveWiggler.Value * -8f;
			if (respawnTimer > 0f)
			{
				respawnTimer -= Engine.DeltaTime;
				if (respawnTimer <= 0f)
				{
					Collidable = (Visible = true);
					ScaleWiggler.Start();
				}
			}
			base.Update();
			if (Visible && base.Scene.OnInterval(0.1f))
			{
				SceneAs<Level>().Particles.Emit(shineParticle, 1, base.Center, Vector2.One * 8f);
			}
		}

		public void OnHoldable(Holdable h)
		{
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (Visible && h.Dangerous(crystalCollider))
			{
				Collect(player, h.GetSpeed().Angle());
			}
		}

		public void OnPlayer(Player player)
		{
			if (!Visible || (base.Scene as Level).Frozen)
			{
				return;
			}
			if (player.DashAttacking)
			{
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
				Collect(player, player.Speed.Angle());
				return;
			}
			if (bounceSfxDelay <= 0f)
			{
				Audio.Play("event:/game/general/crystalheart_bounce", Position);
				bounceSfxDelay = 0.1f;
			}
			player.PointBounce(base.Center);
			moveWiggler.Start();
			ScaleWiggler.Start();
			moveWiggleDir = (base.Center - player.Center).SafeNormalize(Vector2.UnitY);
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
		}

		private void Collect(Player player, float angle)
		{
			if (Collidable)
			{
				Collidable = (Visible = false);
				respawnTimer = 3f;
				Celeste.Freeze(0.05f);
				SceneAs<Level>().Shake();
				SlashFx.Burst(Position, angle);
				player?.RefillDash();
			}
		}
	}
}
