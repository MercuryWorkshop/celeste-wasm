using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class RotateSpinner : Entity
	{
		private const float RotationTime = 1.8f;

		public bool Moving = true;

		private Vector2 center;

		private float rotationPercent;

		private float length;

		private bool fallOutOfScreen;

		public float Angle => MathHelper.Lerp(4.712389f, -(float)Math.PI / 2f, Easer(rotationPercent));

		public bool Clockwise { get; private set; }

		public RotateSpinner(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			base.Depth = -50;
			center = data.Nodes[0] + offset;
			Clockwise = data.Bool("clockwise");
			base.Collider = new Circle(6f);
			Add(new PlayerCollider(OnPlayer));
			Add(new StaticMover
			{
				SolidChecker = (Solid s) => s.CollidePoint(center),
				JumpThruChecker = (JumpThru jt) => jt.CollidePoint(center),
				OnMove = delegate(Vector2 v)
				{
					center += v;
					Position += v;
				},
				OnDestroy = delegate
				{
					fallOutOfScreen = true;
				}
			});
			float angle = Calc.Angle(center, Position);
			angle = Calc.WrapAngle(angle);
			rotationPercent = EaserInverse(Calc.Percent(angle, -(float)Math.PI / 2f, 4.712389f));
			length = (Position - center).Length();
			Position = center + Calc.AngleToVector(Angle, length);
		}

		private float Easer(float v)
		{
			return v;
		}

		private float EaserInverse(float v)
		{
			return v;
		}

		public override void Update()
		{
			base.Update();
			if (Moving)
			{
				if (Clockwise)
				{
					rotationPercent -= Engine.DeltaTime / 1.8f;
					rotationPercent += 1f;
				}
				else
				{
					rotationPercent += Engine.DeltaTime / 1.8f;
				}
				rotationPercent %= 1f;
				Position = center + Calc.AngleToVector(Angle, length);
			}
			if (fallOutOfScreen)
			{
				center.Y += 160f * Engine.DeltaTime;
				if (base.Y > (float)((base.Scene as Level).Bounds.Bottom + 32))
				{
					RemoveSelf();
				}
			}
		}

		public virtual void OnPlayer(Player player)
		{
			if (player.Die((player.Position - Position).SafeNormalize()) != null)
			{
				Moving = false;
			}
		}
	}
}
