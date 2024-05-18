using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS06_BossMid : CutsceneEntity
	{
		public const string Flag = "boss_mid";

		private Player player;

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			while (player == null)
			{
				player = base.Scene.Tracker.GetEntity<Player>();
				yield return null;
			}
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			while (!player.OnGround())
			{
				yield return null;
			}
			yield return player.DummyWalkToExact((int)player.X + 20);
			yield return level.ZoomTo(new Vector2(80f, 110f), 2f, 0.5f);
			yield return Textbox.Say("ch6_boss_middle");
			yield return 0.1f;
			yield return level.ZoomBack(0.4f);
			EndCutscene(level);
		}

		public override void OnEnd(Level level)
		{
			if (WasSkipped && player != null)
			{
				while (!player.OnGround() && player.Y < (float)level.Bounds.Bottom)
				{
					player.Y++;
				}
			}
			if (player != null)
			{
				player.StateMachine.Locked = false;
				player.StateMachine.State = 0;
			}
			level.Entities.FindFirst<FinalBoss>()?.OnPlayer(null);
			level.Session.SetFlag("boss_mid");
		}
	}
}
