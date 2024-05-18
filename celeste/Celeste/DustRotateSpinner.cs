using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class DustRotateSpinner : RotateSpinner
	{
		private DustGraphic dusty;

		public DustRotateSpinner(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			Add(dusty = new DustGraphic(ignoreSolids: true));
		}

		public override void Update()
		{
			base.Update();
			if (Moving)
			{
				dusty.EyeDirection = (dusty.EyeTargetDirection = Calc.AngleToVector(base.Angle + (float)Math.PI / 2f * (float)(base.Clockwise ? 1 : (-1)), 1f));
				if (base.Scene.OnInterval(0.02f))
				{
					SceneAs<Level>().ParticlesBG.Emit(DustStaticSpinner.P_Move, 1, Position, Vector2.One * 4f);
				}
			}
		}

		public override void OnPlayer(Player player)
		{
			base.OnPlayer(player);
			dusty.OnHitPlayer();
		}
	}
}
