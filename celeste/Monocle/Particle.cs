using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Monocle
{
	public struct Particle
	{
		public Entity Track;

		public ParticleType Type;

		public MTexture Source;

		public bool Active;

		public Color Color;

		public Color StartColor;

		public Vector2 Position;

		public Vector2 Speed;

		public float Size;

		public float StartSize;

		public float Life;

		public float StartLife;

		public float ColorSwitch;

		public float Rotation;

		public float Spin;

		public bool SimulateFor(float duration)
		{
			if (duration > Life)
			{
				Life = 0f;
				Active = false;
				return false;
			}
			float dt = Engine.TimeRate * ((float)Engine.Instance.TargetElapsedTime.Milliseconds / 1000f);
			if (dt > 0f)
			{
				for (float t = 0f; t < duration; t += dt)
				{
					Update(dt);
				}
			}
			return true;
		}

		public void Update(float? delta = null)
		{
			float dt = 0f;
			dt = ((!delta.HasValue) ? (Type.UseActualDeltaTime ? Engine.RawDeltaTime : Engine.DeltaTime) : delta.Value);
			float ease = Life / StartLife;
			Life -= dt;
			if (Life <= 0f)
			{
				Active = false;
				return;
			}
			if (Type.RotationMode == ParticleType.RotationModes.SameAsDirection)
			{
				if (Speed != Vector2.Zero)
				{
					Rotation = Speed.Angle();
				}
			}
			else
			{
				Rotation += Spin * dt;
			}
			float alpha = ((Type.FadeMode == ParticleType.FadeModes.Linear) ? ease : ((Type.FadeMode == ParticleType.FadeModes.Late) ? Math.Min(1f, ease / 0.25f) : ((Type.FadeMode != ParticleType.FadeModes.InAndOut) ? 1f : ((ease > 0.75f) ? (1f - (ease - 0.75f) / 0.25f) : ((!(ease < 0.25f)) ? 1f : (ease / 0.25f))))));
			if (alpha == 0f)
			{
				Color = Color.Transparent;
			}
			else
			{
				if (Type.ColorMode == ParticleType.ColorModes.Static)
				{
					Color = StartColor;
				}
				else if (Type.ColorMode == ParticleType.ColorModes.Fade)
				{
					Color = Color.Lerp(Type.Color2, StartColor, ease);
				}
				else if (Type.ColorMode == ParticleType.ColorModes.Blink)
				{
					Color = (Calc.BetweenInterval(Life, 0.1f) ? StartColor : Type.Color2);
				}
				else if (Type.ColorMode == ParticleType.ColorModes.Choose)
				{
					Color = StartColor;
				}
				if (alpha < 1f)
				{
					Color *= alpha;
				}
			}
			Position += Speed * dt;
			Speed += Type.Acceleration * dt;
			Speed = Calc.Approach(Speed, Vector2.Zero, Type.Friction * dt);
			if (Type.SpeedMultiplier != 1f)
			{
				Speed *= (float)Math.Pow(Type.SpeedMultiplier, dt);
			}
			if (Type.ScaleOut)
			{
				Size = StartSize * Ease.CubeOut(ease);
			}
		}

		public void Render()
		{
			Vector2 renderAt = new Vector2((int)Position.X, (int)Position.Y);
			if (Track != null)
			{
				renderAt += Track.Position;
			}
			Draw.SpriteBatch.Draw(Source.Texture.Texture, renderAt, Source.ClipRect, Color, Rotation, Source.Center, Size, SpriteEffects.None, 0f);
		}

		public void Render(float alpha)
		{
			Vector2 renderAt = new Vector2((int)Position.X, (int)Position.Y);
			if (Track != null)
			{
				renderAt += Track.Position;
			}
			Draw.SpriteBatch.Draw(Source.Texture.Texture, renderAt, Source.ClipRect, Color * alpha, Rotation, Source.Center, Size, SpriteEffects.None, 0f);
		}
	}
}
