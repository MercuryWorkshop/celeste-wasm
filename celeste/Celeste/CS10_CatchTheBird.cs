using System;
using System.Collections;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS10_CatchTheBird : CutsceneEntity
	{
		private Player player;

		private FlingBirdIntro flingBird;

		private BadelineDummy badeline;

		private BirdNPC bird;

		private Vector2 birdWaitPosition;

		private EventInstance snapshot;

		public CS10_CatchTheBird(Player player, FlingBirdIntro flingBird)
		{
			this.player = player;
			this.flingBird = flingBird;
			birdWaitPosition = flingBird.BirdEndPosition;
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			Audio.SetMusic("event:/new_content/music/lvl10/cinematic/bird_crash_second");
			BadelineBoost boost = base.Scene.Entities.FindFirst<BadelineBoost>();
			if (boost != null)
			{
				boost.Active = (boost.Visible = (boost.Collidable = false));
			}
			yield return flingBird.DoGrabbingRoutine(player);
			flingBird.Sprite.Play("hurt");
			flingBird.X += 8f;
			while (!player.OnGround())
			{
				player.MoveVExact(1);
			}
			while (player.CollideCheck<Solid>())
			{
				player.Y--;
			}
			Engine.TimeRate = 0.65f;
			float ground = player.Position.Y;
			player.Dashes = 1;
			player.Sprite.Play("roll");
			player.Speed.X = 200f;
			player.DummyFriction = false;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime)
			{
				player.Speed.X = Calc.Approach(player.Speed.X, 0f, 160f * Engine.DeltaTime);
				if (player.Speed.X != 0f && base.Scene.OnInterval(0.1f))
				{
					Dust.BurstFG(player.Position, -(float)Math.PI / 2f, 2);
				}
				flingBird.Position.X += Engine.DeltaTime * 80f * Ease.CubeOut(1f - p);
				flingBird.Position.Y = ground;
				yield return null;
			}
			player.Speed.X = 0f;
			player.DummyFriction = true;
			player.DummyGravity = true;
			yield return 0.25f;
			while (Engine.TimeRate < 1f)
			{
				Engine.TimeRate = Calc.Approach(Engine.TimeRate, 1f, 4f * Engine.DeltaTime);
				yield return null;
			}
			player.ForceCameraUpdate = false;
			yield return 0.6f;
			player.Sprite.Play("rollGetUp");
			yield return 0.8f;
			level.Session.Audio.Music.Event = "event:/new_content/music/lvl10/reconciliation";
			level.Session.Audio.Apply();
			yield return Textbox.Say("CH9_CATCH_THE_BIRD", BirdLooksHurt, BirdSquakOnGround, ApproachBird, ApproachBirdAgain, BadelineAppears, WaitABeat, MadelineSits, BadelineHugs, StandUp, ShiftCameraToBird);
			yield return level.ZoomBack(0.5f);
			if (badeline != null)
			{
				badeline.Vanish();
			}
			yield return 0.5f;
			if (boost != null)
			{
				Level.Displacement.AddBurst(boost.Center, 0.5f, 8f, 32f, 0.5f);
				Audio.Play("event:/new_content/char/badeline/booster_first_appear", boost.Center);
				boost.Active = (boost.Visible = (boost.Collidable = true));
				yield return 0.2f;
			}
			EndCutscene(level);
		}

		private IEnumerator BirdTwitches(string sfx = null)
		{
			flingBird.Sprite.Scale.Y = 1.6f;
			if (!string.IsNullOrWhiteSpace(sfx))
			{
				Audio.Play(sfx, flingBird.Position);
			}
			while (flingBird.Sprite.Scale.Y > 1f)
			{
				flingBird.Sprite.Scale.Y = Calc.Approach(flingBird.Sprite.Scale.Y, 1f, 2f * Engine.DeltaTime);
				yield return null;
			}
		}

		private IEnumerator BirdLooksHurt()
		{
			yield return 0.8f;
			yield return BirdTwitches("event:/new_content/game/10_farewell/bird_crashscene_twitch_1");
			yield return 0.4f;
			yield return BirdTwitches("event:/new_content/game/10_farewell/bird_crashscene_twitch_2");
			yield return 0.5f;
		}

		private IEnumerator BirdSquakOnGround()
		{
			yield return 0.6f;
			yield return BirdTwitches("event:/new_content/game/10_farewell/bird_crashscene_twitch_3");
			yield return 0.8f;
			Audio.Play("event:/new_content/game/10_farewell/bird_crashscene_recover", flingBird.Position);
			flingBird.RemoveSelf();
			base.Scene.Add(bird = new BirdNPC(flingBird.Position, BirdNPC.Modes.None));
			bird.Facing = Facings.Right;
			bird.Sprite.Play("recover");
			yield return 0.6f;
			bird.Facing = Facings.Left;
			bird.Sprite.Play("idle");
			bird.X += 3f;
			yield return 0.4f;
			yield return bird.Caw();
		}

		private IEnumerator ApproachBird()
		{
			player.DummyAutoAnimate = true;
			yield return 0.25f;
			yield return bird.Caw();
			Add(new Coroutine(player.DummyWalkTo(player.X + 20f)));
			yield return 0.1f;
			Audio.Play("event:/game/general/bird_startle", bird.Position);
			yield return bird.Startle("event:/new_content/game/10_farewell/bird_crashscene_relocate");
			yield return bird.FlyTo(new Vector2(player.X + 80f, player.Y), 3f, relocateSfx: false);
		}

		private IEnumerator ApproachBirdAgain()
		{
			Audio.Play("event:/new_content/game/10_farewell/bird_crashscene_leave", bird.Position);
			Add(new Coroutine(bird.FlyTo(birdWaitPosition, 2f, relocateSfx: false)));
			yield return player.DummyWalkTo(player.X + 20f);
			snapshot = Audio.CreateSnapshot("snapshot:/game_10_bird_wings_silenced");
			yield return 0.8f;
			bird.RemoveSelf();
			base.Scene.Add(bird = new BirdNPC(birdWaitPosition, BirdNPC.Modes.WaitForLightningOff));
			bird.Facing = Facings.Right;
			bird.FlyAwayUp = false;
			bird.WaitForLightningPostDelay = 1f;
		}

		private IEnumerator BadelineAppears()
		{
			yield return player.DummyWalkToExact((int)player.X + 20, walkBackwards: false, 0.5f);
			Level.Add(badeline = new BadelineDummy(player.Position + new Vector2(24f, -8f)));
			Level.Displacement.AddBurst(badeline.Center, 0.5f, 8f, 32f, 0.5f);
			Audio.Play("event:/char/badeline/maddy_split", player.Position);
			badeline.Sprite.Scale.X = -1f;
			yield return 0.2f;
		}

		private IEnumerator WaitABeat()
		{
			yield return player.DummyWalkToExact((int)player.X - 4, walkBackwards: true, 0.5f);
			yield return 0.8f;
		}

		private IEnumerator MadelineSits()
		{
			yield return 0.5f;
			yield return player.DummyWalkToExact((int)player.X - 16, walkBackwards: false, 0.25f);
			player.DummyAutoAnimate = false;
			player.Sprite.Play("sitDown");
			yield return 1.5f;
		}

		private IEnumerator BadelineHugs()
		{
			yield return 1f;
			yield return badeline.FloatTo(badeline.Position + new Vector2(0f, 8f), null, faceDirection: true, fadeLight: false, quickEnd: true);
			badeline.Floatness = 0f;
			badeline.AutoAnimator.Enabled = false;
			badeline.Sprite.Play("idle");
			Audio.Play("event:/char/badeline/landing", badeline.Position);
			yield return 0.5f;
			yield return badeline.WalkTo(player.X - 9f, 40f);
			badeline.Sprite.Scale.X = 1f;
			yield return 0.2f;
			Audio.Play("event:/char/badeline/duck", badeline.Position);
			badeline.Depth = player.Depth + 5;
			badeline.Sprite.Play("hug");
			yield return 1f;
		}

		private IEnumerator StandUp()
		{
			Audio.Play("event:/char/badeline/stand", badeline.Position);
			yield return badeline.WalkTo(badeline.X - 8f);
			badeline.Sprite.Scale.X = 1f;
			yield return 0.2f;
			player.DummyAutoAnimate = true;
			Level.NextColorGrade("none", 0.25f);
			yield return 0.25f;
		}

		private IEnumerator ShiftCameraToBird()
		{
			Audio.ReleaseSnapshot(snapshot);
			snapshot = null;
			Audio.Play("event:/new_content/char/badeline/birdcrash_scene_float", badeline.Position);
			Add(new Coroutine(badeline.FloatTo(player.Position + new Vector2(-16f, -16f), 1)));
			Level level = base.Scene as Level;
			player.Facing = Facings.Right;
			yield return level.ZoomAcross(level.ZoomFocusPoint + new Vector2(70f, 0f), 1.5f, 1f);
			yield return 0.4;
		}

		public override void OnEnd(Level level)
		{
			Audio.ReleaseSnapshot(snapshot);
			snapshot = null;
			if (WasSkipped)
			{
				CutsceneNode playerSkipNode = CutsceneNode.Find("player_skip");
				if (playerSkipNode != null)
				{
					player.Sprite.Play("idle");
					player.Position = playerSkipNode.Position.Floor();
					level.Camera.Position = player.CameraTarget;
				}
				foreach (Lightning item in base.Scene.Entities.FindAll<Lightning>())
				{
					item.ToggleCheck();
				}
				base.Scene.Tracker.GetEntity<LightningRenderer>()?.ToggleEdges(immediate: true);
				level.Session.Audio.Music.Event = "event:/new_content/music/lvl10/reconciliation";
				level.Session.Audio.Apply();
			}
			player.Speed = Vector2.Zero;
			player.DummyGravity = true;
			player.DummyFriction = true;
			player.DummyAutoAnimate = true;
			player.ForceCameraUpdate = false;
			player.StateMachine.State = 0;
			BadelineBoost boost = base.Scene.Entities.FindFirst<BadelineBoost>();
			if (boost != null)
			{
				boost.Active = (boost.Visible = (boost.Collidable = true));
			}
			if (badeline != null)
			{
				badeline.RemoveSelf();
			}
			if (flingBird != null)
			{
				if (flingBird.CrashSfxEmitter != null)
				{
					flingBird.CrashSfxEmitter.RemoveSelf();
				}
				flingBird.RemoveSelf();
			}
			if (WasSkipped)
			{
				if (bird != null)
				{
					bird.RemoveSelf();
				}
				base.Scene.Add(bird = new BirdNPC(birdWaitPosition, BirdNPC.Modes.WaitForLightningOff));
				bird.Facing = Facings.Right;
				bird.FlyAwayUp = false;
				bird.WaitForLightningPostDelay = 1f;
				level.SnapColorGrade("none");
			}
			level.ResetZoom();
		}

		public override void Removed(Scene scene)
		{
			Audio.ReleaseSnapshot(snapshot);
			snapshot = null;
			base.Removed(scene);
		}

		public override void SceneEnd(Scene scene)
		{
			Audio.ReleaseSnapshot(snapshot);
			snapshot = null;
			base.SceneEnd(scene);
		}

		public static void HandlePostCutsceneSpawn(FlingBirdIntro flingBird, Level level)
		{
			BadelineBoost boost = level.Entities.FindFirst<BadelineBoost>();
			if (boost != null)
			{
				boost.Active = (boost.Visible = (boost.Collidable = true));
			}
			flingBird?.RemoveSelf();
			BirdNPC bird;
			level.Add(bird = new BirdNPC(flingBird.BirdEndPosition, BirdNPC.Modes.WaitForLightningOff));
			bird.Facing = Facings.Right;
			bird.FlyAwayUp = false;
		}
	}
}
