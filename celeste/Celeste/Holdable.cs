using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class Holdable : Component
	{
		public Collider PickupCollider;

		public Action OnPickup;

		public Action<Vector2> OnCarry;

		public Action<Vector2> OnRelease;

		public Func<HoldableCollider, bool> DangerousCheck;

		public Action<Seeker> OnHitSeeker;

		public Action<HoldableCollider, int> OnSwat;

		public Func<Spring, bool> OnHitSpring;

		public Action<Entity> OnHitSpinner;

		public Func<Vector2> SpeedGetter;

		public bool SlowRun = true;

		public bool SlowFall;

		private float cannotHoldDelay;

		private Vector2 startPos;

		private float gravityTimer;

		private float cannotHoldTimer;

		private int idleDepth;

		public Player Holder { get; private set; }

		public bool IsHeld => Holder != null;

		public bool ShouldHaveGravity => gravityTimer <= 0f;

		public Holdable(float cannotHoldDelay = 0.1f)
			: base(active: true, visible: false)
		{
			this.cannotHoldDelay = cannotHoldDelay;
		}

		public bool Check(Player player)
		{
			Collider was = base.Entity.Collider;
			if (PickupCollider != null)
			{
				base.Entity.Collider = PickupCollider;
			}
			bool result = player.CollideCheck(base.Entity);
			base.Entity.Collider = was;
			return result;
		}

		public override void Added(Entity entity)
		{
			base.Added(entity);
			startPos = base.Entity.Position;
		}

		public override void EntityRemoved(Scene scene)
		{
			base.EntityRemoved(scene);
			if (Holder != null && Holder != null)
			{
				Holder.Holding = null;
			}
		}

		public bool Pickup(Player player)
		{
			if (cannotHoldTimer > 0f || base.Scene == null || base.Entity.Scene == null)
			{
				return false;
			}
			idleDepth = base.Entity.Depth;
			base.Entity.Depth = player.Depth - 1;
			base.Entity.Visible = true;
			Holder = player;
			if (OnPickup != null)
			{
				OnPickup();
			}
			return true;
		}

		public void Carry(Vector2 position)
		{
			if (OnCarry != null)
			{
				OnCarry(position);
			}
			else
			{
				base.Entity.Position = position;
			}
		}

		public void Release(Vector2 force)
		{
			if (base.Entity.CollideCheck<Solid>())
			{
				if (force.X != 0f)
				{
					bool free = false;
					int sign = Math.Sign(force.X);
					int step = 0;
					while (!free && step++ < 10)
					{
						if (!base.Entity.CollideCheck<Solid>(base.Entity.Position + sign * step * Vector2.UnitX))
						{
							free = true;
						}
					}
					if (free)
					{
						base.Entity.X += sign * step;
					}
				}
				while (base.Entity.CollideCheck<Solid>())
				{
					base.Entity.Position += Vector2.UnitY;
				}
			}
			base.Entity.Depth = idleDepth;
			Holder = null;
			gravityTimer = 0.1f;
			cannotHoldTimer = cannotHoldDelay;
			if (OnRelease != null)
			{
				OnRelease(force);
			}
		}

		public override void Update()
		{
			base.Update();
			if (cannotHoldTimer > 0f)
			{
				cannotHoldTimer -= Engine.DeltaTime;
			}
			if (gravityTimer > 0f)
			{
				gravityTimer -= Engine.DeltaTime;
			}
		}

		public void CheckAgainstColliders()
		{
			foreach (HoldableCollider c in base.Scene.Tracker.GetComponents<HoldableCollider>())
			{
				if (c.Check(this))
				{
					c.OnCollide(this);
				}
			}
		}

		public override void DebugRender(Camera camera)
		{
			base.DebugRender(camera);
			if (PickupCollider != null)
			{
				Collider was = base.Entity.Collider;
				base.Entity.Collider = PickupCollider;
				base.Entity.Collider.Render(camera, Color.Pink);
				base.Entity.Collider = was;
			}
		}

		public bool Dangerous(HoldableCollider hc)
		{
			if (DangerousCheck == null)
			{
				return false;
			}
			return DangerousCheck(hc);
		}

		public void HitSeeker(Seeker seeker)
		{
			if (OnHitSeeker != null)
			{
				OnHitSeeker(seeker);
			}
		}

		public void Swat(HoldableCollider hc, int dir)
		{
			if (OnSwat != null)
			{
				OnSwat(hc, dir);
			}
		}

		public bool HitSpring(Spring spring)
		{
			if (OnHitSpring != null)
			{
				return OnHitSpring(spring);
			}
			return false;
		}

		public void HitSpinner(Entity spinner)
		{
			if (OnHitSpinner != null)
			{
				OnHitSpinner(spinner);
			}
		}

		public Vector2 GetSpeed()
		{
			if (SpeedGetter != null)
			{
				return SpeedGetter();
			}
			return Vector2.Zero;
		}
	}
}
