using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS03_OshiroBreakdown : CutsceneEntity
	{
		public const string Flag = "oshiro_breakdown";

		private const int PlayerWalkTo = 200;

		private List<DustStaticSpinner> creatures = new List<DustStaticSpinner>();

		private List<Vector2> creatureHomes = new List<Vector2>();

		private NPC oshiro;

		private Player player;

		private Vector2 origin;

		private const int DustAmountA = 4;

		public CS03_OshiroBreakdown(Player player, NPC oshiro)
		{
			this.oshiro = oshiro;
			this.player = player;
			origin = oshiro.Position;
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			Add(new Coroutine(player.DummyWalkTo(player.X - 64f)));
			List<DustStaticSpinner> list = level.Entities.FindAll<DustStaticSpinner>();
			list.Shuffle();
			foreach (DustStaticSpinner dust in list)
			{
				if ((dust.Position - oshiro.Position).Length() < 128f)
				{
					creatures.Add(dust);
					creatureHomes.Add(dust.Position);
					dust.Visible = false;
				}
			}
			yield return PanCamera(level.Bounds.Left);
			yield return 0.2f;
			yield return Level.ZoomTo(new Vector2(100f, 120f), 2f, 0.5f);
			yield return Textbox.Say("CH3_OSHIRO_BREAKDOWN", WalkLeft, WalkRight, CreateDustA, CreateDustB);
			Add(new Coroutine(oshiro.MoveTo(new Vector2(level.Bounds.Left - 64, oshiro.Y))));
			oshiro.Add(new SoundSource("event:/char/oshiro/move_06_04d_exit"));
			yield return 0.25f;
			yield return PanCamera(player.CameraTarget.X);
			EndCutscene(level);
		}

		private IEnumerator PanCamera(float to)
		{
			float from = Level.Camera.X;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime)
			{
				Level.Camera.X = from + (to - from) * Ease.CubeInOut(p);
				yield return null;
			}
		}

		private IEnumerator WalkLeft()
		{
			(oshiro.Sprite as OshiroSprite).AllowSpriteChanges = false;
			yield return oshiro.MoveTo(origin + new Vector2(-24f, 0f));
			(oshiro.Sprite as OshiroSprite).AllowSpriteChanges = true;
		}

		private IEnumerator WalkRight()
		{
			(oshiro.Sprite as OshiroSprite).AllowSpriteChanges = false;
			yield return oshiro.MoveTo(origin + new Vector2(0f, 0f));
			(oshiro.Sprite as OshiroSprite).AllowSpriteChanges = true;
		}

		private IEnumerator CreateDustA()
		{
			Add(new SoundSource(oshiro.Position, "event:/game/03_resort/sequence_oshirofluff_pt1"));
			(oshiro.Sprite as OshiroSprite).AllowSpriteChanges = false;
			oshiro.Sprite.Play("fall");
			Audio.Play("event:/char/oshiro/chat_collapse", oshiro.Position);
			Distort.AnxietyOrigin = new Vector2(0.5f, 0.5f);
			for (int i = 0; i < 4; i++)
			{
				Add(new Coroutine(MoveDust(creatures[i], creatureHomes[i])));
				Distort.Anxiety = 0.1f + Calc.Random.NextFloat(0.1f);
				if (i % 4 == 0)
				{
					Distort.Anxiety = 0.1f + Calc.Random.NextFloat(0.1f);
					Level.Shake();
					Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
					yield return 0.4f;
				}
				else
				{
					yield return 0.1f;
				}
			}
			yield return 0.5f;
		}

		private IEnumerator CreateDustB()
		{
			Add(new SoundSource(oshiro.Position, "event:/game/03_resort/sequence_oshirofluff_pt2"));
			for (int i = 4; i < creatures.Count; i++)
			{
				Add(new Coroutine(MoveDust(creatures[i], creatureHomes[i])));
				Distort.Anxiety = 0.1f + Calc.Random.NextFloat(0.1f);
				Level.Shake();
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
				if ((i - 4) % 4 == 0)
				{
					Distort.Anxiety = 0.1f + Calc.Random.NextFloat(0.1f);
					yield return 0.4f;
				}
				else
				{
					yield return 0.1f;
				}
			}
			yield return 1f;
			while (Distort.Anxiety > 0f)
			{
				Distort.Anxiety -= Engine.DeltaTime;
				yield return null;
			}
			yield return Level.ZoomBack(0.5f);
			yield return player.DummyWalkToExact(Level.Bounds.Left + 200);
			yield return 1f;
			Audio.Play("event:/char/oshiro/chat_get_up", oshiro.Position);
			oshiro.Sprite.Play("recover");
			yield return 0.7f;
			oshiro.Sprite.Scale.X = 1f;
			yield return 0.5f;
		}

		private IEnumerator MoveDust(DustStaticSpinner creature, Vector2 to)
		{
			Vector2 from = oshiro.Position + new Vector2(0f, -12f);
			SimpleCurve curve = new SimpleCurve(from, to, (to + from) / 2f + Vector2.UnitY * (-30f + Calc.Random.NextFloat(60f)));
			for (float p = 0f; p < 1f; p += Engine.DeltaTime)
			{
				yield return null;
				creature.Sprite.Scale = 0.5f + p * 0.5f;
				creature.Position = curve.GetPoint(Ease.CubeOut(p));
				creature.Visible = true;
				if (base.Scene.OnInterval(0.02f))
				{
					SceneAs<Level>().ParticlesBG.Emit(DustStaticSpinner.P_Move, 1, creature.Position, Vector2.One * 4f);
				}
			}
		}

		public override void OnEnd(Level level)
		{
			player.StateMachine.Locked = false;
			player.StateMachine.State = 0;
			if (WasSkipped)
			{
				player.X = level.Bounds.Left + 200;
				while (!player.OnGround())
				{
					player.Y++;
				}
				for (int i = 0; i < creatures.Count; i++)
				{
					creatures[i].ForceInstantiate();
					creatures[i].Visible = true;
					creatures[i].Position = creatureHomes[i];
				}
			}
			level.Camera.Position = player.CameraTarget;
			level.Remove(oshiro);
			level.Session.SetFlag("oshiro_breakdown");
		}
	}
}
