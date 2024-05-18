using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS04_Granny : CutsceneEntity
	{
		public const string Flag = "granny_1";

		private NPC04_Granny granny;

		private Player player;

		public CS04_Granny(NPC04_Granny granny, Player player)
		{
			this.granny = granny;
			this.player = player;
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
			yield return player.DummyWalkTo(granny.X - 30f);
			player.Facing = Facings.Right;
			yield return Textbox.Say("CH4_GRANNY_1", Laughs, StopLaughing, WaitABeat, ZoomIn, MaddyTurnsAround, MaddyApproaches, MaddyWalksPastGranny);
			yield return Level.ZoomBack(0.5f);
			EndCutscene(level);
		}

		private IEnumerator Laughs()
		{
			granny.Sprite.Play("laugh");
			yield return 1f;
		}

		private IEnumerator StopLaughing()
		{
			granny.Sprite.Play("idle");
			yield return 0.25f;
		}

		private IEnumerator WaitABeat()
		{
			yield return 1.2f;
		}

		private IEnumerator ZoomIn()
		{
			yield return Level.ZoomTo(new Vector2(123f, 116f), 2f, 0.5f);
		}

		private IEnumerator MaddyTurnsAround()
		{
			yield return 0.2f;
			player.Facing = Facings.Left;
			yield return 0.1f;
		}

		private IEnumerator MaddyApproaches()
		{
			yield return player.DummyWalkTo(granny.X - 20f);
		}

		private IEnumerator MaddyWalksPastGranny()
		{
			yield return player.DummyWalkToExact((int)granny.X + 30);
		}

		public override void OnEnd(Level level)
		{
			player.X = granny.X + 30f;
			player.StateMachine.Locked = false;
			player.StateMachine.State = 0;
			player.ForceCameraUpdate = false;
			if (WasSkipped)
			{
				level.Camera.Position = player.CameraTarget;
			}
			granny.Sprite.Play("laugh");
			level.Session.SetFlag("granny_1");
		}
	}
}
