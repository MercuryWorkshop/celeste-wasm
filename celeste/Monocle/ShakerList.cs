using System;
using Microsoft.Xna.Framework;

namespace Monocle
{
	public class ShakerList : Component
	{
		public Vector2[] Values;

		public float Interval = 0.05f;

		public float Timer;

		public bool RemoveOnFinish;

		public Action<Vector2[]> OnShake;

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
				if (Values[0] != Vector2.Zero)
				{
					for (int i = 0; i < Values.Length; i++)
					{
						Values[i] = Vector2.Zero;
					}
					if (OnShake != null)
					{
						OnShake(Values);
					}
				}
			}
		}

		public ShakerList(int length, bool on = true, Action<Vector2[]> onShake = null)
			: base(active: true, visible: false)
		{
			Values = new Vector2[length];
			this.on = on;
			OnShake = onShake;
		}

		public ShakerList(int length, float time, bool removeOnFinish, Action<Vector2[]> onShake = null)
			: this(length, on: true, onShake)
		{
			Timer = time;
			RemoveOnFinish = removeOnFinish;
		}

		public ShakerList ShakeFor(float seconds, bool removeOnFinish)
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
					for (int j = 0; j < Values.Length; j++)
					{
						Values[j] = Vector2.Zero;
					}
					if (OnShake != null)
					{
						OnShake(Values);
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
				for (int i = 0; i < Values.Length; i++)
				{
					Values[i] = Calc.Random.ShakeVector();
				}
				if (OnShake != null)
				{
					OnShake(Values);
				}
			}
		}
	}
}
