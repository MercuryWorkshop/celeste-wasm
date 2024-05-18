using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class StaticMover : Component
	{
		public Func<Solid, bool> SolidChecker;

		public Func<JumpThru, bool> JumpThruChecker;

		public Action<Vector2> OnMove;

		public Action<Vector2> OnShake;

		public Action<Platform> OnAttach;

		public Action OnDestroy;

		public Action OnDisable;

		public Action OnEnable;

		public Platform Platform;

		public StaticMover()
			: base(active: false, visible: false)
		{
		}

		public void Destroy()
		{
			if (OnDestroy != null)
			{
				OnDestroy();
			}
			else
			{
				base.Entity.RemoveSelf();
			}
		}

		public void Shake(Vector2 amount)
		{
			if (OnShake != null)
			{
				OnShake(amount);
			}
		}

		public void Move(Vector2 amount)
		{
			if (OnMove != null)
			{
				OnMove(amount);
			}
			else
			{
				base.Entity.Position += amount;
			}
		}

		public bool IsRiding(Solid solid)
		{
			if (SolidChecker != null)
			{
				return SolidChecker(solid);
			}
			return false;
		}

		public bool IsRiding(JumpThru jumpThru)
		{
			if (JumpThruChecker != null)
			{
				return JumpThruChecker(jumpThru);
			}
			return false;
		}

		public void TriggerPlatform()
		{
			if (Platform != null)
			{
				Platform.OnStaticMoverTrigger(this);
			}
		}

		public void Disable()
		{
			if (OnDisable != null)
			{
				OnDisable();
			}
			else
			{
				base.Entity.Active = (base.Entity.Visible = (base.Entity.Collidable = false));
			}
		}

		public void Enable()
		{
			if (OnEnable != null)
			{
				OnEnable();
			}
			else
			{
				base.Entity.Active = (base.Entity.Visible = (base.Entity.Collidable = true));
			}
		}
	}
}
