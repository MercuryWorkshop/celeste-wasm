using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Spring : Entity
	{
		public enum Orientations
		{
			Floor,
			WallLeft,
			WallRight
		}

		private Sprite sprite;

		private Wiggler wiggler;

		private StaticMover staticMover;

		public Orientations Orientation;

		private bool playerCanUse;

		public Color DisabledColor = Color.White;

		public bool VisibleWhenDisabled;

		public Spring(Vector2 position, Orientations orientation, bool playerCanUse)
			: base(position)
		{
			Orientation = orientation;
			this.playerCanUse = playerCanUse;
			Add(new PlayerCollider(OnCollide));
			Add(new HoldableCollider(OnHoldable));
			PufferCollider pc = new PufferCollider(OnPuffer);
			Add(pc);
			Add(sprite = new Sprite(GFX.Game, "objects/spring/"));
			sprite.Add("idle", "", 0f, default(int));
			sprite.Add("bounce", "", 0.07f, "idle", 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 4, 5);
			sprite.Add("disabled", "white", 0.07f);
			sprite.Play("idle");
			sprite.Origin.X = sprite.Width / 2f;
			sprite.Origin.Y = sprite.Height;
			base.Depth = -8501;
			staticMover = new StaticMover();
			staticMover.OnAttach = delegate(Platform p)
			{
				base.Depth = p.Depth + 1;
			};
			switch (orientation)
			{
			case Orientations.Floor:
				staticMover.SolidChecker = (Solid s) => CollideCheck(s, Position + Vector2.UnitY);
				staticMover.JumpThruChecker = (JumpThru jt) => CollideCheck(jt, Position + Vector2.UnitY);
				Add(staticMover);
				break;
			case Orientations.WallLeft:
				staticMover.SolidChecker = (Solid s) => CollideCheck(s, Position - Vector2.UnitX);
				staticMover.JumpThruChecker = (JumpThru jt) => CollideCheck(jt, Position - Vector2.UnitX);
				Add(staticMover);
				break;
			case Orientations.WallRight:
				staticMover.SolidChecker = (Solid s) => CollideCheck(s, Position + Vector2.UnitX);
				staticMover.JumpThruChecker = (JumpThru jt) => CollideCheck(jt, Position + Vector2.UnitX);
				Add(staticMover);
				break;
			}
			Add(wiggler = Wiggler.Create(1f, 4f, delegate(float v)
			{
				sprite.Scale.Y = 1f + v * 0.2f;
			}));
			switch (orientation)
			{
			case Orientations.Floor:
				base.Collider = new Hitbox(16f, 6f, -8f, -6f);
				pc.Collider = new Hitbox(16f, 10f, -8f, -10f);
				break;
			case Orientations.WallLeft:
				base.Collider = new Hitbox(6f, 16f, 0f, -8f);
				pc.Collider = new Hitbox(12f, 16f, 0f, -8f);
				sprite.Rotation = (float)Math.PI / 2f;
				break;
			case Orientations.WallRight:
				base.Collider = new Hitbox(6f, 16f, -6f, -8f);
				pc.Collider = new Hitbox(12f, 16f, -12f, -8f);
				sprite.Rotation = -(float)Math.PI / 2f;
				break;
			default:
				throw new Exception("Orientation not supported!");
			}
			staticMover.OnEnable = OnEnable;
			staticMover.OnDisable = OnDisable;
		}

		public Spring(EntityData data, Vector2 offset, Orientations orientation)
			: this(data.Position + offset, orientation, data.Bool("playerCanUse", defaultValue: true))
		{
		}

		private void OnEnable()
		{
			Visible = (Collidable = true);
			sprite.Color = Color.White;
			sprite.Play("idle");
		}

		private void OnDisable()
		{
			Collidable = false;
			if (VisibleWhenDisabled)
			{
				sprite.Play("disabled");
				sprite.Color = DisabledColor;
			}
			else
			{
				Visible = false;
			}
		}

		private void OnCollide(Player player)
		{
			if (player.StateMachine.State == 9 || !playerCanUse)
			{
				return;
			}
			if (Orientation == Orientations.Floor)
			{
				if (player.Speed.Y >= 0f)
				{
					BounceAnimate();
					player.SuperBounce(base.Top);
				}
				return;
			}
			if (Orientation == Orientations.WallLeft)
			{
				if (player.SideBounce(1, base.Right, base.CenterY))
				{
					BounceAnimate();
				}
				return;
			}
			if (Orientation == Orientations.WallRight)
			{
				if (player.SideBounce(-1, base.Left, base.CenterY))
				{
					BounceAnimate();
				}
				return;
			}
			throw new Exception("Orientation not supported!");
		}

		private void BounceAnimate()
		{
			Audio.Play("event:/game/general/spring", base.BottomCenter);
			staticMover.TriggerPlatform();
			sprite.Play("bounce", restart: true);
			wiggler.Start();
		}

		private void OnHoldable(Holdable h)
		{
			if (h.HitSpring(this))
			{
				BounceAnimate();
			}
		}

		private void OnPuffer(Puffer p)
		{
			if (p.HitSpring(this))
			{
				BounceAnimate();
			}
		}

		private void OnSeeker(Seeker seeker)
		{
			if (seeker.Speed.Y >= -120f)
			{
				BounceAnimate();
				seeker.HitSpring();
			}
		}

		public override void Render()
		{
			if (Collidable)
			{
				sprite.DrawOutline();
			}
			base.Render();
		}
	}
}
