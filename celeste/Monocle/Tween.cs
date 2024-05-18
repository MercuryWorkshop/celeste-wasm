using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Monocle
{
	public class Tween : Component
	{
		public enum TweenMode
		{
			Persist,
			Oneshot,
			Looping,
			YoyoOneshot,
			YoyoLooping
		}

		public Ease.Easer Easer;

		public Action<Tween> OnUpdate;

		public Action<Tween> OnComplete;

		public Action<Tween> OnStart;

		public bool UseRawDeltaTime;

		private bool startedReversed;

		private ulong cachedFrame;

		private static List<Tween> cached = new List<Tween>();

		public TweenMode Mode { get; private set; }

		public float Duration { get; private set; }

		public float TimeLeft { get; private set; }

		public float Percent { get; private set; }

		public float Eased { get; private set; }

		public bool Reverse { get; private set; }

		public float Inverted => 1f - Eased;

		public static Tween Create(TweenMode mode, Ease.Easer easer = null, float duration = 1f, bool start = false)
		{
			Tween tween = null;
			foreach (Tween t in cached)
			{
				if (Engine.FrameCounter > t.cachedFrame + 3)
				{
					tween = t;
					cached.Remove(t);
					break;
				}
			}
			if (tween == null)
			{
				tween = new Tween();
			}
			tween.OnUpdate = (tween.OnComplete = (tween.OnStart = null));
			tween.Init(mode, easer, duration, start);
			return tween;
		}

		public static Tween Set(Entity entity, TweenMode tweenMode, float duration, Ease.Easer easer, Action<Tween> onUpdate, Action<Tween> onComplete = null)
		{
			Tween tween = Create(tweenMode, easer, duration, start: true);
			tween.OnUpdate = (Action<Tween>)Delegate.Combine(tween.OnUpdate, onUpdate);
			tween.OnComplete = (Action<Tween>)Delegate.Combine(tween.OnComplete, onComplete);
			entity.Add(tween);
			return tween;
		}

		public static Tween Position(Entity entity, Vector2 targetPosition, float duration, Ease.Easer easer, TweenMode tweenMode = TweenMode.Oneshot)
		{
			Vector2 startPosition = entity.Position;
			Tween tween = Create(tweenMode, easer, duration, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				entity.Position = Vector2.Lerp(startPosition, targetPosition, t.Eased);
			};
			entity.Add(tween);
			return tween;
		}

		private Tween()
			: base(active: false, visible: false)
		{
		}

		private void Init(TweenMode mode, Ease.Easer easer, float duration, bool start)
		{
			if (duration <= 0f)
			{
				duration = 1E-06f;
			}
			UseRawDeltaTime = false;
			Mode = mode;
			Easer = easer;
			Duration = duration;
			TimeLeft = 0f;
			Percent = 0f;
			Active = false;
			if (start)
			{
				Start();
			}
		}

		public override void Removed(Entity entity)
		{
			base.Removed(entity);
			cached.Add(this);
			cachedFrame = Engine.FrameCounter;
		}

		public override void Update()
		{
			TimeLeft -= (UseRawDeltaTime ? Engine.RawDeltaTime : Engine.DeltaTime);
			Percent = Math.Max(0f, TimeLeft) / Duration;
			if (!Reverse)
			{
				Percent = 1f - Percent;
			}
			if (Easer != null)
			{
				Eased = Easer(Percent);
			}
			else
			{
				Eased = Percent;
			}
			if (OnUpdate != null)
			{
				OnUpdate(this);
			}
			if (!(TimeLeft <= 0f))
			{
				return;
			}
			TimeLeft = 0f;
			if (OnComplete != null)
			{
				OnComplete(this);
			}
			switch (Mode)
			{
			case TweenMode.Persist:
				Active = false;
				break;
			case TweenMode.Oneshot:
				Active = false;
				RemoveSelf();
				break;
			case TweenMode.Looping:
				Start(Reverse);
				break;
			case TweenMode.YoyoOneshot:
				if (Reverse == startedReversed)
				{
					Start(!Reverse);
					startedReversed = !Reverse;
				}
				else
				{
					Active = false;
					RemoveSelf();
				}
				break;
			case TweenMode.YoyoLooping:
				Start(!Reverse);
				break;
			}
		}

		public void Start()
		{
			Start(reverse: false);
		}

		public void Start(bool reverse)
		{
			bool flag2 = (Reverse = reverse);
			startedReversed = flag2;
			TimeLeft = Duration;
			float num3 = (Eased = (Percent = (Reverse ? 1 : 0)));
			Active = true;
			if (OnStart != null)
			{
				OnStart(this);
			}
		}

		public void Start(float duration, bool reverse = false)
		{
			Duration = duration;
			Start(reverse);
		}

		public void Stop()
		{
			Active = false;
		}

		public void Reset()
		{
			TimeLeft = Duration;
			float num3 = (Eased = (Percent = (Reverse ? 1 : 0)));
		}

		public IEnumerator Wait()
		{
			while (Active)
			{
				yield return null;
			}
		}
	}
}
