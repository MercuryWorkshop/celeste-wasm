using System;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class SeekerCollider : Component
	{
		public Action<Seeker> OnCollide;

		public Collider Collider;

		public SeekerCollider(Action<Seeker> onCollide, Collider collider = null)
			: base(active: false, visible: false)
		{
			OnCollide = onCollide;
			Collider = null;
		}

		public void Check(Seeker seeker)
		{
			if (OnCollide != null)
			{
				Collider was = base.Entity.Collider;
				if (Collider != null)
				{
					base.Entity.Collider = Collider;
				}
				if (seeker.CollideCheck(base.Entity))
				{
					OnCollide(seeker);
				}
				base.Entity.Collider = was;
			}
		}
	}
}
