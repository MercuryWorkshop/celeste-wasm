using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class Tentacles : Backdrop
	{
		public enum Side
		{
			Right,
			Left,
			Top,
			Bottom
		}

		private struct Tentacle
		{
			public float Length;

			public float Offset;

			public float Step;

			public float Position;

			public float Approach;

			public float Width;
		}

		private const int NodesPerTentacle = 10;

		private Side side;

		private float width;

		private Vector2 origin;

		private Vector2 outwards;

		private float outwardsOffset;

		private VertexPositionColor[] vertices;

		private int vertexCount;

		private Tentacle[] tentacles;

		private int tentacleCount;

		private float hideTimer = 5f;

		public Tentacles(Side side, Color color, float outwardsOffset)
		{
			this.side = side;
			this.outwardsOffset = outwardsOffset;
			UseSpritebatch = false;
			switch (side)
			{
			case Side.Right:
				outwards = new Vector2(-1f, 0f);
				width = 180f;
				origin = new Vector2(320f, 90f);
				break;
			case Side.Left:
				outwards = new Vector2(1f, 0f);
				width = 180f;
				origin = new Vector2(0f, 90f);
				break;
			case Side.Top:
				outwards = new Vector2(0f, 1f);
				width = 320f;
				origin = new Vector2(160f, 0f);
				break;
			case Side.Bottom:
				outwards = new Vector2(0f, -1f);
				width = 320f;
				origin = new Vector2(160f, 180f);
				break;
			}
			float w = 0f;
			tentacles = new Tentacle[100];
			for (int j = 0; j < tentacles.Length; j++)
			{
				if (!(w < width + 40f))
				{
					break;
				}
				tentacles[j].Length = Calc.Random.NextFloat();
				tentacles[j].Offset = Calc.Random.NextFloat();
				tentacles[j].Step = Calc.Random.NextFloat();
				tentacles[j].Position = -200f;
				tentacles[j].Approach = Calc.Random.NextFloat();
				w += (tentacles[j].Width = 6f + Calc.Random.NextFloat(20f));
				tentacleCount++;
			}
			vertices = new VertexPositionColor[tentacleCount * 11 * 6];
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i].Color = color;
			}
		}

		public override void Update(Scene scene)
		{
			bool num = IsVisible(scene as Level);
			float targetOffset = 0f;
			if (num)
			{
				Camera camera = (scene as Level).Camera;
				Player player = scene.Tracker.GetEntity<Player>();
				if (player != null)
				{
					if (side == Side.Right)
					{
						targetOffset = 320f - (player.X - camera.X) - 160f;
					}
					else if (side == Side.Bottom)
					{
						targetOffset = 180f - (player.Y - camera.Y) - 180f;
					}
				}
				hideTimer = 0f;
			}
			else
			{
				targetOffset = -200f;
				hideTimer += Engine.DeltaTime;
			}
			targetOffset += outwardsOffset;
			Visible = hideTimer < 5f;
			if (!Visible)
			{
				return;
			}
			Vector2 perp = -outwards.Perpendicular();
			int v = 0;
			Vector2 screenEdge = origin - perp * (width / 2f + 2f);
			for (int i = 0; i < tentacleCount; i++)
			{
				screenEdge += perp * tentacles[i].Width * 0.5f;
				tentacles[i].Position += (targetOffset - tentacles[i].Position) * (1f - (float)Math.Pow(0.5f * (0.5f + tentacles[i].Approach * 0.5f), Engine.DeltaTime));
				Vector2 outwardsPosition = (tentacles[i].Position + (float)Math.Sin(scene.TimeActive + tentacles[i].Offset * 4f) * 8f + (origin - screenEdge).Length() * 0.7f) * outwards;
				Vector2 start = screenEdge + outwardsPosition;
				float lengthPerNode = 2f + tentacles[i].Length * 8f;
				Vector2 last = start;
				Vector2 lastPerp = perp * tentacles[i].Width * 0.5f;
				vertices[v++].Position = new Vector3(screenEdge + lastPerp, 0f);
				vertices[v++].Position = new Vector3(screenEdge - lastPerp, 0f);
				vertices[v++].Position = new Vector3(start - lastPerp, 0f);
				vertices[v++].Position = new Vector3(start - lastPerp, 0f);
				vertices[v++].Position = new Vector3(screenEdge + lastPerp, 0f);
				vertices[v++].Position = new Vector3(start + lastPerp, 0f);
				for (int j = 1; j < 10; j++)
				{
					float num2 = scene.TimeActive * tentacles[i].Offset * (float)Math.Pow(1.100000023841858, j) * 2f;
					float waveOffset = tentacles[i].Offset * 3f + (float)j * (0.1f + tentacles[i].Step * 0.2f) + lengthPerNode * (float)j * 0.1f;
					float waveAmp = 2f + 4f * ((float)j / 10f);
					Vector2 wave = (float)Math.Sin(num2 + waveOffset) * perp * waveAmp;
					float size = (1f - (float)j / 10f) * tentacles[i].Width * 0.5f;
					Vector2 next = last + outwards * lengthPerNode + wave;
					Vector2 nextPerp = (last - next).SafeNormalize().Perpendicular() * size;
					vertices[v++].Position = new Vector3(last + lastPerp, 0f);
					vertices[v++].Position = new Vector3(last - lastPerp, 0f);
					vertices[v++].Position = new Vector3(next - nextPerp, 0f);
					vertices[v++].Position = new Vector3(next - nextPerp, 0f);
					vertices[v++].Position = new Vector3(last + lastPerp, 0f);
					vertices[v++].Position = new Vector3(next + nextPerp, 0f);
					last = next;
					lastPerp = nextPerp;
				}
				screenEdge += perp * tentacles[i].Width * 0.5f;
			}
			vertexCount = v;
		}

		public override void Render(Scene scene)
		{
			if (vertexCount > 0)
			{
				GFX.DrawVertices(Matrix.Identity, vertices, vertexCount);
			}
		}
	}
}
