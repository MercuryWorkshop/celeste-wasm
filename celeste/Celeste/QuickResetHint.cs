using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste
{
	public class QuickResetHint : Entity
	{
		private string textStart;

		private string textHold;

		private string textPress;

		private List<object> controllerList;

		private List<object> keyboardList;

		public QuickResetHint()
		{
			base.Tag = Tags.HUD;
			Buttons L = Buttons.LeftShoulder;
			Buttons R = Buttons.RightShoulder;
			textStart = Dialog.Clean("UI_QUICK_RESTART_TITLE") + " ";
			textHold = Dialog.Clean("UI_QUICK_RESTART_HOLD");
			textPress = Dialog.Clean("UI_QUICK_RESTART_PRESS");
			if (Settings.Instance.Language == "japanese")
			{
				controllerList = new List<object>
				{
					textStart,
					L,
					R,
					textHold,
					"、",
					Input.FirstButton(Input.Pause),
					textPress
				};
				keyboardList = new List<object>
				{
					textStart,
					Input.FirstKey(Input.QuickRestart),
					textPress
				};
			}
			else
			{
				controllerList = new List<object>
				{
					textStart,
					textHold,
					L,
					R,
					",  ",
					textPress,
					Input.FirstButton(Input.Pause)
				};
				keyboardList = new List<object>
				{
					textStart,
					textPress,
					Input.FirstKey(Input.QuickRestart)
				};
			}
		}

		public override void Render()
		{
			List<object> list = (Input.GuiInputController() ? controllerList : keyboardList);
			float width = 0f;
			foreach (object obj2 in list)
			{
				if (obj2 is string)
				{
					width += ActiveFont.Measure(obj2 as string).X;
				}
				else if (obj2 is Buttons)
				{
					width += (float)Input.GuiSingleButton((Buttons)obj2).Width + 16f;
				}
				else if (obj2 is Keys)
				{
					width += (float)Input.GuiKey((Keys)obj2).Width + 16f;
				}
			}
			width *= 0.75f;
			Vector2 pos = new Vector2((1920f - width) / 2f, 980f);
			foreach (object obj in list)
			{
				if (obj is string)
				{
					ActiveFont.DrawOutline(obj as string, pos, new Vector2(0f, 0.5f), Vector2.One * 0.75f, Color.LightGray, 2f, Color.Black);
					pos.X += ActiveFont.Measure(obj as string).X * 0.75f;
				}
				else if (obj is Buttons)
				{
					MTexture gui2 = Input.GuiSingleButton((Buttons)obj);
					gui2.DrawJustified(pos + new Vector2(((float)gui2.Width + 16f) * 0.75f * 0.5f, 0f), new Vector2(0.5f, 0.5f), Color.White, 0.75f);
					pos.X += ((float)gui2.Width + 16f) * 0.75f;
				}
				else if (obj is Keys)
				{
					MTexture gui = Input.GuiKey((Keys)obj);
					gui.DrawJustified(pos + new Vector2(((float)gui.Width + 16f) * 0.75f * 0.5f, 0f), new Vector2(0.5f, 0.5f), Color.White, 0.75f);
					pos.X += ((float)gui.Width + 16f) * 0.75f;
				}
			}
		}
	}
}
