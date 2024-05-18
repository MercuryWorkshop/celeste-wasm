using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public static class Dust
	{
		public static void Burst(Vector2 position, float direction, int count = 1, ParticleType particleType = null)
		{
			if (particleType == null)
			{
				particleType = ParticleTypes.Dust;
			}
			Vector2 range = Calc.AngleToVector(direction - (float)Math.PI / 2f, 4f);
			range.X = Math.Abs(range.X);
			range.Y = Math.Abs(range.Y);
			Level area = Engine.Scene as Level;
			for (int i = 0; i < count; i++)
			{
				area.Particles.Emit(particleType, position + Calc.Random.Range(-range, range), direction);
			}
		}

		public static void BurstFG(Vector2 position, float direction, int count = 1, float range = 4f, ParticleType particleType = null)
		{
			if (particleType == null)
			{
				particleType = ParticleTypes.Dust;
			}
			Vector2 r = Calc.AngleToVector(direction - (float)Math.PI / 2f, range);
			r.X = Math.Abs(r.X);
			r.Y = Math.Abs(r.Y);
			Level area = Engine.Scene as Level;
			for (int i = 0; i < count; i++)
			{
				area.ParticlesFG.Emit(particleType, position + Calc.Random.Range(-r, r), direction);
			}
		}
	}
}
