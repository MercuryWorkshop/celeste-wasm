using System;
using System.Collections;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS03_Ending : CutsceneEntity
	{
		private class BgFlash : Entity
		{
			public float Alpha;

			public BgFlash()
			{
				base.Depth = 10100;
			}

			public override void Render()
			{
				Camera camera = (base.Scene as Level).Camera;
				Draw.Rect(camera.X - 10f, camera.Y - 10f, 340f, 200f, Color.Black * Alpha);
			}
		}

		public const string Flag = "oshiroEnding";

		private ResortRoofEnding roof;

		private AngryOshiro angryOshiro;

		private Player player;

		private Entity oshiro;

		private Sprite oshiroSprite;

		private EventInstance smashSfx;

		private bool smashRumble;

		public CS03_Ending(ResortRoofEnding roof, Player player)
			: base(fadeInOnSkip: false, endingChapterAfter: true)
		{
			this.roof = roof;
			this.player = player;
			base.Depth = -1000000;
		}

		public override void OnBegin(Level level)
		{
			level.RegisterAreaComplete();
			Add(new Coroutine(Cutscene(level)));
		}

		public override void Update()
		{
			base.Update();
			if (smashRumble)
			{
				Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
			}
		}

		private IEnumerator Cutscene(Level level)
		{
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			player.ForceCameraUpdate = false;
			Add(new Coroutine(player.DummyRunTo(roof.X + roof.Width - 32f, fastAnim: true)));
			yield return null;
			player.DummyAutoAnimate = false;
			yield return 0.5f;
			angryOshiro = base.Scene.Entities.FindFirst<AngryOshiro>();
			Add(new Coroutine(MoveGhostTo(new Vector2(roof.X + 40f, roof.Y - 12f))));
			yield return 1f;
			player.DummyAutoAnimate = true;
			yield return level.ZoomTo(new Vector2(130f, 60f), 2f, 0.5f);
			player.Facing = Facings.Left;
			yield return 0.5f;
			yield return Textbox.Say("CH3_OSHIRO_CHASE_END", GhostSmash);
			yield return GhostSmash(0.5f, final: true);
			Audio.SetMusic(null);
			oshiroSprite = null;
			BgFlash bgFlash = new BgFlash
			{
				Alpha = 1f
			};
			level.Add(bgFlash);
			Distort.GameRate = 0f;
			Sprite lightning = GFX.SpriteBank.Create("oshiro_boss_lightning");
			lightning.Position = angryOshiro.Position + new Vector2(140f, -100f);
			lightning.Rotation = Calc.Angle(lightning.Position, angryOshiro.Position + new Vector2(0f, 10f));
			lightning.Play("once");
			Add(lightning);
			yield return null;
			Celeste.Freeze(0.3f);
			yield return null;
			level.Shake();
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
			smashRumble = false;
			yield return 0.2f;
			Distort.GameRate = 1f;
			level.Flash(Color.White);
			player.DummyGravity = false;
			angryOshiro.Sprite.Play("transformBack");
			player.Sprite.Play("fall");
			roof.BeginFalling = true;
			yield return null;
			Engine.TimeRate = 0.01f;
			player.Sprite.Play("fallFast");
			player.DummyGravity = true;
			player.Speed.Y = -200f;
			player.Speed.X = 300f;
			Vector2 oshiroFallSpeed = new Vector2(-100f, -250f);
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 1.5f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				angryOshiro.Sprite.Rotation = t.Eased * -100f * ((float)Math.PI / 180f);
			};
			Add(tween);
			float t3;
			for (t3 = 0f; t3 < 2f; t3 += Engine.DeltaTime)
			{
				oshiroFallSpeed.X = Calc.Approach(oshiroFallSpeed.X, 0f, Engine.DeltaTime * 400f);
				oshiroFallSpeed.Y += Engine.DeltaTime * 800f;
				angryOshiro.Position += oshiroFallSpeed * Engine.DeltaTime;
				bgFlash.Alpha = Calc.Approach(bgFlash.Alpha, 0f, Engine.RawDeltaTime);
				Engine.TimeRate = Calc.Approach(Engine.TimeRate, 1f, Engine.RawDeltaTime * 0.6f);
				yield return null;
			}
			level.DirectionalShake(new Vector2(0f, -1f), 0.5f);
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Long);
			yield return 1f;
			while (!player.OnGround())
			{
				player.MoveV(1f);
			}
			player.DummyAutoAnimate = false;
			player.Sprite.Play("tired");
			angryOshiro.RemoveSelf();
			base.Scene.Add(oshiro = new Entity(new Vector2(level.Bounds.Left + 110, player.Y)));
			oshiro.Add(oshiroSprite = GFX.SpriteBank.Create("oshiro"));
			oshiroSprite.Play("fall");
			oshiroSprite.Scale.X = 1f;
			oshiro.Collider = new Hitbox(8f, 8f, -4f, -8f);
			oshiro.Add(new VertexLight(new Vector2(0f, -8f), Color.White, 1f, 16, 32));
			yield return CutsceneEntity.CameraTo(player.CameraTarget + new Vector2(0f, 40f), 1f, Ease.CubeOut);
			yield return 1.5f;
			Audio.SetMusic("event:/music/lvl3/intro");
			yield return 3f;
			Audio.Play("event:/char/oshiro/chat_get_up", oshiro.Position);
			oshiroSprite.Play("recover");
			float target = oshiro.Y + 4f;
			while (oshiro.Y != target)
			{
				oshiro.Y = Calc.Approach(oshiro.Y, target, 6f * Engine.DeltaTime);
				yield return null;
			}
			yield return 0.6f;
			yield return Textbox.Say("CH3_ENDING", OshiroTurns);
			Add(new Coroutine(CutsceneEntity.CameraTo(level.Camera.Position + new Vector2(-80f, 0f), 3f)));
			yield return 0.5f;
			oshiroSprite.Scale.X = -1f;
			yield return 0.2f;
			t3 = 0f;
			oshiro.Add(new SoundSource("event:/char/oshiro/move_08_roof07_exit"));
			while (oshiro.X > (float)(level.Bounds.Left - 16))
			{
				oshiro.X -= 40f * Engine.DeltaTime;
				Sprite sprite = oshiroSprite;
				float num;
				t3 = (num = t3 + Engine.DeltaTime * 2f);
				sprite.Y = (float)Math.Sin(num) * 2f;
				oshiro.CollideFirst<Door>()?.Open(oshiro.X);
				yield return null;
			}
			Add(new Coroutine(CutsceneEntity.CameraTo(level.Camera.Position + new Vector2(80f, 0f), 2f)));
			yield return 1.2f;
			player.DummyAutoAnimate = true;
			yield return player.DummyWalkTo(player.X - 16f);
			yield return 2f;
			player.Facing = Facings.Right;
			yield return 1f;
			player.ForceCameraUpdate = false;
			player.Add(new Coroutine(RunPlayerRight()));
			EndCutscene(level);
		}

		private IEnumerator OshiroTurns()
		{
			yield return 1f;
			oshiroSprite.Scale.X = -1f;
			yield return 0.2f;
		}

		private IEnumerator MoveGhostTo(Vector2 target)
		{
			if (angryOshiro != null)
			{
				target.Y -= angryOshiro.Height / 2f;
				angryOshiro.EnterDummyMode();
				angryOshiro.Collidable = false;
				while (angryOshiro.Position != target)
				{
					angryOshiro.Position = Calc.Approach(angryOshiro.Position, target, 64f * Engine.DeltaTime);
					yield return null;
				}
			}
		}

		private IEnumerator GhostSmash()
		{
			yield return GhostSmash(0f, final: false);
		}

		private IEnumerator GhostSmash(float topDelay, bool final)
		{
			if (angryOshiro == null)
			{
				yield break;
			}
			if (final)
			{
				smashSfx = Audio.Play("event:/char/oshiro/boss_slam_final", angryOshiro.Position);
			}
			else
			{
				smashSfx = Audio.Play("event:/char/oshiro/boss_slam_first", angryOshiro.Position);
			}
			float from = angryOshiro.Y;
			float to = angryOshiro.Y - 32f;
			for (float p3 = 0f; p3 < 1f; p3 += Engine.DeltaTime * 2f)
			{
				angryOshiro.Y = MathHelper.Lerp(from, to, Ease.CubeOut(p3));
				yield return null;
			}
			yield return topDelay;
			float ground = from + 20f;
			for (float p3 = 0f; p3 < 1f; p3 += Engine.DeltaTime * 8f)
			{
				angryOshiro.Y = MathHelper.Lerp(to, ground, Ease.CubeOut(p3));
				yield return null;
			}
			angryOshiro.Squish();
			Level.Shake(0.5f);
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			smashRumble = true;
			roof.StartShaking(0.5f);
			if (!final)
			{
				for (float p3 = 0f; p3 < 1f; p3 += Engine.DeltaTime * 16f)
				{
					angryOshiro.Y = MathHelper.Lerp(ground, from, Ease.CubeOut(p3));
					yield return null;
				}
			}
			else
			{
				angryOshiro.Y = (ground + from) / 2f;
			}
			if (angryOshiro != null)
			{
				player.DummyAutoAnimate = false;
				player.Sprite.Play("shaking");
				roof.Wobble(angryOshiro, final);
				if (!final)
				{
					yield return 0.5f;
				}
			}
		}

		private IEnumerator RunPlayerRight()
		{
			yield return 0.75f;
			yield return player.DummyRunTo(player.X + 128f);
		}

		public override void OnEnd(Level level)
		{
			Audio.SetMusic("event:/music/lvl3/intro");
			Audio.Stop(smashSfx);
			level.CompleteArea();
			SpotlightWipe.FocusPoint = new Vector2(192f, 120f);
		}
	}
}
