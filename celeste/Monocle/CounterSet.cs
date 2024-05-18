using System;
using System.Collections.Generic;

namespace Monocle
{
	public class CounterSet<T> : Component
	{
		private Dictionary<T, float> counters;

		private float timer;

		public float this[T index]
		{
			get
			{
				if (counters.TryGetValue(index, out var value))
				{
					return Math.Max(value - timer, 0f);
				}
				return 0f;
			}
			set
			{
				counters[index] = timer + value;
			}
		}

		public CounterSet()
			: base(active: true, visible: false)
		{
			counters = new Dictionary<T, float>();
		}

		public bool Check(T index)
		{
			if (counters.TryGetValue(index, out var value))
			{
				return value - timer > 0f;
			}
			return false;
		}

		public override void Update()
		{
			timer += Engine.DeltaTime;
		}
	}
}
