using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class SeekerBarrier : Solid
	{
		public float Flash;

		public float Solidify;

		public bool Flashing;

		private float solidifyDelay;

		private List<Vector2> particles = new List<Vector2>();

		private List<SeekerBarrier> adjacent = new List<SeekerBarrier>();

		private float[] speeds = new float[3] { 12f, 20f, 40f };

		public SeekerBarrier(Vector2 position, float width, float height)
			: base(position, width, height, safe: false)
		{
			Collidable = false;
			for (int i = 0; (float)i < base.Width * base.Height / 16f; i++)
			{
				particles.Add(new Vector2(Calc.Random.NextFloat(base.Width - 1f), Calc.Random.NextFloat(base.Height - 1f)));
			}
		}

		public SeekerBarrier(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height)
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			scene.Tracker.GetEntity<SeekerBarrierRenderer>().Track(this);
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);
			scene.Tracker.GetEntity<SeekerBarrierRenderer>().Untrack(this);
		}

		public override void Update()
		{
			if (Flashing)
			{
				Flash = Calc.Approach(Flash, 0f, Engine.DeltaTime * 4f);
				if (Flash <= 0f)
				{
					Flashing = false;
				}
			}
			else if (solidifyDelay > 0f)
			{
				solidifyDelay -= Engine.DeltaTime;
			}
			else if (Solidify > 0f)
			{
				Solidify = Calc.Approach(Solidify, 0f, Engine.DeltaTime);
			}
			int spdCount = speeds.Length;
			float height = base.Height;
			int i = 0;
			for (int c = particles.Count; i < c; i++)
			{
				Vector2 p = particles[i] + Vector2.UnitY * speeds[i % spdCount] * Engine.DeltaTime;
				p.Y %= height - 1f;
				particles[i] = p;
			}
			base.Update();
		}

		public void OnReflectSeeker()
		{
			Flash = 1f;
			Solidify = 1f;
			solidifyDelay = 1f;
			Flashing = true;
			base.Scene.CollideInto(new Rectangle((int)base.X, (int)base.Y - 2, (int)base.Width, (int)base.Height + 4), adjacent);
			base.Scene.CollideInto(new Rectangle((int)base.X - 2, (int)base.Y, (int)base.Width + 4, (int)base.Height), adjacent);
			foreach (SeekerBarrier barrier in adjacent)
			{
				if (!barrier.Flashing)
				{
					barrier.OnReflectSeeker();
				}
			}
			adjacent.Clear();
		}

		public override void Render()
		{
			Color col = Color.White * 0.5f;
			foreach (Vector2 p in particles)
			{
				Draw.Pixel.Draw(Position + p, Vector2.Zero, col);
			}
			if (Flashing)
			{
				Draw.Rect(base.Collider, Color.White * Flash * 0.5f);
			}
		}
	}
}
