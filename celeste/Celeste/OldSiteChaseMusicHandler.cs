using Monocle;

namespace Celeste
{
	public class OldSiteChaseMusicHandler : Entity
	{
		public OldSiteChaseMusicHandler()
		{
			base.Tag = (int)Tags.TransitionUpdate | (int)Tags.Global;
		}

		public override void Update()
		{
			base.Update();
			int minX = 1150;
			int maxX = 2832;
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null && Audio.CurrentMusic == "event:/music/lvl2/chase")
			{
				float ease = (player.X - (float)minX) / (float)(maxX - minX);
				Audio.SetMusicParam("escape", ease);
			}
		}
	}
}
