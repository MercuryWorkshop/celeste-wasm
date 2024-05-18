using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class VertexLight : Component
	{
		public int Index = -1;

		public bool Dirty = true;

		public bool InSolid;

		public Vector2 LastNonSolidPosition;

		public Vector2 LastEntityPosition;

		public Vector2 LastPosition;

		public float InSolidAlphaMultiplier = 1f;

		public bool Started;

		public bool Spotlight;

		public float SpotlightDirection;

		public float SpotlightPush;

		public Color Color = Color.White;

		public float Alpha = 1f;

		private Vector2 position;

		private float startRadius = 16f;

		private float endRadius = 32f;

		public Vector2 Center => base.Entity.Position + position;

		public float X
		{
			get
			{
				return position.X;
			}
			set
			{
				Position = new Vector2(value, position.Y);
			}
		}

		public float Y
		{
			get
			{
				return position.Y;
			}
			set
			{
				Position = new Vector2(position.X, value);
			}
		}

		public Vector2 Position
		{
			get
			{
				return position;
			}
			set
			{
				if (position != value)
				{
					Dirty = true;
					position = value;
				}
			}
		}

		public float StartRadius
		{
			get
			{
				return startRadius;
			}
			set
			{
				if (startRadius != value)
				{
					Dirty = true;
					startRadius = value;
				}
			}
		}

		public float EndRadius
		{
			get
			{
				return endRadius;
			}
			set
			{
				if (endRadius != value)
				{
					Dirty = true;
					endRadius = value;
				}
			}
		}

		public VertexLight()
			: base(active: true, visible: true)
		{
		}

		public VertexLight(Color color, float alpha, int startFade, int endFade)
			: this(Vector2.Zero, color, alpha, startFade, endFade)
		{
		}

		public VertexLight(Vector2 position, Color color, float alpha, int startFade, int endFade)
			: base(active: true, visible: true)
		{
			Position = position;
			Color = color;
			Alpha = alpha;
			StartRadius = startFade;
			EndRadius = endFade;
		}

		public override void Added(Entity entity)
		{
			base.Added(entity);
			LastNonSolidPosition = Center;
			LastEntityPosition = base.Entity.Position;
			LastPosition = Position;
		}

		public override void Update()
		{
			InSolidAlphaMultiplier = Calc.Approach(InSolidAlphaMultiplier, (!InSolid) ? 1 : 0, Engine.DeltaTime * 4f);
			base.Update();
		}

		public override void HandleGraphicsReset()
		{
			Dirty = true;
			base.HandleGraphicsReset();
		}

		public Tween CreatePulseTween()
		{
			float startA = StartRadius;
			float startB = startA + 6f;
			float endA = EndRadius;
			float endB = endA + 12f;
			Tween tween = Tween.Create(Tween.TweenMode.Persist, null, 0.5f);
			tween.OnUpdate = delegate(Tween t)
			{
				StartRadius = (int)MathHelper.Lerp(startB, startA, t.Eased);
				EndRadius = (int)MathHelper.Lerp(endB, endA, t.Eased);
			};
			return tween;
		}

		public Tween CreateFadeInTween(float time)
		{
			float from = 0f;
			float to = Alpha;
			Alpha = 0f;
			Tween tween = Tween.Create(Tween.TweenMode.Persist, Ease.CubeOut, time);
			tween.OnUpdate = delegate(Tween t)
			{
				Alpha = MathHelper.Lerp(from, to, t.Eased);
			};
			return tween;
		}

		public Tween CreateBurstTween(float time)
		{
			time += 0.8f;
			float delay = (time - 0.8f) / time;
			float startA = StartRadius;
			float startB = startA + 6f;
			float endA = EndRadius;
			float endB = endA + 12f;
			Tween tween = Tween.Create(Tween.TweenMode.Persist, null, time);
			tween.OnUpdate = delegate(Tween t)
			{
				float value;
				if (t.Percent >= delay)
				{
					value = (t.Percent - delay) / (1f - delay);
					value = MathHelper.Clamp(value, 0f, 1f);
					value = Ease.CubeIn(value);
				}
				else
				{
					value = 0f;
				}
				StartRadius = (int)MathHelper.Lerp(startB, startA, value);
				EndRadius = (int)MathHelper.Lerp(endB, endA, value);
			};
			return tween;
		}
	}
}
