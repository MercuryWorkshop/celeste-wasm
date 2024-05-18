using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class MirrorSurface : Component
	{
		public Action OnRender;

		private Vector2 reflectionOffset;

		public Vector2 ReflectionOffset
		{
			get
			{
				return reflectionOffset;
			}
			set
			{
				reflectionOffset = value;
				ReflectionColor = new Color(0.5f + Calc.Clamp(reflectionOffset.X / 32f, -1f, 1f) * 0.5f, 0.5f + Calc.Clamp(reflectionOffset.Y / 32f, -1f, 1f) * 0.5f, 0f, 1f);
			}
		}

		public Color ReflectionColor { get; private set; }

		public MirrorSurface(Action onRender = null)
			: base(active: false, visible: true)
		{
			OnRender = onRender;
		}
	}
}
