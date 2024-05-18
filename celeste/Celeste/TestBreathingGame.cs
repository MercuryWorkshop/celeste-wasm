using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class TestBreathingGame : Scene
	{
		private BreathingMinigame game;

		public TestBreathingGame()
		{
			game = new BreathingMinigame();
			Add(game);
		}

		public override void BeforeRender()
		{
			game.BeforeRender();
			base.BeforeRender();
		}

		public override void Render()
		{
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);
			game.Render();
			Draw.SpriteBatch.End();
		}
	}
}
