using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class NPC06_Theo_Plateau : NPC
	{
		private float speedY;

		public NPC06_Theo_Plateau(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			Add(Sprite = GFX.SpriteBank.Create("theo"));
			IdleAnim = "idle";
			MoveAnim = "walk";
			Maxspeed = 48f;
			MoveY = false;
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			Player player = base.Scene.Tracker.GetEntity<Player>();
			base.Scene.Add(new CS06_Campfire(this, player));
			Add(Light = new VertexLight(new Vector2(0f, -6f), Color.White, 1f, 16, 48));
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
