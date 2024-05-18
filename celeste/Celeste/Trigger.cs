using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(true)]
	public abstract class Trigger : Entity
	{
		public enum PositionModes
		{
			NoEffect,
			HorizontalCenter,
			VerticalCenter,
			TopToBottom,
			BottomToTop,
			LeftToRight,
			RightToLeft
		}

		public bool Triggered;

		public bool PlayerIsInside { get; private set; }

		public Trigger(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			base.Collider = new Hitbox(data.Width, data.Height);
			Visible = false;
		}

		public virtual void OnEnter(Player player)
		{
			PlayerIsInside = true;
		}

		public virtual void OnStay(Player player)
		{
		}

		public virtual void OnLeave(Player player)
		{
			PlayerIsInside = false;
		}

		protected float GetPositionLerp(Player player, PositionModes mode)
		{
			return mode switch
			{
				PositionModes.LeftToRight => Calc.ClampedMap(player.CenterX, base.Left, base.Right), 
				PositionModes.RightToLeft => Calc.ClampedMap(player.CenterX, base.Right, base.Left), 
				PositionModes.TopToBottom => Calc.ClampedMap(player.CenterY, base.Top, base.Bottom), 
				PositionModes.BottomToTop => Calc.ClampedMap(player.CenterY, base.Bottom, base.Top), 
				PositionModes.HorizontalCenter => Math.Min(Calc.ClampedMap(player.CenterX, base.Left, base.CenterX), Calc.ClampedMap(player.CenterX, base.Right, base.CenterX)), 
				PositionModes.VerticalCenter => Math.Min(Calc.ClampedMap(player.CenterY, base.Top, base.CenterY), Calc.ClampedMap(player.CenterY, base.Bottom, base.CenterY)), 
				_ => 1f, 
			};
		}
	}
}
