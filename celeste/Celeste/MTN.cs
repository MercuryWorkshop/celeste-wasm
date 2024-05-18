using System;
using System.Diagnostics;
using System.IO;
using Monocle;

namespace Celeste
{
	public static class MTN
	{
		public static Atlas FileSelect;

		public static Atlas Journal;

		public static Atlas Mountain;

		public static Atlas Checkpoints;

		public static ObjModel MountainTerrain;

		public static ObjModel MountainBuildings;

		public static ObjModel MountainCoreWall;

		public static ObjModel MountainMoon;

		public static ObjModel MountainBird;

		public static VirtualTexture[] MountainTerrainTextures;

		public static VirtualTexture[] MountainBuildingTextures;

		public static VirtualTexture[] MountainSkyboxTextures;

		public static VirtualTexture MountainFogTexture;

		public static VirtualTexture MountainMoonTexture;

		public static VirtualTexture MountainStarSky;

		public static VirtualTexture MountainStars;

		public static VirtualTexture MountainStarStream;

		public static bool Loaded { get; private set; }

		public static bool DataLoaded { get; private set; }

		public static void Load()
		{
			if (!Loaded)
			{
				Stopwatch t = Stopwatch.StartNew();
				FileSelect = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "FileSelect"), Atlas.AtlasDataFormat.Packer);
				Journal = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Journal"), Atlas.AtlasDataFormat.Packer);
				Mountain = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Mountain"), Atlas.AtlasDataFormat.PackerNoAtlas);
				Checkpoints = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Checkpoints"), Atlas.AtlasDataFormat.PackerNoAtlas);
				MountainTerrainTextures = new VirtualTexture[3];
				MountainBuildingTextures = new VirtualTexture[3];
				MountainSkyboxTextures = new VirtualTexture[3];
				for (int i = 0; i < 3; i++)
				{
					MountainSkyboxTextures[i] = Mountain["skybox_" + i].Texture;
					MountainTerrainTextures[i] = Mountain["mountain_" + i].Texture;
					MountainBuildingTextures[i] = Mountain["buildings_" + i].Texture;
				}
				MountainMoonTexture = Mountain["moon"].Texture;
				MountainFogTexture = Mountain["fog"].Texture;
				MountainStarSky = Mountain["space"].Texture;
				MountainStars = Mountain["spacestars"].Texture;
				MountainStarStream = Mountain["starstream"].Texture;
				Console.WriteLine(" - MTN LOAD: " + t.ElapsedMilliseconds + "ms");
			}
			Loaded = true;
		}

		public static void LoadData()
		{
			if (!DataLoaded)
			{
				Stopwatch t = Stopwatch.StartNew();
				string ext = ".obj";
				MountainTerrain = ObjModel.Create(Path.Combine(Engine.ContentDirectory, "Overworld", "mountain" + ext));
				MountainBuildings = ObjModel.Create(Path.Combine(Engine.ContentDirectory, "Overworld", "buildings" + ext));
				MountainCoreWall = ObjModel.Create(Path.Combine(Engine.ContentDirectory, "Overworld", "mountain_wall" + ext));
				MountainMoon = ObjModel.Create(Path.Combine(Engine.ContentDirectory, "Overworld", "moon" + ext));
				MountainBird = ObjModel.Create(Path.Combine(Engine.ContentDirectory, "Overworld", "bird" + ext));
				Console.WriteLine(" - MTN DATA LOAD: " + t.ElapsedMilliseconds + "ms");
			}
			DataLoaded = true;
		}

		public static void Unload()
		{
			if (Loaded)
			{
				Journal.Dispose();
				Journal = null;
				Mountain.Dispose();
				Mountain = null;
				Checkpoints.Dispose();
				Checkpoints = null;
				FileSelect.Dispose();
				FileSelect = null;
			}
			Loaded = false;
		}

		public static void UnloadData()
		{
			if (DataLoaded)
			{
				MountainTerrain.Dispose();
				MountainTerrain = null;
				MountainBuildings.Dispose();
				MountainBuildings = null;
				MountainCoreWall.Dispose();
				MountainCoreWall = null;
				MountainMoon.Dispose();
				MountainMoon = null;
				MountainBird.Dispose();
				MountainBird = null;
			}
			DataLoaded = false;
		}
	}
}
