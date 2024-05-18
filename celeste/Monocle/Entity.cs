using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Monocle
{
	public class Entity : IEnumerable<Component>, IEnumerable
	{
		public bool Active = true;

		public bool Visible = true;

		public bool Collidable = true;

		public Vector2 Position;

		private int tag;

		private Collider collider;

		internal int depth;

		internal double actualDepth;

		public Scene Scene { get; private set; }

		public ComponentList Components { get; private set; }

		public int Depth
		{
			get
			{
				return depth;
			}
			set
			{
				if (depth != value)
				{
					depth = value;
					if (Scene != null)
					{
						Scene.SetActualDepth(this);
					}
				}
			}
		}

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

		public Collider Collider
		{
			get
			{
				return collider;
			}
			set
			{
				if (value != collider)
				{
					if (collider != null)
					{
						collider.Removed();
					}
					collider = value;
					if (collider != null)
					{
						collider.Added(this);
					}
				}
			}
		}

		public float Width
		{
			get
			{
				if (Collider == null)
				{
					return 0f;
				}
				return Collider.Width;
			}
		}

		public float Height
		{
			get
			{
				if (Collider == null)
				{
					return 0f;
				}
				return Collider.Height;
			}
		}

		public float Left
		{
			get
			{
				if (Collider == null)
				{
					return X;
				}
				return Position.X + Collider.Left;
			}
			set
			{
				if (Collider == null)
				{
					Position.X = value;
				}
				else
				{
					Position.X = value - Collider.Left;
				}
			}
		}

		public float Right
		{
			get
			{
				if (Collider == null)
				{
					return Position.X;
				}
				return Position.X + Collider.Right;
			}
			set
			{
				if (Collider == null)
				{
					Position.X = value;
				}
				else
				{
					Position.X = value - Collider.Right;
				}
			}
		}

		public float Top
		{
			get
			{
				if (Collider == null)
				{
					return Position.Y;
				}
				return Position.Y + Collider.Top;
			}
			set
			{
				if (Collider == null)
				{
					Position.Y = value;
				}
				else
				{
					Position.Y = value - Collider.Top;
				}
			}
		}

		public float Bottom
		{
			get
			{
				if (Collider == null)
				{
					return Position.Y;
				}
				return Position.Y + Collider.Bottom;
			}
			set
			{
				if (Collider == null)
				{
					Position.Y = value;
				}
				else
				{
					Position.Y = value - Collider.Bottom;
				}
			}
		}

		public float CenterX
		{
			get
			{
				if (Collider == null)
				{
					return Position.X;
				}
				return Position.X + Collider.CenterX;
			}
			set
			{
				if (Collider == null)
				{
					Position.X = value;
				}
				else
				{
					Position.X = value - Collider.CenterX;
				}
			}
		}

		public float CenterY
		{
			get
			{
				if (Collider == null)
				{
					return Position.Y;
				}
				return Position.Y + Collider.CenterY;
			}
			set
			{
				if (Collider == null)
				{
					Position.Y = value;
				}
				else
				{
					Position.Y = value - Collider.CenterY;
				}
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

		public int Tag
		{
			get
			{
				return tag;
			}
			set
			{
				if (tag == value)
				{
					return;
				}
				if (Scene != null)
				{
					for (int i = 0; i < BitTag.TotalTags; i++)
					{
						int check = 1 << i;
						bool add = (value & check) != 0;
						if ((Tag & check) != 0 != add)
						{
							if (add)
							{
								Scene.TagLists[i].Add(this);
							}
							else
							{
								Scene.TagLists[i].Remove(this);
							}
						}
					}
				}
				tag = value;
			}
		}

		public Entity(Vector2 position)
		{
			Position = position;
			Components = new ComponentList(this);
		}

		public Entity()
			: this(Vector2.Zero)
		{
		}

		public virtual void SceneBegin(Scene scene)
		{
		}

		public virtual void SceneEnd(Scene scene)
		{
			if (Components == null)
			{
				return;
			}
			foreach (Component component in Components)
			{
				component.SceneEnd(scene);
			}
		}

		public virtual void Awake(Scene scene)
		{
			if (Components == null)
			{
				return;
			}
			foreach (Component component in Components)
			{
				component.EntityAwake();
			}
		}

		public virtual void Added(Scene scene)
		{
			Scene = scene;
			if (Components != null)
			{
				foreach (Component component in Components)
				{
					component.EntityAdded(scene);
				}
			}
			Scene.SetActualDepth(this);
		}

		public virtual void Removed(Scene scene)
		{
			if (Components != null)
			{
				foreach (Component component in Components)
				{
					component.EntityRemoved(scene);
				}
			}
			Scene = null;
		}

		public virtual void Update()
		{
			Components.Update();
		}

		public virtual void Render()
		{
			Components.Render();
		}

		public virtual void DebugRender(Camera camera)
		{
			if (Collider != null)
			{
				Collider.Render(camera, Collidable ? Color.Red : Color.DarkRed);
			}
			Components.DebugRender(camera);
		}

		public virtual void HandleGraphicsReset()
		{
			Components.HandleGraphicsReset();
		}

		public virtual void HandleGraphicsCreate()
		{
			Components.HandleGraphicsCreate();
		}

		public void RemoveSelf()
		{
			if (Scene != null)
			{
				Scene.Entities.Remove(this);
			}
		}

		public bool TagFullCheck(int tag)
		{
			return (this.tag & tag) == tag;
		}

		public bool TagCheck(int tag)
		{
			return (this.tag & tag) != 0;
		}

		public void AddTag(int tag)
		{
			Tag |= tag;
		}

		public void RemoveTag(int tag)
		{
			Tag &= ~tag;
		}

		public bool CollideCheck(Entity other)
		{
			return Collide.Check(this, other);
		}

		public bool CollideCheck(Entity other, Vector2 at)
		{
			return Collide.Check(this, other, at);
		}

		public bool CollideCheck(BitTag tag)
		{
			return Collide.Check(this, Scene[tag]);
		}

		public bool CollideCheck(BitTag tag, Vector2 at)
		{
			return Collide.Check(this, Scene[tag], at);
		}

		public bool CollideCheck<T>() where T : Entity
		{
			return Collide.Check(this, Scene.Tracker.Entities[typeof(T)]);
		}

		public bool CollideCheck<T>(Vector2 at) where T : Entity
		{
			return Collide.Check(this, Scene.Tracker.Entities[typeof(T)], at);
		}

		public bool CollideCheck<T, Exclude>() where T : Entity where Exclude : Entity
		{
			List<Entity> exclude = Scene.Tracker.Entities[typeof(Exclude)];
			foreach (Entity e in Scene.Tracker.Entities[typeof(T)])
			{
				if (!exclude.Contains(e) && Collide.Check(this, e))
				{
					return true;
				}
			}
			return false;
		}

		public bool CollideCheck<T, Exclude>(Vector2 at) where T : Entity where Exclude : Entity
		{
			Vector2 was = Position;
			Position = at;
			bool result = CollideCheck<T, Exclude>();
			Position = was;
			return result;
		}

		public bool CollideCheck<T, Exclude1, Exclude2>() where T : Entity where Exclude1 : Entity where Exclude2 : Entity
		{
			List<Entity> exclude1 = Scene.Tracker.Entities[typeof(Exclude1)];
			List<Entity> exclude2 = Scene.Tracker.Entities[typeof(Exclude2)];
			foreach (Entity e in Scene.Tracker.Entities[typeof(T)])
			{
				if (!exclude1.Contains(e) && !exclude2.Contains(e) && Collide.Check(this, e))
				{
					return true;
				}
			}
			return false;
		}

		public bool CollideCheck<T, Exclude1, Exclude2>(Vector2 at) where T : Entity where Exclude1 : Entity where Exclude2 : Entity
		{
			Vector2 was = Position;
			Position = at;
			bool result = CollideCheck<T, Exclude1, Exclude2>();
			Position = was;
			return result;
		}

		public bool CollideCheckByComponent<T>() where T : Component
		{
			foreach (Component c in Scene.Tracker.Components[typeof(T)])
			{
				if (Collide.Check(this, c.Entity))
				{
					return true;
				}
			}
			return false;
		}

		public bool CollideCheckByComponent<T>(Vector2 at) where T : Component
		{
			Vector2 old = Position;
			Position = at;
			bool result = CollideCheckByComponent<T>();
			Position = old;
			return result;
		}

		public bool CollideCheckOutside(Entity other, Vector2 at)
		{
			if (!Collide.Check(this, other))
			{
				return Collide.Check(this, other, at);
			}
			return false;
		}

		public bool CollideCheckOutside(BitTag tag, Vector2 at)
		{
			foreach (Entity entity in Scene[tag])
			{
				if (!Collide.Check(this, entity) && Collide.Check(this, entity, at))
				{
					return true;
				}
			}
			return false;
		}

		public bool CollideCheckOutside<T>(Vector2 at) where T : Entity
		{
			foreach (Entity entity in Scene.Tracker.Entities[typeof(T)])
			{
				if (!Collide.Check(this, entity) && Collide.Check(this, entity, at))
				{
					return true;
				}
			}
			return false;
		}

		public bool CollideCheckOutsideByComponent<T>(Vector2 at) where T : Component
		{
			foreach (Component component in Scene.Tracker.Components[typeof(T)])
			{
				if (!Collide.Check(this, component.Entity) && Collide.Check(this, component.Entity, at))
				{
					return true;
				}
			}
			return false;
		}

		public Entity CollideFirst(BitTag tag)
		{
			return Collide.First(this, Scene[tag]);
		}

		public Entity CollideFirst(BitTag tag, Vector2 at)
		{
			return Collide.First(this, Scene[tag], at);
		}

		public T CollideFirst<T>() where T : Entity
		{
			return Collide.First(this, Scene.Tracker.Entities[typeof(T)]) as T;
		}

		public T CollideFirst<T>(Vector2 at) where T : Entity
		{
			return Collide.First(this, Scene.Tracker.Entities[typeof(T)], at) as T;
		}

		public T CollideFirstByComponent<T>() where T : Component
		{
			foreach (Component component in Scene.Tracker.Components[typeof(T)])
			{
				if (Collide.Check(this, component.Entity))
				{
					return component as T;
				}
			}
			return null;
		}

		public T CollideFirstByComponent<T>(Vector2 at) where T : Component
		{
			foreach (Component component in Scene.Tracker.Components[typeof(T)])
			{
				if (Collide.Check(this, component.Entity, at))
				{
					return component as T;
				}
			}
			return null;
		}

		public Entity CollideFirstOutside(BitTag tag, Vector2 at)
		{
			foreach (Entity entity in Scene[tag])
			{
				if (!Collide.Check(this, entity) && Collide.Check(this, entity, at))
				{
					return entity;
				}
			}
			return null;
		}

		public T CollideFirstOutside<T>(Vector2 at) where T : Entity
		{
			foreach (Entity entity in Scene.Tracker.Entities[typeof(T)])
			{
				if (!Collide.Check(this, entity) && Collide.Check(this, entity, at))
				{
					return entity as T;
				}
			}
			return null;
		}

		public T CollideFirstOutsideByComponent<T>(Vector2 at) where T : Component
		{
			foreach (Component component in Scene.Tracker.Components[typeof(T)])
			{
				if (!Collide.Check(this, component.Entity) && Collide.Check(this, component.Entity, at))
				{
					return component as T;
				}
			}
			return null;
		}

		public List<Entity> CollideAll(BitTag tag)
		{
			return Collide.All(this, Scene[tag]);
		}

		public List<Entity> CollideAll(BitTag tag, Vector2 at)
		{
			return Collide.All(this, Scene[tag], at);
		}

		public List<Entity> CollideAll<T>() where T : Entity
		{
			return Collide.All(this, Scene.Tracker.Entities[typeof(T)]);
		}

		public List<Entity> CollideAll<T>(Vector2 at) where T : Entity
		{
			return Collide.All(this, Scene.Tracker.Entities[typeof(T)], at);
		}

		public List<Entity> CollideAll<T>(Vector2 at, List<Entity> into) where T : Entity
		{
			into.Clear();
			return Collide.All(this, Scene.Tracker.Entities[typeof(T)], into, at);
		}

		public List<T> CollideAllByComponent<T>() where T : Component
		{
			List<T> list = new List<T>();
			foreach (Component component in Scene.Tracker.Components[typeof(T)])
			{
				if (Collide.Check(this, component.Entity))
				{
					list.Add(component as T);
				}
			}
			return list;
		}

		public List<T> CollideAllByComponent<T>(Vector2 at) where T : Component
		{
			Vector2 old = Position;
			Position = at;
			List<T> result = CollideAllByComponent<T>();
			Position = old;
			return result;
		}

		public bool CollideDo(BitTag tag, Action<Entity> action)
		{
			bool hit = false;
			foreach (Entity other in Scene[tag])
			{
				if (CollideCheck(other))
				{
					action(other);
					hit = true;
				}
			}
			return hit;
		}

		public bool CollideDo(BitTag tag, Action<Entity> action, Vector2 at)
		{
			bool hit = false;
			Vector2 was = Position;
			Position = at;
			foreach (Entity other in Scene[tag])
			{
				if (CollideCheck(other))
				{
					action(other);
					hit = true;
				}
			}
			Position = was;
			return hit;
		}

		public bool CollideDo<T>(Action<T> action) where T : Entity
		{
			bool hit = false;
			foreach (Entity other in Scene.Tracker.Entities[typeof(T)])
			{
				if (CollideCheck(other))
				{
					action(other as T);
					hit = true;
				}
			}
			return hit;
		}

		public bool CollideDo<T>(Action<T> action, Vector2 at) where T : Entity
		{
			bool hit = false;
			Vector2 was = Position;
			Position = at;
			foreach (Entity other in Scene.Tracker.Entities[typeof(T)])
			{
				if (CollideCheck(other))
				{
					action(other as T);
					hit = true;
				}
			}
			Position = was;
			return hit;
		}

		public bool CollideDoByComponent<T>(Action<T> action) where T : Component
		{
			bool hit = false;
			foreach (Component component in Scene.Tracker.Components[typeof(T)])
			{
				if (CollideCheck(component.Entity))
				{
					action(component as T);
					hit = true;
				}
			}
			return hit;
		}

		public bool CollideDoByComponent<T>(Action<T> action, Vector2 at) where T : Component
		{
			bool hit = false;
			Vector2 was = Position;
			Position = at;
			foreach (Component component in Scene.Tracker.Components[typeof(T)])
			{
				if (CollideCheck(component.Entity))
				{
					action(component as T);
					hit = true;
				}
			}
			Position = was;
			return hit;
		}

		public bool CollidePoint(Vector2 point)
		{
			return Collide.CheckPoint(this, point);
		}

		public bool CollidePoint(Vector2 point, Vector2 at)
		{
			return Collide.CheckPoint(this, point, at);
		}

		public bool CollideLine(Vector2 from, Vector2 to)
		{
			return Collide.CheckLine(this, from, to);
		}

		public bool CollideLine(Vector2 from, Vector2 to, Vector2 at)
		{
			return Collide.CheckLine(this, from, to, at);
		}

		public bool CollideRect(Rectangle rect)
		{
			return Collide.CheckRect(this, rect);
		}

		public bool CollideRect(Rectangle rect, Vector2 at)
		{
			return Collide.CheckRect(this, rect, at);
		}

		public void Add(Component component)
		{
			Components.Add(component);
		}

		public void Remove(Component component)
		{
			Components.Remove(component);
		}

		public void Add(params Component[] components)
		{
			Components.Add(components);
		}

		public void Remove(params Component[] components)
		{
			Components.Remove(components);
		}

		public T Get<T>() where T : Component
		{
			return Components.Get<T>();
		}

		public IEnumerator<Component> GetEnumerator()
		{
			return Components.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public Entity Closest(params Entity[] entities)
		{
			Entity closest = entities[0];
			float dist = Vector2.DistanceSquared(Position, closest.Position);
			for (int i = 1; i < entities.Length; i++)
			{
				float current = Vector2.DistanceSquared(Position, entities[i].Position);
				if (current < dist)
				{
					closest = entities[i];
					dist = current;
				}
			}
			return closest;
		}

		public Entity Closest(BitTag tag)
		{
			List<Entity> list = Scene[tag];
			Entity closest = null;
			if (list.Count >= 1)
			{
				closest = list[0];
				float dist = Vector2.DistanceSquared(Position, closest.Position);
				for (int i = 1; i < list.Count; i++)
				{
					float current = Vector2.DistanceSquared(Position, list[i].Position);
					if (current < dist)
					{
						closest = list[i];
						dist = current;
					}
				}
			}
			return closest;
		}

		public T SceneAs<T>() where T : Scene
		{
			return Scene as T;
		}
	}
}
