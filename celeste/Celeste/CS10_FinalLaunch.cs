using System;
using System.Collections;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS10_FinalLaunch : CutsceneEntity
	{
		private Player player;

		private BadelineBoost boost;

		private BirdNPC bird;

		private float fadeToWhite;

		private Vector2 birdScreenPosition;

		private AscendManager.Streaks streaks;

		private Vector2 cameraWaveOffset;

		private Vector2 cameraOffset;

		private float timer;

		private Coroutine wave;

		private bool hasGolden;

		private bool sayDialog;

		public CS10_FinalLaunch(Player player, BadelineBoost boost, bool sayDialog = true)
			: base(fadeInOnSkip: false)
		{
			this.player = player;
			this.boost = boost;
			this.sayDialog = sayDialog;
			base.Depth = 10010;
		}

		public override void OnBegin(Level level)
		{
			Audio.SetMusic(null);
			ScreenWipe.WipeColor = Color.White;
			foreach (Follower follower in player.Leader.Followers)
			{
				if (follower.Entity is Strawberry strawb && strawb.Golden)
				{
					hasGolden = true;
					break;
				}
			}
			Add(new Coroutine(Cutscene()));
		}

		private IEnumerator Cutscene()
		{
			Engine.TimeRate = 1f;
			boost.Active = false;
			yield return null;
			if (sayDialog)
			{
				yield return Textbox.Say("CH9_LAST_BOOST");
			}
			else
			{
				yield return 0.152f;
			}
			cameraOffset = new Vector2(0f, -20f);
			boost.Active = true;
			player.EnforceLevelBounds = false;
			yield return null;
			BlackholeBG blackholeBG = Level.Background.Get<BlackholeBG>();
			blackholeBG.Direction = -2.5f;
			blackholeBG.SnapStrength(Level, BlackholeBG.Strengths.High);
			blackholeBG.CenterOffset.Y = 100f;
			blackholeBG.OffsetOffset.Y = -50f;
			Add(wave = new Coroutine(WaveCamera()));
			Add(new Coroutine(BirdRoutine(0.8f)));
			Level.Add(streaks = new AscendManager.Streaks(null));
			float p2;
			for (p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime / 12f)
			{
				fadeToWhite = p2;
				streaks.Alpha = p2;
				foreach (Parallax item in Level.Foreground.GetEach<Parallax>("blackhole"))
				{
					item.FadeAlphaMultiplier = 1f - p2;
				}
				yield return null;
			}
			while (bird != null)
			{
				yield return null;
			}
			FadeWipe wipe = new FadeWipe(Level, wipeIn: false)
			{
				Duration = 4f
			};
			ScreenWipe.WipeColor = Color.White;
			if (!hasGolden)
			{
				Audio.SetMusic("event:/new_content/music/lvl10/granny_farewell");
			}
			p2 = cameraOffset.Y;
			int to = 180;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / 2f)
			{
				cameraOffset.Y = p2 + ((float)to - p2) * Ease.BigBackIn(p);
				yield return null;
			}
			while (wipe.Percent < 1f)
			{
				yield return null;
			}
			EndCutscene(Level);
		}

		public override void OnEnd(Level level)
		{
			if (WasSkipped && boost != null && boost.Ch9FinalBoostSfx != null)
			{
				boost.Ch9FinalBoostSfx.stop(STOP_MODE.ALLOWFADEOUT);
				boost.Ch9FinalBoostSfx.release();
			}
			string nextLevelName = "end-granny";
			Player.IntroTypes nextLevelIntro = Player.IntroTypes.Transition;
			if (hasGolden)
			{
				nextLevelName = "end-golden";
				nextLevelIntro = Player.IntroTypes.Jump;
			}
			player.Active = true;
			player.Speed = Vector2.Zero;
			player.EnforceLevelBounds = true;
			player.StateMachine.State = 0;
			player.DummyFriction = true;
			player.DummyGravity = true;
			player.DummyAutoAnimate = true;
			player.ForceCameraUpdate = false;
			Engine.TimeRate = 1f;
			Level.OnEndOfFrame += delegate
			{
				Level.TeleportTo(player, nextLevelName, nextLevelIntro);
				if (hasGolden)
				{
					if (Level.Wipe != null)
					{
						Level.Wipe.Cancel();
					}
					Level.SnapColorGrade("golden");
					new FadeWipe(level, wipeIn: true).Duration = 2f;
					ScreenWipe.WipeColor = Color.White;
				}
			};
		}

		private IEnumerator WaveCamera()
		{
			float timer = 0f;
			while (true)
			{
				cameraWaveOffset.X = (float)Math.Sin(timer) * 16f;
				cameraWaveOffset.Y = (float)Math.Sin(timer * 0.5f) * 16f + (float)Math.Sin(timer * 0.25f) * 8f;
				timer += Engine.DeltaTime * 2f;
				yield return null;
			}
		}

		private IEnumerator BirdRoutine(float delay)
		{
			yield return delay;
			Level.Add(bird = new BirdNPC(Vector2.Zero, BirdNPC.Modes.None));
			bird.Sprite.Play("flyupIdle");
			Vector2 center = new Vector2(320f, 180f) / 2f;
			Vector2 topCenter = new Vector2(center.X, 0f);
			Vector2 botCenter = new Vector2(center.X, 180f);
			Vector2 from2 = botCenter + new Vector2(40f, 40f);
			Vector2 to2 = center + new Vector2(-32f, -24f);
			for (float t3 = 0f; t3 < 1f; t3 += Engine.DeltaTime / 4f)
			{
				birdScreenPosition = from2 + (to2 - from2) * Ease.BackOut(t3);
				yield return null;
			}
			bird.Sprite.Play("flyupRoll");
			for (float t3 = 0f; t3 < 1f; t3 += Engine.DeltaTime / 2f)
			{
				birdScreenPosition = to2 + new Vector2(64f, 0f) * Ease.CubeInOut(t3);
				yield return null;
			}
			to2 = birdScreenPosition;
			from2 = topCenter + new Vector2(-40f, -100f);
			bool playedAnim = false;
			for (float t3 = 0f; t3 < 1f; t3 += Engine.DeltaTime / 4f)
			{
				if (t3 >= 0.35f && !playedAnim)
				{
					bird.Sprite.Play("flyupRoll");
					playedAnim = true;
				}
				birdScreenPosition = to2 + (from2 - to2) * Ease.BigBackIn(t3);
				birdScreenPosition.X += t3 * 32f;
				yield return null;
			}
			bird.RemoveSelf();
			bird = null;
		}

		public override void Update()
		{
			base.Update();
			timer += Engine.DeltaTime;
			if (bird != null)
			{
				bird.Position = Level.Camera.Position + birdScreenPosition;
				bird.Position.X += (float)Math.Sin(timer) * 4f;
				bird.Position.Y += (float)Math.Sin(timer * 0.1f) * 4f + (float)Math.Sin(timer * 0.25f) * 4f;
			}
			Level.CameraOffset = cameraOffset + cameraWaveOffset;
		}

		public override void Render()
		{
			Camera cam = Level.Camera;
			Draw.Rect(cam.X - 1f, cam.Y - 1f, 322f, 322f, Color.White * fadeToWhite);
		}
	}
}
