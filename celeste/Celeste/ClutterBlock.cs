using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class ClutterBlock : Entity
	{
		public enum Colors
		{
			Red,
			Green,
			Yellow,
			Lightning
		}

		public Colors BlockColor;

		public Image Image;

		public HashSet<ClutterBlock> HasBelow = new HashSet<ClutterBlock>();

		public List<ClutterBlock> Below = new List<ClutterBlock>();

		public List<ClutterBlock> Above = new List<ClutterBlock>();

		public bool OnTheGround;

		public bool TopSideOpen;

		public bool LeftSideOpen;

		public bool RightSideOpen;

		private float floatTarget;

		private float floatDelay;

		private float floatTimer;

		private float WaveTarget => 0f - ((float)Math.Sin((float)((int)Position.X / 16) * 0.25f + floatTimer * 2f) + 1f) / 2f - 1f;

		public ClutterBlock(Vector2 position, MTexture texture, Colors color)
			: base(position)
		{
			BlockColor = color;
			Add(Image = new Image(texture));
			base.Collider = new Hitbox(texture.Width, texture.Height);
			base.Depth = -9998;
		}

		public void WeightDown()
		{
			foreach (ClutterBlock item in Below)
			{
				item.WeightDown();
			}
			floatTarget = 0f;
			floatDelay = 0.1f;
		}

		public override void Update()
		{
			base.Update();
			if (OnTheGround)
			{
				return;
			}
			if (floatDelay <= 0f)
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null && ((TopSideOpen && player.Right > base.Left && player.Left < base.Right && player.Bottom >= base.Top - 1f && player.Bottom <= base.Top + 4f) | (player.StateMachine.State == 1 && LeftSideOpen && player.Right >= base.Left - 1f && player.Right < base.Left + 4f && player.Bottom > base.Top && player.Top < base.Bottom) | (player.StateMachine.State == 1 && RightSideOpen && player.Left <= base.Right + 1f && player.Left > base.Right - 4f && player.Bottom > base.Top && player.Top < base.Bottom)))
				{
					WeightDown();
				}
			}
			floatTimer += Engine.DeltaTime;
			floatDelay -= Engine.DeltaTime;
			if (floatDelay <= 0f)
			{
				floatTarget = Calc.Approach(floatTarget, WaveTarget, Engine.DeltaTime * 4f);
			}
			Image.Y = floatTarget;
		}

		public void Absorb(ClutterAbsorbEffect effect)
		{
			effect.FlyClutter(Position + new Vector2(Image.Width * 0.5f, Image.Height * 0.5f + floatTarget), Image.Texture, shake: true, Calc.Random.NextFloat(0.5f));
			base.Scene.Remove(this);
		}
	}
}
