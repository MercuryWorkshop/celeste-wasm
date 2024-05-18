using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class WindTrigger : Trigger
	{
		public WindController.Patterns Pattern;

		public WindTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			Pattern = data.Enum("pattern", WindController.Patterns.None);
		}

		public override void OnEnter(Player player)
		{
			base.OnEnter(player);
			WindController controller = base.Scene.Entities.FindFirst<WindController>();
			if (controller == null)
			{
				controller = new WindController(Pattern);
				base.Scene.Add(controller);
			}
			else
			{
				controller.SetPattern(Pattern);
			}
		}
	}
}
