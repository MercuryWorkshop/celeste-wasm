using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Pathfinder
	{
		private struct Tile
		{
			public bool Solid;

			public int Cost;

			public Point? Parent;
		}

		private class PointMapComparer : IComparer<Point>
		{
			private Tile[,] map;

			public PointMapComparer(Tile[,] map)
			{
				this.map = map;
			}

			public int Compare(Point a, Point b)
			{
				return map[b.X, b.Y].Cost - map[a.X, a.Y].Cost;
			}
		}

		private static readonly Point[] directions = new Point[4]
		{
			new Point(1, 0),
			new Point(0, 1),
			new Point(-1, 0),
			new Point(0, -1)
		};

		private const int MapSize = 200;

		private Level level;

		private Tile[,] map;

		private List<Point> active = new List<Point>();

		private PointMapComparer comparer;

		public bool DebugRenderEnabled;

		private List<Vector2> lastPath;

		private Point debugLastStart;

		private Point debugLastEnd;

		public Pathfinder(Level level)
		{
			this.level = level;
			map = new Tile[200, 200];
			comparer = new PointMapComparer(map);
		}

		public bool Find(ref List<Vector2> path, Vector2 from, Vector2 to, bool fewerTurns = true, bool logging = false)
		{
			lastPath = null;
			int left = level.Bounds.Left / 8;
			int top = level.Bounds.Top / 8;
			int width = level.Bounds.Width / 8;
			int height = level.Bounds.Height / 8;
			Point solidsOffset = level.LevelSolidOffset;
			for (int tx2 = 0; tx2 < width; tx2++)
			{
				for (int ty = 0; ty < height; ty++)
				{
					map[tx2, ty].Solid = level.SolidsData[tx2 + solidsOffset.X, ty + solidsOffset.Y] != '0';
					map[tx2, ty].Cost = int.MaxValue;
					map[tx2, ty].Parent = null;
				}
			}
			foreach (Entity solid in level.Tracker.GetEntities<Solid>())
			{
				if (!solid.Collidable || !(solid.Collider is Hitbox))
				{
					continue;
				}
				int tx = (int)Math.Floor(solid.Left / 8f);
				for (int tr = (int)Math.Ceiling(solid.Right / 8f); tx < tr; tx++)
				{
					int ty2 = (int)Math.Floor(solid.Top / 8f);
					for (int tb = (int)Math.Ceiling(solid.Bottom / 8f); ty2 < tb; ty2++)
					{
						int x = tx - left;
						int y = ty2 - top;
						if (x >= 0 && y >= 0 && x < width && y < height)
						{
							map[x, y].Solid = true;
						}
					}
				}
			}
			Point start = (debugLastStart = new Point((int)Math.Floor(from.X / 8f) - left, (int)Math.Floor(from.Y / 8f) - top));
			Point end = (debugLastEnd = new Point((int)Math.Floor(to.X / 8f) - left, (int)Math.Floor(to.Y / 8f) - top));
			if (start.X < 0 || start.Y < 0 || start.X >= width || start.Y >= height || end.X < 0 || end.Y < 0 || end.X >= width || end.Y >= height)
			{
				if (logging)
				{
					Calc.Log("PF: FAILED - Start or End outside the level bounds");
				}
				return false;
			}
			if (map[start.X, start.Y].Solid)
			{
				if (logging)
				{
					Calc.Log("PF: FAILED - Start inside a solid");
				}
				return false;
			}
			if (map[end.X, end.Y].Solid)
			{
				if (logging)
				{
					Calc.Log("PF: FAILED - End inside a solid");
				}
				return false;
			}
			active.Clear();
			active.Add(start);
			map[start.X, start.Y].Cost = 0;
			bool found = false;
			while (active.Count > 0 && !found)
			{
				Point point = active[active.Count - 1];
				active.RemoveAt(active.Count - 1);
				for (int t = 0; t < 4; t++)
				{
					Point dir = new Point(directions[t].X, directions[t].Y);
					Point step = new Point(point.X + dir.X, point.Y + dir.Y);
					int stepCost = 1;
					if (step.X < 0 || step.Y < 0 || step.X >= width || step.Y >= height || map[step.X, step.Y].Solid)
					{
						continue;
					}
					for (int t2 = 0; t2 < 4; t2++)
					{
						Point check = new Point(step.X + directions[t2].X, step.Y + directions[t2].Y);
						if (check.X >= 0 && check.Y >= 0 && check.X < width && check.Y < height && map[check.X, check.Y].Solid)
						{
							stepCost = 7;
							break;
						}
					}
					if (fewerTurns && map[point.X, point.Y].Parent.HasValue && step.X != map[point.X, point.Y].Parent.Value.X && step.Y != map[point.X, point.Y].Parent.Value.Y)
					{
						stepCost += 4;
					}
					int existing = map[point.X, point.Y].Cost;
					if (dir.Y != 0)
					{
						stepCost += (int)((float)existing * 0.5f);
					}
					int cost = existing + stepCost;
					if (map[step.X, step.Y].Cost > cost)
					{
						map[step.X, step.Y].Cost = cost;
						map[step.X, step.Y].Parent = point;
						int index = active.BinarySearch(step, comparer);
						if (index < 0)
						{
							index = ~index;
						}
						active.Insert(index, step);
						if (step == end)
						{
							found = true;
							break;
						}
					}
				}
			}
			if (!found)
			{
				if (logging)
				{
					Calc.Log("PF: FAILED - ran out of active nodes, can't find ending");
				}
				return false;
			}
			path.Clear();
			Point current = end;
			int steps = 0;
			while (current != start && steps++ < 1000)
			{
				path.Add(new Vector2((float)current.X + 0.5f, (float)current.Y + 0.5f) * 8f + level.LevelOffset);
				current = map[current.X, current.Y].Parent.Value;
			}
			if (steps >= 1000)
			{
				Console.WriteLine("WARNING: Pathfinder 'succeeded' but then was unable to work out its path?");
				return false;
			}
			for (int i = 1; i < path.Count - 1; i++)
			{
				if (path.Count <= 2)
				{
					break;
				}
				if ((path[i].X == path[i - 1].X && path[i].X == path[i + 1].X) || (path[i].Y == path[i - 1].Y && path[i].Y == path[i + 1].Y))
				{
					path.RemoveAt(i);
					i--;
				}
			}
			path.Reverse();
			lastPath = path;
			if (logging)
			{
				Calc.Log("PF: SUCCESS");
			}
			return true;
		}

		public void Render()
		{
			for (int tx = 0; tx < 200; tx++)
			{
				for (int ty = 0; ty < 200; ty++)
				{
					if (map[tx, ty].Solid)
					{
						Draw.Rect(level.Bounds.Left + tx * 8, level.Bounds.Top + ty * 8, 8f, 8f, Color.Red * 0.25f);
					}
				}
			}
			if (lastPath != null)
			{
				Vector2 last = lastPath[0];
				for (int i = 1; i < lastPath.Count; i++)
				{
					Vector2 next = lastPath[i];
					Draw.Line(last, next, Color.Red);
					Draw.Rect(last.X - 2f, last.Y - 2f, 4f, 4f, Color.Red);
					last = next;
				}
				Draw.Rect(last.X - 2f, last.Y - 2f, 4f, 4f, Color.Red);
			}
			Draw.Rect(level.Bounds.Left + debugLastStart.X * 8 + 2, level.Bounds.Top + debugLastStart.Y * 8 + 2, 4f, 4f, Color.Green);
			Draw.Rect(level.Bounds.Left + debugLastEnd.X * 8 + 2, level.Bounds.Top + debugLastEnd.Y * 8 + 2, 4f, 4f, Color.Green);
		}
	}
}
