using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class CameraLocker : Component
	{
		public const float UpwardMaxYOffset = 180f;

		public Level.CameraLockModes LockMode;

		public float MaxXOffset;

		public float MaxYOffset;

		public CameraLocker(Level.CameraLockModes lockMode, float maxXOffset, float maxYOffset)
			: base(lockMode == Level.CameraLockModes.BoostSequence, visible: false)
		{
			LockMode = lockMode;
			MaxXOffset = maxXOffset;
			MaxYOffset = maxYOffset;
		}

		public override void EntityAdded(Scene scene)
		{
			base.EntityAdded(scene);
			SceneAs<Level>().CameraLockMode = LockMode;
		}
	}
}
