using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class ButtonConfigUI : TextMenu
	{
		private bool remapping;

		private float remappingEase;

		private Binding remappingBinding;

		private string remappingText;

		private float inputDelay;

		private float timeout;

		private bool closing;

		private float closingDelay;

		private bool waitingForController;

		private bool resetHeld;

		private float resetTime;

		private float resetDelay;

		private List<Buttons> all = new List<Buttons>
		{
			Buttons.A,
			Buttons.B,
			Buttons.X,
			Buttons.Y,
			Buttons.LeftShoulder,
			Buttons.RightShoulder,
			Buttons.LeftTrigger,
			Buttons.RightTrigger
		};

		public static readonly string StadiaControllerDisclaimer = "No endorsement or affiliation is intended between Stadia and the manufacturers\nof non-Stadia controllers or consoles. STADIA, the Stadia beacon, Google, and related\nmarks and logos are trademarks of Google LLC. All other trademarks are the\nproperty of their respective owners.";

		public ButtonConfigUI()
		{
			Add(new Header(Dialog.Clean("BTN_CONFIG_TITLE")));
			Add(new InputMappingInfo(controllerMode: true));
			Add(new SubHeader(Dialog.Clean("KEY_CONFIG_GAMEPLAY")));
			AddMap("LEFT", Settings.Instance.Left);
			AddMap("RIGHT", Settings.Instance.Right);
			AddMap("UP", Settings.Instance.Up);
			AddMap("DOWN", Settings.Instance.Down);
			AddMap("JUMP", Settings.Instance.Jump);
			AddMap("DASH", Settings.Instance.Dash);
			AddMap("GRAB", Settings.Instance.Grab);
			AddMap("TALK", Settings.Instance.Talk);
			Add(new SubHeader(Dialog.Clean("KEY_CONFIG_MENUS")));
			Add(new SubHeader(Dialog.Clean("KEY_CONFIG_MENU_NOTICE"), topPadding: false));
			AddMap("LEFT", Settings.Instance.MenuLeft);
			AddMap("RIGHT", Settings.Instance.MenuRight);
			AddMap("UP", Settings.Instance.MenuUp);
			AddMap("DOWN", Settings.Instance.MenuDown);
			AddMap("CONFIRM", Settings.Instance.Confirm);
			AddMap("CANCEL", Settings.Instance.Cancel);
			AddMap("JOURNAL", Settings.Instance.Journal);
			AddMap("PAUSE", Settings.Instance.Pause);
			Add(new SubHeader(""));
			Add(new Button(Dialog.Clean("KEY_CONFIG_RESET"))
			{
				IncludeWidthInMeasurement = false,
				AlwaysCenter = true,
				OnPressed = delegate
				{
					resetHeld = true;
					resetTime = 0f;
					resetDelay = 0f;
				},
				ConfirmSfx = "event:/ui/main/button_lowkey"
			});
			Add(new SubHeader(Dialog.Clean("KEY_CONFIG_ADVANCED")));
			AddMap("QUICKRESTART", Settings.Instance.QuickRestart);
			AddMap("DEMO", Settings.Instance.DemoDash);
			Add(new SubHeader(Dialog.Clean("KEY_CONFIG_MOVE_ONLY")));
			AddMap("LEFT", Settings.Instance.LeftMoveOnly);
			AddMap("RIGHT", Settings.Instance.RightMoveOnly);
			AddMap("UP", Settings.Instance.UpMoveOnly);
			AddMap("DOWN", Settings.Instance.DownMoveOnly);
			Add(new SubHeader(Dialog.Clean("KEY_CONFIG_DASH_ONLY")));
			AddMap("LEFT", Settings.Instance.LeftDashOnly);
			AddMap("RIGHT", Settings.Instance.RightDashOnly);
			AddMap("UP", Settings.Instance.UpDashOnly);
			AddMap("DOWN", Settings.Instance.DownDashOnly);
			OnESC = (OnCancel = delegate
			{
				MenuOptions.UpdateCrouchDashModeVisibility();
				Focused = false;
				closing = true;
			});
			MinWidth = 600f;
			Position.Y = base.ScrollTargetY;
			Alpha = 0f;
		}

		private void AddMap(string label, Binding binding)
		{
			string txt = Dialog.Clean("KEY_CONFIG_" + label);
			Add(new Setting(txt, binding, controllerMode: true).Pressed(delegate
			{
				remappingText = txt;
				Remap(binding);
			}).AltPressed(delegate
			{
				Clear(binding);
			}));
		}

		private void Remap(Binding binding)
		{
			if (Input.GuiInputController())
			{
				remapping = true;
				remappingBinding = binding;
				timeout = 5f;
				Focused = false;
			}
		}

		private void AddRemap(Buttons btn)
		{
			while (remappingBinding.Controller.Count >= Input.MaxBindings)
			{
				remappingBinding.Controller.RemoveAt(0);
			}
			remapping = false;
			inputDelay = 0.25f;
			if (!remappingBinding.Add(btn))
			{
				Audio.Play("event:/ui/main/button_invalid");
			}
			Input.Initialize();
		}

		private void Clear(Binding binding)
		{
			if (!binding.ClearGamepad())
			{
				Audio.Play("event:/ui/main/button_invalid");
			}
		}

		public override void Update()
		{
			if (resetHeld)
			{
				resetDelay += Engine.DeltaTime;
				resetTime += Engine.DeltaTime;
				if (resetTime > 1.5f)
				{
					resetDelay = 0f;
					resetHeld = false;
					Settings.Instance.SetDefaultButtonControls(reset: true);
					Input.Initialize();
					Audio.Play("event:/ui/main/button_select");
				}
				if (!Input.MenuConfirm.Check && resetDelay > 0.3f)
				{
					Audio.Play("event:/ui/main/button_invalid");
					resetHeld = false;
				}
				if (resetHeld)
				{
					return;
				}
			}
			base.Update();
			Focused = !closing && inputDelay <= 0f && !waitingForController && !remapping;
			if (!closing)
			{
				if (!MInput.GamePads[Input.Gamepad].Attached)
				{
					waitingForController = true;
				}
				else if (waitingForController)
				{
					waitingForController = false;
				}
				if (Input.MenuCancel.Pressed && !remapping)
				{
					OnCancel();
				}
			}
			if (inputDelay > 0f && !remapping)
			{
				inputDelay -= Engine.RawDeltaTime;
			}
			remappingEase = Calc.Approach(remappingEase, remapping ? 1 : 0, Engine.RawDeltaTime * 4f);
			if (remappingEase >= 0.25f && remapping)
			{
				if (Input.ESC.Pressed || timeout <= 0f || !Input.GuiInputController())
				{
					remapping = false;
					Focused = true;
				}
				else
				{
					MInput.GamePadData gamepad = MInput.GamePads[Input.Gamepad];
					float deadzone = 0.25f;
					if (gamepad.LeftStickLeftPressed(deadzone))
					{
						AddRemap(Buttons.LeftThumbstickLeft);
					}
					else if (gamepad.LeftStickRightPressed(deadzone))
					{
						AddRemap(Buttons.LeftThumbstickRight);
					}
					else if (gamepad.LeftStickUpPressed(deadzone))
					{
						AddRemap(Buttons.LeftThumbstickUp);
					}
					else if (gamepad.LeftStickDownPressed(deadzone))
					{
						AddRemap(Buttons.LeftThumbstickDown);
					}
					else if (gamepad.RightStickLeftPressed(deadzone))
					{
						AddRemap(Buttons.RightThumbstickLeft);
					}
					else if (gamepad.RightStickRightPressed(deadzone))
					{
						AddRemap(Buttons.RightThumbstickRight);
					}
					else if (gamepad.RightStickDownPressed(deadzone))
					{
						AddRemap(Buttons.RightThumbstickDown);
					}
					else if (gamepad.RightStickUpPressed(deadzone))
					{
						AddRemap(Buttons.RightThumbstickUp);
					}
					else if (gamepad.LeftTriggerPressed(deadzone))
					{
						AddRemap(Buttons.LeftTrigger);
					}
					else if (gamepad.RightTriggerPressed(deadzone))
					{
						AddRemap(Buttons.RightTrigger);
					}
					else if (gamepad.Pressed(Buttons.DPadLeft))
					{
						AddRemap(Buttons.DPadLeft);
					}
					else if (gamepad.Pressed(Buttons.DPadRight))
					{
						AddRemap(Buttons.DPadRight);
					}
					else if (gamepad.Pressed(Buttons.DPadUp))
					{
						AddRemap(Buttons.DPadUp);
					}
					else if (gamepad.Pressed(Buttons.DPadDown))
					{
						AddRemap(Buttons.DPadDown);
					}
					else if (gamepad.Pressed(Buttons.A))
					{
						AddRemap(Buttons.A);
					}
					else if (gamepad.Pressed(Buttons.B))
					{
						AddRemap(Buttons.B);
					}
					else if (gamepad.Pressed(Buttons.X))
					{
						AddRemap(Buttons.X);
					}
					else if (gamepad.Pressed(Buttons.Y))
					{
						AddRemap(Buttons.Y);
					}
					else if (gamepad.Pressed(Buttons.Start))
					{
						AddRemap(Buttons.Start);
					}
					else if (gamepad.Pressed(Buttons.Back))
					{
						AddRemap(Buttons.Back);
					}
					else if (gamepad.Pressed(Buttons.LeftShoulder))
					{
						AddRemap(Buttons.LeftShoulder);
					}
					else if (gamepad.Pressed(Buttons.RightShoulder))
					{
						AddRemap(Buttons.RightShoulder);
					}
					else if (gamepad.Pressed(Buttons.LeftStick))
					{
						AddRemap(Buttons.LeftStick);
					}
					else if (gamepad.Pressed(Buttons.RightStick))
					{
						AddRemap(Buttons.RightStick);
					}
				}
				timeout -= Engine.RawDeltaTime;
			}
			closingDelay -= Engine.RawDeltaTime;
			Alpha = Calc.Approach(Alpha, (!closing || !(closingDelay <= 0f)) ? 1 : 0, Engine.RawDeltaTime * 8f);
			if (closing && Alpha <= 0f)
			{
				Close();
			}
		}

		public override void Render()
		{
			Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * Ease.CubeOut(Alpha));
			Vector2 center = new Vector2(1920f, 1080f) * 0.5f;
			if (MInput.GamePads[Input.Gamepad].Attached)
			{
				base.Render();
				if (Celeste.IsGGP && Input.GuiInputPrefix() != "stadia")
				{
					float scale = 0.33f;
					ActiveFont.Draw(StadiaControllerDisclaimer, new Vector2(50f, 1080f - 50f * Alpha), new Vector2(0f, 1f), Vector2.One * scale, Color.White * Alpha * 0.5f);
				}
				if (remappingEase > 0f)
				{
					Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * 0.95f * Ease.CubeInOut(remappingEase));
					ActiveFont.Draw(Dialog.Get("BTN_CONFIG_CHANGING"), center + new Vector2(0f, -8f), new Vector2(0.5f, 1f), Vector2.One * 0.7f, Color.LightGray * Ease.CubeIn(remappingEase));
					ActiveFont.Draw(remappingText, center + new Vector2(0f, 8f), new Vector2(0.5f, 0f), Vector2.One * 2f, Color.White * Ease.CubeIn(remappingEase));
				}
			}
			else
			{
				ActiveFont.Draw(Dialog.Clean("BTN_CONFIG_NOCONTROLLER"), center, new Vector2(0.5f, 0.5f), Vector2.One, Color.White * Ease.CubeOut(Alpha));
			}
			if (resetHeld)
			{
				float ease = Ease.CubeInOut(Calc.Min(1f, resetDelay / 0.2f));
				float fill = Ease.SineOut(Calc.Min(1f, resetTime / 1.5f));
				Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * 0.95f * ease);
				float width = 480f;
				float x = (1920f - width) / 2f;
				Draw.Rect(x, 530f, width, 20f, Color.White * 0.25f * ease);
				Draw.Rect(x, 530f, width * fill, 20f, Color.White * ease);
			}
		}
	}
}
