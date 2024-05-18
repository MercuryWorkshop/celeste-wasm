using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class ClimbBlocker : Component
	{
		public bool Blocking = true;

		public bool Edge;

		public ClimbBlocker(bool edge)
			: base(active: false, visible: false)
		{
			Edge = edge;
		}

		public static bool Check(Scene scene, Entity entity, Vector2 at)
		{
			Vector2 was = entity.Position;
			entity.Position = at;
			bool result = Check(scene, entity);
			entity.Position = was;
			return result;
		}

		public static bool Check(Scene scene, Entity entity)
		{
			foreach (ClimbBlocker cb in scene.Tracker.GetComponents<ClimbBlocker>())
			{
				if (cb.Blocking && entity.CollideCheck(cb.Entity))
				{
					return true;
				}
			}
			return false;
		}

		public static bool EdgeCheck(Scene scene, Entity entity, int dir)
		{
			foreach (ClimbBlocker cb in scene.Tracker.GetComponents<ClimbBlocker>())
			{
				if (cb.Blocking && cb.Edge && entity.CollideCheck(cb.Entity, entity.Position + Vector2.UnitX * dir))
				{
					return true;
				}
			}
			return false;
		}
	}
}
