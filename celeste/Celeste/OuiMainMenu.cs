using System.Collections;
using System.Collections.Generic;
using Celeste.Pico8;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class OuiMainMenu : Oui
	{
		private static readonly Vector2 TargetPosition = new Vector2(160f, 160f);

		private static readonly Vector2 TweenFrom = new Vector2(-500f, 160f);

		private static readonly Color UnselectedColor = Color.White;

		private static readonly Color SelectedColorA = TextMenu.HighlightColorA;

		private static readonly Color SelectedColorB = TextMenu.HighlightColorB;

		private const float IconWidth = 64f;

		private const float IconSpacing = 20f;

		private float ease;

		private MainMenuClimb climbButton;

		private List<MenuButton> buttons;

		private bool startOnOptions;

		private bool mountainStartFront;

		public Color SelectionColor
		{
			get
			{
				if (!Settings.Instance.DisableFlashes && !base.Scene.BetweenInterval(0.1f))
				{
					return SelectedColorB;
				}
				return SelectedColorA;
			}
		}

		public OuiMainMenu()
		{
			buttons = new List<MenuButton>();
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Position = TweenFrom;
			CreateButtons();
		}

		public void CreateButtons()
		{
			foreach (MenuButton button in buttons)
			{
				button.RemoveSelf();
			}
			buttons.Clear();
			Vector2 at = new Vector2(320f, 160f);
			Vector2 tweenAdd = new Vector2(-640f, 0f);
			climbButton = new MainMenuClimb(this, at, at + tweenAdd, OnBegin);
			if (!startOnOptions)
			{
				climbButton.StartSelected();
			}
			buttons.Add(climbButton);
			at += Vector2.UnitY * climbButton.ButtonHeight;
			at.X -= 140f;
			if (Celeste.PlayMode == Celeste.PlayModes.Debug)
			{
				MainMenuSmallButton debug = new MainMenuSmallButton("menu_debug", "menu/options", this, at, at + tweenAdd, OnDebug);
				buttons.Add(debug);
				at += Vector2.UnitY * debug.ButtonHeight;
			}
			if (Settings.Instance.Pico8OnMainMenu || Celeste.PlayMode == Celeste.PlayModes.Debug || Celeste.PlayMode == Celeste.PlayModes.Event)
			{
				MainMenuSmallButton pico8 = new MainMenuSmallButton("menu_pico8", "menu/pico8", this, at, at + tweenAdd, OnPico8);
				buttons.Add(pico8);
				at += Vector2.UnitY * pico8.ButtonHeight;
			}
			MainMenuSmallButton options = new MainMenuSmallButton("menu_options", "menu/options", this, at, at + tweenAdd, OnOptions);
			if (startOnOptions)
			{
				options.StartSelected();
			}
			buttons.Add(options);
			at += Vector2.UnitY * options.ButtonHeight;
			MainMenuSmallButton credits = new MainMenuSmallButton("menu_credits", "menu/credits", this, at, at + tweenAdd, OnCredits);
			buttons.Add(credits);
			at += Vector2.UnitY * credits.ButtonHeight;
			if (!Celeste.IsGGP)
			{
				MainMenuSmallButton exit = new MainMenuSmallButton("menu_exit", "menu/exit", this, at, at + tweenAdd, OnExit);
				buttons.Add(exit);
				at += Vector2.UnitY * exit.ButtonHeight;
			}
			for (int i = 0; i < buttons.Count; i++)
			{
				if (i > 0)
				{
					buttons[i].UpButton = buttons[i - 1];
				}
				if (i < buttons.Count - 1)
				{
					buttons[i].DownButton = buttons[i + 1];
				}
				base.Scene.Add(buttons[i]);
			}
			if (!Visible || !Focused)
			{
				return;
			}
			foreach (MenuButton button2 in buttons)
			{
				button2.Position = button2.TargetPosition;
			}
		}

		public override void Removed(Scene scene)
		{
			foreach (MenuButton button in buttons)
			{
				scene.Remove(button);
			}
			base.Removed(scene);
		}

		public override bool IsStart(Overworld overworld, Overworld.StartMode start)
		{
			switch (start)
			{
			case Overworld.StartMode.ReturnFromOptions:
				startOnOptions = true;
				Add(new Coroutine(Enter(null)));
				return true;
			case Overworld.StartMode.MainMenu:
				mountainStartFront = true;
				Add(new Coroutine(Enter(null)));
				return true;
			default:
				if (start != Overworld.StartMode.ReturnFromOptions)
				{
					return start == Overworld.StartMode.ReturnFromPico8;
				}
				return true;
			}
		}

		public override IEnumerator Enter(Oui from)
		{
			if (from is OuiTitleScreen || from is OuiFileSelect)
			{
				Audio.Play("event:/ui/main/whoosh_list_in");
				yield return 0.1f;
			}
			if (from is OuiTitleScreen)
			{
				MenuButton.ClearSelection(base.Scene);
				climbButton.StartSelected();
			}
			Visible = true;
			if (mountainStartFront)
			{
				base.Overworld.Mountain.SnapCamera(-1, new MountainCamera(new Vector3(0f, 6f, 12f), MountainRenderer.RotateLookAt));
			}
			base.Overworld.Mountain.GotoRotationMode();
			base.Overworld.Maddy.Hide();
			foreach (MenuButton button in buttons)
			{
				button.TweenIn(0.2f);
			}
			yield return 0.2f;
			Focused = true;
			mountainStartFront = false;
			yield return null;
		}

		public override IEnumerator Leave(Oui next)
		{
			Focused = false;
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, 0.2f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				ease = 1f - t.Eased;
				Position = Vector2.Lerp(TargetPosition, TweenFrom, t.Eased);
			};
			Add(tween);
			bool keepClimb = climbButton.Selected && !(next is OuiTitleScreen);
			foreach (MenuButton button in buttons)
			{
				if (!(button == climbButton && keepClimb))
				{
					button.TweenOut(0.2f);
				}
			}
			yield return 0.2f;
			if (keepClimb)
			{
				Add(new Coroutine(SlideClimbOutLate()));
			}
			else
			{
				Visible = false;
			}
		}

		private IEnumerator SlideClimbOutLate()
		{
			yield return 0.2f;
			climbButton.TweenOut(0.2f);
			yield return 0.2f;
			Visible = false;
		}

		public override void Update()
		{
			if (base.Selected && Focused && Input.MenuCancel.Pressed)
			{
				Focused = false;
				Audio.Play("event:/ui/main/whoosh_list_out");
				Audio.Play("event:/ui/main/button_back");
				base.Overworld.Goto<OuiTitleScreen>();
			}
			base.Update();
		}

		public override void Render()
		{
			foreach (MenuButton button in buttons)
			{
				if (button.Scene == base.Scene)
				{
					button.Render();
				}
			}
		}

		private void OnDebug()
		{
			Audio.Play("event:/ui/main/whoosh_list_out");
			Audio.Play("event:/ui/main/button_select");
			SaveData.InitializeDebugMode();
			base.Overworld.Goto<OuiChapterSelect>();
		}

		private void OnBegin()
		{
			Audio.Play("event:/ui/main/whoosh_list_out");
			Audio.Play("event:/ui/main/button_climb");
			if (Celeste.PlayMode == Celeste.PlayModes.Event)
			{
				SaveData.InitializeDebugMode(loadExisting: false);
				base.Overworld.Goto<OuiChapterSelect>();
			}
			else
			{
				base.Overworld.Goto<OuiFileSelect>();
			}
		}

		private void OnPico8()
		{
			Audio.Play("event:/ui/main/button_select");
			Focused = false;
			new FadeWipe(base.Scene, wipeIn: false, delegate
			{
				Focused = true;
				base.Overworld.EnteringPico8 = true;
				SaveData.Instance = null;
				SaveData.NoFileAssistChecks();
				Engine.Scene = new Emulator(base.Overworld);
			});
		}

		private void OnOptions()
		{
			Audio.Play("event:/ui/main/button_select");
			Audio.Play("event:/ui/main/whoosh_large_in");
			base.Overworld.Goto<OuiOptions>();
		}

		private void OnCredits()
		{
			Audio.Play("event:/ui/main/button_select");
			Audio.Play("event:/ui/main/whoosh_large_in");
			base.Overworld.Goto<OuiCredits>();
		}

		private void OnExit()
		{
			Audio.Play("event:/ui/main/button_select");
			Focused = false;
			new FadeWipe(base.Scene, wipeIn: false, delegate
			{
				Engine.Scene = new Scene();
				Engine.Instance.Exit();
                Program.exitGame = true;
			});
		}
	}
}
