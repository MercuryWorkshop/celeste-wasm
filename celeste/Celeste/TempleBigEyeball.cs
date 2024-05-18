using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class TempleBigEyeball : Entity
	{
		private class Fader : Entity
		{
			public float Fade;

			public Fader()
			{
				base.Tag = Tags.HUD;
			}

			public override void Render()
			{
				Draw.Rect(-10f, -10f, Engine.Width + 20, Engine.Height + 20, Color.White * Fade);
			}
		}

		private Sprite sprite;

		private Image pupil;

		private bool triggered;

		private Vector2 pupilTarget;

		private float pupilDelay;

		private Wiggler bounceWiggler;

		private Wiggler pupilWiggler;

		private float shockwaveTimer;

		private bool shockwaveFlag;

		private float pupilSpeed = 40f;

		private bool bursting;

		public TempleBigEyeball(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			Add(sprite = GFX.SpriteBank.Create("temple_eyeball"));
			Add(pupil = new Image(GFX.Game["danger/templeeye/pupil"]));
			pupil.CenterOrigin();
			base.Collider = new Hitbox(48f, 64f, -24f, -32f);
			Add(new PlayerCollider(OnPlayer));
			Add(new HoldableCollider(OnHoldable));
			Add(bounceWiggler = Wiggler.Create(0.5f, 3f));
			Add(pupilWiggler = Wiggler.Create(0.5f, 3f));
			shockwaveTimer = 2f;
		}

		private void OnPlayer(Player player)
		{
			if (!triggered)
			{
				Audio.Play("event:/game/05_mirror_temple/eyewall_bounce", player.Position);
				player.ExplodeLaunch(player.Center + Vector2.UnitX * 20f);
				player.Swat(-1);
				bounceWiggler.Start();
			}
		}

		private void OnHoldable(Holdable h)
		{
			if (!(h.Entity is TheoCrystal))
			{
				return;
			}
			TheoCrystal theo = h.Entity as TheoCrystal;
			if (!triggered && theo.Speed.X > 32f && !theo.Hold.IsHeld)
			{
				theo.Speed.X = -50f;
				theo.Speed.Y = -10f;
				triggered = true;
				bounceWiggler.Start();
				Collidable = false;
				Audio.SetAmbience(null);
				Audio.Play("event:/game/05_mirror_temple/eyewall_destroy", Position);
				Alarm.Set(this, 1.3f, delegate
				{
					Audio.SetMusic(null);
				});
				Add(new Coroutine(Burst()));
			}
		}

		private IEnumerator Burst()
		{
			bursting = true;
			Level level = base.Scene as Level;
			level.StartCutscene(OnSkip, fadeInOnSkip: false, endingChapterAfterCutscene: true);
			level.RegisterAreaComplete();
			Celeste.Freeze(0.1f);
			yield return null;
			float start = Glitch.Value;
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, null, 0.5f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				Glitch.Value = MathHelper.Lerp(start, 0f, t.Eased);
			};
			Add(tween);
			Player player = base.Scene.Tracker.GetEntity<Player>();
			TheoCrystal theo = base.Scene.Tracker.GetEntity<TheoCrystal>();
			if (player != null)
			{
				player.StateMachine.State = 11;
				player.StateMachine.Locked = true;
				if (player.OnGround())
				{
					player.DummyAutoAnimate = false;
					player.Sprite.Play("shaking");
				}
			}
			Add(new Coroutine(level.ZoomTo(theo.TopCenter - level.Camera.Position, 2f, 0.5f)));
			Add(new Coroutine(theo.Shatter()));
			foreach (TempleEye item in base.Scene.Entities.FindAll<TempleEye>())
			{
				item.Burst();
			}
			sprite.Play("burst");
			pupil.Visible = false;
			level.Shake(0.4f);
			yield return 2f;
			if (player != null && player.OnGround())
			{
				player.DummyAutoAnimate = false;
				player.Sprite.Play("shaking");
			}
			Visible = false;
			Fader fade = new Fader();
			level.Add(fade);
			while ((fade.Fade += Engine.DeltaTime) < 1f)
			{
				yield return null;
			}
			yield return 1f;
			level.EndCutscene();
			level.CompleteArea(spotlightWipe: false);
		}

		private void OnSkip(Level level)
		{
			level.CompleteArea(spotlightWipe: false);
		}

		public override void Update()
		{
			base.Update();
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				Audio.SetMusicParam("eye_distance", Calc.ClampedMap(player.X, (base.Scene as Level).Bounds.Left, base.X));
			}
			if (player != null && !bursting)
			{
				Glitch.Value = Calc.ClampedMap(Math.Abs(base.X - player.X), 100f, 900f, 0.2f, 0f);
			}
			if (!triggered && shockwaveTimer > 0f)
			{
				shockwaveTimer -= Engine.DeltaTime;
				if (shockwaveTimer <= 0f)
				{
					if (player != null)
					{
						shockwaveTimer = Calc.ClampedMap(Math.Abs(base.X - player.X), 100f, 500f, 2f, 3f);
						shockwaveFlag = !shockwaveFlag;
						if (shockwaveFlag)
						{
							shockwaveTimer -= 1f;
						}
					}
					base.Scene.Add(Engine.Pooler.Create<TempleBigEyeballShockwave>().Init(base.Center + new Vector2(50f, 0f)));
					pupilWiggler.Start();
					pupilTarget = new Vector2(-1f, 0f);
					pupilSpeed = 120f;
					pupilDelay = Math.Max(0.5f, pupilDelay);
				}
			}
			pupil.Position = Calc.Approach(pupil.Position, pupilTarget * 12f, Engine.DeltaTime * pupilSpeed);
			pupilSpeed = Calc.Approach(pupilSpeed, 40f, Engine.DeltaTime * 400f);
			TheoCrystal theo = base.Scene.Tracker.GetEntity<TheoCrystal>();
			if (theo != null && Math.Abs(base.X - theo.X) < 64f && Math.Abs(base.Y - theo.Y) < 64f)
			{
				pupilTarget = (theo.Center - Position).SafeNormalize();
			}
			else if (pupilDelay < 0f)
			{
				pupilTarget = Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 1f);
				pupilDelay = Calc.Random.Choose(0.2f, 1f, 2f);
			}
			else
			{
				pupilDelay -= Engine.DeltaTime;
			}
			if (player != null)
			{
				Level level = base.Scene as Level;
				Audio.SetMusicParam("eye_distance", Calc.ClampedMap(player.X, level.Bounds.Left + 32, base.X - 32f, 1f, 0f));
			}
		}

		public override void Render()
		{
			sprite.Scale.X = 1f + 0.15f * bounceWiggler.Value;
			pupil.Scale = Vector2.One * (1f + pupilWiggler.Value * 0.15f);
			base.Render();
		}
	}
}
