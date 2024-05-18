using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class GoldBerryCollectTrigger : Trigger
	{
		public GoldBerryCollectTrigger(EntityData e, Vector2 offset)
			: base(e, offset)
		{
		}
	}
}
