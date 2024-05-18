using System;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class TransitionListener : Component
	{
		public Action OnInBegin;

		public Action OnInEnd;

		public Action<float> OnIn;

		public Action OnOutBegin;

		public Action<float> OnOut;

		public TransitionListener()
			: base(active: false, visible: false)
		{
		}
	}
}
