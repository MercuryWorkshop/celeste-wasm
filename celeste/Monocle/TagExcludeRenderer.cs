using Microsoft.Xna.Framework.Graphics;

namespace Monocle
{
	public class TagExcludeRenderer : Renderer
	{
		public BlendState BlendState;

		public SamplerState SamplerState;

		public Effect Effect;

		public Camera Camera;

		public int ExcludeTag;

		public TagExcludeRenderer(int excludeTag)
		{
			ExcludeTag = excludeTag;
			BlendState = BlendState.AlphaBlend;
			SamplerState = SamplerState.LinearClamp;
			Camera = new Camera();
		}

		public override void BeforeRender(Scene scene)
		{
		}

		public override void Render(Scene scene)
		{
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState, SamplerState, DepthStencilState.None, RasterizerState.CullNone, Effect, Camera.Matrix * Engine.ScreenMatrix);
			foreach (Entity entity2 in scene.Entities)
			{
				if (entity2.Visible && (entity2.Tag & ExcludeTag) == 0)
				{
					entity2.Render();
				}
			}
			if (Engine.Commands.Open)
			{
				foreach (Entity entity in scene.Entities)
				{
					if ((entity.Tag & ExcludeTag) == 0)
					{
						entity.DebugRender(Camera);
					}
				}
			}
			Draw.SpriteBatch.End();
		}

		public override void AfterRender(Scene scene)
		{
		}
	}
}
