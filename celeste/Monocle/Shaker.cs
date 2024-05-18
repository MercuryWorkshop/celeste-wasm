using System;
using Microsoft.Xna.Framework;

namespace Monocle
{
	public class Shaker : Component
	{
		public Vector2 Value;

		public float Interval = 0.05f;

		public float Timer;

		public bool RemoveOnFinish;

		public Action<Vector2> OnShake;

		private bool on;

		public bool On
		{
			get
			{
				return on;
			}
			set
			{
				on = value;
				if (on)
				{
					return;
				}
				Timer = 0f;
				if (Value != Vector2.Zero)
				{
					Value = Vector2.Zero;
					if (OnShake != null)
					{
						OnShake(Vector2.Zero);
					}
				}
			}
		}

		public Shaker(bool on = true, Action<Vector2> onShake = null)
			: base(active: true, visible: false)
		{
			this.on = on;
			OnShake = onShake;
		}

		public Shaker(float time, bool removeOnFinish, Action<Vector2> onShake = null)
			: this(on: true, onShake)
		{
			Timer = time;
			RemoveOnFinish = removeOnFinish;
		}

		public Shaker ShakeFor(float seconds, bool removeOnFinish)
		{
			on = true;
			Timer = seconds;
			RemoveOnFinish = removeOnFinish;
			return this;
		}

		public override void Update()
		{
			if (on && Timer > 0f)
			{
				Timer -= Engine.DeltaTime;
				if (Timer <= 0f)
				{
					on = false;
					Value = Vector2.Zero;
					if (OnShake != null)
					{
						OnShake(Vector2.Zero);
					}
					if (RemoveOnFinish)
					{
						RemoveSelf();
					}
					return;
				}
			}
			if (on && base.Scene.OnInterval(Interval))
			{
				Value = Calc.Random.ShakeVector();
				if (OnShake != null)
				{
					OnShake(Value);
				}
			}
		}
	}
}
