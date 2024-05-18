using System;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class CustomBloom : Component
	{
		public Action OnRenderBloom;

		public CustomBloom(Action onRenderBloom)
			: base(active: false, visible: true)
		{
			OnRenderBloom = onRenderBloom;
		}
	}
}
