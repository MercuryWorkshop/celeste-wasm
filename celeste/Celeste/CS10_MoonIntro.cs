using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS10_MoonIntro : CutsceneEntity
	{
		public const string Flag = "moon_intro";

		private Player player;

		private BadelineDummy badeline;

		private BirdNPC bird;

		private float fade = 1f;

		private float targetX;

		public CS10_MoonIntro(Player player)
		{
			base.Depth = -8500;
			this.player = player;
			targetX = player.CameraTarget.X + 8f;
		}

		public override void OnBegin(Level level)
		{
			bird = base.Scene.Entities.FindFirst<BirdNPC>();
			player.StateMachine.State = 11;
			if (level.Wipe != null)
			{
				level.Wipe.Cancel();
			}
			level.Wipe = new FadeWipe(level, wipeIn: true);
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			player.StateMachine.State = 11;
			player.Visible = false;
			player.Active = false;
			player.Dashes = 2;
			for (float t = 0f; t < 1f; t += Engine.DeltaTime / 0.9f)
			{
				level.Wipe.Percent = 0f;
				yield return null;
			}
			Add(new Coroutine(FadeIn(5f)));
			level.Camera.Position = level.LevelOffset + new Vector2(-100f, 0f);
			yield return CutsceneEntity.CameraTo(new Vector2(targetX, level.Camera.Y), 6f, Ease.SineOut);
			level.Camera.Position = new Vector2(targetX, level.Camera.Y);
			if (bird != null)
			{
				yield return bird.StartleAndFlyAway();
				level.Session.DoNotLoad.Add(bird.EntityID);
				bird = null;
			}
			yield return 0.5f;
			player.Speed = Vector2.Zero;
			player.Position = level.GetSpawnPoint(player.Position);
			player.Active = true;
			player.StateMachine.State = 23;
			while (player.Top > (float)level.Bounds.Bottom)
			{
				yield return null;
			}
			yield return 0.2f;
			Audio.Play("event:/new_content/char/madeline/screenentry_lowgrav", player.Position);
			while (player.StateMachine.State == 23)
			{
				yield return null;
			}
			player.X = (int)player.X;
			player.Y = (int)player.Y;
			while (!player.OnGround() && player.Bottom < (float)level.Bounds.Bottom)
			{
				player.MoveVExact(16);
			}
			player.StateMachine.State = 11;
			yield return 0.5f;
			yield return BadelineAppears();
			yield return Textbox.Say("CH9_LANDING", BadelineTurns, BadelineVanishes);
			EndCutscene(level);
		}

		private IEnumerator BadelineTurns()
		{
			yield return 0.1f;
			int current = Math.Sign(badeline.Sprite.Scale.X);
			int target = current * -1;
			Wiggler wiggle = Wiggler.Create(0.5f, 3f, delegate(float v)
			{
				badeline.Sprite.Scale = new Vector2(target, 1f) * (1f + 0.2f * v);
			}, start: true, removeSelfOnFinish: true);
			Add(wiggle);
			Audio.Play((target < 0) ? "event:/char/badeline/jump_wall_left" : "event:/char/badeline/jump_wall_left", badeline.Position);
			yield return 0.6f;
		}

		private IEnumerator BadelineAppears()
		{
			Level.Session.Inventory.Dashes = 1;
			player.Dashes = 1;
			Level.Add(badeline = new BadelineDummy(player.Position));
			Level.Displacement.AddBurst(player.Center, 0.5f, 8f, 32f, 0.5f);
			Audio.Play("event:/char/badeline/maddy_split", player.Position);
			badeline.Sprite.Scale.X = 1f;
			yield return badeline.FloatTo(player.Position + new Vector2(-16f, -16f), 1, faceDirection: false);
			player.Facing = Facings.Left;
			yield return null;
		}

		private IEnumerator BadelineVanishes()
		{
			yield return 0.5f;
			badeline.Vanish();
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			badeline = null;
			yield return 0.8f;
			player.Facing = Facings.Right;
			yield return 0.6f;
		}

		private IEnumerator FadeIn(float duration)
		{
			while (fade > 0f)
			{
				fade = Calc.Approach(fade, 0f, Engine.DeltaTime / duration);
				yield return null;
			}
		}

		public override void OnEnd(Level level)
		{
			level.Session.Inventory.Dashes = 1;
			player.Dashes = 1;
			player.Depth = 0;
			player.Speed = Vector2.Zero;
			player.Position = level.GetSpawnPoint(player.Position) + new Vector2(0f, -32f);
			player.Active = true;
			player.Visible = true;
			player.StateMachine.State = 0;
			player.X = (int)player.X;
			player.Y = (int)player.Y;
			while (!player.OnGround() && player.Bottom < (float)level.Bounds.Bottom)
			{
				player.MoveVExact(16);
			}
			if (badeline != null)
			{
				badeline.RemoveSelf();
			}
			if (bird != null)
			{
				bird.RemoveSelf();
				level.Session.DoNotLoad.Add(bird.EntityID);
			}
			level.Camera.Position = new Vector2(targetX, level.Camera.Y);
			level.Session.SetFlag("moon_intro");
		}

		public override void Render()
		{
			Camera cam = (base.Scene as Level).Camera;
			Draw.Rect(cam.X - 10f, cam.Y - 10f, 340f, 200f, Color.Black * fade);
		}
	}
}
