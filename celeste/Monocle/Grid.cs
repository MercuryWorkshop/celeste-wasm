using System;
using Microsoft.Xna.Framework;

namespace Monocle
{
	public class Grid : Collider
	{
		public VirtualMap<bool> Data;

		public float CellWidth { get; private set; }

		public float CellHeight { get; private set; }

		public bool this[int x, int y]
		{
			get
			{
				if (x >= 0 && y >= 0 && x < CellsX && y < CellsY)
				{
					return Data[x, y];
				}
				return false;
			}
			set
			{
				Data[x, y] = value;
			}
		}

		public int CellsX => Data.Columns;

		public int CellsY => Data.Rows;

		public override float Width
		{
			get
			{
				return CellWidth * (float)CellsX;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public override float Height
		{
			get
			{
				return CellHeight * (float)CellsY;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public bool IsEmpty
		{
			get
			{
				for (int i = 0; i < CellsX; i++)
				{
					for (int j = 0; j < CellsY; j++)
					{
						if (Data[i, j])
						{
							return false;
						}
					}
				}
				return true;
			}
		}

		public override float Left
		{
			get
			{
				return Position.X;
			}
			set
			{
				Position.X = value;
			}
		}

		public override float Top
		{
			get
			{
				return Position.Y;
			}
			set
			{
				Position.Y = value;
			}
		}

		public override float Right
		{
			get
			{
				return Position.X + Width;
			}
			set
			{
				Position.X = value - Width;
			}
		}

		public override float Bottom
		{
			get
			{
				return Position.Y + Height;
			}
			set
			{
				Position.Y = value - Height;
			}
		}

		public Grid(int cellsX, int cellsY, float cellWidth, float cellHeight)
		{
			Data = new VirtualMap<bool>(cellsX, cellsY, emptyValue: false);
			CellWidth = cellWidth;
			CellHeight = cellHeight;
		}

		public Grid(float cellWidth, float cellHeight, string bitstring)
		{
			CellWidth = cellWidth;
			CellHeight = cellHeight;
			int longest = 0;
			int currentX = 0;
			int currentY = 1;
			for (int i = 0; i < bitstring.Length; i++)
			{
				if (bitstring[i] == '\n')
				{
					currentY++;
					longest = Math.Max(currentX, longest);
					currentX = 0;
				}
				else
				{
					currentX++;
				}
			}
			Data = new VirtualMap<bool>(longest, currentY, emptyValue: false);
			LoadBitstring(bitstring);
		}

		public Grid(float cellWidth, float cellHeight, bool[,] data)
		{
			CellWidth = cellWidth;
			CellHeight = cellHeight;
			Data = new VirtualMap<bool>(data, emptyValue: false);
		}

		public Grid(float cellWidth, float cellHeight, VirtualMap<bool> data)
		{
			CellWidth = cellWidth;
			CellHeight = cellHeight;
			Data = data;
		}

		public void Extend(int left, int right, int up, int down)
		{
			Position -= new Vector2((float)left * CellWidth, (float)up * CellHeight);
			int newWidth = Data.Columns + left + right;
			int newHeight = Data.Rows + up + down;
			if (newWidth <= 0 || newHeight <= 0)
			{
				Data = new VirtualMap<bool>(0, 0, emptyValue: false);
				return;
			}
			VirtualMap<bool> newData = new VirtualMap<bool>(newWidth, newHeight, emptyValue: false);
			for (int x5 = 0; x5 < Data.Columns; x5++)
			{
				for (int y = 0; y < Data.Rows; y++)
				{
					int atX = x5 + left;
					int atY = y + up;
					if (atX >= 0 && atX < newWidth && atY >= 0 && atY < newHeight)
					{
						newData[atX, atY] = Data[x5, y];
					}
				}
			}
			for (int x4 = 0; x4 < left; x4++)
			{
				for (int y2 = 0; y2 < newHeight; y2++)
				{
					newData[x4, y2] = Data[0, Calc.Clamp(y2 - up, 0, Data.Rows - 1)];
				}
			}
			for (int x3 = newWidth - right; x3 < newWidth; x3++)
			{
				for (int y3 = 0; y3 < newHeight; y3++)
				{
					newData[x3, y3] = Data[Data.Columns - 1, Calc.Clamp(y3 - up, 0, Data.Rows - 1)];
				}
			}
			for (int y5 = 0; y5 < up; y5++)
			{
				for (int x = 0; x < newWidth; x++)
				{
					newData[x, y5] = Data[Calc.Clamp(x - left, 0, Data.Columns - 1), 0];
				}
			}
			for (int y4 = newHeight - down; y4 < newHeight; y4++)
			{
				for (int x2 = 0; x2 < newWidth; x2++)
				{
					newData[x2, y4] = Data[Calc.Clamp(x2 - left, 0, Data.Columns - 1), Data.Rows - 1];
				}
			}
			Data = newData;
		}

		public void LoadBitstring(string bitstring)
		{
			int x = 0;
			int y = 0;
			for (int i = 0; i < bitstring.Length; i++)
			{
				if (bitstring[i] == '\n')
				{
					for (; x < CellsX; x++)
					{
						Data[x, y] = false;
					}
					x = 0;
					y++;
					if (y >= CellsY)
					{
						break;
					}
				}
				else if (x < CellsX)
				{
					if (bitstring[i] == '0')
					{
						Data[x, y] = false;
						x++;
					}
					else
					{
						Data[x, y] = true;
						x++;
					}
				}
			}
		}

		public string GetBitstring()
		{
			string bits = "";
			for (int y = 0; y < CellsY; y++)
			{
				if (y != 0)
				{
					bits += "\n";
				}
				for (int x = 0; x < CellsX; x++)
				{
					bits = ((!Data[x, y]) ? (bits + "0") : (bits + "1"));
				}
			}
			return bits;
		}

		public void Clear(bool to = false)
		{
			for (int i = 0; i < CellsX; i++)
			{
				for (int j = 0; j < CellsY; j++)
				{
					Data[i, j] = to;
				}
			}
		}

		public void SetRect(int x, int y, int width, int height, bool to = true)
		{
			if (x < 0)
			{
				width += x;
				x = 0;
			}
			if (y < 0)
			{
				height += y;
				y = 0;
			}
			if (x + width > CellsX)
			{
				width = CellsX - x;
			}
			if (y + height > CellsY)
			{
				height = CellsY - y;
			}
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					Data[x + i, y + j] = to;
				}
			}
		}

		public bool CheckRect(int x, int y, int width, int height)
		{
			if (x < 0)
			{
				width += x;
				x = 0;
			}
			if (y < 0)
			{
				height += y;
				y = 0;
			}
			if (x + width > CellsX)
			{
				width = CellsX - x;
			}
			if (y + height > CellsY)
			{
				height = CellsY - y;
			}
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					if (Data[x + i, y + j])
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool CheckColumn(int x)
		{
			for (int i = 0; i < CellsY; i++)
			{
				if (!Data[x, i])
				{
					return false;
				}
			}
			return true;
		}

		public bool CheckRow(int y)
		{
			for (int i = 0; i < CellsX; i++)
			{
				if (!Data[i, y])
				{
					return false;
				}
			}
			return true;
		}

		public override Collider Clone()
		{
			return new Grid(CellWidth, CellHeight, Data.Clone());
		}

		public override void Render(Camera camera, Color color)
		{
			if (camera == null)
			{
				for (int i = 0; i < CellsX; i++)
				{
					for (int j = 0; j < CellsY; j++)
					{
						if (Data[i, j])
						{
							Draw.HollowRect(base.AbsoluteLeft + (float)i * CellWidth, base.AbsoluteTop + (float)j * CellHeight, CellWidth, CellHeight, color);
						}
					}
				}
				return;
			}
			int num = (int)Math.Max(0f, (camera.Left - base.AbsoluteLeft) / CellWidth);
			int right = (int)Math.Min(CellsX - 1, Math.Ceiling((camera.Right - base.AbsoluteLeft) / CellWidth));
			int top = (int)Math.Max(0f, (camera.Top - base.AbsoluteTop) / CellHeight);
			int bottom = (int)Math.Min(CellsY - 1, Math.Ceiling((camera.Bottom - base.AbsoluteTop) / CellHeight));
			for (int tx = num; tx <= right; tx++)
			{
				for (int ty = top; ty <= bottom; ty++)
				{
					if (Data[tx, ty])
					{
						Draw.HollowRect(base.AbsoluteLeft + (float)tx * CellWidth, base.AbsoluteTop + (float)ty * CellHeight, CellWidth, CellHeight, color);
					}
				}
			}
		}

		public override bool Collide(Vector2 point)
		{
			if (point.X >= base.AbsoluteLeft && point.Y >= base.AbsoluteTop && point.X < base.AbsoluteRight && point.Y < base.AbsoluteBottom)
			{
				return Data[(int)((point.X - base.AbsoluteLeft) / CellWidth), (int)((point.Y - base.AbsoluteTop) / CellHeight)];
			}
			return false;
		}

		public override bool Collide(Rectangle rect)
		{
			if (rect.Intersects(base.Bounds))
			{
				int x = (int)(((float)rect.Left - base.AbsoluteLeft) / CellWidth);
				int y = (int)(((float)rect.Top - base.AbsoluteTop) / CellHeight);
				int w = (int)(((float)rect.Right - base.AbsoluteLeft - 1f) / CellWidth) - x + 1;
				int h = (int)(((float)rect.Bottom - base.AbsoluteTop - 1f) / CellHeight) - y + 1;
				return CheckRect(x, y, w, h);
			}
			return false;
		}

		public override bool Collide(Vector2 from, Vector2 to)
		{
			from -= base.AbsolutePosition;
			to -= base.AbsolutePosition;
			from /= new Vector2(CellWidth, CellHeight);
			to /= new Vector2(CellWidth, CellHeight);
			bool steep = Math.Abs(to.Y - from.Y) > Math.Abs(to.X - from.X);
			if (steep)
			{
				float temp = from.X;
				from.X = from.Y;
				from.Y = temp;
				temp = to.X;
				to.X = to.Y;
				to.Y = temp;
			}
			if (from.X > to.X)
			{
				Vector2 vector = from;
				from = to;
				to = vector;
			}
			float error = 0f;
			float deltaError = Math.Abs(to.Y - from.Y) / (to.X - from.X);
			int yStep = ((from.Y < to.Y) ? 1 : (-1));
			int y = (int)from.Y;
			int toX = (int)to.X;
			for (int x = (int)from.X; x <= toX; x++)
			{
				if (steep)
				{
					if (this[y, x])
					{
						return true;
					}
				}
				else if (this[x, y])
				{
					return true;
				}
				error += deltaError;
				if (error >= 0.5f)
				{
					y += yStep;
					error -= 1f;
				}
			}
			return false;
		}

		public override bool Collide(Hitbox hitbox)
		{
			return Collide(hitbox.Bounds);
		}

		public override bool Collide(Grid grid)
		{
			throw new NotImplementedException();
		}

		public override bool Collide(Circle circle)
		{
			return false;
		}

		public override bool Collide(ColliderList list)
		{
			return list.Collide(this);
		}

		public static bool IsBitstringEmpty(string bitstring)
		{
			for (int i = 0; i < bitstring.Length; i++)
			{
				if (bitstring[i] == '1')
				{
					return false;
				}
			}
			return true;
		}
	}
}
