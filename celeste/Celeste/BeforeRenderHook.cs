using System;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class BeforeRenderHook : Component
	{
		public Action Callback;

		public BeforeRenderHook(Action callback)
			: base(active: false, visible: true)
		{
			Callback = callback;
		}
	}
}
