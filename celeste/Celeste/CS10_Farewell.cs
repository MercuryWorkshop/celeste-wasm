using System.Collections;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS10_Farewell : CutsceneEntity
	{
		private Player player;

		private NPC granny;

		private float fade;

		private Coroutine grannyWalk;

		private EventInstance snapshot;

		private EventInstance dissipate;

		public CS10_Farewell(Player player)
			: base(fadeInOnSkip: false)
		{
			this.player = player;
			base.Depth = -1000000;
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			Level obj = scene as Level;
			obj.TimerStopped = true;
			obj.TimerHidden = true;
			obj.SaveQuitDisabled = true;
			obj.SnapColorGrade("none");
			snapshot = Audio.CreateSnapshot("snapshot:/game_10_granny_clouds_dialogue");
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			player.Dashes = 1;
			player.StateMachine.State = 11;
			player.Sprite.Play("idle");
			player.Visible = false;
			Audio.SetMusic("event:/new_content/music/lvl10/granny_farewell");
			FadeWipe wipe = new FadeWipe(Level, wipeIn: true);
			wipe.Duration = 2f;
			ScreenWipe.WipeColor = Color.White;
			yield return wipe.Duration;
			yield return 1.5f;
			Add(new Coroutine(Level.ZoomTo(new Vector2(160f, 125f), 2f, 5f)));
			yield return 0.2f;
			Audio.Play("event:/new_content/char/madeline/screenentry_gran");
			yield return 0.3f;
			_ = player.Position;
			player.Position = new Vector2(player.X, level.Bounds.Bottom + 8);
			player.Speed.Y = -160f;
			player.Visible = true;
			player.DummyGravity = false;
			player.MuffleLanding = true;
			Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
			while (!player.OnGround() || player.Speed.Y < 0f)
			{
				float y = player.Speed.Y;
				player.Speed.Y += Engine.DeltaTime * 900f * 0.2f;
				if (y < 0f && player.Speed.Y >= 0f)
				{
					player.Speed.Y = 0f;
					yield return 0.2f;
				}
				yield return null;
			}
			Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
			Audio.Play("event:/new_content/char/madeline/screenentry_gran_landing", player.Position);
			granny = new NPC(player.Position + new Vector2(164f, 0f));
			granny.IdleAnim = "idle";
			granny.MoveAnim = "walk";
			granny.Maxspeed = 15f;
			granny.Add(granny.Sprite = GFX.SpriteBank.Create("granny"));
			GrannyLaughSfx grannyLaugh = new GrannyLaughSfx(granny.Sprite);
			grannyLaugh.FirstPlay = false;
			granny.Add(grannyLaugh);
			granny.Sprite.OnFrameChange = delegate(string anim)
			{
				int currentAnimationFrame = granny.Sprite.CurrentAnimationFrame;
				if (anim == "walk" && currentAnimationFrame == 2)
				{
					float volume = Calc.ClampedMap((player.Position - granny.Position).Length(), 64f, 128f, 1f, 0f);
					Audio.Play("event:/new_content/char/granny/cane_tap_ending", granny.Position).setVolume(volume);
				}
			};
			base.Scene.Add(granny);
			grannyWalk = new Coroutine(granny.MoveTo(player.Position + new Vector2(32f, 0f)));
			Add(grannyWalk);
			yield return 2f;
			player.Facing = Facings.Left;
			yield return 1.6f;
			player.Facing = Facings.Right;
			yield return 0.8f;
			yield return player.DummyWalkToExact((int)player.X + 4, walkBackwards: false, 0.4f);
			yield return 0.8f;
			yield return Textbox.Say("CH9_FAREWELL", Laugh, StopLaughing, StepForward, GrannyDisappear, FadeToWhite, WaitForGranny);
			yield return 2f;
			while (fade < 1f)
			{
				yield return null;
			}
			EndCutscene(level);
		}

		private IEnumerator WaitForGranny()
		{
			while (grannyWalk != null && !grannyWalk.Finished)
			{
				yield return null;
			}
		}

		private IEnumerator Laugh()
		{
			granny.Sprite.Play("laugh");
			yield break;
		}

		private IEnumerator StopLaughing()
		{
			granny.Sprite.Play("idle");
			yield break;
		}

		private IEnumerator StepForward()
		{
			yield return player.DummyWalkToExact((int)player.X + 8, walkBackwards: false, 0.4f);
		}

		private IEnumerator GrannyDisappear()
		{
			Audio.SetMusicParam("end", 1f);
			Add(new Coroutine(player.DummyWalkToExact((int)player.X + 8, walkBackwards: false, 0.4f)));
			yield return 0.1f;
			dissipate = Audio.Play("event:/new_content/char/granny/dissipate", granny.Position);
			MTexture tex = granny.Sprite.GetFrame(granny.Sprite.CurrentAnimationID, granny.Sprite.CurrentAnimationFrame);
			Level.Add(new DisperseImage(granny.Position, new Vector2(1f, -0.1f), granny.Sprite.Origin, granny.Sprite.Scale, tex));
			yield return null;
			granny.Visible = false;
			yield return 3.5f;
		}

		private IEnumerator FadeToWhite()
		{
			Add(new Coroutine(DoFadeToWhite()));
			yield break;
		}

		private IEnumerator DoFadeToWhite()
		{
			Add(new Coroutine(Level.ZoomBack(8f)));
			while (fade < 1f)
			{
				fade = Calc.Approach(fade, 1f, Engine.DeltaTime / 8f);
				yield return null;
			}
		}

		public override void OnEnd(Level level)
		{
			Dispose();
			if (WasSkipped)
			{
				Audio.Stop(dissipate);
			}
			Level.OnEndOfFrame += delegate
			{
				Achievements.Register(Achievement.FAREWELL);
				Level.TeleportTo(player, "end-cinematic", Player.IntroTypes.Transition);
			};
		}

		public override void SceneEnd(Scene scene)
		{
			base.SceneEnd(scene);
			Dispose();
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);
			Dispose();
		}

		private void Dispose()
		{
			Audio.ReleaseSnapshot(snapshot);
			snapshot = null;
		}

		public override void Render()
		{
			if (fade > 0f)
			{
				Draw.Rect(Level.Camera.X - 1f, Level.Camera.Y - 1f, 322f, 182f, Color.White * fade);
			}
		}
	}
}
