using Microsoft.Xna.Framework;

namespace Monocle
{
	public class Hitbox : Collider
	{
		private float width;

		private float height;

		public override float Width
		{
			get
			{
				return width;
			}
			set
			{
				width = value;
			}
		}

		public override float Height
		{
			get
			{
				return height;
			}
			set
			{
				height = value;
			}
		}

		public override float Left
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

		public override float Top
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

		public override float Right
		{
			get
			{
				return Position.X + Width;
			}
			set
			{
				Position.X = value - Width;
			}
		}

		public override float Bottom
		{
			get
			{
				return Position.Y + Height;
			}
			set
			{
				Position.Y = value - Height;
			}
		}

		public Hitbox(float width, float height, float x = 0f, float y = 0f)
		{
			this.width = width;
			this.height = height;
			Position.X = x;
			Position.Y = y;
		}

		public bool Intersects(Hitbox hitbox)
		{
			if (base.AbsoluteLeft < hitbox.AbsoluteRight && base.AbsoluteRight > hitbox.AbsoluteLeft && base.AbsoluteBottom > hitbox.AbsoluteTop)
			{
				return base.AbsoluteTop < hitbox.AbsoluteBottom;
			}
			return false;
		}

		public bool Intersects(float x, float y, float width, float height)
		{
			if (base.AbsoluteRight > x && base.AbsoluteBottom > y && base.AbsoluteLeft < x + width)
			{
				return base.AbsoluteTop < y + height;
			}
			return false;
		}

		public override Collider Clone()
		{
			return new Hitbox(width, height, Position.X, Position.Y);
		}

		public override void Render(Camera camera, Color color)
		{
			Draw.HollowRect(base.AbsoluteX, base.AbsoluteY, Width, Height, color);
		}

		public void SetFromRectangle(Rectangle rect)
		{
			Position = new Vector2(rect.X, rect.Y);
			Width = rect.Width;
			Height = rect.Height;
		}

		public void Set(float x, float y, float w, float h)
		{
			Position = new Vector2(x, y);
			Width = w;
			Height = h;
		}

		public void GetTopEdge(out Vector2 from, out Vector2 to)
		{
			from.X = base.AbsoluteLeft;
			to.X = base.AbsoluteRight;
			from.Y = (to.Y = base.AbsoluteTop);
		}

		public void GetBottomEdge(out Vector2 from, out Vector2 to)
		{
			from.X = base.AbsoluteLeft;
			to.X = base.AbsoluteRight;
			from.Y = (to.Y = base.AbsoluteBottom);
		}

		public void GetLeftEdge(out Vector2 from, out Vector2 to)
		{
			from.Y = base.AbsoluteTop;
			to.Y = base.AbsoluteBottom;
			from.X = (to.X = base.AbsoluteLeft);
		}

		public void GetRightEdge(out Vector2 from, out Vector2 to)
		{
			from.Y = base.AbsoluteTop;
			to.Y = base.AbsoluteBottom;
			from.X = (to.X = base.AbsoluteRight);
		}

		public override bool Collide(Vector2 point)
		{
			return Monocle.Collide.RectToPoint(base.AbsoluteLeft, base.AbsoluteTop, Width, Height, point);
		}

		public override bool Collide(Rectangle rect)
		{
			if (base.AbsoluteRight > (float)rect.Left && base.AbsoluteBottom > (float)rect.Top && base.AbsoluteLeft < (float)rect.Right)
			{
				return base.AbsoluteTop < (float)rect.Bottom;
			}
			return false;
		}

		public override bool Collide(Vector2 from, Vector2 to)
		{
			return Monocle.Collide.RectToLine(base.AbsoluteLeft, base.AbsoluteTop, Width, Height, from, to);
		}

		public override bool Collide(Hitbox hitbox)
		{
			return Intersects(hitbox);
		}

		public override bool Collide(Grid grid)
		{
			return grid.Collide(base.Bounds);
		}

		public override bool Collide(Circle circle)
		{
			return Monocle.Collide.RectToCircle(base.AbsoluteLeft, base.AbsoluteTop, Width, Height, circle.AbsolutePosition, circle.Radius);
		}

		public override bool Collide(ColliderList list)
		{
			return list.Collide(this);
		}
	}
}
