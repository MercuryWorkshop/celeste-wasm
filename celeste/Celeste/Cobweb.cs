using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Cobweb : Entity
	{
		private Color color;

		private Color edge;

		private Vector2 anchorA;

		private Vector2 anchorB;

		private List<Vector2> offshoots;

		private List<float> offshootEndings;

		private float waveTimer;

		public Cobweb(EntityData data, Vector2 offset)
		{
			base.Depth = -1;
			anchorA = (Position = data.Position + offset);
			anchorB = data.Nodes[0] + offset;
			Vector2[] nodes = data.Nodes;
			foreach (Vector2 node in nodes)
			{
				if (offshoots == null)
				{
					offshoots = new List<Vector2>();
					offshootEndings = new List<float>();
				}
				else
				{
					offshoots.Add(node + offset);
					offshootEndings.Add(0.3f + Calc.Random.NextFloat(0.4f));
				}
			}
			waveTimer = Calc.Random.NextFloat();
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			color = Calc.Random.Choose(AreaData.Get(scene).CobwebColor);
			edge = Color.Lerp(color, Calc.HexToColor("0f0e17"), 0.2f);
			if (!base.Scene.CollideCheck<Solid>(new Rectangle((int)anchorA.X - 2, (int)anchorA.Y - 2, 4, 4)) || !base.Scene.CollideCheck<Solid>(new Rectangle((int)anchorB.X - 2, (int)anchorB.Y - 2, 4, 4)))
			{
				RemoveSelf();
			}
			for (int i = 0; i < offshoots.Count; i++)
			{
				Vector2 p = offshoots[i];
				if (!base.Scene.CollideCheck<Solid>(new Rectangle((int)p.X - 2, (int)p.Y - 2, 4, 4)))
				{
					offshoots.RemoveAt(i);
					offshootEndings.RemoveAt(i);
					i--;
				}
			}
		}

		public override void Update()
		{
			waveTimer += Engine.DeltaTime;
			base.Update();
		}

		public override void Render()
		{
			DrawCobweb(anchorA, anchorB, 12, drawOffshoots: true);
		}

		private void DrawCobweb(Vector2 a, Vector2 b, int steps, bool drawOffshoots)
		{
			SimpleCurve curve = new SimpleCurve(a, b, (a + b) / 2f + Vector2.UnitY * (8f + (float)Math.Sin(waveTimer) * 4f));
			if (drawOffshoots && offshoots != null)
			{
				for (int j = 0; j < offshoots.Count; j++)
				{
					DrawCobweb(offshoots[j], curve.GetPoint(offshootEndings[j]), 4, drawOffshoots: false);
				}
			}
			Vector2 prev = curve.Begin;
			for (int i = 1; i <= steps; i++)
			{
				float percent = (float)i / (float)steps;
				Vector2 next = curve.GetPoint(percent);
				Draw.Line(prev, next, (i <= 2 || i >= steps - 1) ? edge : color);
				prev = next + (prev - next).SafeNormalize();
			}
		}
	}
}
