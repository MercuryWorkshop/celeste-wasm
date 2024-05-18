using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Clothesline : Entity
	{
		private static readonly Color[] colors = new Color[4]
		{
			Calc.HexToColor("0d2e6b"),
			Calc.HexToColor("3d2688"),
			Calc.HexToColor("4f6e9d"),
			Calc.HexToColor("47194a")
		};

		private static readonly Color lineColor = Color.Lerp(Color.Gray, Color.DarkBlue, 0.25f);

		private static readonly Color pinColor = Color.Gray;

		public Clothesline(Vector2 from, Vector2 to)
		{
			base.Depth = 8999;
			Position = from;
			Add(new Flagline(to, lineColor, pinColor, colors, 8, 20, 8, 16, 2, 8));
		}

		public Clothesline(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Nodes[0] + offset)
		{
		}
	}
}
