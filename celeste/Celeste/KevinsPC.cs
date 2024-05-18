using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class KevinsPC : Actor
	{
		private Image image;

		private MTexture spectogram;

		private MTexture subtex;

		private SoundSource sfx;

		private float timer;

		public KevinsPC(Vector2 position)
			: base(position)
		{
			Add(image = new Image(GFX.Game["objects/kevinspc/pc"]));
			image.JustifyOrigin(0.5f, 1f);
			base.Depth = 8999;
			spectogram = GFX.Game["objects/kevinspc/spectogram"];
			subtex = spectogram.GetSubtexture(0, 0, 32, 18, subtex);
			Add(sfx = new SoundSource("event:/new_content/env/local/kevinpc"));
			sfx.Position = new Vector2(0f, -16f);
			timer = 0f;
		}

		public KevinsPC(EntityData data, Vector2 offset)
			: this(data.Position + offset)
		{
		}

		public override bool IsRiding(Solid solid)
		{
			return base.Scene.CollideCheck(new Rectangle((int)base.X - 4, (int)base.Y, 8, 2), solid);
		}

		public override void Update()
		{
			base.Update();
			timer += Engine.DeltaTime;
			int distance = spectogram.Width - 32;
			int x = (int)(timer * ((float)distance / 22f) % (float)distance);
			subtex = spectogram.GetSubtexture(x, 0, 32, 18, subtex);
		}

		public override void Render()
		{
			base.Render();
			if (subtex != null)
			{
				subtex.Draw(Position + new Vector2(-16f, -39f));
				Draw.Rect(base.X - 16f, base.Y - 39f, 32f, 18f, Color.Black * 0.25f);
			}
		}
	}
}
