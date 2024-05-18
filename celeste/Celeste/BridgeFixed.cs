using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class BridgeFixed : Solid
	{
		public BridgeFixed(EntityData data, Vector2 offset)
			: base(data.Position + offset, data.Width, 8f, safe: true)
		{
			MTexture tex = GFX.Game["scenery/bridge_fixed"];
			for (int x = 0; (float)x < base.Width; x += tex.Width)
			{
				Rectangle subrect = new Rectangle(0, 0, tex.Width, tex.Height);
				if ((float)(x + subrect.Width) > base.Width)
				{
					subrect.Width = (int)base.Width - x;
				}
				Add(new Image(tex)
				{
					Position = new Vector2(x, -8f)
				});
			}
		}
	}
}
