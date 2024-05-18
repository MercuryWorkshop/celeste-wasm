using Monocle;

namespace Celeste
{
	public class SpaceController : Entity
	{
		private Level level;

		public override void Added(Scene scene)
		{
			base.Added(scene);
			level = SceneAs<Level>();
		}

		public override void Update()
		{
			base.Update();
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				if (player.Top > level.Camera.Bottom + 12f)
				{
					player.Bottom = level.Camera.Top - 4f;
				}
				else if (player.Bottom < level.Camera.Top - 4f)
				{
					player.Top = level.Camera.Bottom + 12f;
				}
			}
		}
	}
}
