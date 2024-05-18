using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Input;

namespace Monocle
{
	[Serializable]
	public class Binding
	{
		public List<Keys> Keyboard = new List<Keys>();

		public List<Buttons> Controller = new List<Buttons>();

		[XmlIgnore]
		public List<Binding> ExclusiveFrom = new List<Binding>();

		public bool HasInput
		{
			get
			{
				if (Keyboard.Count <= 0)
				{
					return Controller.Count > 0;
				}
				return true;
			}
		}

		public bool Add(params Keys[] keys)
		{
			bool anyAdded = false;
			foreach (Keys key in keys)
			{
				if (Keyboard.Contains(key))
				{
					continue;
				}
				foreach (Binding item in ExclusiveFrom)
				{
					if (!item.Needs(key))
					{
						continue;
					}
					goto IL_0061;
				}
				Keyboard.Add(key);
				anyAdded = true;
				IL_0061:;
			}
			return anyAdded;
		}

		public bool Add(params Buttons[] buttons)
		{
			bool anyAdded = false;
			foreach (Buttons btn in buttons)
			{
				if (Controller.Contains(btn))
				{
					continue;
				}
				foreach (Binding item in ExclusiveFrom)
				{
					if (!item.Needs(btn))
					{
						continue;
					}
					goto IL_0061;
				}
				Controller.Add(btn);
				anyAdded = true;
				IL_0061:;
			}
			return anyAdded;
		}

		public bool Needs(Buttons button)
		{
			if (Controller.Contains(button))
			{
				if (Controller.Count <= 1)
				{
					return true;
				}
				if (!IsExclusive(button))
				{
					return false;
				}
				foreach (Buttons b in Controller)
				{
					if (b != button && IsExclusive(b))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		public bool Needs(Keys key)
		{
			if (Keyboard.Contains(key))
			{
				if (Keyboard.Count <= 1)
				{
					return true;
				}
				if (!IsExclusive(key))
				{
					return false;
				}
				foreach (Keys i in Keyboard)
				{
					if (i != key && IsExclusive(i))
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		public bool IsExclusive(Buttons button)
		{
			foreach (Binding item in ExclusiveFrom)
			{
				if (item.Controller.Contains(button))
				{
					return false;
				}
			}
			return true;
		}

		public bool IsExclusive(Keys key)
		{
			foreach (Binding item in ExclusiveFrom)
			{
				if (item.Keyboard.Contains(key))
				{
					return false;
				}
			}
			return true;
		}

		public bool ClearKeyboard()
		{
			if (ExclusiveFrom.Count > 0)
			{
				if (Keyboard.Count <= 1)
				{
					return false;
				}
				int lastExclusive = 0;
				for (int i = 1; i < Keyboard.Count; i++)
				{
					if (IsExclusive(Keyboard[i]))
					{
						lastExclusive = i;
					}
				}
				Keys keep = Keyboard[lastExclusive];
				Keyboard.Clear();
				Keyboard.Add(keep);
			}
			else
			{
				Keyboard.Clear();
			}
			return true;
		}

		public bool ClearGamepad()
		{
			if (ExclusiveFrom.Count > 0)
			{
				if (Controller.Count <= 1)
				{
					return false;
				}
				int lastExclusive = 0;
				for (int i = 1; i < Controller.Count; i++)
				{
					if (IsExclusive(Controller[i]))
					{
						lastExclusive = i;
					}
				}
				Buttons keep = Controller[lastExclusive];
				Controller.Clear();
				Controller.Add(keep);
			}
			else
			{
				Controller.Clear();
			}
			return true;
		}

		public float Axis(int gamepadIndex, float threshold)
		{
			foreach (Keys i in Keyboard)
			{
				if (MInput.Keyboard.Check(i))
				{
					return 1f;
				}
			}
			foreach (Buttons b in Controller)
			{
				float v = MInput.GamePads[gamepadIndex].Axis(b, threshold);
				if (v != 0f)
				{
					return v;
				}
			}
			return 0f;
		}

		public bool Check(int gamepadIndex, float threshold)
		{
			for (int j = 0; j < Keyboard.Count; j++)
			{
				if (MInput.Keyboard.Check(Keyboard[j]))
				{
					return true;
				}
			}
			for (int i = 0; i < Controller.Count; i++)
			{
				if (MInput.GamePads[gamepadIndex].Check(Controller[i], threshold))
				{
					return true;
				}
			}
			return false;
		}

		public bool Pressed(int gamepadIndex, float threshold)
		{
			for (int j = 0; j < Keyboard.Count; j++)
			{
				if (MInput.Keyboard.Pressed(Keyboard[j]))
				{
					return true;
				}
			}
			for (int i = 0; i < Controller.Count; i++)
			{
				if (MInput.GamePads[gamepadIndex].Pressed(Controller[i], threshold))
				{
					return true;
				}
			}
			return false;
		}

		public bool Released(int gamepadIndex, float threshold)
		{
			for (int j = 0; j < Keyboard.Count; j++)
			{
				if (MInput.Keyboard.Released(Keyboard[j]))
				{
					return true;
				}
			}
			for (int i = 0; i < Controller.Count; i++)
			{
				if (MInput.GamePads[gamepadIndex].Released(Controller[i], threshold))
				{
					return true;
				}
			}
			return false;
		}

		public static void SetExclusive(params Binding[] list)
		{
			Binding[] array = list;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].ExclusiveFrom.Clear();
			}
			array = list;
			foreach (Binding a in array)
			{
				foreach (Binding b in list)
				{
					if (a != b)
					{
						a.ExclusiveFrom.Add(b);
						b.ExclusiveFrom.Add(a);
					}
				}
			}
		}
	}
}
