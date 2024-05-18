using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class TileInterceptor : Component
	{
		public Action<MTexture, Vector2, Point> Intercepter;

		public bool HighPriority;

		public TileInterceptor(Action<MTexture, Vector2, Point> intercepter, bool highPriority)
			: base(active: false, visible: false)
		{
			Intercepter = intercepter;
			HighPriority = highPriority;
		}

		public TileInterceptor(TileGrid applyToGrid, bool highPriority)
			: base(active: false, visible: false)
		{
			Intercepter = delegate(MTexture t, Vector2 v, Point p)
			{
				applyToGrid.Tiles[p.X, p.Y] = t;
			};
			HighPriority = highPriority;
		}

		public static bool TileCheck(Scene scene, MTexture tile, Vector2 at)
		{
			at += Vector2.One * 4f;
			TileInterceptor hit = null;
			List<Component> list = scene.Tracker.GetComponents<TileInterceptor>();
			for (int i = list.Count - 1; i >= 0; i--)
			{
				TileInterceptor ti = (TileInterceptor)list[i];
				if ((hit == null || ti.HighPriority) && ti.Entity.CollidePoint(at))
				{
					hit = ti;
					if (ti.HighPriority)
					{
						break;
					}
				}
			}
			if (hit != null)
			{
				Point p = new Point((int)((at.X - hit.Entity.X) / 8f), (int)((at.Y - hit.Entity.Y) / 8f));
				hit.Intercepter(tile, at, p);
				return true;
			}
			return false;
		}
	}
}
