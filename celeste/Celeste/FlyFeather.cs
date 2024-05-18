using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class FlyFeather : Entity
	{
		public static ParticleType P_Collect;

		public static ParticleType P_Boost;

		public static ParticleType P_Flying;

		public static ParticleType P_Respawn;

		private const float RespawnTime = 3f;

		private Sprite sprite;

		private Image outline;

		private Wiggler wiggler;

		private BloomPoint bloom;

		private VertexLight light;

		private Level level;

		private SineWave sine;

		private bool shielded;

		private bool singleUse;

		private Wiggler shieldRadiusWiggle;

		private Wiggler moveWiggle;

		private Vector2 moveWiggleDir;

		private float respawnTimer;

		public FlyFeather(Vector2 position, bool shielded, bool singleUse)
			: base(position)
		{
			this.shielded = shielded;
			this.singleUse = singleUse;
			base.Collider = new Hitbox(20f, 20f, -10f, -10f);
			Add(new PlayerCollider(OnPlayer));
			Add(sprite = GFX.SpriteBank.Create("flyFeather"));
			Add(wiggler = Wiggler.Create(1f, 4f, delegate(float v)
			{
				sprite.Scale = Vector2.One * (1f + v * 0.2f);
			}));
			Add(bloom = new BloomPoint(0.5f, 20f));
			Add(light = new VertexLight(Color.White, 1f, 16, 48));
			Add(sine = new SineWave(0.6f).Randomize());
			Add(outline = new Image(GFX.Game["objects/flyFeather/outline"]));
			outline.CenterOrigin();
			outline.Visible = false;
			shieldRadiusWiggle = Wiggler.Create(0.5f, 4f);
			Add(shieldRadiusWiggle);
			moveWiggle = Wiggler.Create(0.8f, 2f);
			moveWiggle.StartZero = true;
			Add(moveWiggle);
			UpdateY();
		}

		public FlyFeather(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Bool("shielded"), data.Bool("singleUse"))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			level = SceneAs<Level>();
		}

		public override void Update()
		{
			base.Update();
			if (respawnTimer > 0f)
			{
				respawnTimer -= Engine.DeltaTime;
				if (respawnTimer <= 0f)
				{
					Respawn();
				}
			}
			UpdateY();
			light.Alpha = Calc.Approach(light.Alpha, sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
			bloom.Alpha = light.Alpha * 0.8f;
		}

		public override void Render()
		{
			base.Render();
			if (shielded && sprite.Visible)
			{
				Draw.Circle(Position + sprite.Position, 10f - shieldRadiusWiggle.Value * 2f, Color.White, 3);
			}
		}

		private void Respawn()
		{
			if (!Collidable)
			{
				outline.Visible = false;
				Collidable = true;
				sprite.Visible = true;
				wiggler.Start();
				Audio.Play("event:/game/06_reflection/feather_reappear", Position);
				level.ParticlesFG.Emit(P_Respawn, 16, Position, Vector2.One * 2f);
			}
		}

		private void UpdateY()
		{
			sprite.X = 0f;
			float num3 = (sprite.Y = (bloom.Y = sine.Value * 2f));
			sprite.Position += moveWiggleDir * moveWiggle.Value * -8f;
		}

		private void OnPlayer(Player player)
		{
			Vector2 playerSpeed = player.Speed;
			if (shielded && !player.DashAttacking)
			{
				player.PointBounce(base.Center);
				moveWiggle.Start();
				shieldRadiusWiggle.Start();
				moveWiggleDir = (base.Center - player.Center).SafeNormalize(Vector2.UnitY);
				Audio.Play("event:/game/06_reflection/feather_bubble_bounce", Position);
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
				return;
			}
			bool playerWasFlying = player.StateMachine.State == 19;
			if (player.StartStarFly())
			{
				if (!playerWasFlying)
				{
					Audio.Play(shielded ? "event:/game/06_reflection/feather_bubble_get" : "event:/game/06_reflection/feather_get", Position);
				}
				else
				{
					Audio.Play(shielded ? "event:/game/06_reflection/feather_bubble_renew" : "event:/game/06_reflection/feather_renew", Position);
				}
				Collidable = false;
				Add(new Coroutine(CollectRoutine(player, playerSpeed)));
				if (!singleUse)
				{
					outline.Visible = true;
					respawnTimer = 3f;
				}
			}
		}

		private IEnumerator CollectRoutine(Player player, Vector2 playerSpeed)
		{
			level.Shake();
			sprite.Visible = false;
			yield return 0.05f;
			float angle = ((!(playerSpeed != Vector2.Zero)) ? (Position - player.Center).Angle() : playerSpeed.Angle());
			level.ParticlesFG.Emit(P_Collect, 10, Position, Vector2.One * 6f);
			SlashFx.Burst(Position, angle);
		}
	}
}
