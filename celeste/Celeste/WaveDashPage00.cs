using System;
using System.Collections;
using System.Globalization;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class WaveDashPage00 : WaveDashPage
	{
		private Color taskbarColor = Calc.HexToColor("d9d3b1");

		private string time;

		private Vector2 pptIcon;

		private Vector2 cursor;

		private bool selected;

		public WaveDashPage00()
		{
			AutoProgress = true;
			ClearColor = Calc.HexToColor("118475");
			time = DateTime.Now.ToString("h:mm tt", CultureInfo.CreateSpecificCulture("en-US"));
			pptIcon = new Vector2(600f, 500f);
			cursor = new Vector2(1000f, 700f);
		}

		public override IEnumerator Routine()
		{
			yield return 1f;
			yield return MoveCursor(cursor + new Vector2(0f, -80f), 0.3f);
			yield return 0.2f;
			yield return MoveCursor(pptIcon, 0.8f);
			yield return 0.7f;
			selected = true;
			Audio.Play("event:/new_content/game/10_farewell/ppt_doubleclick");
			yield return 0.1f;
			selected = false;
			yield return 0.1f;
			selected = true;
			yield return 0.08f;
			selected = false;
			yield return 0.5f;
			Presentation.ScaleInPoint = pptIcon;
		}

		private IEnumerator MoveCursor(Vector2 to, float time)
		{
			Vector2 from = cursor;
			for (float t = 0f; t < 1f; t += Engine.DeltaTime / time)
			{
				cursor = from + (to - from) * Ease.SineOut(t);
				yield return null;
			}
		}

		public override void Update()
		{
		}

		public override void Render()
		{
			DrawIcon(new Vector2(160f, 120f), "desktop/mymountain_icon", Dialog.Clean("WAVEDASH_DESKTOP_MYPC"));
			DrawIcon(new Vector2(160f, 320f), "desktop/recyclebin_icon", Dialog.Clean("WAVEDASH_DESKTOP_RECYCLEBIN"));
			DrawIcon(pptIcon, "desktop/wavedashing_icon", Dialog.Clean("WAVEDASH_DESKTOP_POWERPOINT"));
			DrawTaskbar();
			Presentation.Gfx["desktop/cursor"].DrawCentered(cursor);
		}

		public void DrawTaskbar()
		{
			Draw.Rect(0f, (float)base.Height - 80f, base.Width, 80f, taskbarColor);
			Draw.Rect(0f, (float)base.Height - 80f, base.Width, 4f, Color.White * 0.5f);
			MTexture icon = Presentation.Gfx["desktop/startberry"];
			float height = 64f;
			float iconScale = height / (float)icon.Height * 0.7f;
			string text = Dialog.Clean("WAVEDASH_DESKTOP_STARTBUTTON");
			float textScale = 0.6f;
			float width = (float)icon.Width * iconScale + ActiveFont.Measure(text).X * textScale + 32f;
			Vector2 pos = new Vector2(8f, (float)base.Height - 80f + 8f);
			Draw.Rect(pos.X, pos.Y, width, height, Color.White * 0.5f);
			icon.DrawJustified(pos + new Vector2(8f, height / 2f), new Vector2(0f, 0.5f), Color.White, Vector2.One * iconScale);
			ActiveFont.Draw(text, pos + new Vector2((float)icon.Width * iconScale + 16f, height / 2f), new Vector2(0f, 0.5f), Vector2.One * textScale, Color.Black * 0.8f);
			ActiveFont.Draw(time, new Vector2((float)base.Width - 24f, (float)base.Height - 40f), new Vector2(1f, 0.5f), Vector2.One * 0.6f, Color.Black * 0.8f);
		}

		private void DrawIcon(Vector2 position, string icon, string text)
		{
			bool over = cursor.X > position.X - 64f && cursor.Y > position.Y - 64f && cursor.X < position.X + 64f && cursor.Y < position.Y + 80f;
			if (selected && over)
			{
				Draw.Rect(position.X - 80f, position.Y - 80f, 160f, 200f, Color.White * 0.25f);
			}
			if (over)
			{
				DrawDottedRect(position.X - 80f, position.Y - 80f, 160f, 200f);
			}
			MTexture tex = Presentation.Gfx[icon];
			float scale = 128f / (float)tex.Height;
			tex.DrawCentered(position, Color.White, scale);
			ActiveFont.Draw(text, position + new Vector2(0f, 80f), new Vector2(0.5f, 0f), Vector2.One * 0.6f, (selected && over) ? Color.Black : Color.White);
		}

		private void DrawDottedRect(float x, float y, float w, float h)
		{
			float t = 4f;
			Draw.Rect(x, y, w, t, Color.White);
			Draw.Rect(x + w - t, y, t, h, Color.White);
			Draw.Rect(x, y, t, h, Color.White);
			Draw.Rect(x, y + h - t, w, t, Color.White);
			if (!selected)
			{
				for (float tx = 4f; tx < w; tx += t * 2f)
				{
					Draw.Rect(x + tx, y, t, t, ClearColor);
					Draw.Rect(x + w - tx, y + h - t, t, t, ClearColor);
				}
				for (float ty = 4f; ty < h; ty += t * 2f)
				{
					Draw.Rect(x, y + ty, t, t, ClearColor);
					Draw.Rect(x + w - t, y + h - ty, t, t, ClearColor);
				}
			}
		}
	}
}
