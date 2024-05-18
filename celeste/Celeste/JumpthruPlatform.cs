using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class JumpthruPlatform : JumpThru
	{
		private int columns;

		private string overrideTexture;

		private int overrideSoundIndex = -1;

		public JumpthruPlatform(Vector2 position, int width, string overrideTexture, int overrideSoundIndex = -1)
			: base(position, width, safe: true)
		{
			columns = width / 8;
			base.Depth = -60;
			this.overrideTexture = overrideTexture;
			this.overrideSoundIndex = overrideSoundIndex;
		}

		public JumpthruPlatform(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Attr("texture", "default"), data.Int("surfaceIndex", -1))
		{
		}

		public override void Awake(Scene scene)
		{
			string textureName = AreaData.Get(scene).Jumpthru;
			if (!string.IsNullOrEmpty(overrideTexture) && !overrideTexture.Equals("default"))
			{
				textureName = overrideTexture;
			}
			if (overrideSoundIndex > 0)
			{
				SurfaceSoundIndex = overrideSoundIndex;
			}
			else
			{
				switch (textureName.ToLower())
				{
				case "dream":
					SurfaceSoundIndex = 32;
					break;
				case "temple":
				case "templeb":
					SurfaceSoundIndex = 8;
					break;
				case "core":
					SurfaceSoundIndex = 3;
					break;
				default:
					SurfaceSoundIndex = 5;
					break;
				}
			}
			MTexture texture = GFX.Game["objects/jumpthru/" + textureName];
			int textureColumns = texture.Width / 8;
			for (int i = 0; i < columns; i++)
			{
				int tx;
				int ty;
				if (i == 0)
				{
					tx = 0;
					ty = ((!CollideCheck<Solid, SwapBlock, ExitBlock>(Position + new Vector2(-1f, 0f))) ? 1 : 0);
				}
				else if (i == columns - 1)
				{
					tx = textureColumns - 1;
					ty = ((!CollideCheck<Solid, SwapBlock, ExitBlock>(Position + new Vector2(1f, 0f))) ? 1 : 0);
				}
				else
				{
					tx = 1 + Calc.Random.Next(textureColumns - 2);
					ty = Calc.Random.Choose(0, 1);
				}
				Image image = new Image(texture.GetSubtexture(tx * 8, ty * 8, 8, 8));
				image.X = i * 8;
				Add(image);
			}
		}
	}
}
