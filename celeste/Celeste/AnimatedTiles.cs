using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class AnimatedTiles : Component
	{
		private class Tile
		{
			public int AnimationID;

			public float Frame;

			public Vector2 Scale;
		}

		public Camera ClipCamera;

		public Vector2 Position;

		public Color Color = Color.White;

		public float Alpha = 1f;

		public AnimatedTilesBank Bank;

		private VirtualMap<List<Tile>> tiles;

		public AnimatedTiles(int columns, int rows, AnimatedTilesBank bank)
			: base(active: true, visible: true)
		{
			tiles = new VirtualMap<List<Tile>>(columns, rows);
			Bank = bank;
		}

		public void Set(int x, int y, string name, float scaleX = 1f, float scaleY = 1f)
		{
			if (!string.IsNullOrEmpty(name))
			{
				AnimatedTilesBank.Animation anim = Bank.AnimationsByName[name];
				List<Tile> list = tiles[x, y];
				if (list == null)
				{
					List<Tile> list3 = (tiles[x, y] = new List<Tile>());
					list = list3;
				}
				list.Add(new Tile
				{
					AnimationID = anim.ID,
					Frame = Calc.Random.Next(anim.Frames.Length),
					Scale = new Vector2(scaleX, scaleY)
				});
			}
		}

		public Rectangle GetClippedRenderTiles(int extend)
		{
			Vector2 pos = base.Entity.Position + Position;
			int left;
			int top;
			int right;
			int bottom;
			if (ClipCamera == null)
			{
				left = -extend;
				top = -extend;
				right = tiles.Columns + extend;
				bottom = tiles.Rows + extend;
			}
			else
			{
				Camera camera = ClipCamera;
				left = (int)Math.Max(0.0, Math.Floor((camera.Left - pos.X) / 8f) - (double)extend);
				top = (int)Math.Max(0.0, Math.Floor((camera.Top - pos.Y) / 8f) - (double)extend);
				right = (int)Math.Min(tiles.Columns, Math.Ceiling((camera.Right - pos.X) / 8f) + (double)extend);
				bottom = (int)Math.Min(tiles.Rows, Math.Ceiling((camera.Bottom - pos.Y) / 8f) + (double)extend);
			}
			left = Math.Max(left, 0);
			top = Math.Max(top, 0);
			right = Math.Min(right, tiles.Columns);
			bottom = Math.Min(bottom, tiles.Rows);
			return new Rectangle(left, top, right - left, bottom - top);
		}

		public override void Update()
		{
			Rectangle clip = GetClippedRenderTiles(1);
			for (int tx = clip.Left; tx < clip.Right; tx++)
			{
				for (int ty = clip.Top; ty < clip.Bottom; ty++)
				{
					List<Tile> list = tiles[tx, ty];
					if (list != null)
					{
						for (int i = 0; i < list.Count; i++)
						{
							AnimatedTilesBank.Animation anim = Bank.Animations[list[i].AnimationID];
							list[i].Frame += Engine.DeltaTime / anim.Delay;
						}
					}
				}
			}
		}

		public override void Render()
		{
			RenderAt(base.Entity.Position + Position);
		}

		public void RenderAt(Vector2 position)
		{
			Rectangle clip = GetClippedRenderTiles(1);
			Color color = Color * Alpha;
			for (int tx = clip.Left; tx < clip.Right; tx++)
			{
				for (int ty = clip.Top; ty < clip.Bottom; ty++)
				{
					List<Tile> list = tiles[tx, ty];
					if (list != null)
					{
						for (int i = 0; i < list.Count; i++)
						{
							Tile tile = list[i];
							AnimatedTilesBank.Animation anim = Bank.Animations[tile.AnimationID];
							anim.Frames[(int)tile.Frame % anim.Frames.Length].Draw(position + anim.Offset + new Vector2((float)tx + 0.5f, (float)ty + 0.5f) * 8f, anim.Origin, color, tile.Scale);
						}
					}
				}
			}
		}
	}
}
