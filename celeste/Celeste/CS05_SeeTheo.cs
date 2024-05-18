using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS05_SeeTheo : CutsceneEntity
	{
		private const float NewDarknessAlpha = 0.3f;

		public const string Flag = "seeTheoInCrystal";

		private int index;

		private Player player;

		private TheoCrystal theo;

		public CS05_SeeTheo(Player player, int index)
		{
			this.player = player;
			this.index = index;
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			while (player.Scene == null || !player.OnGround())
			{
				yield return null;
			}
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			yield return 0.25f;
			theo = base.Scene.Tracker.GetEntity<TheoCrystal>();
			if (theo != null && Math.Sign(player.X - theo.X) != 0)
			{
				player.Facing = (Facings)Math.Sign(theo.X - player.X);
			}
			yield return 0.25f;
			if (index == 0)
			{
				yield return Textbox.Say("ch5_see_theo", ZoomIn, MadelineTurnsAround, WaitABit, MadelineTurnsBackAndBrighten);
			}
			else if (index == 1)
			{
				yield return Textbox.Say("ch5_see_theo_b");
			}
			yield return Level.ZoomBack(0.5f);
			EndCutscene(level);
		}

		private IEnumerator ZoomIn()
		{
			yield return Level.ZoomTo(Vector2.Lerp(player.Position, theo.Position, 0.5f) - Level.Camera.Position + new Vector2(0f, -20f), 2f, 0.5f);
		}

		private IEnumerator MadelineTurnsAround()
		{
			yield return 0.3f;
			player.Facing = Facings.Left;
			yield return 0.1f;
		}

		private IEnumerator WaitABit()
		{
			yield return 1f;
		}

		private IEnumerator MadelineTurnsBackAndBrighten()
		{
			yield return 0.1f;
			Coroutine coroutine = new Coroutine(Brighten());
			Add(coroutine);
			yield return 0.2f;
			player.Facing = Facings.Right;
			yield return 0.1f;
			while (coroutine.Active)
			{
				yield return null;
			}
		}

		private IEnumerator Brighten()
		{
			yield return Level.ZoomBack(0.5f);
			yield return 0.3f;
			Level.Session.DarkRoomAlpha = 0.3f;
			float darkness = Level.Session.DarkRoomAlpha;
			while (Level.Lighting.Alpha != darkness)
			{
				Level.Lighting.Alpha = Calc.Approach(Level.Lighting.Alpha, darkness, Engine.DeltaTime * 0.5f);
				yield return null;
			}
		}

		public override void OnEnd(Level level)
		{
			player.StateMachine.Locked = false;
			player.StateMachine.State = 0;
			player.ForceCameraUpdate = false;
			player.DummyAutoAnimate = true;
			level.Session.DarkRoomAlpha = 0.3f;
			level.Lighting.Alpha = level.Session.DarkRoomAlpha;
			level.Session.SetFlag("seeTheoInCrystal");
		}
	}
}
