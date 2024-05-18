using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	[Pooled]
	public class SlashFx : Entity
	{
		public Sprite Sprite;

		public Vector2 Direction;

		public SlashFx()
		{
			Add(Sprite = new Sprite(GFX.Game, "effects/slash/"));
			Sprite.Add("play", "", 0.1f, 0, 1, 2, 3);
			Sprite.CenterOrigin();
			Sprite.OnFinish = delegate
			{
				RemoveSelf();
			};
			base.Depth = -100;
		}

		public override void Update()
		{
			Position += Direction * 8f * Engine.DeltaTime;
			base.Update();
		}

		public static SlashFx Burst(Vector2 position, float direction)
		{
			Scene scene = Engine.Scene;
			SlashFx slash = Engine.Pooler.Create<SlashFx>();
			scene.Add(slash);
			slash.Position = position;
			slash.Direction = Calc.AngleToVector(direction, 1f);
			slash.Sprite.Play("play", restart: true);
			slash.Sprite.Scale = Vector2.One;
			slash.Sprite.Rotation = 0f;
			if (Math.Abs(direction - (float)Math.PI) > 0.01f)
			{
				slash.Sprite.Rotation = direction;
			}
			slash.Visible = (slash.Active = true);
			return slash;
		}
	}
}
