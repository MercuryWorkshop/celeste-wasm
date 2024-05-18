using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class ClutterAbsorbEffect : Entity
	{
		private Level level;

		private List<ClutterCabinet> cabinets = new List<ClutterCabinet>();

		public ClutterAbsorbEffect()
		{
			Position = Vector2.Zero;
			base.Tag = Tags.TransitionUpdate;
			base.Depth = -10001;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			level = SceneAs<Level>();
			foreach (Entity cabinet in level.Tracker.GetEntities<ClutterCabinet>())
			{
				cabinets.Add(cabinet as ClutterCabinet);
			}
		}

		public void FlyClutter(Vector2 position, MTexture texture, bool shake, float delay)
		{
			Image img = new Image(texture);
			img.Position = position - Position;
			img.CenterOrigin();
			Add(img);
			Coroutine routine = new Coroutine(FlyClutterRoutine(img, shake, delay));
			routine.RemoveOnComplete = true;
			Add(routine);
		}

		private IEnumerator FlyClutterRoutine(Image img, bool shake, float delay)
		{
			yield return delay;
			ClutterCabinet cabinet = Calc.Random.Choose(cabinets);
			Vector2 vector = cabinet.Position + new Vector2(8f);
			Vector2 from = img.Position;
			Vector2 to = vector + new Vector2(Calc.Random.Next(16) - 8, Calc.Random.Next(4) - 2);
			Vector2 normal = (to - from).SafeNormalize();
			float dist = (to - from).Length();
			Vector2 perp = new Vector2(0f - normal.Y, normal.X) * (dist / 4f + Calc.Random.NextFloat(40f)) * ((!Calc.Random.Chance(0.5f)) ? 1 : (-1));
			SimpleCurve curve = new SimpleCurve(from, to, (to + from) / 2f + perp);
			if (shake)
			{
				for (float time2 = 0.25f; time2 > 0f; time2 -= Engine.DeltaTime)
				{
					img.X = from.X + (float)Calc.Random.Next(3) - 1f;
					img.Y = from.Y + (float)Calc.Random.Next(3) - 1f;
					yield return null;
				}
			}
			for (float time2 = 0f; time2 < 1f; time2 += Engine.DeltaTime)
			{
				img.Position = curve.GetPoint(Ease.CubeInOut(time2));
				img.Scale = Vector2.One * Ease.CubeInOut(1f - time2 * 0.5f);
				if (time2 > 0.5f && !cabinet.Opened)
				{
					cabinet.Open();
				}
				if (level.OnInterval(0.25f))
				{
					level.ParticlesFG.Emit(ClutterSwitch.P_ClutterFly, img.Position);
				}
				yield return null;
			}
			Remove(img);
		}

		public void CloseCabinets()
		{
			Add(new Coroutine(CloseCabinetsRoutine()));
		}

		private IEnumerator CloseCabinetsRoutine()
		{
			cabinets.Sort((ClutterCabinet a, ClutterCabinet b) => (Math.Abs(a.Y - b.Y) < 24f) ? Math.Sign(a.X - b.X) : Math.Sign(a.Y - b.Y));
			int i = 0;
			foreach (ClutterCabinet cabinet in cabinets)
			{
				cabinet.Close();
				if (i++ % 3 == 0)
				{
					yield return 0.1f;
				}
			}
		}
	}
}
