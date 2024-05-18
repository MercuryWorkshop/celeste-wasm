using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class Water : Entity
	{
		public class Ripple
		{
			public float Position;

			public float Speed;

			public float Height;

			public float Percent;

			public float Duration;
		}

		public class Tension
		{
			public float Position;

			public float Strength;
		}

		public class Ray
		{
			public float Position;

			public float Percent;

			public float Duration;

			public float Width;

			public float Length;

			private float MaxWidth;

			public Ray(float maxWidth)
			{
				MaxWidth = maxWidth;
				Reset(Calc.Random.NextFloat());
			}

			public void Reset(float percent)
			{
				Position = Calc.Random.NextFloat() * MaxWidth;
				Percent = percent;
				Duration = Calc.Random.Range(2f, 8f);
				Width = Calc.Random.Range(2, 16);
				Length = Calc.Random.Range(8f, 128f);
			}
		}

		public class Surface
		{
			public const int Resolution = 4;

			public const float RaysPerPixel = 0.2f;

			public const float BaseHeight = 6f;

			public readonly Vector2 Outwards;

			public readonly int Width;

			public readonly int BodyHeight;

			public Vector2 Position;

			public List<Ripple> Ripples = new List<Ripple>();

			public List<Ray> Rays = new List<Ray>();

			public List<Tension> Tensions = new List<Tension>();

			private float timer;

			private VertexPositionColor[] mesh;

			private int fillStartIndex;

			private int rayStartIndex;

			private int surfaceStartIndex;

			public Surface(Vector2 position, Vector2 outwards, float width, float bodyHeight)
			{
				Position = position;
				Outwards = outwards;
				Width = (int)width;
				BodyHeight = (int)bodyHeight;
				int columns = (int)(width / 4f);
				int rays = (int)(width * 0.2f);
				Rays = new List<Ray>();
				for (int l = 0; l < rays; l++)
				{
					Rays.Add(new Ray(width));
				}
				fillStartIndex = 0;
				rayStartIndex = columns * 6;
				surfaceStartIndex = (columns + rays) * 6;
				mesh = new VertexPositionColor[(columns * 2 + rays) * 6];
				for (int k = fillStartIndex; k < fillStartIndex + columns * 6; k++)
				{
					mesh[k].Color = FillColor;
				}
				for (int j = rayStartIndex; j < rayStartIndex + rays * 6; j++)
				{
					mesh[j].Color = Color.Transparent;
				}
				for (int i = surfaceStartIndex; i < surfaceStartIndex + columns * 6; i++)
				{
					mesh[i].Color = SurfaceColor;
				}
			}

			public float GetPointAlong(Vector2 position)
			{
				Vector2 perp = Outwards.Perpendicular();
				Vector2 vector = Position + perp * (-Width / 2);
				Vector2 right = Position + perp * (Width / 2);
				Vector2 pos = Calc.ClosestPointOnLine(vector, right, position);
				return (vector - pos).Length();
			}

			public Tension SetTension(Vector2 position, float strength)
			{
				Tension tension = new Tension
				{
					Position = GetPointAlong(position),
					Strength = strength
				};
				Tensions.Add(tension);
				return tension;
			}

			public void RemoveTension(Tension tension)
			{
				Tensions.Remove(tension);
			}

			public void DoRipple(Vector2 position, float multiplier)
			{
				float speed = 80f;
				float duration = 3f;
				float along = GetPointAlong(position);
				int height = 2;
				if (Width < 200)
				{
					duration *= Calc.ClampedMap(Width, 0f, 200f, 0.25f);
					multiplier *= Calc.ClampedMap(Width, 0f, 200f, 0.5f);
				}
				Ripples.Add(new Ripple
				{
					Position = along,
					Speed = 0f - speed,
					Height = (float)height * multiplier,
					Percent = 0f,
					Duration = duration
				});
				Ripples.Add(new Ripple
				{
					Position = along,
					Speed = speed,
					Height = (float)height * multiplier,
					Percent = 0f,
					Duration = duration
				});
			}

			public void Update()
			{
				timer += Engine.DeltaTime;
				Vector2 perp = Outwards.Perpendicular();
				for (int k = Ripples.Count - 1; k >= 0; k--)
				{
					Ripple ripple = Ripples[k];
					if (ripple.Percent > 1f)
					{
						Ripples.RemoveAt(k);
					}
					else
					{
						ripple.Position += ripple.Speed * Engine.DeltaTime;
						if (ripple.Position < 0f || ripple.Position > (float)Width)
						{
							ripple.Speed = 0f - ripple.Speed;
							ripple.Position = Calc.Clamp(ripple.Position, 0f, Width);
						}
						ripple.Percent += Engine.DeltaTime / ripple.Duration;
					}
				}
				int pos2 = 0;
				int j = fillStartIndex;
				int l = surfaceStartIndex;
				while (pos2 < Width)
				{
					int a = pos2;
					float aHeight = GetSurfaceHeight(a);
					int b = Math.Min(pos2 + 4, Width);
					float bHeight = GetSurfaceHeight(b);
					mesh[j].Position = new Vector3(Position + perp * (-Width / 2 + a) + Outwards * aHeight, 0f);
					mesh[j + 1].Position = new Vector3(Position + perp * (-Width / 2 + b) + Outwards * bHeight, 0f);
					mesh[j + 2].Position = new Vector3(Position + perp * (-Width / 2 + a), 0f);
					mesh[j + 3].Position = new Vector3(Position + perp * (-Width / 2 + b) + Outwards * bHeight, 0f);
					mesh[j + 4].Position = new Vector3(Position + perp * (-Width / 2 + b), 0f);
					mesh[j + 5].Position = new Vector3(Position + perp * (-Width / 2 + a), 0f);
					mesh[l].Position = new Vector3(Position + perp * (-Width / 2 + a) + Outwards * (aHeight + 1f), 0f);
					mesh[l + 1].Position = new Vector3(Position + perp * (-Width / 2 + b) + Outwards * (bHeight + 1f), 0f);
					mesh[l + 2].Position = new Vector3(Position + perp * (-Width / 2 + a) + Outwards * aHeight, 0f);
					mesh[l + 3].Position = new Vector3(Position + perp * (-Width / 2 + b) + Outwards * (bHeight + 1f), 0f);
					mesh[l + 4].Position = new Vector3(Position + perp * (-Width / 2 + b) + Outwards * bHeight, 0f);
					mesh[l + 5].Position = new Vector3(Position + perp * (-Width / 2 + a) + Outwards * aHeight, 0f);
					pos2 += 4;
					j += 6;
					l += 6;
				}
				Vector2 pos = Position + perp * ((float)(-Width) / 2f);
				int i = rayStartIndex;
				foreach (Ray ray in Rays)
				{
					if (ray.Percent > 1f)
					{
						ray.Reset(0f);
					}
					ray.Percent += Engine.DeltaTime / ray.Duration;
					float alpha = 1f;
					if (ray.Percent < 0.1f)
					{
						alpha = Calc.ClampedMap(ray.Percent, 0f, 0.1f);
					}
					else if (ray.Percent > 0.9f)
					{
						alpha = Calc.ClampedMap(ray.Percent, 0.9f, 1f, 1f, 0f);
					}
					float left = Math.Max(0f, ray.Position - ray.Width / 2f);
					float right = Math.Min(Width, ray.Position + ray.Width / 2f);
					float moveDown = Math.Min(BodyHeight, 0.7f * ray.Length);
					float moveSide = 0.3f * ray.Length;
					Vector2 a2 = pos + perp * left + Outwards * GetSurfaceHeight(left);
					Vector2 b2 = pos + perp * right + Outwards * GetSurfaceHeight(right);
					Vector2 c = pos + perp * (right - moveSide) - Outwards * moveDown;
					Vector2 d = pos + perp * (left - moveSide) - Outwards * moveDown;
					mesh[i].Position = new Vector3(a2, 0f);
					mesh[i].Color = RayTopColor * alpha;
					mesh[i + 1].Position = new Vector3(b2, 0f);
					mesh[i + 1].Color = RayTopColor * alpha;
					mesh[i + 2].Position = new Vector3(d, 0f);
					mesh[i + 3].Position = new Vector3(b2, 0f);
					mesh[i + 3].Color = RayTopColor * alpha;
					mesh[i + 4].Position = new Vector3(c, 0f);
					mesh[i + 5].Position = new Vector3(d, 0f);
					i += 6;
				}
			}

			public float GetSurfaceHeight(Vector2 position)
			{
				return GetSurfaceHeight(GetPointAlong(position));
			}

			public float GetSurfaceHeight(float position)
			{
				if (position < 0f || position > (float)Width)
				{
					return 0f;
				}
				float height = 0f;
				foreach (Ripple ripple in Ripples)
				{
					float dist2 = Math.Abs(ripple.Position - position);
					float amount = 0f;
					amount = ((!(dist2 < 12f)) ? Calc.ClampedMap(dist2, 16f, 32f, -0.75f, 0f) : Calc.ClampedMap(dist2, 0f, 16f, 1f, -0.75f));
					height += amount * ripple.Height * Ease.CubeIn(1f - ripple.Percent);
				}
				height = Calc.Clamp(height, -4f, 4f);
				foreach (Tension tension in Tensions)
				{
					float dist = Calc.ClampedMap(Math.Abs(tension.Position - position), 0f, 24f, 1f, 0f);
					height += Ease.CubeOut(dist) * tension.Strength * 12f;
				}
				float p = position / (float)Width;
				height *= Calc.ClampedMap(p, 0f, 0.1f, 0.5f);
				height *= Calc.ClampedMap(p, 0.9f, 1f, 1f, 0.5f);
				height += (float)Math.Sin(timer + position * 0.1f);
				return height + 6f;
			}

			public void Render(Camera camera)
			{
				GFX.DrawVertices(camera.Matrix, mesh, mesh.Length);
			}
		}

		public static ParticleType P_Splash;

		public static readonly Color FillColor = Color.LightSkyBlue * 0.3f;

		public static readonly Color SurfaceColor = Color.LightSkyBlue * 0.8f;

		public static readonly Color RayTopColor = Color.LightSkyBlue * 0.6f;

		public static readonly Vector2 RayAngle = new Vector2(-4f, 8f).SafeNormalize();

		public Surface TopSurface;

		public Surface BottomSurface;

		public List<Surface> Surfaces = new List<Surface>();

		private Rectangle fill;

		private bool[,] grid;

		private Tension playerBottomTension;

		private HashSet<WaterInteraction> contains = new HashSet<WaterInteraction>();

		public Water(EntityData data, Vector2 offset)
			: this(data.Position + offset, topSurface: true, data.Bool("hasBottom"), data.Width, data.Height)
		{
		}

		public Water(Vector2 position, bool topSurface, bool bottomSurface, float width, float height)
		{
			Position = position;
			base.Tag = Tags.TransitionUpdate;
			base.Depth = -9999;
			base.Collider = new Hitbox(width, height);
			grid = new bool[(int)(width / 8f), (int)(height / 8f)];
			fill = new Rectangle(0, 0, (int)width, (int)height);
			int padding = 8;
			if (topSurface)
			{
				TopSurface = new Surface(Position + new Vector2(width / 2f, padding), new Vector2(0f, -1f), width, height);
				Surfaces.Add(TopSurface);
				fill.Y += padding;
				fill.Height -= padding;
			}
			if (bottomSurface)
			{
				BottomSurface = new Surface(Position + new Vector2(width / 2f, height - (float)padding), new Vector2(0f, 1f), width, height);
				Surfaces.Add(BottomSurface);
				fill.Height -= padding;
			}
			Add(new DisplacementRenderHook(RenderDisplacement));
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			int tx = 0;
			for (int tw = grid.GetLength(0); tx < tw; tx++)
			{
				int ty = 0;
				for (int th = grid.GetLength(1); ty < th; ty++)
				{
					grid[tx, ty] = !base.Scene.CollideCheck<Solid>(new Rectangle((int)base.X + tx * 8, (int)base.Y + ty * 8, 8, 8));
				}
			}
		}

		public override void Update()
		{
			base.Update();
			foreach (Surface surface in Surfaces)
			{
				surface.Update();
			}
			foreach (WaterInteraction component in base.Scene.Tracker.GetComponents<WaterInteraction>())
			{
				Entity entity = component.Entity;
				bool wasInside = contains.Contains(component);
				bool isInside = CollideCheck(entity);
				if (wasInside != isInside)
				{
					if (entity.Center.Y <= base.Center.Y && TopSurface != null)
					{
						TopSurface.DoRipple(entity.Center, 1f);
					}
					else if (entity.Center.Y > base.Center.Y && BottomSurface != null)
					{
						BottomSurface.DoRipple(entity.Center, 1f);
					}
					bool dash = component.IsDashing();
					int deep = ((entity.Center.Y < base.Center.Y && !base.Scene.CollideCheck<Solid>(new Rectangle((int)entity.Center.X - 4, (int)entity.Center.Y, 8, 16))) ? 1 : 0);
					if (wasInside)
					{
						if (dash)
						{
							Audio.Play("event:/char/madeline/water_dash_out", entity.Center, "deep", deep);
						}
						else
						{
							Audio.Play("event:/char/madeline/water_out", entity.Center, "deep", deep);
						}
						component.DrippingTimer = 2f;
					}
					else
					{
						if (dash && deep == 1)
						{
							Audio.Play("event:/char/madeline/water_dash_in", entity.Center, "deep", deep);
						}
						else
						{
							Audio.Play("event:/char/madeline/water_in", entity.Center, "deep", deep);
						}
						component.DrippingTimer = 0f;
					}
					if (wasInside)
					{
						contains.Remove(component);
					}
					else
					{
						contains.Add(component);
					}
				}
				if (BottomSurface == null || !(entity is Player))
				{
					continue;
				}
				if (isInside && entity.Y > base.Bottom - 8f)
				{
					if (playerBottomTension == null)
					{
						playerBottomTension = BottomSurface.SetTension(entity.Position, 0f);
					}
					playerBottomTension.Position = BottomSurface.GetPointAlong(entity.Position);
					playerBottomTension.Strength = Calc.ClampedMap(entity.Y, base.Bottom - 8f, base.Bottom + 4f);
				}
				else if (playerBottomTension != null)
				{
					BottomSurface.RemoveTension(playerBottomTension);
					playerBottomTension = null;
				}
			}
		}

		public void RenderDisplacement()
		{
			Color color = new Color(0.5f, 0.5f, 0.25f, 1f);
			int tx = 0;
			int tw = grid.GetLength(0);
			int th = grid.GetLength(1);
			for (; tx < tw; tx++)
			{
				if (th > 0 && grid[tx, 0])
				{
					Draw.Rect(base.X + (float)(tx * 8), base.Y + 3f, 8f, 5f, color);
				}
				for (int ty = 1; ty < th; ty++)
				{
					if (grid[tx, ty])
					{
						int height;
						for (height = 1; ty + height < th && grid[tx, ty + height]; height++)
						{
						}
						Draw.Rect(base.X + (float)(tx * 8), base.Y + (float)(ty * 8), 8f, height * 8, color);
						ty += height - 1;
					}
				}
			}
		}

		public override void Render()
		{
			Draw.Rect(base.X + (float)fill.X, base.Y + (float)fill.Y, fill.Width, fill.Height, FillColor);
			GameplayRenderer.End();
			foreach (Surface surface in Surfaces)
			{
				surface.Render((base.Scene as Level).Camera);
			}
			GameplayRenderer.Begin();
		}
	}
}
