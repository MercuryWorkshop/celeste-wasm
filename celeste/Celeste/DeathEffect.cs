using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class DeathEffect : Component
	{
		public Vector2 Position;

		public Color Color;

		public float Percent;

		public float Duration = 0.834f;

		public Action<float> OnUpdate;

		public Action OnEnd;

		public DeathEffect(Color color, Vector2? offset = null)
			: base(active: true, visible: true)
		{
			Color = color;
			Position = (offset.HasValue ? offset.Value : Vector2.Zero);
			Percent = 0f;
		}

		public override void Update()
		{
			base.Update();
			if (Percent > 1f)
			{
				RemoveSelf();
				if (OnEnd != null)
				{
					OnEnd();
				}
			}
			Percent = Calc.Approach(Percent, 1f, Engine.DeltaTime / Duration);
			if (OnUpdate != null)
			{
				OnUpdate(Percent);
			}
		}

		public override void Render()
		{
			Draw(base.Entity.Position + Position, Color, Percent);
		}

		public static void Draw(Vector2 position, Color color, float ease)
		{
			Color col = ((Math.Floor(ease * 10f) % 2.0 == 0.0) ? color : Color.White);
			MTexture tex = GFX.Game["characters/player/hair00"];
			float s = ((ease < 0.5f) ? (0.5f + ease) : Ease.CubeOut(1f - (ease - 0.5f) * 2f));
			for (int j = 0; j < 8; j++)
			{
				Vector2 angle = Calc.AngleToVector(((float)j / 8f + ease * 0.25f) * ((float)Math.PI * 2f), Ease.CubeOut(ease) * 24f);
				tex.DrawCentered(position + angle + new Vector2(-1f, 0f), Color.Black, new Vector2(s, s));
				tex.DrawCentered(position + angle + new Vector2(1f, 0f), Color.Black, new Vector2(s, s));
				tex.DrawCentered(position + angle + new Vector2(0f, -1f), Color.Black, new Vector2(s, s));
				tex.DrawCentered(position + angle + new Vector2(0f, 1f), Color.Black, new Vector2(s, s));
			}
			for (int i = 0; i < 8; i++)
			{
				Vector2 angle2 = Calc.AngleToVector(((float)i / 8f + ease * 0.25f) * ((float)Math.PI * 2f), Ease.CubeOut(ease) * 24f);
				tex.DrawCentered(position + angle2, col, new Vector2(s, s));
			}
		}
	}
}
