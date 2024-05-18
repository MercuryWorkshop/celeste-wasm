using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class CreditsTrigger : Trigger
	{
		public string Event;

		public CreditsTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			Event = data.Attr("event");
		}

		public override void OnEnter(Player player)
		{
			Triggered = true;
			if (CS07_Credits.Instance != null)
			{
				CS07_Credits.Instance.Event = Event;
			}
		}
	}
}
