using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class EntityData
	{
		public int ID;

		public string Name;

		public LevelData Level;

		public Vector2 Position;

		public Vector2 Origin;

		public int Width;

		public int Height;

		public Vector2[] Nodes;

		public Dictionary<string, object> Values;

		public Vector2[] NodesOffset(Vector2 offset)
		{
			if (Nodes == null)
			{
				return null;
			}
			Vector2[] j = new Vector2[Nodes.Length];
			for (int i = 0; i < Nodes.Length; i++)
			{
				j[i] = Nodes[i] + offset;
			}
			return j;
		}

		public Vector2[] NodesWithPosition(Vector2 offset)
		{
			if (Nodes == null)
			{
				return new Vector2[1] { Position + offset };
			}
			Vector2[] j = new Vector2[Nodes.Length + 1];
			j[0] = Position + offset;
			for (int i = 0; i < Nodes.Length; i++)
			{
				j[i + 1] = Nodes[i] + offset;
			}
			return j;
		}

		public bool Has(string key)
		{
			return Values.ContainsKey(key);
		}

		public string Attr(string key, string defaultValue = "")
		{
			if (Values != null && Values.TryGetValue(key, out var value))
			{
				return value.ToString();
			}
			return defaultValue;
		}

		public float Float(string key, float defaultValue = 0f)
		{
			if (Values != null && Values.TryGetValue(key, out var value))
			{
				if (value is float)
				{
					return (float)value;
				}
				if (float.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
				{
					return result;
				}
			}
			return defaultValue;
		}

		public bool Bool(string key, bool defaultValue = false)
		{
			if (Values != null && Values.TryGetValue(key, out var value))
			{
				if (value is bool)
				{
					return (bool)value;
				}
				if (bool.TryParse(value.ToString(), out var result))
				{
					return result;
				}
			}
			return defaultValue;
		}

		public int Int(string key, int defaultValue = 0)
		{
			if (Values != null && Values.TryGetValue(key, out var value))
			{
				if (value is int)
				{
					return (int)value;
				}
				if (int.TryParse(value.ToString(), out var result))
				{
					return result;
				}
			}
			return defaultValue;
		}

		public char Char(string key, char defaultValue = '\0')
		{
			if (Values != null && Values.TryGetValue(key, out var value) && char.TryParse(value.ToString(), out var result))
			{
				return result;
			}
			return defaultValue;
		}

		public Vector2? FirstNodeNullable(Vector2? offset = null)
		{
			if (Nodes == null || Nodes.Length == 0)
			{
				return null;
			}
			if (offset.HasValue)
			{
				return Nodes[0] + offset.Value;
			}
			return Nodes[0];
		}

		public T Enum<T>(string key, T defaultValue = default(T)) where T : struct
		{
			if (Values != null && Values.TryGetValue(key, out var value) && System.Enum.TryParse<T>(value.ToString(), ignoreCase: true, out var result))
			{
				return result;
			}
			return defaultValue;
		}

		public Color HexColor(string key, Color defaultValue = default(Color))
		{
			if (Values.TryGetValue(key, out var value))
			{
				string str = value.ToString();
				if (str.Length == 6)
				{
					return Calc.HexToColor(str);
				}
			}
			return defaultValue;
		}
	}
}
