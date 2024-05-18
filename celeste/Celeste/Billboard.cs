using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(true)]
	public class Billboard : Component
	{
		public MTexture Texture;

		public Vector3 Position;

		public Color Color = Color.White;

		public Vector2 Size = Vector2.One;

		public Vector2 Scale = Vector2.One;

		public Action BeforeRender;

		public Billboard(MTexture texture, Vector3 position, Vector2? size = null, Color? color = null, Vector2? scale = null)
			: base(active: true, visible: true)
		{
			Texture = texture;
			Position = position;
			Size = (size.HasValue ? size.Value : Vector2.One);
			Color = (color.HasValue ? color.Value : Color.White);
			Scale = (scale.HasValue ? scale.Value : Vector2.One);
		}
	}
}
