using System;
using System.Collections.Generic;

namespace Monocle
{
	public static class Cache
	{
		public static Dictionary<Type, Stack<Entity>> cache;

		private static void Init<T>() where T : Entity, new()
		{
			if (cache == null)
			{
				cache = new Dictionary<Type, Stack<Entity>>();
			}
			if (!cache.ContainsKey(typeof(T)))
			{
				cache.Add(typeof(T), new Stack<Entity>());
			}
		}

		public static void Store<T>(T instance) where T : Entity, new()
		{
			Init<T>();
			cache[typeof(T)].Push(instance);
		}

		public static T Create<T>() where T : Entity, new()
		{
			Init<T>();
			if (cache[typeof(T)].Count > 0)
			{
				return cache[typeof(T)].Pop() as T;
			}
			return new T();
		}

		public static void Clear<T>() where T : Entity, new()
		{
			if (cache != null && cache.ContainsKey(typeof(T)))
			{
				cache[typeof(T)].Clear();
			}
		}

		public static void ClearAll()
		{
			if (cache == null)
			{
				return;
			}
			foreach (KeyValuePair<Type, Stack<Entity>> item in cache)
			{
				item.Value.Clear();
			}
		}
	}
}
