using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class WhiteBlock : JumpThru
	{
		private const float duckDuration = 3f;

		private float playerDuckTimer;

		private bool enabled = true;

		private bool activated;

		private Image sprite;

		private Entity bgSolidTiles;

		public WhiteBlock(EntityData data, Vector2 offset)
			: base(data.Position + offset, 48, safe: true)
		{
			Add(sprite = new Image(GFX.Game["objects/whiteblock"]));
			base.Depth = 8990;
			SurfaceSoundIndex = 27;
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if ((scene as Level).Session.HeartGem)
			{
				Disable();
			}
		}

		private void Disable()
		{
			enabled = false;
			sprite.Color = Color.White * 0.25f;
			Collidable = false;
		}

		private void Activate(Player player)
		{
			Audio.Play("event:/game/04_cliffside/whiteblock_fallthru", base.Center);
			activated = true;
			Collidable = false;
			player.Depth = 10001;
			base.Depth = -9000;
			Level level = base.Scene as Level;
			Rectangle levelBounds = new Rectangle(level.Bounds.Left / 8, level.Bounds.Y / 8, level.Bounds.Width / 8, level.Bounds.Height / 8);
			Rectangle tileBounds = level.Session.MapData.TileBounds;
			bool[,] bgTiles = new bool[levelBounds.Width, levelBounds.Height];
			for (int tx = 0; tx < levelBounds.Width; tx++)
			{
				for (int ty = 0; ty < levelBounds.Height; ty++)
				{
					bgTiles[tx, ty] = level.BgData[tx + levelBounds.Left - tileBounds.Left, ty + levelBounds.Top - tileBounds.Top] != '0';
				}
			}
			bgSolidTiles = new Solid(new Vector2(level.Bounds.Left, level.Bounds.Top), 1f, 1f, safe: true);
			bgSolidTiles.Collider = new Grid(8f, 8f, bgTiles);
			base.Scene.Add(bgSolidTiles);
		}

		public override void Update()
		{
			base.Update();
			if (!enabled)
			{
				return;
			}
			if (!activated)
			{
				Player player2 = base.Scene.Tracker.GetEntity<Player>();
				if (HasPlayerRider() && player2 != null && player2.Ducking)
				{
					playerDuckTimer += Engine.DeltaTime;
					if (playerDuckTimer >= 3f)
					{
						Activate(player2);
					}
				}
				else
				{
					playerDuckTimer = 0f;
				}
				if ((base.Scene as Level).Session.HeartGem)
				{
					Disable();
				}
			}
			else if (base.Scene.Tracker.GetEntity<HeartGem>() == null)
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null)
				{
					Disable();
					player.Depth = 0;
					base.Scene.Remove(bgSolidTiles);
				}
			}
		}
	}
}
