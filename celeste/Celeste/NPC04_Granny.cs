using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class NPC04_Granny : NPC
	{
		public Hahaha Hahaha;

		private bool cutscene;

		private Coroutine talkRoutine;

		private const string talkedFlagA = "granny_2";

		private const string talkedFlagB = "granny_3";

		public NPC04_Granny(Vector2 position)
			: base(position)
		{
			Add(Sprite = GFX.SpriteBank.Create("granny"));
			Sprite.Scale.X = -1f;
			Sprite.Play("idle");
			Add(new GrannyLaughSfx(Sprite));
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			scene.Add(Hahaha = new Hahaha(Position + new Vector2(8f, -4f)));
			Hahaha.Enabled = false;
			if (base.Session.GetFlag("granny_1") && !base.Session.GetFlag("granny_2"))
			{
				Sprite.Play("laugh");
			}
			if (!base.Session.GetFlag("granny_3"))
			{
				Add(Talker = new TalkComponent(new Rectangle(-20, -16, 40, 16), new Vector2(0f, -24f), OnTalk));
				if (!base.Session.GetFlag("granny_1"))
				{
					Talker.Enabled = false;
				}
			}
		}

		public override void Update()
		{
			Player player = Level.Tracker.GetEntity<Player>();
			if (player != null && !base.Session.GetFlag("granny_1") && !cutscene && player.X > base.X - 40f)
			{
				cutscene = true;
				base.Scene.Add(new CS04_Granny(this, player));
				if (Talker != null)
				{
					Talker.Enabled = true;
				}
			}
			Hahaha.Enabled = Sprite.CurrentAnimationID == "laugh";
			base.Update();
		}

		private void OnTalk(Player player)
		{
			Level.StartCutscene(TalkEnd);
			Add(talkRoutine = new Coroutine(TalkRoutine(player)));
		}

		private IEnumerator TalkRoutine(Player player)
		{
			Sprite.Play("idle");
			player.ForceCameraUpdate = true;
			yield return PlayerApproachLeftSide(player, turnToFace: true, 20f);
			yield return Level.ZoomTo(new Vector2((player.X + base.X) / 2f - Level.Camera.X, 116f), 2f, 0.5f);
			if (!base.Session.GetFlag("granny_2"))
			{
				yield return Textbox.Say("CH4_GRANNY_2");
			}
			else
			{
				yield return Textbox.Say("CH4_GRANNY_3");
			}
			yield return Level.ZoomBack(0.5f);
			Level.EndCutscene();
			TalkEnd(Level);
		}

		private void TalkEnd(Level level)
		{
			if (!base.Session.GetFlag("granny_2"))
			{
				base.Session.SetFlag("granny_2");
			}
			else if (!base.Session.GetFlag("granny_3"))
			{
				base.Session.SetFlag("granny_3");
				Remove(Talker);
			}
			if (talkRoutine != null)
			{
				talkRoutine.RemoveSelf();
				talkRoutine = null;
			}
			Player player = Level.Tracker.GetEntity<Player>();
			if (player != null)
			{
				player.StateMachine.Locked = false;
				player.StateMachine.State = 0;
				player.ForceCameraUpdate = false;
			}
		}
	}
}
