using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class _3dSoundTest : Entity
	{
		public SoundSource sfx;

		public _3dSoundTest(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			Add(sfx = new SoundSource());
			sfx.Play("event:/3d_testing");
		}

		public override void Render()
		{
			Draw.Rect(base.X - 8f, base.Y - 8f, 16f, 16f, Color.Yellow);
			Camera cam = (base.Scene as Level).Camera;
			Draw.HollowRect(base.X - 320f, cam.Y, 640f, 180f, Color.Red);
			Draw.HollowRect(base.X - 160f, cam.Y, 320f, 180f, Color.Yellow);
			Draw.HollowRect(base.X - 160f - 320f, cam.Y, 960f, 180f, Color.Yellow);
		}
	}
}
