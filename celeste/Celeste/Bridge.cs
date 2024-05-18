using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Bridge : Entity
	{
		private List<BridgeTile> tiles;

		private Level level;

		private bool canCollapse;

		private bool canEndCollapseA;

		private bool canEndCollapseB;

		private float collapseTimer;

		private int width;

		private List<Rectangle> tileSizes = new List<Rectangle>();

		private bool ending;

		private float gapStartX;

		private float gapEndX;

		private SoundSource collapseSfx;

		public Bridge(Vector2 position, int width, float gapStartX, float gapEndX)
			: base(position)
		{
			this.width = width;
			this.gapStartX = gapStartX;
			this.gapEndX = gapEndX;
			tileSizes.Add(new Rectangle(0, 0, 16, 52));
			tileSizes.Add(new Rectangle(16, 0, 8, 52));
			tileSizes.Add(new Rectangle(24, 0, 8, 52));
			tileSizes.Add(new Rectangle(32, 0, 8, 52));
			tileSizes.Add(new Rectangle(40, 0, 8, 52));
			tileSizes.Add(new Rectangle(48, 0, 8, 52));
			tileSizes.Add(new Rectangle(56, 0, 8, 52));
			tileSizes.Add(new Rectangle(64, 0, 8, 52));
			tileSizes.Add(new Rectangle(72, 0, 8, 52));
			tileSizes.Add(new Rectangle(80, 0, 16, 52));
			tileSizes.Add(new Rectangle(96, 0, 8, 52));
			Add(collapseSfx = new SoundSource());
		}

		public Bridge(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Nodes[0].X, data.Nodes[1].X)
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			level = scene as Level;
			tiles = new List<BridgeTile>();
			gapStartX += level.Bounds.Left;
			gapEndX += level.Bounds.Left;
			Calc.PushRandom(1);
			Vector2 at = Position;
			int tx = 0;
			while (at.X < base.X + (float)width)
			{
				Rectangle size = ((tx < 2 || tx > 7) ? tileSizes[tx] : tileSizes[2 + Calc.Random.Next(6)]);
				if (at.X < gapStartX || at.X >= gapEndX)
				{
					BridgeTile tile = new BridgeTile(at, size);
					tiles.Add(tile);
					level.Add(tile);
				}
				at.X += size.Width;
				tx = (tx + 1) % tileSizes.Count;
			}
			Calc.PopRandom();
		}

		public override void Update()
		{
			base.Update();
			Player p = level.Tracker.GetEntity<Player>();
			if (p == null || p.Dead)
			{
				collapseSfx.Stop();
			}
			if (!canCollapse)
			{
				if (p != null && p.X >= base.X + 112f)
				{
					Audio.SetMusic("event:/music/lvl0/bridge");
					collapseSfx.Play("event:/game/00_prologue/bridge_rumble_loop");
					canCollapse = true;
					canEndCollapseA = true;
					canEndCollapseB = true;
					for (int l = 0; l < 11; l++)
					{
						tiles[0].Fall(Calc.Random.Range(0.1f, 0.5f));
						tiles.RemoveAt(0);
					}
				}
			}
			else if (tiles.Count > 0)
			{
				if (p == null)
				{
					return;
				}
				if (canEndCollapseA && p.X > base.X + (float)width - 216f)
				{
					canEndCollapseA = false;
					for (int k = 0; k < 5; k++)
					{
						tiles[tiles.Count - 8].Fall(Calc.Random.Range(0.1f, 0.5f));
						tiles.RemoveAt(tiles.Count - 8);
					}
				}
				else if (canEndCollapseB && p.X > base.X + (float)width - 104f)
				{
					canEndCollapseB = false;
					for (int j = 0; j < 7; j++)
					{
						if (tiles.Count <= 0)
						{
							break;
						}
						tiles[tiles.Count - 1].Fall(Calc.Random.Range(0.1f, 0.3f));
						tiles.RemoveAt(tiles.Count - 1);
					}
				}
				else if (collapseTimer > 0f)
				{
					collapseTimer -= Engine.DeltaTime;
					if (tiles.Count >= 5 && p.X >= tiles[4].X)
					{
						int i = 0;
						tiles[i].Fall();
						tiles.RemoveAt(i);
					}
				}
				else
				{
					tiles[0].Fall();
					tiles.RemoveAt(0);
					collapseTimer = 0.2f;
				}
			}
			else if (!ending)
			{
				ending = true;
				StopCollapseLoop();
			}
		}

		public void StopCollapseLoop()
		{
			collapseSfx.Stop();
		}
	}
}
