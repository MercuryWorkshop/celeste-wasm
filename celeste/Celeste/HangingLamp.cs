using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class HangingLamp : Entity
	{
		public readonly int Length;

		private List<Image> images = new List<Image>();

		private BloomPoint bloom;

		private VertexLight light;

		private float speed;

		private float rotation;

		private float soundDelay;

		private SoundSource sfx;

		public HangingLamp(Vector2 position, int length)
		{
			Position = position + Vector2.UnitX * 4f;
			Length = Math.Max(16, length);
			base.Depth = 2000;
			MTexture texture = GFX.Game["objects/hanginglamp"];
			Image img = null;
			for (int i = 0; i < Length - 8; i += 8)
			{
				Add(img = new Image(texture.GetSubtexture(0, 8, 8, 8)));
				img.Origin.X = 4f;
				img.Origin.Y = -i;
				images.Add(img);
			}
			Add(img = new Image(texture.GetSubtexture(0, 0, 8, 8)));
			img.Origin.X = 4f;
			Add(img = new Image(texture.GetSubtexture(0, 16, 8, 8)));
			img.Origin.X = 4f;
			img.Origin.Y = -(Length - 8);
			images.Add(img);
			Add(bloom = new BloomPoint(Vector2.UnitY * (Length - 4), 1f, 48f));
			Add(light = new VertexLight(Vector2.UnitY * (Length - 4), Color.White, 1f, 24, 48));
			Add(sfx = new SoundSource());
			base.Collider = new Hitbox(8f, Length, -4f);
		}

		public HangingLamp(EntityData e, Vector2 position)
			: this(position, Math.Max(16, e.Height))
		{
		}

		public override void Update()
		{
			base.Update();
			soundDelay -= Engine.DeltaTime;
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null && base.Collider.Collide(player))
			{
				speed = (0f - player.Speed.X) * 0.005f * ((player.Y - base.Y) / (float)Length);
				if (Math.Abs(speed) < 0.1f)
				{
					speed = 0f;
				}
				else if (soundDelay <= 0f)
				{
					sfx.Play("event:/game/02_old_site/lantern_hit");
					soundDelay = 0.25f;
				}
			}
			float str = ((Math.Sign(rotation) == Math.Sign(speed)) ? 8f : 6f);
			if (Math.Abs(rotation) < 0.5f)
			{
				str *= 0.5f;
			}
			if (Math.Abs(rotation) < 0.25f)
			{
				str *= 0.5f;
			}
			float was = rotation;
			speed += (float)(-Math.Sign(rotation)) * str * Engine.DeltaTime;
			rotation += speed * Engine.DeltaTime;
			rotation = Calc.Clamp(rotation, -0.4f, 0.4f);
			if (Math.Abs(rotation) < 0.02f && Math.Abs(speed) < 0.2f)
			{
				rotation = (speed = 0f);
			}
			else if (Math.Sign(rotation) != Math.Sign(was) && soundDelay <= 0f && Math.Abs(speed) > 0.5f)
			{
				sfx.Play("event:/game/02_old_site/lantern_hit");
				soundDelay = 0.25f;
			}
			foreach (Image image in images)
			{
				image.Rotation = rotation;
			}
			Vector2 target = Calc.AngleToVector(rotation + (float)Math.PI / 2f, (float)Length - 4f);
			Vector2 vector2 = (bloom.Position = (light.Position = target));
			sfx.Position = target;
		}

		public override void Render()
		{
			foreach (Component component in base.Components)
			{
				if (component is Image img)
				{
					img.DrawOutline();
				}
			}
			base.Render();
		}
	}
}
