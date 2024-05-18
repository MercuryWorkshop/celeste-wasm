using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste
{
	public static class Input
	{
		public enum PrefixMode
		{
			Latest,
			Attached
		}

		private static int gamepad = 0;

		public static readonly int MaxBindings = 8;

		public static VirtualButton ESC;

		public static VirtualButton Pause;

		public static VirtualButton MenuLeft;

		public static VirtualButton MenuRight;

		public static VirtualButton MenuUp;

		public static VirtualButton MenuDown;

		public static VirtualButton MenuConfirm;

		public static VirtualButton MenuCancel;

		public static VirtualButton MenuJournal;

		public static VirtualButton QuickRestart;

		public static VirtualIntegerAxis MoveX;

		public static VirtualIntegerAxis MoveY;

		public static VirtualIntegerAxis GliderMoveY;

		public static VirtualJoystick Aim;

		public static VirtualJoystick Feather;

		public static VirtualJoystick MountainAim;

		public static VirtualButton Jump;

		public static VirtualButton Dash;

		public static VirtualButton Grab;

		public static VirtualButton Talk;

		public static VirtualButton CrouchDash;

		private static bool grabToggle;

		public static Vector2 LastAim;

		public static string OverrideInputPrefix = null;

		private static Dictionary<Keys, string> keyNameLookup = new Dictionary<Keys, string>();

		private static Dictionary<Buttons, string> buttonNameLookup = new Dictionary<Buttons, string>();

		private static Dictionary<string, Dictionary<string, string>> guiPathLookup = new Dictionary<string, Dictionary<string, string>>();

		private static float[] rumbleStrengths = new float[4] { 0.15f, 0.4f, 1f, 0.05f };

		private static float[] rumbleLengths = new float[5] { 0.1f, 0.25f, 0.5f, 1f, 2f };

		public static int Gamepad
		{
			get
			{
				return gamepad;
			}
			set
			{
				int index = Calc.Clamp(value, 0, MInput.GamePads.Length - 1);
				if (gamepad != index)
				{
					gamepad = index;
					Initialize();
				}
			}
		}

		public static bool GrabCheck => Settings.Instance.GrabMode switch
		{
			GrabModes.Invert => !Grab.Check, 
			GrabModes.Toggle => grabToggle, 
			_ => Grab.Check, 
		};

		public static bool DashPressed
		{
			get
			{
				CrouchDashModes crouchDashMode = Settings.Instance.CrouchDashMode;
				if (crouchDashMode == CrouchDashModes.Press || crouchDashMode != CrouchDashModes.Hold)
				{
					return Dash.Pressed;
				}
				if (Dash.Pressed)
				{
					return !CrouchDash.Check;
				}
				return false;
			}
		}

		public static bool CrouchDashPressed
		{
			get
			{
				CrouchDashModes crouchDashMode = Settings.Instance.CrouchDashMode;
				if (crouchDashMode == CrouchDashModes.Press || crouchDashMode != CrouchDashModes.Hold)
				{
					return CrouchDash.Pressed;
				}
				if (Dash.Pressed)
				{
					return CrouchDash.Check;
				}
				return false;
			}
		}

		public static void Initialize()
		{
			bool inverted = false;
			if (MoveX != null)
			{
				inverted = MoveX.Inverted;
			}
			Deregister();
			MoveX = new VirtualIntegerAxis(Settings.Instance.Left, Settings.Instance.LeftMoveOnly, Settings.Instance.Right, Settings.Instance.RightMoveOnly, Gamepad, 0.3f);
			MoveX.Inverted = inverted;
			MoveY = new VirtualIntegerAxis(Settings.Instance.Up, Settings.Instance.UpMoveOnly, Settings.Instance.Down, Settings.Instance.DownMoveOnly, Gamepad, 0.7f);
			GliderMoveY = new VirtualIntegerAxis(Settings.Instance.Up, Settings.Instance.UpMoveOnly, Settings.Instance.Down, Settings.Instance.DownMoveOnly, Gamepad, 0.3f);
			Aim = new VirtualJoystick(Settings.Instance.Up, Settings.Instance.UpDashOnly, Settings.Instance.Down, Settings.Instance.DownDashOnly, Settings.Instance.Left, Settings.Instance.LeftDashOnly, Settings.Instance.Right, Settings.Instance.RightDashOnly, Gamepad, 0.25f);
			Aim.InvertedX = inverted;
			Feather = new VirtualJoystick(Settings.Instance.Up, Settings.Instance.UpMoveOnly, Settings.Instance.Down, Settings.Instance.DownMoveOnly, Settings.Instance.Left, Settings.Instance.LeftMoveOnly, Settings.Instance.Right, Settings.Instance.RightMoveOnly, Gamepad, 0.25f);
			Feather.InvertedX = inverted;
			Jump = new VirtualButton(Settings.Instance.Jump, Gamepad, 0.08f, 0.2f);
			Dash = new VirtualButton(Settings.Instance.Dash, Gamepad, 0.08f, 0.2f);
			Talk = new VirtualButton(Settings.Instance.Talk, Gamepad, 0.08f, 0.2f);
			Grab = new VirtualButton(Settings.Instance.Grab, Gamepad, 0f, 0.2f);
			CrouchDash = new VirtualButton(Settings.Instance.DemoDash, Gamepad, 0.08f, 0.2f);
			Binding mtLeft = new Binding();
			mtLeft.Add(Keys.A);
			mtLeft.Add(Buttons.RightThumbstickLeft);
			Binding mtRight = new Binding();
			mtRight.Add(Keys.D);
			mtRight.Add(Buttons.RightThumbstickRight);
			Binding mtUp = new Binding();
			mtUp.Add(Keys.W);
			mtUp.Add(Buttons.RightThumbstickUp);
			Binding mtDown = new Binding();
			mtDown.Add(Keys.S);
			mtDown.Add(Buttons.RightThumbstickDown);
			MountainAim = new VirtualJoystick(mtUp, mtDown, mtLeft, mtRight, Gamepad, 0.1f);
			Binding esc = new Binding();
			esc.Add(Keys.Escape);
			ESC = new VirtualButton(esc, Gamepad, 0.1f, 0.2f);
			Pause = new VirtualButton(Settings.Instance.Pause, Gamepad, 0.1f, 0.2f);
			QuickRestart = new VirtualButton(Settings.Instance.QuickRestart, Gamepad, 0.1f, 0.2f);
			MenuLeft = new VirtualButton(Settings.Instance.MenuLeft, Gamepad, 0f, 0.4f);
			MenuLeft.SetRepeat(0.4f, 0.1f);
			MenuRight = new VirtualButton(Settings.Instance.MenuRight, Gamepad, 0f, 0.4f);
			MenuRight.SetRepeat(0.4f, 0.1f);
			MenuUp = new VirtualButton(Settings.Instance.MenuUp, Gamepad, 0f, 0.4f);
			MenuUp.SetRepeat(0.4f, 0.1f);
			MenuDown = new VirtualButton(Settings.Instance.MenuDown, Gamepad, 0f, 0.4f);
			MenuDown.SetRepeat(0.4f, 0.1f);
			MenuJournal = new VirtualButton(Settings.Instance.Journal, Gamepad, 0f, 0.2f);
			MenuConfirm = new VirtualButton(Settings.Instance.Confirm, Gamepad, 0f, 0.2f);
			MenuCancel = new VirtualButton(Settings.Instance.Cancel, Gamepad, 0f, 0.2f);
		}

		public static void Deregister()
		{
			if (ESC != null)
			{
				ESC.Deregister();
			}
			if (Pause != null)
			{
				Pause.Deregister();
			}
			if (MenuLeft != null)
			{
				MenuLeft.Deregister();
			}
			if (MenuRight != null)
			{
				MenuRight.Deregister();
			}
			if (MenuUp != null)
			{
				MenuUp.Deregister();
			}
			if (MenuDown != null)
			{
				MenuDown.Deregister();
			}
			if (MenuConfirm != null)
			{
				MenuConfirm.Deregister();
			}
			if (MenuCancel != null)
			{
				MenuCancel.Deregister();
			}
			if (MenuJournal != null)
			{
				MenuJournal.Deregister();
			}
			if (QuickRestart != null)
			{
				QuickRestart.Deregister();
			}
			if (MoveX != null)
			{
				MoveX.Deregister();
			}
			if (MoveY != null)
			{
				MoveY.Deregister();
			}
			if (GliderMoveY != null)
			{
				GliderMoveY.Deregister();
			}
			if (Aim != null)
			{
				Aim.Deregister();
			}
			if (MountainAim != null)
			{
				MountainAim.Deregister();
			}
			if (Jump != null)
			{
				Jump.Deregister();
			}
			if (Dash != null)
			{
				Dash.Deregister();
			}
			if (Grab != null)
			{
				Grab.Deregister();
			}
			if (Talk != null)
			{
				Talk.Deregister();
			}
			if (CrouchDash != null)
			{
				CrouchDash.Deregister();
			}
		}

		public static bool AnyGamepadConfirmPressed(out int gamepadIndex)
		{
			bool result = false;
			gamepadIndex = -1;
			int was = MenuConfirm.GamepadIndex;
			for (int i = 0; i < MInput.GamePads.Length; i++)
			{
				MenuConfirm.GamepadIndex = i;
				if (MenuConfirm.Pressed)
				{
					result = true;
					gamepadIndex = i;
					break;
				}
			}
			MenuConfirm.GamepadIndex = was;
			return result;
		}

		public static void Rumble(RumbleStrength strength, RumbleLength length)
		{
			float multiplier = 1f;
			if (Settings.Instance.Rumble == RumbleAmount.Half)
			{
				multiplier = 0.5f;
			}
			if (Settings.Instance.Rumble != 0 && MInput.GamePads.Length != 0 && !MInput.Disabled)
			{
				MInput.GamePads[Gamepad].Rumble(rumbleStrengths[(int)strength] * multiplier, rumbleLengths[(int)length]);
			}
		}

		public static void RumbleSpecific(float strength, float time)
		{
			float multiplier = 1f;
			if (Settings.Instance.Rumble == RumbleAmount.Half)
			{
				multiplier = 0.5f;
			}
			if (Settings.Instance.Rumble != 0 && MInput.GamePads.Length != 0 && !MInput.Disabled)
			{
				MInput.GamePads[Gamepad].Rumble(strength * multiplier, time);
			}
		}

		public static void UpdateGrab()
		{
			if (Settings.Instance.GrabMode == GrabModes.Toggle && Grab.Pressed)
			{
				grabToggle = !grabToggle;
			}
		}

		public static void ResetGrab()
		{
			grabToggle = false;
		}

		public static Vector2 GetAimVector(Facings defaultFacing = Facings.Right)
		{
			Vector2 vec = Aim.Value;
			if (vec == Vector2.Zero)
			{
				if (SaveData.Instance != null && SaveData.Instance.Assists.DashAssist)
				{
					return LastAim;
				}
				LastAim = Vector2.UnitX * (float)defaultFacing;
			}
			else if (SaveData.Instance != null && SaveData.Instance.Assists.ThreeSixtyDashing)
			{
				LastAim = vec.SafeNormalize();
			}
			else
			{
				float angle = vec.Angle();
				int diagonals = ((angle < 0f) ? 1 : 0);
				float difference = (float)Math.PI / 8f - (float)diagonals * 0.08726646f;
				if (Calc.AbsAngleDiff(angle, 0f) < difference)
				{
					LastAim = new Vector2(1f, 0f);
				}
				else if (Calc.AbsAngleDiff(angle, (float)Math.PI) < difference)
				{
					LastAim = new Vector2(-1f, 0f);
				}
				else if (Calc.AbsAngleDiff(angle, -(float)Math.PI / 2f) < difference)
				{
					LastAim = new Vector2(0f, -1f);
				}
				else if (Calc.AbsAngleDiff(angle, (float)Math.PI / 2f) < difference)
				{
					LastAim = new Vector2(0f, 1f);
				}
				else
				{
					LastAim = new Vector2(Math.Sign(vec.X), Math.Sign(vec.Y)).SafeNormalize();
				}
			}
			return LastAim;
		}

		public static string GuiInputPrefix(PrefixMode mode = PrefixMode.Latest)
		{
			if (!string.IsNullOrEmpty(OverrideInputPrefix))
			{
				return OverrideInputPrefix;
			}
			bool showController = false;
			if ((mode != 0) ? MInput.GamePads[Gamepad].Attached : MInput.ControllerHasFocus)
			{
				string guid = GamePad.GetGUIDEXT(MInput.GamePads[Gamepad].PlayerIndex);
				if (guid.Equals("4c05c405") || guid.Equals("4c05cc09"))
				{
					return "ps4";
				}
				if (guid.Equals("7e050920") || guid.Equals("7e053003"))
				{
					return "ns";
				}
				if (guid.Equals("d1180094"))
				{
					return "stadia";
				}
				return "xb1";
			}
			return "keyboard";
		}

		public static bool GuiInputController(PrefixMode mode = PrefixMode.Latest)
		{
			return !GuiInputPrefix(mode).Equals("keyboard");
		}

		public static MTexture GuiButton(VirtualButton button, PrefixMode mode = PrefixMode.Latest, string fallback = "controls/keyboard/oemquestion")
		{
			string prefix = GuiInputPrefix(mode);
			bool num = GuiInputController(mode);
			string input = "";
			if (num)
			{
				using List<Buttons>.Enumerator enumerator = button.Binding.Controller.GetEnumerator();
				if (enumerator.MoveNext())
				{
					Buttons btn = enumerator.Current;
					if (!buttonNameLookup.TryGetValue(btn, out input))
					{
						buttonNameLookup.Add(btn, input = btn.ToString());
					}
				}
			}
			else
			{
				Keys key = FirstKey(button);
				if (!keyNameLookup.TryGetValue(key, out input))
				{
					keyNameLookup.Add(key, input = key.ToString());
				}
			}
			MTexture tex = GuiTexture(prefix, input);
			if (tex == null && fallback != null)
			{
				return GFX.Gui[fallback];
			}
			return tex;
		}

		public static MTexture GuiSingleButton(Buttons button, PrefixMode mode = PrefixMode.Latest, string fallback = "controls/keyboard/oemquestion")
		{
			string prefix = ((!GuiInputController(mode)) ? "xb1" : GuiInputPrefix(mode));
			string input = "";
			if (!buttonNameLookup.TryGetValue(button, out input))
			{
				buttonNameLookup.Add(button, input = button.ToString());
			}
			MTexture tex = GuiTexture(prefix, input);
			if (tex == null && fallback != null)
			{
				return GFX.Gui[fallback];
			}
			return tex;
		}

		public static MTexture GuiKey(Keys key, string fallback = "controls/keyboard/oemquestion")
		{
			if (!keyNameLookup.TryGetValue(key, out var input))
			{
				keyNameLookup.Add(key, input = key.ToString());
			}
			MTexture tex = GuiTexture("keyboard", input);
			if (tex == null && fallback != null)
			{
				return GFX.Gui[fallback];
			}
			return tex;
		}

		public static Buttons FirstButton(VirtualButton button)
		{
			using (List<Buttons>.Enumerator enumerator = button.Binding.Controller.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					return enumerator.Current;
				}
			}
			return Buttons.A;
		}

		public static Keys FirstKey(VirtualButton button)
		{
			foreach (Keys key in button.Binding.Keyboard)
			{
				if (key != 0)
				{
					return key;
				}
			}
			return Keys.None;
		}

		public static MTexture GuiDirection(Vector2 direction)
		{
			int num = Math.Sign(direction.X);
			string input = string.Concat(arg2: Math.Sign(direction.Y), arg0: num, arg1: "x");
			return GuiTexture("directions", input);
		}

		private static MTexture GuiTexture(string prefix, string input)
		{
			if (!guiPathLookup.TryGetValue(prefix, out var list))
			{
				guiPathLookup.Add(prefix, list = new Dictionary<string, string>());
			}
			if (!list.TryGetValue(input, out var path))
			{
				list.Add(input, path = "controls/" + prefix + "/" + input);
			}
			if (!GFX.Gui.Has(path))
			{
				if (prefix != "fallback")
				{
					return GuiTexture("fallback", input);
				}
				return null;
			}
			return GFX.Gui[path];
		}

		public static void SetLightbarColor(Color color)
		{
			color.R = (byte)(Math.Pow((float)(int)color.R / 255f, 3.0) * 255.0);
			color.G = (byte)(Math.Pow((float)(int)color.G / 255f, 3.0) * 255.0);
			color.B = (byte)(Math.Pow((float)(int)color.B / 255f, 3.0) * 255.0);
			GamePad.SetLightBarEXT((PlayerIndex)Gamepad, color);
		}
	}
}
