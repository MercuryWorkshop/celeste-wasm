using Microsoft.Xna.Framework;

namespace Celeste
{
	public class NPC08_Theo : NPC
	{
		public NPC08_Theo(EntityData data, Vector2 position)
			: base(data.Position + position)
		{
			Add(Sprite = GFX.SpriteBank.Create("theo"));
			Sprite.Scale.X = -1f;
			Sprite.Play("idle");
			IdleAnim = "idle";
			MoveAnim = "walk";
			Maxspeed = 30f;
		}
	}
}
