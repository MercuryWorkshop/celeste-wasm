using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class IceBlock : Entity
	{
		public static ParticleType P_Deactivate;

		private LavaRect lava;

		private Solid solid;

		public IceBlock(Vector2 position, float width, float height)
			: base(position)
		{
			base.Collider = new Hitbox(width, height);
			Add(new CoreModeListener(OnChangeMode));
			Add(new PlayerCollider(OnPlayer));
			Add(lava = new LavaRect(width, height, 2));
			lava.UpdateMultiplier = 0f;
			lava.SurfaceColor = Calc.HexToColor("a6fff4");
			lava.EdgeColor = Calc.HexToColor("6cd6eb");
			lava.CenterColor = Calc.HexToColor("4ca8d6");
			lava.SmallWaveAmplitude = 1f;
			lava.BigWaveAmplitude = 1f;
			lava.CurveAmplitude = 1f;
			lava.Spikey = 3f;
			base.Depth = -8500;
		}

		public IceBlock(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height)
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			scene.Add(solid = new Solid(Position + new Vector2(2f, 3f), base.Width - 4f, base.Height - 5f, safe: false));
			Collidable = (solid.Collidable = SceneAs<Level>().CoreMode == Session.CoreModes.Cold);
		}

		private void OnChangeMode(Session.CoreModes mode)
		{
			Collidable = (solid.Collidable = mode == Session.CoreModes.Cold);
			if (Collidable)
			{
				return;
			}
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
		}

		private void OnPlayer(Player player)
		{
			player.Die((player.Center - base.Center).SafeNormalize());
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
