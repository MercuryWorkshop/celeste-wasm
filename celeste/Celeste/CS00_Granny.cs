using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS00_Granny : CutsceneEntity
	{
		public const string Flag = "granny";

		private NPC00_Granny granny;

		private Player player;

		private Vector2 endPlayerPosition;

		private Coroutine zoomCoroutine;

		public CS00_Granny(NPC00_Granny granny, Player player)
		{
			this.granny = granny;
			this.player = player;
			endPlayerPosition = granny.Position + new Vector2(48f, 0f);
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene()));
		}

		private IEnumerator Cutscene()
		{
			player.StateMachine.State = 11;
			if (Math.Abs(player.X - granny.X) < 20f)
			{
				yield return player.DummyWalkTo(granny.X - 48f);
			}
			player.Facing = Facings.Right;
			yield return 0.5f;
			yield return Textbox.Say("CH0_GRANNY", Meet, RunAlong, LaughAndAirQuotes, Laugh, StopLaughing, OminousZoom, PanToMaddy);
			yield return Level.ZoomBack(0.5f);
			EndCutscene(Level);
		}

		private IEnumerator Meet()
		{
			yield return 0.25f;
			granny.Sprite.Scale.X = Math.Sign(player.X - granny.X);
			yield return player.DummyWalkTo(granny.X - 20f);
			player.Facing = Facings.Right;
			yield return 0.8f;
		}

		private IEnumerator RunAlong()
		{
			yield return player.DummyWalkToExact((int)endPlayerPosition.X);
			yield return 0.8f;
			player.Facing = Facings.Left;
			yield return 0.4f;
			granny.Sprite.Scale.X = 1f;
			yield return Level.ZoomTo(new Vector2(210f, 90f), 2f, 0.5f);
			yield return 0.2f;
		}

		private IEnumerator LaughAndAirQuotes()
		{
			yield return 0.6f;
			granny.LaughSfx.FirstPlay = true;
			granny.Sprite.Play("laugh");
			yield return 2f;
			granny.Sprite.Play("airQuotes");
		}

		private IEnumerator Laugh()
		{
			granny.LaughSfx.FirstPlay = false;
			yield return null;
			granny.Sprite.Play("laugh");
		}

		private IEnumerator StopLaughing()
		{
			granny.Sprite.Play("idle");
			yield break;
		}

		private IEnumerator OminousZoom()
		{
			Vector2 zoomAt = new Vector2(210f, 100f);
			zoomCoroutine = new Coroutine(Level.ZoomAcross(zoomAt, 4f, 3f));
			Add(zoomCoroutine);
			granny.Sprite.Play("idle");
			yield return 0.2f;
		}

		private IEnumerator PanToMaddy()
		{
			while (zoomCoroutine != null && zoomCoroutine.Active)
			{
				yield return null;
			}
			yield return 0.2f;
			yield return Level.ZoomAcross(new Vector2(210f, 90f), 2f, 0.5f);
			yield return 0.2f;
		}

		public override void OnEnd(Level level)
		{
			granny.Hahaha.Enabled = true;
			granny.Sprite.Play("laugh");
			granny.Sprite.Scale.X = 1f;
			player.Position.X = endPlayerPosition.X;
			player.Facing = Facings.Left;
			player.StateMachine.State = 0;
			level.Session.SetFlag("granny");
			level.ResetZoom();
		}
	}
}
