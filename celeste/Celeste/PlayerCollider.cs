using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class PlayerCollider : Component
	{
		public Action<Player> OnCollide;

		public Collider Collider;

		public Collider FeatherCollider;

		public PlayerCollider(Action<Player> onCollide, Collider collider = null, Collider featherCollider = null)
			: base(active: false, visible: false)
		{
			OnCollide = onCollide;
			Collider = collider;
			FeatherCollider = featherCollider;
		}

		public bool Check(Player player)
		{
			Collider switchTo = Collider;
			if (FeatherCollider != null && player.StateMachine.State == 19)
			{
				switchTo = FeatherCollider;
			}
			if (switchTo == null)
			{
				if (player.CollideCheck(base.Entity))
				{
					OnCollide(player);
					return true;
				}
				return false;
			}
			Collider was = base.Entity.Collider;
			base.Entity.Collider = switchTo;
			bool num = player.CollideCheck(base.Entity);
			base.Entity.Collider = was;
			if (num)
			{
				OnCollide(player);
				return true;
			}
			return false;
		}

		public override void DebugRender(Camera camera)
		{
			if (Collider != null)
			{
				Collider was = base.Entity.Collider;
				base.Entity.Collider = Collider;
				Collider.Render(camera, Color.HotPink);
				base.Entity.Collider = was;
			}
		}
	}
}
