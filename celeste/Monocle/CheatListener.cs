using System;
using System.Collections.Generic;

namespace Monocle
{
	public class CheatListener : Entity
	{
		public string CurrentInput;

		public bool Logging;

		private List<Tuple<char, Func<bool>>> inputs;

		private List<Tuple<string, Action>> cheats;

		private int maxInput;

		public CheatListener()
		{
			Visible = false;
			CurrentInput = "";
			inputs = new List<Tuple<char, Func<bool>>>();
			cheats = new List<Tuple<string, Action>>();
		}

		public override void Update()
		{
			bool changed = false;
			foreach (Tuple<char, Func<bool>> input in inputs)
			{
				if (input.Item2())
				{
					CurrentInput += input.Item1;
					changed = true;
				}
			}
			if (!changed)
			{
				return;
			}
			if (CurrentInput.Length > maxInput)
			{
				CurrentInput = CurrentInput.Substring(CurrentInput.Length - maxInput);
			}
			if (Logging)
			{
				Calc.Log(CurrentInput);
			}
			foreach (Tuple<string, Action> cheat in cheats)
			{
				if (CurrentInput.Contains(cheat.Item1))
				{
					CurrentInput = "";
					if (cheat.Item2 != null)
					{
						cheat.Item2();
					}
					cheats.Remove(cheat);
					if (Logging)
					{
						Calc.Log("Cheat Activated: " + cheat.Item1);
					}
					break;
				}
			}
		}

		public void AddCheat(string code, Action onEntered = null)
		{
			cheats.Add(new Tuple<string, Action>(code, onEntered));
			maxInput = Math.Max(code.Length, maxInput);
		}

		public void AddInput(char id, Func<bool> checker)
		{
			inputs.Add(new Tuple<char, Func<bool>>(id, checker));
		}
	}
}
