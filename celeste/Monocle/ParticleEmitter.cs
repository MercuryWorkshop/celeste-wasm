using Microsoft.Xna.Framework;

namespace Monocle
{
	public class ParticleEmitter : Component
	{
		public ParticleSystem System;

		public ParticleType Type;

		public Entity Track;

		public float Interval;

		public Vector2 Position;

		public Vector2 Range;

		public int Amount;

		public float? Direction;

		private float timer;

		public ParticleEmitter(ParticleSystem system, ParticleType type, Vector2 position, Vector2 range, int amount, float interval)
			: base(active: true, visible: false)
		{
			System = system;
			Type = type;
			Position = position;
			Range = range;
			Amount = amount;
			Interval = interval;
		}

		public ParticleEmitter(ParticleSystem system, ParticleType type, Vector2 position, Vector2 range, float direction, int amount, float interval)
			: this(system, type, position, range, amount, interval)
		{
			Direction = direction;
		}

		public ParticleEmitter(ParticleSystem system, ParticleType type, Entity track, Vector2 position, Vector2 range, float direction, int amount, float interval)
			: this(system, type, position, range, amount, interval)
		{
			Direction = direction;
			Track = track;
		}

		public void SimulateCycle()
		{
			Simulate(Type.LifeMax);
		}

		public void Simulate(float duration)
		{
			float steps = duration / Interval;
			for (int i = 0; (float)i < steps; i++)
			{
				for (int j = 0; j < Amount; j++)
				{
					Particle particle = default(Particle);
					Vector2 pos = base.Entity.Position + Position + Calc.Random.Range(-Range, Range);
					particle = ((!Direction.HasValue) ? Type.Create(ref particle, pos) : Type.Create(ref particle, pos, Direction.Value));
					particle.Track = Track;
					float simulateFor = duration - Interval * (float)i;
					if (particle.SimulateFor(simulateFor))
					{
						System.Add(particle);
					}
				}
			}
		}

		public void Emit()
		{
			if (Direction.HasValue)
			{
				System.Emit(Type, Amount, base.Entity.Position + Position, Range, Direction.Value);
			}
			else
			{
				System.Emit(Type, Amount, base.Entity.Position + Position, Range);
			}
		}

		public override void Update()
		{
			timer -= Engine.DeltaTime;
			if (timer <= 0f)
			{
				timer = Interval;
				Emit();
			}
		}
	}
}
