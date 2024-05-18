namespace Monocle
{
	public class VirtualIntegerAxis : VirtualInput
	{
		public Binding Positive;

		public Binding Negative;

		public Binding PositiveAlt;

		public Binding NegativeAlt;

		public float Threshold;

		public int GamepadIndex;

		public OverlapBehaviors OverlapBehavior;

		public bool Inverted;

		public int Value;

		public int PreviousValue;

		private bool turned;

		public VirtualIntegerAxis()
		{
		}

		public VirtualIntegerAxis(Binding negative, Binding positive, int gamepadIndex, float threshold, OverlapBehaviors overlapBehavior = OverlapBehaviors.TakeNewer)
		{
			Positive = positive;
			Negative = negative;
			Threshold = threshold;
			GamepadIndex = gamepadIndex;
			OverlapBehavior = overlapBehavior;
		}

		public VirtualIntegerAxis(Binding negative, Binding negativeAlt, Binding positive, Binding positiveAlt, int gamepadIndex, float threshold, OverlapBehaviors overlapBehavior = OverlapBehaviors.TakeNewer)
		{
			Positive = positive;
			Negative = negative;
			PositiveAlt = positiveAlt;
			NegativeAlt = negativeAlt;
			Threshold = threshold;
			GamepadIndex = gamepadIndex;
			OverlapBehavior = overlapBehavior;
		}

		public override void Update()
		{
			PreviousValue = Value;
			if (MInput.Disabled)
			{
				return;
			}
			bool pos = Positive.Axis(GamepadIndex, Threshold) > 0f || (PositiveAlt != null && PositiveAlt.Axis(GamepadIndex, Threshold) > 0f);
			bool neg = Negative.Axis(GamepadIndex, Threshold) > 0f || (NegativeAlt != null && NegativeAlt.Axis(GamepadIndex, Threshold) > 0f);
			if (pos && neg)
			{
				switch (OverlapBehavior)
				{
				case OverlapBehaviors.CancelOut:
					Value = 0;
					break;
				case OverlapBehaviors.TakeNewer:
					if (!turned)
					{
						Value *= -1;
						turned = true;
					}
					break;
				case OverlapBehaviors.TakeOlder:
					Value = PreviousValue;
					break;
				}
			}
			else if (pos)
			{
				turned = false;
				Value = 1;
			}
			else if (neg)
			{
				turned = false;
				Value = -1;
			}
			else
			{
				turned = false;
				Value = 0;
			}
			if (Inverted)
			{
				Value = -Value;
			}
		}

		public static implicit operator float(VirtualIntegerAxis axis)
		{
			return axis.Value;
		}
	}
}
