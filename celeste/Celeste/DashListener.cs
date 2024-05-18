using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class DashListener : Component
	{
		public Action<Vector2> OnDash;

		public Action OnSet;

		public DashListener()
			: base(active: false, visible: false)
		{
		}
	}
}
