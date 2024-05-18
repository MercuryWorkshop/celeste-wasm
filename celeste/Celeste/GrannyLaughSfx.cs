using Monocle;

namespace Celeste
{
	public class GrannyLaughSfx : Component
	{
		public bool FirstPlay;

		private Sprite sprite;

		private bool ready = true;

		public GrannyLaughSfx(Sprite sprite)
			: base(active: true, visible: false)
		{
			this.sprite = sprite;
		}

		public override void Update()
		{
			if (sprite.CurrentAnimationID == "laugh" && sprite.CurrentAnimationFrame == 0 && ready)
			{
				if (FirstPlay)
				{
					Audio.Play("event:/char/granny/laugh_firstphrase", base.Entity.Position);
				}
				else
				{
					Audio.Play("event:/char/granny/laugh_oneha", base.Entity.Position);
				}
				ready = false;
			}
			if (!FirstPlay && (sprite.CurrentAnimationID != "laugh" || sprite.CurrentAnimationFrame > 0))
			{
				ready = true;
			}
		}
	}
}
