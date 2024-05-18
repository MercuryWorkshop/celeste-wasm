using System;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class DisplacementRenderHook : Component
	{
		public Action RenderDisplacement;

		public DisplacementRenderHook(Action render)
			: base(active: false, visible: true)
		{
			RenderDisplacement = render;
		}
	}
}
