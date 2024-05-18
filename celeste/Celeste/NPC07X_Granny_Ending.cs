using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class NPC07X_Granny_Ending : NPC
	{
		public Hahaha Hahaha;

		public GrannyLaughSfx LaughSfx;

		private Player player;

		private TalkComponent talker;

		private Coroutine talkRoutine;

		private int conversation;

		private bool ch9EasterEgg;

		public NPC07X_Granny_Ending(EntityData data, Vector2 offset, bool ch9EasterEgg = false)
			: base(data.Position + offset)
		{
			Add(Sprite = GFX.SpriteBank.Create("granny"));
			Sprite.Play("idle");
			Sprite.Scale.X = -1f;
			Add(LaughSfx = new GrannyLaughSfx(Sprite));
			Add(talker = new TalkComponent(new Rectangle(-20, -8, 40, 8), new Vector2(0f, -24f), OnTalk));
			MoveAnim = "walk";
			Maxspeed = 40f;
			this.ch9EasterEgg = ch9EasterEgg;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			scene.Add(Hahaha = new Hahaha(Position + new Vector2(8f, -4f)));
			Hahaha.Enabled = false;
		}

		public override void Update()
		{
			Hahaha.Enabled = Sprite.CurrentAnimationID == "laugh";
			base.Update();
		}

		private void OnTalk(Player player)
		{
			this.player = player;
			(base.Scene as Level).StartCutscene(EndTalking);
			Add(talkRoutine = new Coroutine(TalkRoutine(player)));
		}

		private IEnumerator TalkRoutine(Player player)
		{
			player.StateMachine.State = 11;
			player.ForceCameraUpdate = true;
			while (!player.OnGround())
			{
				yield return null;
			}
			yield return player.DummyWalkToExact((int)base.X - 16);
			player.Facing = Facings.Right;
			if (ch9EasterEgg)
			{
				yield return 0.5f;
				yield return Level.ZoomTo(Position - Level.Camera.Position + new Vector2(0f, -32f), 2f, 0.5f);
				Dialog.Language.Dialog["CH10_GRANNY_EASTEREGG"] = "{portrait GRANNY right mock} I see you have discovered Debug Mode.";
				yield return Textbox.Say("CH10_GRANNY_EASTEREGG");
				talker.Enabled = false;
			}
			else if (conversation == 0)
			{
				yield return 0.5f;
				yield return Level.ZoomTo(Position - Level.Camera.Position + new Vector2(0f, -32f), 2f, 0.5f);
				yield return Textbox.Say("CH7_CSIDE_OLDLADY", StartLaughing, StopLaughing);
			}
			else if (conversation == 1)
			{
				yield return 0.5f;
				yield return Level.ZoomTo(Position - Level.Camera.Position + new Vector2(0f, -32f), 2f, 0.5f);
				yield return Textbox.Say("CH7_CSIDE_OLDLADY_B", StartLaughing, StopLaughing);
				talker.Enabled = false;
			}
			yield return Level.ZoomBack(0.5f);
			Level.EndCutscene();
			EndTalking(Level);
		}

		private IEnumerator StartLaughing()
		{
			Sprite.Play("laugh");
			yield return null;
		}

		private IEnumerator StopLaughing()
		{
			Sprite.Play("idle");
			yield return null;
		}

		private void EndTalking(Level level)
		{
			if (player != null)
			{
				player.StateMachine.State = 0;
				player.ForceCameraUpdate = false;
			}
			conversation++;
			if (talkRoutine != null)
			{
				talkRoutine.RemoveSelf();
				talkRoutine = null;
			}
			Sprite.Play("idle");
		}
	}
}
