using System.Collections;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS10_BadelineHelps : CutsceneEntity
	{
		public const string Flag = "badeline_helps";

		private Player player;

		private BadelineDummy badeline;

		private EventInstance? entrySfx;

		public CS10_BadelineHelps(Player player)
		{
			base.Depth = -8500;
			this.player = player;
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			Vector2 spawn = level.GetSpawnPoint(player.Position);
			player.Dashes = 2;
			player.StateMachine.State = 11;
			player.DummyGravity = false;
			entrySfx = Audio.Play("event:/new_content/char/madeline/screenentry_stubborn", player.Position);
			yield return player.MoonLanding(spawn);
			yield return level.ZoomTo(new Vector2(spawn.X - level.Camera.X, 134f), 2f, 0.5f);
			yield return 1f;
			yield return BadelineAppears();
			yield return 0.3f;
			yield return Textbox.Say("CH9_HELPING_HAND", MadelineFacesAway, MadelineFacesBadeline, MadelineStepsForwards);
			if (badeline != null)
			{
				yield return BadelineVanishes();
			}
			yield return level.ZoomBack(0.5f);
			EndCutscene(level);
		}

		private IEnumerator BadelineAppears()
		{
			StartMusic();
			Audio.Play("event:/char/badeline/maddy_split", player.Position);
			Level.Add(badeline = new BadelineDummy(player.Center));
			Level.Displacement.AddBurst(badeline.Center, 0.5f, 8f, 32f, 0.5f);
			player.Dashes = 1;
			badeline.Sprite.Scale.X = -1f;
			yield return badeline.FloatTo(player.Center + new Vector2(18f, -10f), -1, faceDirection: false);
			yield return 0.2f;
			player.Facing = Facings.Right;
			yield return null;
		}

		private IEnumerator MadelineFacesAway()
		{
			Level.NextColorGrade("feelingdown", 0.1f);
			yield return player.DummyWalkTo(player.X - 16f);
		}

		private IEnumerator MadelineFacesBadeline()
		{
			player.Facing = Facings.Right;
			yield return 0.2f;
		}

		private IEnumerator MadelineStepsForwards()
		{
			Vector2 spawn = Level.GetSpawnPoint(player.Position);
			Add(new Coroutine(player.DummyWalkToExact((int)spawn.X)));
			yield return 0.1f;
			yield return badeline.FloatTo(badeline.Position + new Vector2(20f, 0f), null, faceDirection: false);
		}

		private IEnumerator BadelineVanishes()
		{
			yield return 0.2f;
			badeline.Vanish();
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			badeline = null;
			yield return 0.2f;
		}

		private void StartMusic()
		{
			if (Level.Session.Audio.Music.Event != "event:/new_content/music/lvl10/cassette_rooms")
			{
				int note = 0;
				CassetteBlockManager blockManager = Level.Tracker.GetEntity<CassetteBlockManager>();
				if (blockManager != null)
				{
					note = blockManager.GetSixteenthNote();
				}
				Level.Session.Audio.Music.Event = "event:/new_content/music/lvl10/cassette_rooms";
				Level.Session.Audio.Music.Param("sixteenth_note", note);
				Level.Session.Audio.Apply(forceSixteenthNoteHack: true);
				Level.Session.Audio.Music.Param("sixteenth_note", 7f);
			}
		}

		public override void OnEnd(Level level)
		{
			Level.Session.Inventory.Dashes = 1;
			player.Dashes = 1;
			player.Depth = 0;
			player.Dashes = 1;
			player.Speed = Vector2.Zero;
			player.Position = level.GetSpawnPoint(player.Position);
			player.Position -= Vector2.UnitY * 12f;
			player.MoveVExact(100);
			player.Active = true;
			player.Visible = true;
			player.StateMachine.State = 0;
			if (badeline != null)
			{
				badeline.RemoveSelf();
			}
			level.ResetZoom();
			level.Session.SetFlag("badeline_helps");
			if (WasSkipped)
			{
				Audio.Stop(entrySfx);
				StartMusic();
				level.SnapColorGrade("feelingdown");
			}
		}
	}
}
