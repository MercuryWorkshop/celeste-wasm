using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Lamp : Entity
	{
		private Image sprite;

		public Lamp(Vector2 position, bool broken)
		{
			Position = position;
			base.Depth = 5;
			Add(sprite = new Image(GFX.Game["scenery/lamp"].GetSubtexture(broken ? 16 : 0, 0, 16, 80)));
			sprite.Origin = new Vector2(sprite.Width / 2f, sprite.Height);
			if (!broken)
			{
				Add(new BloomPoint(new Vector2(0f, -66f), 1f, 16f));
			}
		}
	}
}
