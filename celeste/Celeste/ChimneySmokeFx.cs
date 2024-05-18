using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class ChimneySmokeFx
	{
		public static void Burst(Vector2 position, float direction, int count, ParticleSystem system = null)
		{
			Vector2 range = Calc.AngleToVector(direction - (float)Math.PI / 2f, 2f);
			range.X = Math.Abs(range.X);
			range.Y = Math.Abs(range.Y);
			if (system == null)
			{
				system = (Engine.Scene as Level).ParticlesFG;
			}
			for (int i = 0; i < count; i++)
			{
				system.Emit(Calc.Random.Choose<ParticleType>(ParticleTypes.Chimney), position + Calc.Random.Range(-range, range), direction);
			}
		}
	}
}
