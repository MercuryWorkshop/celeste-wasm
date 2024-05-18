using System;
using System.Collections.Generic;

namespace Monocle
{
	public class ChoiceSet<T>
	{
		private struct Choice
		{
			public T Data;

			public int Weight;

			public Choice(T data, int weight)
			{
				Data = data;
				Weight = weight;
			}
		}

		private Dictionary<T, int> choices;

		public int TotalWeight { get; private set; }

		public int this[T choice]
		{
			get
			{
				int weight = 0;
				choices.TryGetValue(choice, out weight);
				return weight;
			}
			set
			{
				Set(choice, value);
			}
		}

		public ChoiceSet()
		{
			choices = new Dictionary<T, int>();
			TotalWeight = 0;
		}

		public void Set(T choice, int weight)
		{
			int oldWeight = 0;
			choices.TryGetValue(choice, out oldWeight);
			TotalWeight -= oldWeight;
			if (weight <= 0)
			{
				if (choices.ContainsKey(choice))
				{
					choices.Remove(choice);
				}
			}
			else
			{
				TotalWeight += weight;
				choices[choice] = weight;
			}
		}

		public void Set(T choice, float chance)
		{
			int oldWeight = 0;
			choices.TryGetValue(choice, out oldWeight);
			TotalWeight -= oldWeight;
			int weight = (int)Math.Round((float)TotalWeight / (1f - chance));
			if (weight <= 0 && chance > 0f)
			{
				weight = 1;
			}
			if (weight <= 0)
			{
				if (choices.ContainsKey(choice))
				{
					choices.Remove(choice);
				}
			}
			else
			{
				TotalWeight += weight;
				choices[choice] = weight;
			}
		}

		public void SetMany(float totalChance, params T[] choices)
		{
			if (choices.Length == 0)
			{
				return;
			}
			_ = totalChance / (float)choices.Length;
			int oldTotalWeight = 0;
			T[] array = choices;
			foreach (T c in array)
			{
				int oldWeight = 0;
				this.choices.TryGetValue(c, out oldWeight);
				oldTotalWeight += oldWeight;
			}
			TotalWeight -= oldTotalWeight;
			int weight = (int)Math.Round((float)TotalWeight / (1f - totalChance) / (float)choices.Length);
			if (weight <= 0 && totalChance > 0f)
			{
				weight = 1;
			}
			if (weight <= 0)
			{
				array = choices;
				foreach (T c3 in array)
				{
					if (this.choices.ContainsKey(c3))
					{
						this.choices.Remove(c3);
					}
				}
			}
			else
			{
				TotalWeight += weight * choices.Length;
				array = choices;
				foreach (T c2 in array)
				{
					this.choices[c2] = weight;
				}
			}
		}

		public T Get(Random random)
		{
			int at = random.Next(TotalWeight);
			foreach (KeyValuePair<T, int> kv in choices)
			{
				if (at < kv.Value)
				{
					return kv.Key;
				}
				at -= kv.Value;
			}
			throw new Exception("Random choice error!");
		}

		public T Get()
		{
			return Get(Calc.Random);
		}
	}
}
