using System;
using Monocle;

namespace Celeste
{
	public class OshiroSprite : Sprite
	{
		public bool AllowSpriteChanges = true;

		public bool AllowTurnInvisible = true;

		private Wiggler wiggler;

		public OshiroSprite(int facing)
		{
			Scale.X = facing;
			GFX.SpriteBank.CreateOn(this, "oshiro");
		}

		public override void Added(Entity entity)
		{
			base.Added(entity);
			entity.Add(wiggler = Wiggler.Create(0.3f, 2f, delegate(float f)
			{
				Scale.X = (float)Math.Sign(Scale.X) * (1f + f * 0.2f);
				Scale.Y = 1f - f * 0.2f;
			}));
		}

		public override void Update()
		{
			base.Update();
			if (AllowSpriteChanges)
			{
				Textbox textbox = base.Scene.Tracker.GetEntity<Textbox>();
				if (textbox != null)
				{
					if (textbox.PortraitName.Equals("oshiro", StringComparison.OrdinalIgnoreCase) && textbox.PortraitAnimation.StartsWith("side", StringComparison.OrdinalIgnoreCase))
					{
						if (base.CurrentAnimationID.Equals("idle"))
						{
							Pop("side", flip: true);
						}
					}
					else if (base.CurrentAnimationID.Equals("side"))
					{
						Pop("idle", flip: true);
					}
				}
			}
			if (AllowTurnInvisible && Visible)
			{
				Level level = base.Scene as Level;
				Visible = base.RenderPosition.X > (float)(level.Bounds.Left - 8) && base.RenderPosition.Y > (float)(level.Bounds.Top - 8) && base.RenderPosition.X < (float)(level.Bounds.Right + 8) && base.RenderPosition.Y < (float)(level.Bounds.Bottom + 16);
			}
		}

		public void Wiggle()
		{
			wiggler.Start();
		}

		public void Pop(string name, bool flip)
		{
			if (base.CurrentAnimationID.Equals(name))
			{
				return;
			}
			Play(name);
			if (flip)
			{
				Scale.X = 0f - Scale.X;
				if (Scale.X < 0f)
				{
					Audio.Play("event:/char/oshiro/chat_turn_left", base.Entity.Position);
				}
				else
				{
					Audio.Play("event:/char/oshiro/chat_turn_right", base.Entity.Position);
				}
			}
			wiggler.Start();
		}
	}
}
