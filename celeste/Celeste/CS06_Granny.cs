using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS06_Granny : CutsceneEntity
	{
		public const string FlagPrefix = "granny_";

		private NPC06_Granny granny;

		private Player player;

		private float startX;

		private int index;

		private bool firstLaugh;

		public CS06_Granny(NPC06_Granny granny, Player player, int index)
		{
			this.granny = granny;
			this.player = player;
			this.index = index;
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
			if (index == 0)
			{
				yield return player.DummyWalkTo(granny.X - 40f);
				startX = player.X;
				player.Facing = Facings.Right;
				firstLaugh = true;
				yield return Textbox.Say("ch6_oldlady", ZoomIn, Laughs, StopLaughing, MaddyWalksRight, MaddyWalksLeft, WaitABit, MaddyTurnsRight);
			}
			else if (index == 1)
			{
				yield return ZoomIn();
				yield return player.DummyWalkTo(granny.X - 20f);
				player.Facing = Facings.Right;
				yield return Textbox.Say("ch6_oldlady_b");
			}
			else if (index == 2)
			{
				yield return ZoomIn();
				yield return player.DummyWalkTo(granny.X - 20f);
				player.Facing = Facings.Right;
				yield return Textbox.Say("ch6_oldlady_c");
			}
			yield return Level.ZoomBack(0.5f);
			EndCutscene(level);
		}

		private IEnumerator ZoomIn()
		{
			Vector2 center = Vector2.Lerp(granny.Position, player.Position, 0.5f) - Level.Camera.Position + new Vector2(0f, -20f);
			yield return Level.ZoomTo(center, 2f, 0.5f);
		}

		private IEnumerator Laughs()
		{
			if (firstLaugh)
			{
				firstLaugh = false;
				yield return 0.5f;
			}
			granny.Sprite.Play("laugh");
			yield return 1f;
		}

		private IEnumerator StopLaughing()
		{
			granny.Sprite.Play("idle");
			yield return 0.25f;
		}

		private IEnumerator MaddyWalksLeft()
		{
			yield return 0.1f;
			player.Facing = Facings.Left;
			yield return player.DummyWalkToExact((int)player.X - 8);
			yield return 0.1f;
		}

		private IEnumerator MaddyWalksRight()
		{
			yield return 0.1f;
			player.Facing = Facings.Right;
			yield return player.DummyWalkToExact((int)player.X + 8);
			yield return 0.1f;
		}

		private IEnumerator WaitABit()
		{
			yield return 0.8f;
		}

		private IEnumerator MaddyTurnsRight()
		{
			yield return 0.1f;
			player.Facing = Facings.Right;
			yield return 0.1f;
		}

		public override void OnEnd(Level level)
		{
			player.StateMachine.Locked = false;
			player.StateMachine.State = 0;
			player.ForceCameraUpdate = false;
			granny.Sprite.Play("idle");
			level.Session.SetFlag("granny_" + index);
		}
	}
}
