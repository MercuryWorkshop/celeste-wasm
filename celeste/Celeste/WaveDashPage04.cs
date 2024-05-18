using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class WaveDashPage04 : WaveDashPage
	{
		private WaveDashPlaybackTutorial tutorial;

		private FancyText.Text list;

		private int listIndex;

		private float time;

		public WaveDashPage04()
		{
			Transition = Transitions.FadeIn;
			ClearColor = Calc.HexToColor("f4cccc");
		}

		public override void Added(WaveDashPresentation presentation)
		{
			base.Added(presentation);
			List<MTexture> textures = Presentation.Gfx.GetAtlasSubtextures("playback/platforms");
			tutorial = new WaveDashPlaybackTutorial("wavedashppt", new Vector2(-126f, 0f), new Vector2(1f, 1f), new Vector2(1f, -1f));
			tutorial.OnRender = delegate
			{
				textures[(int)(time % (float)textures.Count)].DrawCentered(Vector2.Zero);
			};
		}

		public override IEnumerator Routine()
		{
			yield return 0.5f;
			list = FancyText.Parse(Dialog.Get("WAVEDASH_PAGE4_LIST"), base.Width, 32, 1f, Color.Black * 0.7f);
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
		}

		public override void Update()
		{
			time += Engine.DeltaTime * 4f;
			tutorial.Update();
		}

		public override void Render()
		{
			ActiveFont.DrawOutline(Dialog.Clean("WAVEDASH_PAGE4_TITLE"), new Vector2(128f, 100f), Vector2.Zero, Vector2.One * 1.5f, Color.White, 2f, Color.Black);
			tutorial.Render(new Vector2((float)base.Width / 2f, (float)base.Height / 2f - 100f), 4f);
			if (list != null)
			{
				list.Draw(new Vector2(160f, base.Height - 400), new Vector2(0f, 0f), Vector2.One, 1f, 0, listIndex);
			}
		}
	}
}
