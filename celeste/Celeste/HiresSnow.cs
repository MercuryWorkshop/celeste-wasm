using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class HiresSnow : Renderer
	{
		private struct Particle
		{
			public float Scale;

			public Vector2 Position;

			public float Speed;

			public float Sin;

			public float Rotation;

			public Color Color;

			public void Reset(Vector2 direction)
			{
				float range = Calc.Random.NextFloat();
				range *= range * range * range;
				Scale = Calc.Map(range, 0f, 1f, 0.05f, 0.8f);
				Speed = Scale * (float)Calc.Random.Range(2500, 5000);
				if (direction.X < 0f)
				{
					Position = new Vector2(Engine.Width + 128, Calc.Random.NextFloat(Engine.Height));
				}
				else if (direction.X > 0f)
				{
					Position = new Vector2(-128f, Calc.Random.NextFloat(Engine.Height));
				}
				else if (direction.Y > 0f)
				{
					Position = new Vector2(Calc.Random.NextFloat(Engine.Width), -128f);
				}
				else if (direction.Y < 0f)
				{
					Position = new Vector2(Calc.Random.NextFloat(Engine.Width), Engine.Height + 128);
				}
				Sin = Calc.Random.NextFloat((float)Math.PI * 2f);
				Rotation = Calc.Random.NextFloat((float)Math.PI * 2f);
				Color = Color.Lerp(Color.White, Color.Transparent, range * 0.8f);
			}
		}

		public float Alpha = 1f;

		public float ParticleAlpha = 1f;

		public Vector2 Direction = new Vector2(-1f, 0f);

		public ScreenWipe AttachAlphaTo;

		private Particle[] particles;

		private MTexture overlay;

		private MTexture snow;

		private float timer;

		private float overlayAlpha;

		public HiresSnow(float overlayAlpha = 0.45f)
		{
			this.overlayAlpha = overlayAlpha;
			overlay = OVR.Atlas["overlay"];
			snow = OVR.Atlas["snow"].GetSubtexture(1, 1, 254, 254);
			particles = new Particle[50];
			Reset();
		}

		public void Reset()
		{
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Reset(Direction);
				particles[i].Position.X = Calc.Random.NextFloat(Engine.Width);
				particles[i].Position.Y = Calc.Random.NextFloat(Engine.Height);
			}
		}

		public override void Update(Scene scene)
		{
			base.Update(scene);
			if (AttachAlphaTo != null)
			{
				Alpha = AttachAlphaTo.Percent;
			}
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Position += Direction * particles[i].Speed * Engine.DeltaTime;
				particles[i].Position.Y += (float)Math.Sin(particles[i].Sin) * 100f * Engine.DeltaTime;
				particles[i].Sin += Engine.DeltaTime;
				if (particles[i].Position.X < -128f || particles[i].Position.X > (float)(Engine.Width + 128) || particles[i].Position.Y < -128f || particles[i].Position.Y > (float)(Engine.Height + 128))
				{
					particles[i].Reset(Direction);
				}
			}
			timer += Engine.DeltaTime;
		}

		public override void Render(Scene scene)
		{
			float spd = Calc.Clamp(Direction.Length(), 0f, 20f);
			float rot = 0f;
			Vector2 stretch = Vector2.One;
			bool doStretch = spd > 1f;
			if (doStretch)
			{
				rot = Direction.Angle();
				stretch = new Vector2(spd, 0.2f + (1f - spd / 20f) * 0.8f);
			}
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, null, null, null, Engine.ScreenMatrix);
			float pa = Alpha * ParticleAlpha;
			for (int i = 0; i < particles.Length; i++)
			{
				Color c = particles[i].Color;
				float r = particles[i].Rotation;
				if (pa < 1f)
				{
					c *= pa;
				}
				snow.DrawCentered(particles[i].Position, c, stretch * particles[i].Scale, doStretch ? rot : r);
			}
			float x = timer * 32f % (float)overlay.Width;
			float y = timer * 20f % (float)overlay.Height;
			Color color = Color.White * (Alpha * overlayAlpha);
			Draw.SpriteBatch.Draw(overlay.Texture.Texture, Vector2.Zero, new Rectangle(-(int)x, -(int)y, 1920, 1080), color);
			Draw.SpriteBatch.End();
		}
	}
}
