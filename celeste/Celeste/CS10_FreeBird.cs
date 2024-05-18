using System.Collections;
using Monocle;

namespace Celeste
{
	public class CS10_FreeBird : CutsceneEntity
	{
		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			yield return Textbox.Say("CH9_FREE_BIRD");
			FadeWipe wipe = new FadeWipe(level, wipeIn: false);
			wipe.Duration = 3f;
			yield return wipe.Duration;
			EndCutscene(level);
		}

		public override void OnEnd(Level level)
		{
			level.CompleteArea(spotlightWipe: false, skipScreenWipe: true);
		}
	}
}
