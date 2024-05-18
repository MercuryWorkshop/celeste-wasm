using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS06_Reflection : CutsceneEntity
	{
		public const string Flag = "reflection";

		private Player player;

		private float targetX;

		public CS06_Reflection(Player player, float targetX)
		{
			this.player = player;
			this.targetX = targetX;
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			player.ForceCameraUpdate = true;
			yield return player.DummyWalkToExact((int)targetX);
			yield return 0.1f;
			player.Facing = Facings.Right;
			yield return 0.1f;
			yield return Level.ZoomTo(new Vector2(200f, 90f), 2f, 1f);
			yield return Textbox.Say("CH6_REFLECT_AFTER");
			yield return Level.ZoomBack(0.5f);
			EndCutscene(level);
		}

		public override void OnEnd(Level level)
		{
			player.StateMachine.Locked = false;
			player.StateMachine.State = 0;
			player.ForceCameraUpdate = false;
			player.FlipInReflection = false;
			level.Session.SetFlag("reflection");
		}
	}
}
