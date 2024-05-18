using System;
using Microsoft.Xna.Framework;

namespace Monocle
{
	public class ParticleSystem : Entity
	{
		private Particle[] particles;

		private int nextSlot;

		public ParticleSystem(int depth, int maxParticles)
		{
			particles = new Particle[maxParticles];
			base.Depth = depth;
		}

		public void Clear()
		{
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Active = false;
			}
		}

		public void ClearRect(Rectangle rect, bool inside)
		{
			for (int i = 0; i < particles.Length; i++)
			{
				Vector2 pos = particles[i].Position;
				if ((pos.X > (float)rect.Left && pos.Y > (float)rect.Top && pos.X < (float)rect.Right && pos.Y < (float)rect.Bottom) == inside)
				{
					particles[i].Active = false;
				}
			}
		}

		public override void Update()
		{
			for (int i = 0; i < particles.Length; i++)
			{
				if (particles[i].Active)
				{
					particles[i].Update();
				}
			}
		}

		public override void Render()
		{
			Particle[] array = particles;
			for (int i = 0; i < array.Length; i++)
			{
				Particle p = array[i];
				if (p.Active)
				{
					p.Render();
				}
			}
		}

		public void Render(float alpha)
		{
			Particle[] array = particles;
			for (int i = 0; i < array.Length; i++)
			{
				Particle p = array[i];
				if (p.Active)
				{
					p.Render(alpha);
				}
			}
		}

		public void Simulate(float duration, float interval, Action<ParticleSystem> emitter)
		{
			float delta = 0.016f;
			for (float time = 0f; time < duration; time += delta)
			{
				if ((int)((time - delta) / interval) < (int)(time / interval))
				{
					emitter(this);
				}
				for (int i = 0; i < particles.Length; i++)
				{
					if (particles[i].Active)
					{
						particles[i].Update(delta);
					}
				}
			}
		}

		public void Add(Particle particle)
		{
			particles[nextSlot] = particle;
			nextSlot = (nextSlot + 1) % particles.Length;
		}

		public void Emit(ParticleType type, Vector2 position)
		{
			type.Create(ref particles[nextSlot], position);
			nextSlot = (nextSlot + 1) % particles.Length;
		}

		public void Emit(ParticleType type, Vector2 position, float direction)
		{
			type.Create(ref particles[nextSlot], position, direction);
			nextSlot = (nextSlot + 1) % particles.Length;
		}

		public void Emit(ParticleType type, Vector2 position, Color color)
		{
			type.Create(ref particles[nextSlot], position, color);
			nextSlot = (nextSlot + 1) % particles.Length;
		}

		public void Emit(ParticleType type, Vector2 position, Color color, float direction)
		{
			type.Create(ref particles[nextSlot], position, color, direction);
			nextSlot = (nextSlot + 1) % particles.Length;
		}

		public void Emit(ParticleType type, int amount, Vector2 position, Vector2 positionRange)
		{
			for (int i = 0; i < amount; i++)
			{
				Emit(type, Calc.Random.Range(position - positionRange, position + positionRange));
			}
		}

		public void Emit(ParticleType type, int amount, Vector2 position, Vector2 positionRange, float direction)
		{
			for (int i = 0; i < amount; i++)
			{
				Emit(type, Calc.Random.Range(position - positionRange, position + positionRange), direction);
			}
		}

		public void Emit(ParticleType type, int amount, Vector2 position, Vector2 positionRange, Color color)
		{
			for (int i = 0; i < amount; i++)
			{
				Emit(type, Calc.Random.Range(position - positionRange, position + positionRange), color);
			}
		}

		public void Emit(ParticleType type, int amount, Vector2 position, Vector2 positionRange, Color color, float direction)
		{
			for (int i = 0; i < amount; i++)
			{
				Emit(type, Calc.Random.Range(position - positionRange, position + positionRange), color, direction);
			}
		}

		public void Emit(ParticleType type, Entity track, int amount, Vector2 position, Vector2 positionRange, float direction)
		{
			for (int i = 0; i < amount; i++)
			{
				type.Create(ref particles[nextSlot], track, Calc.Random.Range(position - positionRange, position + positionRange), direction, type.Color);
				nextSlot = (nextSlot + 1) % particles.Length;
			}
		}
	}
}
