using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste
{
	[Serializable]
	public class Settings
	{
		public static Settings Instance;

		public static bool Existed;

		public static string LastVersion;

		public const string EnglishLanguage = "english";

		public string Version;

		public string DefaultFileName = "";

		public bool Fullscreen;

		public int WindowScale = 6;

		public int ViewportPadding;

		public bool VSync = true;

		public bool DisableFlashes;

		public ScreenshakeAmount ScreenShake = ScreenshakeAmount.Half;

		public RumbleAmount Rumble = RumbleAmount.On;

		public GrabModes GrabMode;

		public CrouchDashModes CrouchDashMode;

		public int MusicVolume = 10;

		public int SFXVolume = 10;

		public SpeedrunType SpeedrunClock;

		public int LastSaveFile;

		public string Language = "english";

		public bool Pico8OnMainMenu;

		public bool SetViewportOnce;

		public bool VariantsUnlocked;

		public Binding Left = new Binding();

		public Binding Right = new Binding();

		public Binding Down = new Binding();

		public Binding Up = new Binding();

		public Binding MenuLeft = new Binding();

		public Binding MenuRight = new Binding();

		public Binding MenuDown = new Binding();

		public Binding MenuUp = new Binding();

		public Binding Grab = new Binding();

		public Binding Jump = new Binding();

		public Binding Dash = new Binding();

		public Binding Talk = new Binding();

		public Binding Pause = new Binding();

		public Binding Confirm = new Binding();

		public Binding Cancel = new Binding();

		public Binding Journal = new Binding();

		public Binding QuickRestart = new Binding();

		public Binding DemoDash = new Binding();

		public Binding RightMoveOnly = new Binding();

		public Binding LeftMoveOnly = new Binding();

		public Binding UpMoveOnly = new Binding();

		public Binding DownMoveOnly = new Binding();

		public Binding RightDashOnly = new Binding();

		public Binding LeftDashOnly = new Binding();

		public Binding UpDashOnly = new Binding();

		public Binding DownDashOnly = new Binding();

		public bool LaunchWithFMODLiveUpdate;

		public bool LaunchInDebugMode;

		public const string Filename = "settings";

		[XmlAnyElement("LaunchInDebugModeComment")]
		public XmlComment DebugModeComment
		{
			get
			{
				return new XmlDocument().CreateComment("\n\t\tLaunchWithFMODLiveUpdate:\n\t\t\tThis Enables FMOD Studio Live Update so you can interact with the sounds in real time.\n\t\t\tNote this will also require access to the private network.\n\t\t\n\t\tLaunchInDebugMode:\n\t\t\tDebug Mode can destroy save files, crash the game, and do other unwanted behaviour.\n\t\t\tIt is not documented. Use at own risk.\n\t");
			}
			set
			{
			}
		}

		public int MaxScale => Math.Min(Engine.Instance.GraphicsDevice.Adapter.CurrentDisplayMode.Width / 320, Engine.Instance.GraphicsDevice.Adapter.CurrentDisplayMode.Height / 180);

		public Settings()
		{
			if (Celeste.PlayMode != Celeste.PlayModes.Debug)
			{
				Fullscreen = true;
			}
			if (Celeste.IsGGP)
			{
				Fullscreen = false;
				VSync = false;
			}
		}

		public void AfterLoad()
		{
			Binding.SetExclusive(MenuLeft, MenuRight, MenuUp, MenuDown, Confirm, Cancel, Journal, Pause);
			MusicVolume = Calc.Clamp(MusicVolume, 0, 10);
			SFXVolume = Calc.Clamp(SFXVolume, 0, 10);
			WindowScale = Math.Min(WindowScale, MaxScale);
			WindowScale = Calc.Clamp(WindowScale, 3, 10);
			SetDefaultKeyboardControls(reset: false);
			SetDefaultButtonControls(reset: false);
			if (LaunchInDebugMode)
			{
				Celeste.PlayMode = Celeste.PlayModes.Debug;
			}
			LastVersion = (Existed ? Instance.Version : Celeste.Instance.Version.ToString());
			Instance.Version = Celeste.Instance.Version.ToString();
		}

		public void SetDefaultKeyboardControls(bool reset)
		{
			if (reset || Left.Keyboard.Count <= 0)
			{
				Left.Keyboard.Clear();
				Left.Add(Keys.Left);
			}
			if (reset || Right.Keyboard.Count <= 0)
			{
				Right.Keyboard.Clear();
				Right.Add(Keys.Right);
			}
			if (reset || Down.Keyboard.Count <= 0)
			{
				Down.Keyboard.Clear();
				Down.Add(Keys.Down);
			}
			if (reset || Up.Keyboard.Count <= 0)
			{
				Up.Keyboard.Clear();
				Up.Add(Keys.Up);
			}
			if (reset || MenuLeft.Keyboard.Count <= 0)
			{
				MenuLeft.Keyboard.Clear();
				MenuLeft.Add(Keys.Left);
			}
			if (reset || MenuRight.Keyboard.Count <= 0)
			{
				MenuRight.Keyboard.Clear();
				MenuRight.Add(Keys.Right);
			}
			if (reset || MenuDown.Keyboard.Count <= 0)
			{
				MenuDown.Keyboard.Clear();
				MenuDown.Add(Keys.Down);
			}
			if (reset || MenuUp.Keyboard.Count <= 0)
			{
				MenuUp.Keyboard.Clear();
				MenuUp.Add(Keys.Up);
			}
			if (reset || Grab.Keyboard.Count <= 0)
			{
				Grab.Keyboard.Clear();
				Grab.Add(Keys.Z, Keys.V, Keys.LeftShift);
			}
			if (reset || Jump.Keyboard.Count <= 0)
			{
				Jump.Keyboard.Clear();
				Jump.Add(Keys.C);
			}
			if (reset || Dash.Keyboard.Count <= 0)
			{
				Dash.Keyboard.Clear();
				Dash.Add(Keys.X);
			}
			if (reset || Talk.Keyboard.Count <= 0)
			{
				Talk.Keyboard.Clear();
				Talk.Add(Keys.X);
			}
			if (reset || Pause.Keyboard.Count <= 0)
			{
				Pause.Keyboard.Clear();
				Pause.Add(Keys.Enter);
			}
			if (reset || Confirm.Keyboard.Count <= 0)
			{
				Confirm.Keyboard.Clear();
				Confirm.Add(Keys.C);
			}
			if (reset || Cancel.Keyboard.Count <= 0)
			{
				Cancel.Keyboard.Clear();
				Cancel.Add(Keys.X, Keys.Back);
			}
			if (reset || Journal.Keyboard.Count <= 0)
			{
				Journal.Keyboard.Clear();
				Journal.Add(Keys.Tab);
			}
			if (reset || QuickRestart.Keyboard.Count <= 0)
			{
				QuickRestart.Keyboard.Clear();
				QuickRestart.Add(Keys.R);
			}
			if (reset)
			{
				DemoDash.Keyboard.Clear();
				LeftMoveOnly.Keyboard.Clear();
				RightMoveOnly.Keyboard.Clear();
				UpMoveOnly.Keyboard.Clear();
				DownMoveOnly.Keyboard.Clear();
				LeftDashOnly.Keyboard.Clear();
				RightDashOnly.Keyboard.Clear();
				UpDashOnly.Keyboard.Clear();
				DownDashOnly.Keyboard.Clear();
			}
			TranslateKeys(Left.Keyboard);
			TranslateKeys(Right.Keyboard);
			TranslateKeys(Down.Keyboard);
			TranslateKeys(Up.Keyboard);
			TranslateKeys(MenuLeft.Keyboard);
			TranslateKeys(MenuRight.Keyboard);
			TranslateKeys(MenuDown.Keyboard);
			TranslateKeys(MenuUp.Keyboard);
			TranslateKeys(Grab.Keyboard);
			TranslateKeys(Jump.Keyboard);
			TranslateKeys(Dash.Keyboard);
			TranslateKeys(Talk.Keyboard);
			TranslateKeys(Pause.Keyboard);
			TranslateKeys(Confirm.Keyboard);
			TranslateKeys(Cancel.Keyboard);
			TranslateKeys(Journal.Keyboard);
			TranslateKeys(QuickRestart.Keyboard);
			TranslateKeys(DemoDash.Keyboard);
			TranslateKeys(LeftMoveOnly.Keyboard);
			TranslateKeys(RightMoveOnly.Keyboard);
			TranslateKeys(UpMoveOnly.Keyboard);
			TranslateKeys(DownMoveOnly.Keyboard);
			TranslateKeys(LeftDashOnly.Keyboard);
			TranslateKeys(RightDashOnly.Keyboard);
			TranslateKeys(UpDashOnly.Keyboard);
			TranslateKeys(DownDashOnly.Keyboard);
		}

		private static void TranslateKeys(List<Keys> keys)
		{
			for (int i = 0; i < keys.Count; i++)
			{
				keys[i] = Keyboard.GetKeyFromScancodeEXT(keys[i]);
			}
		}

		public void SetDefaultButtonControls(bool reset)
		{
			if (reset || Left.Controller.Count <= 0)
			{
				Left.Controller.Clear();
				Left.Add(Buttons.LeftThumbstickLeft, Buttons.DPadLeft);
			}
			if (reset || Right.Controller.Count <= 0)
			{
				Right.Controller.Clear();
				Right.Add(Buttons.LeftThumbstickRight, Buttons.DPadRight);
			}
			if (reset || Down.Controller.Count <= 0)
			{
				Down.Controller.Clear();
				Down.Add(Buttons.LeftThumbstickDown, Buttons.DPadDown);
			}
			if (reset || Up.Controller.Count <= 0)
			{
				Up.Controller.Clear();
				Up.Add(Buttons.LeftThumbstickUp, Buttons.DPadUp);
			}
			if (reset || MenuLeft.Controller.Count <= 0)
			{
				MenuLeft.Controller.Clear();
				MenuLeft.Add(Buttons.LeftThumbstickLeft, Buttons.DPadLeft);
			}
			if (reset || MenuRight.Controller.Count <= 0)
			{
				MenuRight.Controller.Clear();
				MenuRight.Add(Buttons.LeftThumbstickRight, Buttons.DPadRight);
			}
			if (reset || MenuDown.Controller.Count <= 0)
			{
				MenuDown.Controller.Clear();
				MenuDown.Add(Buttons.LeftThumbstickDown, Buttons.DPadDown);
			}
			if (reset || MenuUp.Controller.Count <= 0)
			{
				MenuUp.Controller.Clear();
				MenuUp.Add(Buttons.LeftThumbstickUp, Buttons.DPadUp);
			}
			if (reset || Grab.Controller.Count <= 0)
			{
				Grab.Controller.Clear();
				Grab.Add(Buttons.LeftTrigger, Buttons.RightTrigger, Buttons.LeftShoulder, Buttons.RightShoulder);
			}
			if (reset || Jump.Controller.Count <= 0)
			{
				Jump.Controller.Clear();
				Jump.Add(Buttons.A, Buttons.Y);
			}
			if (reset || Dash.Controller.Count <= 0)
			{
				Dash.Controller.Clear();
				Dash.Add(Buttons.X, Buttons.B);
			}
			if (reset || Talk.Controller.Count <= 0)
			{
				Talk.Controller.Clear();
				Talk.Add(Buttons.B);
			}
			if (reset || Pause.Controller.Count <= 0)
			{
				Pause.Controller.Clear();
				Pause.Add(Buttons.Start);
			}
			if (reset || Confirm.Controller.Count <= 0)
			{
				Confirm.Controller.Clear();
				Confirm.Add(Buttons.A);
			}
			if (reset || Cancel.Controller.Count <= 0)
			{
				Cancel.Controller.Clear();
				Cancel.Add(Buttons.B, Buttons.X);
			}
			if (reset || Journal.Controller.Count <= 0)
			{
				Journal.Controller.Clear();
				Journal.Add(Buttons.LeftTrigger);
			}
			if (reset || QuickRestart.Controller.Count <= 0)
			{
				QuickRestart.Controller.Clear();
			}
			if (reset)
			{
				DemoDash.Controller.Clear();
				LeftMoveOnly.Controller.Clear();
				RightMoveOnly.Controller.Clear();
				UpMoveOnly.Controller.Clear();
				DownMoveOnly.Controller.Clear();
				LeftDashOnly.Controller.Clear();
				RightDashOnly.Controller.Clear();
				UpDashOnly.Controller.Clear();
				DownDashOnly.Controller.Clear();
			}
		}

		public void ApplyVolumes()
		{
			ApplySFXVolume();
			ApplyMusicVolume();
		}

		public void ApplySFXVolume()
		{
			Audio.SfxVolume = (float)SFXVolume / 10f;
		}

		public void ApplyMusicVolume()
		{
			Audio.MusicVolume = (float)MusicVolume / 10f;
		}

		public void ApplyScreen()
		{
			if (Fullscreen)
			{
				Engine.ViewPadding = ViewportPadding;
				Engine.SetFullscreen();
			}
			else
			{
				Engine.ViewPadding = 0;
				Engine.SetWindowed(320 * WindowScale, 180 * WindowScale);
			}
		}

		public void ApplyLanguage()
		{
			if (!Dialog.Languages.ContainsKey(Language))
			{
				Language = "english";
			}
			Dialog.Language = Dialog.Languages[Language];
			Fonts.Load(Dialog.Languages[Language].FontFace);
		}

		public static void Initialize()
		{
			if (UserIO.Open(UserIO.Mode.Read))
			{
				Instance = UserIO.Load<Settings>("settings");
				UserIO.Close();
			}
			Existed = Instance != null;
			if (Instance == null)
			{
				Instance = new Settings();
			}
		}

		public static void Reload()
		{
			Initialize();
			Instance.AfterLoad();
			Instance.ApplyVolumes();
			Instance.ApplyScreen();
			Instance.ApplyLanguage();
			if (Engine.Scene is Overworld)
			{
				(Engine.Scene as Overworld).GetUI<OuiMainMenu>()?.CreateButtons();
			}
		}
	}
}
