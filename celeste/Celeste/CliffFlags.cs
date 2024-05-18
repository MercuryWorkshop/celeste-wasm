using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CliffFlags : Entity
	{
		private static readonly Color[] colors = new Color[4]
		{
			Calc.HexToColor("d85f2f"),
			Calc.HexToColor("d82f63"),
			Calc.HexToColor("2fd8a2"),
			Calc.HexToColor("d8d62f")
		};

		private static readonly Color lineColor = Color.Lerp(Color.Gray, Color.DarkBlue, 0.25f);

		private static readonly Color pinColor = Color.Gray;

		public CliffFlags(Vector2 from, Vector2 to)
		{
			base.Depth = 8999;
			Position = from;
			Flagline line;
			Add(line = new Flagline(to, lineColor, pinColor, colors, 10, 10, 10, 10, 2, 8));
			line.ClothDroopAmount = 0.2f;
		}

		public CliffFlags(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Nodes[0] + offset)
		{
		}
	}
}
