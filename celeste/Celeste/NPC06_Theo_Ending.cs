using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class NPC06_Theo_Ending : NPC
	{
		private float speedY;

		public NPC06_Theo_Ending(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			Add(Sprite = GFX.SpriteBank.Create("theo"));
			IdleAnim = "idle";
			MoveAnim = "run";
			Maxspeed = 72f;
			MoveY = false;
			Visible = false;
			Add(Light = new VertexLight(new Vector2(0f, -8f), Color.White, 1f, 16, 32));
			SetupTheoSpriteSounds();
		}

		public override void Update()
		{
			base.Update();
			if (!CollideCheck<Solid>(Position + new Vector2(0f, 1f)))
			{
				speedY += 400f * Engine.DeltaTime;
				Position.Y += speedY * Engine.DeltaTime;
			}
			else
			{
				speedY = 0f;
			}
		}
	}
}
