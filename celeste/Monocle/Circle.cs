using Microsoft.Xna.Framework;

namespace Monocle
{
	public class Circle : Collider
	{
		public float Radius;

		public override float Width
		{
			get
			{
				return Radius * 2f;
			}
			set
			{
				Radius = value / 2f;
			}
		}

		public override float Height
		{
			get
			{
				return Radius * 2f;
			}
			set
			{
				Radius = value / 2f;
			}
		}

		public override float Left
		{
			get
			{
				return Position.X - Radius;
			}
			set
			{
				Position.X = value + Radius;
			}
		}

		public override float Top
		{
			get
			{
				return Position.Y - Radius;
			}
			set
			{
				Position.Y = value + Radius;
			}
		}

		public override float Right
		{
			get
			{
				return Position.X + Radius;
			}
			set
			{
				Position.X = value - Radius;
			}
		}

		public override float Bottom
		{
			get
			{
				return Position.Y + Radius;
			}
			set
			{
				Position.Y = value - Radius;
			}
		}

		public Circle(float radius, float x = 0f, float y = 0f)
		{
			Radius = radius;
			Position.X = x;
			Position.Y = y;
		}

		public override Collider Clone()
		{
			return new Circle(Radius, Position.X, Position.Y);
		}

		public override void Render(Camera camera, Color color)
		{
			Draw.Circle(base.AbsolutePosition, Radius, color, 4);
		}

		public override bool Collide(Vector2 point)
		{
			return Monocle.Collide.CircleToPoint(base.AbsolutePosition, Radius, point);
		}

		public override bool Collide(Rectangle rect)
		{
			return Monocle.Collide.RectToCircle(rect, base.AbsolutePosition, Radius);
		}

		public override bool Collide(Vector2 from, Vector2 to)
		{
			return Monocle.Collide.CircleToLine(base.AbsolutePosition, Radius, from, to);
		}

		public override bool Collide(Circle circle)
		{
			return Vector2.DistanceSquared(base.AbsolutePosition, circle.AbsolutePosition) < (Radius + circle.Radius) * (Radius + circle.Radius);
		}

		public override bool Collide(Hitbox hitbox)
		{
			return hitbox.Collide(this);
		}

		public override bool Collide(Grid grid)
		{
			return grid.Collide(this);
		}

		public override bool Collide(ColliderList list)
		{
			return list.Collide(this);
		}
	}
}
