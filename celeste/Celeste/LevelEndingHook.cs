using System;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class LevelEndingHook : Component
	{
		public Action OnEnd;

		public LevelEndingHook(Action onEnd)
			: base(active: false, visible: false)
		{
			OnEnd = onEnd;
		}
	}
}
