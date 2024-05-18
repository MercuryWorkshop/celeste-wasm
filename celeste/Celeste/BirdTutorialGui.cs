using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class BirdTutorialGui : Entity
	{
		public enum ButtonPrompt
		{
			Dash,
			Jump,
			Grab,
			Talk
		}

		public Entity Entity;

		public bool Open;

		public float Scale;

		private object info;

		private List<object> controls;

		private float controlsWidth;

		private float infoWidth;

		private float infoHeight;

		private float buttonPadding = 8f;

		private Color bgColor = Calc.HexToColor("061526");

		private Color lineColor = new Color(1f, 1f, 1f);

		private Color textColor = Calc.HexToColor("6179e2");

		public BirdTutorialGui(Entity entity, Vector2 position, object info, params object[] controls)
		{
			AddTag(Tags.HUD);
			Entity = entity;
			Position = position;
			this.info = info;
			this.controls = new List<object>(controls);
			if (info is string)
			{
				infoWidth = ActiveFont.Measure((string)info).X;
				infoHeight = ActiveFont.LineHeight;
			}
			else if (info is MTexture)
			{
				infoWidth = ((MTexture)info).Width;
				infoHeight = ((MTexture)info).Height;
			}
			UpdateControlsSize();
		}

		public void UpdateControlsSize()
		{
			controlsWidth = 0f;
			foreach (object obj in controls)
			{
				if (obj is ButtonPrompt)
				{
					controlsWidth += (float)Input.GuiButton(ButtonPromptToVirtualButton((ButtonPrompt)obj)).Width + buttonPadding * 2f;
				}
				else if (obj is Vector2)
				{
					controlsWidth += (float)Input.GuiDirection((Vector2)obj).Width + buttonPadding * 2f;
				}
				else if (obj is string)
				{
					controlsWidth += ActiveFont.Measure(obj.ToString()).X;
				}
				else if (obj is MTexture)
				{
					controlsWidth += ((MTexture)obj).Width;
				}
			}
		}

		public override void Update()
		{
			UpdateControlsSize();
			Scale = Calc.Approach(Scale, Open ? 1 : 0, Engine.RawDeltaTime * 8f);
			base.Update();
		}

		public override void Render()
		{
			Level level = base.Scene as Level;
			if (level.FrozenOrPaused || level.RetryPlayerCorpse != null || Scale <= 0f)
			{
				return;
			}
			Camera camera = SceneAs<Level>().Camera;
			Vector2 p = Entity.Position + Position - camera.Position.Floor();
			if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
			{
				p.X = 320f - p.X;
			}
			p.X *= 6f;
			p.Y *= 6f;
			float lineHeight = ActiveFont.LineHeight;
			float width = (Math.Max(controlsWidth, infoWidth) + 64f) * Scale;
			float height = infoHeight + lineHeight + 32f;
			float num = p.X - width / 2f;
			float top = p.Y - height - 32f;
			Draw.Rect(num - 6f, top - 6f, width + 12f, height + 12f, lineColor);
			Draw.Rect(num, top, width, height, bgColor);
			for (int i = 0; i <= 36; i++)
			{
				float w = (float)(73 - i * 2) * Scale;
				Draw.Rect(p.X - w / 2f, top + height + (float)i, w, 1f, lineColor);
				if (w > 12f)
				{
					Draw.Rect(p.X - w / 2f + 6f, top + height + (float)i, w - 12f, 1f, bgColor);
				}
			}
			if (!(width > 3f))
			{
				return;
			}
			Vector2 pos = new Vector2(p.X, top + 16f);
			if (info is string)
			{
				ActiveFont.Draw((string)info, pos, new Vector2(0.5f, 0f), new Vector2(Scale, 1f), textColor);
			}
			else if (info is MTexture)
			{
				((MTexture)info).DrawJustified(pos, new Vector2(0.5f, 0f), Color.White, new Vector2(Scale, 1f));
			}
			pos.Y += infoHeight + lineHeight * 0.5f;
			Vector2 orig = new Vector2((0f - controlsWidth) / 2f, 0f);
			foreach (object obj in controls)
			{
				if (obj is ButtonPrompt)
				{
					MTexture texture3 = Input.GuiButton(ButtonPromptToVirtualButton((ButtonPrompt)obj));
					orig.X += buttonPadding;
					texture3.Draw(pos, new Vector2(0f - orig.X, texture3.Height / 2), Color.White, new Vector2(Scale, 1f));
					orig.X += (float)texture3.Width + buttonPadding;
				}
				else if (obj is Vector2 dir)
				{
					if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
					{
						dir.X = 0f - dir.X;
					}
					MTexture texture2 = Input.GuiDirection(dir);
					orig.X += buttonPadding;
					texture2.Draw(pos, new Vector2(0f - orig.X, texture2.Height / 2), Color.White, new Vector2(Scale, 1f));
					orig.X += (float)texture2.Width + buttonPadding;
				}
				else if (obj is string)
				{
					string text = obj.ToString();
					float lineWidth = ActiveFont.Measure(text).X;
					ActiveFont.Draw(text, pos + new Vector2(1f, 2f), new Vector2((0f - orig.X) / lineWidth, 0.5f), new Vector2(Scale, 1f), textColor);
					ActiveFont.Draw(text, pos + new Vector2(1f, -2f), new Vector2((0f - orig.X) / lineWidth, 0.5f), new Vector2(Scale, 1f), Color.White);
					orig.X += lineWidth + 1f;
				}
				else if (obj is MTexture)
				{
					MTexture texture = (MTexture)obj;
					texture.Draw(pos, new Vector2(0f - orig.X, texture.Height / 2), Color.White, new Vector2(Scale, 1f));
					orig.X += texture.Width;
				}
			}
		}

		public static VirtualButton ButtonPromptToVirtualButton(ButtonPrompt prompt)
		{
			return prompt switch
			{
				ButtonPrompt.Dash => Input.Dash, 
				ButtonPrompt.Jump => Input.Jump, 
				ButtonPrompt.Grab => Input.Grab, 
				ButtonPrompt.Talk => Input.Talk, 
				_ => Input.Jump, 
			};
		}
	}
}
