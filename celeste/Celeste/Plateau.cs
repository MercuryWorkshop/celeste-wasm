using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Plateau : Solid
	{
		private Image sprite;

		public LightOcclude Occluder;

		public Plateau(EntityData e, Vector2 offset)
			: base(e.Position + offset, 104f, 4f, safe: true)
		{
			base.Collider.Left += 8f;
			Add(sprite = new Image(GFX.Game["scenery/fallplateau"]));
			Add(Occluder = new LightOcclude());
			SurfaceSoundIndex = 23;
			EnableAssistModeChecks = false;
		}
	}
}
