using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Monocle
{
	public static class MInput
	{
		public class KeyboardData
		{
			public KeyboardState PreviousState;

			public KeyboardState CurrentState;

			internal KeyboardData()
			{
			}

			internal void Update()
			{
				PreviousState = CurrentState;
				CurrentState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
			}

			internal void UpdateNull()
			{
				PreviousState = CurrentState;
				CurrentState = default(KeyboardState);
				CurrentState.GetPressedKeys();
			}

			public bool HasAnyInput()
			{
				return CurrentState.GetPressedKeys().Length != 0;
			}

			public bool Check(Keys key)
			{
				if (Disabled)
				{
					return false;
				}
				if (key != 0)
				{
					return CurrentState.IsKeyDown(key);
				}
				return false;
			}

			public bool Pressed(Keys key)
			{
				if (Disabled)
				{
					return false;
				}
				if (key != 0 && CurrentState.IsKeyDown(key))
				{
					return !PreviousState.IsKeyDown(key);
				}
				return false;
			}

			public bool Released(Keys key)
			{
				if (Disabled)
				{
					return false;
				}
				if (key != 0 && !CurrentState.IsKeyDown(key))
				{
					return PreviousState.IsKeyDown(key);
				}
				return false;
			}

			public bool Check(Keys keyA, Keys keyB)
			{
				if (!Check(keyA))
				{
					return Check(keyB);
				}
				return true;
			}

			public bool Pressed(Keys keyA, Keys keyB)
			{
				if (!Pressed(keyA))
				{
					return Pressed(keyB);
				}
				return true;
			}

			public bool Released(Keys keyA, Keys keyB)
			{
				if (!Released(keyA))
				{
					return Released(keyB);
				}
				return true;
			}

			public bool Check(Keys keyA, Keys keyB, Keys keyC)
			{
				if (!Check(keyA) && !Check(keyB))
				{
					return Check(keyC);
				}
				return true;
			}

			public bool Pressed(Keys keyA, Keys keyB, Keys keyC)
			{
				if (!Pressed(keyA) && !Pressed(keyB))
				{
					return Pressed(keyC);
				}
				return true;
			}

			public bool Released(Keys keyA, Keys keyB, Keys keyC)
			{
				if (!Released(keyA) && !Released(keyB))
				{
					return Released(keyC);
				}
				return true;
			}

			public int AxisCheck(Keys negative, Keys positive)
			{
				if (Check(negative))
				{
					if (Check(positive))
					{
						return 0;
					}
					return -1;
				}
				if (Check(positive))
				{
					return 1;
				}
				return 0;
			}

			public int AxisCheck(Keys negative, Keys positive, int both)
			{
				if (Check(negative))
				{
					if (Check(positive))
					{
						return both;
					}
					return -1;
				}
				if (Check(positive))
				{
					return 1;
				}
				return 0;
			}
		}

		public class MouseData
		{
			public MouseState PreviousState;

			public MouseState CurrentState;

			public bool CheckLeftButton => CurrentState.LeftButton == ButtonState.Pressed;

			public bool CheckRightButton => CurrentState.RightButton == ButtonState.Pressed;

			public bool CheckMiddleButton => CurrentState.MiddleButton == ButtonState.Pressed;

			public bool PressedLeftButton
			{
				get
				{
					if (CurrentState.LeftButton == ButtonState.Pressed)
					{
						return PreviousState.LeftButton == ButtonState.Released;
					}
					return false;
				}
			}

			public bool PressedRightButton
			{
				get
				{
					if (CurrentState.RightButton == ButtonState.Pressed)
					{
						return PreviousState.RightButton == ButtonState.Released;
					}
					return false;
				}
			}

			public bool PressedMiddleButton
			{
				get
				{
					if (CurrentState.MiddleButton == ButtonState.Pressed)
					{
						return PreviousState.MiddleButton == ButtonState.Released;
					}
					return false;
				}
			}

			public bool ReleasedLeftButton
			{
				get
				{
					if (CurrentState.LeftButton == ButtonState.Released)
					{
						return PreviousState.LeftButton == ButtonState.Pressed;
					}
					return false;
				}
			}

			public bool ReleasedRightButton
			{
				get
				{
					if (CurrentState.RightButton == ButtonState.Released)
					{
						return PreviousState.RightButton == ButtonState.Pressed;
					}
					return false;
				}
			}

			public bool ReleasedMiddleButton
			{
				get
				{
					if (CurrentState.MiddleButton == ButtonState.Released)
					{
						return PreviousState.MiddleButton == ButtonState.Pressed;
					}
					return false;
				}
			}

			public int Wheel => CurrentState.ScrollWheelValue;

			public int WheelDelta => CurrentState.ScrollWheelValue - PreviousState.ScrollWheelValue;

			public bool WasMoved
			{
				get
				{
					if (CurrentState.X == PreviousState.X)
					{
						return CurrentState.Y != PreviousState.Y;
					}
					return true;
				}
			}

			public float X
			{
				get
				{
					return Position.X;
				}
				set
				{
					Position = new Vector2(value, Position.Y);
				}
			}

			public float Y
			{
				get
				{
					return Position.Y;
				}
				set
				{
					Position = new Vector2(Position.X, value);
				}
			}

			public Vector2 Position
			{
				get
				{
					return Vector2.Transform(new Vector2(CurrentState.X, CurrentState.Y), Matrix.Invert(Engine.ScreenMatrix));
				}
				set
				{
					Vector2 vector = Vector2.Transform(value, Engine.ScreenMatrix);
					Microsoft.Xna.Framework.Input.Mouse.SetPosition((int)Math.Round(vector.X), (int)Math.Round(vector.Y));
				}
			}

			internal MouseData()
			{
				PreviousState = default(MouseState);
				CurrentState = default(MouseState);
			}

			internal void Update()
			{
				PreviousState = CurrentState;
				CurrentState = Microsoft.Xna.Framework.Input.Mouse.GetState();
			}

			internal void UpdateNull()
			{
				PreviousState = CurrentState;
				CurrentState = default(MouseState);
			}
		}

		public class GamePadData
		{
			public readonly PlayerIndex PlayerIndex;

			public GamePadState PreviousState;

			public GamePadState CurrentState;

			public bool Attached;

			public bool HadInputThisFrame;

			private float rumbleStrength;

			private float rumbleTime;

			public int DPadHorizontal
			{
				get
				{
					if (CurrentState.DPad.Right != ButtonState.Pressed)
					{
						if (CurrentState.DPad.Left != ButtonState.Pressed)
						{
							return 0;
						}
						return -1;
					}
					return 1;
				}
			}

			public int DPadVertical
			{
				get
				{
					if (CurrentState.DPad.Down != ButtonState.Pressed)
					{
						if (CurrentState.DPad.Up != ButtonState.Pressed)
						{
							return 0;
						}
						return -1;
					}
					return 1;
				}
			}

			public Vector2 DPad => new Vector2(DPadHorizontal, DPadVertical);

			public bool DPadLeftCheck => CurrentState.DPad.Left == ButtonState.Pressed;

			public bool DPadLeftPressed
			{
				get
				{
					if (CurrentState.DPad.Left == ButtonState.Pressed)
					{
						return PreviousState.DPad.Left == ButtonState.Released;
					}
					return false;
				}
			}

			public bool DPadLeftReleased
			{
				get
				{
					if (CurrentState.DPad.Left == ButtonState.Released)
					{
						return PreviousState.DPad.Left == ButtonState.Pressed;
					}
					return false;
				}
			}

			public bool DPadRightCheck => CurrentState.DPad.Right == ButtonState.Pressed;

			public bool DPadRightPressed
			{
				get
				{
					if (CurrentState.DPad.Right == ButtonState.Pressed)
					{
						return PreviousState.DPad.Right == ButtonState.Released;
					}
					return false;
				}
			}

			public bool DPadRightReleased
			{
				get
				{
					if (CurrentState.DPad.Right == ButtonState.Released)
					{
						return PreviousState.DPad.Right == ButtonState.Pressed;
					}
					return false;
				}
			}

			public bool DPadUpCheck => CurrentState.DPad.Up == ButtonState.Pressed;

			public bool DPadUpPressed
			{
				get
				{
					if (CurrentState.DPad.Up == ButtonState.Pressed)
					{
						return PreviousState.DPad.Up == ButtonState.Released;
					}
					return false;
				}
			}

			public bool DPadUpReleased
			{
				get
				{
					if (CurrentState.DPad.Up == ButtonState.Released)
					{
						return PreviousState.DPad.Up == ButtonState.Pressed;
					}
					return false;
				}
			}

			public bool DPadDownCheck => CurrentState.DPad.Down == ButtonState.Pressed;

			public bool DPadDownPressed
			{
				get
				{
					if (CurrentState.DPad.Down == ButtonState.Pressed)
					{
						return PreviousState.DPad.Down == ButtonState.Released;
					}
					return false;
				}
			}

			public bool DPadDownReleased
			{
				get
				{
					if (CurrentState.DPad.Down == ButtonState.Released)
					{
						return PreviousState.DPad.Down == ButtonState.Pressed;
					}
					return false;
				}
			}

			internal GamePadData(int playerIndex)
			{
				PlayerIndex = (PlayerIndex)Calc.Clamp(playerIndex, 0, 3);
			}

			public bool HasAnyInput()
			{
				if (!PreviousState.IsConnected && CurrentState.IsConnected)
				{
					return true;
				}
				if (PreviousState.Buttons != CurrentState.Buttons)
				{
					return true;
				}
				if (PreviousState.DPad != CurrentState.DPad)
				{
					return true;
				}
				if (CurrentState.Triggers.Left > 0.01f || CurrentState.Triggers.Right > 0.01f)
				{
					return true;
				}
				if (CurrentState.ThumbSticks.Left.Length() > 0.01f || CurrentState.ThumbSticks.Right.Length() > 0.01f)
				{
					return true;
				}
				return false;
			}

			public void Update()
			{
				PreviousState = CurrentState;
				CurrentState = GamePad.GetState(PlayerIndex);
				if (!Attached && CurrentState.IsConnected)
				{
					IsControllerFocused = true;
				}
				Attached = CurrentState.IsConnected;
				if (rumbleTime > 0f)
				{
					rumbleTime -= Engine.DeltaTime;
					if (rumbleTime <= 0f)
					{
						GamePad.SetVibration(PlayerIndex, 0f, 0f);
					}
				}
			}

			public void UpdateNull()
			{
				PreviousState = CurrentState;
				CurrentState = default(GamePadState);
				Attached = GamePad.GetState(PlayerIndex).IsConnected;
				if (rumbleTime > 0f)
				{
					rumbleTime -= Engine.DeltaTime;
				}
				GamePad.SetVibration(PlayerIndex, 0f, 0f);
			}

			public void Rumble(float strength, float time)
			{
				if (rumbleTime <= 0f || strength > rumbleStrength || (strength == rumbleStrength && time > rumbleTime))
				{
					GamePad.SetVibration(PlayerIndex, strength, strength);
					rumbleStrength = strength;
					rumbleTime = time;
				}
			}

			public void StopRumble()
			{
				GamePad.SetVibration(PlayerIndex, 0f, 0f);
				rumbleTime = 0f;
			}

			public bool Check(Buttons button)
			{
				if (Disabled)
				{
					return false;
				}
				return CurrentState.IsButtonDown(button);
			}

			public bool Pressed(Buttons button)
			{
				if (Disabled)
				{
					return false;
				}
				if (CurrentState.IsButtonDown(button))
				{
					return PreviousState.IsButtonUp(button);
				}
				return false;
			}

			public bool Released(Buttons button)
			{
				if (Disabled)
				{
					return false;
				}
				if (CurrentState.IsButtonUp(button))
				{
					return PreviousState.IsButtonDown(button);
				}
				return false;
			}

			public bool Check(Buttons buttonA, Buttons buttonB)
			{
				if (!Check(buttonA))
				{
					return Check(buttonB);
				}
				return true;
			}

			public bool Pressed(Buttons buttonA, Buttons buttonB)
			{
				if (!Pressed(buttonA))
				{
					return Pressed(buttonB);
				}
				return true;
			}

			public bool Released(Buttons buttonA, Buttons buttonB)
			{
				if (!Released(buttonA))
				{
					return Released(buttonB);
				}
				return true;
			}

			public bool Check(Buttons buttonA, Buttons buttonB, Buttons buttonC)
			{
				if (!Check(buttonA) && !Check(buttonB))
				{
					return Check(buttonC);
				}
				return true;
			}

			public bool Pressed(Buttons buttonA, Buttons buttonB, Buttons buttonC)
			{
				if (!Pressed(buttonA) && !Pressed(buttonB))
				{
					return Check(buttonC);
				}
				return true;
			}

			public bool Released(Buttons buttonA, Buttons buttonB, Buttons buttonC)
			{
				if (!Released(buttonA) && !Released(buttonB))
				{
					return Check(buttonC);
				}
				return true;
			}

			public Vector2 GetLeftStick()
			{
				Vector2 ret = CurrentState.ThumbSticks.Left;
				ret.Y = 0f - ret.Y;
				return ret;
			}

			public Vector2 GetLeftStick(float deadzone)
			{
				Vector2 ret = CurrentState.ThumbSticks.Left;
				if (ret.LengthSquared() < deadzone * deadzone)
				{
					ret = Vector2.Zero;
				}
				else
				{
					ret.Y = 0f - ret.Y;
				}
				return ret;
			}

			public Vector2 GetRightStick()
			{
				Vector2 ret = CurrentState.ThumbSticks.Right;
				ret.Y = 0f - ret.Y;
				return ret;
			}

			public Vector2 GetRightStick(float deadzone)
			{
				Vector2 ret = CurrentState.ThumbSticks.Right;
				if (ret.LengthSquared() < deadzone * deadzone)
				{
					ret = Vector2.Zero;
				}
				else
				{
					ret.Y = 0f - ret.Y;
				}
				return ret;
			}

			public bool LeftStickLeftCheck(float deadzone)
			{
				return CurrentState.ThumbSticks.Left.X <= 0f - deadzone;
			}

			public bool LeftStickLeftPressed(float deadzone)
			{
				if (CurrentState.ThumbSticks.Left.X <= 0f - deadzone)
				{
					return PreviousState.ThumbSticks.Left.X > 0f - deadzone;
				}
				return false;
			}

			public bool LeftStickLeftReleased(float deadzone)
			{
				if (CurrentState.ThumbSticks.Left.X > 0f - deadzone)
				{
					return PreviousState.ThumbSticks.Left.X <= 0f - deadzone;
				}
				return false;
			}

			public bool LeftStickRightCheck(float deadzone)
			{
				return CurrentState.ThumbSticks.Left.X >= deadzone;
			}

			public bool LeftStickRightPressed(float deadzone)
			{
				if (CurrentState.ThumbSticks.Left.X >= deadzone)
				{
					return PreviousState.ThumbSticks.Left.X < deadzone;
				}
				return false;
			}

			public bool LeftStickRightReleased(float deadzone)
			{
				if (CurrentState.ThumbSticks.Left.X < deadzone)
				{
					return PreviousState.ThumbSticks.Left.X >= deadzone;
				}
				return false;
			}

			public bool LeftStickDownCheck(float deadzone)
			{
				return CurrentState.ThumbSticks.Left.Y <= 0f - deadzone;
			}

			public bool LeftStickDownPressed(float deadzone)
			{
				if (CurrentState.ThumbSticks.Left.Y <= 0f - deadzone)
				{
					return PreviousState.ThumbSticks.Left.Y > 0f - deadzone;
				}
				return false;
			}

			public bool LeftStickDownReleased(float deadzone)
			{
				if (CurrentState.ThumbSticks.Left.Y > 0f - deadzone)
				{
					return PreviousState.ThumbSticks.Left.Y <= 0f - deadzone;
				}
				return false;
			}

			public bool LeftStickUpCheck(float deadzone)
			{
				return CurrentState.ThumbSticks.Left.Y >= deadzone;
			}

			public bool LeftStickUpPressed(float deadzone)
			{
				if (CurrentState.ThumbSticks.Left.Y >= deadzone)
				{
					return PreviousState.ThumbSticks.Left.Y < deadzone;
				}
				return false;
			}

			public bool LeftStickUpReleased(float deadzone)
			{
				if (CurrentState.ThumbSticks.Left.Y < deadzone)
				{
					return PreviousState.ThumbSticks.Left.Y >= deadzone;
				}
				return false;
			}

			public float LeftStickHorizontal(float deadzone)
			{
				float h = CurrentState.ThumbSticks.Left.X;
				if (Math.Abs(h) < deadzone)
				{
					return 0f;
				}
				return h;
			}

			public float LeftStickVertical(float deadzone)
			{
				float v = CurrentState.ThumbSticks.Left.Y;
				if (Math.Abs(v) < deadzone)
				{
					return 0f;
				}
				return 0f - v;
			}

			public bool RightStickLeftCheck(float deadzone)
			{
				return CurrentState.ThumbSticks.Right.X <= 0f - deadzone;
			}

			public bool RightStickLeftPressed(float deadzone)
			{
				if (CurrentState.ThumbSticks.Right.X <= 0f - deadzone)
				{
					return PreviousState.ThumbSticks.Right.X > 0f - deadzone;
				}
				return false;
			}

			public bool RightStickLeftReleased(float deadzone)
			{
				if (CurrentState.ThumbSticks.Right.X > 0f - deadzone)
				{
					return PreviousState.ThumbSticks.Right.X <= 0f - deadzone;
				}
				return false;
			}

			public bool RightStickRightCheck(float deadzone)
			{
				return CurrentState.ThumbSticks.Right.X >= deadzone;
			}

			public bool RightStickRightPressed(float deadzone)
			{
				if (CurrentState.ThumbSticks.Right.X >= deadzone)
				{
					return PreviousState.ThumbSticks.Right.X < deadzone;
				}
				return false;
			}

			public bool RightStickRightReleased(float deadzone)
			{
				if (CurrentState.ThumbSticks.Right.X < deadzone)
				{
					return PreviousState.ThumbSticks.Right.X >= deadzone;
				}
				return false;
			}

			public bool RightStickDownCheck(float deadzone)
			{
				return CurrentState.ThumbSticks.Right.Y <= 0f - deadzone;
			}

			public bool RightStickDownPressed(float deadzone)
			{
				if (CurrentState.ThumbSticks.Right.Y <= 0f - deadzone)
				{
					return PreviousState.ThumbSticks.Right.Y > 0f - deadzone;
				}
				return false;
			}

			public bool RightStickDownReleased(float deadzone)
			{
				if (CurrentState.ThumbSticks.Right.Y > 0f - deadzone)
				{
					return PreviousState.ThumbSticks.Right.Y <= 0f - deadzone;
				}
				return false;
			}

			public bool RightStickUpCheck(float deadzone)
			{
				return CurrentState.ThumbSticks.Right.Y >= deadzone;
			}

			public bool RightStickUpPressed(float deadzone)
			{
				if (CurrentState.ThumbSticks.Right.Y >= deadzone)
				{
					return PreviousState.ThumbSticks.Right.Y < deadzone;
				}
				return false;
			}

			public bool RightStickUpReleased(float deadzone)
			{
				if (CurrentState.ThumbSticks.Right.Y < deadzone)
				{
					return PreviousState.ThumbSticks.Right.Y >= deadzone;
				}
				return false;
			}

			public float RightStickHorizontal(float deadzone)
			{
				float h = CurrentState.ThumbSticks.Right.X;
				if (Math.Abs(h) < deadzone)
				{
					return 0f;
				}
				return h;
			}

			public float RightStickVertical(float deadzone)
			{
				float v = CurrentState.ThumbSticks.Right.Y;
				if (Math.Abs(v) < deadzone)
				{
					return 0f;
				}
				return 0f - v;
			}

			public bool LeftTriggerCheck(float threshold)
			{
				if (Disabled)
				{
					return false;
				}
				return CurrentState.Triggers.Left >= threshold;
			}

			public bool LeftTriggerPressed(float threshold)
			{
				if (Disabled)
				{
					return false;
				}
				if (CurrentState.Triggers.Left >= threshold)
				{
					return PreviousState.Triggers.Left < threshold;
				}
				return false;
			}

			public bool LeftTriggerReleased(float threshold)
			{
				if (Disabled)
				{
					return false;
				}
				if (CurrentState.Triggers.Left < threshold)
				{
					return PreviousState.Triggers.Left >= threshold;
				}
				return false;
			}

			public bool RightTriggerCheck(float threshold)
			{
				if (Disabled)
				{
					return false;
				}
				return CurrentState.Triggers.Right >= threshold;
			}

			public bool RightTriggerPressed(float threshold)
			{
				if (Disabled)
				{
					return false;
				}
				if (CurrentState.Triggers.Right >= threshold)
				{
					return PreviousState.Triggers.Right < threshold;
				}
				return false;
			}

			public bool RightTriggerReleased(float threshold)
			{
				if (Disabled)
				{
					return false;
				}
				if (CurrentState.Triggers.Right < threshold)
				{
					return PreviousState.Triggers.Right >= threshold;
				}
				return false;
			}

			public float Axis(Buttons button, float threshold)
			{
				if (Disabled)
				{
					return 0f;
				}
				switch (button)
				{
				case Buttons.DPadUp:
				case Buttons.DPadDown:
				case Buttons.DPadLeft:
				case Buttons.DPadRight:
				case Buttons.Start:
				case Buttons.Back:
				case Buttons.LeftStick:
				case Buttons.RightStick:
				case Buttons.LeftShoulder:
				case Buttons.RightShoulder:
				case Buttons.A:
				case Buttons.B:
				case Buttons.X:
				case Buttons.Y:
					if (Check(button))
					{
						return 1f;
					}
					break;
				case Buttons.RightTrigger:
					if (CurrentState.Triggers.Right >= threshold)
					{
						return CurrentState.Triggers.Right;
					}
					break;
				case Buttons.LeftTrigger:
					if (CurrentState.Triggers.Left >= threshold)
					{
						return CurrentState.Triggers.Left;
					}
					break;
				case Buttons.LeftThumbstickUp:
					if (CurrentState.ThumbSticks.Left.Y >= threshold)
					{
						return CurrentState.ThumbSticks.Left.Y;
					}
					break;
				case Buttons.LeftThumbstickDown:
					if (CurrentState.ThumbSticks.Left.Y <= 0f - threshold)
					{
						return 0f - CurrentState.ThumbSticks.Left.Y;
					}
					break;
				case Buttons.LeftThumbstickRight:
					if (CurrentState.ThumbSticks.Left.X >= threshold)
					{
						return CurrentState.ThumbSticks.Left.X;
					}
					break;
				case Buttons.LeftThumbstickLeft:
					if (CurrentState.ThumbSticks.Left.X <= 0f - threshold)
					{
						return 0f - CurrentState.ThumbSticks.Left.X;
					}
					break;
				case Buttons.RightThumbstickUp:
					if (CurrentState.ThumbSticks.Right.Y >= threshold)
					{
						return CurrentState.ThumbSticks.Right.Y;
					}
					break;
				case Buttons.RightThumbstickDown:
					if (CurrentState.ThumbSticks.Right.Y <= 0f - threshold)
					{
						return 0f - CurrentState.ThumbSticks.Right.Y;
					}
					break;
				case Buttons.RightThumbstickRight:
					if (CurrentState.ThumbSticks.Right.X >= threshold)
					{
						return CurrentState.ThumbSticks.Right.X;
					}
					break;
				case Buttons.RightThumbstickLeft:
					if (CurrentState.ThumbSticks.Right.X <= 0f - threshold)
					{
						return 0f - CurrentState.ThumbSticks.Right.X;
					}
					break;
				}
				return 0f;
			}

			public bool Check(Buttons button, float threshold)
			{
				if (Disabled)
				{
					return false;
				}
				switch (button)
				{
				case Buttons.DPadUp:
				case Buttons.DPadDown:
				case Buttons.DPadLeft:
				case Buttons.DPadRight:
				case Buttons.Start:
				case Buttons.Back:
				case Buttons.LeftStick:
				case Buttons.RightStick:
				case Buttons.LeftShoulder:
				case Buttons.RightShoulder:
				case Buttons.A:
				case Buttons.B:
				case Buttons.X:
				case Buttons.Y:
					if (Check(button))
					{
						return true;
					}
					break;
				case Buttons.RightTrigger:
					if (RightTriggerCheck(threshold))
					{
						return true;
					}
					break;
				case Buttons.LeftTrigger:
					if (LeftTriggerCheck(threshold))
					{
						return true;
					}
					break;
				case Buttons.LeftThumbstickUp:
					if (LeftStickUpCheck(threshold))
					{
						return true;
					}
					break;
				case Buttons.LeftThumbstickDown:
					if (LeftStickDownCheck(threshold))
					{
						return true;
					}
					break;
				case Buttons.LeftThumbstickRight:
					if (LeftStickRightCheck(threshold))
					{
						return true;
					}
					break;
				case Buttons.LeftThumbstickLeft:
					if (LeftStickLeftCheck(threshold))
					{
						return true;
					}
					break;
				case Buttons.RightThumbstickUp:
					if (RightStickUpCheck(threshold))
					{
						return true;
					}
					break;
				case Buttons.RightThumbstickDown:
					if (RightStickDownCheck(threshold))
					{
						return true;
					}
					break;
				case Buttons.RightThumbstickRight:
					if (RightStickRightCheck(threshold))
					{
						return true;
					}
					break;
				case Buttons.RightThumbstickLeft:
					if (RightStickLeftCheck(threshold))
					{
						return true;
					}
					break;
				}
				return false;
			}

			public bool Pressed(Buttons button, float threshold)
			{
				if (Disabled)
				{
					return false;
				}
				switch (button)
				{
				case Buttons.DPadUp:
				case Buttons.DPadDown:
				case Buttons.DPadLeft:
				case Buttons.DPadRight:
				case Buttons.Start:
				case Buttons.Back:
				case Buttons.LeftStick:
				case Buttons.RightStick:
				case Buttons.LeftShoulder:
				case Buttons.RightShoulder:
				case Buttons.A:
				case Buttons.B:
				case Buttons.X:
				case Buttons.Y:
					if (Pressed(button))
					{
						return true;
					}
					break;
				case Buttons.RightTrigger:
					if (RightTriggerPressed(threshold))
					{
						return true;
					}
					break;
				case Buttons.LeftTrigger:
					if (LeftTriggerPressed(threshold))
					{
						return true;
					}
					break;
				case Buttons.LeftThumbstickUp:
					if (LeftStickUpPressed(threshold))
					{
						return true;
					}
					break;
				case Buttons.LeftThumbstickDown:
					if (LeftStickDownPressed(threshold))
					{
						return true;
					}
					break;
				case Buttons.LeftThumbstickRight:
					if (LeftStickRightPressed(threshold))
					{
						return true;
					}
					break;
				case Buttons.LeftThumbstickLeft:
					if (LeftStickLeftPressed(threshold))
					{
						return true;
					}
					break;
				case Buttons.RightThumbstickUp:
					if (RightStickUpPressed(threshold))
					{
						return true;
					}
					break;
				case Buttons.RightThumbstickDown:
					if (RightStickDownPressed(threshold))
					{
						return true;
					}
					break;
				case Buttons.RightThumbstickRight:
					if (RightStickRightPressed(threshold))
					{
						return true;
					}
					break;
				case Buttons.RightThumbstickLeft:
					if (RightStickLeftPressed(threshold))
					{
						return true;
					}
					break;
				}
				return false;
			}

			public bool Released(Buttons button, float threshold)
			{
				if (Disabled)
				{
					return false;
				}
				switch (button)
				{
				case Buttons.DPadUp:
				case Buttons.DPadDown:
				case Buttons.DPadLeft:
				case Buttons.DPadRight:
				case Buttons.Start:
				case Buttons.Back:
				case Buttons.LeftStick:
				case Buttons.RightStick:
				case Buttons.LeftShoulder:
				case Buttons.RightShoulder:
				case Buttons.A:
				case Buttons.B:
				case Buttons.X:
				case Buttons.Y:
					if (Released(button))
					{
						return true;
					}
					break;
				case Buttons.RightTrigger:
					if (RightTriggerReleased(threshold))
					{
						return true;
					}
					break;
				case Buttons.LeftTrigger:
					if (LeftTriggerReleased(threshold))
					{
						return true;
					}
					break;
				case Buttons.LeftThumbstickUp:
					if (LeftStickUpReleased(threshold))
					{
						return true;
					}
					break;
				case Buttons.LeftThumbstickDown:
					if (LeftStickDownReleased(threshold))
					{
						return true;
					}
					break;
				case Buttons.LeftThumbstickRight:
					if (LeftStickRightReleased(threshold))
					{
						return true;
					}
					break;
				case Buttons.LeftThumbstickLeft:
					if (LeftStickLeftReleased(threshold))
					{
						return true;
					}
					break;
				case Buttons.RightThumbstickUp:
					if (RightStickUpReleased(threshold))
					{
						return true;
					}
					break;
				case Buttons.RightThumbstickDown:
					if (RightStickDownReleased(threshold))
					{
						return true;
					}
					break;
				case Buttons.RightThumbstickRight:
					if (RightStickRightReleased(threshold))
					{
						return true;
					}
					break;
				case Buttons.RightThumbstickLeft:
					if (RightStickLeftReleased(threshold))
					{
						return true;
					}
					break;
				}
				return false;
			}
		}

		internal static List<VirtualInput> VirtualInputs;

		public static bool Active = true;

		public static bool Disabled = false;

		public static bool ControllerHasFocus = false;

		public static bool IsControllerFocused = false;

		public static KeyboardData Keyboard { get; private set; }

		public static MouseData Mouse { get; private set; }

		public static GamePadData[] GamePads { get; private set; }

		internal static void Initialize()
		{
			Keyboard = new KeyboardData();
			Mouse = new MouseData();
			GamePads = new GamePadData[4];
			for (int i = 0; i < 4; i++)
			{
				GamePads[i] = new GamePadData(i);
			}
			VirtualInputs = new List<VirtualInput>();
		}

		internal static void Shutdown()
		{
			GamePadData[] gamePads = GamePads;
			for (int i = 0; i < gamePads.Length; i++)
			{
				gamePads[i].StopRumble();
			}
		}

		internal static void Update()
		{
			if (Engine.Instance.IsActive && Active)
			{
				if (Engine.Commands.Open)
				{
					Keyboard.UpdateNull();
					Mouse.UpdateNull();
				}
				else
				{
					Keyboard.Update();
					Mouse.Update();
				}
				bool gamepadInUse = false;
				bool gamepadDetected = false;
				for (int j = 0; j < 4; j++)
				{
					GamePads[j].Update();
					if (GamePads[j].HasAnyInput())
					{
						ControllerHasFocus = true;
						gamepadInUse = true;
					}
					if (GamePads[j].Attached)
					{
						gamepadDetected = true;
					}
				}
				if (!gamepadDetected || (!gamepadInUse && Keyboard.HasAnyInput()))
				{
					ControllerHasFocus = false;
				}
			}
			else
			{
				Keyboard.UpdateNull();
				Mouse.UpdateNull();
				for (int i = 0; i < 4; i++)
				{
					GamePads[i].UpdateNull();
				}
			}
			UpdateVirtualInputs();
		}

		public static void UpdateNull()
		{
			Keyboard.UpdateNull();
			Mouse.UpdateNull();
			for (int i = 0; i < 4; i++)
			{
				GamePads[i].UpdateNull();
			}
			UpdateVirtualInputs();
		}

		private static void UpdateVirtualInputs()
		{
			foreach (VirtualInput virtualInput in VirtualInputs)
			{
				virtualInput.Update();
			}
		}

		public static void RumbleFirst(float strength, float time)
		{
			GamePads[0].Rumble(strength, time);
		}

		public static int Axis(bool negative, bool positive, int bothValue)
		{
			if (negative)
			{
				if (positive)
				{
					return bothValue;
				}
				return -1;
			}
			if (positive)
			{
				return 1;
			}
			return 0;
		}

		public static int Axis(float axisValue, float deadzone)
		{
			if (Math.Abs(axisValue) >= deadzone)
			{
				return Math.Sign(axisValue);
			}
			return 0;
		}

		public static int Axis(bool negative, bool positive, int bothValue, float axisValue, float deadzone)
		{
			int ret = Axis(axisValue, deadzone);
			if (ret == 0)
			{
				ret = Axis(negative, positive, bothValue);
			}
			return ret;
		}
	}
}
