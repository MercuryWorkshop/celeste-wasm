using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class WaveDashPage01 : WaveDashPage
	{
		private AreaCompleteTitle title;

		private float subtitleEase;

		public WaveDashPage01()
		{
			Transition = Transitions.ScaleIn;
			ClearColor = Calc.HexToColor("9fc5e8");
		}

		public override void Added(WaveDashPresentation presentation)
		{
			base.Added(presentation);
		}

		public override IEnumerator Routine()
		{
			Audio.SetAltMusic("event:/new_content/music/lvl10/intermission_powerpoint");
			yield return 1f;
			title = new AreaCompleteTitle(new Vector2((float)base.Width / 2f, (float)base.Height / 2f - 100f), Dialog.Clean("WAVEDASH_PAGE1_TITLE"), 2f, rainbow: true);
			yield return 1f;
			while (subtitleEase < 1f)
			{
				subtitleEase = Calc.Approach(subtitleEase, 1f, Engine.DeltaTime);
				yield return null;
			}
			yield return 0.1f;
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
			if (title != null)
			{
				title.Render();
			}
			if (subtitleEase > 0f)
			{
				Vector2 pos = new Vector2((float)base.Width / 2f, (float)base.Height / 2f + 80f);
				float sx = 1f + Ease.BigBackIn(1f - subtitleEase) * 2f;
				float sy = 0.25f + Ease.BigBackIn(subtitleEase) * 0.75f;
				ActiveFont.Draw(Dialog.Clean("WAVEDASH_PAGE1_SUBTITLE"), pos, new Vector2(0.5f, 0.5f), new Vector2(sx, sy), Color.Black * 0.8f);
			}
		}
	}
}
