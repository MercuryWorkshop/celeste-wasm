using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class SeekerEffectsController : Entity
	{
		private float randomAnxietyOffset;

		public bool enabled = true;

		public SeekerEffectsController()
		{
			base.Tag = Tags.Global;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Level obj = scene as Level;
			obj.Session.Audio.Music.Layer(3, 0f);
			obj.Session.Audio.Apply();
		}

		public override void Update()
		{
			base.Update();
			if (enabled)
			{
				if (base.Scene.OnInterval(0.05f))
				{
					randomAnxietyOffset = Calc.Random.Range(-0.2f, 0.2f);
				}
				Vector2 cam = (base.Scene as Level).Camera.Position;
				Player player = base.Scene.Tracker.GetEntity<Player>();
				float targetTimeRate;
				float targetAnxiety;
				if (player != null && !player.Dead)
				{
					float closestAnxietyDistSq = -1f;
					float closestTimeDistSq = -1f;
					foreach (Seeker seeker in base.Scene.Tracker.GetEntities<Seeker>())
					{
						float distSq = Vector2.DistanceSquared(player.Center, seeker.Center);
						if (!seeker.Regenerating)
						{
							closestAnxietyDistSq = ((!(closestAnxietyDistSq < 0f)) ? Math.Min(closestAnxietyDistSq, distSq) : distSq);
						}
						if (seeker.Attacking)
						{
							closestTimeDistSq = ((!(closestTimeDistSq < 0f)) ? Math.Min(closestTimeDistSq, distSq) : distSq);
						}
					}
					targetTimeRate = ((!(closestTimeDistSq >= 0f)) ? 1f : Calc.ClampedMap(closestTimeDistSq, 256f, 4096f, 0.5f));
					Distort.AnxietyOrigin = new Vector2((player.Center.X - cam.X) / 320f, (player.Center.Y - cam.Y) / 180f);
					targetAnxiety = ((!(closestAnxietyDistSq >= 0f)) ? 0f : Calc.ClampedMap(closestAnxietyDistSq, 256f, 16384f, 1f, 0f));
				}
				else
				{
					targetTimeRate = 1f;
					targetAnxiety = 0f;
				}
				Engine.TimeRate = Calc.Approach(Engine.TimeRate, targetTimeRate, 4f * Engine.DeltaTime);
				Distort.GameRate = Calc.Approach(Distort.GameRate, Calc.Map(Engine.TimeRate, 0.5f, 1f), Engine.DeltaTime * 2f);
				Distort.Anxiety = Calc.Approach(Distort.Anxiety, (0.5f + randomAnxietyOffset) * targetAnxiety, 8f * Engine.DeltaTime);
				if (Engine.TimeRate == 1f && Distort.GameRate == 1f && Distort.Anxiety == 0f && base.Scene.Tracker.CountEntities<Seeker>() == 0)
				{
					enabled = false;
				}
			}
			else if (base.Scene.Tracker.CountEntities<Seeker>() > 0)
			{
				enabled = true;
			}
		}
	}
}
