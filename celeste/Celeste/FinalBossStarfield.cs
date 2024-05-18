using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class FinalBossStarfield : Backdrop
	{
		private struct Particle
		{
			public Vector2 Position;

			public Vector2 Direction;

			public float Speed;

			public Color Color;

			public float DirectionApproach;
		}

		public float Alpha = 1f;

		private const int particleCount = 200;

		private Particle[] particles = new Particle[200];

		private VertexPositionColor[] verts = new VertexPositionColor[1206];

		private static readonly Color[] colors = new Color[4]
		{
			Calc.HexToColor("030c1b"),
			Calc.HexToColor("0b031b"),
			Calc.HexToColor("1b0319"),
			Calc.HexToColor("0f0301")
		};

		public FinalBossStarfield()
		{
			UseSpritebatch = false;
			for (int i = 0; i < 200; i++)
			{
				particles[i].Speed = Calc.Random.Range(500f, 1200f);
				particles[i].Direction = new Vector2(-1f, 0f);
				particles[i].DirectionApproach = Calc.Random.Range(0.25f, 4f);
				particles[i].Position.X = Calc.Random.Range(0, 384);
				particles[i].Position.Y = Calc.Random.Range(0, 244);
				particles[i].Color = Calc.Random.Choose(colors);
			}
		}

		public override void Update(Scene scene)
		{
			base.Update(scene);
			if (Visible && Alpha > 0f)
			{
				Vector2 targetDirection = new Vector2(-1f, 0f);
				Level level = scene as Level;
				if (level.Bounds.Height > level.Bounds.Width)
				{
					targetDirection = new Vector2(0f, -1f);
				}
				float targetAngle = targetDirection.Angle();
				for (int i = 0; i < 200; i++)
				{
					particles[i].Position += particles[i].Direction * particles[i].Speed * Engine.DeltaTime;
					float angle = particles[i].Direction.Angle();
					angle = Calc.AngleApproach(angle, targetAngle, particles[i].DirectionApproach * Engine.DeltaTime);
					particles[i].Direction = Calc.AngleToVector(angle, 1f);
				}
			}
		}

		public override void Render(Scene scene)
		{
			Vector2 cam = (scene as Level).Camera.Position;
			Color bg = Color.Black * Alpha;
			verts[0].Color = bg;
			verts[0].Position = new Vector3(-10f, -10f, 0f);
			verts[1].Color = bg;
			verts[1].Position = new Vector3(330f, -10f, 0f);
			verts[2].Color = bg;
			verts[2].Position = new Vector3(330f, 190f, 0f);
			verts[3].Color = bg;
			verts[3].Position = new Vector3(-10f, -10f, 0f);
			verts[4].Color = bg;
			verts[4].Position = new Vector3(330f, 190f, 0f);
			verts[5].Color = bg;
			verts[5].Position = new Vector3(-10f, 190f, 0f);
			for (int i = 0; i < 200; i++)
			{
				int v = (i + 1) * 6;
				float length = Calc.ClampedMap(particles[i].Speed, 0f, 1200f, 1f, 64f);
				float width = Calc.ClampedMap(particles[i].Speed, 0f, 1200f, 3f, 0.6f);
				Vector2 normal = particles[i].Direction;
				Vector2 perp = normal.Perpendicular();
				Vector2 pos = particles[i].Position;
				pos.X = -32f + Mod(pos.X - cam.X * 0.9f, 384f);
				pos.Y = -32f + Mod(pos.Y - cam.Y * 0.9f, 244f);
				Vector2 a = pos - normal * length * 0.5f - perp * width;
				Vector2 b = pos + normal * length * 1f - perp * width;
				Vector2 c = pos + normal * length * 0.5f + perp * width;
				Vector2 d = pos - normal * length * 1f + perp * width;
				Color color = particles[i].Color * Alpha;
				verts[v].Color = color;
				verts[v].Position = new Vector3(a, 0f);
				verts[v + 1].Color = color;
				verts[v + 1].Position = new Vector3(b, 0f);
				verts[v + 2].Color = color;
				verts[v + 2].Position = new Vector3(c, 0f);
				verts[v + 3].Color = color;
				verts[v + 3].Position = new Vector3(a, 0f);
				verts[v + 4].Color = color;
				verts[v + 4].Position = new Vector3(c, 0f);
				verts[v + 5].Color = color;
				verts[v + 5].Position = new Vector3(d, 0f);
			}
			GFX.DrawVertices(Matrix.Identity, verts, verts.Length);
		}

		private float Mod(float x, float m)
		{
			return (x % m + m) % m;
		}
	}
}
