using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class Payphone : Entity
	{
		public static ParticleType P_Snow;

		public static ParticleType P_SnowB;

		public bool Broken;

		public Sprite Sprite;

		public Image Blink;

		private VertexLight light;

		private BloomPoint bloom;

		private float lightFlickerTimer;

		private float lightFlickerFor = 0.1f;

		private int lastFrame;

		private SoundSource buzzSfx;

		public Payphone(Vector2 pos)
			: base(pos)
		{
			base.Depth = 1;
			Add(Sprite = GFX.SpriteBank.Create("payphone"));
			Sprite.Play("idle");
			Add(Blink = new Image(GFX.Game["cutscenes/payphone/blink"]));
			Blink.Origin = Sprite.Origin;
			Blink.Visible = false;
			Add(light = new VertexLight(new Vector2(-6f, -45f), Color.White, 1f, 8, 96));
			light.Spotlight = true;
			light.SpotlightDirection = new Vector2(0f, 1f).Angle();
			Add(bloom = new BloomPoint(new Vector2(-6f, -45f), 0.8f, 8f));
			Add(buzzSfx = new SoundSource());
			buzzSfx.Play("event:/env/local/02_old_site/phone_lamp");
			buzzSfx.Param("on", 1f);
		}

		public override void Update()
		{
			base.Update();
			if (!Broken)
			{
				lightFlickerTimer -= Engine.DeltaTime;
				if (lightFlickerTimer <= 0f)
				{
					if (base.Scene.OnInterval(0.025f))
					{
						bool on = Calc.Random.NextFloat() > 0.5f;
						light.Visible = on;
						bloom.Visible = on;
						Blink.Visible = !on;
						buzzSfx.Param("on", on ? 1 : 0);
					}
					if (lightFlickerTimer < 0f - lightFlickerFor)
					{
						lightFlickerTimer = Calc.Random.Choose(0.4f, 0.6f, 0.8f, 1f);
						lightFlickerFor = Calc.Random.Choose(0.1f, 0.2f, 0.05f);
						light.Visible = true;
						bloom.Visible = true;
						Blink.Visible = false;
						buzzSfx.Param("on", 1f);
					}
				}
			}
			else
			{
				Blink.Visible = (bloom.Visible = (light.Visible = false));
				buzzSfx.Param("on", 0f);
			}
			if (Sprite.CurrentAnimationID == "eat" && Sprite.CurrentAnimationFrame == 5 && lastFrame != Sprite.CurrentAnimationFrame)
			{
				Level level = SceneAs<Level>();
				level.ParticlesFG.Emit(P_Snow, 10, level.Camera.Position + new Vector2(236f, 152f), new Vector2(10f, 0f));
				level.ParticlesFG.Emit(P_SnowB, 8, level.Camera.Position + new Vector2(236f, 152f), new Vector2(6f, 0f));
				level.DirectionalShake(Vector2.UnitY);
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			}
			if (Sprite.CurrentAnimationID == "eat" && Sprite.CurrentAnimationFrame == Sprite.CurrentAnimationTotalFrames - 5 && lastFrame != Sprite.CurrentAnimationFrame)
			{
				Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
			}
			lastFrame = Sprite.CurrentAnimationFrame;
		}
	}
}
