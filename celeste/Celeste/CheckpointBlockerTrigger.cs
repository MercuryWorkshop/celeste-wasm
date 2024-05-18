using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class CheckpointBlockerTrigger : Trigger
	{
		public CheckpointBlockerTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
		}
	}
}
