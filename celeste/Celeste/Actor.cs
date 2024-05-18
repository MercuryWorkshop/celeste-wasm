using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(true)]
	public class Actor : Entity
	{
		public Collision SquishCallback;

		public bool TreatNaive;

		private Vector2 movementCounter;

		public bool IgnoreJumpThrus;

		public bool AllowPushing = true;

		public float LiftSpeedGraceTime = 0.16f;

		private Vector2 currentLiftSpeed;

		private Vector2 lastLiftSpeed;

		private float liftSpeedTimer;

		public Vector2 ExactPosition => Position + movementCounter;

		public Vector2 PositionRemainder => movementCounter;

		public Vector2 LiftSpeed
		{
			get
			{
				if (currentLiftSpeed == Vector2.Zero)
				{
					return lastLiftSpeed;
				}
				return currentLiftSpeed;
			}
			set
			{
				currentLiftSpeed = value;
				if (value != Vector2.Zero && LiftSpeedGraceTime > 0f)
				{
					lastLiftSpeed = value;
					liftSpeedTimer = LiftSpeedGraceTime;
				}
			}
		}

		public Actor(Vector2 position)
			: base(position)
		{
			SquishCallback = OnSquish;
		}

		protected virtual void OnSquish(CollisionData data)
		{
			if (!TrySquishWiggle(data))
			{
				RemoveSelf();
			}
		}

		protected bool TrySquishWiggle(CollisionData data, int wiggleX = 3, int wiggleY = 3)
		{
			data.Pusher.Collidable = true;
			for (int x2 = 0; x2 <= wiggleX; x2++)
			{
				for (int y = 0; y <= wiggleY; y++)
				{
					if (x2 == 0 && y == 0)
					{
						continue;
					}
					for (int xMult = 1; xMult >= -1; xMult -= 2)
					{
						for (int yMult = 1; yMult >= -1; yMult -= 2)
						{
							Vector2 add = new Vector2(x2 * xMult, y * yMult);
							if (!CollideCheck<Solid>(Position + add))
							{
								Position += add;
								data.Pusher.Collidable = false;
								return true;
							}
						}
					}
				}
			}
			for (int x = 0; x <= wiggleX; x++)
			{
				for (int y2 = 0; y2 <= wiggleY; y2++)
				{
					if (x == 0 && y2 == 0)
					{
						continue;
					}
					for (int xMult2 = 1; xMult2 >= -1; xMult2 -= 2)
					{
						for (int yMult2 = 1; yMult2 >= -1; yMult2 -= 2)
						{
							Vector2 add2 = new Vector2(x * xMult2, y2 * yMult2);
							if (!CollideCheck<Solid>(data.TargetPosition + add2))
							{
								Position = data.TargetPosition + add2;
								data.Pusher.Collidable = false;
								return true;
							}
						}
					}
				}
			}
			data.Pusher.Collidable = false;
			return false;
		}

		public virtual bool IsRiding(JumpThru jumpThru)
		{
			if (IgnoreJumpThrus)
			{
				return false;
			}
			return CollideCheckOutside(jumpThru, Position + Vector2.UnitY);
		}

		public virtual bool IsRiding(Solid solid)
		{
			return CollideCheck(solid, Position + Vector2.UnitY);
		}

		public bool OnGround(int downCheck = 1)
		{
			if (!CollideCheck<Solid>(Position + Vector2.UnitY * downCheck))
			{
				if (!IgnoreJumpThrus)
				{
					return CollideCheckOutside<JumpThru>(Position + Vector2.UnitY * downCheck);
				}
				return false;
			}
			return true;
		}

		public bool OnGround(Vector2 at, int downCheck = 1)
		{
			Vector2 was = Position;
			Position = at;
			bool result = OnGround(downCheck);
			Position = was;
			return result;
		}

		public void ZeroRemainderX()
		{
			movementCounter.X = 0f;
		}

		public void ZeroRemainderY()
		{
			movementCounter.Y = 0f;
		}

		public override void Update()
		{
			base.Update();
			LiftSpeed = Vector2.Zero;
			if (liftSpeedTimer > 0f)
			{
				liftSpeedTimer -= Engine.DeltaTime;
				if (liftSpeedTimer <= 0f)
				{
					lastLiftSpeed = Vector2.Zero;
				}
			}
		}

		public void ResetLiftSpeed()
		{
			currentLiftSpeed = (lastLiftSpeed = Vector2.Zero);
			liftSpeedTimer = 0f;
		}

		public bool MoveH(float moveH, Collision onCollide = null, Solid pusher = null)
		{
			movementCounter.X += moveH;
			int move = (int)Math.Round(movementCounter.X, MidpointRounding.ToEven);
			if (move != 0)
			{
				movementCounter.X -= move;
				return MoveHExact(move, onCollide, pusher);
			}
			return false;
		}

		public bool MoveV(float moveV, Collision onCollide = null, Solid pusher = null)
		{
			movementCounter.Y += moveV;
			int move = (int)Math.Round(movementCounter.Y, MidpointRounding.ToEven);
			if (move != 0)
			{
				movementCounter.Y -= move;
				return MoveVExact(move, onCollide, pusher);
			}
			return false;
		}

		public bool MoveHExact(int moveH, Collision onCollide = null, Solid pusher = null)
		{
			Vector2 target = Position + Vector2.UnitX * moveH;
			int sign = Math.Sign(moveH);
			int moved = 0;
			while (moveH != 0)
			{
				Solid hit = CollideFirst<Solid>(Position + Vector2.UnitX * sign);
				if (hit != null)
				{
					movementCounter.X = 0f;
					onCollide?.Invoke(new CollisionData
					{
						Direction = Vector2.UnitX * sign,
						Moved = Vector2.UnitX * moved,
						TargetPosition = target,
						Hit = hit,
						Pusher = pusher
					});
					return true;
				}
				moved += sign;
				moveH -= sign;
				base.X += sign;
			}
			return false;
		}

		public bool MoveVExact(int moveV, Collision onCollide = null, Solid pusher = null)
		{
			Vector2 target = Position + Vector2.UnitY * moveV;
			int sign = Math.Sign(moveV);
			int moved = 0;
			while (moveV != 0)
			{
				Platform hit = CollideFirst<Solid>(Position + Vector2.UnitY * sign);
				if (hit != null)
				{
					movementCounter.Y = 0f;
					onCollide?.Invoke(new CollisionData
					{
						Direction = Vector2.UnitY * sign,
						Moved = Vector2.UnitY * moved,
						TargetPosition = target,
						Hit = hit,
						Pusher = pusher
					});
					return true;
				}
				if (moveV > 0 && !IgnoreJumpThrus)
				{
					hit = CollideFirstOutside<JumpThru>(Position + Vector2.UnitY * sign);
					if (hit != null)
					{
						movementCounter.Y = 0f;
						onCollide?.Invoke(new CollisionData
						{
							Direction = Vector2.UnitY * sign,
							Moved = Vector2.UnitY * moved,
							TargetPosition = target,
							Hit = hit,
							Pusher = pusher
						});
						return true;
					}
				}
				moved += sign;
				moveV -= sign;
				base.Y += sign;
			}
			return false;
		}

		public void MoveTowardsX(float targetX, float maxAmount, Collision onCollide = null)
		{
			float moveTo = Calc.Approach(ExactPosition.X, targetX, maxAmount);
			MoveToX(moveTo, onCollide);
		}

		public void MoveTowardsY(float targetY, float maxAmount, Collision onCollide = null)
		{
			float moveTo = Calc.Approach(ExactPosition.Y, targetY, maxAmount);
			MoveToY(moveTo, onCollide);
		}

		public void MoveToX(float toX, Collision onCollide = null)
		{
			MoveH(toX - ExactPosition.X, onCollide);
		}

		public void MoveToY(float toY, Collision onCollide = null)
		{
			MoveV(toY - ExactPosition.Y, onCollide);
		}

		public void NaiveMove(Vector2 amount)
		{
			movementCounter += amount;
			int moveX = (int)Math.Round(movementCounter.X);
			int moveY = (int)Math.Round(movementCounter.Y);
			Position += new Vector2(moveX, moveY);
			movementCounter -= new Vector2(moveX, moveY);
		}
	}
}
