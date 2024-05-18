using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class NPC03_Oshiro_Hallway2 : NPC
	{
		private bool talked;

		public NPC03_Oshiro_Hallway2(Vector2 position)
			: base(position)
		{
			Add(Sprite = new OshiroSprite(-1));
			Add(Light = new VertexLight(-Vector2.UnitY * 16f, Color.White, 1f, 32, 64));
			MoveAnim = "move";
			IdleAnim = "idle";
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (base.Session.GetFlag("oshiro_resort_talked_3"))
			{
				RemoveSelf();
			}
			else
			{
				base.Session.LightingAlphaAdd = 0.15f;
			}
		}

		public override void Update()
		{
			base.Update();
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (!talked && player != null && player.X > base.X - 60f)
			{
				base.Scene.Add(new CS03_OshiroHallway2(player, this));
				talked = true;
			}
		}
	}
}
