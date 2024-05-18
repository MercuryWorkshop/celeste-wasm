using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class NPC03_Oshiro_Suite : NPC
	{
		private const string ConversationCounter = "oshiroSuiteSadConversation";

		private bool finishedTalking;

		public NPC03_Oshiro_Suite(Vector2 position)
			: base(position)
		{
			Add(Sprite = new OshiroSprite(1));
			Add(Light = new VertexLight(-Vector2.UnitY * 16f, Color.White, 1f, 32, 64));
			Add(Talker = new TalkComponent(new Rectangle(-16, -8, 32, 8), new Vector2(0f, -24f), OnTalk));
			Talker.Enabled = false;
			MoveAnim = "move";
			IdleAnim = "idle";
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (!base.Session.GetFlag("oshiro_resort_suite"))
			{
				base.Scene.Add(new CS03_OshiroMasterSuite(this));
				return;
			}
			Sprite.Play("idle_ground");
			Talker.Enabled = true;
		}

		private void OnTalk(Player player)
		{
			finishedTalking = false;
			Level.StartCutscene(EndTalking);
			Add(new Coroutine(Talk(player)));
		}

		private IEnumerator Talk(Player player)
		{
			int conversation = base.Session.GetCounter("oshiroSuiteSadConversation");
			yield return PlayerApproach(player, turnToFace: false, 12f);
			yield return Textbox.Say("CH3_OSHIRO_SUITE_SAD" + conversation);
			yield return PlayerLeave(player);
			EndTalking(SceneAs<Level>());
		}

		private void EndTalking(Level level)
		{
			Player player = base.Scene.Entities.FindFirst<Player>();
			if (player != null)
			{
				player.StateMachine.Locked = false;
				player.StateMachine.State = 0;
			}
			if (!finishedTalking)
			{
				int conversation = base.Session.GetCounter("oshiroSuiteSadConversation");
				conversation++;
				conversation %= 7;
				if (conversation == 0)
				{
					conversation++;
				}
				base.Session.SetCounter("oshiroSuiteSadConversation", conversation);
				finishedTalking = true;
			}
		}
	}
}
