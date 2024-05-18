using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class StaminaDisplay : Component
	{
		private Player player;

		private float drawStamina;

		private float displayTimer;

		private Level level;

		public StaminaDisplay()
			: base(active: true, visible: false)
		{
		}

		public override void Added(Entity entity)
		{
			base.Added(entity);
			level = SceneAs<Level>();
			player = EntityAs<Player>();
			drawStamina = player.Stamina;
		}

		public override void Update()
		{
			base.Update();
			drawStamina = Calc.Approach(drawStamina, player.Stamina, 300f * Engine.DeltaTime);
			if (drawStamina < 110f && drawStamina > 0f)
			{
				displayTimer = 0.75f;
			}
			else if (displayTimer > 0f)
			{
				displayTimer -= Engine.DeltaTime;
			}
		}

		public void RenderHUD()
		{
			if (displayTimer > 0f)
			{
				Vector2 at = level.Camera.CameraToScreen(player.Position + new Vector2(0f, -18f)) * 6f;
				Color color = ((!(drawStamina < 20f)) ? Color.Lime : Color.Red);
				Draw.Rect(at.X - 48f - 1f, at.Y - 6f - 1f, 98f, 14f, Color.Black);
				Draw.Rect(at.X - 48f, at.Y - 6f, 96f * (drawStamina / 110f), 12f, color);
			}
		}
	}
}
