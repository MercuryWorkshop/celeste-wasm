using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS03_OshiroHallway2 : CutsceneEntity
	{
		public const string Flag = "oshiro_resort_talked_3";

		private Player player;

		private NPC oshiro;

		public CS03_OshiroHallway2(Player player, NPC oshiro)
		{
			this.player = player;
			this.oshiro = oshiro;
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			level.Session.Audio.Music.Layer(1, value: false);
			level.Session.Audio.Music.Layer(2, value: true);
			level.Session.Audio.Apply();
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			yield return Textbox.Say("CH3_OSHIRO_HALLWAY_B");
			oshiro.MoveToAndRemove(new Vector2(level.Bounds.Right + 64, oshiro.Y));
			oshiro.Add(new SoundSource("event:/char/oshiro/move_03_08a_exit"));
			yield return 1f;
			EndCutscene(level);
		}

		public override void OnEnd(Level level)
		{
			level.Session.Audio.Music.Layer(1, value: true);
			level.Session.Audio.Music.Layer(2, value: false);
			level.Session.Audio.Apply();
			player.StateMachine.Locked = false;
			player.StateMachine.State = 0;
			level.Session.SetFlag("oshiro_resort_talked_3");
			if (WasSkipped)
			{
				level.Remove(oshiro);
			}
		}
	}
}
