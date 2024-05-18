using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class NPC00_Granny : NPC
	{
		public Hahaha Hahaha;

		public GrannyLaughSfx LaughSfx;

		private bool talking;

		public NPC00_Granny(Vector2 position)
			: base(position)
		{
			Add(Sprite = GFX.SpriteBank.Create("granny"));
			Sprite.Play("idle");
			Add(LaughSfx = new GrannyLaughSfx(Sprite));
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if ((scene as Level).Session.GetFlag("granny"))
			{
				Sprite.Play("laugh");
			}
			scene.Add(Hahaha = new Hahaha(Position + new Vector2(8f, -4f)));
			Hahaha.Enabled = false;
		}

		public override void Update()
		{
			Player player = Level.Tracker.GetEntity<Player>();
			if (player != null && !base.Session.GetFlag("granny") && !talking)
			{
				int minX = Level.Bounds.Left + 96;
				if (player.OnGround() && player.X >= (float)minX && player.X <= base.X + 16f && Math.Abs(player.Y - base.Y) < 4f && player.Facing == (Facings)Math.Sign(base.X - player.X))
				{
					talking = true;
					base.Scene.Add(new CS00_Granny(this, player));
				}
			}
			Hahaha.Enabled = Sprite.CurrentAnimationID == "laugh";
			base.Update();
		}
	}
}
