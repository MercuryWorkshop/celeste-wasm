using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS05_TheoInMirror : CutsceneEntity
	{
		public const string Flag = "theoInMirror";

		private NPC theo;

		private Player player;

		private int playerFinalX;

		public CS05_TheoInMirror(NPC theo, Player player)
		{
			this.theo = theo;
			this.player = player;
			playerFinalX = (int)theo.Position.X + 24;
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			yield return player.DummyWalkTo(theo.X - 16f);
			yield return 0.5f;
			theo.Sprite.Scale.X = -1f;
			yield return 0.25f;
			yield return Textbox.Say("ch5_theo_mirror");
			Add(new Coroutine(theo.MoveTo(theo.Position + new Vector2(64f, 0f))));
			yield return 0.4f;
			yield return player.DummyWalkToExact(playerFinalX);
			EndCutscene(level);
		}

		public override void OnEnd(Level level)
		{
			player.StateMachine.Locked = false;
			player.StateMachine.State = 0;
			player.X = playerFinalX;
			player.MoveV(200f);
			player.Speed = Vector2.Zero;
			base.Scene.Remove(theo);
			level.Session.SetFlag("theoInMirror");
		}
	}
}
