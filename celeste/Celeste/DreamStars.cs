using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class DreamStars : Backdrop
	{
		private struct Stars
		{
			public Vector2 Position;

			public float Speed;

			public float Size;
		}

		private Stars[] stars = new Stars[50];

		private Vector2 angle = Vector2.Normalize(new Vector2(-2f, -7f));

		private Vector2 lastCamera = Vector2.Zero;

		public DreamStars()
		{
			for (int i = 0; i < stars.Length; i++)
			{
				stars[i].Position = new Vector2(Calc.Random.NextFloat(320f), Calc.Random.NextFloat(180f));
				stars[i].Speed = 24f + Calc.Random.NextFloat(24f);
				stars[i].Size = 2f + Calc.Random.NextFloat(6f);
			}
		}

		public override void Update(Scene scene)
		{
			base.Update(scene);
			Vector2 nextCamera = (scene as Level).Camera.Position;
			Vector2 deltaCamera = nextCamera - lastCamera;
			for (int i = 0; i < stars.Length; i++)
			{
				stars[i].Position += angle * stars[i].Speed * Engine.DeltaTime - deltaCamera * 0.5f;
			}
			lastCamera = nextCamera;
		}

		public override void Render(Scene scene)
		{
			for (int i = 0; i < stars.Length; i++)
			{
				Draw.HollowRect(new Vector2(mod(stars[i].Position.X, 320f), mod(stars[i].Position.Y, 180f)), stars[i].Size, stars[i].Size, Color.Teal);
			}
		}

		private float mod(float x, float m)
		{
			return (x % m + m) % m;
		}
	}
}
