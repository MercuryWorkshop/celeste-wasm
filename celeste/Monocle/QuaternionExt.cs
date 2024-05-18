using Microsoft.Xna.Framework;

namespace Monocle
{
	public static class QuaternionExt
	{
		public static Quaternion Conjugated(this Quaternion q)
		{
			Quaternion c = q;
			c.Conjugate();
			return c;
		}

		public static Quaternion LookAt(this Quaternion q, Vector3 from, Vector3 to, Vector3 up)
		{
			return Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(from, to, up));
		}

		public static Quaternion LookAt(this Quaternion q, Vector3 direction, Vector3 up)
		{
			return Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.Zero, direction, up));
		}

		public static Vector3 Forward(this Quaternion q)
		{
			return Vector3.Transform(Vector3.Forward, q.Conjugated());
		}

		public static Vector3 Left(this Quaternion q)
		{
			return Vector3.Transform(Vector3.Left, q.Conjugated());
		}

		public static Vector3 Up(this Quaternion q)
		{
			return Vector3.Transform(Vector3.Up, q.Conjugated());
		}
	}
}
