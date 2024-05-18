using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS03_OshiroClutter : CutsceneEntity
	{
		private int index;

		private Player player;

		private NPC03_Oshiro_Cluttter oshiro;

		private List<ClutterDoor> doors;

		public CS03_OshiroClutter(Player player, NPC03_Oshiro_Cluttter oshiro, int index)
		{
			this.player = player;
			this.oshiro = oshiro;
			this.index = index;
		}

		public override void OnBegin(Level level)
		{
			doors = base.Scene.Entities.FindAll<ClutterDoor>();
			doors.Sort((ClutterDoor a, ClutterDoor b) => (int)(a.Y - b.Y));
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			int side = ((index != 1 && index != 2) ? 1 : (-1));
			if (side == -1)
			{
				yield return player.DummyWalkToExact((int)oshiro.X - 24);
				player.Facing = Facings.Right;
				oshiro.Sprite.Scale.X = -1f;
			}
			else
			{
				Add(new Coroutine(oshiro.PaceRight()));
				yield return player.DummyWalkToExact((int)oshiro.HomePosition.X + 24);
				player.Facing = Facings.Left;
				oshiro.Sprite.Scale.X = 1f;
			}
			if (index < 3)
			{
				yield return Level.ZoomTo(oshiro.ZoomPoint, 2f, 0.5f);
				yield return Textbox.Say("CH3_OSHIRO_CLUTTER" + index, Collapse, oshiro.PaceLeft, oshiro.PaceRight);
				yield return Level.ZoomBack(0.5f);
				level.Session.SetFlag("oshiro_clutter_door_open");
				if (index == 0)
				{
					SetMusic();
				}
				foreach (ClutterDoor door in doors)
				{
					if (!door.IsLocked(level.Session))
					{
						yield return door.UnlockRoutine();
					}
				}
			}
			else
			{
				yield return CutsceneEntity.CameraTo(new Vector2(Level.Bounds.X, Level.Bounds.Y), 0.5f);
				yield return Level.ZoomTo(new Vector2(90f, 60f), 2f, 0.5f);
				yield return Textbox.Say("CH3_OSHIRO_CLUTTER_ENDING");
				yield return oshiro.MoveTo(new Vector2(oshiro.X, level.Bounds.Top - 32));
				oshiro.Add(new SoundSource("event:/char/oshiro/move_05_09b_exit"));
				yield return Level.ZoomBack(0.5f);
			}
			EndCutscene(level);
		}

		private IEnumerator Collapse()
		{
			Audio.Play("event:/char/oshiro/chat_collapse", oshiro.Position);
			oshiro.Sprite.Play("fall");
			yield return 0.5f;
		}

		private void SetMusic()
		{
			Level obj = base.Scene as Level;
			obj.Session.Audio.Music.Event = "event:/music/lvl3/clean";
			obj.Session.Audio.Music.Progress = 1;
			obj.Session.Audio.Apply();
		}

		public override void OnEnd(Level level)
		{
			player.StateMachine.Locked = false;
			player.StateMachine.State = 0;
			if (oshiro.Sprite.CurrentAnimationID == "side")
			{
				(oshiro.Sprite as OshiroSprite).Pop("idle", flip: true);
			}
			if (index < 3)
			{
				level.Session.SetFlag("oshiro_clutter_door_open");
				level.Session.SetFlag("oshiro_clutter_" + index);
				if (index == 0 && WasSkipped)
				{
					SetMusic();
				}
				foreach (ClutterDoor door in doors)
				{
					if (!door.IsLocked(level.Session))
					{
						door.InstantUnlock();
					}
				}
				if (WasSkipped && index == 0)
				{
					oshiro.Sprite.Play("idle_ground");
				}
			}
			else
			{
				level.Session.SetFlag("oshiro_clutter_finished");
				base.Scene.Remove(oshiro);
			}
		}
	}
}
