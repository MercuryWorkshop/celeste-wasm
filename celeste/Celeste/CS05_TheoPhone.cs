using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS05_TheoPhone : CutsceneEntity
	{
		private Player player;

		private float targetX;

		public CS05_TheoPhone(Player player, float targetX)
		{
			this.player = player;
			this.targetX = targetX;
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Routine()));
		}

		private IEnumerator Routine()
		{
			player.StateMachine.State = 11;
			if (player.X != targetX)
			{
				player.Facing = (Facings)Math.Sign(targetX - player.X);
			}
			yield return 0.5f;
			yield return Level.ZoomTo(new Vector2(80f, 60f), 2f, 0.5f);
			yield return Textbox.Say("CH5_PHONE", WalkToPhone, StandBackUp);
			yield return Level.ZoomBack(0.5f);
			EndCutscene(Level);
		}

		private IEnumerator WalkToPhone()
		{
			yield return 0.25f;
			yield return player.DummyWalkToExact((int)targetX);
			player.Facing = Facings.Left;
			yield return 0.5f;
			player.DummyAutoAnimate = false;
			player.Sprite.Play("duck");
			yield return 0.5f;
		}

		private IEnumerator StandBackUp()
		{
			RemovePhone();
			yield return 0.6f;
			player.Sprite.Play("idle");
			yield return 0.2f;
		}

		public override void OnEnd(Level level)
		{
			RemovePhone();
			player.StateMachine.State = 0;
		}

		private void RemovePhone()
		{
			base.Scene.Entities.FindFirst<TheoPhone>()?.RemoveSelf();
		}
	}
}
