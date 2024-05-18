using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste
{
	public static class SpawnManager
	{
		public static Dictionary<string, Spawn> SpawnActions = new Dictionary<string, Spawn>(StringComparer.InvariantCultureIgnoreCase);

		public static void Init()
		{
			Type[] types = Assembly.GetCallingAssembly().GetTypes();
			foreach (Type type in types)
			{
				if (type.GetCustomAttribute(typeof(SpawnableAttribute)) == null)
				{
					continue;
				}
				MethodInfo[] methods = type.GetMethods();
				foreach (MethodInfo method in methods)
				{
					SpawnerAttribute attribute = method.GetCustomAttribute(typeof(SpawnerAttribute)) as SpawnerAttribute;
					if (method.IsStatic && attribute != null)
					{
						string name = attribute.Name;
						if (name == null)
						{
							name = type.Name;
						}
						SpawnActions.Add(name, (Spawn)method.CreateDelegate(typeof(Spawn)));
					}
				}
			}
		}
	}
}
