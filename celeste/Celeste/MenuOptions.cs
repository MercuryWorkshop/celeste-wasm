using FMOD.Studio;
using Monocle;

namespace Celeste
{
	public static class MenuOptions
	{
		private static TextMenu menu;

		private static bool inGame;

		private static TextMenu.Item crouchDashMode;

		private static TextMenu.Item window;

		private static TextMenu.Item viewport;

		private static EventInstance snapshot;

		public static TextMenu Create(bool inGame = false, EventInstance snapshot = null)
		{
			MenuOptions.inGame = inGame;
			MenuOptions.snapshot = snapshot;
			menu = new TextMenu();
			menu.Add(new TextMenu.Header(Dialog.Clean("options_title")));
			menu.OnClose = delegate
			{
				crouchDashMode = null;
			};
			if (!inGame && Dialog.Languages.Count > 1)
			{
				menu.Add(new TextMenu.SubHeader(""));
				TextMenu.LanguageButton languages = new TextMenu.LanguageButton(Dialog.Clean("options_language"), Dialog.Language);
				languages.Pressed(SelectLanguage);
				menu.Add(languages);
			}
			menu.Add(new TextMenu.SubHeader(Dialog.Clean("options_controls")));
			CreateRumble(menu);
			CreateGrabMode(menu);
			crouchDashMode = CreateCrouchDashMode(menu);
			menu.Add(new TextMenu.Button(Dialog.Clean("options_keyconfig")).Pressed(OpenKeyboardConfig));
			menu.Add(new TextMenu.Button(Dialog.Clean("options_btnconfig")).Pressed(OpenButtonConfig));
			menu.Add(new TextMenu.SubHeader(Dialog.Clean("options_video")));
			if (!Celeste.IsGGP)
			{
				menu.Add(new TextMenu.OnOff(Dialog.Clean("options_fullscreen"), Settings.Instance.Fullscreen).Change(SetFullscreen));
				menu.Add(window = new TextMenu.Slider(Dialog.Clean("options_window"), (int i) => i + "x", 3, 10, Settings.Instance.WindowScale).Change(SetWindow));
				menu.Add(new TextMenu.OnOff(Dialog.Clean("options_vsync"), Settings.Instance.VSync).Change(SetVSync));
			}
			menu.Add(new TextMenu.OnOff(Dialog.Clean("OPTIONS_DISABLE_FLASH"), Settings.Instance.DisableFlashes).Change(delegate(bool b)
			{
				Settings.Instance.DisableFlashes = b;
			}));
			menu.Add(new TextMenu.Slider(Dialog.Clean("OPTIONS_DISABLE_SHAKE"), (int i) => i switch
			{
				2 => Dialog.Clean("OPTIONS_RUMBLE_ON"), 
				1 => Dialog.Clean("OPTIONS_RUMBLE_HALF"), 
				_ => Dialog.Clean("OPTIONS_RUMBLE_OFF"), 
			}, 0, 2, (int)Settings.Instance.ScreenShake).Change(delegate(int i)
			{
				Settings.Instance.ScreenShake = (ScreenshakeAmount)i;
			}));
			menu.Add(viewport = new TextMenu.Button(Dialog.Clean("OPTIONS_VIEWPORT_PC")).Pressed(OpenViewportAdjustment));
			menu.Add(new TextMenu.SubHeader(Dialog.Clean("options_audio")));
			menu.Add(new TextMenu.Slider(Dialog.Clean("options_music"), (int i) => i.ToString(), 0, 10, Settings.Instance.MusicVolume).Change(SetMusic).Enter(EnterSound).Leave(LeaveSound));
			menu.Add(new TextMenu.Slider(Dialog.Clean("options_sounds"), (int i) => i.ToString(), 0, 10, Settings.Instance.SFXVolume).Change(SetSfx).Enter(EnterSound).Leave(LeaveSound));
			menu.Add(new TextMenu.SubHeader(Dialog.Clean("options_gameplay")));
			menu.Add(new TextMenu.Slider(Dialog.Clean("options_speedrun"), (int i) => i switch
			{
				0 => Dialog.Get("OPTIONS_OFF"), 
				1 => Dialog.Get("OPTIONS_SPEEDRUN_CHAPTER"), 
				_ => Dialog.Get("OPTIONS_SPEEDRUN_FILE"), 
			}, 0, 2, (int)Settings.Instance.SpeedrunClock).Change(SetSpeedrunClock));
			viewport.Visible = Settings.Instance.Fullscreen;
			if (window != null)
			{
				window.Visible = !Settings.Instance.Fullscreen;
			}
			if (menu.Height > menu.ScrollableMinSize)
			{
				menu.Position.Y = menu.ScrollTargetY;
			}
			return menu;
		}

		private static void CreateRumble(TextMenu menu)
		{
			menu.Add(new TextMenu.Slider(Dialog.Clean("options_rumble_PC"), (int i) => i switch
			{
				2 => Dialog.Clean("OPTIONS_RUMBLE_ON"), 
				1 => Dialog.Clean("OPTIONS_RUMBLE_HALF"), 
				_ => Dialog.Clean("OPTIONS_RUMBLE_OFF"), 
			}, 0, 2, (int)Settings.Instance.Rumble).Change(delegate(int i)
			{
				Settings.Instance.Rumble = (RumbleAmount)i;
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			}));
		}

