using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class WaveDashPage02 : WaveDashPage
	{
		private class TitleText
		{
			public const float Scale = 1.5f;

			public string Text;

			public Vector2 Position;

			public float Width;

			private float ease;

			public TitleText(Vector2 pos, string text)
			{
				Position = pos;
				Text = text;
				Width = ActiveFont.Measure(text).X * 1.5f;
			}

			public IEnumerator Stamp()
			{
				while (ease < 1f)
				{
					ease = Calc.Approach(ease, 1f, Engine.DeltaTime * 4f);
					yield return null;
				}
				yield return 0.2f;
			}

			public void Render()
			{
				if (!(ease <= 0f))
				{
					Vector2 scale = Vector2.One * (1f + (1f - Ease.CubeOut(ease))) * 1.5f;
					ActiveFont.DrawOutline(Text, Position + new Vector2(Width / 2f, ActiveFont.LineHeight * 0.5f * 1.5f), new Vector2(0.5f, 0.5f), scale, Color.White, 2f, Color.Black);
				}
			}
		}

		private List<TitleText> title = new List<TitleText>();

		private FancyText.Text list;

		private int listIndex;

		private float impossibleEase;

		public WaveDashPage02()
		{
			Transition = Transitions.Rotate3D;
			ClearColor = Calc.HexToColor("fff2cc");
		}

		public override void Added(WaveDashPresentation presentation)
		{
			base.Added(presentation);
		}

		public override IEnumerator Routine()
		{
			string[] text = Dialog.Clean("WAVEDASH_PAGE2_TITLE").Split('|');
			Vector2 pos = new Vector2(128f, 128f);
			for (int i = 0; i < text.Length; i++)
			{
				TitleText item = new TitleText(pos, text[i]);
				title.Add(item);
				yield return item.Stamp();
				pos.X += item.Width + ActiveFont.Measure(' ').X * 1.5f;
			}
			yield return PressButton();
			list = FancyText.Parse(Dialog.Get("WAVEDASH_PAGE2_LIST"), base.Width, 32, 1f, Color.Black * 0.7f);
			float delay = 0f;
			while (listIndex < list.Nodes.Count)
			{
				if (list.Nodes[listIndex] is FancyText.NewLine)
				{
					yield return PressButton();
				}
				else
				{
					delay += 0.008f;
					if (delay >= 0.016f)
					{
						delay -= 0.016f;
						yield return 0.016f;
					}
				}
				listIndex++;
			}
			yield return PressButton();
			Audio.Play("event:/new_content/game/10_farewell/ppt_impossible");
			while (impossibleEase < 1f)
			{
				impossibleEase = Calc.Approach(impossibleEase, 1f, Engine.DeltaTime);
				yield return null;
			}
		}

		public override void Update()
		{
		}

		public override void Render()
		{
			foreach (TitleText item in title)
			{
				item.Render();
			}
			if (list != null)
			{
				list.Draw(new Vector2(160f, 260f), new Vector2(0f, 0f), Vector2.One, 1f, 0, listIndex);
			}
			if (impossibleEase > 0f)
			{
				MTexture clipArt = Presentation.Gfx["Guy Clip Art"];
				float clipArtScale = 0.75f;
				clipArt.Draw(new Vector2((float)base.Width - (float)clipArt.Width * clipArtScale, (float)base.Height - 640f * impossibleEase), Vector2.Zero, Color.White, clipArtScale);
				Matrix matrix = Matrix.CreateRotationZ(-0.5f + Ease.CubeIn(1f - impossibleEase) * 8f) * Matrix.CreateTranslation(base.Width - 500, base.Height - 600, 0f);
				Draw.SpriteBatch.End();
				Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, matrix);
				ActiveFont.Draw(Dialog.Clean("WAVEDASH_PAGE2_IMPOSSIBLE"), Vector2.Zero, new Vector2(0.5f, 0.5f), Vector2.One * (2f + (1f - impossibleEase) * 0.5f), Color.Black * impossibleEase);
				Draw.SpriteBatch.End();
				Draw.SpriteBatch.Begin();
			}
		}
	}
}
