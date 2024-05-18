using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class WaterInteraction : Component
	{
		public static ParticleType P_Drip;

		public Func<bool> IsDashing;

		public float DrippingTimer;

		public float DrippingOffset;

		public WaterInteraction(Func<bool> isDashing)
			: base(active: false, visible: false)
		{
			IsDashing = isDashing;
		}

		public override void Update()
		{
			if (DrippingTimer > 0f)
			{
				DrippingTimer -= Engine.DeltaTime;
				if (base.Scene.OnInterval(0.1f))
				{
					float x = base.Entity.Left - 2f + Calc.Random.NextFloat(base.Entity.Width + 4f);
					float y = base.Entity.Top + DrippingOffset + Calc.Random.NextFloat(base.Entity.Height - DrippingOffset);
					(base.Scene as Level).ParticlesFG.Emit(P_Drip, new Vector2(x, y));
				}
			}
		}
	}
}
