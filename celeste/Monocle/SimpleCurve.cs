using Microsoft.Xna.Framework;

namespace Monocle
{
	public struct SimpleCurve
	{
		public Vector2 Begin;

		public Vector2 End;

		public Vector2 Control;

		public SimpleCurve(Vector2 begin, Vector2 end, Vector2 control)
		{
			Begin = begin;
			End = end;
			Control = control;
		}

		public void DoubleControl()
		{
			Vector2 axis = End - Begin;
			Vector2 mid = Begin + axis / 2f;
			Vector2 diff = Control - mid;
			Control += diff;
		}

		public Vector2 GetPoint(float percent)
		{
			float reverse = 1f - percent;
			return reverse * reverse * Begin + 2f * reverse * percent * Control + percent * percent * End;
		}

		public float GetLengthParametric(int resolution)
		{
			Vector2 last = Begin;
			float length = 0f;
			for (int i = 1; i <= resolution; i++)
			{
				Vector2 at = GetPoint((float)i / (float)resolution);
				length += (at - last).Length();
				last = at;
			}
			return length;
		}

		public void Render(Vector2 offset, Color color, int resolution)
		{
			Vector2 last = offset + Begin;
			for (int i = 1; i <= resolution; i++)
			{
				Vector2 at = offset + GetPoint((float)i / (float)resolution);
				Draw.Line(last, at, color);
				last = at;
			}
		}

		public void Render(Vector2 offset, Color color, int resolution, float thickness)
		{
			Vector2 last = offset + Begin;
			for (int i = 1; i <= resolution; i++)
			{
				Vector2 at = offset + GetPoint((float)i / (float)resolution);
				Draw.Line(last, at, color, thickness);
				last = at;
			}
		}

		public void Render(Color color, int resolution)
		{
			Render(Vector2.Zero, color, resolution);
		}

		public void Render(Color color, int resolution, float thickness)
		{
			Render(Vector2.Zero, color, resolution, thickness);
		}
	}
}
