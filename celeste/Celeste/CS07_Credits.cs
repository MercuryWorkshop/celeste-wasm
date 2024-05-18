using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS07_Credits : CutsceneEntity
	{
		private class Fill : Backdrop
		{
			public override void Render(Scene scene)
			{
				Draw.Rect(-10f, -10f, 340f, 200f, Color);
			}
		}

		public const float CameraXOffset = 70f;

		public const float CameraYOffset = -24f;

		public static CS07_Credits Instance;

		public string Event;

		private MTexture gradient = GFX.Gui["creditsgradient"].GetSubtexture(0, 1, 1920, 1);

		private Credits credits;

		private Player player;

		private bool autoWalk = true;

		private bool autoUpdateCamera = true;

		private BadelineDummy badeline;

		private bool badelineAutoFloat = true;

		private bool badelineAutoWalk;

		private float badelineWalkApproach;

		private Vector2 badelineWalkApproachFrom;

		private float walkOffset;

		private bool wasDashAssistOn;

		private Fill fillbg;

		private float fade = 1f;

		private HiresSnow snow;

		private bool gotoEpilogue;

		public CS07_Credits()
		{
			MInput.Disabled = true;
			Instance = this;
			base.Tag = (int)Tags.Global | (int)Tags.HUD;
			wasDashAssistOn = SaveData.Instance.Assists.DashAssist;
			SaveData.Instance.Assists.DashAssist = false;
		}

		public override void OnBegin(Level level)
		{
			Audio.BusMuted("bus:/gameplay_sfx", true);
			gotoEpilogue = level.Session.OldStats.Modes[0].Completed;
			gotoEpilogue = true;
			Add(new Coroutine(Routine()));
			Add(new PostUpdateHook(PostUpdate));
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			(base.Scene as Level).InCredits = true;
		}

		private IEnumerator Routine()
		{
			Level.Background.Backdrops.Add(fillbg = new Fill());
			Level.Completed = true;
			Level.Entities.FindFirst<SpeedrunTimerDisplay>()?.RemoveSelf();
			Level.Entities.FindFirst<TotalStrawberriesDisplay>()?.RemoveSelf();
			Level.Entities.FindFirst<GameplayStats>()?.RemoveSelf();
			yield return null;
			Level.Wipe.Cancel();
			yield return 0.5f;
			float alignment = 1f;
			if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
			{
				alignment = 0f;
			}
			credits = new Credits(alignment, 0.6f, haveTitle: false, havePolaroids: true);
			credits.AllowInput = false;
			yield return 3f;
			SetBgFade(0f);
			Add(new Coroutine(FadeTo(0f)));
			yield return SetupLevel();
			yield return WaitForPlayer();
			yield return FadeTo(1f);
			yield return 1f;
			SetBgFade(0.1f);
			yield return NextLevel("credits-dashes");
			yield return SetupLevel();
			Add(new Coroutine(FadeTo(0f)));
			yield return WaitForPlayer();
			yield return FadeTo(1f);
			yield return 1f;
			SetBgFade(0.2f);
			yield return NextLevel("credits-walking");
			yield return SetupLevel();
			Add(new Coroutine(FadeTo(0f)));
			yield return 5.8f;
			badelineAutoFloat = false;
			yield return 0.5f;
			badeline.Sprite.Scale.X = 1f;
			yield return 0.5f;
			autoWalk = false;
			player.Speed = Vector2.Zero;
			player.Facing = Facings.Right;
			yield return 1.5f;
			badeline.Sprite.Scale.X = -1f;
			yield return 1f;
			badeline.Sprite.Scale.X = -1f;
			badelineAutoWalk = true;
			badelineWalkApproachFrom = badeline.Position;
			Add(new Coroutine(BadelineApproachWalking()));
			yield return 0.7f;
			autoWalk = true;
			player.Facing = Facings.Left;
			yield return WaitForPlayer();
			yield return FadeTo(1f);
			yield return 1f;
			SetBgFade(0.3f);
			yield return NextLevel("credits-tree");
			yield return SetupLevel();
			Petals petals = new Petals();
			Level.Foreground.Backdrops.Add(petals);
			autoUpdateCamera = false;
			Vector2 cameraTo2 = Level.Camera.Position + new Vector2(-220f, 32f);
			Level.Camera.Position += new Vector2(-100f, 0f);
			badelineWalkApproach = 1f;
			badelineAutoFloat = false;
			badelineAutoWalk = true;
			badeline.Floatness = 0f;
			Add(new Coroutine(FadeTo(0f)));
			Add(new Coroutine(CutsceneEntity.CameraTo(cameraTo2, 12f, Ease.Linear)));
			yield return 3.5f;
			badeline.Sprite.Play("idle");
			badelineAutoWalk = false;
			yield return 0.25f;
			autoWalk = false;
			player.Sprite.Play("idle");
			player.Speed = Vector2.Zero;
			player.DummyAutoAnimate = false;
			player.Facing = Facings.Right;
			yield return 0.5f;
			player.Sprite.Play("sitDown");
			yield return 4f;
			badeline.Sprite.Play("laugh");
			yield return 1.75f;
			yield return FadeTo(1f);
			Level.Foreground.Backdrops.Remove(petals);
			yield return 1f;
			SetBgFade(0.4f);
			yield return NextLevel("credits-clouds");
			yield return SetupLevel();
			autoWalk = false;
			player.Speed = Vector2.Zero;
			autoUpdateCamera = false;
			player.ForceCameraUpdate = false;
			badeline.Visible = false;
			Player other = null;
			foreach (CreditsTrigger trigger2 in base.Scene.Tracker.GetEntities<CreditsTrigger>())
			{
				if (trigger2.Event == "BadelineOffset")
				{
					other = new Player(trigger2.Position, PlayerSpriteMode.Badeline)
					{
						OverrideHairColor = BadelineOldsite.HairColor
					};
					yield return null;
					other.StateMachine.State = 11;
					other.Facing = Facings.Left;
					base.Scene.Add(other);
				}
			}
			Add(new Coroutine(FadeTo(0f)));
			Level.Camera.Position += new Vector2(0f, -100f);
			Vector2 cameraTo3 = Level.Camera.Position + new Vector2(0f, 160f);
			Add(new Coroutine(CutsceneEntity.CameraTo(cameraTo3, 12f, Ease.Linear)));
			float playerHighJump = 0f;
			float baddyHighJump2 = 0f;
			for (float p = 0f; p < 10f; p += Engine.DeltaTime)
			{
				if (((p > 3f && p < 6f) || p > 9f) && player.Speed.Y < 0f && player.OnGround(4))
				{
					playerHighJump = 0.25f;
				}
				if (p > 5f && p < 8f && other.Speed.Y < 0f && other.OnGround(4))
				{
					baddyHighJump2 = 0.25f;
				}
				if (playerHighJump > 0f)
				{
					playerHighJump -= Engine.DeltaTime;
					player.Speed.Y = -200f;
				}
				if (baddyHighJump2 > 0f)
				{
					baddyHighJump2 -= Engine.DeltaTime;
					other.Speed.Y = -200f;
				}
				yield return null;
			}
			yield return FadeTo(1f);
			yield return 1f;
			SetBgFade(0.5f);
			yield return NextLevel("credits-resort");
			yield return SetupLevel();
			Add(new Coroutine(FadeTo(0f)));
			badelineWalkApproach = 1f;
			badelineAutoFloat = false;
			badelineAutoWalk = true;
			badeline.Floatness = 0f;
			Vector2 point = Vector2.Zero;
			foreach (CreditsTrigger credit in base.Scene.Entities.FindAll<CreditsTrigger>())
			{
				if (credit.Event == "Oshiro")
				{
					point = credit.Position;
				}
			}
			NPC oshiro = new NPC(point + new Vector2(0f, 4f));
			oshiro.Add(oshiro.Sprite = new OshiroSprite(1));
			oshiro.MoveAnim = "sweeping";
			oshiro.IdleAnim = "sweeping";
			oshiro.Sprite.Play("sweeping");
			oshiro.Maxspeed = 10f;
			oshiro.Depth = -60;
			base.Scene.Add(oshiro);
			Add(new Coroutine(DustyRoutine(oshiro)));
			yield return 4.8f;
			Vector2 oshiroTarget2 = oshiro.Position + new Vector2(116f, 0f);
			Coroutine oshiroRoutine = new Coroutine(oshiro.MoveTo(oshiroTarget2));
			Add(oshiroRoutine);
			yield return 2f;
			autoUpdateCamera = false;
			yield return CutsceneEntity.CameraTo(new Vector2(Level.Bounds.Left + 64, Level.Bounds.Top), 2f);
			yield return 5f;
			BirdNPC bird2 = new BirdNPC(oshiro.Position + new Vector2(280f, -160f), BirdNPC.Modes.None)
			{
				Depth = 10010,
				Light = 
				{
					Visible = false
				}
			};
			base.Scene.Add(bird2);
			bird2.Facing = Facings.Left;
			bird2.Sprite.Play("fall");
			Vector2 from = bird2.Position;
			Vector2 to2 = oshiroTarget2 + new Vector2(50f, -12f);
			baddyHighJump2 = 0f;
			while (baddyHighJump2 < 1f)
			{
				bird2.Position = from + (to2 - from) * Ease.QuadOut(baddyHighJump2);
				if (baddyHighJump2 > 0.5f)
				{
					bird2.Sprite.Play("fly");
					bird2.Depth = -1000000;
					bird2.Light.Visible = true;
				}
				baddyHighJump2 += Engine.DeltaTime * 0.5f;
				yield return null;
			}
			bird2.Position = to2;
			oshiroRoutine.RemoveSelf();
			oshiro.Sprite.Play("putBroomAway");
			oshiro.Sprite.OnFrameChange = delegate
			{
				if (oshiro.Sprite.CurrentAnimationFrame == 10)
				{
					Entity entity = new Entity(oshiro.Position)
					{
						Depth = oshiro.Depth + 1
					};
					base.Scene.Add(entity);
					entity.Add(new Image(GFX.Game["characters/oshiro/broom"])
					{
						Origin = oshiro.Sprite.Origin
					});
					oshiro.Sprite.OnFrameChange = null;
				}
			};
			bird2.Sprite.Play("idle");
			yield return 0.5f;
			bird2.Sprite.Play("croak");
			yield return 0.6f;
			oshiro.Maxspeed = 40f;
			oshiro.MoveAnim = "move";
			oshiro.IdleAnim = "idle";
			yield return oshiro.MoveTo(oshiroTarget2 + new Vector2(14f, 0f));
			yield return 2f;
			Add(new Coroutine(bird2.StartleAndFlyAway()));
			yield return 0.75f;
			bird2.Light.Visible = false;
			bird2.Depth = 10010;
			oshiro.Sprite.Scale.X = -1f;
			yield return FadeTo(1f);
			yield return 1f;
			SetBgFade(0.6f);
			yield return NextLevel("credits-wallslide");
			yield return SetupLevel();
			badelineAutoFloat = false;
			badeline.Floatness = 0f;
			badeline.Sprite.Play("idle");
			badeline.Sprite.Scale.X = 1f;
			foreach (CreditsTrigger trigger in base.Scene.Tracker.GetEntities<CreditsTrigger>())
			{
				if (trigger.Event == "BadelineOffset")
				{
					badeline.Position = trigger.Position + new Vector2(8f, 16f);
				}
			}
			Add(new Coroutine(FadeTo(0f)));
			Add(new Coroutine(WaitForPlayer()));
			while (player.X > badeline.X - 16f)
			{
				yield return null;
			}
			badeline.Sprite.Scale.X = -1f;
			yield return 0.1f;
			badelineAutoWalk = true;
			badelineWalkApproachFrom = badeline.Position;
			badelineWalkApproach = 0f;
			badeline.Sprite.Play("walk");
			while (badelineWalkApproach != 1f)
			{
				badelineWalkApproach = Calc.Approach(badelineWalkApproach, 1f, Engine.DeltaTime * 4f);
				yield return null;
			}
			while (player.X > (float)(Level.Bounds.X + 160))
			{
				yield return null;
			}
			yield return FadeTo(1f);
			yield return 1f;
			SetBgFade(0.7f);
			yield return NextLevel("credits-payphone");
			yield return SetupLevel();
			player.Speed = Vector2.Zero;
			player.Facing = Facings.Left;
			autoWalk = false;
			badeline.Sprite.Play("idle");
			badeline.Floatness = 0f;
			badeline.Y = player.Y;
			badeline.Sprite.Scale.X = 1f;
			badelineAutoFloat = false;
			autoUpdateCamera = false;
			Level.Camera.X += 100f;
			Vector2 cameraTo = Level.Camera.Position + new Vector2(-200f, 0f);
			Add(new Coroutine(CutsceneEntity.CameraTo(cameraTo, 14f, Ease.Linear)));
			Add(new Coroutine(FadeTo(0f)));
			yield return 1.5f;
			badeline.Sprite.Scale.X = -1f;
			yield return 0.5f;
			Add(new Coroutine(badeline.FloatTo(badeline.Position + new Vector2(16f, -12f), -1, faceDirection: false)));
			yield return 0.5f;
			player.Facing = Facings.Right;
			yield return 1.5f;
			oshiroTarget2 = badeline.Position;
			to2 = player.Center;
			Add(new Coroutine(BadelineAround(oshiroTarget2, to2, badeline)));
			yield return 0.5f;
			Add(new Coroutine(BadelineAround(oshiroTarget2, to2)));
			yield return 0.5f;
			Add(new Coroutine(BadelineAround(oshiroTarget2, to2)));
			yield return 3f;
			badeline.Sprite.Play("laugh");
			yield return 0.5f;
			player.Facing = Facings.Left;
			yield return 0.5f;
			player.DummyAutoAnimate = false;
			player.Sprite.Play("sitDown");
			yield return 3f;
			yield return FadeTo(1f);
			yield return 1f;
			SetBgFade(0.8f);
			yield return NextLevel("credits-city");
			yield return SetupLevel();
			BirdNPC bird = base.Scene.Entities.FindFirst<BirdNPC>();
			if (bird != null)
			{
				bird.Facing = Facings.Right;
			}
			badelineWalkApproach = 1f;
			badelineAutoFloat = false;
			badelineAutoWalk = true;
			badeline.Floatness = 0f;
			Add(new Coroutine(FadeTo(0f)));
			yield return WaitForPlayer();
			yield return FadeTo(1f);
			yield return 1f;
			SetBgFade(0f);
			yield return NextLevel("credits-prologue");
			yield return SetupLevel();
			badelineWalkApproach = 1f;
			badelineAutoFloat = false;
			badelineAutoWalk = true;
			badeline.Floatness = 0f;
			Add(new Coroutine(FadeTo(0f)));
			yield return WaitForPlayer();
			yield return FadeTo(1f);
			while (credits.BottomTimer < 2f)
			{
				yield return null;
			}
			if (!gotoEpilogue)
			{
				snow = new HiresSnow();
				snow.Alpha = 0f;
				snow.AttachAlphaTo = new FadeWipe(Level, wipeIn: false, delegate
				{
					EndCutscene(Level);
				});
				Level.Add(Level.HiresSnow = snow);
			}
			else
			{
				new FadeWipe(Level, wipeIn: false, delegate
				{
					EndCutscene(Level);
				});
			}
		}

		private IEnumerator SetupLevel()
		{
			Level.SnapColorGrade("credits");
			player = null;
			while ((player = base.Scene.Tracker.GetEntity<Player>()) == null)
			{
				yield return null;
			}
			Level.Add(badeline = new BadelineDummy(player.Position + new Vector2(16f, -16f)));
			badeline.Floatness = 4f;
			badelineAutoFloat = true;
			badelineAutoWalk = false;
			badelineWalkApproach = 0f;
			Level.Session.Inventory.Dashes = 1;
			player.Dashes = 1;
			player.StateMachine.State = 11;
			player.DummyFriction = false;
			player.DummyMaxspeed = false;
			player.Facing = Facings.Left;
			autoWalk = true;
			autoUpdateCamera = true;
			Level.CameraOffset.X = 70f;
			Level.CameraOffset.Y = -24f;
			Level.Camera.Position = player.CameraTarget;
		}

		private IEnumerator WaitForPlayer()
		{
			while (player.X > (float)(Level.Bounds.X + 160))
			{
				if (Event != null)
				{
					yield return DoEvent(Event);
				}
				Event = null;
				yield return null;
			}
		}

		private IEnumerator NextLevel(string name)
		{
			if (player != null)
			{
				player.RemoveSelf();
			}
			player = null;
			Level.OnEndOfFrame += delegate
			{
				Level.UnloadLevel();
				Level.Session.Level = name;
				Level.Session.RespawnPoint = Level.GetSpawnPoint(new Vector2(Level.Bounds.Left, Level.Bounds.Top));
				Level.LoadLevel(Player.IntroTypes.None);
				Level.Wipe.Cancel();
			};
			yield return null;
			yield return null;
		}

		private IEnumerator FadeTo(float value)
		{
			while ((fade = Calc.Approach(fade, value, Engine.DeltaTime * 0.5f)) != value)
			{
				yield return null;
			}
			fade = value;
		}

		private IEnumerator BadelineApproachWalking()
		{
			while (badelineWalkApproach < 1f)
			{
				badeline.Floatness = Calc.Approach(badeline.Floatness, 0f, Engine.DeltaTime * 8f);
				badelineWalkApproach = Calc.Approach(badelineWalkApproach, 1f, Engine.DeltaTime * 0.6f);
				yield return null;
			}
		}

		private IEnumerator DustyRoutine(Entity oshiro)
		{
			List<Entity> dusty = new List<Entity>();
			float timer = 0f;
			Vector2 offset = oshiro.Position + new Vector2(220f, -24f);
			Vector2 start = offset;
			for (int j = 0; j < 3; j++)
			{
				Entity dust = new Entity(offset + new Vector2(j * 24, 0f));
				dust.Depth = -50;
				dust.Add(new DustGraphic(ignoreSolids: true, autoControlEyes: false, autoExpandDust: true));
				Image img = new Image(GFX.Game["decals/3-resort/brokenbox_" + (char)(97 + j)]);
				img.JustifyOrigin(0.5f, 1f);
				img.Position = new Vector2(0f, -4f);
				dust.Add(img);
				base.Scene.Add(dust);
				dusty.Add(dust);
			}
			yield return 3.8f;
			while (true)
			{
				for (int i = 0; i < dusty.Count; i++)
				{
					Entity entity = dusty[i];
					entity.X = offset.X + (float)(i * 24);
					entity.Y = offset.Y + (float)Math.Sin(timer * 4f + (float)i * 0.8f) * 4f;
				}
				if (offset.X < (float)(Level.Bounds.Left + 120))
				{
					offset.Y = Calc.Approach(offset.Y, start.Y + 16f, Engine.DeltaTime * 16f);
				}
				offset.X -= 26f * Engine.DeltaTime;
				timer += Engine.DeltaTime;
				yield return null;
			}
		}

		private IEnumerator BadelineAround(Vector2 start, Vector2 around, BadelineDummy badeline = null)
		{
			bool removeAtEnd = badeline == null;
			if (badeline == null)
			{
				Scene scene = base.Scene;
				BadelineDummy entity;
				badeline = (entity = new BadelineDummy(start));
				scene.Add(entity);
			}
			badeline.Sprite.Play("fallSlow");
			float angle = Calc.Angle(around, start);
			float dist = (around - start).Length();
			float duration = 3f;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
			{
				float a = p * 2f;
				badeline.Position = around + Calc.AngleToVector(angle - a * ((float)Math.PI * 2f), dist + Calc.YoYo(p) * 16f + (float)Math.Sin(p * ((float)Math.PI * 2f) * 4f) * 5f);
				badeline.Sprite.Scale.X = Math.Sign(around.X - badeline.X);
				if (!removeAtEnd)
				{
					player.Facing = (Facings)Math.Sign(badeline.X - player.X);
				}
				if (base.Scene.OnInterval(0.1f))
				{
					TrailManager.Add(badeline, Player.NormalHairColor);
				}
				yield return null;
			}
			if (removeAtEnd)
			{
				badeline.Vanish();
			}
			else
			{
				badeline.Sprite.Play("laugh");
			}
		}

		private IEnumerator DoEvent(string e)
		{
			switch (e)
			{
			case "WaitJumpDash":
				yield return EventWaitJumpDash();
				break;
			case "WaitJumpDoubleDash":
				yield return EventWaitJumpDoubleDash();
				break;
			case "ClimbDown":
				yield return EventClimbDown();
				break;
			case "Wait":
				yield return EventWait();
				break;
			}
		}

		private IEnumerator EventWaitJumpDash()
		{
			autoWalk = false;
			player.DummyFriction = true;
			yield return 0.1f;
			PlayerJump(-1);
			yield return 0.2f;
			player.OverrideDashDirection = new Vector2(-1f, -1f);
			player.StateMachine.State = player.StartDash();
			yield return 0.6f;
			player.OverrideDashDirection = null;
			player.StateMachine.State = 11;
			autoWalk = true;
		}

		private IEnumerator EventWaitJumpDoubleDash()
		{
			autoWalk = false;
			player.DummyFriction = true;
			yield return 0.1f;
			player.Facing = Facings.Right;
			yield return 0.25f;
			yield return BadelineCombine();
			player.Dashes = 2;
			yield return 0.5f;
			player.Facing = Facings.Left;
			yield return 0.7f;
			PlayerJump(-1);
			yield return 0.4f;
			player.OverrideDashDirection = new Vector2(-1f, -1f);
			player.StateMachine.State = player.StartDash();
			yield return 0.6f;
			player.OverrideDashDirection = new Vector2(-1f, 0f);
			player.StateMachine.State = player.StartDash();
			yield return 0.6f;
			player.OverrideDashDirection = null;
			player.StateMachine.State = 11;
			autoWalk = true;
			while (!player.OnGround())
			{
				yield return null;
			}
			autoWalk = false;
			player.DummyFriction = true;
			player.Dashes = 2;
			yield return 0.5f;
			player.Facing = Facings.Right;
			yield return 1f;
			Level.Displacement.AddBurst(player.Position, 0.4f, 8f, 32f, 0.5f);
			badeline.Position = player.Position;
			badeline.Visible = true;
			badelineAutoFloat = true;
			player.Dashes = 1;
			yield return 0.8f;
			player.Facing = Facings.Left;
			autoWalk = true;
			player.DummyFriction = false;
		}

		private IEnumerator EventClimbDown()
		{
			autoWalk = false;
			player.DummyFriction = true;
			yield return 0.1f;
			PlayerJump(-1);
			yield return 0.4f;
			while (!player.CollideCheck<Solid>(player.Position + new Vector2(-1f, 0f)))
			{
				yield return null;
			}
			player.DummyAutoAnimate = false;
			player.Sprite.Play("wallslide");
			while (player.CollideCheck<Solid>(player.Position + new Vector2(-1f, 32f)))
			{
				player.CreateWallSlideParticles(-1);
				player.Speed.Y = Math.Min(player.Speed.Y, 40f);
				yield return null;
			}
			PlayerJump(1);
			yield return 0.4f;
			while (!player.CollideCheck<Solid>(player.Position + new Vector2(1f, 0f)))
			{
				yield return null;
			}
			player.DummyAutoAnimate = false;
			player.Sprite.Play("wallslide");
			while (!player.CollideCheck<Solid>(player.Position + new Vector2(0f, 32f)))
			{
				player.CreateWallSlideParticles(1);
				player.Speed.Y = Math.Min(player.Speed.Y, 40f);
				yield return null;
			}
			PlayerJump(-1);
			yield return 0.4f;
			autoWalk = true;
		}

		private IEnumerator EventWait()
		{
			badeline.Sprite.Play("idle");
			badelineAutoWalk = false;
			autoWalk = false;
			player.DummyFriction = true;
			yield return 0.1f;
			player.DummyAutoAnimate = false;
			player.Speed = Vector2.Zero;
			yield return 0.5f;
			player.Sprite.Play("lookUp");
			yield return 2f;
			BirdNPC bird = base.Scene.Entities.FindFirst<BirdNPC>();
			if (bird != null)
			{
				bird.AutoFly = true;
			}
			yield return 0.1f;
			player.Sprite.Play("idle");
			yield return 1f;
			autoWalk = true;
			player.DummyFriction = false;
			player.DummyAutoAnimate = true;
			badelineAutoWalk = true;
			badelineWalkApproach = 0f;
			badelineWalkApproachFrom = badeline.Position;
			badeline.Sprite.Play("walk");
			while (badelineWalkApproach < 1f)
			{
				badelineWalkApproach += Engine.DeltaTime * 4f;
				yield return null;
			}
		}

		private IEnumerator BadelineCombine()
		{
			Vector2 from = badeline.Position;
			badelineAutoFloat = false;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.25f)
			{
				badeline.Position = Vector2.Lerp(from, player.Position, Ease.CubeIn(p));
				yield return null;
			}
			badeline.Visible = false;
			Level.Displacement.AddBurst(player.Position, 0.4f, 8f, 32f, 0.5f);
		}

		private void PlayerJump(int direction)
		{
			player.Facing = (Facings)direction;
			player.DummyFriction = false;
			player.DummyAutoAnimate = true;
			player.Speed.X = direction * 120;
			player.Jump();
			player.AutoJump = true;
			player.AutoJumpTimer = 2f;
		}

		private void SetBgFade(float alpha)
		{
			fillbg.Color = Color.Black * alpha;
		}

		public override void Update()
		{
			MInput.Disabled = false;
			if (Level.CanPause && (Input.Pause.Pressed || Input.ESC.Pressed))
			{
				Input.Pause.ConsumeBuffer();
				Input.ESC.ConsumeBuffer();
				Level.Pause(0, minimal: true);
			}
			MInput.Disabled = true;
			if (player != null && player.Scene != null)
			{
				if (player.OverrideDashDirection.HasValue)
				{
					Input.MoveX.Value = (int)player.OverrideDashDirection.Value.X;
					Input.MoveY.Value = (int)player.OverrideDashDirection.Value.Y;
				}
				if (autoWalk)
				{
					if (player.OnGround())
					{
						player.Speed.X = -44.8f;
						bool wall = player.CollideCheck<Solid>(player.Position + new Vector2(-20f, 0f));
						bool noground = !player.CollideCheck<Solid>(player.Position + new Vector2(-8f, 1f)) && !player.CollideCheck<Solid>(player.Position + new Vector2(-8f, 32f));
						if (wall || noground)
						{
							player.Jump();
							player.AutoJump = true;
							player.AutoJumpTimer = (wall ? 0.6f : 2f);
						}
					}
					else
					{
						player.Speed.X = -64f;
					}
				}
				if (badeline != null && badelineAutoFloat)
				{
					Vector2 from = badeline.Position;
					Vector2 to = player.Position + new Vector2(16f, -16f);
					badeline.Position = from + (to - from) * (1f - (float)Math.Pow(0.009999999776482582, Engine.DeltaTime));
					badeline.Sprite.Scale.X = -1f;
				}
				if (badeline != null && badelineAutoWalk)
				{
					player.GetChasePosition(base.Scene.TimeActive, 0.35f + (float)Math.Sin(walkOffset) * 0.1f, out var chaseState);
					if (chaseState.OnGround)
					{
						walkOffset += Engine.DeltaTime;
					}
					if (badelineWalkApproach >= 1f)
					{
						badeline.Position = chaseState.Position;
						if (badeline.Sprite.Has(chaseState.Animation))
						{
							badeline.Sprite.Play(chaseState.Animation);
						}
						badeline.Sprite.Scale.X = (float)chaseState.Facing;
					}
					else
					{
						badeline.Position = Vector2.Lerp(badelineWalkApproachFrom, chaseState.Position, badelineWalkApproach);
					}
				}
				if (Math.Abs(player.Speed.X) > 90f)
				{
					player.Speed.X = Calc.Approach(player.Speed.X, 90f * (float)Math.Sign(player.Speed.X), 1000f * Engine.DeltaTime);
				}
			}
			if (credits != null)
			{
				credits.Update();
			}
			base.Update();
		}

		public void PostUpdate()
		{
			if (player != null && player.Scene != null && autoUpdateCamera)
			{
				Vector2 from = Level.Camera.Position;
				Vector2 target = player.CameraTarget;
				if (!player.OnGround())
				{
					target.Y = (Level.Camera.Y * 2f + target.Y) / 3f;
				}
				Level.Camera.Position = from + (target - from) * (1f - (float)Math.Pow(0.009999999776482582, Engine.DeltaTime));
				Level.Camera.X = (int)target.X;
			}
		}

		public override void Render()
		{
			bool mirror = SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode;
			if (!Level.Paused)
			{
				if (mirror)
				{
					gradient.Draw(new Vector2(1720f, -10f), Vector2.Zero, Color.White * 0.6f, new Vector2(-1f, 1100f));
				}
				else
				{
					gradient.Draw(new Vector2(200f, -10f), Vector2.Zero, Color.White * 0.6f, new Vector2(1f, 1100f));
				}
			}
			if (fade > 0f)
			{
				Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * Ease.CubeInOut(fade));
			}
			if (credits != null && !Level.Paused)
			{
				credits.Render(new Vector2(mirror ? 100 : 1820, 0f));
			}
			base.Render();
		}

		public override void OnEnd(Level level)
		{
			SaveData.Instance.Assists.DashAssist = wasDashAssistOn;
			Audio.BusMuted("bus:/gameplay_sfx", false);
			Instance = null;
			MInput.Disabled = false;
			if (!gotoEpilogue)
			{
				Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaComplete, snow);
			}
			else
			{
				LevelEnter.Go(new Session(new AreaKey(8)), fromSaveData: false);
			}
		}
	}
}
