using Microsoft.Xna.Framework;

namespace Celeste
{
	public class NPC08_Granny : NPC
	{
		public NPC08_Granny(EntityData data, Vector2 position)
			: base(data.Position + position)
		{
			Add(Sprite = GFX.SpriteBank.Create("granny"));
			Sprite.Scale.X = -1f;
			Sprite.Play("idle");
			IdleAnim = "idle";
			MoveAnim = "walk";
			Maxspeed = 30f;
			base.Depth = -10;
		}
	}
}
