using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class CassetteBlock : Solid
	{
		public enum Modes
		{
			Solid,
			Leaving,
			Disabled,
			Returning
		}

		private class BoxSide : Entity
		{
			private CassetteBlock block;

			private Color color;

			public BoxSide(CassetteBlock block, Color color)
			{
				this.block = block;
				this.color = color;
			}

			public override void Render()
			{
				Draw.Rect(block.X, block.Y + block.Height - 8f, block.Width, 8 + block.blockHeight, color);
			}
		}

		public int Index;

		public float Tempo;

		public bool Activated;

		public Modes Mode;

		public EntityID ID;

		private int blockHeight = 2;

		private List<CassetteBlock> group;

		private bool groupLeader;

		private Vector2 groupOrigin;

		private Color color;

		private List<Image> pressed = new List<Image>();

		private List<Image> solid = new List<Image>();

		private List<Image> all = new List<Image>();

		private LightOcclude occluder;

		private Wiggler wiggler;

		private Vector2 wigglerScaler;

		private BoxSide side;

		public CassetteBlock(Vector2 position, EntityID id, float width, float height, int index, float tempo)
			: base(position, width, height, safe: false)
		{
			SurfaceSoundIndex = 35;
			Index = index;
			Tempo = tempo;
			Collidable = false;
			ID = id;
			switch (Index)
			{
			default:
				color = Calc.HexToColor("49aaf0");
				break;
			case 1:
				color = Calc.HexToColor("f049be");
				break;
			case 2:
				color = Calc.HexToColor("fcdc3a");
				break;
			case 3:
				color = Calc.HexToColor("38e04e");
				break;
			}
			Add(occluder = new LightOcclude());
		}

		public CassetteBlock(EntityData data, Vector2 offset, EntityID id)
			: this(data.Position + offset, id, data.Width, data.Height, data.Int("index"), data.Float("tempo", 1f))
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			Color disabledColor = Calc.HexToColor("667da5");
			Color disabledColorMult = new Color((float)(int)disabledColor.R / 255f * ((float)(int)color.R / 255f), (float)(int)disabledColor.G / 255f * ((float)(int)color.G / 255f), (float)(int)disabledColor.B / 255f * ((float)(int)color.B / 255f), 1f);
			scene.Add(side = new BoxSide(this, disabledColorMult));
			foreach (StaticMover staticMover in staticMovers)
			{
				if (staticMover.Entity is Spikes spike2)
				{
					spike2.EnabledColor = color;
					spike2.DisabledColor = disabledColorMult;
					spike2.VisibleWhenDisabled = true;
					spike2.SetSpikeColor(color);
				}
				if (staticMover.Entity is Spring spring)
				{
					spring.DisabledColor = disabledColorMult;
					spring.VisibleWhenDisabled = true;
				}
			}
			if (group == null)
			{
				groupLeader = true;
				group = new List<CassetteBlock>();
				group.Add(this);
				FindInGroup(this);
				float left2 = float.MaxValue;
				float right2 = float.MinValue;
				float top = float.MaxValue;
				float bottom = float.MinValue;
				foreach (CassetteBlock block in group)
				{
					if (block.Left < left2)
					{
						left2 = block.Left;
					}
					if (block.Right > right2)
					{
						right2 = block.Right;
					}
					if (block.Bottom > bottom)
					{
						bottom = block.Bottom;
					}
					if (block.Top < top)
					{
						top = block.Top;
					}
				}
				groupOrigin = new Vector2((int)(left2 + (right2 - left2) / 2f), (int)bottom);
				wigglerScaler = new Vector2(Calc.ClampedMap(right2 - left2, 32f, 96f, 1f, 0.2f), Calc.ClampedMap(bottom - top, 32f, 96f, 1f, 0.2f));
				Add(wiggler = Wiggler.Create(0.3f, 3f));
				foreach (CassetteBlock item in group)
				{
					item.wiggler = wiggler;
					item.wigglerScaler = wigglerScaler;
					item.groupOrigin = groupOrigin;
				}
			}
			foreach (StaticMover staticMover2 in staticMovers)
			{
				if (staticMover2.Entity is Spikes spike)
				{
					spike.SetOrigins(groupOrigin);
				}
			}
			for (float x = base.Left; x < base.Right; x += 8f)
			{
				for (float y = base.Top; y < base.Bottom; y += 8f)
				{
					bool left = CheckForSame(x - 8f, y);
					bool right = CheckForSame(x + 8f, y);
					bool up = CheckForSame(x, y - 8f);
					bool down = CheckForSame(x, y + 8f);
					if (left && right && up && down)
					{
						if (!CheckForSame(x + 8f, y - 8f))
						{
							SetImage(x, y, 3, 0);
						}
						else if (!CheckForSame(x - 8f, y - 8f))
						{
							SetImage(x, y, 3, 1);
						}
						else if (!CheckForSame(x + 8f, y + 8f))
						{
							SetImage(x, y, 3, 2);
						}
						else if (!CheckForSame(x - 8f, y + 8f))
						{
							SetImage(x, y, 3, 3);
						}
						else
						{
							SetImage(x, y, 1, 1);
						}
					}
					else if (left && right && !up && down)
					{
						SetImage(x, y, 1, 0);
					}
					else if (left && right && up && !down)
					{
						SetImage(x, y, 1, 2);
					}
					else if (left && !right && up && down)
					{
						SetImage(x, y, 2, 1);
					}
					else if (!left && right && up && down)
					{
						SetImage(x, y, 0, 1);
					}
					else if (left && !right && !up && down)
					{
						SetImage(x, y, 2, 0);
					}
					else if (!left && right && !up && down)
					{
						SetImage(x, y, 0, 0);
					}
					else if (left && !right && up && !down)
					{
						SetImage(x, y, 2, 2);
					}
					else if (!left && right && up && !down)
					{
						SetImage(x, y, 0, 2);
					}
				}
			}
			UpdateVisualState();
		}

		private void FindInGroup(CassetteBlock block)
		{
			foreach (CassetteBlock other in base.Scene.Tracker.GetEntities<CassetteBlock>())
			{
				if (other != this && other != block && other.Index == Index && (other.CollideRect(new Rectangle((int)block.X - 1, (int)block.Y, (int)block.Width + 2, (int)block.Height)) || other.CollideRect(new Rectangle((int)block.X, (int)block.Y - 1, (int)block.Width, (int)block.Height + 2))) && !group.Contains(other))
				{
					group.Add(other);
					FindInGroup(other);
					other.group = group;
				}
			}
		}

		private bool CheckForSame(float x, float y)
		{
			foreach (CassetteBlock hit in base.Scene.Tracker.GetEntities<CassetteBlock>())
			{
				if (hit.Index == Index && hit.Collider.Collide(new Rectangle((int)x, (int)y, 8, 8)))
				{
					return true;
				}
			}
			return false;
		}

		private void SetImage(float x, float y, int tx, int ty)
		{
			List<MTexture> textures = GFX.Game.GetAtlasSubtextures("objects/cassetteblock/pressed");
			pressed.Add(CreateImage(x, y, tx, ty, textures[Index % textures.Count]));
			solid.Add(CreateImage(x, y, tx, ty, GFX.Game["objects/cassetteblock/solid"]));
		}

		private Image CreateImage(float x, float y, int tx, int ty, MTexture tex)
		{
			Vector2 off = new Vector2(x - base.X, y - base.Y);
			Image img = new Image(tex.GetSubtexture(tx * 8, ty * 8, 8, 8));
			Vector2 center = groupOrigin - Position;
			img.Origin = center - off;
			img.Position = center;
			img.Color = color;
			Add(img);
			all.Add(img);
			return img;
		}

		public override void Update()
		{
			base.Update();
			if (groupLeader && Activated && !Collidable)
			{
				bool blocked = false;
				foreach (CassetteBlock item in group)
				{
					if (item.BlockedCheck())
					{
						blocked = true;
						break;
					}
				}
				if (!blocked)
				{
					foreach (CassetteBlock item2 in group)
					{
						item2.Collidable = true;
						item2.EnableStaticMovers();
						item2.ShiftSize(-1);
					}
					wiggler.Start();
				}
			}
			else if (!Activated && Collidable)
			{
				ShiftSize(1);
				Collidable = false;
				DisableStaticMovers();
			}
			UpdateVisualState();
		}

		public bool BlockedCheck()
		{
			TheoCrystal theo = CollideFirst<TheoCrystal>();
			if (theo != null && !TryActorWiggleUp(theo))
			{
				return true;
			}
			Player player = CollideFirst<Player>();
			if (player != null && !TryActorWiggleUp(player))
			{
				return true;
			}
			return false;
		}

		private void UpdateVisualState()
		{
			if (!Collidable)
			{
				base.Depth = 8990;
			}
			else
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null && player.Top >= base.Bottom - 1f)
				{
					base.Depth = 10;
				}
				else
				{
					base.Depth = -10;
				}
			}
			foreach (StaticMover staticMover in staticMovers)
			{
				staticMover.Entity.Depth = base.Depth + 1;
			}
			side.Depth = base.Depth + 5;
			side.Visible = blockHeight > 0;
			occluder.Visible = Collidable;
			foreach (Image item in solid)
			{
				item.Visible = Collidable;
			}
			foreach (Image item2 in pressed)
			{
				item2.Visible = !Collidable;
			}
			if (!groupLeader)
			{
				return;
			}
			Vector2 scale = new Vector2(1f + wiggler.Value * 0.05f * wigglerScaler.X, 1f + wiggler.Value * 0.15f * wigglerScaler.Y);
			foreach (CassetteBlock block in group)
			{
				foreach (Image item3 in block.all)
				{
					item3.Scale = scale;
				}
				foreach (StaticMover staticMover2 in block.staticMovers)
				{
					if (!(staticMover2.Entity is Spikes spike))
					{
						continue;
					}
					foreach (Component component in spike.Components)
					{
						if (component is Image img)
						{
							img.Scale = scale;
						}
					}
				}
			}
		}

		public void SetActivatedSilently(bool activated)
		{
			Activated = (Collidable = activated);
			UpdateVisualState();
			if (activated)
			{
				EnableStaticMovers();
				return;
			}
			ShiftSize(2);
			DisableStaticMovers();
		}

		public void Finish()
		{
			Activated = false;
		}

		public void WillToggle()
		{
			ShiftSize(Collidable ? 1 : (-1));
			UpdateVisualState();
		}

		private void ShiftSize(int amount)
		{
			MoveV(amount);
			blockHeight -= amount;
		}

		private bool TryActorWiggleUp(Entity actor)
		{
			foreach (CassetteBlock other in group)
			{
				if (other != this && other.CollideCheck(actor, other.Position + Vector2.UnitY * 4f))
				{
					return false;
				}
			}
			bool was = Collidable;
			Collidable = true;
			for (int i = 1; i <= 4; i++)
			{
				if (!actor.CollideCheck<Solid>(actor.Position - Vector2.UnitY * i))
				{
					actor.Position -= Vector2.UnitY * i;
					Collidable = was;
					return true;
				}
			}
			Collidable = was;
			return false;
		}
	}
}
