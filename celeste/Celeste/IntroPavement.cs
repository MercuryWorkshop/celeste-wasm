using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class IntroPavement : Solid
	{
		private int columns;

		public IntroPavement(Vector2 position, int width)
			: base(position, width, 8f, safe: true)
		{
			columns = width / 8;
			base.Depth = -10;
			SurfaceSoundIndex = 1;
			SurfaceSoundPriority = 10;
		}

		public override void Awake(Scene scene)
		{
			for (int i = 0; i < columns; i++)
			{
				int tx = 0;
				tx = ((i >= columns - 2) ? ((i != columns - 2) ? 3 : 2) : Calc.Random.Next(0, 2));
				Image image = new Image(GFX.Game["scenery/car/pavement"].GetSubtexture(tx * 8, 0, 8, 8));
				image.Position = new Vector2(i * 8, 0f);
				Add(image);
			}
		}
	}
}
