using System.Collections.Generic;
using Monocle;

namespace Celeste
{
	public class SurfaceIndex
	{
		public const string Param = "surface_index";

		public const int Asphalt = 1;

		public const int Car = 2;

		public const int Dirt = 3;

		public const int Snow = 4;

		public const int Wood = 5;

		public const int Girder = 7;

		public const int Brick = 8;

		public const int ZipMover = 9;

		public const int ResortWood = 13;

		public const int DreamBlockInactive = 11;

		public const int DreamBlockActive = 12;

		public const int ResortRoof = 14;

		public const int ResortSinkingPlatforms = 15;

		public const int ResortLinens = 17;

		public const int ResortBoxes = 18;

		public const int ResortBooks = 19;

		public const int ClutterDoor = 20;

		public const int ClutterSwitch = 21;

		public const int ResortElevator = 22;

		public const int CliffsideSnow = 23;

		public const int CliffsideGrass = 25;

		public const int CliffsideWhiteBlock = 27;

		public const int Gondola = 28;

		public const int AuroraGlass = 32;

		public const int Grass = 33;

		public const int CassetteBlock = 35;

		public const int CoreIce = 36;

		public const int CoreMoltenRock = 37;

		public const int Glitch = 40;

		public const int MoonCafe = 42;

		public const int DreamClouds = 43;

		public const int Moon = 44;

		public const int StoneBridge = 6;

		public const int ResortBasementTile = 16;

		public const int ResortMagicButton = 21;

		public static Dictionary<char, int> TileToIndex = new Dictionary<char, int>
		{
			{ '1', 3 },
			{ '3', 4 },
			{ '4', 7 },
			{ '5', 8 },
			{ '6', 8 },
			{ '7', 8 },
			{ '8', 8 },
			{ '9', 13 },
			{ 'a', 8 },
			{ 'b', 23 },
			{ 'c', 8 },
			{ 'd', 8 },
			{ 'e', 8 },
			{ 'f', 8 },
			{ 'g', 8 },
			{ 'h', 33 },
			{ 'i', 4 },
			{ 'j', 8 },
			{ 'k', 3 },
			{ 'l', 25 },
			{ 'm', 44 },
			{ 'n', 40 },
			{ 'o', 43 }
		};

		public static Platform GetPlatformByPriority(List<Entity> platforms)
		{
			Platform landed = null;
			foreach (Entity hit in platforms)
			{
				if (hit is Platform && (landed == null || (hit as Platform).SurfaceSoundPriority > landed.SurfaceSoundPriority))
				{
					landed = hit as Platform;
				}
			}
			return landed;
		}
	}
}
