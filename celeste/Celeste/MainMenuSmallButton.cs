using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class MainMenuSmallButton : MenuButton
	{
		private const float IconWidth = 64f;

		private const float IconSpacing = 20f;

		private const float MaxLabelWidth = 400f;

		private MTexture icon;

		private string label;

		private float labelScale;

		private Wiggler wiggler;

		private float ease;

		public override float ButtonHeight => ActiveFont.LineHeight * 1.25f;

		public MainMenuSmallButton(string labelName, string iconName, Oui oui, Vector2 targetPosition, Vector2 tweenFrom, Action onConfirm)
			: base(oui, targetPosition, tweenFrom, onConfirm)
		{
			label = Dialog.Clean(labelName);
			icon = GFX.Gui[iconName];
			labelScale = 1f;
			float labelWidth = ActiveFont.Measure(label).X;
			if (labelWidth > 400f)
			{
				labelScale = 400f / labelWidth;
			}
			Add(wiggler = Wiggler.Create(0.25f, 4f));
		}

		public override void Update()
		{
			base.Update();
			ease = Calc.Approach(ease, base.Selected ? 1 : 0, 6f * Engine.DeltaTime);
		}

		public override void Render()
		{
			base.Render();
			float scale = 64f / (float)icon.Width;
			Vector2 offset = new Vector2(Ease.CubeInOut(ease) * 32f, ActiveFont.LineHeight / 2f + wiggler.Value * 8f);
			icon.DrawOutlineJustified(Position + offset, new Vector2(0f, 0.5f), Color.White, scale);
			ActiveFont.DrawOutline(label, Position + offset + new Vector2(84f, 0f), new Vector2(0f, 0.5f), Vector2.One * labelScale, base.SelectionColor, 2f, Color.Black);
		}

		public override void OnSelect()
		{
			wiggler.Start();
		}
	}
}
