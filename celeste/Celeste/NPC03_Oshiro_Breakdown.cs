using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class NPC03_Oshiro_Breakdown : NPC
	{
		private bool talked;

		public NPC03_Oshiro_Breakdown(Vector2 position)
			: base(position)
		{
			Add(Sprite = new OshiroSprite(1));
			Add(Light = new VertexLight(-Vector2.UnitY * 16f, Color.White, 1f, 32, 64));
			MoveAnim = "move";
			IdleAnim = "idle";
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (base.Session.GetFlag("oshiro_breakdown"))
			{
				RemoveSelf();
			}
		}

		public override void Update()
		{
			base.Update();
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (!talked && player != null && ((player.X <= (float)(Level.Bounds.Left + 370) && player.OnSafeGround && player.Y < (float)Level.Bounds.Center.Y) || player.X <= (float)(Level.Bounds.Left + 320)))
			{
				base.Scene.Add(new CS03_OshiroBreakdown(player, this));
				talked = true;
			}
		}
	}
}
