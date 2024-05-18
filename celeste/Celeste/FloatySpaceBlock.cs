using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class FloatySpaceBlock : Solid
	{
		private TileGrid tiles;

		private char tileType;

		private float yLerp;

		private float sinkTimer;

		private float sineWave;

		private float dashEase;

		private Vector2 dashDirection;

		private FloatySpaceBlock master;

		private bool awake;

		public List<FloatySpaceBlock> Group;

		public List<JumpThru> Jumpthrus;

		public Dictionary<Platform, Vector2> Moves;

		public Point GroupBoundsMin;

		public Point GroupBoundsMax;

		public bool HasGroup { get; private set; }

		public bool MasterOfGroup { get; private set; }

		public FloatySpaceBlock(Vector2 position, float width, float height, char tileType, bool disableSpawnOffset)
			: base(position, width, height, safe: true)
		{
			this.tileType = tileType;
			base.Depth = -9000;
			Add(new LightOcclude());
			SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
			if (!disableSpawnOffset)
			{
				sineWave = Calc.Random.NextFloat((float)Math.PI * 2f);
			}
			else
			{
				sineWave = 0f;
			}
		}

		public FloatySpaceBlock(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.Char("tiletype", '3'), data.Bool("disableSpawnOffset"))
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			awake = true;
			if (!HasGroup)
			{
				MasterOfGroup = true;
				Moves = new Dictionary<Platform, Vector2>();
				Group = new List<FloatySpaceBlock>();
				Jumpthrus = new List<JumpThru>();
				GroupBoundsMin = new Point((int)base.X, (int)base.Y);
				GroupBoundsMax = new Point((int)base.Right, (int)base.Bottom);
				AddToGroupAndFindChildren(this);
				_ = base.Scene;
				Rectangle bounds = new Rectangle(GroupBoundsMin.X / 8, GroupBoundsMin.Y / 8, (GroupBoundsMax.X - GroupBoundsMin.X) / 8 + 1, (GroupBoundsMax.Y - GroupBoundsMin.Y) / 8 + 1);
				VirtualMap<char> terrain = new VirtualMap<char>(bounds.Width, bounds.Height, '0');
				foreach (FloatySpaceBlock item in Group)
				{
					int tx = (int)(item.X / 8f) - bounds.X;
					int ty = (int)(item.Y / 8f) - bounds.Y;
					int tw = (int)(item.Width / 8f);
					int th = (int)(item.Height / 8f);
					for (int x = tx; x < tx + tw; x++)
					{
						for (int y = ty; y < ty + th; y++)
						{
							terrain[x, y] = tileType;
						}
					}
				}
				tiles = GFX.FGAutotiler.GenerateMap(terrain, new Autotiler.Behaviour
				{
					EdgesExtend = false,
					EdgesIgnoreOutOfLevel = false,
					PaddingIgnoreOutOfLevel = false
				}).TileGrid;
				tiles.Position = new Vector2((float)GroupBoundsMin.X - base.X, (float)GroupBoundsMin.Y - base.Y);
				Add(tiles);
			}
			TryToInitPosition();
		}

		public override void OnStaticMoverTrigger(StaticMover sm)
		{
			if (sm.Entity is Spring)
			{
				switch ((sm.Entity as Spring).Orientation)
				{
				case Spring.Orientations.Floor:
					sinkTimer = 0.5f;
					break;
				case Spring.Orientations.WallLeft:
					dashEase = 1f;
					dashDirection = -Vector2.UnitX;
					break;
				case Spring.Orientations.WallRight:
					dashEase = 1f;
					dashDirection = Vector2.UnitX;
					break;
				}
			}
		}

		private void TryToInitPosition()
		{
			if (MasterOfGroup)
			{
				foreach (FloatySpaceBlock item in Group)
				{
					if (!item.awake)
					{
						return;
					}
				}
				MoveToTarget();
			}
			else
			{
				master.TryToInitPosition();
			}
		}

		private void AddToGroupAndFindChildren(FloatySpaceBlock from)
		{
			if (from.X < (float)GroupBoundsMin.X)
			{
				GroupBoundsMin.X = (int)from.X;
			}
			if (from.Y < (float)GroupBoundsMin.Y)
			{
				GroupBoundsMin.Y = (int)from.Y;
			}
			if (from.Right > (float)GroupBoundsMax.X)
			{
				GroupBoundsMax.X = (int)from.Right;
			}
			if (from.Bottom > (float)GroupBoundsMax.Y)
			{
				GroupBoundsMax.Y = (int)from.Bottom;
			}
			from.HasGroup = true;
			from.OnDashCollide = OnDash;
			Group.Add(from);
			Moves.Add(from, from.Position);
			if (from != this)
			{
				from.master = this;
			}
			foreach (JumpThru jumpthru2 in base.Scene.CollideAll<JumpThru>(new Rectangle((int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height)))
			{
				if (!Jumpthrus.Contains(jumpthru2))
				{
					AddJumpThru(jumpthru2);
				}
			}
			foreach (JumpThru jumpthru in base.Scene.CollideAll<JumpThru>(new Rectangle((int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2)))
			{
				if (!Jumpthrus.Contains(jumpthru))
				{
					AddJumpThru(jumpthru);
				}
			}
			foreach (FloatySpaceBlock block in base.Scene.Tracker.GetEntities<FloatySpaceBlock>())
			{
				if (!block.HasGroup && block.tileType == tileType && (base.Scene.CollideCheck(new Rectangle((int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height), block) || base.Scene.CollideCheck(new Rectangle((int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2), block)))
				{
					AddToGroupAndFindChildren(block);
				}
			}
		}

		private void AddJumpThru(JumpThru jp)
		{
			jp.OnDashCollide = OnDash;
			Jumpthrus.Add(jp);
			Moves.Add(jp, jp.Position);
			foreach (FloatySpaceBlock block in base.Scene.Tracker.GetEntities<FloatySpaceBlock>())
			{
				if (!block.HasGroup && block.tileType == tileType && base.Scene.CollideCheck(new Rectangle((int)jp.X - 1, (int)jp.Y, (int)jp.Width + 2, (int)jp.Height), block))
				{
					AddToGroupAndFindChildren(block);
				}
			}
		}

		private DashCollisionResults OnDash(Player player, Vector2 direction)
		{
			if (MasterOfGroup && dashEase <= 0.2f)
			{
				dashEase = 1f;
				dashDirection = direction;
			}
			return DashCollisionResults.NormalOverride;
		}

		public override void Update()
		{
			base.Update();
			if (MasterOfGroup)
			{
				bool hasPlayer = false;
				foreach (FloatySpaceBlock item in Group)
				{
					if (item.HasPlayerRider())
					{
						hasPlayer = true;
						break;
					}
				}
				if (!hasPlayer)
				{
					foreach (JumpThru jumpthru in Jumpthrus)
					{
						if (jumpthru.HasPlayerRider())
						{
							hasPlayer = true;
							break;
						}
					}
				}
				if (hasPlayer)
				{
					sinkTimer = 0.3f;
				}
				else if (sinkTimer > 0f)
				{
					sinkTimer -= Engine.DeltaTime;
				}
				if (sinkTimer > 0f)
				{
					yLerp = Calc.Approach(yLerp, 1f, 1f * Engine.DeltaTime);
				}
				else
				{
					yLerp = Calc.Approach(yLerp, 0f, 1f * Engine.DeltaTime);
				}
				sineWave += Engine.DeltaTime;
				dashEase = Calc.Approach(dashEase, 0f, Engine.DeltaTime * 1.5f);
				MoveToTarget();
			}
			LiftSpeed = Vector2.Zero;
		}

		private void MoveToTarget()
		{
			float sineOffset = (float)Math.Sin(sineWave) * 4f;
			Vector2 dashOffset = Calc.YoYo(Ease.QuadIn(dashEase)) * dashDirection * 8f;
			for (int pass = 0; pass < 2; pass++)
			{
				foreach (KeyValuePair<Platform, Vector2> kv in Moves)
				{
					Platform platform = kv.Key;
					bool hasRider = false;
					JumpThru jumpthru = platform as JumpThru;
					Solid solid = platform as Solid;
					if ((jumpthru != null && jumpthru.HasRider()) || (solid != null && solid.HasRider()))
					{
						hasRider = true;
					}
					if ((hasRider || pass != 0) && (!hasRider || pass != 1))
					{
						Vector2 start = kv.Value;
						float targetY = MathHelper.Lerp(start.Y, start.Y + 12f, Ease.SineInOut(yLerp)) + sineOffset;
						platform.MoveToY(targetY + dashOffset.Y);
						platform.MoveToX(start.X + dashOffset.X);
					}
				}
			}
		}

		public override void OnShake(Vector2 amount)
		{
			if (!MasterOfGroup)
			{
				return;
			}
			base.OnShake(amount);
			tiles.Position += amount;
			foreach (JumpThru jumpthru in Jumpthrus)
			{
				foreach (Component component in jumpthru.Components)
				{
					if (component is Image img)
					{
						img.Position += amount;
					}
				}
			}
		}
	}
}
