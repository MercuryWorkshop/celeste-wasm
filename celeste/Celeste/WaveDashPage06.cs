using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class WaveDashPage06 : WaveDashPage
	{
		private AreaCompleteTitle title;

		public WaveDashPage06()
		{
			Transition = Transitions.Rotate3D;
			ClearColor = Calc.HexToColor("d9d2e9");
		}

		public override IEnumerator Routine()
		{
			yield return 1f;
			Audio.Play("event:/new_content/game/10_farewell/ppt_happy_wavedashing");
			title = new AreaCompleteTitle(new Vector2((float)base.Width / 2f, 150f), Dialog.Clean("WAVEDASH_PAGE6_TITLE"), 2f, rainbow: true);
			yield return 1.5f;
		}

		public override void Update()
		{
			if (title != null)
			{
				title.Update();
			}
		}

		public override void Render()
		{
			Presentation.Gfx["Bird Clip Art"].DrawCentered(new Vector2(base.Width, base.Height) / 2f, Color.White, 1.5f);
			if (title != null)
			{
				title.Render();
			}
		}
	}
}
