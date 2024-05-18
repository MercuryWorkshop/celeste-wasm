using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class RotatingPlatform : JumpThru
	{
		private const float RotateSpeed = (float)Math.PI / 3f;

		private Vector2 center;

		private bool clockwise;

		private float length;

		private float currentAngle;

		public RotatingPlatform(Vector2 position, int width, Vector2 center, bool clockwise)
			: base(position, width, safe: false)
		{
			base.Collider.Position.X = -width / 2;
			base.Collider.Position.Y = (0f - base.Height) / 2f;
			this.center = center;
			this.clockwise = clockwise;
			length = (position - center).Length();
			currentAngle = (position - center).Angle();
			SurfaceSoundIndex = 5;
			Add(new LightOcclude(0.2f));
		}

		public override void Update()
		{
			base.Update();
			if (clockwise)
			{
				currentAngle -= (float)Math.PI / 3f * Engine.DeltaTime;
			}
			else
			{
				currentAngle += (float)Math.PI / 3f * Engine.DeltaTime;
			}
			currentAngle = Calc.WrapAngle(currentAngle);
			MoveTo(center + Calc.AngleToVector(currentAngle, length));
		}

		public override void Render()
		{
			base.Render();
			Draw.Rect(base.Collider, Color.White);
		}
	}
}
