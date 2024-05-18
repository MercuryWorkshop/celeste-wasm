using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class ViewportAdjustmentUI : Entity
	{
		private const float minPadding = 0f;

		private const float maxPadding = 128f;

		private readonly float originalPadding = Engine.ViewPadding;

		private float viewPadding = Engine.ViewPadding;

		private float inputDelay;

		private bool closing;

		private bool canceling;

		private float leftAlpha = 1f;

		private float rightAlpha = 1f;

		public Action OnClose;

		public float Alpha { get; private set; }

		public bool Open { get; private set; }

		public ViewportAdjustmentUI()
		{
			Open = true;
			base.Tag = (int)Tags.HUD | (int)Tags.PauseUpdate;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (scene is Overworld)
			{
				(scene as Overworld).Mountain.Model.LockBufferResizing = true;
			}
		}

		public override void Removed(Scene scene)
		{
			if (scene is Overworld)
			{
				(scene as Overworld).Mountain.Model.LockBufferResizing = false;
			}
			base.Removed(scene);
		}

		public override void Update()
		{
			base.Update();
			if (!closing)
			{
				inputDelay += Engine.RawDeltaTime;
				if (inputDelay > 0.25f)
				{
					if (Input.MenuCancel.Pressed || Input.ESC.Pressed)
					{
						closing = (canceling = true);
					}
					else if (Input.MenuConfirm.Pressed)
					{
						closing = true;
					}
				}
			}
			else if (Alpha <= 0f)
			{
				if (canceling)
				{
					Engine.ViewPadding = (int)originalPadding;
				}
				else
				{
					Settings.Instance.ViewportPadding = (int)viewPadding;
				}
				Settings.Instance.SetViewportOnce = true;
				Open = false;
				RemoveSelf();
				if (OnClose != null)
				{
					OnClose();
				}
				return;
			}
			Alpha = Calc.Approach(Alpha, (!closing) ? 1 : 0, Engine.RawDeltaTime * 4f);
			viewPadding -= Input.Aim.Value.X * 48f * Engine.RawDeltaTime;
			viewPadding = Calc.Clamp(viewPadding, 0f, 128f);
			leftAlpha = Calc.Approach(leftAlpha, (viewPadding < 128f) ? 1f : 0.25f, Engine.DeltaTime * 4f);
			rightAlpha = Calc.Approach(rightAlpha, (viewPadding > 0f) ? 1f : 0.25f, Engine.DeltaTime * 4f);
			Engine.ViewPadding = (int)viewPadding;
		}

		public override void Render()
		{
			float a = Ease.SineInOut(Alpha);
			Color black = Color.Black * 0.75f * a;
			Color white = Color.White * a;
			if (!(base.Scene is Level))
			{
				Draw.Rect(-1f, -1f, Engine.Width + 2, Engine.Height + 2, black);
			}
			Draw.Rect(0f, 0f, Engine.Width, 16f, white);
			Draw.Rect(0f, 16f, 16f, Engine.Height - 32, white);
			Draw.Rect(Engine.Width - 16, 16f, 16f, Engine.Height - 32, white);
			Draw.Rect(0f, Engine.Height - 16, Engine.Width, 16f, white);
			Draw.LineAngle(new Vector2(8f, 8f), (float)Math.PI / 4f, 128f, white, 16f);
			Draw.LineAngle(new Vector2(Engine.Width - 8, 8f), (float)Math.PI * 3f / 4f, 128f, white, 16f);
			Draw.LineAngle(new Vector2(8f, Engine.Height - 8), -(float)Math.PI / 4f, 128f, white, 16f);
			Draw.LineAngle(new Vector2(Engine.Width - 8, Engine.Height - 8), (float)Math.PI * -3f / 4f, 128f, white, 16f);
			string text = Dialog.Clean("OPTIONS_VIEWPORT_PC");
			ActiveFont.Measure(text);
			float sin = (float)Math.Sin(base.Scene.RawTimeActive * 2f) * 16f;
			Vector2 center = new Vector2(Engine.Width, Engine.Height) * 0.5f;
			ActiveFont.Draw(text, center + new Vector2(0f, -60f), new Vector2(0.5f, 0.5f), Vector2.One * 1.2f, white);
			float confirmWidth = ButtonUI.Width(Dialog.Clean("ui_confirm"), Input.MenuConfirm) * 0.8f;
			ButtonUI.Render(center + new Vector2(0f, 60f), Dialog.Clean("ui_confirm"), Input.MenuConfirm, 0.8f, 0.5f, 0f, a);
			Vector2 rightPosition = center + new Vector2(confirmWidth * 0.6f + 80f + sin, 60f);
			GFX.Gui["adjustarrowright"].DrawCentered(rightPosition + new Vector2(8f, 4f), white * rightAlpha, Vector2.One);
			Vector2 leftPosition = center + new Vector2(0f - (confirmWidth * 0.6f + 80f + sin), 60f);
			GFX.Gui["adjustarrowleft"].DrawCentered(leftPosition + new Vector2(-8f, 4f), white * leftAlpha, Vector2.One);
		}
	}
}
