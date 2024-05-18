using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public abstract class Backdrop
	{
		public class Fader
		{
			private struct Segment
			{
				public float PositionFrom;

				public float PositionTo;

				public float From;

				public float To;
			}

			private List<Segment> Segments = new List<Segment>();

			public Fader Add(float posFrom, float posTo, float fadeFrom, float fadeTo)
			{
				Segments.Add(new Segment
				{
					PositionFrom = posFrom,
					PositionTo = posTo,
					From = fadeFrom,
					To = fadeTo
				});
				return this;
			}

			public float Value(float position)
			{
				float result = 1f;
				foreach (Segment segment in Segments)
				{
					result *= Calc.ClampedMap(position, segment.PositionFrom, segment.PositionTo, segment.From, segment.To);
				}
				return result;
			}
		}

		public bool UseSpritebatch = true;

		public string Name;

		public HashSet<string> Tags = new HashSet<string>();

		public Vector2 Position;

		public Vector2 Scroll = Vector2.One;

		public Vector2 Speed;

		public Color Color = Color.White;

		public bool LoopX = true;

		public bool LoopY = true;

		public bool FlipX;

		public bool FlipY;

		public Fader FadeX;

		public Fader FadeY;

		public float FadeAlphaMultiplier = 1f;

		public float WindMultiplier;

		public HashSet<string> ExcludeFrom;

		public HashSet<string> OnlyIn;

		public string OnlyIfFlag;

		public string OnlyIfNotFlag;

		public string AlsoIfFlag;

		public bool? Dreaming;

		public bool Visible;

		public bool InstantIn = true;

		public bool InstantOut;

		public bool ForceVisible;

		public BackdropRenderer Renderer;

		public Backdrop()
		{
			Visible = true;
		}

		public bool IsVisible(Level level)
		{
			if (ForceVisible)
			{
				return true;
			}
			if (!string.IsNullOrEmpty(OnlyIfNotFlag) && level.Session.GetFlag(OnlyIfNotFlag))
			{
				return false;
			}
			if (!string.IsNullOrEmpty(AlsoIfFlag) && level.Session.GetFlag(AlsoIfFlag))
			{
				return true;
			}
			if (Dreaming.HasValue && Dreaming.Value != level.Session.Dreaming)
			{
				return false;
			}
			if (!string.IsNullOrEmpty(OnlyIfFlag) && !level.Session.GetFlag(OnlyIfFlag))
			{
				return false;
			}
			if (ExcludeFrom != null && ExcludeFrom.Contains(level.Session.Level))
			{
				return false;
			}
			if (OnlyIn != null && !OnlyIn.Contains(level.Session.Level))
			{
				return false;
			}
			return true;
		}

		public virtual void Update(Scene scene)
		{
			Level level = scene as Level;
			if (level.Transitioning)
			{
				if (InstantIn && IsVisible(level))
				{
					Visible = true;
				}
				if (InstantOut && !IsVisible(level))
				{
					Visible = false;
				}
			}
			else
			{
				Visible = IsVisible(level);
			}
		}

		public virtual void BeforeRender(Scene scene)
		{
		}

		public virtual void Render(Scene scene)
		{
		}

		public virtual void Ended(Scene scene)
		{
		}
	}
}
