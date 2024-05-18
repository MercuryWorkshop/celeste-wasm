using System.Collections;
using Monocle;

namespace Celeste
{
	public class CS02_Ending : CutsceneEntity
	{
		private Player player;

		private Payphone payphone;

		private SoundSource phoneSfx;

		public CS02_Ending(Player player)
			: base(fadeInOnSkip: false, endingChapterAfter: true)
		{
			this.player = player;
			Add(phoneSfx = new SoundSource());
		}

		public override void OnBegin(Level level)
		{
			level.RegisterAreaComplete();
			payphone = base.Scene.Tracker.GetEntity<Payphone>();
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			player.StateMachine.State = 11;
			player.Dashes = 1;
			while (player.Light.Alpha > 0f)
			{
				player.Light.Alpha -= Engine.DeltaTime * 1.25f;
				yield return null;
			}
			yield return 1f;
			yield return player.DummyWalkTo(payphone.X - 4f);
			yield return 0.2f;
			player.Facing = Facings.Right;
			yield return 0.5f;
			player.Visible = false;
			Audio.Play("event:/game/02_old_site/sequence_phone_pickup", player.Position);
			yield return payphone.Sprite.PlayRoutine("pickUp");
			yield return 0.25f;
			phoneSfx.Position = player.Position;
			phoneSfx.Play("event:/game/02_old_site/sequence_phone_ringtone_loop");
			yield return 6f;
			phoneSfx.Stop();
			payphone.Sprite.Play("talkPhone");
			yield return Textbox.Say("CH2_END_PHONECALL");
			yield return 0.3f;
			EndCutscene(level);
		}

		public override void OnEnd(Level level)
		{
			level.CompleteArea();
		}
	}
}
