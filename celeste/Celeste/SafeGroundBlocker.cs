using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class SafeGroundBlocker : Component
	{
		public bool Blocking = true;

		public Collider CheckWith;

		public SafeGroundBlocker(Collider checkWith = null)
			: base(active: false, visible: false)
		{
			CheckWith = checkWith;
		}

		public bool Check(Player player)
		{
			if (!Blocking)
			{
				return false;
			}
			Collider old = base.Entity.Collider;
			if (CheckWith != null)
			{
				base.Entity.Collider = CheckWith;
			}
			bool result = player.CollideCheck(base.Entity);
			base.Entity.Collider = old;
			return result;
		}

		public override void DebugRender(Camera camera)
		{
			Collider old = base.Entity.Collider;
			if (CheckWith != null)
			{
				base.Entity.Collider = CheckWith;
			}
			base.Entity.Collider.Render(camera, Color.Aqua);
			base.Entity.Collider = old;
		}
	}
}
