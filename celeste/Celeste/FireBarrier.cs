using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class FireBarrier : Entity
	{
		public static ParticleType P_Deactivate;

		private LavaRect Lava;

		private Solid solid;

		private SoundSource idleSfx;

		public FireBarrier(Vector2 position, float width, float height)
			: base(position)
		{
			base.Tag = Tags.TransitionUpdate;
			base.Collider = new Hitbox(width, height);
			Add(new PlayerCollider(OnPlayer));
			Add(new CoreModeListener(OnChangeMode));
			Add(Lava = new LavaRect(width, height, 4));
			Lava.SurfaceColor = RisingLava.Hot[0];
			Lava.EdgeColor = RisingLava.Hot[1];
			Lava.CenterColor = RisingLava.Hot[2];
			Lava.SmallWaveAmplitude = 2f;
			Lava.BigWaveAmplitude = 1f;
			Lava.CurveAmplitude = 1f;
			base.Depth = -8500;
			Add(idleSfx = new SoundSource());
			idleSfx.Position = new Vector2(base.Width, base.Height) / 2f;
		}

		public FireBarrier(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height)
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			scene.Add(solid = new Solid(Position + new Vector2(2f, 3f), base.Width - 4f, base.Height - 5f, safe: false));
			Collidable = (solid.Collidable = SceneAs<Level>().CoreMode == Session.CoreModes.Hot);
			if (Collidable)
			{
				idleSfx.Play("event:/env/local/09_core/lavagate_idle");
			}
		}

		private void OnChangeMode(Session.CoreModes mode)
		{
			Collidable = (solid.Collidable = mode == Session.CoreModes.Hot);
			if (!Collidable)
			{
				Level level = SceneAs<Level>();
				Vector2 center = base.Center;
				for (int x = 0; (float)x < base.Width; x += 4)
				{
					for (int y = 0; (float)y < base.Height; y += 4)
					{
						Vector2 at = Position + new Vector2(x + 2, y + 2) + Calc.Random.Range(-Vector2.One * 2f, Vector2.One * 2f);
						level.Particles.Emit(P_Deactivate, at, (at - center).Angle());
					}
				}
				idleSfx.Stop();
			}
			else
			{
				idleSfx.Play("event:/env/local/09_core/lavagate_idle");
			}
		}

		private void OnPlayer(Player player)
		{
			player.Die((player.Center - base.Center).SafeNormalize());
		}

		public override void Update()
		{
			if ((base.Scene as Level).Transitioning)
			{
				if (idleSfx != null)
				{
					idleSfx.UpdateSfxPosition();
				}
			}
			else
			{
				base.Update();
			}
		}

		public override void Render()
		{
			if (Collidable)
			{
				base.Render();
			}
		}
	}
}
