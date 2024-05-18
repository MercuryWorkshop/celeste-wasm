using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class GameplayRenderer : Renderer
	{
		public Camera Camera;

		private static GameplayRenderer instance;

		public GameplayRenderer()
		{
			instance = this;
			Camera = new Camera(320, 180);
		}

		public static void Begin()
		{
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, instance.Camera.Matrix);
		}

		public override void Render(Scene scene)
		{
			Begin();
			scene.Entities.RenderExcept(Tags.HUD);
			if (Engine.Commands.Open)
			{
				scene.Entities.DebugRender(Camera);
			}
			End();
		}

		public static void End()
		{
			Draw.SpriteBatch.End();
		}
	}
}
