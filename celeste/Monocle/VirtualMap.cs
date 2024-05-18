namespace Monocle
{
	public class VirtualMap<T>
	{
		public const int SegmentSize = 50;

		public readonly int Columns;

		public readonly int Rows;

		public readonly int SegmentColumns;

		public readonly int SegmentRows;

		public readonly T EmptyValue;

		private T[,][,] segments;

		public T this[int x, int y]
		{
			get
			{
				int cx = x / 50;
				int cy = y / 50;
				T[,] seg = segments[cx, cy];
				if (seg == null)
				{
					return EmptyValue;
				}
				return seg[x - cx * 50, y - cy * 50];
			}
			set
			{
				int cx = x / 50;
				int cy = y / 50;
				if (segments[cx, cy] == null)
				{
					segments[cx, cy] = new T[50, 50];
					if (EmptyValue != null)
					{
						T emptyValue = EmptyValue;
						if (!emptyValue.Equals(default(T)))
						{
							for (int tx = 0; tx < 50; tx++)
							{
								for (int ty = 0; ty < 50; ty++)
								{
									segments[cx, cy][tx, ty] = EmptyValue;
								}
							}
						}
					}
				}
				segments[cx, cy][x - cx * 50, y - cy * 50] = value;
			}
		}

		public VirtualMap(int columns, int rows, T emptyValue = default(T))
		{
			Columns = columns;
			Rows = rows;
			SegmentColumns = columns / 50 + 1;
			SegmentRows = rows / 50 + 1;
			segments = new T[SegmentColumns, SegmentRows][,];
			EmptyValue = emptyValue;
		}

		public VirtualMap(T[,] map, T emptyValue = default(T))
			: this(map.GetLength(0), map.GetLength(1), emptyValue)
		{
			for (int x = 0; x < Columns; x++)
			{
				for (int y = 0; y < Rows; y++)
				{
					this[x, y] = map[x, y];
				}
			}
		}

		public bool AnyInSegmentAtTile(int x, int y)
		{
			int cx = x / 50;
			int cy = y / 50;
			return segments[cx, cy] != null;
		}

		public bool AnyInSegment(int segmentX, int segmentY)
		{
			return segments[segmentX, segmentY] != null;
		}

		public T InSegment(int segmentX, int segmentY, int x, int y)
		{
			return segments[segmentX, segmentY][x, y];
		}

		public T[,] GetSegment(int segmentX, int segmentY)
		{
			return segments[segmentX, segmentY];
		}

		public T SafeCheck(int x, int y)
		{
			if (x >= 0 && y >= 0 && x < Columns && y < Rows)
			{
				return this[x, y];
			}
			return EmptyValue;
		}

		public T[,] ToArray()
		{
			T[,] array = new T[Columns, Rows];
			for (int x = 0; x < Columns; x++)
			{
				for (int y = 0; y < Rows; y++)
				{
					array[x, y] = this[x, y];
				}
			}
			return array;
		}

		public VirtualMap<T> Clone()
		{
			VirtualMap<T> clone = new VirtualMap<T>(Columns, Rows, EmptyValue);
			for (int x = 0; x < Columns; x++)
			{
				for (int y = 0; y < Rows; y++)
				{
					clone[x, y] = this[x, y];
				}
			}
			return clone;
		}
	}
}
