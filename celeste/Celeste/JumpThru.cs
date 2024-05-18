using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(true)]
	public class JumpThru : Platform
	{
		public JumpThru(Vector2 position, int width, bool safe)
			: base(position, safe)
		{
			base.Collider = new Hitbox(width, 5f);
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			foreach (StaticMover sm in scene.Tracker.GetComponents<StaticMover>())
			{
				if (sm.IsRiding(this) && sm.Platform == null)
				{
					staticMovers.Add(sm);
					sm.Platform = this;
					if (sm.OnAttach != null)
					{
						sm.OnAttach(this);
					}
				}
			}
		}

		public bool HasRider()
		{
			foreach (Actor entity in base.Scene.Tracker.GetEntities<Actor>())
			{
				if (entity.IsRiding(this))
				{
					return true;
				}
			}
			return false;
		}

		public bool HasPlayerRider()
		{
			foreach (Player entity in base.Scene.Tracker.GetEntities<Player>())
			{
				if (entity.IsRiding(this))
				{
					return true;
				}
			}
			return false;
		}

		public Player GetPlayerRider()
		{
			foreach (Player player in base.Scene.Tracker.GetEntities<Player>())
			{
				if (player.IsRiding(this))
				{
					return player;
				}
			}
			return null;
		}

		public override void MoveHExact(int move)
		{
			if (Collidable)
			{
				foreach (Actor actor in base.Scene.Tracker.GetEntities<Actor>())
				{
					if (actor.IsRiding(this))
					{
						if (actor.TreatNaive)
						{
							actor.NaiveMove(Vector2.UnitX * move);
						}
						else
						{
							actor.MoveHExact(move);
						}
					}
				}
			}
			base.X += move;
			MoveStaticMovers(Vector2.UnitX * move);
		}

		public override void MoveVExact(int move)
		{
			if (Collidable)
			{
				if (move < 0)
				{
					foreach (Actor actor2 in base.Scene.Tracker.GetEntities<Actor>())
					{
						if (actor2.IsRiding(this))
						{
							Collidable = false;
							if (actor2.TreatNaive)
							{
								actor2.NaiveMove(Vector2.UnitY * move);
							}
							else
							{
								actor2.MoveVExact(move);
							}
							actor2.LiftSpeed = LiftSpeed;
							Collidable = true;
						}
						else if (!actor2.TreatNaive && CollideCheck(actor2, Position + Vector2.UnitY * move) && !CollideCheck(actor2))
						{
							Collidable = false;
							actor2.MoveVExact((int)(base.Top + (float)move - actor2.Bottom));
							actor2.LiftSpeed = LiftSpeed;
							Collidable = true;
						}
					}
				}
				else
				{
					foreach (Actor actor in base.Scene.Tracker.GetEntities<Actor>())
					{
						if (actor.IsRiding(this))
						{
							Collidable = false;
							if (actor.TreatNaive)
							{
								actor.NaiveMove(Vector2.UnitY * move);
							}
							else
							{
								actor.MoveVExact(move);
							}
							actor.LiftSpeed = LiftSpeed;
							Collidable = true;
						}
					}
				}
			}
			base.Y += move;
			MoveStaticMovers(Vector2.UnitY * move);
		}
	}
}
