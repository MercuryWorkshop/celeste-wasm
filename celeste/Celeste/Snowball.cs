using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Snowball : Entity
	{
		private const float ResetTime = 0.8f;

		private Sprite sprite;

		private float resetTimer;

		private Level level;

		private SineWave sine;

		private float atY;

		private SoundSource spawnSfx;

		private Collider bounceCollider;

		public Snowball()
		{
			base.Depth = -12500;
			base.Collider = new Hitbox(12f, 9f, -5f, -2f);
			bounceCollider = new Hitbox(16f, 6f, -6f, -8f);
			Add(new PlayerCollider(OnPlayer));
			Add(new PlayerCollider(OnPlayerBounce, bounceCollider));
			Add(sine = new SineWave(0.5f));
			Add(sprite = GFX.SpriteBank.Create("snowball"));
			sprite.Play("spin");
			Add(spawnSfx = new SoundSource());
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			level = SceneAs<Level>();
			ResetPosition();
		}

		private void ResetPosition()
		{
			Player player = level.Tracker.GetEntity<Player>();
			if (player != null && player.Right < (float)(level.Bounds.Right - 64))
			{
				spawnSfx.Play("event:/game/04_cliffside/snowball_spawn");
				Collidable = (Visible = true);
				resetTimer = 0f;
				base.X = level.Camera.Right + 10f;
				float num = (atY = (base.Y = player.CenterY));
				sine.Reset();
				sprite.Play("spin");
			}
			else
			{
				resetTimer = 0.05f;
			}
		}

		private void Destroy()
		{
			Collidable = false;
			sprite.Play("break");
		}

		private void OnPlayer(Player player)
		{
			player.Die(new Vector2(-1f, 0f));
			Destroy();
			Audio.Play("event:/game/04_cliffside/snowball_impact", Position);
		}

		private void OnPlayerBounce(Player player)
		{
			if (!CollideCheck(player))
			{
				Celeste.Freeze(0.1f);
				player.Bounce(base.Top - 2f);
				Destroy();
				Audio.Play("event:/game/general/thing_booped", Position);
			}
		}

		public override void Update()
		{
			base.Update();
			base.X -= 200f * Engine.DeltaTime;
			base.Y = atY + 4f * sine.Value;
			if (base.X < level.Camera.Left - 60f)
			{
				resetTimer += Engine.DeltaTime;
				if (resetTimer >= 0.8f)
				{
					ResetPosition();
				}
			}
		}

		public override void Render()
		{
			sprite.DrawOutline();
			base.Render();
		}
	}
}
