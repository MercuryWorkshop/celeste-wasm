using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS05_Badeline : CutsceneEntity
	{
		private Player player;

		private NPC05_Badeline npc;

		private BadelineDummy badeline;

		private int index;

		private bool moved;

		public static string GetFlag(int index)
		{
			return "badeline_" + index;
		}

		public CS05_Badeline(Player player, NPC05_Badeline npc, BadelineDummy badeline, int index)
		{
			this.player = player;
			this.npc = npc;
			this.badeline = badeline;
			this.index = index;
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			yield return 0.25f;
			if (index == 3)
			{
				player.DummyAutoAnimate = false;
				player.Sprite.Play("tired");
				yield return 0.2f;
			}
			while (player.Scene != null && !player.OnGround())
			{
				yield return null;
			}
			Vector2 zoomAt = (badeline.Center + player.Center) * 0.5f - Level.Camera.Position + new Vector2(0f, -12f);
			yield return Level.ZoomTo(zoomAt, 2f, 0.5f);
			yield return Textbox.Say("ch5_shadow_maddy_" + index, BadelineLeaves);
			if (!moved)
			{
				npc.MoveToNode(index);
			}
			yield return Level.ZoomBack(0.5f);
			EndCutscene(level);
		}

		public override void OnEnd(Level level)
		{
			npc.SnapToNode(index);
			player.StateMachine.Locked = false;
			player.StateMachine.State = 0;
			level.Session.SetFlag(GetFlag(index));
		}

		private IEnumerator BadelineLeaves()
		{
			yield return 0.1f;
			moved = true;
			npc.MoveToNode(index);
			yield return 0.5f;
			player.Sprite.Play("tiredStill");
			yield return 0.5f;
			player.Sprite.Play("idle");
			yield return 0.6f;
		}
	}
}
