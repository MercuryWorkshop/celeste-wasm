using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class PropLight : Entity
	{
		public PropLight(Vector2 position, Color color, float alpha)
			: base(position)
		{
			Add(new VertexLight(color, alpha, 128, 256));
		}

		public PropLight(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.HexColor("color"), data.Float("alpha"))
		{
		}
	}
}
