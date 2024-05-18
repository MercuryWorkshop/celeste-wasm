using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class HeartGem : Entity
	{
		private const string FAKE_HEART_FLAG = "fake_heart";

		public static ParticleType P_BlueShine;

		public static ParticleType P_RedShine;

		public static ParticleType P_GoldShine;

		public static ParticleType P_FakeShine;

		public bool IsGhost;

		public const float GhostAlpha = 0.8f;

		public bool IsFake;

		private Sprite sprite;

		private Sprite white;

		private ParticleType shineParticle;

		public Wiggler ScaleWiggler;

		private Wiggler moveWiggler;

		private Vector2 moveWiggleDir;

		private BloomPoint bloom;

		private VertexLight light;

		private Poem poem;

		private BirdNPC bird;

		private float timer;

		private bool collected;

		private bool autoPulse = true;

		private float bounceSfxDelay;

		private bool removeCameraTriggers;

		private SoundEmitter sfx;

		private List<InvisibleBarrier> walls = new List<InvisibleBarrier>();

		private HoldableCollider holdableCollider;

		private EntityID entityID;

		private InvisibleBarrier fakeRightWall;

		public HeartGem(Vector2 position)
			: base(position)
		{
			Add(holdableCollider = new HoldableCollider(OnHoldable));
			Add(new MirrorReflection());
		}

		public HeartGem(EntityData data, Vector2 offset)
			: this(data.Position + offset)
		{
			removeCameraTriggers = data.Bool("removeCameraTriggers");
			IsFake = data.Bool("fake");
			entityID = new EntityID(data.Level.Name, data.ID);
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			AreaKey area = (base.Scene as Level).Session.Area;
			IsGhost = !IsFake && SaveData.Instance.Areas[area.ID].Modes[(int)area.Mode].HeartGem;
			string spriteName = (IsFake ? "heartgem3" : ((!IsGhost) ? ("heartgem" + (int)area.Mode) : "heartGemGhost"));
			Add(sprite = GFX.SpriteBank.Create(spriteName));
			sprite.Play("spin");
			sprite.OnLoop = delegate(string anim)
			{
				if (Visible && anim == "spin" && autoPulse)
				{
					if (IsFake)
					{
						Audio.Play("event:/new_content/game/10_farewell/fakeheart_pulse", Position);
					}
					else
					{
						Audio.Play("event:/game/general/crystalheart_pulse", Position);
					}
					ScaleWiggler.Start();
					(base.Scene as Level).Displacement.AddBurst(Position, 0.35f, 8f, 48f, 0.25f);
				}
			};
			if (IsGhost)
			{
				sprite.Color = Color.White * 0.8f;
			}
			base.Collider = new Hitbox(16f, 16f, -8f, -8f);
			Add(new PlayerCollider(OnPlayer));
			Add(ScaleWiggler = Wiggler.Create(0.5f, 4f, delegate(float f)
			{
				sprite.Scale = Vector2.One * (1f + f * 0.25f);
			}));
			Add(bloom = new BloomPoint(0.75f, 16f));
			Color color;
			if (IsFake)
			{
				color = Calc.HexToColor("dad8cc");
				shineParticle = P_FakeShine;
			}
			else if (area.Mode == AreaMode.Normal)
			{
				color = Color.Aqua;
				shineParticle = P_BlueShine;
			}
			else if (area.Mode == AreaMode.BSide)
			{
				color = Color.Red;
				shineParticle = P_RedShine;
			}
			else
			{
				color = Color.Gold;
				shineParticle = P_GoldShine;
			}
			color = Color.Lerp(color, Color.White, 0.5f);
			Add(light = new VertexLight(color, 1f, 32, 64));
			if (IsFake)
			{
				bloom.Alpha = 0f;
				light.Alpha = 0f;
			}
			moveWiggler = Wiggler.Create(0.8f, 2f);
			moveWiggler.StartZero = true;
			Add(moveWiggler);
			if (!IsFake)
			{
				return;
			}
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if ((player != null && player.X > base.X) || (scene as Level).Session.GetFlag("fake_heart"))
			{
				Visible = false;
				Alarm.Set(this, 0.0001f, delegate
				{
					FakeRemoveCameraTrigger();
					RemoveSelf();
				});
			}
			else
			{
				scene.Add(fakeRightWall = new InvisibleBarrier(new Vector2(base.X + 160f, base.Y - 200f), 8f, 400f));
			}
		}

		public override void Update()
		{
			bounceSfxDelay -= Engine.DeltaTime;
			timer += Engine.DeltaTime;
			sprite.Position = Vector2.UnitY * (float)Math.Sin(timer * 2f) * 2f + moveWiggleDir * moveWiggler.Value * -8f;
			if (white != null)
			{
				white.Position = sprite.Position;
				white.Scale = sprite.Scale;
				if (white.CurrentAnimationID != sprite.CurrentAnimationID)
				{
					white.Play(sprite.CurrentAnimationID);
				}
				white.SetAnimationFrame(sprite.CurrentAnimationFrame);
			}
			if (collected)
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player == null || player.Dead)
				{
					EndCutscene();
				}
			}
			base.Update();
			if (!collected && base.Scene.OnInterval(0.1f))
			{
				SceneAs<Level>().Particles.Emit(shineParticle, 1, base.Center, Vector2.One * 8f);
			}
		}

		public void OnHoldable(Holdable h)
		{
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (!collected && player != null && h.Dangerous(holdableCollider))
			{
				Collect(player);
			}
		}

		public void OnPlayer(Player player)
		{
			if (collected || (base.Scene as Level).Frozen)
			{
				return;
			}
			if (player.DashAttacking)
			{
				Collect(player);
				return;
			}
			if (bounceSfxDelay <= 0f)
			{
				if (IsFake)
				{
					Audio.Play("event:/new_content/game/10_farewell/fakeheart_bounce", Position);
				}
				else
				{
					Audio.Play("event:/game/general/crystalheart_bounce", Position);
				}
				bounceSfxDelay = 0.1f;
			}
			player.PointBounce(base.Center);
			moveWiggler.Start();
			ScaleWiggler.Start();
			moveWiggleDir = (base.Center - player.Center).SafeNormalize(Vector2.UnitY);
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
		}

		private void Collect(Player player)
		{
			base.Scene.Tracker.GetEntity<AngryOshiro>()?.StopControllingTime();
			Coroutine routine = new Coroutine(CollectRoutine(player));
			routine.UseRawDeltaTime = true;
			Add(routine);
			collected = true;
			if (!removeCameraTriggers)
			{
				return;
			}
			foreach (CameraOffsetTrigger item in base.Scene.Entities.FindAll<CameraOffsetTrigger>())
			{
				item.RemoveSelf();
			}
		}

		private IEnumerator CollectRoutine(Player player)
		{
			Level level = base.Scene as Level;
			AreaKey area = level.Session.Area;
			string poemID = AreaData.Get(level).Mode[(int)area.Mode].PoemID;
			bool completeArea = !IsFake && (area.Mode != 0 || area.ID == 9);
			if (IsFake)
			{
				level.StartCutscene(SkipFakeHeartCutscene);
			}
			else
			{
				level.CanRetry = false;
			}
			if (completeArea || IsFake)
			{
				Audio.SetMusic(null);
				Audio.SetAmbience(null);
			}
			if (completeArea)
			{
				List<Strawberry> strawbs = new List<Strawberry>();
				foreach (Follower follower in player.Leader.Followers)
				{
					if (follower.Entity is Strawberry)
					{
						strawbs.Add(follower.Entity as Strawberry);
					}
				}
				foreach (Strawberry item in strawbs)
				{
					item.OnCollect();
				}
			}
			string sfxEvent = "event:/game/general/crystalheart_blue_get";
			if (IsFake)
			{
				sfxEvent = "event:/new_content/game/10_farewell/fakeheart_get";
			}
			else if (area.Mode == AreaMode.BSide)
			{
				sfxEvent = "event:/game/general/crystalheart_red_get";
			}
			else if (area.Mode == AreaMode.CSide)
			{
				sfxEvent = "event:/game/general/crystalheart_gold_get";
			}
			sfx = SoundEmitter.Play(sfxEvent, this);
			Add(new LevelEndingHook(delegate
			{
				sfx.Source.Stop();
			}));
			walls.Add(new InvisibleBarrier(new Vector2(level.Bounds.Right, level.Bounds.Top), 8f, level.Bounds.Height));
			walls.Add(new InvisibleBarrier(new Vector2(level.Bounds.Left - 8, level.Bounds.Top), 8f, level.Bounds.Height));
			walls.Add(new InvisibleBarrier(new Vector2(level.Bounds.Left, level.Bounds.Top - 8), level.Bounds.Width, 8f));
			foreach (InvisibleBarrier wall in walls)
			{
				base.Scene.Add(wall);
			}
			Add(white = GFX.SpriteBank.Create("heartGemWhite"));
			base.Depth = -2000000;
			yield return null;
			Celeste.Freeze(0.2f);
			yield return null;
			Engine.TimeRate = 0.5f;
			player.Depth = -2000000;
			for (int i = 0; i < 10; i++)
			{
				base.Scene.Add(new AbsorbOrb(Position));
			}
			level.Shake();
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			level.Flash(Color.White);
			level.FormationBackdrop.Display = true;
			level.FormationBackdrop.Alpha = 1f;
			light.Alpha = (bloom.Alpha = 0f);
			Visible = false;
			for (float t3 = 0f; t3 < 2f; t3 += Engine.RawDeltaTime)
			{
				Engine.TimeRate = Calc.Approach(Engine.TimeRate, 0f, Engine.RawDeltaTime * 0.25f);
				yield return null;
			}
			yield return null;
			if (player.Dead)
			{
				yield return 100f;
			}
			Engine.TimeRate = 1f;
			base.Tag = Tags.FrozenUpdate;
			level.Frozen = true;
			if (!IsFake)
			{
				RegisterAsCollected(level, poemID);
				if (completeArea)
				{
					level.TimerStopped = true;
					level.RegisterAreaComplete();
				}
			}
			string poemText = null;
			if (!string.IsNullOrEmpty(poemID))
			{
				poemText = Dialog.Clean("poem_" + poemID);
			}
			poem = new Poem(poemText, (int)(IsFake ? ((AreaMode)3) : area.Mode), (area.Mode == AreaMode.CSide || IsFake) ? 1f : 0.6f);
			poem.Alpha = 0f;
			base.Scene.Add(poem);
			for (float t3 = 0f; t3 < 1f; t3 += Engine.RawDeltaTime)
			{
				poem.Alpha = Ease.CubeOut(t3);
				yield return null;
			}
			if (IsFake)
			{
				yield return DoFakeRoutineWithBird(player);
				yield break;
			}
			while (!Input.MenuConfirm.Pressed && !Input.MenuCancel.Pressed)
			{
				yield return null;
			}
			sfx.Source.Param("end", 1f);
			if (!completeArea)
			{
				level.FormationBackdrop.Display = false;
				for (float t3 = 0f; t3 < 1f; t3 += Engine.RawDeltaTime * 2f)
				{
					poem.Alpha = Ease.CubeIn(1f - t3);
					yield return null;
				}
				player.Depth = 0;
				EndCutscene();
			}
			else
			{
				FadeWipe wipe = new FadeWipe(level, wipeIn: false);
				wipe.Duration = 3.25f;
				yield return wipe.Duration;
				level.CompleteArea(spotlightWipe: false, skipScreenWipe: true);
			}
		}

		private void EndCutscene()
		{
			Level obj = base.Scene as Level;
			obj.Frozen = false;
			obj.CanRetry = true;
			obj.FormationBackdrop.Display = false;
			Engine.TimeRate = 1f;
			if (poem != null)
			{
				poem.RemoveSelf();
			}
			foreach (InvisibleBarrier wall in walls)
			{
				wall.RemoveSelf();
			}
			RemoveSelf();
		}

		private void RegisterAsCollected(Level level, string poemID)
		{
			level.Session.HeartGem = true;
			level.Session.UpdateLevelStartDashes();
			int unlockedModes = SaveData.Instance.UnlockedModes;
			SaveData.Instance.RegisterHeartGem(level.Session.Area);
			if (!string.IsNullOrEmpty(poemID))
			{
				SaveData.Instance.RegisterPoemEntry(poemID);
			}
			if (unlockedModes < 3 && SaveData.Instance.UnlockedModes >= 3)
			{
				level.Session.UnlockedCSide = true;
			}
			if (SaveData.Instance.TotalHeartGems >= 24)
			{
				Achievements.Register(Achievement.CSIDES);
			}
		}

		private IEnumerator DoFakeRoutineWithBird(Player player)
		{
			Level level = base.Scene as Level;
			int panAmount = 64;
			Vector2 panFrom = level.Camera.Position;
			Vector2 panTo = level.Camera.Position + new Vector2(-panAmount, 0f);
			Vector2 birdFrom = new Vector2(panTo.X - 16f, player.Y - 20f);
			Vector2 birdTo = new Vector2(panFrom.X + 320f + 16f, player.Y - 20f);
			yield return 2f;
			Glitch.Value = 0.75f;
			while (Glitch.Value > 0f)
			{
				Glitch.Value = Calc.Approach(Glitch.Value, 0f, Engine.RawDeltaTime * 4f);
				level.Shake();
				yield return null;
			}
			yield return 1.1f;
			Glitch.Value = 0.75f;
			while (Glitch.Value > 0f)
			{
				Glitch.Value = Calc.Approach(Glitch.Value, 0f, Engine.RawDeltaTime * 4f);
				level.Shake();
				yield return null;
			}
			yield return 0.4f;
			for (float p3 = 0f; p3 < 1f; p3 += Engine.RawDeltaTime / 2f)
			{
				level.Camera.Position = panFrom + (panTo - panFrom) * Ease.CubeInOut(p3);
				poem.Offset = new Vector2(panAmount * 8, 0f) * Ease.CubeInOut(p3);
				yield return null;
			}
			bird = new BirdNPC(birdFrom, BirdNPC.Modes.None);
			bird.Sprite.Play("fly");
			bird.Sprite.UseRawDeltaTime = true;
			bird.Facing = Facings.Right;
			bird.Depth = -2000100;
			bird.Tag = Tags.FrozenUpdate;
			bird.Add(new VertexLight(Color.White, 0.5f, 8, 32));
			bird.Add(new BloomPoint(0.5f, 12f));
			level.Add(bird);
			for (float p3 = 0f; p3 < 1f; p3 += Engine.RawDeltaTime / 2.6f)
			{
				level.Camera.Position = panTo + (panFrom - panTo) * Ease.CubeInOut(p3);
				poem.Offset = new Vector2(panAmount * 8, 0f) * Ease.CubeInOut(1f - p3);
				float birdStartEase = 0.1f;
				float birdEndEase = 0.9f;
				if (p3 > birdStartEase && p3 <= birdEndEase)
				{
					float e = (p3 - birdStartEase) / (birdEndEase - birdStartEase);
					bird.Position = birdFrom + (birdTo - birdFrom) * e + Vector2.UnitY * (float)Math.Sin(e * 8f) * 8f;
				}
				if (level.OnRawInterval(0.2f))
				{
					TrailManager.Add(bird, Calc.HexToColor("639bff"), 1f, frozenUpdate: true, useRawDeltaTime: true);
				}
				yield return null;
			}
			bird.RemoveSelf();
			bird = null;
			Engine.TimeRate = 0f;
			level.Frozen = false;
			player.Active = false;
			player.StateMachine.State = 11;
			while (Engine.TimeRate != 1f)
			{
				Engine.TimeRate = Calc.Approach(Engine.TimeRate, 1f, 0.5f * Engine.RawDeltaTime);
				yield return null;
			}
			Engine.TimeRate = 1f;
			yield return Textbox.Say("CH9_FAKE_HEART");
			sfx.Source.Param("end", 1f);
			yield return 0.283f;
			level.FormationBackdrop.Display = false;
			for (float p3 = 0f; p3 < 1f; p3 += Engine.RawDeltaTime / 0.2f)
			{
				poem.TextAlpha = Ease.CubeIn(1f - p3);
				poem.ParticleSpeed = poem.TextAlpha;
				yield return null;
			}
			poem.Heart.Play("break");
			while (poem.Heart.Animating)
			{
				poem.Shake += Engine.DeltaTime;
				yield return null;
			}
			poem.RemoveSelf();
			poem = null;
			for (int i = 0; i < 10; i++)
			{
				Vector2 pos = level.Camera.Position + new Vector2(320f, 180f) * 0.5f;
				Vector2 tar = level.Camera.Position + new Vector2(160f, -64f);
				base.Scene.Add(new AbsorbOrb(pos, null, tar));
			}
			level.Shake();
			Glitch.Value = 0.8f;
			while (Glitch.Value > 0f)
			{
				Glitch.Value -= Engine.DeltaTime * 4f;
				yield return null;
			}
			yield return 0.25f;
			level.Session.Audio.Music.Event = "event:/new_content/music/lvl10/intermission_heartgroove";
			level.Session.Audio.Apply();
			player.Active = true;
			player.Depth = 0;
			player.StateMachine.State = 11;
			while (!player.OnGround() && player.Bottom < (float)level.Bounds.Bottom)
			{
				yield return null;
			}
			player.Facing = Facings.Right;
			yield return 0.5f;
			yield return Textbox.Say("CH9_KEEP_GOING", PlayerStepForward);
			SkipFakeHeartCutscene(level);
			level.EndCutscene();
		}

		private IEnumerator PlayerStepForward()
		{
			yield return 0.1f;
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null && player.CollideCheck<Solid>(player.Position + new Vector2(12f, 1f)))
			{
				yield return player.DummyWalkToExact((int)player.X + 10);
			}
			yield return 0.2f;
		}

		private void SkipFakeHeartCutscene(Level level)
		{
			Engine.TimeRate = 1f;
			Glitch.Value = 0f;
			if (sfx != null)
			{
				sfx.Source.Stop();
			}
			level.Session.SetFlag("fake_heart");
			level.Frozen = false;
			level.FormationBackdrop.Display = false;
			level.Session.Audio.Music.Event = "event:/new_content/music/lvl10/intermission_heartgroove";
			level.Session.Audio.Apply();
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				player.Sprite.Play("idle");
				player.Active = true;
				player.StateMachine.State = 0;
				player.Dashes = 1;
				player.Speed = Vector2.Zero;
				player.MoveV(200f);
				player.Depth = 0;
				for (int i = 0; i < 10; i++)
				{
					player.UpdateHair(applyGravity: true);
				}
			}
			foreach (AbsorbOrb item in base.Scene.Entities.FindAll<AbsorbOrb>())
			{
				item.RemoveSelf();
			}
			if (poem != null)
			{
				poem.RemoveSelf();
			}
			if (bird != null)
			{
				bird.RemoveSelf();
			}
			if (fakeRightWall != null)
			{
				fakeRightWall.RemoveSelf();
			}
			FakeRemoveCameraTrigger();
			foreach (InvisibleBarrier wall in walls)
			{
				wall.RemoveSelf();
			}
			RemoveSelf();
		}

		private void FakeRemoveCameraTrigger()
		{
			CameraTargetTrigger trigger = CollideFirst<CameraTargetTrigger>();
			if (trigger != null)
			{
				trigger.LerpStrength = 0f;
			}
		}
	}
}
