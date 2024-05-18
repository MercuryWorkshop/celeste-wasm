using System.Xml;

namespace Monocle
{
	public class AutotileData
	{
		public int[] Center;

		public int[] Single;

		public int[] SingleHorizontalLeft;

		public int[] SingleHorizontalCenter;

		public int[] SingleHorizontalRight;

		public int[] SingleVerticalTop;

		public int[] SingleVerticalCenter;

		public int[] SingleVerticalBottom;

		public int[] Top;

		public int[] Bottom;

		public int[] Left;

		public int[] Right;

		public int[] TopLeft;

		public int[] TopRight;

		public int[] BottomLeft;

		public int[] BottomRight;

		public int[] InsideTopLeft;

		public int[] InsideTopRight;

		public int[] InsideBottomLeft;

		public int[] InsideBottomRight;

		public AutotileData(XmlElement xml)
		{
			Center = Calc.ReadCSVInt(xml.ChildText("Center", ""));
			Single = Calc.ReadCSVInt(xml.ChildText("Single", ""));
			SingleHorizontalLeft = Calc.ReadCSVInt(xml.ChildText("SingleHorizontalLeft", ""));
			SingleHorizontalCenter = Calc.ReadCSVInt(xml.ChildText("SingleHorizontalCenter", ""));
			SingleHorizontalRight = Calc.ReadCSVInt(xml.ChildText("SingleHorizontalRight", ""));
			SingleVerticalTop = Calc.ReadCSVInt(xml.ChildText("SingleVerticalTop", ""));
			SingleVerticalCenter = Calc.ReadCSVInt(xml.ChildText("SingleVerticalCenter", ""));
			SingleVerticalBottom = Calc.ReadCSVInt(xml.ChildText("SingleVerticalBottom", ""));
			Top = Calc.ReadCSVInt(xml.ChildText("Top", ""));
			Bottom = Calc.ReadCSVInt(xml.ChildText("Bottom", ""));
			Left = Calc.ReadCSVInt(xml.ChildText("Left", ""));
			Right = Calc.ReadCSVInt(xml.ChildText("Right", ""));
			TopLeft = Calc.ReadCSVInt(xml.ChildText("TopLeft", ""));
			TopRight = Calc.ReadCSVInt(xml.ChildText("TopRight", ""));
			BottomLeft = Calc.ReadCSVInt(xml.ChildText("BottomLeft", ""));
			BottomRight = Calc.ReadCSVInt(xml.ChildText("BottomRight", ""));
			InsideTopLeft = Calc.ReadCSVInt(xml.ChildText("InsideTopLeft", ""));
			InsideTopRight = Calc.ReadCSVInt(xml.ChildText("InsideTopRight", ""));
			InsideBottomLeft = Calc.ReadCSVInt(xml.ChildText("InsideBottomLeft", ""));
			InsideBottomRight = Calc.ReadCSVInt(xml.ChildText("InsideBottomRight", ""));
		}

		public int TileHandler()
		{
			if (Tiler.Left && Tiler.Right && Tiler.Up && Tiler.Down && Tiler.UpLeft && Tiler.UpRight && Tiler.DownLeft && Tiler.DownRight)
			{
				return GetTileID(Center);
			}
			if (!Tiler.Up && !Tiler.Down)
			{
				if (Tiler.Left && Tiler.Right)
				{
					return GetTileID(SingleHorizontalCenter);
				}
				if (!Tiler.Left && !Tiler.Right)
				{
					return GetTileID(Single);
				}
				if (Tiler.Left)
				{
					return GetTileID(SingleHorizontalRight);
				}
				return GetTileID(SingleHorizontalLeft);
			}
			if (!Tiler.Left && !Tiler.Right)
			{
				if (Tiler.Up && Tiler.Down)
				{
					return GetTileID(SingleVerticalCenter);
				}
				if (Tiler.Down)
				{
					return GetTileID(SingleVerticalTop);
				}
				return GetTileID(SingleVerticalBottom);
			}
			if (Tiler.Up && Tiler.Down && Tiler.Left && !Tiler.Right)
			{
				return GetTileID(Right);
			}
			if (Tiler.Up && Tiler.Down && !Tiler.Left && Tiler.Right)
			{
				return GetTileID(Left);
			}
			if (Tiler.Up && !Tiler.Left && Tiler.Right && !Tiler.Down)
			{
				return GetTileID(BottomLeft);
			}
			if (Tiler.Up && Tiler.Left && !Tiler.Right && !Tiler.Down)
			{
				return GetTileID(BottomRight);
			}
			if (Tiler.Down && Tiler.Right && !Tiler.Left && !Tiler.Up)
			{
				return GetTileID(TopLeft);
			}
			if (Tiler.Down && !Tiler.Right && Tiler.Left && !Tiler.Up)
			{
				return GetTileID(TopRight);
			}
			if (Tiler.Up && Tiler.Down && !Tiler.DownRight && Tiler.DownLeft)
			{
				return GetTileID(InsideTopLeft);
			}
			if (Tiler.Up && Tiler.Down && Tiler.DownRight && !Tiler.DownLeft)
			{
				return GetTileID(InsideTopRight);
			}
			if (Tiler.Up && Tiler.Down && Tiler.UpLeft && !Tiler.UpRight)
			{
				return GetTileID(InsideBottomLeft);
			}
			if (Tiler.Up && Tiler.Down && !Tiler.UpLeft && Tiler.UpRight)
			{
				return GetTileID(InsideBottomRight);
			}
			if (!Tiler.Down)
			{
				return GetTileID(Bottom);
			}
			return GetTileID(Top);
		}

		private int GetTileID(int[] choices)
		{
			if (choices.Length == 0)
			{
				return -1;
			}
			if (choices.Length == 1)
			{
				return choices[0];
			}
			return Calc.Random.Choose(choices);
		}
	}
}
