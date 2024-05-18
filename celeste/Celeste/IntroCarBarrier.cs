using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class IntroCarBarrier : Entity
	{
		public IntroCarBarrier(Vector2 position, int depth, Color color)
		{
			Position = position;
			base.Depth = depth;
			Image image = new Image(GFX.Game["scenery/car/barrier"]);
			image.Origin = new Vector2(0f, image.Height);
			image.Color = color;
			Add(image);
		}
	}
}