		private static void CreateGrabMode(TextMenu menu)
		{
			menu.Add(new TextMenu.Slider(Dialog.Clean("OPTIONS_GRAB_MODE"), (int i) => i switch
			{
				0 => Dialog.Clean("OPTIONS_BUTTON_HOLD"), 
				1 => Dialog.Clean("OPTIONS_BUTTON_INVERT"), 
				_ => Dialog.Clean("OPTIONS_BUTTON_TOGGLE"), 
			}, 0, 2, (int)Settings.Instance.GrabMode).Change(delegate(int i)
			{
				Settings.Instance.GrabMode = (GrabModes)i;
				Input.ResetGrab();
			}));
		}

		private static TextMenu.Item CreateCrouchDashMode(TextMenu menu)
		{
			TextMenu.Option<int> item = new TextMenu.Slider(Dialog.Clean("OPTIONS_CROUCH_DASH_MODE"), (int i) => (i == 0) ? Dialog.Clean("OPTIONS_BUTTON_PRESS") : Dialog.Clean("OPTIONS_BUTTON_HOLD"), 0, 1, (int)Settings.Instance.CrouchDashMode).Change(delegate(int i)
			{
				Settings.Instance.CrouchDashMode = (CrouchDashModes)i;
			});
			item.Visible = Input.CrouchDash.Binding.HasInput;
			menu.Add(item);
			return item;
		}

		private static void SetFullscreen(bool on)
		{
			Settings.Instance.Fullscreen = on;
			Settings.Instance.ApplyScreen();
			if (window != null)
			{
				window.Visible = !on;
			}
			if (viewport != null)
			{
				viewport.Visible = on;
			}
		}

		private static void SetVSync(bool on)
		{
			Settings.Instance.VSync = on;
			Engine.Graphics.SynchronizeWithVerticalRetrace = Settings.Instance.VSync;
			Engine.Graphics.ApplyChanges();
		}

		private static void SetWindow(int scale)
		{
			Settings.Instance.WindowScale = scale;
			Settings.Instance.ApplyScreen();
		}

		private static void SetMusic(int volume)
		{
			Settings.Instance.MusicVolume = volume;
			Settings.Instance.ApplyMusicVolume();
		}

		private static void SetSfx(int volume)
		{
			Settings.Instance.SFXVolume = volume;
			Settings.Instance.ApplySFXVolume();
		}

		private static void SetSpeedrunClock(int val)
		{
			Settings.Instance.SpeedrunClock = (SpeedrunType)val;
		}

		private static void OpenViewportAdjustment()
		{
			if (Engine.Scene is Overworld)
			{
				(Engine.Scene as Overworld).ShowInputUI = false;
			}
			menu.Visible = false;
			menu.Focused = false;
			ViewportAdjustmentUI adjuster = new ViewportAdjustmentUI();
			adjuster.OnClose = delegate
			{
				menu.Visible = true;
				menu.Focused = true;
				if (Engine.Scene is Overworld)
				{
					(Engine.Scene as Overworld).ShowInputUI = true;
				}
			};
			Engine.Scene.Add(adjuster);
			Engine.Scene.OnEndOfFrame += delegate
			{
				Engine.Scene.Entities.UpdateLists();
			};
		}

		private static void SelectLanguage()
		{
			menu.Focused = false;
			LanguageSelectUI ui = new LanguageSelectUI();
			ui.OnClose = delegate
			{
				menu.Focused = true;
			};
			Engine.Scene.Add(ui);
			Engine.Scene.OnEndOfFrame += delegate
			{
				Engine.Scene.Entities.UpdateLists();
			};
		}

		private static void OpenKeyboardConfig()
		{
			menu.Focused = false;
			KeyboardConfigUI config = new KeyboardConfigUI();
			config.OnClose = delegate
			{
				menu.Focused = true;
			};
			Engine.Scene.Add(config);
			Engine.Scene.OnEndOfFrame += delegate
			{
				Engine.Scene.Entities.UpdateLists();
			};
		}

		private static void OpenButtonConfig()
		{
			menu.Focused = false;
			if (Engine.Scene is Overworld)
			{
				(Engine.Scene as Overworld).ShowConfirmUI = false;
			}
			ButtonConfigUI config = new ButtonConfigUI();
			config.OnClose = delegate
			{
				menu.Focused = true;
				if (Engine.Scene is Overworld)
				{
					(Engine.Scene as Overworld).ShowConfirmUI = true;
				}
			};
			Engine.Scene.Add(config);
			Engine.Scene.OnEndOfFrame += delegate
			{
				Engine.Scene.Entities.UpdateLists();
			};
		}

		private static void EnterSound()
		{
			if (inGame && snapshot != null)
			{
				Audio.EndSnapshot(snapshot);
			}
		}

		private static void LeaveSound()
		{
			if (inGame && snapshot != null)
			{
				Audio.ResumeSnapshot(snapshot);
			}
		}

		public static void UpdateCrouchDashModeVisibility()
		{
			if (crouchDashMode != null)
			{
				crouchDashMode.Visible = Input.CrouchDash.Binding.HasInput;
			}
		}
	}
}
