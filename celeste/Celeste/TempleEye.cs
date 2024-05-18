using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class TempleEye : Entity
	{
		private MTexture eyeTexture;

		private MTexture pupilTexture;

		private Sprite eyelid;

		private Vector2 pupilPosition;

		private Vector2 pupilTarget;

		private float blinkTimer;

		private bool bursting;

		private bool isBG;

		public TempleEye(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			isBG = !scene.CollideCheck<Solid>(Position);
			if (isBG)
			{
				eyeTexture = GFX.Game["scenery/temple/eye/bg_eye"];
				pupilTexture = GFX.Game["scenery/temple/eye/bg_pupil"];
				Add(eyelid = new Sprite(GFX.Game, "scenery/temple/eye/bg_lid"));
				base.Depth = 8990;
			}
			else
			{
				eyeTexture = GFX.Game["scenery/temple/eye/fg_eye"];
				pupilTexture = GFX.Game["scenery/temple/eye/fg_pupil"];
				Add(eyelid = new Sprite(GFX.Game, "scenery/temple/eye/fg_lid"));
				base.Depth = -10001;
			}
			eyelid.AddLoop("open", "", 0f, default(int));
			eyelid.Add("blink", "", 0.08f, "open", 0, 1, 1, 2, 3, 0);
			eyelid.Play("open");
			eyelid.CenterOrigin();
			SetBlinkTimer();
		}

		private void SetBlinkTimer()
		{
			blinkTimer = Calc.Random.Range(1f, 15f);
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			TheoCrystal theo = base.Scene.Tracker.GetEntity<TheoCrystal>();
			if (theo != null)
			{
				pupilTarget = (theo.Center - Position).SafeNormalize();
				pupilPosition = pupilTarget * 3f;
			}
		}

		public override void Update()
		{
			if (!bursting)
			{
				pupilPosition = Calc.Approach(pupilPosition, pupilTarget * 3f, Engine.DeltaTime * 16f);
				TheoCrystal theo = base.Scene.Tracker.GetEntity<TheoCrystal>();
				if (theo != null)
				{
					pupilTarget = (theo.Center - Position).SafeNormalize();
					if (base.Scene.OnInterval(0.25f) && Calc.Random.Chance(0.01f))
					{
						eyelid.Play("blink");
					}
				}
				blinkTimer -= Engine.DeltaTime;
				if (blinkTimer <= 0f)
				{
					SetBlinkTimer();
					eyelid.Play("blink");
				}
			}
			base.Update();
		}

		public void Burst()
		{
			bursting = true;
			Sprite burst = new Sprite(GFX.Game, isBG ? "scenery/temple/eye/bg_burst" : "scenery/temple/eye/fg_burst");
			burst.Add("burst", "", 0.08f);
			burst.Play("burst");
			burst.OnLastFrame = delegate
			{
				RemoveSelf();
			};
			burst.CenterOrigin();
			Add(burst);
			Remove(eyelid);
		}

		public override void Render()
		{
			if (!bursting)
			{
				eyeTexture.DrawCentered(Position);
				pupilTexture.DrawCentered(Position + pupilPosition);
			}
			base.Render();
		}
	}
}
