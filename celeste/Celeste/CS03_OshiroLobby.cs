using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS03_OshiroLobby : CutsceneEntity
	{
		public const string Flag = "oshiro_resort_talked_1";

		private Player player;

		private NPC oshiro;

		private float startLightAlpha;

		private bool createSparks;

		private SoundSource sfx = new SoundSource();

		public CS03_OshiroLobby(Player player, NPC oshiro)
		{
			this.player = player;
			this.oshiro = oshiro;
			Add(sfx);
		}

		public override void Update()
		{
			base.Update();
			if (createSparks && Level.OnInterval(0.025f))
			{
				Vector2 at = oshiro.Position + new Vector2(0f, -12f) + new Vector2(Calc.Random.Range(4, 12) * Calc.Random.Choose(1, -1), Calc.Random.Range(4, 12) * Calc.Random.Choose(1, -1));
				Level.Particles.Emit(NPC03_Oshiro_Lobby.P_AppearSpark, at, (at - oshiro.Position).Angle());
			}
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			startLightAlpha = level.Lighting.Alpha;
			float endLightAlpha = 1f;
			float from = oshiro.Y;
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			yield return 0.5f;
			yield return player.DummyWalkTo(oshiro.X - 16f);
			player.Facing = Facings.Right;
			sfx.Play("event:/game/03_resort/sequence_oshiro_intro");
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			yield return 1.4f;
			level.Shake();
			level.Lighting.Alpha += 0.5f;
			while (level.Lighting.Alpha > startLightAlpha)
			{
				level.Lighting.Alpha -= Engine.DeltaTime * 4f;
				yield return null;
			}
			VertexLight light = new VertexLight(new Vector2(0f, -8f), Color.White, 1f, 32, 64);
			BloomPoint bloom = new BloomPoint(new Vector2(0f, -8f), 1f, 16f);
			level.Lighting.SetSpotlight(light);
			oshiro.Add(light);
			oshiro.Add(bloom);
			oshiro.Y -= 16f;
			Vector2 target = light.Position;
			Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.5f, start: true);
			tween2.OnUpdate = delegate(Tween t)
			{
				light.Alpha = (bloom.Alpha = t.Percent);
				light.Position = Vector2.Lerp(target - Vector2.UnitY * 48f, target, t.Percent);
				level.Lighting.Alpha = MathHelper.Lerp(startLightAlpha, endLightAlpha, t.Eased);
			};
			Add(tween2);
			yield return tween2.Wait();
			yield return 0.2f;
			yield return level.ZoomTo(new Vector2(170f, 126f), 2f, 0.5f);
			yield return 0.6f;
			level.Shake();
			oshiro.Sprite.Visible = true;
			oshiro.Sprite.Play("appear");
			yield return player.DummyWalkToExact((int)(player.X - 12f), walkBackwards: true);
			player.DummyAutoAnimate = false;
			player.Sprite.Play("shaking");
			Input.Rumble(RumbleStrength.Medium, RumbleLength.FullSecond);
			yield return 0.6f;
			createSparks = true;
			yield return 0.4f;
			createSparks = false;
			yield return 0.2f;
			level.Shake();
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			yield return 1.4f;
			level.Lighting.UnsetSpotlight();
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 0.5f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				level.Lighting.Alpha = MathHelper.Lerp(endLightAlpha, startLightAlpha, t.Percent);
				bloom.Alpha = 1f - t.Percent;
			};
			Add(tween);
			while (oshiro.Y != from)
			{
				oshiro.Y = Calc.Approach(oshiro.Y, from, Engine.DeltaTime * 40f);
				yield return null;
			}
			yield return tween.Wait();
			Audio.SetMusic("event:/music/lvl3/oshiro_theme");
			player.DummyAutoAnimate = true;
			yield return Textbox.Say("CH3_OSHIRO_FRONT_DESK", ZoomOut);
			foreach (MrOshiroDoor item in base.Scene.Entities.FindAll<MrOshiroDoor>())
			{
				item.Open();
			}
			oshiro.MoveToAndRemove(new Vector2(level.Bounds.Right + 64, oshiro.Y));
			oshiro.Add(new SoundSource("event:/char/oshiro/move_01_0xa_exit"));
			yield return 1.5f;
			EndCutscene(level);
		}

		private IEnumerator ZoomOut()
		{
			yield return 0.2f;
			yield return Level.ZoomBack(0.5f);
			yield return 0.2f;
		}

		public override void OnEnd(Level level)
		{
			player.StateMachine.Locked = false;
			player.StateMachine.State = 0;
			if (WasSkipped)
			{
				foreach (MrOshiroDoor item in base.Scene.Entities.FindAll<MrOshiroDoor>())
				{
					item.InstantOpen();
				}
			}
			level.Lighting.Alpha = startLightAlpha;
			level.Lighting.UnsetSpotlight();
			level.Session.SetFlag("oshiro_resort_talked_1");
			level.Session.Audio.Music.Event = "event:/music/lvl3/explore";
			level.Session.Audio.Music.Progress = 1;
			level.Session.Audio.Apply();
			if (WasSkipped)
			{
				level.Remove(oshiro);
			}
		}
	}
}
