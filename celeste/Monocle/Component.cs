namespace Monocle
{
	public class Component
	{
		public bool Active;

		public bool Visible;

		public Entity Entity { get; private set; }

		public Scene Scene
		{
			get
			{
				if (Entity == null)
				{
					return null;
				}
				return Entity.Scene;
			}
		}

		public Component(bool active, bool visible)
		{
			Active = active;
			Visible = visible;
		}

		public virtual void Added(Entity entity)
		{
			Entity = entity;
			if (Scene != null)
			{
				Scene.Tracker.ComponentAdded(this);
			}
		}

		public virtual void Removed(Entity entity)
		{
			if (Scene != null)
			{
				Scene.Tracker.ComponentRemoved(this);
			}
			Entity = null;
		}

		public virtual void EntityAdded(Scene scene)
		{
			scene.Tracker.ComponentAdded(this);
		}

		public virtual void EntityRemoved(Scene scene)
		{
			scene.Tracker.ComponentRemoved(this);
		}

		public virtual void SceneEnd(Scene scene)
		{
		}

		public virtual void EntityAwake()
		{
		}

		public virtual void Update()
		{
		}

		public virtual void Render()
		{
		}

		public virtual void DebugRender(Camera camera)
		{
		}

		public virtual void HandleGraphicsReset()
		{
		}

		public virtual void HandleGraphicsCreate()
		{
		}

		public void RemoveSelf()
		{
			if (Entity != null)
			{
				Entity.Remove(this);
			}
		}

		public T SceneAs<T>() where T : Scene
		{
			return Scene as T;
		}

		public T EntityAs<T>() where T : Entity
		{
			return Entity as T;
		}
	}
}
