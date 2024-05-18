using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Monocle
{
	public static class VirtualContent
	{
		private static List<VirtualAsset> assets = new List<VirtualAsset>();

		private static bool reloading;

		public static int Count => assets.Count;

		public static VirtualTexture CreateTexture(string path)
		{
			VirtualTexture asset = new VirtualTexture(path);
			assets.Add(asset);
			return asset;
		}

		public static VirtualTexture CreateTexture(string name, int width, int height, Color color)
		{
			VirtualTexture asset = new VirtualTexture(name, width, height, color);
			assets.Add(asset);
			return asset;
		}

		public static VirtualRenderTarget CreateRenderTarget(string name, int width, int height, bool depth = false, bool preserve = true, int multiSampleCount = 0)
		{
			VirtualRenderTarget asset = new VirtualRenderTarget(name, width, height, multiSampleCount, depth, preserve);
			assets.Add(asset);
			return asset;
		}

		public static void BySize()
		{
			Dictionary<int, Dictionary<int, int>> list = new Dictionary<int, Dictionary<int, int>>();
			foreach (VirtualAsset asset in assets)
			{
				if (!list.ContainsKey(asset.Width))
				{
					list.Add(asset.Width, new Dictionary<int, int>());
				}
				if (!list[asset.Width].ContainsKey(asset.Height))
				{
					list[asset.Width].Add(asset.Height, 0);
				}
				list[asset.Width][asset.Height]++;
			}
			foreach (KeyValuePair<int, Dictionary<int, int>> a in list)
			{
				foreach (KeyValuePair<int, int> b in a.Value)
				{
					Console.WriteLine(a.Key + "x" + b.Key + ": " + b.Value);
				}
			}
		}

		public static void ByName()
		{
			foreach (VirtualAsset asset in assets)
			{
				Console.WriteLine(asset.Name + "[" + asset.Width + "x" + asset.Height + "]");
			}
		}

		internal static void Remove(VirtualAsset asset)
		{
			assets.Remove(asset);
		}

		internal static void Reload()
		{
			if (reloading)
			{
				foreach (VirtualAsset asset in assets)
				{
					asset.Reload();
				}
			}
			reloading = false;
		}

		internal static void Unload()
		{
			foreach (VirtualAsset asset in assets)
			{
				asset.Unload();
			}
			reloading = true;
		}
	}
}
