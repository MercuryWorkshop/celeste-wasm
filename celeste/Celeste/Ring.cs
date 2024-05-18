using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class Ring
	{
		public VertexPositionColorTexture[] Verts;

		public VirtualTexture Texture;

		public Color TopColor;

		public Color BotColor;

		public Ring(float top, float bottom, float distance, float wavy, int steps, Color color, VirtualTexture texture, float texScale = 1f)
			: this(top, bottom, distance, wavy, steps, color, color, texture, texScale)
		{
		}

		public Ring(float top, float bottom, float distance, float wavy, int steps, Color topColor, Color botColor, VirtualTexture texture, float texScale = 1f)
		{
			Texture = texture;
			TopColor = topColor;
			BotColor = botColor;
			Verts = new VertexPositionColorTexture[steps * 24];
			float txTop = (1f - texScale) * 0.5f + 0.01f;
			float txBot = 1f - (1f - texScale) * 0.5f;
			for (int i = 0; i < steps; i++)
			{
				float lastPercent = (float)(i - 1) / (float)steps;
				float nextPercent = (float)i / (float)steps;
				Vector2 lastAngle = Calc.AngleToVector(lastPercent * ((float)Math.PI * 2f), distance);
				Vector2 nextAngle = Calc.AngleToVector(nextPercent * ((float)Math.PI * 2f), distance);
				float lastWave = 0f;
				float nextWave = 0f;
				if (wavy > 0f)
				{
					lastWave = (float)Math.Sin(lastPercent * ((float)Math.PI * 2f) * 3f + wavy) * Math.Abs(top - bottom) * 0.4f;
					nextWave = (float)Math.Sin(nextPercent * ((float)Math.PI * 2f) * 3f + wavy) * Math.Abs(top - bottom) * 0.4f;
				}
				int ind = i * 6;
				Verts[ind].Color = topColor;
				Verts[ind].TextureCoordinate = new Vector2(lastPercent * texScale, txTop);
				Verts[ind].Position = new Vector3(lastAngle.X, top + lastWave, lastAngle.Y);
				Verts[ind + 1].Color = topColor;
				Verts[ind + 1].TextureCoordinate = new Vector2(nextPercent * texScale, txTop);
				Verts[ind + 1].Position = new Vector3(nextAngle.X, top + nextWave, nextAngle.Y);
				Verts[ind + 2].Color = botColor;
				Verts[ind + 2].TextureCoordinate = new Vector2(nextPercent * texScale, txBot);
				Verts[ind + 2].Position = new Vector3(nextAngle.X, bottom + nextWave, nextAngle.Y);
				Verts[ind + 3].Color = topColor;
				Verts[ind + 3].TextureCoordinate = new Vector2(lastPercent * texScale, txTop);
				Verts[ind + 3].Position = new Vector3(lastAngle.X, top + lastWave, lastAngle.Y);
				Verts[ind + 4].Color = botColor;
				Verts[ind + 4].TextureCoordinate = new Vector2(nextPercent * texScale, txBot);
				Verts[ind + 4].Position = new Vector3(nextAngle.X, bottom + nextWave, nextAngle.Y);
				Verts[ind + 5].Color = botColor;
				Verts[ind + 5].TextureCoordinate = new Vector2(lastPercent * texScale, txBot);
				Verts[ind + 5].Position = new Vector3(lastAngle.X, bottom + lastWave, lastAngle.Y);
			}
		}

		public void Rotate(float amount)
		{
			for (int i = 0; i < Verts.Length; i++)
			{
				Verts[i].TextureCoordinate.X += amount;
			}
		}

		public void Draw(Matrix matrix, RasterizerState rstate = null, float alpha = 1f)
		{
			Engine.Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			Engine.Graphics.GraphicsDevice.RasterizerState = ((rstate == null) ? MountainModel.CullCCRasterizer : rstate);
			Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
			Engine.Graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
			Engine.Graphics.GraphicsDevice.Textures[0] = Texture.Texture;
			Color tc = TopColor * alpha;
			Color bc = BotColor * alpha;
			for (int i = 0; i < Verts.Length; i += 6)
			{
				Verts[i].Color = tc;
				Verts[i + 1].Color = tc;
				Verts[i + 2].Color = bc;
				Verts[i + 3].Color = tc;
				Verts[i + 4].Color = bc;
				Verts[i + 5].Color = bc;
			}
			GFX.FxTexture.Parameters["World"].SetValue(matrix);
			foreach (EffectPass pass in GFX.FxTexture.CurrentTechnique.Passes)
			{
				pass.Apply();
				Engine.Graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, Verts, 0, Verts.Length / 3);
			}
		}
	}
}
