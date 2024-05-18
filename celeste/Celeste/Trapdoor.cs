using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Trapdoor : Entity
	{
		private Sprite sprite;

		private PlayerCollider playerCollider;

		private LightOcclude occluder;

		public Trapdoor(EntityData data, Vector2 offset)
		{
			Position = data.Position + offset;
			base.Depth = 8999;
			Add(sprite = GFX.SpriteBank.Create("trapdoor"));
			sprite.Play("idle");
			sprite.Y = 6f;
			base.Collider = new Hitbox(24f, 4f, 0f, 6f);
			Add(playerCollider = new PlayerCollider(Open));
			Add(occluder = new LightOcclude(new Rectangle(0, 6, 24, 2)));
		}

		private void Open(Player player)
		{
			Collidable = false;
			occluder.Visible = false;
			if (player.Speed.Y >= 0f)
			{
				Audio.Play("event:/game/03_resort/trapdoor_fromtop", Position);
				sprite.Play("open");
			}
			else
			{
				Audio.Play("event:/game/03_resort/trapdoor_frombottom", Position);
				Add(new Coroutine(OpenFromBottom()));
			}
		}

		private IEnumerator OpenFromBottom()
		{
			sprite.Scale.Y = -1f;
			yield return sprite.PlayRoutine("open_partial");
			yield return 0.1f;
			sprite.Rate = -1f;
			yield return sprite.PlayRoutine("open_partial", restart: true);
			sprite.Scale.Y = 1f;
			sprite.Rate = 1f;
			sprite.Play("open", restart: true);
		}
	}
}
