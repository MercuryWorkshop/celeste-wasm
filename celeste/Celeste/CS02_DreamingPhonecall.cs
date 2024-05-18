using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS02_DreamingPhonecall : CutsceneEntity
	{
		private BadelineDummy evil;

		private Player player;

		private Payphone payphone;

		private SoundSource ringtone;

		public CS02_DreamingPhonecall(Player player)
			: base(fadeInOnSkip: false)
		{
			this.player = player;
		}

		public override void OnBegin(Level level)
		{
			payphone = base.Scene.Tracker.GetEntity<Payphone>();
			Add(new Coroutine(Cutscene(level)));
			Add(ringtone = new SoundSource());
			ringtone.Position = payphone.Position;
		}

		private IEnumerator Cutscene(Level level)
		{
			player.StateMachine.State = 11;
			player.Dashes = 1;
			yield return 0.3f;
			ringtone.Play("event:/game/02_old_site/sequence_phone_ring_loop");
			while (player.Light.Alpha > 0f)
			{
				player.Light.Alpha -= Engine.DeltaTime * 2f;
				yield return null;
			}
			yield return 3.2f;
			yield return player.DummyWalkTo(payphone.X - 24f);
			yield return 1.5f;
			player.Facing = Facings.Left;
			yield return 1.5f;
			player.Facing = Facings.Right;
			yield return 0.25f;
			yield return player.DummyWalkTo(payphone.X - 4f);
			yield return 1.5f;
			Add(Alarm.Create(Alarm.AlarmMode.Oneshot, delegate
			{
				ringtone.Param("end", 1f);
			}, 0.43f, start: true));
			player.Visible = false;
			Audio.Play("event:/game/02_old_site/sequence_phone_pickup", player.Position);
			yield return payphone.Sprite.PlayRoutine("pickUp");
			yield return 1f;
			if (level.Session.Area.Mode == AreaMode.Normal)
			{
				Audio.SetMusic("event:/music/lvl2/phone_loop");
			}
			payphone.Sprite.Play("talkPhone");
			yield return Textbox.Say("CH2_DREAM_PHONECALL", ShowShadowMadeline);
			if (evil != null)
			{
				if (level.Session.Area.Mode == AreaMode.Normal)
				{
					Audio.SetMusic("event:/music/lvl2/phone_end");
				}
				evil.Vanish();
				evil = null;
				yield return 1f;
			}
			Add(new Coroutine(WireFalls()));
			payphone.Broken = true;
			level.Shake(0.2f);
			VertexLight light = new VertexLight(new Vector2(16f, -28f), Color.White, 0f, 32, 48);
			payphone.Add(light);
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, null, 2f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				light.Alpha = t.Eased;
			};
			Add(tween);
			Audio.Play("event:/game/02_old_site/sequence_phone_transform", payphone.Position);
			yield return payphone.Sprite.PlayRoutine("transform");
			yield return 0.4f;
			yield return payphone.Sprite.PlayRoutine("eat");
			payphone.Sprite.Play("monsterIdle");
			yield return 1.2f;
			level.EndCutscene();
			new FadeWipe(level, wipeIn: false, delegate
			{
				EndCutscene(level);
			});
		}

		private IEnumerator ShowShadowMadeline()
		{
			Payphone payphone = base.Scene.Tracker.GetEntity<Payphone>();
			Level level = base.Scene as Level;
			yield return level.ZoomTo(new Vector2(240f, 116f), 2f, 0.5f);
			evil = new BadelineDummy(payphone.Position + new Vector2(32f, -24f));
			evil.Appear(level);
			base.Scene.Add(evil);
			yield return 0.2f;
			payphone.Blink.X += 1f;
			yield return payphone.Sprite.PlayRoutine("jumpBack");
			yield return payphone.Sprite.PlayRoutine("scare");
			yield return 1.2f;
		}

		private IEnumerator WireFalls()
		{
			yield return 0.5f;
			Wire wire = base.Scene.Entities.FindFirst<Wire>();
			Vector2 speed = Vector2.Zero;
			Level level = SceneAs<Level>();
			while (wire != null && wire.Curve.Begin.X < (float)level.Bounds.Right)
			{
				speed += new Vector2(0.7f, 1f) * 200f * Engine.DeltaTime;
				wire.Curve.Begin += speed * Engine.DeltaTime;
				yield return null;
			}
		}

		public override void OnEnd(Level level)
		{
			Leader.StoreStrawberries(player.Leader);
			level.ResetZoom();
			level.Bloom.Base = 0f;
			level.Remove(player);
			level.UnloadLevel();
			level.Session.Dreaming = false;
			level.Session.Level = "end_0";
			level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Bottom));
			level.Session.Audio.Music.Event = "event:/music/lvl2/awake";
			level.Session.Audio.Ambience.Event = "event:/env/amb/02_awake";
			level.LoadLevel(Player.IntroTypes.WakeUp);
			level.EndCutscene();
			Leader.RestoreStrawberries(level.Tracker.GetEntity<Player>().Leader);
		}
	}
}
