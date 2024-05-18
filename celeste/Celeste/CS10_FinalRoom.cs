using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS10_FinalRoom : CutsceneEntity
	{
		private Player player;

		private BadelineDummy badeline;

		private bool first;

		public CS10_FinalRoom(Player player, bool first)
		{
			base.Depth = -8500;
			this.player = player;
			this.first = first;
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			player.StateMachine.State = 11;
			if (first)
			{
				yield return player.DummyWalkToExact((int)(player.X + 16f));
				yield return 0.5f;
			}
			else
			{
				player.DummyAutoAnimate = false;
				player.Sprite.Play("sitDown");
				player.Sprite.SetAnimationFrame(player.Sprite.CurrentAnimationTotalFrames - 1);
				yield return 1.25f;
			}
			yield return BadelineAppears();
			if (first)
			{
				yield return Textbox.Say("CH9_LAST_ROOM");
			}
			else
			{
				yield return Textbox.Say("CH9_LAST_ROOM_ALT");
			}
			yield return BadelineVanishes();
			EndCutscene(level);
		}

		private IEnumerator BadelineAppears()
		{
			Level.Add(badeline = new BadelineDummy(player.Position + new Vector2(18f, -8f)));
			Level.Displacement.AddBurst(badeline.Center, 0.5f, 8f, 32f, 0.5f);
			Audio.Play("event:/char/badeline/maddy_split", badeline.Position);
			badeline.Sprite.Scale.X = -1f;
			yield return null;
		}

		private IEnumerator BadelineVanishes()
		{
			yield return 0.2f;
			badeline.Vanish();
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			badeline = null;
			yield return 0.5f;
			player.Facing = Facings.Right;
		}

		public override void OnEnd(Level level)
		{
			Level.Session.Inventory.Dashes = 1;
			player.StateMachine.State = 0;
			if (!first && !WasSkipped)
			{
				Audio.Play("event:/char/madeline/stand", player.Position);
			}
			if (badeline != null)
			{
				badeline.RemoveSelf();
			}
		}
	}
}
