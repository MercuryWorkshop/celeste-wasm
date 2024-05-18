using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace Monocle
{
	public class Tracker
	{
		public static Dictionary<Type, List<Type>> TrackedEntityTypes { get; private set; }

		public static Dictionary<Type, List<Type>> TrackedComponentTypes { get; private set; }

		public static HashSet<Type> StoredEntityTypes { get; private set; }

		public static HashSet<Type> StoredComponentTypes { get; private set; }

		public Dictionary<Type, List<Entity>> Entities { get; private set; }

		public Dictionary<Type, List<Component>> Components { get; private set; }

		public static void Initialize()
		{
			TrackedEntityTypes = new Dictionary<Type, List<Type>>();
			TrackedComponentTypes = new Dictionary<Type, List<Type>>();
			StoredEntityTypes = new HashSet<Type>();
			StoredComponentTypes = new HashSet<Type>();
			Type[] types = Assembly.GetEntryAssembly().GetTypes();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(Tracked), inherit: false);
				if (attrs.Length == 0)
				{
					continue;
				}
				bool inherited = (attrs[0] as Tracked).Inherited;
				if (typeof(Entity).IsAssignableFrom(type))
				{
					if (!type.IsAbstract)
					{
						if (!TrackedEntityTypes.ContainsKey(type))
						{
							TrackedEntityTypes.Add(type, new List<Type>());
						}
						TrackedEntityTypes[type].Add(type);
					}
					StoredEntityTypes.Add(type);
					if (!inherited)
					{
						continue;
					}
					foreach (Type subclass2 in GetSubclasses(type))
					{
						if (!subclass2.IsAbstract)
						{
							if (!TrackedEntityTypes.ContainsKey(subclass2))
							{
								TrackedEntityTypes.Add(subclass2, new List<Type>());
							}
							TrackedEntityTypes[subclass2].Add(type);
						}
					}
					continue;
				}
				if (typeof(Component).IsAssignableFrom(type))
				{
					if (!type.IsAbstract)
					{
						if (!TrackedComponentTypes.ContainsKey(type))
						{
							TrackedComponentTypes.Add(type, new List<Type>());
						}
						TrackedComponentTypes[type].Add(type);
					}
					StoredComponentTypes.Add(type);
					if (!inherited)
					{
						continue;
					}
					foreach (Type subclass in GetSubclasses(type))
					{
						if (!subclass.IsAbstract)
						{
							if (!TrackedComponentTypes.ContainsKey(subclass))
							{
								TrackedComponentTypes.Add(subclass, new List<Type>());
							}
							TrackedComponentTypes[subclass].Add(type);
						}
					}
					continue;
				}
				throw new Exception("Type '" + type.Name + "' cannot be Tracked because it does not derive from Entity or Component");
			}
		}

		private static List<Type> GetSubclasses(Type type)
		{
			List<Type> matches = new List<Type>();
			Type[] types = Assembly.GetEntryAssembly().GetTypes();
			foreach (Type check in types)
			{
				if (type != check && type.IsAssignableFrom(check))
				{
					matches.Add(check);
				}
			}
			return matches;
		}

		public Tracker()
		{
			Entities = new Dictionary<Type, List<Entity>>(TrackedEntityTypes.Count);
			foreach (Type type2 in StoredEntityTypes)
			{
				Entities.Add(type2, new List<Entity>());
			}
			Components = new Dictionary<Type, List<Component>>(TrackedComponentTypes.Count);
			foreach (Type type in StoredComponentTypes)
			{
				Components.Add(type, new List<Component>());
			}
		}

		public bool IsEntityTracked<T>() where T : Entity
		{
			return Entities.ContainsKey(typeof(T));
		}

		public bool IsComponentTracked<T>() where T : Component
		{
			return Components.ContainsKey(typeof(T));
		}

		public T GetEntity<T>() where T : Entity
		{
			List<Entity> list = Entities[typeof(T)];
			if (list.Count == 0)
			{
				return null;
			}
			return list[0] as T;
		}

		public T GetNearestEntity<T>(Vector2 nearestTo) where T : Entity
		{
			List<Entity> entities = GetEntities<T>();
			T nearest = null;
			float nearestDistSq = 0f;
			foreach (T entity in entities)
			{
				float distSq = Vector2.DistanceSquared(nearestTo, entity.Position);
				if (nearest == null || distSq < nearestDistSq)
				{
					nearest = entity;
					nearestDistSq = distSq;
				}
			}
			return nearest;
		}

		public List<Entity> GetEntities<T>() where T : Entity
		{
			return Entities[typeof(T)];
		}

		public List<Entity> GetEntitiesCopy<T>() where T : Entity
		{
			return new List<Entity>(GetEntities<T>());
		}

		public IEnumerator<T> EnumerateEntities<T>() where T : Entity
		{
			foreach (Entity e in Entities[typeof(T)])
			{
				yield return e as T;
			}
		}

		public int CountEntities<T>() where T : Entity
		{
			return Entities[typeof(T)].Count;
		}

		public T GetComponent<T>() where T : Component
		{
			List<Component> list = Components[typeof(T)];
			if (list.Count == 0)
			{
				return null;
			}
			return list[0] as T;
		}

		public T GetNearestComponent<T>(Vector2 nearestTo) where T : Component
		{
			List<Component> components = GetComponents<T>();
			T nearest = null;
			float nearestDistSq = 0f;
			foreach (T component in components)
			{
				float distSq = Vector2.DistanceSquared(nearestTo, component.Entity.Position);
				if (nearest == null || distSq < nearestDistSq)
				{
					nearest = component;
					nearestDistSq = distSq;
				}
			}
			return nearest;
		}

		public List<Component> GetComponents<T>() where T : Component
		{
			return Components[typeof(T)];
		}

		public List<Component> GetComponentsCopy<T>() where T : Component
		{
			return new List<Component>(GetComponents<T>());
		}

		public IEnumerator<T> EnumerateComponents<T>() where T : Component
		{
			foreach (Component c in Components[typeof(T)])
			{
				yield return c as T;
			}
		}

		public int CountComponents<T>() where T : Component
		{
			return Components[typeof(T)].Count;
		}

		internal void EntityAdded(Entity entity)
		{
			Type type = entity.GetType();
			if (!TrackedEntityTypes.TryGetValue(type, out var trackAs))
			{
				return;
			}
			foreach (Type track in trackAs)
			{
				Entities[track].Add(entity);
			}
		}

		internal void EntityRemoved(Entity entity)
		{
			Type type = entity.GetType();
			if (!TrackedEntityTypes.TryGetValue(type, out var trackAs))
			{
				return;
			}
			foreach (Type track in trackAs)
			{
				Entities[track].Remove(entity);
			}
		}

		internal void ComponentAdded(Component component)
		{
			Type type = component.GetType();
			if (!TrackedComponentTypes.TryGetValue(type, out var trackAs))
			{
				return;
			}
			foreach (Type track in trackAs)
			{
				Components[track].Add(component);
			}
		}

		internal void ComponentRemoved(Component component)
		{
			Type type = component.GetType();
			if (!TrackedComponentTypes.TryGetValue(type, out var trackAs))
			{
				return;
			}
			foreach (Type track in trackAs)
			{
				Components[track].Remove(component);
			}
		}

		public void LogEntities()
		{
			foreach (KeyValuePair<Type, List<Entity>> kv in Entities)
			{
				string output = kv.Key.Name + " : " + kv.Value.Count;
				Engine.Commands.Log(output);
			}
		}

		public void LogComponents()
		{
			foreach (KeyValuePair<Type, List<Component>> kv in Components)
			{
				string output = kv.Key.Name + " : " + kv.Value.Count;
				Engine.Commands.Log(output);
			}
		}
	}
}
