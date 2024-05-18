using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class IceTileOverlay : Entity
	{
		private List<MTexture> surfaces;

		private float alpha;

		public IceTileOverlay()
		{
			base.Depth = -10010;
			base.Tag = Tags.Global;
			Visible = false;
			surfaces = GFX.Game.GetAtlasSubtextures("scenery/iceSurface");
		}

		public override void Update()
		{
			base.Update();
			alpha = Calc.Approach(alpha, ((base.Scene as Level).CoreMode == Session.CoreModes.Cold) ? 1 : 0, Engine.DeltaTime * 4f);
			Visible = alpha > 0f;
		}

		public override void Render()
		{
			Level level = base.Scene as Level;
			Camera camera = level.Camera;
			Color color = Color.White * alpha;
			int left = (int)(Math.Floor((camera.Left - level.SolidTiles.X) / 8f) - 1.0);
			int top = (int)(Math.Floor((camera.Top - level.SolidTiles.Y) / 8f) - 1.0);
			int right = (int)(Math.Ceiling((camera.Right - level.SolidTiles.X) / 8f) + 1.0);
			int bottom = (int)(Math.Ceiling((camera.Bottom - level.SolidTiles.Y) / 8f) + 1.0);
			for (int tx = left; tx < right; tx++)
			{
				for (int ty = top; ty < bottom; ty++)
				{
					if (level.SolidsData.SafeCheck(tx, ty) != '0' && level.SolidsData.SafeCheck(tx, ty - 1) == '0')
					{
						Vector2 pos = level.SolidTiles.Position + new Vector2(tx, ty) * 8f;
						int i = (tx * 5 + ty * 17) % surfaces.Count;
						surfaces[i].Draw(pos, Vector2.Zero, color);
					}
				}
			}
		}
	}
}
