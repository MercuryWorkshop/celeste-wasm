using System.Collections;
using Monocle;

namespace Celeste
{
	public class CS03_Diary : CutsceneEntity
	{
		private Player player;

		public CS03_Diary(Player player)
		{
			this.player = player;
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Routine()));
		}

		private IEnumerator Routine()
		{
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			yield return Textbox.Say("CH3_DIARY");
			yield return 0.1f;
			EndCutscene(Level);
		}

		public override void OnEnd(Level level)
		{
			player.StateMachine.Locked = false;
			player.StateMachine.State = 0;
		}
	}
}
