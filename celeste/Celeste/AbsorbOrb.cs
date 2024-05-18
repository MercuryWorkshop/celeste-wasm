using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class AbsorbOrb : Entity
	{
		public Entity AbsorbInto;

		public Vector2? AbsorbTarget;

		private SimpleCurve curve;

		private float duration;

		private float percent;

		private float consumeDelay;

		private float burstSpeed;

		private Vector2 burstDirection;

		private Vector2 burstScale;

		private float alpha = 1f;

		private Image sprite;

		private BloomPoint bloom;

		public AbsorbOrb(Vector2 position, Entity into = null, Vector2? absorbTarget = null)
		{
			AbsorbInto = into;
			AbsorbTarget = absorbTarget;
			Position = position;
			base.Tag = Tags.FrozenUpdate;
			base.Depth = -2000000;
			consumeDelay = 0.7f + Calc.Random.NextFloat() * 0.3f;
			burstSpeed = 80f + Calc.Random.NextFloat() * 40f;
			burstDirection = Calc.AngleToVector(Calc.Random.NextFloat() * ((float)Math.PI * 2f), 1f);
			Add(sprite = new Image(GFX.Game["collectables/heartGem/orb"]));
			sprite.CenterOrigin();
			Add(bloom = new BloomPoint(1f, 16f));
		}

		public override void Update()
		{
			base.Update();
			Vector2 into = Vector2.Zero;
			bool intoDead = false;
			if (AbsorbInto != null)
			{
				into = AbsorbInto.Center;
				intoDead = AbsorbInto.Scene == null || (AbsorbInto is Player && (AbsorbInto as Player).Dead);
			}
			else if (AbsorbTarget.HasValue)
			{
				into = AbsorbTarget.Value;
			}
			else
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null)
				{
					into = player.Center;
				}
				intoDead = player == null || player.Scene == null || player.Dead;
			}
			if (intoDead)
			{
				Position += burstDirection * burstSpeed * Engine.RawDeltaTime;
				burstSpeed = Calc.Approach(burstSpeed, 800f, Engine.RawDeltaTime * 200f);
				sprite.Rotation = burstDirection.Angle();
				sprite.Scale = new Vector2(Math.Min(2f, 0.5f + burstSpeed * 0.02f), Math.Max(0.05f, 0.5f - burstSpeed * 0.004f));
				sprite.Color = Color.White * (alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime));
			}
			else if (consumeDelay > 0f)
			{
				Position += burstDirection * burstSpeed * Engine.RawDeltaTime;
				burstSpeed = Calc.Approach(burstSpeed, 0f, Engine.RawDeltaTime * 120f);
				sprite.Rotation = burstDirection.Angle();
				sprite.Scale = new Vector2(Math.Min(2f, 0.5f + burstSpeed * 0.02f), Math.Max(0.05f, 0.5f - burstSpeed * 0.004f));
				consumeDelay -= Engine.RawDeltaTime;
				if (consumeDelay <= 0f)
				{
					Vector2 from = Position;
					Vector2 to = into;
					Vector2 center = (from + to) / 2f;
					Vector2 perp = (to - from).SafeNormalize().Perpendicular() * (from - to).Length() * (0.05f + Calc.Random.NextFloat() * 0.45f);
					float diffX = to.X - from.X;
					float diffY = to.Y - from.Y;
					if ((Math.Abs(diffX) > Math.Abs(diffY) && Math.Sign(perp.X) != Math.Sign(diffX)) || (Math.Abs(diffY) > Math.Abs(diffY) && Math.Sign(perp.Y) != Math.Sign(diffY)))
					{
						perp *= -1f;
					}
					curve = new SimpleCurve(from, to, center + perp);
					duration = 0.3f + Calc.Random.NextFloat(0.25f);
					burstScale = sprite.Scale;
				}
			}
			else
			{
				curve.End = into;
				if (percent >= 1f)
				{
					RemoveSelf();
				}
				percent = Calc.Approach(percent, 1f, Engine.RawDeltaTime / duration);
				float ease = Ease.CubeIn(percent);
				Position = curve.GetPoint(ease);
				float amount = Calc.YoYo(ease) * curve.GetLengthParametric(10);
				sprite.Scale = new Vector2(Math.Min(2f, 0.5f + amount * 0.02f), Math.Max(0.05f, 0.5f - amount * 0.004f));
				sprite.Color = Color.White * (1f - ease);
				sprite.Rotation = Calc.Angle(Position, curve.GetPoint(Ease.CubeIn(percent + 0.01f)));
			}
		}
	}
}
