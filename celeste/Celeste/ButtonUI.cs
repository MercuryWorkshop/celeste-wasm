using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public static class ButtonUI
	{
		public static float Width(string label, VirtualButton button)
		{
			MTexture icon = Input.GuiButton(button);
			return ActiveFont.Measure(label).X + 8f + (float)icon.Width;
		}

		public static void Render(Vector2 position, string label, VirtualButton button, float scale, float justifyX = 0.5f, float wiggle = 0f, float alpha = 1f)
		{
			MTexture icon = Input.GuiButton(button);
			float width = Width(label, button);
			position.X -= scale * width * (justifyX - 0.5f);
			icon.Draw(position, new Vector2((float)icon.Width - width / 2f, (float)icon.Height / 2f), Color.White * alpha, scale + wiggle);
			DrawText(label, position, width / 2f, scale + wiggle, alpha);
		}

		private static void DrawText(string text, Vector2 position, float justify, float scale, float alpha)
		{
			float width = ActiveFont.Measure(text).X;
			ActiveFont.DrawOutline(text, position, new Vector2(justify / width, 0.5f), Vector2.One * scale, Color.White * alpha, 2f, Color.Black * alpha);
		}
	}
}
