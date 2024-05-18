using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS06_StarJumpEnd : CutsceneEntity
	{
		public const string Flag = "plateau_2";

		private bool waiting = true;

		private bool shaking;

		private NPC theo;

		private Player player;

		private Bonfire bonfire;

		private BadelineDummy badeline;

		private Plateau plateau;

		private BreathingMinigame breathing;

		private List<ReflectionTentacles> tentacles = new List<ReflectionTentacles>();

		private Vector2 playerStart;

		private Vector2 cameraStart;

		private float anxietyFade;

		private SineWave anxietySine;

		private float anxietyJitter;

		private bool hidingNorthingLights;

		private bool charactersSpinning;

		private float maddySine;

		private float maddySineTarget;

		private float maddySineAnchorY;

		private SoundSource shakingLoopSfx;

		private bool baddyCircling;

		private BreathingRumbler rumbler;

		private int tentacleIndex;

		public CS06_StarJumpEnd(NPC theo, Player player, Vector2 playerStart, Vector2 cameraStart)
		{
			base.Depth = 10100;
			this.theo = theo;
			this.player = player;
			this.playerStart = playerStart;
			this.cameraStart = cameraStart;
			Add(anxietySine = new SineWave(0.3f));
		}

		public override void Added(Scene scene)
		{
			Level = scene as Level;
			bonfire = scene.Entities.FindFirst<Bonfire>();
			plateau = scene.Entities.FindFirst<Plateau>();
		}

		public override void Update()
		{
			base.Update();
			if (waiting && player.Y <= (float)(Level.Bounds.Top + 160))
			{
				waiting = false;
				Start();
			}
			if (shaking)
			{
				Level.Shake(0.2f);
			}
			if (Level != null && Level.OnInterval(0.1f))
			{
				anxietyJitter = Calc.Random.Range(-0.1f, 0.1f);
			}
			Distort.Anxiety = anxietyFade * Math.Max(0f, 0f + anxietyJitter + anxietySine.Value * 0.6f);
			maddySine = Calc.Approach(maddySine, maddySineTarget, 12f * Engine.DeltaTime);
			if (maddySine > 0f)
			{
				player.Y = maddySineAnchorY + (float)Math.Sin(Level.TimeActive * 2f) * 3f * maddySine;
			}
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			level.Entities.FindFirst<StarJumpController>()?.RemoveSelf();
			foreach (StarJumpBlock item in level.Entities.FindAll<StarJumpBlock>())
			{
				item.Collidable = false;
			}
			int center = level.Bounds.X + 160;
			Vector2 cutsceneCenter = new Vector2(center, level.Bounds.Top + 150);
			NorthernLights bg = level.Background.Get<NorthernLights>();
			level.CameraOffset.Y = -30f;
			Add(new Coroutine(CutsceneEntity.CameraTo(cutsceneCenter + new Vector2(-160f, -70f), 1.5f, Ease.CubeOut)));
			Add(new Coroutine(CutsceneEntity.CameraTo(cutsceneCenter + new Vector2(-160f, -120f), 2f, Ease.CubeInOut, 1.5f)));
			Tween.Set(this, Tween.TweenMode.Oneshot, 3f, Ease.CubeInOut, delegate(Tween t)
			{
				bg.OffsetY = t.Eased * 32f;
			});
			if (player.StateMachine.State == 19)
			{
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			}
			player.Dashes = 0;
			player.StateMachine.State = 11;
			player.DummyGravity = false;
			player.DummyAutoAnimate = false;
			player.Sprite.Play("fallSlow");
			player.Dashes = 1;
			player.Speed = new Vector2(0f, -80f);
			player.Facing = Facings.Right;
			player.ForceCameraUpdate = false;
			while (player.Speed.Length() > 0f || player.Position != cutsceneCenter)
			{
				player.Speed = Calc.Approach(player.Speed, Vector2.Zero, 200f * Engine.DeltaTime);
				player.Position = Calc.Approach(player.Position, cutsceneCenter, 64f * Engine.DeltaTime);
				yield return null;
			}
			player.Sprite.Play("spin");
			yield return 3.5f;
			player.Facing = Facings.Right;
			level.Add(badeline = new BadelineDummy(player.Position));
			level.Displacement.AddBurst(player.Position, 0.5f, 8f, 48f, 0.5f);
			Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
			player.CreateSplitParticles();
			Audio.Play("event:/char/badeline/maddy_split");
			badeline.Sprite.Scale.X = -1f;
			Vector2 start2 = player.Position;
			Vector2 target2 = cutsceneCenter + new Vector2(-30f, 0f);
			maddySineAnchorY = cutsceneCenter.Y;
			for (float p2 = 0f; p2 <= 1f; p2 += 2f * Engine.DeltaTime)
			{
				yield return null;
				if (p2 > 1f)
				{
					p2 = 1f;
				}
				player.Position = Vector2.Lerp(start2, target2, Ease.CubeOut(p2));
				badeline.Position = new Vector2((float)center + ((float)center - player.X), player.Y);
			}
			charactersSpinning = true;
			Add(new Coroutine(SpinCharacters()));
			SetMusicLayer(2);
			yield return 1f;
			yield return Textbox.Say("ch6_dreaming", TentaclesAppear, TentaclesGrab, FeatherMinigame, EndFeatherMinigame, StartCirclingPlayer);
			Audio.Play("event:/game/06_reflection/badeline_pull_whooshdown");
			Add(new Coroutine(BadelineFlyDown()));
			yield return 0.7f;
			foreach (FlyFeather item2 in level.Entities.FindAll<FlyFeather>())
			{
				item2.RemoveSelf();
			}
			foreach (StarJumpBlock item3 in level.Entities.FindAll<StarJumpBlock>())
			{
				item3.RemoveSelf();
			}
			foreach (JumpThru item4 in level.Entities.FindAll<JumpThru>())
			{
				item4.RemoveSelf();
			}
			level.Shake();
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
			level.CameraOffset.Y = 0f;
			player.Sprite.Play("tentacle_pull");
			player.Speed.Y = 160f;
			FallEffects.Show(visible: true);
			for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime / 3f)
			{
				player.Speed.Y += Engine.DeltaTime * 100f;
				if (player.X < (float)(level.Bounds.X + 32))
				{
					player.X = level.Bounds.X + 32;
				}
				if (player.X > (float)(level.Bounds.Right - 32))
				{
					player.X = level.Bounds.Right - 32;
				}
				if (p2 > 0.7f)
				{
					level.CameraOffset.Y -= 100f * Engine.DeltaTime;
				}
				foreach (ReflectionTentacles tentacle in tentacles)
				{
					tentacle.Nodes[0] = new Vector2(level.Bounds.Center.X, player.Y + 300f);
					tentacle.Nodes[1] = new Vector2(level.Bounds.Center.X, player.Y + 600f);
				}
				FallEffects.SpeedMultiplier += Engine.DeltaTime * 0.75f;
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
				yield return null;
			}
			Audio.Play("event:/game/06_reflection/badeline_pull_impact");
			FallEffects.Show(visible: false);
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			level.Flash(Color.White);
			level.Session.Dreaming = false;
			level.CameraOffset.Y = 0f;
			level.Camera.Position = cameraStart;
			SetBloom(0f);
			bonfire.SetMode(Bonfire.Mode.Smoking);
			plateau.Depth = player.Depth + 10;
			plateau.Remove(plateau.Occluder);
			player.Position = playerStart + new Vector2(0f, 8f);
			player.Speed = Vector2.Zero;
			player.Sprite.Play("tentacle_dangling");
			player.Facing = Facings.Left;
			theo.Position.X -= 24f;
			theo.Sprite.Play("alert");
			foreach (ReflectionTentacles tentacle2 in tentacles)
			{
				tentacle2.Index = 0;
				tentacle2.Nodes[0] = new Vector2(level.Bounds.Center.X, player.Y + 32f);
				tentacle2.Nodes[1] = new Vector2(level.Bounds.Center.X, player.Y + 400f);
				tentacle2.SnapTentacles();
			}
			shaking = true;
			Add(shakingLoopSfx = new SoundSource());
			shakingLoopSfx.Play("event:/game/06_reflection/badeline_pull_rumble_loop");
			yield return Textbox.Say("ch6_theo_watchout");
			Audio.Play("event:/game/06_reflection/badeline_pull_cliffbreak");
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Long);
			shakingLoopSfx.Stop();
			shaking = false;
			for (int x = 0; (float)x < plateau.Width; x += 8)
			{
				level.Add(Engine.Pooler.Create<Debris>().Init(plateau.Position + new Vector2((float)x + Calc.Random.NextFloat(8f), Calc.Random.NextFloat(8f)), '3').BlastFrom(plateau.Center + new Vector2(0f, 8f)));
				level.Add(Engine.Pooler.Create<Debris>().Init(plateau.Position + new Vector2((float)x + Calc.Random.NextFloat(8f), Calc.Random.NextFloat(8f)), '3').BlastFrom(plateau.Center + new Vector2(0f, 8f)));
			}
			plateau.RemoveSelf();
			bonfire.RemoveSelf();
			level.Shake();
			player.Speed.Y = 160f;
			player.Sprite.Play("tentacle_pull");
			player.ForceCameraUpdate = false;
			FadeWipe wipe = new FadeWipe(level, wipeIn: false, delegate
			{
				EndCutscene(level);
			})
			{
				Duration = 3f
			};
			target2 = level.Camera.Position;
			start2 = level.Camera.Position + new Vector2(0f, 400f);
			while (wipe.Percent < 1f)
			{
				level.Camera.Position = Vector2.Lerp(target2, start2, Ease.CubeIn(wipe.Percent));
				player.Speed.Y += 400f * Engine.DeltaTime;
				foreach (ReflectionTentacles tentacle3 in tentacles)
				{
					tentacle3.Nodes[0] = new Vector2(level.Bounds.Center.X, player.Y + 300f);
					tentacle3.Nodes[1] = new Vector2(level.Bounds.Center.X, player.Y + 600f);
				}
				yield return null;
			}
		}

		private void SetMusicLayer(int index)
		{
			for (int i = 1; i <= 3; i++)
			{
				Level.Session.Audio.Music.Layer(i, index == i);
			}
			Level.Session.Audio.Apply();
		}

		private IEnumerator TentaclesAppear()
		{
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			if (tentacleIndex == 0)
			{
				Audio.Play("event:/game/06_reflection/badeline_freakout_1");
			}
			else if (tentacleIndex == 1)
			{
				Audio.Play("event:/game/06_reflection/badeline_freakout_2");
			}
			else if (tentacleIndex == 2)
			{
				Audio.Play("event:/game/06_reflection/badeline_freakout_3");
			}
			else
			{
				Audio.Play("event:/game/06_reflection/badeline_freakout_4");
			}
			if (!hidingNorthingLights)
			{
				Add(new Coroutine(NothernLightsDown()));
				hidingNorthingLights = true;
			}
			Level.Shake();
			anxietyFade += 0.1f;
			if (tentacleIndex == 0)
			{
				SetMusicLayer(3);
			}
			int from = 400;
			int to = 140;
			List<Vector2> nodes = new List<Vector2>();
			nodes.Add(new Vector2(Level.Camera.X + 160f, Level.Camera.Y + (float)from));
			nodes.Add(new Vector2(Level.Camera.X + 160f, Level.Camera.Y + (float)from + 200f));
			ReflectionTentacles t = new ReflectionTentacles();
			t.Create(0f, 0, tentacles.Count, nodes);
			t.Nodes[0] = new Vector2(t.Nodes[0].X, Level.Camera.Y + (float)to);
			Level.Add(t);
			tentacles.Add(t);
			charactersSpinning = false;
			tentacleIndex++;
			badeline.Sprite.Play("angry");
			maddySineTarget = 1f;
			yield return null;
		}

		private IEnumerator TentaclesGrab()
		{
			maddySineTarget = 0f;
			Audio.Play("event:/game/06_reflection/badeline_freakout_5");
			player.Sprite.Play("tentacle_grab");
			yield return 0.1f;
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
			Level.Shake();
			rumbler = new BreathingRumbler();
			Level.Add(rumbler);
		}

		private IEnumerator StartCirclingPlayer()
		{
			Add(new Coroutine(BadelineCirclePlayer()));
			Vector2 from = player.Position;
			Vector2 to = new Vector2(Level.Bounds.Center.X, player.Y);
			Tween.Set(this, Tween.TweenMode.Oneshot, 0.5f, Ease.CubeOut, delegate(Tween t)
			{
				player.Position = Vector2.Lerp(from, to, t.Eased);
			});
			yield return null;
		}

		private IEnumerator EndCirclingPlayer()
		{
			baddyCircling = false;
			yield return null;
		}

		private IEnumerator BadelineCirclePlayer()
		{
			float offset = 0f;
			float dist = (badeline.Position - player.Position).Length();
			baddyCircling = true;
			while (baddyCircling)
			{
				offset -= Engine.DeltaTime * 4f;
				dist = Calc.Approach(dist, 24f, Engine.DeltaTime * 32f);
				badeline.Position = player.Position + Calc.AngleToVector(offset, dist);
				int sign = Math.Sign(player.X - badeline.X);
				if (sign != 0)
				{
					badeline.Sprite.Scale.X = sign;
				}
				if (Level.OnInterval(0.1f))
				{
					TrailManager.Add(badeline, Player.NormalHairColor);
				}
				yield return null;
			}
			badeline.Sprite.Scale.X = -1f;
			yield return badeline.FloatTo(player.Position + new Vector2(40f, -16f), -1, faceDirection: false);
		}

		private IEnumerator FeatherMinigame()
		{
			breathing = new BreathingMinigame(winnable: false, rumbler);
			Level.Add(breathing);
			while (!breathing.Pausing)
			{
				yield return null;
			}
		}

		private IEnumerator EndFeatherMinigame()
		{
			baddyCircling = false;
			breathing.Pausing = false;
			while (!breathing.Completed)
			{
				yield return null;
			}
			breathing = null;
		}

		private IEnumerator BadelineFlyDown()
		{
			badeline.Sprite.Play("fallFast");
			badeline.FloatSpeed = 600f;
			badeline.FloatAccel = 1200f;
			yield return badeline.FloatTo(new Vector2(badeline.X, Level.Camera.Y + 200f), null, faceDirection: true, fadeLight: true);
			badeline.RemoveSelf();
		}

		private IEnumerator NothernLightsDown()
		{
			NorthernLights bg = Level.Background.Get<NorthernLights>();
			if (bg != null)
			{
				while (bg.NorthernLightsAlpha > 0f)
				{
					bg.NorthernLightsAlpha -= Engine.DeltaTime * 0.5f;
					yield return null;
				}
			}
		}

		private IEnumerator SpinCharacters()
		{
			Vector2 maddyStart = player.Position;
			Vector2 baddyStart = badeline.Position;
			Vector2 center = (maddyStart + baddyStart) / 2f;
			float dist = Math.Abs(maddyStart.X - center.X);
			float timer = (float)Math.PI / 2f;
			player.Sprite.Play("spin");
			badeline.Sprite.Play("spin");
			badeline.Sprite.Scale.X = 1f;
			while (charactersSpinning)
			{
				int frame = (int)(timer / ((float)Math.PI * 2f) * 14f + 10f);
				player.Sprite.SetAnimationFrame(frame);
				badeline.Sprite.SetAnimationFrame(frame + 7);
				float sin = (float)Math.Sin(timer);
				float cos = (float)Math.Cos(timer);
				player.Position = center - new Vector2(sin * dist, cos * 8f);
				badeline.Position = center + new Vector2(sin * dist, cos * 8f);
				timer += Engine.DeltaTime * 2f;
				yield return null;
			}
			player.Facing = Facings.Right;
			player.Sprite.Play("fallSlow");
			badeline.Sprite.Scale.X = -1f;
			badeline.Sprite.Play("angry");
			badeline.AutoAnimator.Enabled = false;
			Vector2 maddyFrom = player.Position;
			Vector2 baddyFrom = badeline.Position;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime * 3f)
			{
				player.Position = Vector2.Lerp(maddyFrom, maddyStart, Ease.CubeOut(p));
				badeline.Position = Vector2.Lerp(baddyFrom, baddyStart, Ease.CubeOut(p));
				yield return null;
			}
		}

		public override void OnEnd(Level level)
		{
			if (rumbler != null)
			{
				rumbler.RemoveSelf();
				rumbler = null;
			}
			if (breathing != null)
			{
				breathing.RemoveSelf();
			}
			SetBloom(0f);
			level.Session.Audio.Music.Event = null;
			level.Session.Audio.Apply();
			level.Remove(player);
			level.UnloadLevel();
			level.EndCutscene();
			level.Session.SetFlag("plateau_2");
			level.SnapColorGrade(AreaData.Get(level).ColorGrade);
			level.Session.Dreaming = false;
			level.Session.FirstLevel = false;
			if (WasSkipped)
			{
				level.OnEndOfFrame += delegate
				{
					level.Session.Level = "00";
					level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Bottom));
					level.LoadLevel(Player.IntroTypes.None);
					FallEffects.Show(visible: false);
					level.Session.Audio.Music.Event = "event:/music/lvl6/main";
					level.Session.Audio.Apply();
				};
				return;
			}
			Engine.Scene = new OverworldReflectionsFall(level, delegate
			{
				Audio.SetAmbience(null);
				level.Session.Level = "04";
				level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Center.X, level.Bounds.Top));
				level.LoadLevel(Player.IntroTypes.Fall);
				level.Add(new BackgroundFadeIn(Color.Black, 2f, 30f));
				level.Entities.UpdateLists();
				foreach (CrystalStaticSpinner entity in level.Tracker.GetEntities<CrystalStaticSpinner>())
				{
					entity.ForceInstantiate();
				}
			});
		}

		private void SetBloom(float add)
		{
			Level.Session.BloomBaseAdd = add;
			Level.Bloom.Base = AreaData.Get(Level).BloomBase + add;
		}
	}
}
