using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class FlingBirdIntro : Entity
	{
		public Vector2 BirdEndPosition;

		public Sprite Sprite;

		public SoundEmitter CrashSfxEmitter;

		private Vector2[] nodes;

		private bool startedRoutine;

		private Vector2 start;

		private InvisibleBarrier fakeRightWall;

		private bool crashes;

		private Coroutine flyToRoutine;

		private bool emitParticles;

		private bool inCutscene;

		public FlingBirdIntro(Vector2 position, Vector2[] nodes, bool crashes)
			: base(position)
		{
			this.crashes = crashes;
			Add(Sprite = GFX.SpriteBank.Create("bird"));
			Sprite.Play(crashes ? "hoverStressed" : "hover");
			Sprite.Scale.X = ((!crashes) ? 1 : (-1));
			Sprite.OnFrameChange = delegate
			{
				if (!inCutscene)
				{
					BirdNPC.FlapSfxCheck(Sprite);
				}
			};
			base.Collider = new Circle(16f, 0f, -8f);
			Add(new PlayerCollider(OnPlayer));
			this.nodes = nodes;
			start = position;
			BirdEndPosition = nodes[nodes.Length - 1];
		}

		public FlingBirdIntro(EntityData data, Vector2 levelOffset)
			: this(data.Position + levelOffset, data.NodesOffset(levelOffset), data.Bool("crashes"))
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (!crashes && (scene as Level).Session.GetFlag("MissTheBird"))
			{
				RemoveSelf();
				return;
			}
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null && player.X > base.X)
			{
				if (crashes)
				{
					CS10_CatchTheBird.HandlePostCutsceneSpawn(this, scene as Level);
				}
				CassetteBlockManager cbm = base.Scene.Tracker.GetEntity<CassetteBlockManager>();
				if (cbm != null)
				{
					cbm.StopBlocks();
					cbm.Finish();
				}
				RemoveSelf();
			}
			else
			{
				scene.Add(fakeRightWall = new InvisibleBarrier(new Vector2(base.X + 160f, base.Y - 200f), 8f, 400f));
			}
			if (!crashes)
			{
				Vector2 start = Position;
				Position = new Vector2(base.X - 150f, (scene as Level).Bounds.Top - 8);
				Add(flyToRoutine = new Coroutine(FlyTo(start)));
			}
		}

		private IEnumerator FlyTo(Vector2 to)
		{
			Add(new SoundSource().Play("event:/new_content/game/10_farewell/bird_flappyscene_entry"));
			Sprite.Play("fly");
			Vector2 from = Position;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime * 0.3f)
			{
				Position = from + (to - from) * Ease.SineOut(p);
				yield return null;
			}
			Sprite.Play("hover");
			float sine = 0f;
			while (true)
			{
				Position = to + Vector2.UnitY * (float)Math.Sin(sine) * 8f;
				sine += Engine.DeltaTime * 2f;
				yield return null;
			}
		}

		public override void Removed(Scene scene)
		{
			if (fakeRightWall != null)
			{
				fakeRightWall.RemoveSelf();
			}
			fakeRightWall = null;
			base.Removed(scene);
		}

		private void OnPlayer(Player player)
		{
			if (player.Dead || startedRoutine)
			{
				return;
			}
			if (flyToRoutine != null)
			{
				flyToRoutine.RemoveSelf();
			}
			startedRoutine = true;
			player.Speed = Vector2.Zero;
			base.Depth = player.Depth - 5;
			Sprite.Play("hoverStressed");
			Sprite.Scale.X = 1f;
			fakeRightWall.RemoveSelf();
			fakeRightWall = null;
			if (!crashes)
			{
				base.Scene.Add(new CS10_MissTheBird(player, this));
				return;
			}
			CassetteBlockManager cbm = base.Scene.Tracker.GetEntity<CassetteBlockManager>();
			if (cbm != null)
			{
				cbm.StopBlocks();
				cbm.Finish();
			}
			base.Scene.Add(new CS10_CatchTheBird(player, this));
		}

		public override void Update()
		{
			if (!startedRoutine && fakeRightWall != null)
			{
				Level level = base.Scene as Level;
				if (level.Camera.X > fakeRightWall.X - 320f - 16f)
				{
					level.Camera.X = fakeRightWall.X - 320f - 16f;
				}
			}
			if (emitParticles && base.Scene.OnInterval(0.1f))
			{
				SceneAs<Level>().ParticlesBG.Emit(FlingBird.P_Feather, 1, Position + new Vector2(0f, -8f), new Vector2(6f, 4f));
			}
			base.Update();
		}

		public IEnumerator DoGrabbingRoutine(Player player)
		{
			Level level = base.Scene as Level;
			inCutscene = true;
			if (!crashes)
			{
				CrashSfxEmitter = SoundEmitter.Play("event:/new_content/game/10_farewell/bird_flappyscene", this);
			}
			else
			{
				CrashSfxEmitter = SoundEmitter.Play("event:/new_content/game/10_farewell/bird_crashscene_start", this);
			}
			player.StateMachine.State = 11;
			player.DummyGravity = false;
			player.DummyAutoAnimate = false;
			player.ForceCameraUpdate = true;
			player.Sprite.Play("jumpSlow_carry");
			player.Speed = Vector2.Zero;
			player.Facing = Facings.Right;
			Celeste.Freeze(0.1f);
			level.Shake();
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
			emitParticles = true;
			Add(new Coroutine(level.ZoomTo(new Vector2(140f, 120f), 1.5f, 4f)));
			float sin = 0f;
			for (int index = 0; index < nodes.Length - 1; index++)
			{
				Vector2 from = Position;
				Vector2 to = nodes[index];
				SimpleCurve curve = new SimpleCurve(from, to, from + (to - from) * 0.5f + new Vector2(0f, -24f));
				float length = curve.GetLengthParametric(32);
				float duration2 = length / 100f;
				if (to.Y < from.Y)
				{
					duration2 *= 1.1f;
					Sprite.Rate = 2f;
				}
				else
				{
					duration2 *= 0.8f;
					Sprite.Rate = 1f;
				}
				if (!crashes)
				{
					if (index == 0)
					{
						duration2 = 0.7f;
					}
					if (index == 1)
					{
						duration2 += 0.191f;
					}
					if (index == 2)
					{
						duration2 += 0.191f;
					}
				}
				for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration2)
				{
					sin += Engine.DeltaTime * 10f;
					Position = (curve.GetPoint(p) + Vector2.UnitY * (float)Math.Sin(sin) * 8f).Floor();
					player.Position = Position + new Vector2(2f, 10f);
					switch (Sprite.CurrentAnimationFrame)
					{
					case 1:
						player.Position += new Vector2(1f, -1f);
						break;
					case 2:
						player.Position += new Vector2(-1f, 0f);
						break;
					case 3:
						player.Position += new Vector2(-1f, 1f);
						break;
					case 4:
						player.Position += new Vector2(1f, 3f);
						break;
					case 5:
						player.Position += new Vector2(2f, 5f);
						break;
					}
					yield return null;
				}
				level.Shake();
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
			}
			Sprite.Rate = 1f;
			Celeste.Freeze(0.05f);
			yield return null;
			level.Shake();
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
			level.Flash(Color.White);
			emitParticles = false;
			inCutscene = false;
		}
	}
}
