using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class NPC05_Theo_Entrance : NPC
	{
		public NPC05_Theo_Entrance(Vector2 position)
			: base(position)
		{
			Add(Sprite = GFX.SpriteBank.Create("theo"));
			IdleAnim = "idle";
			MoveAnim = "walk";
			Maxspeed = 48f;
			Add(Light = new VertexLight(-Vector2.UnitY * 12f, Color.White, 1f, 32, 64));
			SetupTheoSpriteSounds();
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (base.Session.GetFlag("entrance"))
			{
				RemoveSelf();
			}
			else
			{
				scene.Add(new CS05_Entrance(this));
			}
		}
	}
}
