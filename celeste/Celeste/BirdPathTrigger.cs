using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class BirdPathTrigger : Trigger
	{
		private BirdPath bird;

		private bool triggered;

		public BirdPathTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			BirdPath bp = base.Scene.Entities.FindFirst<BirdPath>();
			if (bp != null)
			{
				bird = bp;
				bird.WaitForTrigger();
			}
			else
			{
				RemoveSelf();
			}
		}

		public override void OnEnter(Player player)
		{
			base.OnEnter(player);
			if (!triggered)
			{
				bird.Trigger();
				triggered = true;
			}
		}
	}
}
