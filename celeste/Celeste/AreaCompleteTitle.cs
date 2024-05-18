using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class AreaCompleteTitle : Entity
	{
		public class Letter
		{
			public string Value;

			public Vector2 Position;

			public Color Color = Color.White;

			public Color Shadow = Color.Black;

			private float delay;

			private float ease;

			private Vector2 scale;

			private SimpleCurve curve;

			public Letter(int index, string value, Vector2 position)
			{
				Value = value;
				Position = position;
				delay = 0.2f + (float)index * 0.02f;
				curve = new SimpleCurve(position + Vector2.UnitY * 60f, position, position - Vector2.UnitY * 100f);
				scale = new Vector2(0.75f, 1.5f);
			}

			public void Update()
			{
				scale.X = Calc.Approach(scale.X, 1f, 3f * Engine.DeltaTime);
				scale.Y = Calc.Approach(scale.Y, 1f, 3f * Engine.DeltaTime);
				if (delay > 0f)
				{
					delay -= Engine.DeltaTime;
				}
				else if (ease < 1f)
				{
					ease += 4f * Engine.DeltaTime;
					if (ease >= 1f)
					{
						ease = 1f;
						scale = new Vector2(1.5f, 0.75f);
					}
				}
			}

			public void Render(Vector2 offset, float scale, float alphaMultiplier)
			{
				if (ease > 0f)
				{
					Vector2 at = offset + curve.GetPoint(ease);
					float alpha = Calc.LerpClamp(0f, 1f, ease * 3f) * alphaMultiplier;
					Vector2 scaled = this.scale * scale;
					if (alpha < 1f)
					{
						ActiveFont.Draw(Value, at, new Vector2(0.5f, 1f), scaled, Color * alpha);
						return;
					}
					ActiveFont.Draw(Value, at + Vector2.UnitY * 3.5f * scale, new Vector2(0.5f, 1f), scaled, Shadow);
					ActiveFont.DrawOutline(Value, at, new Vector2(0.5f, 1f), scaled, Color, 2f, Shadow);
				}
			}
		}

		public float Alpha = 1f;

		private Vector2 origin;

		private List<Letter> letters = new List<Letter>();

		private float rectangleEase;

		private float scale;

		public AreaCompleteTitle(Vector2 origin, string text, float scale, bool rainbow = false)
		{
			this.origin = origin;
			this.scale = scale;
			Vector2 totalSize = ActiveFont.Measure(text) * scale;
			Vector2 at = origin + Vector2.UnitY * totalSize.Y * 0.5f + Vector2.UnitX * totalSize.X * -0.5f;
			for (int i = 0; i < text.Length; i++)
			{
				Vector2 size = ActiveFont.Measure(text[i].ToString()) * scale;
				if (text[i] != ' ')
				{
					Letter letter = new Letter(i, text[i].ToString(), at + Vector2.UnitX * size.X * 0.5f);
					if (rainbow)
					{
						float hue = (float)i / (float)text.Length;
						letter.Color = Calc.HsvToColor(hue, 0.8f, 0.9f);
						letter.Shadow = Color.Lerp(letter.Color, Color.Black, 0.7f);
					}
					letters.Add(letter);
				}
				at += Vector2.UnitX * size.X;
			}
			Alarm.Set(this, 2.6f, delegate
			{
				Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, 0.5f, start: true);
				tween.OnUpdate = delegate(Tween t)
				{
					rectangleEase = t.Eased;
				};
				Add(tween);
			});
		}

		public override void Update()
		{
			base.Update();
			foreach (Letter letter in letters)
			{
				letter.Update();
			}
		}

		public void DrawLineUI()
		{
			Draw.Rect(base.X, base.Y + origin.Y - 40f, 1920f * rectangleEase, 80f, Color.Black * 0.65f);
		}

		public override void Render()
		{
			base.Render();
			foreach (Letter letter in letters)
			{
				letter.Render(Position, scale, Alpha);
			}
		}
	}
}
