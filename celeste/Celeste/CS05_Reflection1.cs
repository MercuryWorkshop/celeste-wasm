using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS05_Reflection1 : CutsceneEntity
	{
		public const string Flag = "reflection";

		private Player player;

		public CS05_Reflection1(Player player)
		{
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
			TempleMirror mirror = base.Scene.Entities.FindFirst<TempleMirror>();
			yield return player.DummyWalkTo(mirror.Center.X + 8f);
			yield return 0.2f;
			player.Facing = Facings.Left;
			yield return 0.3f;
			if (!player.Dead)
			{
				yield return Textbox.Say("ch5_reflection", MadelineFallsToKnees, MadelineStopsPanicking, MadelineGetsUp);
			}
			else
			{
				yield return 100f;
			}
			yield return Level.ZoomBack(0.5f);
			EndCutscene(level);
		}

		private IEnumerator MadelineFallsToKnees()
		{
			yield return 0.2f;
			player.DummyAutoAnimate = false;
			player.Sprite.Play("tired");
			yield return 0.2f;
			yield return Level.ZoomTo(new Vector2(90f, 116f), 2f, 0.5f);
			yield return 0.2f;
		}

		private IEnumerator MadelineStopsPanicking()
		{
			yield return 0.8f;
			player.Sprite.Play("tiredStill");
			yield return 0.4f;
		}

		private IEnumerator MadelineGetsUp()
		{
			player.DummyAutoAnimate = true;
			player.Sprite.Play("idle");
			yield break;
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
