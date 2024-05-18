using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class KeyboardConfigUI : TextMenu
	{
		private bool remapping;

		private float remappingEase;

		private Binding remappingBinding;

		private string remappingText;

		private float inputDelay;

		private float timeout;

		private bool closing;

		private float closingDelay;

		private bool resetHeld;

		private float resetTime;

		private float resetDelay;

		public KeyboardConfigUI()
		{
			Add(new Header(Dialog.Clean("KEY_CONFIG_TITLE")));
			Add(new InputMappingInfo(controllerMode: false));
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
				ConfirmSfx = "event:/ui/main/button_lowkey",
				OnPressed = delegate
				{
					resetHeld = true;
					resetTime = 0f;
					resetDelay = 0f;
				}
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
			Add(new Setting(txt, binding, controllerMode: false).Pressed(delegate
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
			remapping = true;
			remappingBinding = binding;
			timeout = 5f;
			Focused = false;
		}

		private void AddRemap(Keys key)
		{
			while (remappingBinding.Keyboard.Count >= Input.MaxBindings)
			{
				remappingBinding.Keyboard.RemoveAt(0);
			}
			remapping = false;
			inputDelay = 0.25f;
			if (!remappingBinding.Add(key))
			{
				Audio.Play("event:/ui/main/button_invalid");
			}
			Input.Initialize();
		}

		private void Clear(Binding binding)
		{
			if (!binding.ClearKeyboard())
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
					Settings.Instance.SetDefaultKeyboardControls(reset: true);
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
			Focused = !closing && inputDelay <= 0f && !remapping;
			if (!closing && Input.MenuCancel.Pressed && !remapping)
			{
				OnCancel();
			}
			if (inputDelay > 0f && !remapping)
			{
				inputDelay -= Engine.RawDeltaTime;
			}
			remappingEase = Calc.Approach(remappingEase, remapping ? 1 : 0, Engine.RawDeltaTime * 4f);
			if (remappingEase >= 0.25f && remapping)
			{
				if (Input.ESC.Pressed || timeout <= 0f)
				{
					remapping = false;
					Focused = true;
				}
				else
				{
					Keys[] keys = MInput.Keyboard.CurrentState.GetPressedKeys();
					if (keys != null && keys.Length != 0 && MInput.Keyboard.Pressed(keys[keys.Length - 1]))
					{
						AddRemap(keys[keys.Length - 1]);
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
			base.Render();
			if (remappingEase > 0f)
			{
				Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * 0.95f * Ease.CubeInOut(remappingEase));
				ActiveFont.Draw(Dialog.Get("KEY_CONFIG_CHANGING"), center + new Vector2(0f, -8f), new Vector2(0.5f, 1f), Vector2.One * 0.7f, Color.LightGray * Ease.CubeIn(remappingEase));
				ActiveFont.Draw(remappingText, center + new Vector2(0f, 8f), new Vector2(0.5f, 0f), Vector2.One * 2f, Color.White * Ease.CubeIn(remappingEase));
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
