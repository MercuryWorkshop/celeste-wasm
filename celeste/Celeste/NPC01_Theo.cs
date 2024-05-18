using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class NPC01_Theo : NPC
	{
		public static ParticleType P_YOLO;

		private const string DoneTalking = "theoDoneTalking";

		private int currentConversation;

		private Coroutine talkRoutine;

		public NPC01_Theo(Vector2 position)
			: base(position)
		{
			Add(Sprite = GFX.SpriteBank.Create("theo"));
			Sprite.Play("idle");
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			currentConversation = base.Session.GetCounter("theo");
			if (!base.Session.GetFlag("theoDoneTalking"))
			{
				Add(Talker = new TalkComponent(new Rectangle(-8, -8, 88, 8), new Vector2(0f, -24f), OnTalk));
			}
		}

		private void OnTalk(Player player)
		{
			Level.StartCutscene(OnTalkEnd);
			Add(talkRoutine = new Coroutine(Talk(player)));
		}

		private IEnumerator Talk(Player player)
		{
			if (currentConversation == 0)
			{
				yield return PlayerApproachRightSide(player);
				yield return Textbox.Say("CH1_THEO_A", base.PlayerApproach48px);
			}
			else if (currentConversation == 1)
			{
				yield return PlayerApproachRightSide(player);
				yield return 0.2f;
				yield return PlayerApproach(player, turnToFace: true, 48f);
				yield return Textbox.Say("CH1_THEO_B");
			}
			else if (currentConversation == 2)
			{
				yield return PlayerApproachRightSide(player, turnToFace: true, 48f);
				yield return Textbox.Say("CH1_THEO_C");
			}
			else if (currentConversation == 3)
			{
				yield return PlayerApproachRightSide(player, turnToFace: true, 48f);
				yield return Textbox.Say("CH1_THEO_D");
			}
			else if (currentConversation == 4)
			{
				yield return PlayerApproachRightSide(player, turnToFace: true, 48f);
				yield return Textbox.Say("CH1_THEO_E");
			}
			else if (currentConversation == 5)
			{
				yield return PlayerApproachRightSide(player, turnToFace: true, 48f);
				yield return Textbox.Say("CH1_THEO_F", Yolo);
				Sprite.Play("yoloEnd");
				Remove(Talker);
				yield return Level.ZoomBack(0.5f);
			}
			Level.EndCutscene();
			OnTalkEnd(Level);
		}

		private void OnTalkEnd(Level level)
		{
			if (currentConversation == 0)
			{
				SaveData.Instance.SetFlag("MetTheo");
			}
			else if (currentConversation == 1)
			{
				SaveData.Instance.SetFlag("TheoKnowsName");
			}
			else if (currentConversation == 5)
			{
				base.Session.SetFlag("theoDoneTalking");
				Remove(Talker);
			}
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				player.StateMachine.Locked = false;
				player.StateMachine.State = 0;
			}
			base.Session.IncrementCounter("theo");
			currentConversation++;
			talkRoutine.Cancel();
			talkRoutine.RemoveSelf();
			Sprite.Play("idle");
		}

		private IEnumerator Yolo()
		{
			yield return Level.ZoomTo(new Vector2(128f, 128f), 2f, 0.5f);
			yield return 0.2f;
			Audio.Play("event:/char/theo/yolo_fist", Position);
			Sprite.Play("yolo");
			yield return 0.1f;
			Level.DirectionalShake(-Vector2.UnitY);
			Level.ParticlesFG.Emit(P_YOLO, 6, Position + new Vector2(-3f, -24f), Vector2.One * 4f);
			yield return 0.5f;
		}
	}
}
