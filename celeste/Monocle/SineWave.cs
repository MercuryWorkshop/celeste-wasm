using System;

namespace Monocle
{
	public class SineWave : Component
	{
		public float Frequency = 1f;

		public float Rate = 1f;

		public Action<float> OnUpdate;

		public bool UseRawDeltaTime;

		private float counter;

		public float Value { get; private set; }

		public float ValueOverTwo { get; private set; }

		public float TwoValue { get; private set; }

		public float Counter
		{
			get
			{
				return counter;
			}
			set
			{
				counter = (value + (float)Math.PI * 8f) % ((float)Math.PI * 8f);
				Value = (float)Math.Sin(counter);
				ValueOverTwo = (float)Math.Sin(counter / 2f);
				TwoValue = (float)Math.Sin(counter * 2f);
			}
		}

		public SineWave()
			: base(active: true, visible: false)
		{
		}

		public SineWave(float frequency, float offset = 0f)
			: this()
		{
			Frequency = frequency;
			Counter = offset;
		}

		public override void Update()
		{
			Counter += (float)Math.PI * 2f * Frequency * Rate * (UseRawDeltaTime ? Engine.RawDeltaTime : Engine.DeltaTime);
			if (OnUpdate != null)
			{
				OnUpdate(Value);
			}
		}

		public float ValueOffset(float offset)
		{
			return (float)Math.Sin(counter + offset);
		}

		public SineWave Randomize()
		{
			Counter = Calc.Random.NextFloat() * ((float)Math.PI * 2f) * 2f;
			return this;
		}

		public void Reset()
		{
			Counter = 0f;
		}

		public void StartUp()
		{
			Counter = (float)Math.PI / 2f;
		}

		public void StartDown()
		{
			Counter = 4.712389f;
		}
	}
}
