using Microsoft.Xna.Framework;

namespace Monocle
{
	public class VirtualJoystick : VirtualInput
	{
		public Binding Up;

		public Binding Down;

		public Binding Left;

		public Binding Right;

		public Binding UpAlt;

		public Binding DownAlt;

		public Binding LeftAlt;

		public Binding RightAlt;

		public float Threshold;

		public int GamepadIndex;

		public OverlapBehaviors OverlapBehavior;

		public bool InvertedX;

		public bool InvertedY;

		private Vector2 value;

		private Vector2 previousValue;

		private bool hTurned;

		private bool vTurned;

		public Vector2 Value { get; private set; }

		public Vector2 PreviousValue { get; private set; }

		public VirtualJoystick(Binding up, Binding down, Binding left, Binding right, int gamepadIndex, float threshold, OverlapBehaviors overlapBehavior = OverlapBehaviors.TakeNewer)
		{
			Up = up;
			Down = down;
			Left = left;
			Right = right;
			GamepadIndex = gamepadIndex;
			Threshold = threshold;
			OverlapBehavior = overlapBehavior;
		}

		public VirtualJoystick(Binding up, Binding upAlt, Binding down, Binding downAlt, Binding left, Binding leftAlt, Binding right, Binding rightAlt, int gamepadIndex, float threshold, OverlapBehaviors overlapBehavior = OverlapBehaviors.TakeNewer)
		{
			Up = up;
			Down = down;
			Left = left;
			Right = right;
			UpAlt = upAlt;
			DownAlt = downAlt;
			LeftAlt = leftAlt;
			RightAlt = rightAlt;
			GamepadIndex = gamepadIndex;
			Threshold = threshold;
			OverlapBehavior = overlapBehavior;
		}

		public override void Update()
		{
			previousValue = value;
			if (!MInput.Disabled)
			{
				Vector2 val = value;
				float right = Right.Axis(GamepadIndex, 0f);
				float left = Left.Axis(GamepadIndex, 0f);
				float down = Down.Axis(GamepadIndex, 0f);
				float up = Up.Axis(GamepadIndex, 0f);
				if (right == 0f && RightAlt != null)
				{
					right = RightAlt.Axis(GamepadIndex, 0f);
				}
				if (left == 0f && LeftAlt != null)
				{
					left = LeftAlt.Axis(GamepadIndex, 0f);
				}
				if (down == 0f && DownAlt != null)
				{
					down = DownAlt.Axis(GamepadIndex, 0f);
				}
				if (up == 0f && UpAlt != null)
				{
					up = UpAlt.Axis(GamepadIndex, 0f);
				}
				if (right > left)
				{
					left = 0f;
				}
				else if (left > right)
				{
					right = 0f;
				}
				if (down > up)
				{
					up = 0f;
				}
				else if (up > down)
				{
					down = 0f;
				}
				if (right != 0f && left != 0f)
				{
					switch (OverlapBehavior)
					{
					case OverlapBehaviors.CancelOut:
						val.X = 0f;
						break;
					case OverlapBehaviors.TakeNewer:
						if (!hTurned)
						{
							if (val.X > 0f)
							{
								val.X = 0f - left;
							}
							else if (val.X < 0f)
							{
								val.X = right;
							}
							hTurned = true;
						}
						else if (val.X > 0f)
						{
							val.X = right;
						}
						else if (val.X < 0f)
						{
							val.X = 0f - left;
						}
						break;
					case OverlapBehaviors.TakeOlder:
						if (val.X > 0f)
						{
							val.X = right;
						}
						else if (val.X < 0f)
						{
							val.X = left;
						}
						break;
					}
				}
				else if (right != 0f)
				{
					hTurned = false;
					val.X = right;
				}
				else if (left != 0f)
				{
					hTurned = false;
					val.X = 0f - left;
				}
				else
				{
					hTurned = false;
					val.X = 0f;
				}
				if (down != 0f && up != 0f)
				{
					switch (OverlapBehavior)
					{
					case OverlapBehaviors.CancelOut:
						val.Y = 0f;
						break;
					case OverlapBehaviors.TakeNewer:
						if (!vTurned)
						{
							if (val.Y > 0f)
							{
								val.Y = 0f - up;
							}
							else if (val.Y < 0f)
							{
								val.Y = down;
							}
							vTurned = true;
						}
						else if (val.Y > 0f)
						{
							val.Y = down;
						}
						else if (val.Y < 0f)
						{
							val.Y = 0f - up;
						}
						break;
					case OverlapBehaviors.TakeOlder:
						if (val.Y > 0f)
						{
							val.Y = down;
						}
						else if (val.Y < 0f)
						{
							val.Y = 0f - up;
						}
						break;
					}
				}
				else if (down != 0f)
				{
					vTurned = false;
					val.Y = down;
				}
				else if (up != 0f)
				{
					vTurned = false;
					val.Y = 0f - up;
				}
				else
				{
					vTurned = false;
					val.Y = 0f;
				}
				if (val.Length() < Threshold)
				{
					val = Vector2.Zero;
				}
				value = val;
			}
			Value = new Vector2(InvertedX ? (value.X * -1f) : value.X, InvertedY ? (value.Y * -1f) : value.Y);
			PreviousValue = new Vector2(InvertedX ? (previousValue.X * -1f) : previousValue.X, InvertedY ? (previousValue.Y * -1f) : previousValue.Y);
		}

		public static implicit operator Vector2(VirtualJoystick joystick)
		{
			return joystick.Value;
		}
	}
}
