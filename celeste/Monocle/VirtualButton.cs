using Microsoft.Xna.Framework.Input;

namespace Monocle
{
	public class VirtualButton : VirtualInput
	{
		public Binding Binding;

		public float Threshold;

		public float BufferTime;

		public int GamepadIndex;

		public Keys? DebugOverridePressed;

		private float firstRepeatTime;

		private float multiRepeatTime;

		private float bufferCounter;

		private float repeatCounter;

		private bool canRepeat;

		private bool consumed;

		public bool Repeating { get; private set; }

		public bool Check
		{
			get
			{
				if (MInput.Disabled)
				{
					return false;
				}
				return Binding.Check(GamepadIndex, Threshold);
			}
		}

		public bool Pressed
		{
			get
			{
				if (DebugOverridePressed.HasValue && MInput.Keyboard.Check(DebugOverridePressed.Value))
				{
					return true;
				}
				if (MInput.Disabled)
				{
					return false;
				}
				if (consumed)
				{
					return false;
				}
				if (bufferCounter > 0f || Repeating)
				{
					return true;
				}
				return Binding.Pressed(GamepadIndex, Threshold);
			}
		}

		public bool Released
		{
			get
			{
				if (MInput.Disabled)
				{
					return false;
				}
				return Binding.Released(GamepadIndex, Threshold);
			}
		}

		public VirtualButton(Binding binding, int gamepadIndex, float bufferTime, float triggerThreshold)
		{
			Binding = binding;
			GamepadIndex = gamepadIndex;
			BufferTime = bufferTime;
			Threshold = triggerThreshold;
		}

		public VirtualButton()
		{
		}

		public void SetRepeat(float repeatTime)
		{
			SetRepeat(repeatTime, repeatTime);
		}

		public void SetRepeat(float firstRepeatTime, float multiRepeatTime)
		{
			this.firstRepeatTime = firstRepeatTime;
			this.multiRepeatTime = multiRepeatTime;
			canRepeat = this.firstRepeatTime > 0f;
			if (!canRepeat)
			{
				Repeating = false;
			}
		}

		public override void Update()
		{
			consumed = false;
			bufferCounter -= Engine.DeltaTime;
			bool check = false;
			if (Binding.Pressed(GamepadIndex, Threshold))
			{
				bufferCounter = BufferTime;
				check = true;
			}
			else if (Binding.Check(GamepadIndex, Threshold))
			{
				check = true;
			}
			if (!check)
			{
				Repeating = false;
				repeatCounter = 0f;
				bufferCounter = 0f;
			}
			else
			{
				if (!canRepeat)
				{
					return;
				}
				Repeating = false;
				if (repeatCounter == 0f)
				{
					repeatCounter = firstRepeatTime;
					return;
				}
				repeatCounter -= Engine.DeltaTime;
				if (repeatCounter <= 0f)
				{
					Repeating = true;
					repeatCounter = multiRepeatTime;
				}
			}
		}

		public void ConsumeBuffer()
		{
			bufferCounter = 0f;
		}

		public void ConsumePress()
		{
			bufferCounter = 0f;
			consumed = true;
		}

		public static implicit operator bool(VirtualButton button)
		{
			return button.Check;
		}
	}
}
