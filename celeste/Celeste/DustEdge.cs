using System;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class DustEdge : Component
	{
		public Action RenderDust;

		public DustEdge(Action onRenderDust)
			: base(active: false, visible: true)
		{
			RenderDust = onRenderDust;
		}
	}
}
