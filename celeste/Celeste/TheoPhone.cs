using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class TheoPhone : Entity
	{
		private VertexLight light;

		public TheoPhone(Vector2 position)
			: base(position)
		{
			Add(light = new VertexLight(Color.LawnGreen, 1f, 8, 16));
			Add(new Image(GFX.Game["characters/theo/phone"]).JustifyOrigin(0.5f, 1f));
		}

		public override void Update()
		{
			if (base.Scene.OnInterval(0.5f))
			{
				light.Visible = !light.Visible;
			}
			base.Update();
		}
	}
}
