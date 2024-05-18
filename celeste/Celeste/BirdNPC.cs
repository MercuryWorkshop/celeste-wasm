using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class BirdNPC : Actor
	{
		public enum Modes
		{
			ClimbingTutorial,
			DashingTutorial,
			DreamJumpTutorial,
			SuperWallJumpTutorial,
			HyperJumpTutorial,
			FlyAway,
			None,
			Sleeping,
			MoveToNodes,
			WaitForLightningOff
		}

		public static ParticleType P_Feather;

		private static string FlownFlag = "bird_fly_away_";

		public Facings Facing = Facings.Left;

		public Sprite Sprite;

		public Vector2 StartPosition;

		public VertexLight Light;

		public bool AutoFly;

		public EntityID EntityID;

		public bool FlyAwayUp = true;

		public float WaitForLightningPostDelay;

		public bool DisableFlapSfx;

		private Coroutine tutorialRoutine;

		private Modes mode;

		private BirdTutorialGui gui;

		private Level level;

		private Vector2[] nodes;

		private StaticMover staticMover;

		private bool onlyOnce;

		private bool onlyIfPlayerLeft;

		public BirdNPC(Vector2 position, Modes mode)
			: base(position)
		{
			Add(Sprite = GFX.SpriteBank.Create("bird"));
			Sprite.Scale.X = (float)Facing;
			Sprite.UseRawDeltaTime = true;
			Sprite.OnFrameChange = delegate(string spr)
			{
				if (level != null && base.X > level.Camera.Left + 64f && base.X < level.Camera.Right - 64f && (spr.Equals("peck") || spr.Equals("peckRare")) && Sprite.CurrentAnimationFrame == 6)
				{
					Audio.Play("event:/game/general/bird_peck", Position);
				}
				if (level != null && level.Session.Area.ID == 10 && !DisableFlapSfx)
				{
					FlapSfxCheck(Sprite);
				}
			};
			Add(Light = new VertexLight(new Vector2(0f, -8f), Color.White, 1f, 8, 32));
			StartPosition = Position;
			SetMode(mode);
		}

		public BirdNPC(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Enum("mode", Modes.None))
		{
			EntityID = new EntityID(data.Level.Name, data.ID);
			nodes = data.NodesOffset(offset);
			onlyOnce = data.Bool("onlyOnce");
			onlyIfPlayerLeft = data.Bool("onlyIfPlayerLeft");
		}

		public void SetMode(Modes mode)
		{
			this.mode = mode;
			if (tutorialRoutine != null)
			{
				tutorialRoutine.RemoveSelf();
			}
			switch (mode)
			{
			case Modes.ClimbingTutorial:
				Add(tutorialRoutine = new Coroutine(ClimbingTutorial()));
				break;
			case Modes.DashingTutorial:
				Add(tutorialRoutine = new Coroutine(DashingTutorial()));
				break;
			case Modes.DreamJumpTutorial:
				Add(tutorialRoutine = new Coroutine(DreamJumpTutorial()));
				break;
			case Modes.SuperWallJumpTutorial:
				Add(tutorialRoutine = new Coroutine(SuperWallJumpTutorial()));
				break;
			case Modes.HyperJumpTutorial:
				Add(tutorialRoutine = new Coroutine(HyperJumpTutorial()));
				break;
			case Modes.FlyAway:
				Add(tutorialRoutine = new Coroutine(WaitRoutine()));
				break;
			case Modes.Sleeping:
				Sprite.Play("sleep");
				Facing = Facings.Right;
				break;
			case Modes.MoveToNodes:
				Add(tutorialRoutine = new Coroutine(MoveToNodesRoutine()));
				break;
			case Modes.WaitForLightningOff:
				Add(tutorialRoutine = new Coroutine(WaitForLightningOffRoutine()));
				break;
			}
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			level = scene as Level;
			if (mode == Modes.ClimbingTutorial && level.Session.GetLevelFlag("2"))
			{
				RemoveSelf();
			}
			else if (mode == Modes.FlyAway && level.Session.GetFlag(FlownFlag + level.Session.Level))
			{
				RemoveSelf();
			}
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (mode == Modes.SuperWallJumpTutorial)
			{
				Player player2 = scene.Tracker.GetEntity<Player>();
				if (player2 != null && player2.Y < base.Y + 32f)
				{
					RemoveSelf();
				}
			}
			if (onlyIfPlayerLeft)
			{
				Player player = level.Tracker.GetEntity<Player>();
				if (player != null && player.X > base.X)
				{
					RemoveSelf();
				}
			}
		}

		public override bool IsRiding(Solid solid)
		{
			return base.Scene.CollideCheck(new Rectangle((int)base.X - 4, (int)base.Y, 8, 2), solid);
		}

		public override void Update()
		{
			Sprite.Scale.X = (float)Facing;
			base.Update();
		}

		public IEnumerator Caw()
		{
			Sprite.Play("croak");
			while (Sprite.CurrentAnimationFrame < 9)
			{
				yield return null;
			}
			Audio.Play("event:/game/general/bird_squawk", Position);
		}

		public IEnumerator ShowTutorial(BirdTutorialGui gui, bool caw = false)
		{
			if (caw)
			{
				yield return Caw();
			}
			this.gui = gui;
			gui.Open = true;
			base.Scene.Add(gui);
			while (gui.Scale < 1f)
			{
				yield return null;
			}
		}

		public IEnumerator HideTutorial()
		{
			if (gui != null)
			{
				gui.Open = false;
				while (gui.Scale > 0f)
				{
					yield return null;
				}
				base.Scene.Remove(gui);
				gui = null;
			}
		}

		public IEnumerator StartleAndFlyAway()
		{
			base.Depth = -1000000;
			level.Session.SetFlag(FlownFlag + level.Session.Level);
			yield return Startle("event:/game/general/bird_startle");
			yield return FlyAway();
		}

		public IEnumerator FlyAway(float upwardsMultiplier = 1f)
		{
			if (staticMover != null)
			{
				staticMover.RemoveSelf();
				staticMover = null;
			}
			Sprite.Play("fly");
			Facing = (Facings)(0 - Facing);
			Vector2 speed = new Vector2((int)Facing * 20, -40f * upwardsMultiplier);
			while (base.Y > (float)level.Bounds.Top)
			{
				speed += new Vector2((int)Facing * 140, -120f * upwardsMultiplier) * Engine.DeltaTime;
				Position += speed * Engine.DeltaTime;
				yield return null;
			}
			RemoveSelf();
		}

		private IEnumerator ClimbingTutorial()
		{
			yield return 0.25f;
			Player p = base.Scene.Tracker.GetEntity<Player>();
			while (Math.Abs(p.X - base.X) > 120f)
			{
				yield return null;
			}
			BirdTutorialGui tut1 = new BirdTutorialGui(this, new Vector2(0f, -16f), Dialog.Clean("tutorial_climb"), Dialog.Clean("tutorial_hold"), BirdTutorialGui.ButtonPrompt.Grab);
			BirdTutorialGui tut2 = new BirdTutorialGui(this, new Vector2(0f, -16f), Dialog.Clean("tutorial_climb"), BirdTutorialGui.ButtonPrompt.Grab, "+", new Vector2(0f, -1f));
			bool first = true;
			bool willEnd;
			do
			{
				yield return ShowTutorial(tut1, first);
				first = false;
				while (p.StateMachine.State != 1 && p.Y > base.Y)
				{
					yield return null;
				}
				if (p.Y > base.Y)
				{
					Audio.Play("event:/ui/game/tutorial_note_flip_back");
					yield return HideTutorial();
					yield return ShowTutorial(tut2);
				}
				while (p.Scene != null && (!p.OnGround() || p.StateMachine.State == 1))
				{
					yield return null;
				}
				willEnd = p.Y <= base.Y + 4f;
				if (!willEnd)
				{
					Audio.Play("event:/ui/game/tutorial_note_flip_front");
				}
				yield return HideTutorial();
			}
			while (!willEnd);
			yield return StartleAndFlyAway();
		}

		private IEnumerator DashingTutorial()
		{
			base.Y = level.Bounds.Top;
			base.X += 32f;
			yield return 1f;
			Player player = base.Scene.Tracker.GetEntity<Player>();
			Bridge bridge = base.Scene.Entities.FindFirst<Bridge>();
			while ((player == null || !(player.X > StartPosition.X - 92f) || !(player.Y > StartPosition.Y - 20f) || !(player.Y < StartPosition.Y - 10f)) && (!SaveData.Instance.Assists.Invincible || player == null || !(player.X > StartPosition.X - 60f) || !(player.Y > StartPosition.Y) || !(player.Y < StartPosition.Y + 34f)))
			{
				yield return null;
			}
			base.Scene.Add(new CS00_Ending(player, this, bridge));
		}

		private IEnumerator DreamJumpTutorial()
		{
			yield return ShowTutorial(new BirdTutorialGui(this, new Vector2(0f, -16f), Dialog.Clean("tutorial_dreamjump"), new Vector2(1f, 0f), "+", BirdTutorialGui.ButtonPrompt.Jump), caw: true);
			while (true)
			{
				Player p2 = base.Scene.Tracker.GetEntity<Player>();
				if (p2 != null && (p2.X > base.X || (Position - p2.Position).Length() < 32f))
				{
					break;
				}
				yield return null;
			}
			yield return HideTutorial();
			while (true)
			{
				Player p = base.Scene.Tracker.GetEntity<Player>();
				if (p != null && (Position - p.Position).Length() < 24f)
				{
					break;
				}
				yield return null;
			}
			yield return StartleAndFlyAway();
		}

		private IEnumerator SuperWallJumpTutorial()
		{
			Facing = Facings.Right;
			yield return 0.25f;
			bool first = true;
			BirdTutorialGui tut1 = new BirdTutorialGui(this, new Vector2(0f, -16f), GFX.Gui["hyperjump/tutorial00"], Dialog.Clean("TUTORIAL_DASH"), new Vector2(0f, -1f));
			BirdTutorialGui tut2 = new BirdTutorialGui(this, new Vector2(0f, -16f), GFX.Gui["hyperjump/tutorial01"], Dialog.Clean("TUTORIAL_DREAMJUMP"));
			Player player;
			do
			{
				yield return ShowTutorial(tut1, first);
				Sprite.Play("idleRarePeck");
				yield return 2f;
				gui = tut2;
				gui.Open = true;
				gui.Scale = 1f;
				base.Scene.Add(gui);
				yield return null;
				tut1.Open = false;
				tut1.Scale = 0f;
				base.Scene.Remove(tut1);
				yield return 2f;
				yield return HideTutorial();
				yield return 2f;
				first = false;
				player = base.Scene.Tracker.GetEntity<Player>();
			}
			while (player == null || !(player.Y <= base.Y) || !(player.X > base.X + 144f));
			yield return StartleAndFlyAway();
		}

		private IEnumerator HyperJumpTutorial()
		{
			Facing = Facings.Left;
			BirdTutorialGui tut = new BirdTutorialGui(this, new Vector2(0f, -16f), Dialog.Clean("TUTORIAL_DREAMJUMP"), new Vector2(1f, 1f), "+", BirdTutorialGui.ButtonPrompt.Dash, GFX.Gui["tinyarrow"], BirdTutorialGui.ButtonPrompt.Jump);
			yield return 0.3f;
			yield return ShowTutorial(tut, caw: true);
		}

		private IEnumerator WaitRoutine()
		{
			while (!AutoFly)
			{
				Player player2 = base.Scene.Tracker.GetEntity<Player>();
				if (player2 != null && Math.Abs(player2.X - base.X) < 120f)
				{
					break;
				}
				yield return null;
			}
			yield return Caw();
			while (!AutoFly)
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null && (player.Center - Position).Length() < 32f)
				{
					break;
				}
				yield return null;
			}
			yield return StartleAndFlyAway();
		}

		public IEnumerator Startle(string startleSound, float duration = 0.8f, Vector2? multiplier = null)
		{
			if (!multiplier.HasValue)
			{
				multiplier = new Vector2(1f, 1f);
			}
			if (!string.IsNullOrWhiteSpace(startleSound))
			{
				Audio.Play(startleSound, Position);
			}
			Dust.Burst(Position, -(float)Math.PI / 2f, 8);
			Sprite.Play("jump");
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, duration, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				if (t.Eased < 0.5f && base.Scene.OnInterval(0.05f))
				{
					level.Particles.Emit(P_Feather, 2, Position + Vector2.UnitY * -6f, Vector2.One * 4f);
				}
				Vector2 vector = Vector2.Lerp(new Vector2(100f, -100f) * multiplier.Value, new Vector2(20f, -20f) * multiplier.Value, t.Eased);
				vector.X *= 0 - Facing;
				Position += vector * Engine.DeltaTime;
			};
			Add(tween);
			while (tween.Active)
			{
				yield return null;
			}
		}

		public IEnumerator FlyTo(Vector2 target, float durationMult = 1f, bool relocateSfx = true)
		{
			Sprite.Play("fly");
			if (relocateSfx)
			{
				Add(new SoundSource().Play("event:/new_content/game/10_farewell/bird_relocate"));
			}
			int dir = Math.Sign(target.X - base.X);
			if (dir != 0)
			{
				Facing = (Facings)dir;
			}
			Vector2 start = Position;
			Vector2 end = target;
			SimpleCurve curve = new SimpleCurve(start, end, start + (end - start) * 0.75f - Vector2.UnitY * 30f);
			float duration = (end - start).Length() / 100f * durationMult;
			for (float p = 0f; p < 0.95f; p += Engine.DeltaTime / duration)
			{
				Position = curve.GetPoint(Ease.SineInOut(p)).Floor();
				Sprite.Rate = 1f - p * 0.5f;
				yield return null;
			}
			Dust.Burst(Position, -(float)Math.PI / 2f, 8);
			Position = target;
			Facing = Facings.Left;
			Sprite.Rate = 1f;
			Sprite.Play("idle");
		}

		private IEnumerator MoveToNodesRoutine()
		{
			int index = 0;
			while (true)
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player == null || !((player.Center - Position).Length() < 80f))
				{
					yield return null;
					continue;
				}
				base.Depth = -1000000;
				yield return Startle("event:/new_content/game/10_farewell/bird_startle", 0.2f);
				if (index < nodes.Length)
				{
					yield return FlyTo(nodes[index], 0.6f);
					index++;
					continue;
				}
				base.Tag = Tags.Persistent;
				Add(new SoundSource().Play("event:/new_content/game/10_farewell/bird_relocate"));
				if (onlyOnce)
				{
					level.Session.DoNotLoad.Add(EntityID);
				}
				Sprite.Play("fly");
				Facing = Facings.Right;
				Vector2 speed = new Vector2((int)Facing * 20, -40f);
				while (base.Y > (float)(level.Bounds.Top - 200))
				{
					speed += new Vector2((int)Facing * 140, -60f) * Engine.DeltaTime;
					Position += speed * Engine.DeltaTime;
					yield return null;
				}
				RemoveSelf();
			}
		}

		private IEnumerator WaitForLightningOffRoutine()
		{
			Sprite.Play("hoverStressed");
			while (base.Scene.Entities.FindFirst<Lightning>() != null)
			{
				yield return null;
			}
			if (WaitForLightningPostDelay > 0f)
			{
				yield return WaitForLightningPostDelay;
			}
			if (!FlyAwayUp)
			{
				Sprite.Play("fly");
				Vector2 speed2 = new Vector2((int)Facing * 20, -10f);
				while (base.Y > (float)level.Bounds.Top)
				{
					speed2 += new Vector2((int)Facing * 140, -10f) * Engine.DeltaTime;
					Position += speed2 * Engine.DeltaTime;
					yield return null;
				}
			}
			else
			{
				Sprite.Play("flyup");
				Vector2 speed2 = new Vector2(0f, -32f);
				while (base.Y > (float)level.Bounds.Top)
				{
					speed2 += new Vector2(0f, -100f) * Engine.DeltaTime;
					Position += speed2 * Engine.DeltaTime;
					yield return null;
				}
			}
			RemoveSelf();
		}

		public override void SceneEnd(Scene scene)
		{
			Engine.TimeRate = 1f;
			base.SceneEnd(scene);
		}

		public override void DebugRender(Camera camera)
		{
			base.DebugRender(camera);
			if (mode == Modes.DashingTutorial)
			{
				float left = StartPosition.X - 92f;
				float right2 = level.Bounds.Right;
				float top2 = StartPosition.Y - 20f;
				float bottom2 = StartPosition.Y - 10f;
				Draw.Line(new Vector2(left, top2), new Vector2(left, bottom2), Color.Aqua);
				Draw.Line(new Vector2(left, top2), new Vector2(right2, top2), Color.Aqua);
				Draw.Line(new Vector2(right2, top2), new Vector2(right2, bottom2), Color.Aqua);
				Draw.Line(new Vector2(left, bottom2), new Vector2(right2, bottom2), Color.Aqua);
				float left2 = StartPosition.X - 60f;
				float right = level.Bounds.Right;
				float top = StartPosition.Y;
				float bottom = StartPosition.Y + 34f;
				Draw.Line(new Vector2(left2, top), new Vector2(left2, bottom), Color.Aqua);
				Draw.Line(new Vector2(left2, top), new Vector2(right, top), Color.Aqua);
				Draw.Line(new Vector2(right, top), new Vector2(right, bottom), Color.Aqua);
				Draw.Line(new Vector2(left2, bottom), new Vector2(right, bottom), Color.Aqua);
			}
		}

		public static void FlapSfxCheck(Sprite sprite)
		{
			if (sprite.Entity != null && sprite.Entity.Scene != null)
			{
				Camera cam = (sprite.Entity.Scene as Level).Camera;
				Vector2 pos = sprite.RenderPosition;
				if (pos.X < cam.X - 32f || pos.Y < cam.Y - 32f || pos.X > cam.X + 320f + 32f || pos.Y > cam.Y + 180f + 32f)
				{
					return;
				}
			}
			string anim = sprite.CurrentAnimationID;
			int frame = sprite.CurrentAnimationFrame;
			if ((anim == "hover" && frame == 0) || (anim == "hoverStressed" && frame == 0) || (anim == "fly" && frame == 0))
			{
				Audio.Play("event:/new_content/game/10_farewell/bird_wingflap", sprite.RenderPosition);
			}
		}
	}
}
