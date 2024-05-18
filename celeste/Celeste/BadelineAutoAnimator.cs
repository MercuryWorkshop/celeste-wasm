using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class BadelineAutoAnimator : Component
	{
		public bool Enabled = true;

		private string lastAnimation;

		private bool wasSyncingSprite;

		private Wiggler pop;

		public BadelineAutoAnimator()
			: base(active: true, visible: false)
		{
		}

		public override void Added(Entity entity)
		{
			base.Added(entity);
			entity.Add(pop = Wiggler.Create(0.5f, 4f, delegate(float f)
			{
				Sprite sprite = base.Entity.Get<Sprite>();
				if (sprite != null)
				{
					sprite.Scale = new Vector2(Math.Sign(sprite.Scale.X), 1f) * (1f + 0.25f * f);
				}
			}));
		}

		public override void Removed(Entity entity)
		{
			entity.Remove(pop);
			base.Removed(entity);
		}

		public void SetReturnToAnimation(string anim)
		{
			lastAnimation = anim;
		}

		public override void Update()
		{
			Sprite sprite = base.Entity.Get<Sprite>();
			if (base.Scene == null || sprite == null)
			{
				return;
			}
			bool syncSprite = false;
			Textbox textbox = base.Scene.Tracker.GetEntity<Textbox>();
			if (Enabled && textbox != null && textbox.PortraitName.IsIgnoreCase("badeline"))
			{
				if (textbox.PortraitAnimation.IsIgnoreCase("scoff"))
				{
					if (!wasSyncingSprite)
					{
						lastAnimation = sprite.CurrentAnimationID;
					}
					sprite.Play("laugh");
					syncSprite = (wasSyncingSprite = true);
				}
				else if (textbox.PortraitAnimation.IsIgnoreCase("yell", "freakA", "freakB", "freakC"))
				{
					if (!wasSyncingSprite)
					{
						pop.Start();
						lastAnimation = sprite.CurrentAnimationID;
					}
					sprite.Play("angry");
					syncSprite = (wasSyncingSprite = true);
				}
			}
			if (wasSyncingSprite && !syncSprite)
			{
				wasSyncingSprite = false;
				if (string.IsNullOrEmpty(lastAnimation) || lastAnimation == "spin")
				{
					lastAnimation = "fallSlow";
				}
				if (sprite.CurrentAnimationID == "angry")
				{
					pop.Start();
				}
				sprite.Play(lastAnimation);
			}
		}
	}
}
