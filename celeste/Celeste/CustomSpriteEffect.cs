using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste
{
	public class CustomSpriteEffect : Effect
	{
		private EffectParameter matrixParam;

		public CustomSpriteEffect(Effect effect)
			: base(effect)
		{
			matrixParam = base.Parameters["MatrixTransform"];
		}

		protected override void OnApply()
		{
			Viewport viewport = base.GraphicsDevice.Viewport;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0f, viewport.Width, viewport.Height, 0f, 0f, 1f);
			matrixParam.SetValue(projection);
			base.OnApply();
		}
	}
}
