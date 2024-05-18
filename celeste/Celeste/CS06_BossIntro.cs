using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS06_BossIntro : CutsceneEntity
	{
		public const string Flag = "boss_intro";

		private Player player;

		private FinalBoss boss;

		private Vector2 bossEndPosition;

		private BadelineAutoAnimator animator;

		private float playerTargetX;

		public CS06_BossIntro(float playerTargetX, Player player, FinalBoss boss)
		{
			this.player = player;
			this.boss = boss;
			this.playerTargetX = playerTargetX;
			bossEndPosition = boss.Position + new Vector2(0f, -16f);
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			while (!player.Dead && !player.OnGround())
			{
				yield return null;
			}
			while (player.Dead)
			{
				yield return null;
			}
			player.Facing = Facings.Right;
			Add(new Coroutine(CutsceneEntity.CameraTo(new Vector2((player.X + boss.X) / 2f - 160f, level.Bounds.Bottom - 180), 1f)));
			yield return 0.5f;
			if (!player.Dead)
			{
				yield return player.DummyWalkToExact((int)(playerTargetX - 8f));
			}
			player.Facing = Facings.Right;
			yield return Textbox.Say("ch6_boss_start", BadelineFloat, PlayerStepForward);
			yield return level.ZoomBack(0.5f);
			EndCutscene(level);
		}

		private IEnumerator BadelineFloat()
		{
			Add(new Coroutine(Level.ZoomTo(new Vector2(170f, 110f), 2f, 1f)));
			Audio.Play("event:/char/badeline/boss_prefight_getup", boss.Position);
			boss.Sitting = false;
			boss.NormalSprite.Play("fallSlow");
			boss.NormalSprite.Scale.X = -1f;
			boss.Add(animator = new BadelineAutoAnimator());
			float fromY = boss.Y;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime * 4f)
			{
				boss.Position.Y = MathHelper.Lerp(fromY, bossEndPosition.Y, Ease.CubeInOut(p));
				yield return null;
			}
		}

		private IEnumerator PlayerStepForward()
		{
			yield return player.DummyWalkToExact((int)player.X + 8);
		}

		public override void OnEnd(Level level)
		{
			if (WasSkipped && player != null)
			{
				player.X = playerTargetX;
				while (!player.OnGround() && player.Y < (float)level.Bounds.Bottom)
				{
					player.Y++;
				}
			}
			player.StateMachine.Locked = false;
			player.StateMachine.State = 0;
			boss.Position = bossEndPosition;
			if (boss.NormalSprite != null)
			{
				boss.NormalSprite.Scale.X = -1f;
				boss.NormalSprite.Play("laugh");
			}
			boss.Sitting = false;
			if (animator != null)
			{
				boss.Remove(animator);
			}
			level.Session.SetFlag("boss_intro");
		}
	}
}
