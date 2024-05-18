using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class Door : Actor
	{
		private Sprite sprite;

		private string openSfx;

		private string closeSfx;

		private LightOcclude occlude;

		private bool disabled;

		public Door(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			base.Depth = 8998;
			string type = data.Attr("type", "wood");
			if (type == "wood")
			{
				Add(sprite = GFX.SpriteBank.Create("door"));
				openSfx = "event:/game/03_resort/door_wood_open";
				closeSfx = "event:/game/03_resort/door_wood_close";
			}
			else
			{
				Add(sprite = GFX.SpriteBank.Create(type + "door"));
				openSfx = "event:/game/03_resort/door_metal_open";
				closeSfx = "event:/game/03_resort/door_metal_close";
			}
			sprite.Play("idle");
			base.Collider = new Hitbox(12f, 22f, -6f, -23f);
			Add(occlude = new LightOcclude(new Rectangle(-1, -24, 2, 24)));
			Add(new PlayerCollider(HitPlayer));
		}

		public override bool IsRiding(Solid solid)
		{
			return base.Scene.CollideCheck(new Rectangle((int)base.X - 2, (int)base.Y - 2, 4, 4), solid);
		}

		protected override void OnSquish(CollisionData data)
		{
		}

		private void HitPlayer(Player player)
		{
			if (!disabled)
			{
				Open(player.X);
			}
		}

		public void Open(float x)
		{
			if (sprite.CurrentAnimationID == "idle")
			{
				Audio.Play(openSfx, Position);
				sprite.Play("open");
				if (base.X != x)
				{
					sprite.Scale.X = Math.Sign(x - base.X);
				}
			}
			else if (sprite.CurrentAnimationID == "close")
			{
				sprite.Play("close", restart: true);
			}
		}

		public override void Update()
		{
			string currentAnimationID = sprite.CurrentAnimationID;
			base.Update();
			occlude.Visible = sprite.CurrentAnimationID == "idle";
			if (!disabled && CollideCheck<Solid>())
			{
				disabled = true;
			}
			if (currentAnimationID == "close" && sprite.CurrentAnimationID == "idle")
			{
				Audio.Play(closeSfx, Position);
			}
		}
	}
}
