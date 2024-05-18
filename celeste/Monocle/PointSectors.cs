using System;

namespace Monocle
{
	[Flags]
	public enum PointSectors
	{
		Center = 0,
		Top = 1,
		Bottom = 2,
		TopLeft = 9,
		TopRight = 5,
		Left = 8,
		Right = 4,
		BottomLeft = 0xA,
		BottomRight = 6
	}
}
