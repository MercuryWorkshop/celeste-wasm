using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class DreamHeartGem : Entity
	{
		private Sprite sprite;

		public DreamHeartGem(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			Add(sprite = GFX.SpriteBank.Create("heartgem0"));
			sprite.Color = Color.White * 0.25f;
			sprite.Play("spin");
			Add(new BloomPoint(0.5f, 16f));
			Add(new VertexLight(Color.Aqua, 1f, 32, 64));
		}

		public override void Render()
		{
			for (int i = 0; (float)i < sprite.Height; i++)
			{
				sprite.DrawSubrect(new Vector2((float)Math.Sin(base.Scene.TimeActive * 2f + (float)i * 0.4f) * 2f, i), new Rectangle(0, i, (int)sprite.Width, 1));
			}
		}
	}
}
