using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Monocle
{
	public class ColliderList : Collider
	{
		public Collider[] colliders { get; private set; }

		public override float Width
		{
			get
			{
				return Right - Left;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public override float Height
		{
			get
			{
				return Bottom - Top;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public override float Left
		{
			get
			{
				float left = colliders[0].Left;
				for (int i = 1; i < colliders.Length; i++)
				{
					if (colliders[i].Left < left)
					{
						left = colliders[i].Left;
					}
				}
				return left;
			}
			set
			{
				float changeX = value - Left;
				Collider[] array = colliders;
				for (int i = 0; i < array.Length; i++)
				{
					_ = array[i];
					Position.X += changeX;
				}
			}
		}

		public override float Right
		{
			get
			{
				float right = colliders[0].Right;
				for (int i = 1; i < colliders.Length; i++)
				{
					if (colliders[i].Right > right)
					{
						right = colliders[i].Right;
					}
				}
				return right;
			}
			set
			{
				float changeX = value - Right;
				Collider[] array = colliders;
				for (int i = 0; i < array.Length; i++)
				{
					_ = array[i];
					Position.X += changeX;
				}
			}
		}

		public override float Top
		{
			get
			{
				float top = colliders[0].Top;
				for (int i = 1; i < colliders.Length; i++)
				{
					if (colliders[i].Top < top)
					{
						top = colliders[i].Top;
					}
				}
				return top;
			}
			set
			{
				float changeY = value - Top;
				Collider[] array = colliders;
				for (int i = 0; i < array.Length; i++)
				{
					_ = array[i];
					Position.Y += changeY;
				}
			}
		}

		public override float Bottom
		{
			get
			{
				float bottom = colliders[0].Bottom;
				for (int i = 1; i < colliders.Length; i++)
				{
					if (colliders[i].Bottom > bottom)
					{
						bottom = colliders[i].Bottom;
					}
				}
				return bottom;
			}
			set
			{
				float changeY = value - Bottom;
				Collider[] array = colliders;
				for (int i = 0; i < array.Length; i++)
				{
					_ = array[i];
					Position.Y += changeY;
				}
			}
		}

		public ColliderList(params Collider[] colliders)
		{
			this.colliders = colliders;
		}

		public void Add(params Collider[] toAdd)
		{
			Collider[] newColliders = new Collider[colliders.Length + toAdd.Length];
			for (int j = 0; j < colliders.Length; j++)
			{
				newColliders[j] = colliders[j];
			}
			for (int i = 0; i < toAdd.Length; i++)
			{
				newColliders[i + colliders.Length] = toAdd[i];
				toAdd[i].Added(base.Entity);
			}
			colliders = newColliders;
		}

		public void Remove(params Collider[] toRemove)
		{
			Collider[] newColliders = new Collider[colliders.Length - toRemove.Length];
			int at = 0;
			Collider[] array = colliders;
			foreach (Collider c in array)
			{
				if (!toRemove.Contains(c))
				{
					newColliders[at] = c;
					at++;
				}
			}
			colliders = newColliders;
		}

		internal override void Added(Entity entity)
		{
			base.Added(entity);
			Collider[] array = colliders;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Added(entity);
			}
		}

		internal override void Removed()
		{
			base.Removed();
			Collider[] array = colliders;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Removed();
			}
		}

		public override Collider Clone()
		{
			Collider[] clones = new Collider[colliders.Length];
			for (int i = 0; i < colliders.Length; i++)
			{
				clones[i] = colliders[i].Clone();
			}
			return new ColliderList(clones);
		}

		public override void Render(Camera camera, Color color)
		{
			Collider[] array = colliders;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Render(camera, color);
			}
		}

		public override bool Collide(Vector2 point)
		{
			Collider[] array = colliders;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Collide(point))
				{
					return true;
				}
			}
			return false;
		}

		public override bool Collide(Rectangle rect)
		{
			Collider[] array = colliders;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Collide(rect))
				{
					return true;
				}
			}
			return false;
		}

		public override bool Collide(Vector2 from, Vector2 to)
		{
			Collider[] array = colliders;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Collide(from, to))
				{
					return true;
				}
			}
			return false;
		}

		public override bool Collide(Hitbox hitbox)
		{
			Collider[] array = colliders;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Collide(hitbox))
				{
					return true;
				}
			}
			return false;
		}

		public override bool Collide(Grid grid)
		{
			Collider[] array = colliders;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Collide(grid))
				{
					return true;
				}
			}
			return false;
		}

		public override bool Collide(Circle circle)
		{
			Collider[] array = colliders;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Collide(circle))
				{
					return true;
				}
			}
			return false;
		}

		public override bool Collide(ColliderList list)
		{
			Collider[] array = colliders;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Collide(list))
				{
					return true;
				}
			}
			return false;
		}
	}
}
