using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS03_OshiroMasterSuite : CutsceneEntity
	{
		public const string Flag = "oshiro_resort_suite";

		private Player player;

		private NPC oshiro;

		private BadelineDummy evil;

		private ResortMirror mirror;

		public CS03_OshiroMasterSuite(NPC oshiro)
		{
			this.oshiro = oshiro;
		}

		public override void OnBegin(Level level)
		{
			mirror = base.Scene.Entities.FindFirst<ResortMirror>();
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			while (true)
			{
				player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null)
				{
					break;
				}
				yield return null;
			}
			Audio.SetMusic(null);
			yield return 0.4f;
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			Add(new Coroutine(player.DummyWalkTo(oshiro.X + 32f)));
			yield return 1f;
			Audio.SetMusic("event:/music/lvl3/oshiro_theme");
			yield return Textbox.Say("CH3_OSHIRO_SUITE", SuiteShadowAppear, SuiteShadowDisrupt, SuiteShadowCeiling, Wander, Console, JumpBack, Collapse, AwkwardPause);
			evil.Add(new SoundSource(Vector2.Zero, "event:/game/03_resort/suite_bad_exittop"));
			yield return evil.FloatTo(new Vector2(evil.X, level.Bounds.Top - 32));
			base.Scene.Remove(evil);
			while (level.Lighting.Alpha != level.BaseLightingAlpha)
			{
				level.Lighting.Alpha = Calc.Approach(level.Lighting.Alpha, level.BaseLightingAlpha, Engine.DeltaTime * 0.5f);
				yield return null;
			}
			EndCutscene(level);
		}

		private IEnumerator Wander()
		{
			yield return 0.5f;
			player.Facing = Facings.Right;
			yield return 0.1f;
			yield return player.DummyWalkToExact((int)oshiro.X + 48);
			yield return 1f;
			player.Facing = Facings.Left;
			yield return 0.2f;
			yield return player.DummyWalkToExact((int)oshiro.X - 32);
			yield return 0.1f;
			oshiro.Sprite.Scale.X = -1f;
			yield return 0.2f;
			player.DummyAutoAnimate = false;
			player.Sprite.Play("lookUp");
			yield return 1f;
			player.DummyAutoAnimate = true;
			yield return 0.4f;
			player.Facing = Facings.Right;
			yield return 0.2f;
			yield return player.DummyWalkToExact((int)oshiro.X - 24);
			yield return 0.5f;
			Level level = SceneAs<Level>();
			yield return level.ZoomTo(new Vector2(190f, 110f), 2f, 0.5f);
		}

		private IEnumerator AwkwardPause()
		{
			yield return 2f;
		}

		private IEnumerator SuiteShadowAppear()
		{
			if (mirror != null)
			{
				mirror.EvilAppear();
				SetMusic();
				Audio.Play("event:/game/03_resort/suite_bad_intro", mirror.Position);
				Vector2 from = Level.ZoomFocusPoint;
				Vector2 to = new Vector2(216f, 110f);
				for (float p = 0f; p < 1f; p += Engine.DeltaTime * 2f)
				{
					Level.ZoomFocusPoint = from + (to - from) * Ease.SineInOut(p);
					yield return null;
				}
				yield return null;
			}
		}

		private IEnumerator SuiteShadowDisrupt()
		{
			if (mirror != null)
			{
				Audio.Play("event:/game/03_resort/suite_bad_mirrorbreak", mirror.Position);
				yield return mirror.SmashRoutine();
				evil = new BadelineDummy(mirror.Position + new Vector2(0f, -8f));
				base.Scene.Add(evil);
				yield return 1.2f;
				oshiro.Sprite.Scale.X = 1f;
				yield return evil.FloatTo(oshiro.Position + new Vector2(32f, -24f));
			}
		}

		private IEnumerator Collapse()
		{
			oshiro.Sprite.Play("fall");
			Audio.Play("event:/char/oshiro/chat_collapse", oshiro.Position);
			yield return null;
		}

		private IEnumerator Console()
		{
			yield return player.DummyWalkToExact((int)oshiro.X - 16);
		}

		private IEnumerator JumpBack()
		{
			yield return player.DummyWalkToExact((int)oshiro.X - 24, walkBackwards: true);
			yield return 0.8f;
		}

		private IEnumerator SuiteShadowCeiling()
		{
			yield return SceneAs<Level>().ZoomBack(0.5f);
			evil.Add(new SoundSource(Vector2.Zero, "event:/game/03_resort/suite_bad_movestageleft"));
			yield return evil.FloatTo(new Vector2(Level.Bounds.Left + 96, evil.Y - 16f), 1);
			player.Facing = Facings.Left;
			yield return 0.25f;
			evil.Add(new SoundSource(Vector2.Zero, "event:/game/03_resort/suite_bad_ceilingbreak"));
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
			Level.DirectionalShake(-Vector2.UnitY);
			yield return evil.SmashBlock(evil.Position + new Vector2(0f, -32f));
			yield return 0.8f;
		}

		private void SetMusic()
		{
			if (Level.Session.Area.Mode == AreaMode.Normal)
			{
				Level.Session.Audio.Music.Event = "event:/music/lvl2/evil_madeline";
				Level.Session.Audio.Apply();
			}
		}

		public override void OnEnd(Level level)
		{
			if (WasSkipped)
			{
				if (evil != null)
				{
					base.Scene.Remove(evil);
				}
				if (mirror != null)
				{
					mirror.Broken();
				}
				base.Scene.Entities.FindFirst<DashBlock>()?.RemoveAndFlagAsGone();
				oshiro.Sprite.Play("idle_ground");
			}
			oshiro.Talker.Enabled = true;
			if (player != null)
			{
				player.StateMachine.Locked = false;
				player.StateMachine.State = 0;
			}
			level.Lighting.Alpha = level.BaseLightingAlpha;
			level.Session.SetFlag("oshiro_resort_suite");
			SetMusic();
		}
	}
}
