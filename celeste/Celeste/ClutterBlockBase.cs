using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class ClutterBlockBase : Solid
	{
		private static readonly Color enabledColor = Color.Black * 0.7f;

		private static readonly Color disabledColor = Color.Black * 0.3f;

		public ClutterBlock.Colors BlockColor;

		private Color color;

		private bool enabled;

		private LightOcclude occluder;

		public ClutterBlockBase(Vector2 position, int width, int height, bool enabled, ClutterBlock.Colors blockColor)
			: base(position, width, height, safe: true)
		{
			EnableAssistModeChecks = false;
			BlockColor = blockColor;
			base.Depth = 8999;
			this.enabled = enabled;
			color = (enabled ? enabledColor : disabledColor);
			if (enabled)
			{
				Add(occluder = new LightOcclude());
			}
			else
			{
				Collidable = false;
			}
			switch (blockColor)
			{
			case ClutterBlock.Colors.Green:
				SurfaceSoundIndex = 19;
				break;
			case ClutterBlock.Colors.Red:
				SurfaceSoundIndex = 17;
				break;
			case ClutterBlock.Colors.Yellow:
				SurfaceSoundIndex = 18;
				break;
			}
		}

		public void Deactivate()
		{
			Collidable = false;
			color = disabledColor;
			enabled = false;
			if (occluder != null)
			{
				Remove(occluder);
				occluder = null;
			}
		}

		public override void Render()
		{
			Draw.Rect(base.X, base.Y, base.Width, base.Height + (float)(enabled ? 2 : 0), color);
		}
	}
}
