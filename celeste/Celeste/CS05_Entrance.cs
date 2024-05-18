using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS05_Entrance : CutsceneEntity
	{
		public const string Flag = "entrance";

		private NPC theo;

		private Player player;

		private Vector2 playerMoveTo;

		public CS05_Entrance(NPC theo)
		{
			this.theo = theo;
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			player = level.Tracker.GetEntity<Player>();
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			player.X = theo.X - 32f;
			playerMoveTo = new Vector2(theo.X - 32f, player.Y);
			player.Facing = Facings.Left;
			SpotlightWipe.FocusPoint = theo.TopCenter - Vector2.UnitX * 16f - level.Camera.Position;
			yield return 2f;
			player.Facing = Facings.Right;
			yield return 0.3f;
			yield return theo.MoveTo(new Vector2(theo.X + 48f, theo.Y));
			yield return Textbox.Say("ch5_entrance", MaddyTurnsRight, TheoTurns, TheoLeaves);
			EndCutscene(level);
		}

		private IEnumerator MaddyTurnsRight()
		{
			player.Facing = Facings.Right;
			yield break;
		}

		private IEnumerator TheoTurns()
		{
			theo.Sprite.Scale.X *= -1f;
			yield break;
		}

		private IEnumerator TheoLeaves()
		{
			yield return theo.MoveTo(new Vector2(Level.Bounds.Right + 32, theo.Y));
		}

		public override void OnEnd(Level level)
		{
			if (player != null)
			{
				player.StateMachine.Locked = false;
				player.StateMachine.State = 0;
				player.ForceCameraUpdate = false;
				player.Position = playerMoveTo;
				player.Facing = Facings.Right;
			}
			base.Scene.Remove(theo);
			level.Session.SetFlag("entrance");
		}
	}
}
