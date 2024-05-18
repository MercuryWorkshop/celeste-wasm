using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class ClutterCabinet : Entity
	{
		private Sprite sprite;

		public bool Opened { get; private set; }

		public ClutterCabinet(Vector2 position)
			: base(position)
		{
			Add(sprite = GFX.SpriteBank.Create("clutterCabinet"));
			sprite.Position = new Vector2(8f);
			sprite.Play("idle");
			base.Depth = -10001;
		}

		public ClutterCabinet(EntityData data, Vector2 offset)
			: this(data.Position + offset)
		{
		}

		public void Open()
		{
			sprite.Play("open");
			Opened = true;
		}

		public void Close()
		{
			sprite.Play("close");
			Opened = false;
		}
	}
}
