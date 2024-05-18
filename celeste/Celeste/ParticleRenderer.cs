using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class ParticleRenderer : Renderer
	{
		public List<ParticleSystem> Systems;

		public ParticleRenderer(params ParticleSystem[] system)
		{
			Systems = new List<ParticleSystem>();
			Systems.AddRange(system);
		}

		public override void Update(Scene scene)
		{
			foreach (ParticleSystem system in Systems)
			{
				system.Update();
			}
			base.Update(scene);
		}

		public override void Render(Scene scene)
		{
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Engine.ScreenMatrix);
			foreach (ParticleSystem system in Systems)
			{
				system.Render();
			}
			Draw.SpriteBatch.End();
		}
	}
}
