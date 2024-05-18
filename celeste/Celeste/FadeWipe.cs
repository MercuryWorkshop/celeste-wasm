using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class FadeWipe : ScreenWipe
	{
		private VertexPositionColor[] vertexBuffer = new VertexPositionColor[6];

		public Action<float> OnUpdate;

		public FadeWipe(Scene scene, bool wipeIn, Action onComplete = null)
			: base(scene, wipeIn, onComplete)
		{
		}

		public override void Update(Scene scene)
		{
			base.Update(scene);
			if (OnUpdate != null)
			{
				OnUpdate(Percent);
			}
		}

		public override void Render(Scene scene)
		{
			Color color = ScreenWipe.WipeColor * (WipeIn ? (1f - Ease.CubeIn(Percent)) : Ease.CubeOut(Percent));
			vertexBuffer[0].Color = color;
			vertexBuffer[0].Position = new Vector3(-10f, -10f, 0f);
			vertexBuffer[1].Color = color;
			vertexBuffer[1].Position = new Vector3(base.Right, -10f, 0f);
			vertexBuffer[2].Color = color;
			vertexBuffer[2].Position = new Vector3(-10f, base.Bottom, 0f);
			vertexBuffer[3].Color = color;
			vertexBuffer[3].Position = new Vector3(base.Right, -10f, 0f);
			vertexBuffer[4].Color = color;
			vertexBuffer[4].Position = new Vector3(base.Right, base.Bottom, 0f);
			vertexBuffer[5].Color = color;
			vertexBuffer[5].Position = new Vector3(-10f, base.Bottom, 0f);
			ScreenWipe.DrawPrimitives(vertexBuffer);
		}
	}
}
