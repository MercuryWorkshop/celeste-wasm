using System.Collections.Generic;
using Monocle;

namespace Celeste
{
	public static class GameplayBuffers
	{
		public static VirtualRenderTarget Gameplay;

		public static VirtualRenderTarget Level;

		public static VirtualRenderTarget ResortDust;

		public static VirtualRenderTarget LightBuffer;

		public static VirtualRenderTarget Light;

		public static VirtualRenderTarget Displacement;

		public static VirtualRenderTarget MirrorSources;

		public static VirtualRenderTarget MirrorMasks;

		public static VirtualRenderTarget SpeedRings;

		public static VirtualRenderTarget Lightning;

		public static VirtualRenderTarget TempA;

		public static VirtualRenderTarget TempB;

		private static List<VirtualRenderTarget> all = new List<VirtualRenderTarget>();

		public static void Create()
		{
			Unload();
			Gameplay = Create(320, 180);
			Level = Create(320, 180);
			ResortDust = Create(320, 180);
			Light = Create(320, 180);
			Displacement = Create(320, 180);
			LightBuffer = Create(1024, 1024);
			MirrorSources = Create(384, 244);
			MirrorMasks = Create(384, 244);
			SpeedRings = Create(512, 512);
			Lightning = Create(160, 160);
			TempA = Create(320, 180);
			TempB = Create(320, 180);
		}

		private static VirtualRenderTarget Create(int width, int height)
		{
			VirtualRenderTarget r = VirtualContent.CreateRenderTarget("gameplay-buffer-" + all.Count, width, height);
			all.Add(r);
			return r;
		}

		public static void Unload()
		{
			foreach (VirtualRenderTarget item in all)
			{
				item.Dispose();
			}
			all.Clear();
		}
	}
}
