using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class Skybox
	{
		public VertexPositionColorTexture[] Verts;

		public VirtualTexture Texture;

		public Skybox(VirtualTexture texture, float size = 25f)
		{
			Texture = texture;
			Verts = new VertexPositionColorTexture[30];
			Vector3 at = new Vector3(0f - size, size, 0f - size);
			Vector3 bt = new Vector3(size, size, 0f - size);
			Vector3 ct = new Vector3(size, size, size);
			Vector3 dt = new Vector3(0f - size, size, size);
			Vector3 ab = new Vector3(0f - size, 0f - size, 0f - size);
			Vector3 bb = new Vector3(size, 0f - size, 0f - size);
			Vector3 cb = new Vector3(size, 0f - size, size);
			Vector3 db = new Vector3(0f - size, 0f - size, size);
			MTexture mTexture = new MTexture(texture);
			MTexture up = mTexture.GetSubtexture(0, 0, 820, 820);
			MTexture south = mTexture.GetSubtexture(820, 0, 820, 820);
			MTexture north = mTexture.GetSubtexture(2460, 0, 820, 820);
			MTexture east = mTexture.GetSubtexture(1640, 0, 820, 820);
			MTexture west = mTexture.GetSubtexture(3280, 0, 819, 820);
			AddFace(Verts, 0, up, at, bt, ct, dt);
			AddFace(Verts, 1, north, bt, at, ab, bb);
			AddFace(Verts, 2, south, dt, ct, cb, db);
			AddFace(Verts, 3, west, ct, bt, bb, cb);
			AddFace(Verts, 4, east, at, dt, db, ab);
		}

		private void AddFace(VertexPositionColorTexture[] verts, int face, MTexture tex, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
		{
			float uvLeft = (float)(tex.ClipRect.Left + 1) / (float)tex.Texture.Width;
			float uvTop = (float)(tex.ClipRect.Top + 1) / (float)tex.Texture.Height;
			float uvRight = (float)(tex.ClipRect.Right - 1) / (float)tex.Texture.Width;
			float uvBottom = (float)(tex.ClipRect.Bottom - 1) / (float)tex.Texture.Height;
			int i = face * 6;
			verts[i++] = new VertexPositionColorTexture(a, Color.White, new Vector2(uvLeft, uvTop));
			verts[i++] = new VertexPositionColorTexture(b, Color.White, new Vector2(uvRight, uvTop));
			verts[i++] = new VertexPositionColorTexture(c, Color.White, new Vector2(uvRight, uvBottom));
			verts[i++] = new VertexPositionColorTexture(a, Color.White, new Vector2(uvLeft, uvTop));
			verts[i++] = new VertexPositionColorTexture(c, Color.White, new Vector2(uvRight, uvBottom));
			verts[i++] = new VertexPositionColorTexture(d, Color.White, new Vector2(uvLeft, uvBottom));
		}

		public void Draw(Matrix matrix, Color color)
		{
			Engine.Graphics.GraphicsDevice.RasterizerState = MountainModel.CullNoneRasterizer;
			Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
			Engine.Graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
			Engine.Graphics.GraphicsDevice.Textures[0] = Texture.Texture;
			for (int i = 0; i < Verts.Length; i++)
			{
				Verts[i].Color = color;
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
