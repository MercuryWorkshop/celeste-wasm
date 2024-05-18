using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class LedgeBlocker : Component
	{
		public bool Blocking = true;

		public Func<Player, bool> BlockChecker;

		public LedgeBlocker(Func<Player, bool> blockChecker = null)
			: base(active: false, visible: false)
		{
			BlockChecker = blockChecker;
		}

		public bool HopBlockCheck(Player player)
		{
			if (Blocking && player.CollideCheck(base.Entity, player.Position + Vector2.UnitX * (float)player.Facing * 8f))
			{
				if (BlockChecker != null)
				{
					return BlockChecker(player);
				}
				return true;
			}
			return false;
		}

		public bool JumpThruBoostCheck(Player player)
		{
			if (Blocking && player.CollideCheck(base.Entity, player.Position - Vector2.UnitY * 2f))
			{
				if (BlockChecker != null)
				{
					return BlockChecker(player);
				}
				return true;
			}
			return false;
		}

		public bool DashCorrectCheck(Player player)
		{
			if (Blocking && player.CollideCheck(base.Entity, player.Position))
			{
				if (BlockChecker != null)
				{
					return BlockChecker(player);
				}
				return true;
			}
			return false;
		}
	}
}
