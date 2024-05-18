using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class BloomPoint : Component
	{
		public Vector2 Position = Vector2.Zero;

		public float Alpha = 1f;

		public float Radius = 8f;

		public float X
		{
			get
			{
				return Position.X;
			}
			set
			{
				Position.X = value;
			}
		}

		public float Y
		{
			get
			{
				return Position.Y;
			}
			set
			{
				Position.Y = value;
			}
		}

		public BloomPoint(float alpha, float radius)
			: base(active: false, visible: true)
		{
			Alpha = alpha;
			Radius = radius;
		}

		public BloomPoint(Vector2 position, float alpha, float radius)
			: base(active: false, visible: true)
		{
			Position = position;
			Alpha = alpha;
			Radius = radius;
		}
	}
}
