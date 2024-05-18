using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS06_Ending : CutsceneEntity
	{
		private Player player;

		private BadelineDummy badeline;

		private NPC granny;

		private NPC theo;

		public CS06_Ending(Player player, NPC granny)
			: base(fadeInOnSkip: false, endingChapterAfter: true)
		{
			this.player = player;
			this.granny = granny;
		}

		public override void OnBegin(Level level)
		{
			level.RegisterAreaComplete();
			theo = base.Scene.Entities.FindFirst<NPC06_Theo_Ending>();
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			yield return 1f;
			player.Dashes = 1;
			level.Session.Inventory.Dashes = 1;
			level.Add(badeline = new BadelineDummy(player.Center));
			badeline.Appear(level, silent: true);
			badeline.FloatSpeed = 80f;
			badeline.Sprite.Scale.X = -1f;
			Audio.Play("event:/char/badeline/maddy_split", player.Center);
			yield return badeline.FloatTo(player.Position + new Vector2(24f, -20f), -1, faceDirection: false);
			yield return level.ZoomTo(new Vector2(160f, 120f), 2f, 1f);
			yield return Textbox.Say("ch6_ending", GrannyEnter, TheoEnter, MaddyTurnsRight, BadelineTurnsRight, BadelineTurnsLeft, WaitAbit, TurnToLeft, TheoRaiseFist, TheoStopTired);
			Audio.Play("event:/char/madeline/backpack_drop", player.Position);
			player.DummyAutoAnimate = false;
			player.Sprite.Play("bagdown");
			EndCutscene(level);
		}

		private IEnumerator GrannyEnter()
		{
			yield return 0.25f;
			badeline.Sprite.Scale.X = 1f;
			yield return 0.1f;
			granny.Visible = true;
			Add(new Coroutine(badeline.FloatTo(new Vector2(badeline.X - 10f, badeline.Y), 1, faceDirection: false)));
			yield return granny.MoveTo(player.Position + new Vector2(40f, 0f));
		}

		private IEnumerator TheoEnter()
		{
			player.Facing = Facings.Left;
			badeline.Sprite.Scale.X = -1f;
			yield return 0.25f;
			yield return CutsceneEntity.CameraTo(new Vector2(Level.Camera.X - 40f, Level.Camera.Y), 1f);
			theo.Visible = true;
			Add(new Coroutine(CutsceneEntity.CameraTo(new Vector2(Level.Camera.X + 40f, Level.Camera.Y), 2f, null, 1f)));
			Add(new Coroutine(badeline.FloatTo(new Vector2(badeline.X + 6f, badeline.Y + 4f), -1, faceDirection: false)));
			yield return theo.MoveTo(player.Position + new Vector2(-32f, 0f));
			theo.Sprite.Play("tired");
		}

		private IEnumerator MaddyTurnsRight()
		{
			yield return 0.1f;
			player.Facing = Facings.Right;
			yield return 0.1f;
			yield return badeline.FloatTo(badeline.Position + new Vector2(-2f, 10f), -1, faceDirection: false);
			yield return 0.1f;
		}

		private IEnumerator BadelineTurnsRight()
		{
			yield return 0.1f;
			badeline.Sprite.Scale.X = 1f;
			yield return 0.1f;
		}

		private IEnumerator BadelineTurnsLeft()
		{
			yield return 0.1f;
			badeline.Sprite.Scale.X = -1f;
			yield return 0.1f;
		}

		private IEnumerator WaitAbit()
		{
			yield return 0.4f;
		}

		private IEnumerator TurnToLeft()
		{
			yield return 0.1f;
			player.Facing = Facings.Left;
			yield return 0.05f;
			badeline.Sprite.Scale.X = -1f;
			yield return 0.1f;
		}

		private IEnumerator TheoRaiseFist()
		{
			theo.Sprite.Play("yolo");
			Add(Alarm.Create(Alarm.AlarmMode.Oneshot, delegate
			{
				theo.Sprite.Play("yoloEnd");
			}, 0.8f, start: true));
			yield return null;
		}

		private IEnumerator TheoStopTired()
		{
			theo.Sprite.Play("idle");
			yield return null;
		}

		public override void OnEnd(Level level)
		{
			level.CompleteArea();
			SpotlightWipe.FocusPoint += new Vector2(0f, -20f);
		}
	}
}
