using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class FireBall : Entity
	{
		public static ParticleType P_FireTrail;

		public static ParticleType P_IceTrail;

		public static ParticleType P_IceBreak;

		private const float FireSpeed = 60f;

		private const float IceSpeed = 30f;

		private const float IceSpeedMult = 0.5f;

		private Vector2[] nodes;

		private int amount;

		private int index;

		private float offset;

		private float[] lengths;

		private float speed;

		private float speedMult;

		private float percent;

		private bool iceMode;

		private bool broken;

		private float mult;

		private bool notCoreMode;

		private SoundSource trackSfx;

		private Sprite sprite;

		private Wiggler hitWiggler;

		private Vector2 hitDir;

		public FireBall(Vector2[] nodes, int amount, int index, float offset, float speedMult, bool notCoreMode)
		{
			base.Tag = Tags.TransitionUpdate;
			base.Collider = new Circle(6f);
			this.nodes = nodes;
			this.amount = amount;
			this.index = index;
			this.offset = offset;
			mult = speedMult;
			this.notCoreMode = notCoreMode;
			lengths = new float[nodes.Length];
			for (int i = 1; i < lengths.Length; i++)
			{
				lengths[i] = lengths[i - 1] + Vector2.Distance(nodes[i - 1], nodes[i]);
			}
			speed = 60f / lengths[lengths.Length - 1] * mult;
			if (index == 0)
			{
				percent = 0f;
			}
			else
			{
				percent = (float)index / (float)amount;
			}
			percent += 1f / (float)amount * offset;
			percent %= 1f;
			Position = GetPercentPosition(percent);
			Add(new PlayerCollider(OnPlayer));
			Add(new PlayerCollider(OnBounce, new Hitbox(16f, 6f, -8f, -3f)));
			Add(new CoreModeListener(OnChangeMode));
			Add(sprite = GFX.SpriteBank.Create("fireball"));
			Add(hitWiggler = Wiggler.Create(1.2f, 2f));
			hitWiggler.StartZero = true;
			if (index == 0)
			{
				Add(trackSfx = new SoundSource());
			}
		}

		public FireBall(EntityData data, Vector2 offset)
			: this(data.NodesWithPosition(offset), data.Int("amount", 1), 0, data.Float("offset"), data.Float("speed", 1f), data.Bool("notCoreMode"))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			iceMode = SceneAs<Level>().CoreMode == Session.CoreModes.Cold || notCoreMode;
			speedMult = ((!iceMode) ? 1 : 0);
			sprite.Play(iceMode ? "ice" : "hot", restart: false, randomizeFrame: true);
			if (index == 0)
			{
				for (int i = 1; i < amount; i++)
				{
					base.Scene.Add(new FireBall(nodes, amount, i, offset, mult, notCoreMode));
				}
			}
			if (trackSfx != null && !iceMode)
			{
				PositionTrackSfx();
				trackSfx.Play("event:/env/local/09_core/fireballs_idle");
			}
		}

		public override void Update()
		{
			if ((base.Scene as Level).Transitioning)
			{
				PositionTrackSfx();
				return;
			}
			base.Update();
			speedMult = Calc.Approach(speedMult, iceMode ? 0.5f : 1f, 2f * Engine.DeltaTime);
			percent += speed * speedMult * Engine.DeltaTime;
			if (percent >= 1f)
			{
				percent %= 1f;
				if (broken && nodes[nodes.Length - 1] != nodes[0])
				{
					broken = false;
					Collidable = true;
					sprite.Play(iceMode ? "ice" : "hot", restart: false, randomizeFrame: true);
				}
			}
			Position = GetPercentPosition(percent);
			PositionTrackSfx();
			if (!broken && base.Scene.OnInterval(iceMode ? 0.08f : 0.05f))
			{
				SceneAs<Level>().ParticlesBG.Emit(iceMode ? P_IceTrail : P_FireTrail, 1, base.Center, Vector2.One * 4f);
			}
		}

		public void PositionTrackSfx()
		{
			if (trackSfx == null)
			{
				return;
			}
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player == null)
			{
				return;
			}
			Vector2? closest = null;
			for (int i = 1; i < nodes.Length; i++)
			{
				Vector2 point = Calc.ClosestPointOnLine(nodes[i - 1], nodes[i], player.Center);
				if (!closest.HasValue || (point - player.Center).Length() < (closest.Value - player.Center).Length())
				{
					closest = point;
				}
			}
			if (closest.HasValue)
			{
				trackSfx.Position = closest.Value - Position;
				trackSfx.UpdateSfxPosition();
			}
		}

		public override void Render()
		{
			sprite.Position = hitDir * hitWiggler.Value * 8f;
			if (!broken)
			{
				sprite.DrawOutline(Color.Black);
			}
			base.Render();
		}

		private void OnPlayer(Player player)
		{
			if (!iceMode && !broken)
			{
				KillPlayer(player);
			}
			else if (iceMode && !broken && player.Bottom > base.Y + 4f)
			{
				KillPlayer(player);
			}
		}

		private void KillPlayer(Player player)
		{
			Vector2 dir = (player.Center - base.Center).SafeNormalize();
			if (player.Die(dir) != null)
			{
				hitDir = dir;
				hitWiggler.Start();
			}
		}

		private void OnBounce(Player player)
		{
			if (iceMode && !broken && player.Bottom <= base.Y + 4f && player.Speed.Y >= 0f)
			{
				Audio.Play("event:/game/09_core/iceball_break", Position);
				sprite.Play("shatter");
				broken = true;
				Collidable = false;
				player.Bounce((int)(base.Y - 2f));
				SceneAs<Level>().Particles.Emit(P_IceBreak, 18, base.Center, Vector2.One * 6f);
			}
		}

		private void OnChangeMode(Session.CoreModes mode)
		{
			iceMode = mode == Session.CoreModes.Cold;
			if (!broken)
			{
				sprite.Play(iceMode ? "ice" : "hot", restart: false, randomizeFrame: true);
			}
			if (index == 0 && trackSfx != null)
			{
				if (iceMode)
				{
					trackSfx.Stop();
					return;
				}
				PositionTrackSfx();
				trackSfx.Play("event:/env/local/09_core/fireballs_idle");
			}
		}

		private Vector2 GetPercentPosition(float percent)
		{
			if (percent <= 0f)
			{
				return nodes[0];
			}
			if (percent >= 1f)
			{
				return nodes[nodes.Length - 1];
			}
			float total = lengths[lengths.Length - 1];
			float at = total * percent;
			int i;
			for (i = 0; i < lengths.Length - 1 && !(lengths[i + 1] > at); i++)
			{
			}
			float a = lengths[i] / total;
			float b = lengths[i + 1] / total;
			float t = Calc.ClampedMap(percent, a, b);
			return Vector2.Lerp(nodes[i], nodes[i + 1], t);
		}
	}
}
