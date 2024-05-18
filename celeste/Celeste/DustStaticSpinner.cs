using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class DustStaticSpinner : Entity
	{
		public static ParticleType P_Move;

		public const float ParticleInterval = 0.02f;

		public DustGraphic Sprite;

		private float offset = Calc.Random.NextFloat();

		public DustStaticSpinner(Vector2 position, bool attachToSolid, bool ignoreSolids = false)
			: base(position)
		{
			base.Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
			Add(new PlayerCollider(OnPlayer));
			Add(new HoldableCollider(OnHoldable));
			Add(new LedgeBlocker());
			Add(Sprite = new DustGraphic(ignoreSolids, autoControlEyes: true, autoExpandDust: true));
			base.Depth = -50;
			if (attachToSolid)
			{
				Add(new StaticMover
				{
					OnShake = OnShake,
					SolidChecker = IsRiding
				});
			}
		}

		public DustStaticSpinner(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Bool("attachToSolid"))
		{
		}

		public void ForceInstantiate()
		{
			Sprite.AddDustNodesIfInCamera();
		}

		public override void Update()
		{
			base.Update();
			if (base.Scene.OnInterval(0.05f, offset) && Sprite.Estableshed)
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null)
				{
					Collidable = Math.Abs(player.X - base.X) < 128f && Math.Abs(player.Y - base.Y) < 128f;
				}
			}
		}

		private void OnShake(Vector2 pos)
		{
			Sprite.Position = pos;
		}

		private bool IsRiding(Solid solid)
		{
			return CollideCheck(solid);
		}

		private void OnPlayer(Player player)
		{
			player.Die((player.Position - Position).SafeNormalize());
			Sprite.OnHitPlayer();
		}

		private void OnHoldable(Holdable h)
		{
			h.HitSpinner(this);
		}
	}
}
