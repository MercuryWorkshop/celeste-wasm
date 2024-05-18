using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Monocle
{
	public class Camera
	{
		private Matrix matrix = Matrix.Identity;

		private Matrix inverse = Matrix.Identity;

		private bool changed;

		private Vector2 position = Vector2.Zero;

		private Vector2 zoom = Vector2.One;

		private Vector2 origin = Vector2.Zero;

		private float angle;

		public Viewport Viewport;

		public Matrix Matrix
		{
			get
			{
				if (changed)
				{
					UpdateMatrices();
				}
				return matrix;
			}
		}

		public Matrix Inverse
		{
			get
			{
				if (changed)
				{
					UpdateMatrices();
				}
				return inverse;
			}
		}

		public Vector2 Position
		{
			get
			{
				return position;
			}
			set
			{
				changed = true;
				position = value;
			}
		}

		public Vector2 Origin
		{
			get
			{
				return origin;
			}
			set
			{
				changed = true;
				origin = value;
			}
		}

		public float X
		{
			get
			{
				return position.X;
			}
			set
			{
				changed = true;
				position.X = value;
			}
		}

		public float Y
		{
			get
			{
				return position.Y;
			}
			set
			{
				changed = true;
				position.Y = value;
			}
		}

		public float Zoom
		{
			get
			{
				return zoom.X;
			}
			set
			{
				changed = true;
				zoom.X = (zoom.Y = value);
			}
		}

		public float Angle
		{
			get
			{
				return angle;
			}
			set
			{
				changed = true;
				angle = value;
			}
		}

		public float Left
		{
			get
			{
				if (changed)
				{
					UpdateMatrices();
				}
				return Vector2.Transform(Vector2.Zero, Inverse).X;
			}
			set
			{
				if (changed)
				{
					UpdateMatrices();
				}
				X = Vector2.Transform(Vector2.UnitX * value, Matrix).X;
			}
		}

		public float Right
		{
			get
			{
				if (changed)
				{
					UpdateMatrices();
				}
				return Vector2.Transform(Vector2.UnitX * Viewport.Width, Inverse).X;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public float Top
		{
			get
			{
				if (changed)
				{
					UpdateMatrices();
				}
				return Vector2.Transform(Vector2.Zero, Inverse).Y;
			}
			set
			{
				if (changed)
				{
					UpdateMatrices();
				}
				Y = Vector2.Transform(Vector2.UnitY * value, Matrix).Y;
			}
		}

		public float Bottom
		{
			get
			{
				if (changed)
				{
					UpdateMatrices();
				}
				return Vector2.Transform(Vector2.UnitY * Viewport.Height, Inverse).Y;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public Camera()
		{
			Viewport = default(Viewport);
			Viewport.Width = Engine.Width;
			Viewport.Height = Engine.Height;
			UpdateMatrices();
		}

		public Camera(int width, int height)
		{
			Viewport = default(Viewport);
			Viewport.Width = width;
			Viewport.Height = height;
			UpdateMatrices();
		}

		public override string ToString()
		{
			return "Camera:\n\tViewport: { " + Viewport.X + ", " + Viewport.Y + ", " + Viewport.Width + ", " + Viewport.Height + " }\n\tPosition: { " + position.X + ", " + position.Y + " }\n\tOrigin: { " + origin.X + ", " + origin.Y + " }\n\tZoom: { " + zoom.X + ", " + zoom.Y + " }\n\tAngle: " + angle;
		}

		private void UpdateMatrices()
		{
			matrix = Matrix.Identity * Matrix.CreateTranslation(new Vector3(-new Vector2((int)Math.Floor(position.X), (int)Math.Floor(position.Y)), 0f)) * Matrix.CreateRotationZ(angle) * Matrix.CreateScale(new Vector3(zoom, 1f)) * Matrix.CreateTranslation(new Vector3(new Vector2((int)Math.Floor(origin.X), (int)Math.Floor(origin.Y)), 0f));
			inverse = Matrix.Invert(matrix);
			changed = false;
		}

		public void CopyFrom(Camera other)
		{
			position = other.position;
			origin = other.origin;
			angle = other.angle;
			zoom = other.zoom;
			changed = true;
		}

		public void CenterOrigin()
		{
			origin = new Vector2((float)Viewport.Width / 2f, (float)Viewport.Height / 2f);
			changed = true;
		}

		public void RoundPosition()
		{
			position.X = (float)Math.Round(position.X);
			position.Y = (float)Math.Round(position.Y);
			changed = true;
		}

		public Vector2 ScreenToCamera(Vector2 position)
		{
			return Vector2.Transform(position, Inverse);
		}

		public Vector2 CameraToScreen(Vector2 position)
		{
			return Vector2.Transform(position, Matrix);
		}

		public void Approach(Vector2 position, float ease)
		{
			Position += (position - Position) * ease;
		}

		public void Approach(Vector2 position, float ease, float maxDistance)
		{
			Vector2 move = (position - Position) * ease;
			if (move.Length() > maxDistance)
			{
				Position += Vector2.Normalize(move) * maxDistance;
			}
			else
			{
				Position += move;
			}
		}
	}
}
