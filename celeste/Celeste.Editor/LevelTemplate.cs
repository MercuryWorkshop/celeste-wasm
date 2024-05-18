using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Editor
{
	public class LevelTemplate
	{
		public string Name;

		public LevelTemplateType Type;

		public int X;

		public int Y;

		public int Width;

		public int Height;

		public int ActualWidth;

		public int ActualHeight;

		public Grid Grid;

		public Grid Back;

		public List<Vector2> Spawns;

		public List<Vector2> Strawberries;

		public List<string> StrawberryMetadata;

		public List<Vector2> Checkpoints;

		public List<Rectangle> Jumpthrus;

		public bool Dummy;

		public int EditorColorIndex;

		private Vector2 moveAnchor;

		private Vector2 resizeAnchor;

		private List<Rectangle> solids = new List<Rectangle>();

		private List<Rectangle> backs = new List<Rectangle>();

		private static readonly Color bgTilesColor = Color.DarkSlateGray * 0.5f;

		private static readonly Color[] fgTilesColor = new Color[7]
		{
			Color.White,
			Calc.HexToColor("f6735e"),
			Calc.HexToColor("85f65e"),
			Calc.HexToColor("37d7e3"),
			Calc.HexToColor("376be3"),
			Calc.HexToColor("c337e3"),
			Calc.HexToColor("e33773")
		};

		private static readonly Color inactiveBorderColor = Color.DarkSlateGray;

		private static readonly Color selectedBorderColor = Color.Red;

		private static readonly Color hoveredBorderColor = Color.Yellow;

		private static readonly Color dummyBgTilesColor = Color.DarkSlateGray * 0.5f;

		private static readonly Color dummyFgTilesColor = Color.LightGray;

		private static readonly Color dummyInactiveBorderColor = Color.DarkOrange;

		private static readonly Color firstBorderColor = Color.Aqua;

		private Vector2 resizeHoldSize => new Vector2(Math.Min(Width, 4), Math.Min(Height, 4));

		public Rectangle Rect => new Rectangle(X, Y, Width, Height);

		public int Left
		{
			get
			{
				return X;
			}
			set
			{
				X = value;
			}
		}

		public int Top
		{
			get
			{
				return Y;
			}
			set
			{
				Y = value;
			}
		}

		public int Right
		{
			get
			{
				return X + Width;
			}
			set
			{
				X = value - Width;
			}
		}

		public int Bottom
		{
			get
			{
				return Y + Height;
			}
			set
			{
				Y = value - Height;
			}
		}

		public LevelTemplate(LevelData data)
		{
			Name = data.Name;
			EditorColorIndex = data.EditorColorIndex;
			X = data.Bounds.X / 8;
			Y = data.Bounds.Y / 8;
			ActualWidth = data.Bounds.Width;
			ActualHeight = data.Bounds.Height;
			Width = (int)Math.Ceiling((float)ActualWidth / 8f);
			Height = (int)Math.Ceiling((float)ActualHeight / 8f);
			Grid = new Grid(8f, 8f, data.Solids);
			Back = new Grid(8f, 8f, data.Bg);
			for (int k = 0; k < Height; k++)
			{
				for (int j = 0; j < Width; j++)
				{
					int t;
					for (t = 0; j + t < Width && Back[j + t, k] && !Grid[j + t, k]; t++)
					{
					}
					if (t > 0)
					{
						backs.Add(new Rectangle(j, k, t, 1));
						j += t - 1;
					}
				}
				for (int i = 0; i < Width; i++)
				{
					int t2;
					for (t2 = 0; i + t2 < Width && Grid[i + t2, k]; t2++)
					{
					}
					if (t2 > 0)
					{
						solids.Add(new Rectangle(i, k, t2, 1));
						i += t2 - 1;
					}
				}
			}
			Spawns = new List<Vector2>();
			foreach (Vector2 spawn in data.Spawns)
			{
				Spawns.Add(spawn / 8f - new Vector2(X, Y));
			}
			Strawberries = new List<Vector2>();
			StrawberryMetadata = new List<string>();
			Checkpoints = new List<Vector2>();
			Jumpthrus = new List<Rectangle>();
			foreach (EntityData entity in data.Entities)
			{
				if (entity.Name.Equals("strawberry") || entity.Name.Equals("snowberry"))
				{
					Strawberries.Add(entity.Position / 8f);
					StrawberryMetadata.Add(entity.Int("checkpointID") + ":" + entity.Int("order"));
				}
				else if (entity.Name.Equals("checkpoint"))
				{
					Checkpoints.Add(entity.Position / 8f);
				}
				else if (entity.Name.Equals("jumpThru"))
				{
					Jumpthrus.Add(new Rectangle((int)(entity.Position.X / 8f), (int)(entity.Position.Y / 8f), entity.Width / 8, 1));
				}
			}
			Dummy = data.Dummy;
			Type = LevelTemplateType.Level;
		}

		public LevelTemplate(int x, int y, int w, int h)
		{
			Name = "FILLER";
			X = x;
			Y = y;
			Width = w;
			Height = h;
			ActualWidth = w * 8;
			ActualHeight = h * 8;
			Type = LevelTemplateType.Filler;
		}

		public void RenderContents(Camera camera, List<LevelTemplate> allLevels)
		{
			if (Type == LevelTemplateType.Level)
			{
				bool collide = false;
				if (Engine.Scene.BetweenInterval(0.1f))
				{
					foreach (LevelTemplate level in allLevels)
					{
						if (level != this && level.Rect.Intersects(Rect))
						{
							collide = true;
							break;
						}
					}
				}
				Draw.Rect(X, Y, Width, Height, (collide ? Color.Red : Color.Black) * 0.5f);
				foreach (Rectangle back in backs)
				{
					Draw.Rect(X + back.X, Y + back.Y, back.Width, back.Height, Dummy ? dummyBgTilesColor : bgTilesColor);
				}
				foreach (Rectangle solid in solids)
				{
					Draw.Rect(X + solid.X, Y + solid.Y, solid.Width, solid.Height, Dummy ? dummyFgTilesColor : fgTilesColor[EditorColorIndex]);
				}
				foreach (Vector2 player in Spawns)
				{
					Draw.Rect((float)X + player.X, (float)Y + player.Y - 1f, 1f, 1f, Color.Red);
				}
				foreach (Vector2 strawberry in Strawberries)
				{
					Draw.HollowRect((float)X + strawberry.X - 1f, (float)Y + strawberry.Y - 2f, 3f, 3f, Color.LightPink);
				}
				foreach (Vector2 checkpoint in Checkpoints)
				{
					Draw.HollowRect((float)X + checkpoint.X - 1f, (float)Y + checkpoint.Y - 2f, 3f, 3f, Color.Lime);
				}
				{
					foreach (Rectangle jumpthru in Jumpthrus)
					{
						Draw.Rect(X + jumpthru.X, Y + jumpthru.Y, jumpthru.Width, 1f, Color.Yellow);
					}
					return;
				}
			}
			Draw.Rect(X, Y, Width, Height, dummyFgTilesColor);
			Draw.Rect((float)(X + Width) - resizeHoldSize.X, (float)(Y + Height) - resizeHoldSize.Y, resizeHoldSize.X, resizeHoldSize.Y, Color.Orange);
		}

		public void RenderOutline(Camera camera)
		{
			float t = 1f / camera.Zoom * 2f;
			if (Check(Vector2.Zero))
			{
				Outline(X + 1, Y + 1, Width - 2, Height - 2, t, firstBorderColor);
			}
			Outline(X, Y, Width, Height, t, Dummy ? dummyInactiveBorderColor : inactiveBorderColor);
		}

		public void RenderHighlight(Camera camera, bool hovered, bool selected)
		{
			if (selected || hovered)
			{
				float t = 1f / camera.Zoom * 2f;
				Outline(X, Y, Width, Height, t, hovered ? hoveredBorderColor : selectedBorderColor);
			}
		}

		private void Outline(float x, float y, float w, float h, float t, Color color)
		{
			Draw.Line(x, y, x + w, y, color, t);
			Draw.Line(x + w, y, x + w, y + h, color, t);
			Draw.Line(x, y + h, x + w, y + h, color, t);
			Draw.Line(x, y, x, y + h, color, t);
		}

		public bool Check(Vector2 point)
		{
			if (point.X >= (float)Left && point.Y >= (float)Top && point.X < (float)Right)
			{
				return point.Y < (float)Bottom;
			}
			return false;
		}

		public bool Check(Rectangle rect)
		{
			return Rect.Intersects(rect);
		}

		public void StartMoving()
		{
			moveAnchor = new Vector2(X, Y);
		}

		public void Move(Vector2 relativeMove, List<LevelTemplate> allLevels, bool snap)
		{
			X = (int)(moveAnchor.X + relativeMove.X);
			Y = (int)(moveAnchor.Y + relativeMove.Y);
			if (!snap)
			{
				return;
			}
			foreach (LevelTemplate other in allLevels)
			{
				if (other == this)
				{
					continue;
				}
				if (Bottom >= other.Top && Top <= other.Bottom)
				{
					bool num = Math.Abs(Left - other.Right) < 3;
					bool right = Math.Abs(Right - other.Left) < 3;
					if (num)
					{
						Left = other.Right;
					}
					else if (right)
					{
						Right = other.Left;
					}
					if (num || right)
					{
						if (Math.Abs(Top - other.Top) < 3)
						{
							Top = other.Top;
						}
						else if (Math.Abs(Bottom - other.Bottom) < 3)
						{
							Bottom = other.Bottom;
						}
					}
				}
				if (Right < other.Left || Left > other.Right)
				{
					continue;
				}
				bool num2 = Math.Abs(Top - other.Bottom) < 5;
				bool bottom = Math.Abs(Bottom - other.Top) < 5;
				if (num2)
				{
					Top = other.Bottom;
				}
				else if (bottom)
				{
					Bottom = other.Top;
				}
				if (num2 || bottom)
				{
					if (Math.Abs(Left - other.Left) < 3)
					{
						Left = other.Left;
					}
					else if (Math.Abs(Right - other.Right) < 3)
					{
						Right = other.Right;
					}
				}
			}
		}

		public void StartResizing()
		{
			resizeAnchor = new Vector2(Width, Height);
		}

		public void Resize(Vector2 relativeMove)
		{
			Width = Math.Max(1, (int)(resizeAnchor.X + relativeMove.X));
			Height = Math.Max(1, (int)(resizeAnchor.Y + relativeMove.Y));
			ActualWidth = Width * 8;
			ActualHeight = Height * 8;
		}

		public bool ResizePosition(Vector2 mouse)
		{
			if (mouse.X > (float)(X + Width) - resizeHoldSize.X && mouse.Y > (float)(Y + Height) - resizeHoldSize.Y && mouse.X < (float)(X + Width))
			{
				return mouse.Y < (float)(Y + Height);
			}
			return false;
		}
	}
}
