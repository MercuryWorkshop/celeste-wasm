using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class MainMenuClimb : MenuButton
	{
		private const float MaxLabelWidth = 256f;

		private const float BaseLabelScale = 1.5f;

		private string label;

		private MTexture icon;

		private float labelScale;

		private Wiggler bounceWiggler;

		private Wiggler rotateWiggler;

		private Wiggler bigBounceWiggler;

		private bool confirmed;

		public override float ButtonHeight => (float)icon.Height + ActiveFont.LineHeight + 48f;

		public MainMenuClimb(Oui oui, Vector2 targetPosition, Vector2 tweenFrom, Action onConfirm)
			: base(oui, targetPosition, tweenFrom, onConfirm)
		{
			label = Dialog.Clean("menu_begin");
			icon = GFX.Gui["menu/start"];
			labelScale = 1f;
			float labelWidth = ActiveFont.Measure(label).X * 1.5f;
			if (labelWidth > 256f)
			{
				labelScale = 256f / labelWidth;
			}
			Add(bounceWiggler = Wiggler.Create(0.25f, 4f));
			Add(rotateWiggler = Wiggler.Create(0.3f, 6f));
			Add(bigBounceWiggler = Wiggler.Create(0.4f, 2f));
		}

		public override void OnSelect()
		{
			confirmed = false;
			bounceWiggler.Start();
		}

		public override void Confirm()
		{
			base.Confirm();
			confirmed = true;
			bounceWiggler.Start();
			bigBounceWiggler.Start();
			rotateWiggler.Start();
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
		}

		public override void Render()
		{
			Vector2 offset = new Vector2(0f, bounceWiggler.Value * 8f);
			Vector2 iconAdd = Vector2.UnitY * icon.Height + new Vector2(0f, 0f - Math.Abs(bigBounceWiggler.Value * 40f));
			if (!confirmed)
			{
				iconAdd += offset;
			}
			icon.DrawOutlineJustified(Position + iconAdd, new Vector2(0.5f, 1f), Color.White, 1f, rotateWiggler.Value * 10f * ((float)Math.PI / 180f));
			ActiveFont.DrawOutline(label, Position + offset + new Vector2(0f, 48 + icon.Height), new Vector2(0.5f, 0.5f), Vector2.One * 1.5f * labelScale, base.SelectionColor, 2f, Color.Black);
		}
	}
}
