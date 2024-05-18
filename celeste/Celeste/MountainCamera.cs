using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public struct MountainCamera
	{
		public Vector3 Position;

		public Vector3 Target;

		public Quaternion Rotation;

		public MountainCamera(Vector3 pos, Vector3 target)
		{
			Position = pos;
			Target = target;
			Rotation = default(Quaternion).LookAt(Position, Target, Vector3.Up);
		}

		public void LookAt(Vector3 pos)
		{
			Target = pos;
			Rotation = default(Quaternion).LookAt(Position, Target, Vector3.Up);
		}
	}
}
