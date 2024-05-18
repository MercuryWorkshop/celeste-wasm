using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(true)]
	public class Solid : Platform
	{
		public Vector2 Speed;

		public bool AllowStaticMovers = true;

		public bool EnableAssistModeChecks = true;

		public bool DisableLightsInside = true;

		public bool StopPlayerRunIntoAnimation = true;

		public bool SquishEvenInAssistMode;

		private static HashSet<Actor> riders = new HashSet<Actor>();

		public Solid(Vector2 position, float width, float height, bool safe)
			: base(position, safe)
		{
			base.Collider = new Hitbox(width, height);
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (!AllowStaticMovers)
			{
				return;
			}
			bool was = Collidable;
			Collidable = true;
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
			Collidable = was;
		}

		public override void Update()
		{
			base.Update();
			MoveH(Speed.X * Engine.DeltaTime);
			MoveV(Speed.Y * Engine.DeltaTime);
			if (!EnableAssistModeChecks || SaveData.Instance == null || !SaveData.Instance.Assists.Invincible || base.Components.Get<SolidOnInvinciblePlayer>() != null || !Collidable)
			{
				return;
			}
			Player player = CollideFirst<Player>();
			Level level = base.Scene as Level;
			if (player == null && base.Bottom > (float)level.Bounds.Bottom)
			{
				player = CollideFirst<Player>(Position + Vector2.UnitY);
			}
			if (player != null && player.StateMachine.State != 9 && player.StateMachine.State != 21)
			{
				Add(new SolidOnInvinciblePlayer());
				return;
			}
			TheoCrystal theo = CollideFirst<TheoCrystal>();
			if (theo != null && !theo.Hold.IsHeld)
			{
				Add(new SolidOnInvinciblePlayer());
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

		public bool HasPlayerRider()
		{
			return GetPlayerRider() != null;
		}

		public bool HasPlayerOnTop()
		{
			return GetPlayerOnTop() != null;
		}

		public Player GetPlayerOnTop()
		{
			return CollideFirst<Player>(Position - Vector2.UnitY);
		}

		public bool HasPlayerClimbing()
		{
			return GetPlayerClimbing() != null;
		}

		public Player GetPlayerClimbing()
		{
			foreach (Player player in base.Scene.Tracker.GetEntities<Player>())
			{
				if (player.StateMachine.State == 1)
				{
					if (player.Facing == Facings.Left && CollideCheck(player, Position + Vector2.UnitX))
					{
						return player;
					}
					if (player.Facing == Facings.Right && CollideCheck(player, Position - Vector2.UnitX))
					{
						return player;
					}
				}
			}
			return null;
		}

		public void GetRiders()
		{
			foreach (Actor actor in base.Scene.Tracker.GetEntities<Actor>())
			{
				if (actor.IsRiding(this))
				{
					riders.Add(actor);
				}
			}
		}

		public override void MoveHExact(int move)
		{
			GetRiders();
			float oldRight = base.Right;
			float oldLeft = base.Left;
			Player snap = null;
			snap = base.Scene.Tracker.GetEntity<Player>();
			if (snap != null && Input.MoveX.Value == Math.Sign(move) && Math.Sign(snap.Speed.X) == Math.Sign(move) && !riders.Contains(snap) && CollideCheck(snap, Position + Vector2.UnitX * move - Vector2.UnitY))
			{
				snap.MoveV(1f);
			}
			base.X += move;
			MoveStaticMovers(Vector2.UnitX * move);
			if (Collidable)
			{
				foreach (Actor actor in base.Scene.Tracker.GetEntities<Actor>())
				{
					if (!actor.AllowPushing)
					{
						continue;
					}
					bool was = actor.Collidable;
					actor.Collidable = true;
					if (!actor.TreatNaive && CollideCheck(actor, Position))
					{
						int push = ((move <= 0) ? (move - (int)(actor.Right - oldLeft)) : (move - (int)(actor.Left - oldRight)));
						Collidable = false;
						actor.MoveHExact(push, actor.SquishCallback, this);
						actor.LiftSpeed = LiftSpeed;
						Collidable = true;
					}
					else if (riders.Contains(actor))
					{
						Collidable = false;
						if (actor.TreatNaive)
						{
							actor.NaiveMove(Vector2.UnitX * move);
						}
						else
						{
							actor.MoveHExact(move);
						}
						actor.LiftSpeed = LiftSpeed;
						Collidable = true;
					}
					actor.Collidable = was;
				}
			}
			riders.Clear();
		}

		public override void MoveVExact(int move)
		{
			GetRiders();
			float oldBottom = base.Bottom;
			float oldTop = base.Top;
			base.Y += move;
			MoveStaticMovers(Vector2.UnitY * move);
			if (Collidable)
			{
				foreach (Actor actor in base.Scene.Tracker.GetEntities<Actor>())
				{
					if (!actor.AllowPushing)
					{
						continue;
					}
					bool was = actor.Collidable;
					actor.Collidable = true;
					if (!actor.TreatNaive && CollideCheck(actor, Position))
					{
						int push = ((move <= 0) ? (move - (int)(actor.Bottom - oldTop)) : (move - (int)(actor.Top - oldBottom)));
						Collidable = false;
						actor.MoveVExact(push, actor.SquishCallback, this);
						actor.LiftSpeed = LiftSpeed;
						Collidable = true;
					}
					else if (riders.Contains(actor))
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
					actor.Collidable = was;
				}
			}
			riders.Clear();
		}
	}
}
