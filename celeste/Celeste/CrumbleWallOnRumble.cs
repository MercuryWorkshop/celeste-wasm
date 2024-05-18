using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class CrumbleWallOnRumble : Solid
	{
		private bool permanent;

		private EntityID id;

		private char tileType;

		private bool blendIn;

		public CrumbleWallOnRumble(Vector2 position, char tileType, float width, float height, bool blendIn, bool persistent, EntityID id)
			: base(position, width, height, safe: true)
		{
			base.Depth = -12999;
			this.id = id;
			this.tileType = tileType;
			this.blendIn = blendIn;
			permanent = persistent;
			SurfaceSoundIndex = SurfaceIndex.TileToIndex[this.tileType];
		}

		public CrumbleWallOnRumble(EntityData data, Vector2 offset, EntityID id)
			: this(data.Position + offset, data.Char("tiletype", 'm'), data.Width, data.Height, data.Bool("blendin"), data.Bool("persistent"), id)
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			TileGrid tiles;
			if (!blendIn)
			{
				tiles = GFX.FGAutotiler.GenerateBox(tileType, (int)base.Width / 8, (int)base.Height / 8).TileGrid;
			}
			else
			{
				Level level = SceneAs<Level>();
				Rectangle mapBounds = level.Session.MapData.TileBounds;
				VirtualMap<char> mapSolids = level.SolidsData;
				int x = (int)(base.X / 8f) - mapBounds.Left;
				int y = (int)(base.Y / 8f) - mapBounds.Top;
				int w = (int)base.Width / 8;
				int h = (int)base.Height / 8;
				tiles = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, w, h, mapSolids).TileGrid;
				base.Depth = -10501;
			}
			Add(tiles);
			Add(new TileInterceptor(tiles, highPriority: true));
			Add(new LightOcclude());
			if (CollideCheck<Player>())
			{
				RemoveSelf();
			}
		}

		public void Break()
		{
			if (!Collidable || base.Scene == null)
			{
				return;
			}
			Audio.Play("event:/new_content/game/10_farewell/quake_rockbreak", Position);
			Collidable = false;
			for (int x = 0; (float)x < base.Width / 8f; x++)
			{
				for (int y = 0; (float)y < base.Height / 8f; y++)
				{
					if (!base.Scene.CollideCheck<Solid>(new Rectangle((int)base.X + x * 8, (int)base.Y + y * 8, 8, 8)))
					{
						base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + x * 8, 4 + y * 8), tileType).BlastFrom(base.TopCenter));
					}
				}
			}
			if (permanent)
			{
				SceneAs<Level>().Session.DoNotLoad.Add(id);
			}
			RemoveSelf();
		}
	}
}
