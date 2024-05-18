using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class StarJumpController : Entity
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
				X = Calc.Random.NextFloat(320f);
				Y = Calc.Random.NextFloat(580f);
				Duration = 4f + Calc.Random.NextFloat() * 8f;
				Width = Calc.Random.Next(8, 80);
				Length = Calc.Random.Next(20, 200);
			}
		}

		private Level level;

		private Random random;

		private float minY;

		private float maxY;

		private float minX;

		private float maxX;

		private float cameraOffsetMarker;

		private float cameraOffsetTimer;

		public VirtualRenderTarget BlockFill;

		private const int RayCount = 100;

		private VertexPositionColor[] vertices = new VertexPositionColor[600];

		private int vertexCount;

		private Color rayColor = Calc.HexToColor("a3ffff") * 0.25f;

		private Ray[] rays = new Ray[100];

		public StarJumpController()
		{
			InitBlockFill();
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			level = SceneAs<Level>();
			minY = level.Bounds.Top + 80;
			maxY = level.Bounds.Top + 1800;
			minX = level.Bounds.Left + 80;
			maxX = level.Bounds.Right - 80;
			level.Session.Audio.Music.Event = "event:/music/lvl6/starjump";
			level.Session.Audio.Music.Layer(1, 1f);
			level.Session.Audio.Music.Layer(2, 0f);
			level.Session.Audio.Music.Layer(3, 0f);
			level.Session.Audio.Music.Layer(4, 0f);
			level.Session.Audio.Apply();
			random = new Random(666);
			Add(new BeforeRenderHook(BeforeRender));
		}

		public override void Update()
		{
			base.Update();
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				float y = player.CenterY;
				level.Session.Audio.Music.Layer(1, Calc.ClampedMap(y, maxY, minY, 1f, 0f));
				level.Session.Audio.Music.Layer(2, Calc.ClampedMap(y, maxY, minY));
				level.Session.Audio.Apply();
				if (level.CameraOffset.Y == -38.4f)
				{
					if (player.StateMachine.State != 19)
					{
						cameraOffsetTimer += Engine.DeltaTime;
						if (cameraOffsetTimer >= 0.5f)
						{
							cameraOffsetTimer = 0f;
							level.CameraOffset.Y = -12.8f;
						}
					}
					else
					{
						cameraOffsetTimer = 0f;
					}
				}
				else if (player.StateMachine.State == 19)
				{
					cameraOffsetTimer += Engine.DeltaTime;
					if (cameraOffsetTimer >= 0.1f)
					{
						cameraOffsetTimer = 0f;
						level.CameraOffset.Y = -38.4f;
					}
				}
				else
				{
					cameraOffsetTimer = 0f;
				}
				cameraOffsetMarker = level.Camera.Y;
			}
			else
			{
				level.Session.Audio.Music.Layer(1, 1f);
				level.Session.Audio.Music.Layer(2, 0f);
				level.Session.Audio.Apply();
			}
			UpdateBlockFill();
		}

		private void InitBlockFill()
		{
			for (int i = 0; i < rays.Length; i++)
			{
				rays[i].Reset();
				rays[i].Percent = Calc.Random.NextFloat();
			}
		}

		private void UpdateBlockFill()
		{
			Level level = base.Scene as Level;
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
				float rx = mod(rays[i].X - level.Camera.X * 0.9f, 320f);
				float ry = -200f + mod(rays[i].Y - level.Camera.Y * 0.7f, 580f);
				float rw = rays[i].Width;
				float rh = rays[i].Length;
				Vector2 pos = new Vector2((int)rx, (int)ry);
				Color color = rayColor * Ease.CubeInOut(Calc.YoYo(p));
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

		private void BeforeRender()
		{
			if (BlockFill == null)
			{
				BlockFill = VirtualContent.CreateRenderTarget("block-fill", 320, 180);
			}
			if (vertexCount > 0)
			{
				Engine.Graphics.GraphicsDevice.SetRenderTarget(BlockFill);
				Engine.Graphics.GraphicsDevice.Clear(Color.Lerp(Color.Black, Color.LightSkyBlue, 0.3f));
				GFX.DrawVertices(Matrix.Identity, vertices, vertexCount);
			}
		}

		public override void Removed(Scene scene)
		{
			Dispose();
			base.Removed(scene);
		}

		public override void SceneEnd(Scene scene)
		{
			Dispose();
			base.SceneEnd(scene);
		}

		private void Dispose()
		{
			if (BlockFill != null)
			{
				BlockFill.Dispose();
			}
			BlockFill = null;
		}

		private float mod(float x, float m)
		{
			return (x % m + m) % m;
		}
	}
}
