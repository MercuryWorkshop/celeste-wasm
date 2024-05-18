using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class PowerSourceNumber : Entity
	{
		private readonly Image image;

		private readonly Image glow;

		private float ease;

		private float timer;

		private bool gotKey;

		public PowerSourceNumber(Vector2 position, int index, bool gotCollectables)
		{
			Position = position;
			base.Depth = -10010;
			Add(image = new Image(GFX.Game["scenery/powersource_numbers/1"]));
			Add(glow = new Image(GFX.Game["scenery/powersource_numbers/1_glow"]));
			glow.Color = Color.Transparent;
			gotKey = gotCollectables;
		}

		public override void Update()
		{
			base.Update();
			if ((base.Scene as Level).Session.GetFlag("disable_lightning") && !gotKey)
			{
				timer += Engine.DeltaTime;
				ease = Calc.Approach(ease, 1f, Engine.DeltaTime * 4f);
				glow.Color = Color.White * ease * Calc.SineMap(timer * 2f, 0.5f, 0.9f);
			}
		}
	}
}
