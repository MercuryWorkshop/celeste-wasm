using Microsoft.Xna.Framework;

namespace Celeste
{
	public class CameraOffsetTrigger : Trigger
	{
		public Vector2 CameraOffset;

		public CameraOffsetTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			CameraOffset = new Vector2(data.Float("cameraX"), data.Float("cameraY"));
			CameraOffset.X *= 48f;
			CameraOffset.Y *= 32f;
		}

		public override void OnEnter(Player player)
		{
			SceneAs<Level>().CameraOffset = CameraOffset;
		}
	}
}
