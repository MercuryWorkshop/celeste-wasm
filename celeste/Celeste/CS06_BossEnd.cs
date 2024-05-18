using System.Collections;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class CS06_BossEnd : CutsceneEntity
	{
		public const string Flag = "badeline_connection";

		private Player player;

		private NPC06_Badeline_Crying badeline;

		private float fade;

		private float pictureFade;

		private float pictureGlow;

		private MTexture picture;

		private bool waitForKeyPress;

		private float timer;

		private EventInstance? sfx;

		public CS06_BossEnd(Player player, NPC06_Badeline_Crying badeline)
		{
			base.Tag = Tags.HUD;
			this.player = player;
			this.badeline = badeline;
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			while (!player.OnGround())
			{
				yield return null;
			}
			player.Facing = Facings.Right;
			yield return 1f;
			Level level2 = SceneAs<Level>();
			level2.Session.Audio.Music.Event = "event:/music/lvl6/badeline_acoustic";
			level2.Session.Audio.Apply();
			yield return Textbox.Say("ch6_boss_ending", StartMusic, PlayerHug, BadelineCalmDown);
			yield return 0.5f;
			while ((fade += Engine.DeltaTime) < 1f)
			{
				yield return null;
			}
			picture = GFX.Portraits["hug1"];
			sfx = Audio.Play("event:/game/06_reflection/hug_image_1");
			yield return PictureFade(1f);
			yield return WaitForPress();
			sfx = Audio.Play("event:/game/06_reflection/hug_image_2");
			yield return PictureFade(0f, 0.5f);
			picture = GFX.Portraits["hug2"];
			yield return PictureFade(1f);
			yield return WaitForPress();
			sfx = Audio.Play("event:/game/06_reflection/hug_image_3");
			while ((pictureGlow += Engine.DeltaTime / 2f) < 1f)
			{
				yield return null;
			}
			yield return 0.2f;
			yield return PictureFade(0f, 0.5f);
			while ((fade -= Engine.DeltaTime * 12f) > 0f)
			{
				yield return null;
			}
			level.Session.Audio.Music.Param("levelup", 1f);
			level.Session.Audio.Apply();
			Add(new Coroutine(badeline.TurnWhite(1f)));
			yield return 0.5f;
			player.Sprite.Play("idle");
			yield return 0.25f;
			yield return player.DummyWalkToExact((int)player.X - 8, walkBackwards: true);
			Add(new Coroutine(CenterCameraOnPlayer()));
			yield return badeline.Disperse();
			(base.Scene as Level).Session.SetFlag("badeline_connection");
			level.Flash(Color.White);
			level.Session.Inventory.Dashes = 2;
			badeline.RemoveSelf();
			yield return 0.1f;
			level.Add(new LevelUpEffect(player.Position));
			Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
			yield return 2f;
			yield return level.ZoomBack(0.5f);
			EndCutscene(level);
		}

		private IEnumerator StartMusic()
		{
			Level level = SceneAs<Level>();
			level.Session.Audio.Music.Event = "event:/music/lvl6/badeline_acoustic";
			level.Session.Audio.Apply();
			yield return 0.5f;
		}

		private IEnumerator PlayerHug()
		{
			Add(new Coroutine(Level.ZoomTo(badeline.Center + new Vector2(0f, -24f) - Level.Camera.Position, 2f, 0.5f)));
			yield return 0.6f;
			yield return player.DummyWalkToExact((int)badeline.X - 10);
			player.Facing = Facings.Right;
			yield return 0.25f;
			player.DummyAutoAnimate = false;
			player.Sprite.Play("hug");
			yield return 0.5f;
		}

		private IEnumerator BadelineCalmDown()
		{
			Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "postboss", 0f);
			badeline.LoopingSfx.Param("end", 1f);
			yield return 0.5f;
			badeline.Sprite.Play("scaredTransition");
			Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
			FinalBossStarfield bossBg = Level.Background.Get<FinalBossStarfield>();
			if (bossBg != null)
			{
				while (bossBg.Alpha > 0f)
				{
					bossBg.Alpha -= Engine.DeltaTime;
					yield return null;
				}
			}
			yield return 1.5f;
		}

		private IEnumerator CenterCameraOnPlayer()
		{
			yield return 0.5f;
			Vector2 from = Level.ZoomFocusPoint;
			Vector2 to = new Vector2(Level.Bounds.Left + 580, Level.Bounds.Top + 124) - Level.Camera.Position;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime)
			{
				Level.ZoomFocusPoint = from + (to - from) * Ease.SineInOut(p);
				yield return null;
			}
		}

		private IEnumerator PictureFade(float to, float duration = 1f)
		{
			while ((pictureFade = Calc.Approach(pictureFade, to, Engine.DeltaTime / duration)) != to)
			{
				yield return null;
			}
		}

		private IEnumerator WaitForPress()
		{
			waitForKeyPress = true;
			while (!Input.MenuConfirm.Pressed)
			{
				yield return null;
			}
			waitForKeyPress = false;
		}

		public override void OnEnd(Level level)
		{
			if (WasSkipped && sfx != null)
			{
				Audio.Stop(sfx);
			}
			Audio.SetParameter(Audio.CurrentAmbienceEventInstance, "postboss", 0f);
			level.ResetZoom();
			level.Session.Inventory.Dashes = 2;
			level.Session.Audio.Music.Event = "event:/music/lvl6/badeline_acoustic";
			if (WasSkipped)
			{
				level.Session.Audio.Music.Param("levelup", 2f);
			}
			level.Session.Audio.Apply();
			if (WasSkipped)
			{
				level.Add(new LevelUpEffect(player.Position));
			}
			player.DummyAutoAnimate = true;
			player.StateMachine.Locked = false;
			player.StateMachine.State = 0;
			FinalBossStarfield bossBg = Level.Background.Get<FinalBossStarfield>();
			if (bossBg != null)
			{
				bossBg.Alpha = 0f;
			}
			badeline.RemoveSelf();
			level.Session.SetFlag("badeline_connection");
		}

		public override void Update()
		{
			timer += Engine.DeltaTime;
			base.Update();
		}

		public override void Render()
		{
			if (!(fade > 0f))
			{
				return;
			}
			Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * Ease.CubeOut(fade) * 0.8f);
			if (picture != null && pictureFade > 0f)
			{
				float e = Ease.CubeOut(pictureFade);
				Vector2 pos = new Vector2(960f, 540f);
				float scale = 1f + (1f - e) * 0.025f;
				picture.DrawCentered(pos, Color.White * Ease.CubeOut(pictureFade), scale, 0f);
				if (pictureGlow > 0f)
				{
					GFX.Portraits["hug-light2a"].DrawCentered(pos, Color.White * Ease.CubeOut(pictureFade * pictureGlow), scale);
					GFX.Portraits["hug-light2b"].DrawCentered(pos, Color.White * Ease.CubeOut(pictureFade * pictureGlow), scale);
					GFX.Portraits["hug-light2c"].DrawCentered(pos, Color.White * Ease.CubeOut(pictureFade * pictureGlow), scale);
					HiresRenderer.EndRender();
					HiresRenderer.BeginRender(BlendState.Additive);
					GFX.Portraits["hug-light2a"].DrawCentered(pos, Color.White * Ease.CubeOut(pictureFade * pictureGlow), scale);
					GFX.Portraits["hug-light2b"].DrawCentered(pos, Color.White * Ease.CubeOut(pictureFade * pictureGlow), scale);
					GFX.Portraits["hug-light2c"].DrawCentered(pos, Color.White * Ease.CubeOut(pictureFade * pictureGlow), scale);
					HiresRenderer.EndRender();
					HiresRenderer.BeginRender();
				}
				if (waitForKeyPress)
				{
					GFX.Gui["textboxbutton"].DrawCentered(new Vector2(1520f, 880 + ((timer % 1f < 0.25f) ? 6 : 0)));
				}
			}
		}
	}
}
