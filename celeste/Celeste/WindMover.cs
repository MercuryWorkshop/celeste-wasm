using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class WindMover : Component
	{
		public Action<Vector2> Move;

		public WindMover(Action<Vector2> move)
			: base(active: false, visible: false)
		{
			Move = move;
		}
	}
}
