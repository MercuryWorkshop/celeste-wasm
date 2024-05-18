using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS02_BadelineIntro : CutsceneEntity
	{
		public const string Flag = "evil_maddy_intro";

		private Player player;

		private BadelineOldsite badeline;

		private Vector2 badelineEndPosition;

		private float anxietyFade;

		private float anxietyFadeTarget;

		private SineWave anxietySine;

		private float anxietyJitter;

		public CS02_BadelineIntro(BadelineOldsite badeline)
		{
			this.badeline = badeline;
			badelineEndPosition = badeline.Position + new Vector2(8f, -24f);
			Add(anxietySine = new SineWave(0.3f));
			Distort.AnxietyOrigin = new Vector2(0.5f, 0.75f);
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		public override void Update()
		{
			base.Update();
			anxietyFade = Calc.Approach(anxietyFade, anxietyFadeTarget, 2.5f * Engine.DeltaTime);
			if (base.Scene.OnInterval(0.1f))
			{
				anxietyJitter = Calc.Random.Range(-0.1f, 0.1f);
			}
			Distort.Anxiety = anxietyFade * Math.Max(0f, 0f + anxietyJitter + anxietySine.Value * 0.3f);
		}

		private IEnumerator Cutscene(Level level)
		{
			anxietyFadeTarget = 1f;
			while (true)
			{
				player = level.Tracker.GetEntity<Player>();
				if (player != null)
				{
					break;
				}
				yield return null;
			}
			while (!player.OnGround())
			{
				yield return null;
			}
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			yield return 1f;
			if (level.Session.Area.Mode == AreaMode.Normal)
			{
				Audio.SetMusic("event:/music/lvl2/evil_madeline");
			}
			yield return Textbox.Say("CH2_BADELINE_INTRO", TurnAround, RevealBadeline, StartLaughing, StopLaughing);
			anxietyFadeTarget = 0f;
			yield return Level.ZoomBack(0.5f);
			EndCutscene(level);
		}

		private IEnumerator TurnAround()
		{
			player.Facing = Facings.Left;
			yield return 0.2f;
			Add(new Coroutine(CutsceneEntity.CameraTo(new Vector2(Level.Bounds.X, Level.Camera.Y), 0.5f)));
			yield return Level.ZoomTo(new Vector2(84f, 135f), 2f, 0.5f);
			yield return 0.2f;
		}

		private IEnumerator RevealBadeline()
		{
			Audio.Play("event:/game/02_old_site/sequence_badeline_intro", badeline.Position);
			yield return 0.1f;
			Level.Displacement.AddBurst(badeline.Position + new Vector2(0f, -4f), 0.8f, 8f, 48f, 0.5f);
			Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
			yield return 0.1f;
			badeline.Hovering = true;
			badeline.Hair.Visible = true;
			badeline.Sprite.Play("fallSlow");
			Vector2 from = badeline.Position;
			Vector2 to = badelineEndPosition;
			for (float t = 0f; t < 1f; t += Engine.DeltaTime)
			{
				badeline.Position = from + (to - from) * Ease.CubeInOut(t);
				yield return null;
			}
			player.Facing = (Facings)Math.Sign(badeline.X - player.X);
			yield return 1f;
		}

		private IEnumerator StartLaughing()
		{
			yield return 0.2f;
			badeline.Sprite.Play("laugh", restart: true);
			yield return null;
		}

		private IEnumerator StopLaughing()
		{
			badeline.Sprite.Play("fallSlow", restart: true);
			yield return null;
		}

		public override void OnEnd(Level level)
		{
			Audio.SetMusic(null);
			Distort.Anxiety = 0f;
			if (player != null)
			{
				player.StateMachine.Locked = false;
				player.Facing = Facings.Left;
				player.StateMachine.State = 0;
				player.JustRespawned = true;
			}
			badeline.Position = badelineEndPosition;
			badeline.Visible = true;
			badeline.Hair.Visible = true;
			badeline.Sprite.Play("fallSlow");
			badeline.Hovering = false;
			badeline.Add(new Coroutine(badeline.StartChasingRoutine(level)));
			level.Session.SetFlag("evil_maddy_intro");
		}
	}
}
