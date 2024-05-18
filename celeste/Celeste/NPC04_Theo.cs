using Microsoft.Xna.Framework;

namespace Celeste
{
	public class NPC04_Theo : NPC
	{
		private bool started;

		public NPC04_Theo(Vector2 position)
			: base(position)
		{
			Add(Sprite = GFX.SpriteBank.Create("theo"));
			IdleAnim = "idle";
			MoveAnim = "walk";
			Visible = false;
			Maxspeed = 48f;
			SetupTheoSpriteSounds();
		}

		public override void Update()
		{
			base.Update();
			if (!started)
			{
				Gondola gondola = base.Scene.Entities.FindFirst<Gondola>();
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (gondola != null && player != null && player.X > gondola.Left - 16f)
				{
					started = true;
					base.Scene.Add(new CS04_Gondola(this, gondola, player));
				}
			}
		}
	}
}
