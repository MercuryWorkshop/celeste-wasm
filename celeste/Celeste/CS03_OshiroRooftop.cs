using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS03_OshiroRooftop : CutsceneEntity
	{
		public const string Flag = "oshiro_resort_roof";

		private const float playerEndPosition = 170f;

		private Player player;

		private NPC oshiro;

		private BadelineDummy evil;

		private Vector2 bossSpawnPosition;

		private float anxiety;

		private float anxietyFlicker;

		private Sprite bossSprite = GFX.SpriteBank.Create("oshiro_boss");

		private float bossSpriteOffset;

		private bool oshiroRumble;

		public CS03_OshiroRooftop(NPC oshiro)
		{
			this.oshiro = oshiro;
		}

		public override void OnBegin(Level level)
		{
			bossSpawnPosition = new Vector2(oshiro.X, level.Bounds.Bottom - 40);
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			while (player == null)
			{
				player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null)
				{
					break;
				}
				yield return null;
			}
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			while (!player.OnGround() || player.Speed.Y < 0f)
			{
				yield return null;
			}
			yield return 0.6f;
			evil = new BadelineDummy(new Vector2(oshiro.X - 40f, level.Bounds.Bottom - 60));
			evil.Sprite.Scale.X = 1f;
			evil.Appear(level);
			level.Add(evil);
			yield return 0.1f;
			player.Facing = Facings.Left;
			yield return Textbox.Say("CH3_OSHIRO_START_CHASE", MaddyWalkAway, MaddyTurnAround, EnterOshiro, OshiroGetsAngry);
			yield return OshiroTransform();
			Add(new Coroutine(AnxietyAndCameraOut()));
			yield return level.ZoomBack(0.5f);
			yield return 0.25f;
			EndCutscene(level);
		}

		private IEnumerator MaddyWalkAway()
		{
			Level level = base.Scene as Level;
			Add(new Coroutine(player.DummyWalkTo((float)level.Bounds.Left + 170f)));
			yield return 0.2f;
			Audio.Play("event:/game/03_resort/suite_bad_moveroof", evil.Position);
			Add(new Coroutine(evil.FloatTo(evil.Position + new Vector2(80f, 30f))));
			yield return null;
		}

		private IEnumerator MaddyTurnAround()
		{
			yield return 0.25f;
			player.Facing = Facings.Left;
			yield return 0.1f;
			Level level = SceneAs<Level>();
			yield return level.ZoomTo(new Vector2(150f, bossSpawnPosition.Y - (float)level.Bounds.Y - 8f), 2f, 0.5f);
		}

		private IEnumerator EnterOshiro()
		{
			yield return 0.3f;
			bossSpriteOffset = (bossSprite.Justify.Value.Y - oshiro.Sprite.Justify.Value.Y) * bossSprite.Height;
			oshiro.Visible = true;
			oshiro.Sprite.Scale.X = 1f;
			Add(new Coroutine(oshiro.MoveTo(bossSpawnPosition - new Vector2(0f, bossSpriteOffset))));
			oshiro.Add(new SoundSource("event:/char/oshiro/move_07_roof00_enter"));
			float from = Level.ZoomFocusPoint.X;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.7f)
			{
				Level.ZoomFocusPoint.X = from + (126f - from) * Ease.CubeInOut(p);
				yield return null;
			}
			yield return 0.3f;
			player.Facing = Facings.Left;
			yield return 0.1f;
			evil.Sprite.Scale.X = -1f;
		}

		private IEnumerator OshiroGetsAngry()
		{
			yield return 0.1f;
			evil.Vanish();
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			evil = null;
			yield return 0.8f;
			Audio.Play("event:/char/oshiro/boss_transform_begin", oshiro.Position);
			oshiro.Remove(oshiro.Sprite);
			oshiro.Sprite = bossSprite;
			oshiro.Sprite.Play("transformStart");
			oshiro.Y += bossSpriteOffset;
			oshiro.Add(oshiro.Sprite);
			oshiro.Depth = -12500;
			oshiroRumble = true;
			yield return 1f;
		}

		private IEnumerator OshiroTransform()
		{
			yield return 0.2f;
			Audio.Play("event:/char/oshiro/boss_transform_burst", oshiro.Position);
			oshiro.Sprite.Play("transformFinish");
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
			SceneAs<Level>().Shake(0.5f);
			SetChaseMusic();
			while (anxiety < 0.5f)
			{
				anxiety = Calc.Approach(anxiety, 0.5f, Engine.DeltaTime * 0.5f);
				yield return null;
			}
			yield return 0.25f;
		}

		private IEnumerator AnxietyAndCameraOut()
		{
			Level level = base.Scene as Level;
			Vector2 from = level.Camera.Position;
			Vector2 to = player.CameraTarget;
			for (float t = 0f; t < 1f; t += Engine.DeltaTime * 2f)
			{
				anxiety = Calc.Approach(anxiety, 0f, Engine.DeltaTime * 4f);
				level.Camera.Position = from + (to - from) * Ease.CubeInOut(t);
				yield return null;
			}
		}

		private void SetChaseMusic()
		{
			Level obj = base.Scene as Level;
			obj.Session.Audio.Music.Event = "event:/music/lvl3/oshiro_chase";
			obj.Session.Audio.Apply();
		}

		public override void OnEnd(Level level)
		{
			Distort.Anxiety = (anxiety = (anxietyFlicker = 0f));
			if (evil != null)
			{
				level.Remove(evil);
			}
			player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				player.StateMachine.Locked = false;
				player.StateMachine.State = 0;
				player.X = (float)level.Bounds.Left + 170f;
				player.Speed.Y = 0f;
				while (player.CollideCheck<Solid>())
				{
					player.Y--;
				}
				level.Camera.Position = player.CameraTarget;
			}
			if (WasSkipped)
			{
				SetChaseMusic();
			}
			oshiro.RemoveSelf();
			base.Scene.Add(new AngryOshiro(bossSpawnPosition, fromCutscene: true));
			level.Session.RespawnPoint = new Vector2((float)level.Bounds.Left + 170f, level.Bounds.Top + 160);
			level.Session.SetFlag("oshiro_resort_roof");
		}

		public override void Update()
		{
			Distort.Anxiety = anxiety + anxiety * anxietyFlicker;
			if (base.Scene.OnInterval(0.05f))
			{
				anxietyFlicker = -0.2f + Calc.Random.NextFloat(0.4f);
			}
			base.Update();
			if (oshiroRumble)
			{
				Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
			}
		}
	}
}
