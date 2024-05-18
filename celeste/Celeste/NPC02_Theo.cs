using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class NPC02_Theo : NPC
	{
		private const string DoneTalking = "theoDoneTalking";

		private const string HadntMetAtStart = "hadntMetTheoAtStart";

		private Coroutine talkRoutine;

		private Selfie selfie;

		private int CurrentConversation
		{
			get
			{
				return base.Session.GetCounter("theo");
			}
			set
			{
				base.Session.SetCounter("theo", value);
			}
		}

		public NPC02_Theo(Vector2 position)
			: base(position)
		{
			Add(Sprite = GFX.SpriteBank.Create("theo"));
			Sprite.Play("idle");
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (!base.Session.GetFlag("theoDoneTalking"))
			{
				Add(Talker = new TalkComponent(new Rectangle(-20, -8, 100, 8), new Vector2(0f, -24f), OnTalk));
			}
		}

		private void OnTalk(Player player)
		{
			if (!SaveData.Instance.HasFlag("MetTheo") || !SaveData.Instance.HasFlag("TheoKnowsName"))
			{
				CurrentConversation = -1;
			}
			Level.StartCutscene(OnTalkEnd);
			Add(talkRoutine = new Coroutine(Talk(player)));
		}

		private IEnumerator Talk(Player player)
		{
			if (!SaveData.Instance.HasFlag("MetTheo"))
			{
				base.Session.SetFlag("hadntMetTheoAtStart");
				SaveData.Instance.SetFlag("MetTheo");
				yield return PlayerApproachRightSide(player, turnToFace: true, 48f);
				yield return Textbox.Say("CH2_THEO_INTRO_NEVER_MET");
			}
			else if (!SaveData.Instance.HasFlag("TheoKnowsName"))
			{
				base.Session.SetFlag("hadntMetTheoAtStart");
				SaveData.Instance.SetFlag("TheoKnowsName");
				yield return PlayerApproachRightSide(player, turnToFace: true, 48f);
				yield return Textbox.Say("CH2_THEO_INTRO_NEVER_INTRODUCED");
			}
			else if (CurrentConversation <= 0)
			{
				yield return PlayerApproachRightSide(player);
				yield return 0.2f;
				if (base.Session.GetFlag("hadntMetTheoAtStart"))
				{
					yield return PlayerApproach48px();
					yield return Textbox.Say("CH2_THEO_A", ShowPhotos, HidePhotos, Selfie);
				}
				else
				{
					yield return Textbox.Say("CH2_THEO_A_EXT", ShowPhotos, HidePhotos, Selfie, base.PlayerApproach48px);
				}
			}
			else if (CurrentConversation == 1)
			{
				yield return PlayerApproachRightSide(player, turnToFace: true, 48f);
				yield return Textbox.Say("CH2_THEO_B", SelfieFiltered);
			}
			else if (CurrentConversation == 2)
			{
				yield return PlayerApproachRightSide(player, turnToFace: true, 48f);
				yield return Textbox.Say("CH2_THEO_C");
			}
			else if (CurrentConversation == 3)
			{
				yield return PlayerApproachRightSide(player, turnToFace: true, 48f);
				yield return Textbox.Say("CH2_THEO_D");
			}
			else if (CurrentConversation == 4)
			{
				yield return PlayerApproachRightSide(player, turnToFace: true, 48f);
				yield return Textbox.Say("CH2_THEO_E");
			}
			Level.EndCutscene();
			OnTalkEnd(Level);
		}

		private void OnTalkEnd(Level level)
		{
			if (CurrentConversation == 4)
			{
				base.Session.SetFlag("theoDoneTalking");
				Remove(Talker);
			}
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				player.StateMachine.Locked = false;
				player.StateMachine.State = 0;
				if (level.SkippingCutscene)
				{
					player.X = (int)(base.X + 48f);
					player.Facing = Facings.Left;
				}
			}
			Sprite.Scale.X = 1f;
			if (selfie != null)
			{
				selfie.RemoveSelf();
			}
			CurrentConversation++;
			talkRoutine.Cancel();
			talkRoutine.RemoveSelf();
		}

		private IEnumerator ShowPhotos()
		{
			Player player = base.Scene.Tracker.GetEntity<Player>();
			yield return PlayerApproach(player, turnToFace: true, 10f);
			Sprite.Play("getPhone");
			yield return 2f;
		}

		private IEnumerator HidePhotos()
		{
			Sprite.Play("idle");
			yield return 0.5f;
		}

		private IEnumerator Selfie()
		{
			yield return 0.5f;
			Audio.Play("event:/game/02_old_site/theoselfie_foley", Position);
			Sprite.Scale.X = 0f - Sprite.Scale.X;
			Sprite.Play("takeSelfie");
			yield return 1f;
			base.Scene.Add(selfie = new Selfie(SceneAs<Level>()));
			yield return selfie.PictureRoutine();
			selfie = null;
			Sprite.Scale.X = 0f - Sprite.Scale.X;
		}

		private IEnumerator SelfieFiltered()
		{
			base.Scene.Add(selfie = new Selfie(SceneAs<Level>()));
			yield return selfie.FilterRoutine();
			selfie = null;
		}
	}
}
