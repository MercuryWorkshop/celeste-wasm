using System;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class CoreModeListener : Component
	{
		public Action<Session.CoreModes> OnChange;

		public CoreModeListener(Action<Session.CoreModes> onChange)
			: base(active: false, visible: false)
		{
			OnChange = onChange;
		}
	}
}
