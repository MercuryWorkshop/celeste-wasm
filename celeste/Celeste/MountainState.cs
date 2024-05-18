using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class MountainState
	{
		public Skybox Skybox;

		public VirtualTexture TerrainTexture;

		public VirtualTexture BuildingsTexture;

		public Color FogColor;

		public MountainState(VirtualTexture terrainTexture, VirtualTexture buildingsTexture, VirtualTexture skyboxTexture, Color fogColor)
		{
			TerrainTexture = terrainTexture;
			BuildingsTexture = buildingsTexture;
			Skybox = new Skybox(skyboxTexture);
			FogColor = fogColor;
		}
	}
}
