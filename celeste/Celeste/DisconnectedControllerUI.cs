using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class DisconnectedControllerUI
	{
		private float fade;

		private bool closing;

		public DisconnectedControllerUI()
		{
			Celeste.DisconnectUI = this;
			Engine.OverloadGameLoop = Update;
		}

		private void OnClose()
		{
			Celeste.DisconnectUI = null;
			Engine.OverloadGameLoop = null;
		}

		public void Update()
		{
			bool disabled = MInput.Disabled;
			MInput.Disabled = false;
			fade = Calc.Approach(fade, (!closing) ? 1 : 0, Engine.DeltaTime * 8f);
			if (!closing)
			{
				int gamepadIndex = -1;
				if (Input.AnyGamepadConfirmPressed(out gamepadIndex))
				{
					Input.Gamepad = gamepadIndex;
					closing = true;
				}
			}
			else if (fade <= 0f)
			{
				OnClose();
			}
			MInput.Disabled = disabled;
		}

		public void Render()
		{
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Engine.ScreenMatrix);
			Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * fade * 0.8f);
			ActiveFont.DrawOutline(Dialog.Clean("XB1_RECONNECT_CONTROLLER"), Celeste.TargetCenter, new Vector2(0.5f, 0.5f), Vector2.One, Color.White * fade, 2f, Color.Black * fade * fade);
			Input.GuiButton(Input.MenuConfirm).DrawCentered(Celeste.TargetCenter + new Vector2(0f, 128f), Color.White * fade);
			Draw.SpriteBatch.End();
		}

		private static bool IsGamepadConnected()
		{
			int index = Input.Gamepad;
			return MInput.GamePads[index].Attached;
		}

		private static bool RequiresGamepad()
		{
			if (Engine.Scene == null || Engine.Scene is GameLoader || Engine.Scene is OverworldLoader)
			{
				return false;
			}
			if (Engine.Scene is Overworld overworld && overworld.Current is OuiTitleScreen)
			{
				return false;
			}
			return true;
		}

		public static void CheckGamepadDisconnect()
		{
			if (Celeste.DisconnectUI == null && RequiresGamepad() && !IsGamepadConnected())
			{
				new DisconnectedControllerUI();
			}
		}
	}
}
