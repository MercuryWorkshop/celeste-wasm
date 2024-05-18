using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS08_EnterDoor : CutsceneEntity
	{
		private Player player;

		private float targetX;

		public CS08_EnterDoor(Player player, float targetX)
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
			Add(new Coroutine(player.DummyWalkToExact((int)targetX, walkBackwards: false, 0.7f)));
			Add(new Coroutine(level.ZoomTo(new Vector2(targetX - level.Camera.X, 90f), 2f, 2f)));
			FadeWipe wipe = new FadeWipe(level, wipeIn: false);
			wipe.Duration = 2f;
			yield return wipe.Wait();
			EndCutscene(level);
		}

		public override void OnEnd(Level level)
		{
			level.OnEndOfFrame += delegate
			{
				level.Remove(player);
				level.UnloadLevel();
				level.Session.Level = "inside";
				level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));
				level.LoadLevel(Player.IntroTypes.None);
				level.Add(new CS08_Ending());
			};
		}
	}
}
