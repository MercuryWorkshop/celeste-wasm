using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class LightningStrike : Entity
	{
		private class Node
		{
			public Vector2 Position;

			public float Size;

			public List<Node> Children;

			public Node(float x, float y, float size)
				: this(new Vector2(x, y), size)
			{
			}

			public Node(Vector2 position, float size)
			{
				Position = position;
				Children = new List<Node>();
				Size = size;
			}

			public void Wiggle(Random rand)
			{
				Position.X += rand.Range(-2, 2);
				if (Position.Y != 0f)
				{
					Position.Y += rand.Range(-1, 1);
				}
				foreach (Node child in Children)
				{
					child.Wiggle(rand);
				}
			}

			public void Render(Vector2 offset, float scale)
			{
				float size = Size * scale;
				foreach (Node child in Children)
				{
					Vector2 normal = (child.Position - Position).SafeNormalize();
					Draw.Line(offset + Position, offset + child.Position + normal * size * 0.5f, Color.White, size);
					child.Render(offset, scale);
				}
			}
		}

		private bool on;

		private float scale;

		private Random rand;

		private float strikeHeight;

		private Node strike;

		public LightningStrike(Vector2 position, int seed, float height, float delay = 0f)
		{
			Position = position;
			base.Depth = 10010;
			rand = new Random(seed);
			strikeHeight = height;
			Add(new Coroutine(Routine(delay)));
		}

		private IEnumerator Routine(float delay)
		{
			if (delay > 0f)
			{
				yield return delay;
			}
			scale = 1f;
			GenerateStikeNodes(-1, 10f);
			for (int i = 0; i < 5; i++)
			{
				on = true;
				yield return (1f - (float)i / 5f) * 0.1f;
				scale -= 0.2f;
				on = false;
				strike.Wiggle(rand);
				yield return 0.01f;
			}
			RemoveSelf();
		}

		private void GenerateStikeNodes(int direction, float size, Node parent = null)
		{
			if (parent == null)
			{
				parent = (strike = new Node(0f, 0f, size));
			}
			if (!(parent.Position.Y >= strikeHeight))
			{
				float offsetX = direction * rand.Range(-8, 20);
				float offsetY = rand.Range(8, 16);
				float stroke = (0.25f + (1f - (parent.Position.Y + offsetY) / strikeHeight) * 0.75f) * size;
				Node next = new Node(parent.Position + new Vector2(offsetX, offsetY), stroke);
				parent.Children.Add(next);
				GenerateStikeNodes(direction, size, next);
				if (rand.Chance(0.1f))
				{
					Node offshoot = new Node(parent.Position + new Vector2(0f - offsetX, offsetY * 1.5f), stroke);
					parent.Children.Add(offshoot);
					GenerateStikeNodes(-direction, size, offshoot);
				}
			}
		}

		public override void Render()
		{
			if (on)
			{
				strike.Render(Position, scale);
			}
		}
	}
}
