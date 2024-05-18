using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Flagline : Component
	{
		private struct Cloth
		{
			public int Color;

			public int Height;

			public int Length;

			public int Step;
		}

		private Color[] colors;

		private Color[] highlights;

		private Color lineColor;

		private Color pinColor;

		private Cloth[] clothes;

		private float waveTimer;

		public float ClothDroopAmount = 0.6f;

		public Vector2 To;

		public Vector2 From => base.Entity.Position;

		public Flagline(Vector2 to, Color lineColor, Color pinColor, Color[] colors, int minFlagHeight, int maxFlagHeight, int minFlagLength, int maxFlagLength, int minSpace, int maxSpace)
			: base(active: true, visible: true)
		{
			To = to;
			this.colors = colors;
			this.lineColor = lineColor;
			this.pinColor = pinColor;
			waveTimer = Calc.Random.NextFloat() * ((float)Math.PI * 2f);
			highlights = new Color[colors.Length];
			for (int j = 0; j < colors.Length; j++)
			{
				highlights[j] = Color.Lerp(colors[j], Color.White, 0.1f);
			}
			clothes = new Cloth[10];
			for (int i = 0; i < clothes.Length; i++)
			{
				clothes[i] = new Cloth
				{
					Color = Calc.Random.Next(colors.Length),
					Height = Calc.Random.Next(minFlagHeight, maxFlagHeight),
					Length = Calc.Random.Next(minFlagLength, maxFlagLength),
					Step = Calc.Random.Next(minSpace, maxSpace)
				};
			}
		}

		public override void Update()
		{
			waveTimer += Engine.DeltaTime;
			base.Update();
		}

		public override void Render()
		{
			Vector2 from = ((From.X < To.X) ? From : To);
			Vector2 to = ((From.X < To.X) ? To : From);
			float length = (from - to).Length();
			float droop = length / 8f;
			SimpleCurve bezier = new SimpleCurve(from, to, (to + from) / 2f + Vector2.UnitY * (droop + (float)Math.Sin(waveTimer) * droop * 0.3f));
			Vector2 last = from;
			Vector2 next = from;
			float p = 0f;
			int index = 0;
			bool drawClothes = false;
			while (p < 1f)
			{
				Cloth cloth = clothes[index % clothes.Length];
				p += (float)(drawClothes ? cloth.Length : cloth.Step) / length;
				next = bezier.GetPoint(p);
				Draw.Line(last, next, lineColor);
				if (p < 1f && drawClothes)
				{
					float clothDroop = (float)cloth.Length * ClothDroopAmount;
					SimpleCurve clothBezier = new SimpleCurve(last, next, (last + next) / 2f + new Vector2(0f, clothDroop + (float)Math.Sin(waveTimer * 2f + p) * clothDroop * 0.4f));
					Vector2 clothLast = last;
					for (float i = 1f; i <= (float)cloth.Length; i += 1f)
					{
						Vector2 clothNext = clothBezier.GetPoint(i / (float)cloth.Length);
						if (clothNext.X != clothLast.X)
						{
							Draw.Rect(clothLast.X, clothLast.Y, clothNext.X - clothLast.X + 1f, cloth.Height, colors[cloth.Color]);
							clothLast = clothNext;
						}
					}
					Draw.Rect(last.X, last.Y, 1f, cloth.Height, highlights[cloth.Color]);
					Draw.Rect(next.X, next.Y, 1f, cloth.Height, highlights[cloth.Color]);
					Draw.Rect(last.X, last.Y - 1f, 1f, 3f, pinColor);
					Draw.Rect(next.X, next.Y - 1f, 1f, 3f, pinColor);
					index++;
				}
				last = next;
				drawClothes = !drawClothes;
			}
		}
	}
}
