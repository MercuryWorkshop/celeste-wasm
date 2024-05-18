using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Wire : Entity
	{
		public Color Color = Calc.HexToColor("595866");

		public SimpleCurve Curve;

		private float sineX;

		private float sineY;

		public Wire(Vector2 from, Vector2 to, bool above)
		{
			Curve = new SimpleCurve(from, to, Vector2.Zero);
			base.Depth = (above ? (-8500) : 2000);
			Random rand = new Random((int)Math.Min(from.X, to.X));
			sineX = rand.NextFloat(4f);
			sineY = rand.NextFloat(4f);
		}

		public Wire(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Nodes[0] + offset, data.Bool("above"))
		{
		}

		public override void Render()
		{
			Level level = SceneAs<Level>();
			Vector2 movement = new Vector2((float)Math.Sin(sineX + level.WindSineTimer * 2f), (float)Math.Sin(sineY + level.WindSineTimer * 2.8f)) * 8f * level.VisualWind;
			Curve.Control = (Curve.Begin + Curve.End) / 2f + new Vector2(0f, 24f) + movement;
			Vector2 prev = Curve.Begin;
			for (int i = 1; i <= 16; i++)
			{
				float percent = (float)i / 16f;
				Vector2 next = Curve.GetPoint(percent);
				Draw.Line(prev, next, Color);
				prev = next;
			}
		}
	}
}
