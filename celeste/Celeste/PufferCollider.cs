using System;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class PufferCollider : Component
	{
		public Action<Puffer> OnCollide;

		public Collider Collider;

		public PufferCollider(Action<Puffer> onCollide, Collider collider = null)
			: base(active: false, visible: false)
		{
			OnCollide = onCollide;
			Collider = null;
		}

		public void Check(Puffer puffer)
		{
			if (OnCollide != null)
			{
				Collider was = base.Entity.Collider;
				if (Collider != null)
				{
					base.Entity.Collider = Collider;
				}
				if (puffer.CollideCheck(base.Entity))
				{
					OnCollide(puffer);
				}
				base.Entity.Collider = was;
			}
		}
	}
}
