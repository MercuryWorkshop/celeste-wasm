using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class Godrays : Backdrop
	{
		private struct Ray
		{
			public float X;

			public float Y;

			public float Percent;

			public float Duration;

			public float Width;

			public float Length;

			public void Reset()
			{
				Percent = 0f;
				X = Calc.Random.NextFloat(384f);
				Y = Calc.Random.NextFloat(244f);
				Duration = 4f + Calc.Random.NextFloat() * 8f;
				Width = Calc.Random.Next(8, 16);
				Length = Calc.Random.Next(20, 40);
			}
		}

		private const int RayCount = 6;

		private VertexPositionColor[] vertices = new VertexPositionColor[36];

		private int vertexCount;

		private Color rayColor = Calc.HexToColor("f52b63") * 0.5f;

		private Ray[] rays = new Ray[6];

		private float fade;

		public Godrays()
		{
			UseSpritebatch = false;
			for (int i = 0; i < rays.Length; i++)
			{
				rays[i].Reset();
				rays[i].Percent = Calc.Random.NextFloat();
			}
		}

		public override void Update(Scene scene)
		{
			Level level = scene as Level;
			bool show = IsVisible(level);
			fade = Calc.Approach(fade, show ? 1 : 0, Engine.DeltaTime);
			Visible = fade > 0f;
			if (!Visible)
			{
				return;
			}
			Player player = level.Tracker.GetEntity<Player>();
			Vector2 angle = Calc.AngleToVector(-1.6707964f, 1f);
			Vector2 perp = new Vector2(0f - angle.Y, angle.X);
			int j = 0;
			for (int i = 0; i < rays.Length; i++)
			{
				if (rays[i].Percent >= 1f)
				{
					rays[i].Reset();
				}
				rays[i].Percent += Engine.DeltaTime / rays[i].Duration;
				rays[i].Y += 8f * Engine.DeltaTime;
				float p = rays[i].Percent;
				float rx = -32f + Mod(rays[i].X - level.Camera.X * 0.9f, 384f);
				float ry = -32f + Mod(rays[i].Y - level.Camera.Y * 0.9f, 244f);
				float rw = rays[i].Width;
				float rh = rays[i].Length;
				Vector2 pos = new Vector2((int)rx, (int)ry);
				Color color = rayColor * Ease.CubeInOut(Calc.Clamp(((p < 0.5f) ? p : (1f - p)) * 2f, 0f, 1f)) * fade;
				if (player != null)
				{
					float len = (pos + level.Camera.Position - player.Position).Length();
					if (len < 64f)
					{
						color *= 0.25f + 0.75f * (len / 64f);
					}
				}
				VertexPositionColor a = new VertexPositionColor(new Vector3(pos + perp * rw + angle * rh, 0f), color);
				VertexPositionColor b = new VertexPositionColor(new Vector3(pos - perp * rw, 0f), color);
				VertexPositionColor c = new VertexPositionColor(new Vector3(pos + perp * rw, 0f), color);
				VertexPositionColor d = new VertexPositionColor(new Vector3(pos - perp * rw - angle * rh, 0f), color);
				vertices[j++] = a;
				vertices[j++] = b;
				vertices[j++] = c;
				vertices[j++] = b;
				vertices[j++] = c;
				vertices[j++] = d;
			}
			vertexCount = j;
		}

		private float Mod(float x, float m)
		{
			return (x % m + m) % m;
		}

		public override void Render(Scene scene)
		{
			if (vertexCount > 0 && fade > 0f)
			{
				GFX.DrawVertices(Matrix.Identity, vertices, vertexCount);
			}
		}
	}
}
