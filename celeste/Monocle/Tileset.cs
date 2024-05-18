namespace Monocle
{
	public class Tileset
	{
		private MTexture[,] tiles;

		public MTexture Texture { get; private set; }

		public int TileWidth { get; private set; }

		public int TileHeight { get; private set; }

		public MTexture this[int x, int y] => tiles[x, y];

		public MTexture this[int index]
		{
			get
			{
				if (index < 0)
				{
					return null;
				}
				return tiles[index % tiles.GetLength(0), index / tiles.GetLength(0)];
			}
		}

		public Tileset(MTexture texture, int tileWidth, int tileHeight)
		{
			Texture = texture;
			TileWidth = tileWidth;
			TileHeight = TileHeight;
			tiles = new MTexture[Texture.Width / tileWidth, Texture.Height / tileHeight];
			for (int x = 0; x < Texture.Width / tileWidth; x++)
			{
				for (int y = 0; y < Texture.Height / tileHeight; y++)
				{
					tiles[x, y] = new MTexture(Texture, x * tileWidth, y * tileHeight, tileWidth, tileHeight);
				}
			}
		}
	}
}
