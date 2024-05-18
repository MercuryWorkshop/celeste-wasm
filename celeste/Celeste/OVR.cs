using System.IO;
using Monocle;

namespace Celeste
{
	public static class OVR
	{
		public static Atlas Atlas;

		public static bool Loaded { get; private set; }

		public static void Load()
		{
			Atlas = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Overworld"), Atlas.AtlasDataFormat.PackerNoAtlas);
			Loaded = true;
		}

		public static void Unload()
		{
			Atlas.Dispose();
			Atlas = null;
			Loaded = false;
		}
	}
}
