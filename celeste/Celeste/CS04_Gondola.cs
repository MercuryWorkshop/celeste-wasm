using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS04_Gondola : CutsceneEntity
	{
		private enum GondolaStates
		{
			Stopped,
			MovingToCenter,
			InCenter,
			Shaking,
			MovingToEnd
		}

		private NPC theo;

		private Gondola gondola;

		private Player player;

		private BadelineDummy evil;

		private Parallax loopingCloud;

		private Parallax bottomCloud;

		private WindSnowFG windSnowFg;

		private float LoopCloudsAt;

		private List<ReflectionTentacles> tentacles = new List<ReflectionTentacles>();

		private SoundSource moveLoopSfx;

		private SoundSource haltLoopSfx;

		private float gondolaPercent;

		private bool AutoSnapCharacters;

		private float theoXOffset;

		private float playerXOffset;

		private float gondolaSpeed;

		private float shakeTimer;

		private const float gondolaMaxSpeed = 64f;

		private float anxiety;

		private float anxietyStutter;

		private float anxietyRumble;

		private BreathingRumbler rumbler;

		private GondolaStates gondolaState;

		public CS04_Gondola(NPC theo, Gondola gondola, Player player)
			: base(fadeInOnSkip: false, endingChapterAfter: true)
		{
			this.theo = theo;
			this.gondola = gondola;
			this.player = player;
		}

		public override void OnBegin(Level level)
		{
			level.RegisterAreaComplete();
			foreach (Backdrop bg in level.Foreground.Backdrops)
			{
				if (bg is WindSnowFG)
				{
					windSnowFg = bg as WindSnowFG;
				}
			}
			Add(moveLoopSfx = new SoundSource());
			Add(haltLoopSfx = new SoundSource());
			Add(new Coroutine(Cutscene()));
		}

		private IEnumerator Cutscene()
		{
			player.StateMachine.State = 11;
			yield return player.DummyWalkToExact((int)gondola.X + 16);
			while (!player.OnGround())
			{
				yield return null;
			}
			Audio.SetMusic("event:/music/lvl1/theo");
			yield return Textbox.Say("CH4_GONDOLA", EnterTheo, CheckOnTheo, GetUpTheo, LookAtLever, PullLever, WaitABit, WaitForCenter, SelfieThenStallsOut, MovePlayerLeft, SnapLeverOff, DarknessAppears, DarknessConsumes, CantBreath, StartBreathing, Ascend, WaitABit, TheoTakesOutPhone, FaceTheo);
			yield return ShowPhoto();
			EndCutscene(Level);
		}

		public override void OnEnd(Level level)
		{
			if (rumbler != null)
			{
				rumbler.RemoveSelf();
				rumbler = null;
			}
			level.CompleteArea();
			if (!WasSkipped)
			{
				SpotlightWipe.Modifier = 120f;
				SpotlightWipe.FocusPoint = new Vector2(320f, 180f) / 2f;
			}
		}

		private IEnumerator EnterTheo()
		{
			player.Facing = Facings.Left;
			yield return 0.2f;
			yield return PanCamera(new Vector2(Level.Bounds.Left, theo.Y - 90f), 1f);
			theo.Visible = true;
			float theoStartX = theo.X;
			yield return theo.MoveTo(new Vector2(theoStartX + 35f, theo.Y));
			yield return 0.6f;
			yield return theo.MoveTo(new Vector2(theoStartX + 60f, theo.Y));
			Audio.Play("event:/game/04_cliffside/gondola_theo_fall", theo.Position);
			theo.Sprite.Play("idleEdge");
			yield return 1f;
			theo.Sprite.Play("falling");
			theo.X += 4f;
			theo.Depth = -10010;
			float speed = 80f;
			while (theo.Y < player.Y)
			{
				theo.Y += speed * Engine.DeltaTime;
				speed += 120f * Engine.DeltaTime;
				yield return null;
			}
			Level.DirectionalShake(new Vector2(0f, 1f));
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			theo.Y = player.Y;
			theo.Sprite.Play("hitGround");
			theo.Sprite.Rate = 0f;
			theo.Depth = 1000;
			theo.Sprite.Scale = new Vector2(1.3f, 0.8f);
			yield return 0.5f;
			Vector2 start = theo.Sprite.Scale;
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, null, 2f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				theo.Sprite.Scale.X = MathHelper.Lerp(start.X, 1f, t.Eased);
				theo.Sprite.Scale.Y = MathHelper.Lerp(start.Y, 1f, t.Eased);
			};
			Add(tween);
			yield return PanCamera(new Vector2(Level.Bounds.Left, theo.Y - 120f), 1f);
			yield return 0.6f;
		}

		private IEnumerator CheckOnTheo()
		{
			yield return player.DummyWalkTo(gondola.X - 18f);
		}

		private IEnumerator GetUpTheo()
		{
			yield return 1.4f;
			Audio.Play("event:/game/04_cliffside/gondola_theo_recover", theo.Position);
			theo.Sprite.Rate = 1f;
			theo.Sprite.Play("recoverGround");
			yield return 1.6f;
			yield return theo.MoveTo(new Vector2(gondola.X - 50f, player.Y));
			yield return 0.2f;
		}

		private IEnumerator LookAtLever()
		{
			yield return theo.MoveTo(new Vector2(gondola.X + 7f, theo.Y));
			player.Facing = Facings.Right;
			theo.Sprite.Scale.X = -1f;
		}

		private IEnumerator PullLever()
		{
			Add(new Coroutine(player.DummyWalkToExact((int)gondola.X - 7)));
			theo.Sprite.Scale.X = -1f;
			yield return 0.2f;
			Audio.Play("event:/game/04_cliffside/gondola_theo_lever_start", theo.Position);
			theo.Sprite.Play("pullVent");
			yield return 1f;
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			gondola.Lever.Play("pulled");
			theo.Sprite.Play("fallVent");
			yield return 0.6f;
			Level.Shake();
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
			yield return 0.5f;
			yield return PanCamera(gondola.Position + new Vector2(-160f, -120f), 1f);
			yield return 0.5f;
			Level.Background.Backdrops.Add(loopingCloud = new Parallax(GFX.Game["bgs/04/bgCloudLoop"]));
			Level.Background.Backdrops.Add(bottomCloud = new Parallax(GFX.Game["bgs/04/bgCloud"]));
			loopingCloud.LoopX = (bottomCloud.LoopX = true);
			loopingCloud.LoopY = (bottomCloud.LoopY = false);
			loopingCloud.Position.Y = Level.Camera.Top - (float)loopingCloud.Texture.Height - (float)bottomCloud.Texture.Height;
			bottomCloud.Position.Y = Level.Camera.Top - (float)bottomCloud.Texture.Height;
			LoopCloudsAt = bottomCloud.Position.Y;
			AutoSnapCharacters = true;
			theoXOffset = theo.X - gondola.X;
			playerXOffset = player.X - gondola.X;
			player.StateMachine.State = 17;
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, null, 16f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				if (Audio.CurrentMusic == "event:/music/lvl1/theo")
				{
					Audio.SetMusicParam("fade", 1f - t.Eased);
				}
			};
			Add(tween);
			SoundSource sfx = new SoundSource();
			sfx.Position = gondola.LeftCliffside.Position;
			sfx.Play("event:/game/04_cliffside/gondola_cliffmechanism_start");
			Add(sfx);
			moveLoopSfx.Play("event:/game/04_cliffside/gondola_movement_loop");
			Level.Shake();
			Input.Rumble(RumbleStrength.Strong, RumbleLength.FullSecond);
			gondolaSpeed = 32f;
			gondola.RotationSpeed = 1f;
			gondolaState = GondolaStates.MovingToCenter;
			yield return 1f;
			yield return MoveTheoOnGondola(12f, changeFacing: false);
			yield return 0.2f;
			theo.Sprite.Scale.X = -1f;
		}

		private IEnumerator WaitABit()
		{
			yield return 1f;
		}

		private IEnumerator WaitForCenter()
		{
			while (gondolaState != GondolaStates.InCenter)
			{
				yield return null;
			}
			theo.Sprite.Scale.X = 1f;
			yield return 1f;
			yield return MovePlayerOnGondola(-20f);
			yield return 0.5f;
		}

		private IEnumerator SelfieThenStallsOut()
		{
			Audio.SetMusic("event:/music/lvl4/minigame");
			Add(new Coroutine(Level.ZoomTo(new Vector2(160f, 110f), 2f, 0.5f)));
			yield return 0.3f;
			theo.Sprite.Scale.X = 1f;
			yield return 0.2f;
			Add(new Coroutine(MovePlayerOnGondola(theoXOffset - 8f)));
			yield return 0.4f;
			Audio.Play("event:/game/04_cliffside/gondola_theoselfie_halt", theo.Position);
			theo.Sprite.Play("holdOutPhone");
			yield return 1.5f;
			theoXOffset += 4f;
			playerXOffset += 4f;
			gondola.RotationSpeed = -1f;
			gondolaState = GondolaStates.Stopped;
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
			theo.Sprite.Play("takeSelfieImmediate");
			Add(new Coroutine(PanCamera(gondola.Position + (gondola.Destination - gondola.Position).SafeNormalize() * 32f + new Vector2(-160f, -120f), 0.3f, Ease.CubeOut)));
			yield return 0.5f;
			Level.Flash(Color.White);
			Level.Add(evil = new BadelineDummy(Vector2.Zero));
			evil.Appear(Level);
			evil.Floatness = 0f;
			evil.Depth = -1000000;
			moveLoopSfx.Stop();
			haltLoopSfx.Play("event:/game/04_cliffside/gondola_halted_loop");
			gondolaState = GondolaStates.Shaking;
			yield return PanCamera(gondola.Position + new Vector2(-160f, -120f), 1f);
			yield return 1f;
		}

		private IEnumerator MovePlayerLeft()
		{
			yield return MovePlayerOnGondola(-20f);
			theo.Sprite.Scale.X = -1f;
			yield return 0.5f;
			yield return MovePlayerOnGondola(20f);
			yield return 0.5f;
			yield return MovePlayerOnGondola(-10f);
			yield return 0.5f;
			player.Facing = Facings.Right;
		}

		private IEnumerator SnapLeverOff()
		{
			yield return MoveTheoOnGondola(7f);
			Audio.Play("event:/game/04_cliffside/gondola_theo_lever_fail", theo.Position);
			theo.Sprite.Play("pullVent");
			yield return 1f;
			theo.Sprite.Play("fallVent");
			yield return 1f;
			gondola.BreakLever();
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			Level.Shake();
			yield return 2.5f;
		}

		private IEnumerator DarknessAppears()
		{
			Audio.SetMusicParam("calm", 0f);
			yield return 0.25f;
			player.Sprite.Play("tired");
			yield return 0.25f;
			evil.Vanish();
			evil = null;
			yield return 0.3f;
			Level.NextColorGrade("panicattack");
			Level.Shake();
			Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
			BurstTentacles(3, 90f);
			Audio.Play("event:/game/04_cliffside/gondola_scaryhair_01", gondola.Position);
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / 2f)
			{
				yield return null;
				Level.Background.Fade = p;
				anxiety = p;
				if (windSnowFg != null)
				{
					windSnowFg.Alpha = 1f - p;
				}
			}
			yield return 0.25f;
		}

		private IEnumerator DarknessConsumes()
		{
			Level.Shake();
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			Audio.Play("event:/game/04_cliffside/gondola_scaryhair_02", gondola.Position);
			BurstTentacles(2, 60f);
			yield return MoveTheoOnGondola(0f);
			theo.Sprite.Play("comfortStart");
		}

		private IEnumerator CantBreath()
		{
			Level.Shake();
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			Audio.Play("event:/game/04_cliffside/gondola_scaryhair_03", gondola.Position);
			BurstTentacles(1, 30f);
			BurstTentacles(0, 0f, 100f);
			rumbler = new BreathingRumbler();
			base.Scene.Add(rumbler);
			yield return null;
		}

		private IEnumerator StartBreathing()
		{
			BreathingMinigame breathing = new BreathingMinigame(winnable: true, rumbler);
			base.Scene.Add(breathing);
			while (!breathing.Completed)
			{
				yield return null;
			}
			foreach (ReflectionTentacles tentacle in tentacles)
			{
				tentacle.RemoveSelf();
			}
			anxiety = 0f;
			Level.Background.Fade = 0f;
			Level.SnapColorGrade(null);
			gondola.CancelPullSides();
			Level.ResetZoom();
			yield return 0.5f;
			Audio.Play("event:/game/04_cliffside/gondola_restart", gondola.Position);
			yield return 1f;
			moveLoopSfx.Play("event:/game/04_cliffside/gondola_movement_loop");
			haltLoopSfx.Stop();
			Level.Shake();
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
			gondolaState = GondolaStates.InCenter;
			gondola.RotationSpeed = 0.5f;
			yield return 1.2f;
		}

		private IEnumerator Ascend()
		{
			gondolaState = GondolaStates.MovingToEnd;
			while (gondolaState != 0)
			{
				yield return null;
			}
			Level.Shake();
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
			moveLoopSfx.Stop();
			Audio.Play("event:/game/04_cliffside/gondola_finish", gondola.Position);
			gondola.RotationSpeed = 0.5f;
			yield return 0.1f;
			while (gondola.Rotation > 0f)
			{
				yield return null;
			}
			gondola.Rotation = (gondola.RotationSpeed = 0f);
			Level.Shake();
			AutoSnapCharacters = false;
			player.StateMachine.State = 11;
			player.Position = player.Position.Floor();
			while (player.CollideCheck<Solid>())
			{
				player.Y--;
			}
			theo.Position.Y = player.Position.Y;
			theo.Sprite.Play("comfortRecover");
			theo.Sprite.Scale.X = 1f;
			yield return player.DummyWalkTo(gondola.X + 80f);
			player.DummyAutoAnimate = false;
			player.Sprite.Play("tired");
			yield return theo.MoveTo(new Vector2(gondola.X + 64f, theo.Y));
			yield return 0.5f;
		}

		private IEnumerator TheoTakesOutPhone()
		{
			player.Facing = Facings.Right;
			yield return 0.25f;
			theo.Sprite.Play("usePhone");
			yield return 2f;
		}

		private IEnumerator FaceTheo()
		{
			player.DummyAutoAnimate = true;
			yield return 0.2f;
			player.Facing = Facings.Left;
			yield return 0.2f;
		}

		private IEnumerator ShowPhoto()
		{
			theo.Sprite.Scale.X = -1f;
			yield return 0.25f;
			yield return player.DummyWalkTo(theo.X + 5f);
			yield return 1f;
			Selfie selfie = new Selfie(SceneAs<Level>());
			base.Scene.Add(selfie);
			yield return selfie.OpenRoutine("selfieGondola");
			yield return selfie.WaitForInput();
		}

		public override void Update()
		{
			base.Update();
			if (anxietyRumble > 0f)
			{
				Input.RumbleSpecific(anxietyRumble, 0.1f);
			}
			if (base.Scene.OnInterval(0.05f))
			{
				anxietyStutter = Calc.Random.NextFloat(0.1f);
			}
			Distort.AnxietyOrigin = new Vector2(0.5f, 0.5f);
			Distort.Anxiety = anxiety * 0.2f + anxietyStutter * anxiety;
			if (moveLoopSfx != null && gondola != null)
			{
				moveLoopSfx.Position = gondola.Position;
			}
			if (haltLoopSfx != null && gondola != null)
			{
				haltLoopSfx.Position = gondola.Position;
			}
			if (gondolaState == GondolaStates.MovingToCenter)
			{
				MoveGondolaTowards(0.5f);
				if (gondolaPercent >= 0.5f)
				{
					gondolaState = GondolaStates.InCenter;
				}
			}
			else if (gondolaState == GondolaStates.InCenter)
			{
				Vector2 spd = (gondola.Destination - gondola.Position).SafeNormalize() * gondolaSpeed;
				loopingCloud.CameraOffset.X += spd.X * Engine.DeltaTime;
				loopingCloud.CameraOffset.Y += spd.Y * Engine.DeltaTime;
				windSnowFg.CameraOffset = loopingCloud.CameraOffset;
				loopingCloud.LoopY = true;
			}
			else if (gondolaState != 0)
			{
				if (gondolaState == GondolaStates.Shaking)
				{
					Level.Wind.X = -400f;
					if (shakeTimer <= 0f && (gondola.Rotation == 0f || gondola.Rotation < -0.25f))
					{
						shakeTimer = 1f;
						gondola.RotationSpeed = 0.5f;
					}
					shakeTimer -= Engine.DeltaTime;
				}
				else if (gondolaState == GondolaStates.MovingToEnd)
				{
					MoveGondolaTowards(1f);
					if (gondolaPercent >= 1f)
					{
						gondolaState = GondolaStates.Stopped;
					}
				}
			}
			if (loopingCloud != null && !loopingCloud.LoopY && Level.Camera.Bottom < LoopCloudsAt)
			{
				loopingCloud.LoopY = true;
			}
			if (AutoSnapCharacters)
			{
				theo.Position = gondola.GetRotatedFloorPositionAt(theoXOffset);
				player.Position = gondola.GetRotatedFloorPositionAt(playerXOffset);
				if (evil != null)
				{
					evil.Position = gondola.GetRotatedFloorPositionAt(-24f, 20f);
				}
			}
		}

		private void MoveGondolaTowards(float percent)
		{
			float dist = (gondola.Start - gondola.Destination).Length();
			gondolaSpeed = Calc.Approach(gondolaSpeed, 64f, 120f * Engine.DeltaTime);
			gondolaPercent = Calc.Approach(gondolaPercent, percent, gondolaSpeed / dist * Engine.DeltaTime);
			gondola.Position = (gondola.Start + (gondola.Destination - gondola.Start) * gondolaPercent).Floor();
			Level.Camera.Position = gondola.Position + new Vector2(-160f, -120f);
		}

		private IEnumerator PanCamera(Vector2 to, float duration, Ease.Easer ease = null)
		{
			if (ease == null)
			{
				ease = Ease.CubeInOut;
			}
			Vector2 from = Level.Camera.Position;
			for (float t = 0f; t < 1f; t += Engine.DeltaTime / duration)
			{
				yield return null;
				Level.Camera.Position = from + (to - from) * ease(Math.Min(t, 1f));
			}
		}

		private IEnumerator MovePlayerOnGondola(float x)
		{
			player.Sprite.Play("walk");
			player.Facing = (Facings)Math.Sign(x - playerXOffset);
			while (playerXOffset != x)
			{
				playerXOffset = Calc.Approach(playerXOffset, x, 48f * Engine.DeltaTime);
				yield return null;
			}
			player.Sprite.Play("idle");
		}

		private IEnumerator MoveTheoOnGondola(float x, bool changeFacing = true)
		{
			theo.Sprite.Play("walk");
			if (changeFacing)
			{
				theo.Sprite.Scale.X = Math.Sign(x - theoXOffset);
			}
			while (theoXOffset != x)
			{
				theoXOffset = Calc.Approach(theoXOffset, x, 48f * Engine.DeltaTime);
				yield return null;
			}
			theo.Sprite.Play("idle");
		}

		private void BurstTentacles(int layer, float dist, float from = 200f)
		{
			Vector2 center = Level.Camera.Position + new Vector2(160f, 90f);
			ReflectionTentacles left = new ReflectionTentacles();
			left.Create(0f, 0, layer, new List<Vector2>
			{
				center + new Vector2(0f - from, 0f),
				center + new Vector2(-800f, 0f)
			});
			left.SnapTentacles();
			left.Nodes[0] = center + new Vector2(0f - dist, 0f);
			ReflectionTentacles right = new ReflectionTentacles();
			right.Create(0f, 0, layer, new List<Vector2>
			{
				center + new Vector2(from, 0f),
				center + new Vector2(800f, 0f)
			});
			right.SnapTentacles();
			right.Nodes[0] = center + new Vector2(dist, 0f);
			tentacles.Add(left);
			tentacles.Add(right);
			Level.Add(left);
			Level.Add(right);
		}
	}
}
