using Microsoft.Xna.Framework;

namespace Celeste
{
	public class NPC06_Granny_Ending : NPC
	{
		private bool talked;

		public NPC06_Granny_Ending(EntityData data, Vector2 position)
			: base(data.Position + position)
		{
			Add(Sprite = GFX.SpriteBank.Create("granny"));
			Sprite.Scale.X = -1f;
			Sprite.Play("idle");
			IdleAnim = "idle";
			MoveAnim = "walk";
			Maxspeed = 30f;
			Visible = false;
			Add(Light = new VertexLight(new Vector2(0f, -8f), Color.White, 1f, 16, 32));
			SetupGrannySpriteSounds();
		}

		public override void Update()
		{
			base.Update();
			if (!talked)
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null && player.OnGround())
				{
					talked = true;
					base.Scene.Add(new CS06_Ending(player, this));
				}
			}
		}
	}
}
