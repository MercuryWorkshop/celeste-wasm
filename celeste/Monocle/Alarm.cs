using System;
using System.Collections.Generic;

namespace Monocle
{
	public class Alarm : Component
	{
		public enum AlarmMode
		{
			Persist,
			Oneshot,
			Looping
		}

		public Action OnComplete;

		private static Stack<Alarm> cached = new Stack<Alarm>();

		public AlarmMode Mode { get; private set; }

		public float Duration { get; private set; }

		public float TimeLeft { get; private set; }

		public static Alarm Create(AlarmMode mode, Action onComplete, float duration = 1f, bool start = false)
		{
			Alarm alarm = ((cached.Count != 0) ? cached.Pop() : new Alarm());
			alarm.Init(mode, onComplete, duration, start);
			return alarm;
		}

		public static Alarm Set(Entity entity, float duration, Action onComplete, AlarmMode alarmMode = AlarmMode.Oneshot)
		{
			Alarm alarm = Create(alarmMode, onComplete, duration, start: true);
			entity.Add(alarm);
			return alarm;
		}

		private Alarm()
			: base(active: false, visible: false)
		{
		}

		private void Init(AlarmMode mode, Action onComplete, float duration = 1f, bool start = false)
		{
			Mode = mode;
			Duration = duration;
			OnComplete = onComplete;
			Active = false;
			TimeLeft = 0f;
			if (start)
			{
				Start();
			}
		}

		public override void Update()
		{
			TimeLeft -= Engine.DeltaTime;
			if (TimeLeft <= 0f)
			{
				TimeLeft = 0f;
				if (OnComplete != null)
				{
					OnComplete();
				}
				if (Mode == AlarmMode.Looping)
				{
					Start();
				}
				else if (Mode == AlarmMode.Oneshot)
				{
					RemoveSelf();
				}
				else if (TimeLeft <= 0f)
				{
					Active = false;
				}
			}
		}

		public override void Removed(Entity entity)
		{
			base.Removed(entity);
			cached.Push(this);
		}

		public void Start()
		{
			Active = true;
			TimeLeft = Duration;
		}

		public void Start(float duration)
		{
			Duration = duration;
			Start();
		}

		public void Stop()
		{
			Active = false;
		}
	}
}
