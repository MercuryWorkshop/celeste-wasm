using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class FlutterBird : Entity
	{
		private static readonly Color[] colors = new Color[4]
		{
			Calc.HexToColor("89fbff"),
			Calc.HexToColor("f0fc6c"),
			Calc.HexToColor("f493ff"),
			Calc.HexToColor("93baff")
		};

		private Sprite sprite;

		private Vector2 start;

		private Coroutine routine;

		private bool flyingAway;

		private SoundSource tweetingSfx;

		private SoundSource flyawaySfx;

		public FlutterBird(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			base.Depth = -9999;
			start = Position;
			Add(sprite = GFX.SpriteBank.Create("flutterbird"));
			sprite.Color = Calc.Random.Choose(colors);
			Add(routine = new Coroutine(IdleRoutine()));
			Add(flyawaySfx = new SoundSource());
			Add(tweetingSfx = new SoundSource());
			tweetingSfx.Play("event:/game/general/birdbaby_tweet_loop");
		}

		public override void Update()
		{
			sprite.Scale.X = Calc.Approach(sprite.Scale.X, Math.Sign(sprite.Scale.X), 4f * Engine.DeltaTime);
			sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 1f, 4f * Engine.DeltaTime);
			base.Update();
		}

		private IEnumerator IdleRoutine()
		{
			while (true)
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				float delay = 0.25f + Calc.Random.NextFloat(1f);
				for (float p2 = 0f; p2 < delay; p2 += Engine.DeltaTime)
				{
					if (player != null && Math.Abs(player.X - base.X) < 48f && player.Y > base.Y - 40f && player.Y < base.Y + 8f)
					{
						FlyAway(Math.Sign(base.X - player.X), Calc.Random.NextFloat(0.2f));
					}
					yield return null;
				}
				Audio.Play("event:/game/general/birdbaby_hop", Position);
				Vector2 target = start + new Vector2(-4f + Calc.Random.NextFloat(8f), 0f);
				sprite.Scale.X = Math.Sign(target.X - Position.X);
				SimpleCurve bezier = new SimpleCurve(Position, target, (Position + target) / 2f - Vector2.UnitY * 14f);
				for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime * 4f)
				{
					Position = bezier.GetPoint(p2);
					yield return null;
				}
				sprite.Scale.X = (float)Math.Sign(sprite.Scale.X) * 1.4f;
				sprite.Scale.Y = 0.6f;
				Position = target;
			}
		}

		private IEnumerator FlyAwayRoutine(int direction, float delay)
		{
			Level level = base.Scene as Level;
			yield return delay;
			sprite.Play("fly");
			sprite.Scale.X = (float)(-direction) * 1.25f;
			sprite.Scale.Y = 1.25f;
			level.ParticlesFG.Emit(Calc.Random.Choose<ParticleType>(ParticleTypes.Dust), Position, -(float)Math.PI / 2f);
			Vector2 from = Position;
			Vector2 to2 = Position + new Vector2(direction * 4, -8f);
			for (float p = 0f; p < 1f; p += Engine.DeltaTime * 3f)
			{
				Position = from + (to2 - from) * Ease.CubeOut(p);
				yield return null;
			}
			base.Depth = -10001;
			sprite.Scale.X = 0f - sprite.Scale.X;
			to2 = new Vector2(direction, -4f) * 8f;
			while (base.Y + 8f > (float)level.Bounds.Top)
			{
				to2 += new Vector2(direction * 64, -128f) * Engine.DeltaTime;
				Position += to2 * Engine.DeltaTime;
				if (base.Scene.OnInterval(0.1f) && base.Y > level.Camera.Top + 32f)
				{
					foreach (Entity bird in base.Scene.Tracker.GetEntities<FlutterBird>())
					{
						if (Math.Abs(base.X - bird.X) < 48f && Math.Abs(base.Y - bird.Y) < 48f && !(bird as FlutterBird).flyingAway)
						{
							(bird as FlutterBird).FlyAway(direction, Calc.Random.NextFloat(0.25f));
						}
					}
				}
				yield return null;
			}
			base.Scene.Remove(this);
		}

		public void FlyAway(int direction, float delay)
		{
			if (!flyingAway)
			{
				tweetingSfx.Stop();
				flyingAway = true;
				flyawaySfx.Play("event:/game/general/birdbaby_flyaway");
				Remove(routine);
				Add(routine = new Coroutine(FlyAwayRoutine(direction, delay)));
			}
		}
	}
}
