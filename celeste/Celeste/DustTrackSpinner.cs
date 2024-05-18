using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class DustTrackSpinner : TrackSpinner
	{
		private DustGraphic dusty;

		private Vector2 outwards;

		public DustTrackSpinner(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			Add(dusty = new DustGraphic(ignoreSolids: true));
			dusty.EyeDirection = (dusty.EyeTargetDirection = (base.End - base.Start).SafeNormalize());
			dusty.OnEstablish = Establish;
			base.Depth = -50;
		}

		private void Establish()
		{
			Vector2 normal = (base.End - base.Start).SafeNormalize();
			Vector2 perp = new Vector2(0f - normal.Y, normal.X);
			bool wall = base.Scene.CollideCheck<Solid>(new Rectangle((int)(base.X + perp.X * 4f) - 2, (int)(base.Y + perp.Y * 4f) - 2, 4, 4));
			if (!wall)
			{
				perp = -perp;
				wall = base.Scene.CollideCheck<Solid>(new Rectangle((int)(base.X + perp.X * 4f) - 2, (int)(base.Y + perp.Y * 4f) - 2, 4, 4));
			}
			if (!wall)
			{
				return;
			}
			float len = (base.End - base.Start).Length();
			for (int i = 8; (float)i < len && wall; i += 8)
			{
				wall = wall && base.Scene.CollideCheck<Solid>(new Rectangle((int)(base.X + perp.X * 4f + normal.X * (float)i) - 2, (int)(base.Y + perp.Y * 4f + normal.Y * (float)i) - 2, 4, 4));
			}
			if (!wall)
			{
				return;
			}
			List<DustGraphic.Node> dustNodes = null;
			if (perp.X < 0f)
			{
				dustNodes = dusty.LeftNodes;
			}
			else if (perp.X > 0f)
			{
				dustNodes = dusty.RightNodes;
			}
			else if (perp.Y < 0f)
			{
				dustNodes = dusty.TopNodes;
			}
			else if (perp.Y > 0f)
			{
				dustNodes = dusty.BottomNodes;
			}
			if (dustNodes != null)
			{
				foreach (DustGraphic.Node item in dustNodes)
				{
					item.Enabled = false;
				}
			}
			outwards = -perp;
			dusty.Position -= perp;
			dusty.EyeDirection = (dusty.EyeTargetDirection = Calc.AngleToVector(Calc.AngleLerp(outwards.Angle(), Up ? (Angle + (float)Math.PI) : Angle, 0.3f), 1f));
		}

		public override void Update()
		{
			base.Update();
			if (Moving && PauseTimer < 0f && base.Scene.OnInterval(0.02f))
			{
				SceneAs<Level>().ParticlesBG.Emit(DustStaticSpinner.P_Move, 1, Position, Vector2.One * 4f);
			}
		}

		public override void OnPlayer(Player player)
		{
			base.OnPlayer(player);
			dusty.OnHitPlayer();
		}

		public override void OnTrackEnd()
		{
			if (outwards != Vector2.Zero)
			{
				dusty.EyeTargetDirection = Calc.AngleToVector(Calc.AngleLerp(outwards.Angle(), Up ? (Angle + (float)Math.PI) : Angle, 0.3f), 1f);
				return;
			}
			dusty.EyeTargetDirection = Calc.AngleToVector(Up ? (Angle + (float)Math.PI) : Angle, 1f);
			dusty.EyeFlip = -dusty.EyeFlip;
		}
	}
}
