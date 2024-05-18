using System;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class PostUpdateHook : Component
	{
		public Action OnPostUpdate;

		public PostUpdateHook(Action onPostUpdate)
			: base(active: false, visible: false)
		{
			OnPostUpdate = onPostUpdate;
		}
	}
}
