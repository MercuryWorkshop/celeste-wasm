using System;
using System.Collections.Generic;

namespace Monocle
{
	public class Wiggler : Component
	{
		private static Stack<Wiggler> cache = new Stack<Wiggler>();

		public bool StartZero;

		public bool UseRawDeltaTime;

		private float sineCounter;

		private float increment;

		private float sineAdd;

		private Action<float> onChange;

		private bool removeSelfOnFinish;

		public float Counter { get; private set; }

		public float Value { get; private set; }

		public static Wiggler Create(float duration, float frequency, Action<float> onChange = null, bool start = false, bool removeSelfOnFinish = false)
		{
			Wiggler wiggler = ((cache.Count <= 0) ? new Wiggler() : cache.Pop());
			wiggler.Init(duration, frequency, onChange, start, removeSelfOnFinish);
			return wiggler;
		}

		private Wiggler()
			: base(active: false, visible: false)
		{
		}

		private void Init(float duration, float frequency, Action<float> onChange, bool start, bool removeSelfOnFinish)
		{
			Counter = (sineCounter = 0f);
			UseRawDeltaTime = false;
			increment = 1f / duration;
			sineAdd = (float)Math.PI * 2f * frequency;
			this.onChange = onChange;
			this.removeSelfOnFinish = removeSelfOnFinish;
			if (start)
			{
				Start();
			}
			else
			{
				Active = false;
			}
		}

		public override void Removed(Entity entity)
		{
			base.Removed(entity);
			cache.Push(this);
		}

		public void Start()
		{
			Counter = 1f;
			if (StartZero)
			{
				sineCounter = (float)Math.PI / 2f;
				Value = 0f;
				if (onChange != null)
				{
					onChange(0f);
				}
			}
			else
			{
				sineCounter = 0f;
				Value = 1f;
				if (onChange != null)
				{
					onChange(1f);
				}
			}
			Active = true;
		}

		public void Start(float duration, float frequency)
		{
			increment = 1f / duration;
			sineAdd = (float)Math.PI * 2f * frequency;
			Start();
		}

		public void Stop()
		{
			Active = false;
		}

		public void StopAndClear()
		{
			Stop();
			Value = 0f;
		}

		public override void Update()
		{
			if (UseRawDeltaTime)
			{
				sineCounter += sineAdd * Engine.RawDeltaTime;
				Counter -= increment * Engine.RawDeltaTime;
			}
			else
			{
				sineCounter += sineAdd * Engine.DeltaTime;
				Counter -= increment * Engine.DeltaTime;
			}
			if (Counter <= 0f)
			{
				Counter = 0f;
				Active = false;
				if (removeSelfOnFinish)
				{
					RemoveSelf();
				}
			}
			Value = (float)Math.Cos(sineCounter) * Counter;
			if (onChange != null)
			{
				onChange(Value);
			}
		}
	}
}
