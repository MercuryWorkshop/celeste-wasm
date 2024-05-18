using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste
{
	public class InputMappingInfo : TextMenu.Item
	{
		private List<object> info = new List<object>();

		private bool controllerMode;

		private float borderEase;

		private bool fixedPosition;

		public InputMappingInfo(bool controllerMode)
		{
			string[] text = Dialog.Clean("BTN_CONFIG_INFO").Split('|');
			if (text.Length == 3)
			{
				info.Add(text[0]);
				info.Add(Input.MenuConfirm);
				info.Add(text[1]);
				info.Add(Input.MenuJournal);
				info.Add(text[2]);
			}
			this.controllerMode = controllerMode;
			AboveAll = true;
		}

		public override float LeftWidth()
		{
			return 100f;
		}

		public override float Height()
		{
			return ActiveFont.LineHeight * 2f;
		}

		public override void Update()
		{
			borderEase = Calc.Approach(borderEase, fixedPosition ? 1f : 0f, Engine.DeltaTime * 4f);
			base.Update();
		}

		public override void Render(Vector2 position, bool highlighted)
		{
			fixedPosition = false;
			if (position.Y < 100f)
			{
				fixedPosition = true;
				position.Y = 100f;
			}
			Color textColor = Color.Gray * Ease.CubeOut(Container.Alpha);
			Color textShadow = Color.Black * Ease.CubeOut(Container.Alpha);
			Color btnColor = Color.White * Ease.CubeOut(Container.Alpha);
			float width = 0f;
			for (int j = 0; j < info.Count; j++)
			{
				if (info[j] is string)
				{
					string str = info[j] as string;
					width += ActiveFont.Measure(str).X * 0.6f;
				}
				else if (info[j] is VirtualButton)
				{
					VirtualButton vbtn = info[j] as VirtualButton;
					if (controllerMode)
					{
						MTexture btn3 = Input.GuiButton(vbtn, Input.PrefixMode.Attached);
						width += (float)btn3.Width * 0.6f;
					}
					else if (vbtn.Binding.Keyboard.Count > 0)
					{
						MTexture btn2 = Input.GuiKey(vbtn.Binding.Keyboard[0]);
						width += (float)btn2.Width * 0.6f;
					}
					else
					{
						MTexture btn = Input.GuiKey(Keys.None);
						width += (float)btn.Width * 0.6f;
					}
				}
			}
			Vector2 pos = position + new Vector2(Container.Width - width, 0f) / 2f;
			if (borderEase > 0f)
			{
				Draw.HollowRect(pos.X - 22f, pos.Y - 42f, width + 44f, 84f, Color.White * Ease.CubeOut(Container.Alpha) * borderEase);
				Draw.HollowRect(pos.X - 21f, pos.Y - 41f, width + 42f, 82f, Color.White * Ease.CubeOut(Container.Alpha) * borderEase);
				Draw.Rect(pos.X - 20f, pos.Y - 40f, width + 40f, 80f, Color.Black * Ease.CubeOut(Container.Alpha));
			}
			for (int i = 0; i < info.Count; i++)
			{
				if (info[i] is string)
				{
					string str2 = info[i] as string;
					ActiveFont.DrawOutline(str2, pos, new Vector2(0f, 0.5f), Vector2.One * 0.6f, textColor, 2f, textShadow);
					pos.X += ActiveFont.Measure(str2).X * 0.6f;
				}
				else if (info[i] is VirtualButton)
				{
					VirtualButton vbtn2 = info[i] as VirtualButton;
					if (controllerMode)
					{
						MTexture btn6 = Input.GuiButton(vbtn2, Input.PrefixMode.Attached);
						btn6.DrawJustified(pos, new Vector2(0f, 0.5f), btnColor, 0.6f);
						pos.X += (float)btn6.Width * 0.6f;
					}
					else if (vbtn2.Binding.Keyboard.Count > 0)
					{
						MTexture btn5 = Input.GuiKey(vbtn2.Binding.Keyboard[0]);
						btn5.DrawJustified(pos, new Vector2(0f, 0.5f), btnColor, 0.6f);
						pos.X += (float)btn5.Width * 0.6f;
					}
					else
					{
						MTexture btn4 = Input.GuiKey(Keys.None);
						btn4.DrawJustified(pos, new Vector2(0f, 0.5f), btnColor, 0.6f);
						pos.X += (float)btn4.Width * 0.6f;
					}
				}
			}
		}
	}
}
