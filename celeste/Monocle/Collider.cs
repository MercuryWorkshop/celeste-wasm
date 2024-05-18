using System;
using Microsoft.Xna.Framework;

namespace Monocle
{
	public abstract class Collider
	{
		public Vector2 Position;

		public Entity Entity { get; private set; }

		public abstract float Width { get; set; }

		public abstract float Height { get; set; }

		public abstract float Top { get; set; }

		public abstract float Bottom { get; set; }

		public abstract float Left { get; set; }

		public abstract float Right { get; set; }

		public float CenterX
		{
			get
			{
				return Left + Width / 2f;
			}
			set
			{
				Left = value - Width / 2f;
			}
		}

		public float CenterY
		{
			get
			{
				return Top + Height / 2f;
			}
			set
			{
				Top = value - Height / 2f;
			}
		}

		public Vector2 TopLeft
		{
			get
			{
				return new Vector2(Left, Top);
			}
			set
			{
				Left = value.X;
				Top = value.Y;
			}
		}

		public Vector2 TopCenter
		{
			get
			{
				return new Vector2(CenterX, Top);
			}
			set
			{
				CenterX = value.X;
				Top = value.Y;
			}
		}

		public Vector2 TopRight
		{
			get
			{
				return new Vector2(Right, Top);
			}
			set
			{
				Right = value.X;
				Top = value.Y;
			}
		}

		public Vector2 CenterLeft
		{
			get
			{
				return new Vector2(Left, CenterY);
			}
			set
			{
				Left = value.X;
				CenterY = value.Y;
			}
		}

		public Vector2 Center
		{
			get
			{
				return new Vector2(CenterX, CenterY);
			}
			set
			{
				CenterX = value.X;
				CenterY = value.Y;
			}
		}

		public Vector2 Size => new Vector2(Width, Height);

		public Vector2 HalfSize => Size * 0.5f;

		public Vector2 CenterRight
		{
			get
			{
				return new Vector2(Right, CenterY);
			}
			set
			{
				Right = value.X;
				CenterY = value.Y;
			}
		}

		public Vector2 BottomLeft
		{
			get
			{
				return new Vector2(Left, Bottom);
			}
			set
			{
				Left = value.X;
				Bottom = value.Y;
			}
		}

		public Vector2 BottomCenter
		{
			get
			{
				return new Vector2(CenterX, Bottom);
			}
			set
			{
				CenterX = value.X;
				Bottom = value.Y;
			}
		}

		public Vector2 BottomRight
		{
			get
			{
				return new Vector2(Right, Bottom);
			}
			set
			{
				Right = value.X;
				Bottom = value.Y;
			}
		}

		public Vector2 AbsolutePosition
		{
			get
			{
				if (Entity != null)
				{
					return Entity.Position + Position;
				}
				return Position;
			}
		}

		public float AbsoluteX
		{
			get
			{
				if (Entity != null)
				{
					return Entity.Position.X + Position.X;
				}
				return Position.X;
			}
		}

		public float AbsoluteY
		{
			get
			{
				if (Entity != null)
				{
					return Entity.Position.Y + Position.Y;
				}
				return Position.Y;
			}
		}

		public float AbsoluteTop
		{
			get
			{
				if (Entity != null)
				{
					return Top + Entity.Position.Y;
				}
				return Top;
			}
		}

		public float AbsoluteBottom
		{
			get
			{
				if (Entity != null)
				{
					return Bottom + Entity.Position.Y;
				}
				return Bottom;
			}
		}

		public float AbsoluteLeft
		{
			get
			{
				if (Entity != null)
				{
					return Left + Entity.Position.X;
				}
				return Left;
			}
		}

		public float AbsoluteRight
		{
			get
			{
				if (Entity != null)
				{
					return Right + Entity.Position.X;
				}
				return Right;
			}
		}

		public Rectangle Bounds => new Rectangle((int)AbsoluteLeft, (int)AbsoluteTop, (int)Width, (int)Height);

		internal virtual void Added(Entity entity)
		{
			Entity = entity;
		}

		internal virtual void Removed()
		{
			Entity = null;
		}

		public bool Collide(Entity entity)
		{
			return Collide(entity.Collider);
		}

		public bool Collide(Collider collider)
		{
			if (collider is Hitbox)
			{
				return Collide(collider as Hitbox);
			}
			if (collider is Grid)
			{
				return Collide(collider as Grid);
			}
			if (collider is ColliderList)
			{
				return Collide(collider as ColliderList);
			}
			if (collider is Circle)
			{
				return Collide(collider as Circle);
			}
			throw new Exception("Collisions against the collider type are not implemented!");
		}

		public abstract bool Collide(Vector2 point);

		public abstract bool Collide(Rectangle rect);

		public abstract bool Collide(Vector2 from, Vector2 to);

		public abstract bool Collide(Hitbox hitbox);

		public abstract bool Collide(Grid grid);

		public abstract bool Collide(Circle circle);

		public abstract bool Collide(ColliderList list);

		public abstract Collider Clone();

		public abstract void Render(Camera camera, Color color);

		public void CenterOrigin()
		{
			Position.X = (0f - Width) / 2f;
			Position.Y = (0f - Height) / 2f;
		}

		public void Render(Camera camera)
		{
			Render(camera, Color.Red);
		}
	}
}
