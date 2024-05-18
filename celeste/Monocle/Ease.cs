using System;

namespace Monocle
{
	public static class Ease
	{
		public delegate float Easer(float t);

		public static readonly Easer Linear = (float t) => t;

		public static readonly Easer SineIn = (float t) => 0f - (float)Math.Cos((float)Math.PI / 2f * t) + 1f;

		public static readonly Easer SineOut = (float t) => (float)Math.Sin((float)Math.PI / 2f * t);

		public static readonly Easer SineInOut = (float t) => (0f - (float)Math.Cos((float)Math.PI * t)) / 2f + 0.5f;

		public static readonly Easer QuadIn = (float t) => t * t;

		public static readonly Easer QuadOut = Invert(QuadIn);

		public static readonly Easer QuadInOut = Follow(QuadIn, QuadOut);

		public static readonly Easer CubeIn = (float t) => t * t * t;

		public static readonly Easer CubeOut = Invert(CubeIn);

		public static readonly Easer CubeInOut = Follow(CubeIn, CubeOut);

		public static readonly Easer QuintIn = (float t) => t * t * t * t * t;

		public static readonly Easer QuintOut = Invert(QuintIn);

		public static readonly Easer QuintInOut = Follow(QuintIn, QuintOut);

		public static readonly Easer ExpoIn = (float t) => (float)Math.Pow(2.0, 10f * (t - 1f));

		public static readonly Easer ExpoOut = Invert(ExpoIn);

		public static readonly Easer ExpoInOut = Follow(ExpoIn, ExpoOut);

		public static readonly Easer BackIn = (float t) => t * t * (2.70158f * t - 1.70158f);

		public static readonly Easer BackOut = Invert(BackIn);

		public static readonly Easer BackInOut = Follow(BackIn, BackOut);

		public static readonly Easer BigBackIn = (float t) => t * t * (4f * t - 3f);

		public static readonly Easer BigBackOut = Invert(BigBackIn);

		public static readonly Easer BigBackInOut = Follow(BigBackIn, BigBackOut);

		public static readonly Easer ElasticIn = delegate(float t)
		{
			float num3 = t * t;
			float num4 = num3 * t;
			return 33f * num4 * num3 + -59f * num3 * num3 + 32f * num4 + -5f * num3;
		};

		public static readonly Easer ElasticOut = delegate(float t)
		{
			float num = t * t;
			float num2 = num * t;
			return 33f * num2 * num + -106f * num * num + 126f * num2 + -67f * num + 15f * t;
		};

		public static readonly Easer ElasticInOut = Follow(ElasticIn, ElasticOut);

		private const float B1 = 0.36363637f;

		private const float B2 = 0.72727275f;

		private const float B3 = 0.54545456f;

		private const float B4 = 0.90909094f;

		private const float B5 = 0.8181818f;

		private const float B6 = 21f / 22f;

		public static readonly Easer BounceIn = delegate(float t)
		{
			t = 1f - t;
			if (t < 0.36363637f)
			{
				return 1f - 7.5625f * t * t;
			}
			if (t < 0.72727275f)
			{
				return 1f - (7.5625f * (t - 0.54545456f) * (t - 0.54545456f) + 0.75f);
			}
			return (t < 0.90909094f) ? (1f - (7.5625f * (t - 0.8181818f) * (t - 0.8181818f) + 0.9375f)) : (1f - (7.5625f * (t - 21f / 22f) * (t - 21f / 22f) + 63f / 64f));
		};

		public static readonly Easer BounceOut = delegate(float t)
		{
			if (t < 0.36363637f)
			{
				return 7.5625f * t * t;
			}
			if (t < 0.72727275f)
			{
				return 7.5625f * (t - 0.54545456f) * (t - 0.54545456f) + 0.75f;
			}
			return (t < 0.90909094f) ? (7.5625f * (t - 0.8181818f) * (t - 0.8181818f) + 0.9375f) : (7.5625f * (t - 21f / 22f) * (t - 21f / 22f) + 63f / 64f);
		};

		public static readonly Easer BounceInOut = delegate(float t)
		{
			if (t < 0.5f)
			{
				t = 1f - t * 2f;
				if (t < 0.36363637f)
				{
					return (1f - 7.5625f * t * t) / 2f;
				}
				if (t < 0.72727275f)
				{
					return (1f - (7.5625f * (t - 0.54545456f) * (t - 0.54545456f) + 0.75f)) / 2f;
				}
				if (t < 0.90909094f)
				{
					return (1f - (7.5625f * (t - 0.8181818f) * (t - 0.8181818f) + 0.9375f)) / 2f;
				}
				return (1f - (7.5625f * (t - 21f / 22f) * (t - 21f / 22f) + 63f / 64f)) / 2f;
			}
			t = t * 2f - 1f;
			if (t < 0.36363637f)
			{
				return 7.5625f * t * t / 2f + 0.5f;
			}
			if (t < 0.72727275f)
			{
				return (7.5625f * (t - 0.54545456f) * (t - 0.54545456f) + 0.75f) / 2f + 0.5f;
			}
			return (t < 0.90909094f) ? ((7.5625f * (t - 0.8181818f) * (t - 0.8181818f) + 0.9375f) / 2f + 0.5f) : ((7.5625f * (t - 21f / 22f) * (t - 21f / 22f) + 63f / 64f) / 2f + 0.5f);
		};

		public static Easer Invert(Easer easer)
		{
			return (float t) => 1f - easer(1f - t);
		}

		public static Easer Follow(Easer first, Easer second)
		{
			return (float t) => (!(t <= 0.5f)) ? (second(t * 2f - 1f) / 2f + 0.5f) : (first(t * 2f) / 2f);
		}

		public static float UpDown(float eased)
		{
			if (eased <= 0.5f)
			{
				return eased * 2f;
			}
			return 1f - (eased - 0.5f) * 2f;
		}
	}
}
