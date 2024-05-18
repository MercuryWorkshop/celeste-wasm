using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class CrystalStaticSpinner : Entity
	{
		private class CoreModeListener : Component
		{
			public CrystalStaticSpinner Parent;

			public CoreModeListener(CrystalStaticSpinner parent)
				: base(active: true, visible: false)
			{
				Parent = parent;
			}

			public override void Update()
			{
				Level level = base.Scene as Level;
				if ((Parent.color == CrystalColor.Blue && level.CoreMode == Session.CoreModes.Hot) || (Parent.color == CrystalColor.Red && level.CoreMode == Session.CoreModes.Cold))
				{
					if (Parent.color == CrystalColor.Blue)
					{
						Parent.color = CrystalColor.Red;
					}
					else
					{
						Parent.color = CrystalColor.Blue;
					}
					Parent.ClearSprites();
					Parent.CreateSprites();
				}
			}
		}

		private class Border : Entity
		{
			private Entity[] drawing = new Entity[2];

			public Border(Entity parent, Entity filler)
			{
				drawing[0] = parent;
				drawing[1] = filler;
				base.Depth = parent.Depth + 2;
			}

			public override void Render()
			{
				if (drawing[0].Visible)
				{
					DrawBorder(drawing[0]);
					DrawBorder(drawing[1]);
				}
			}

			private void DrawBorder(Entity entity)
			{
				if (entity == null)
				{
					return;
				}
				foreach (Component component in entity.Components)
				{
					if (component is Image img)
					{
						Color col = img.Color;
						Vector2 pos = img.Position;
						img.Color = Color.Black;
						img.Position = pos + new Vector2(0f, -1f);
						img.Render();
						img.Position = pos + new Vector2(0f, 1f);
						img.Render();
						img.Position = pos + new Vector2(-1f, 0f);
						img.Render();
						img.Position = pos + new Vector2(1f, 0f);
						img.Render();
						img.Color = col;
						img.Position = pos;
					}
				}
			}
		}

		public static ParticleType P_Move;

		public const float ParticleInterval = 0.02f;

		private static Dictionary<CrystalColor, string> fgTextureLookup = new Dictionary<CrystalColor, string>
		{
			{
				CrystalColor.Blue,
				"danger/crystal/fg_blue"
			},
			{
				CrystalColor.Red,
				"danger/crystal/fg_red"
			},
			{
				CrystalColor.Purple,
				"danger/crystal/fg_purple"
			},
			{
				CrystalColor.Rainbow,
				"danger/crystal/fg_white"
			}
		};

		private static Dictionary<CrystalColor, string> bgTextureLookup = new Dictionary<CrystalColor, string>
		{
			{
				CrystalColor.Blue,
				"danger/crystal/bg_blue"
			},
			{
				CrystalColor.Red,
				"danger/crystal/bg_red"
			},
			{
				CrystalColor.Purple,
				"danger/crystal/bg_purple"
			},
			{
				CrystalColor.Rainbow,
				"danger/crystal/bg_white"
			}
		};

		public bool AttachToSolid;

		private Entity filler;

		private Border border;

		private float offset = Calc.Random.NextFloat();

		private bool expanded;

		private int randomSeed;

		private CrystalColor color;

		public CrystalStaticSpinner(Vector2 position, bool attachToSolid, CrystalColor color)
			: base(position)
		{
			this.color = color;
			base.Tag = Tags.TransitionUpdate;
			base.Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
			Visible = false;
			Add(new PlayerCollider(OnPlayer));
			Add(new HoldableCollider(OnHoldable));
			Add(new LedgeBlocker());
			base.Depth = -8500;
			AttachToSolid = attachToSolid;
			if (attachToSolid)
			{
				Add(new StaticMover
				{
					OnShake = OnShake,
					SolidChecker = IsRiding,
					OnDestroy = base.RemoveSelf
				});
			}
			randomSeed = Calc.Random.Next();
		}

		public CrystalStaticSpinner(EntityData data, Vector2 offset, CrystalColor color)
			: this(data.Position + offset, data.Bool("attachToSolid"), color)
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if ((scene as Level).Session.Area.ID == 9)
			{
				Add(new CoreModeListener(this));
				if ((scene as Level).CoreMode == Session.CoreModes.Cold)
				{
					color = CrystalColor.Blue;
				}
				else
				{
					color = CrystalColor.Red;
				}
			}
			if (InView())
			{
				CreateSprites();
			}
		}

		public void ForceInstantiate()
		{
			CreateSprites();
			Visible = true;
		}

		public override void Update()
		{
			if (!Visible)
			{
				Collidable = false;
				if (InView())
				{
					Visible = true;
					if (!expanded)
					{
						CreateSprites();
					}
					if (color == CrystalColor.Rainbow)
					{
						UpdateHue();
					}
				}
			}
			else
			{
				base.Update();
				if (color == CrystalColor.Rainbow && base.Scene.OnInterval(0.08f, offset))
				{
					UpdateHue();
				}
				if (base.Scene.OnInterval(0.25f, offset) && !InView())
				{
					Visible = false;
				}
				if (base.Scene.OnInterval(0.05f, offset))
				{
					Player player = base.Scene.Tracker.GetEntity<Player>();
					if (player != null)
					{
						Collidable = Math.Abs(player.X - base.X) < 128f && Math.Abs(player.Y - base.Y) < 128f;
					}
				}
			}
			if (filler != null)
			{
				filler.Position = Position;
			}
		}

		private void UpdateHue()
		{
			foreach (Component component in base.Components)
			{
				if (component is Image img2)
				{
					img2.Color = GetHue(Position + img2.Position);
				}
			}
			if (filler == null)
			{
				return;
			}
			foreach (Component component2 in filler.Components)
			{
				if (component2 is Image img)
				{
					img.Color = GetHue(Position + img.Position);
				}
			}
		}

		private bool InView()
		{
			Camera camera = (base.Scene as Level).Camera;
			if (base.X > camera.X - 16f && base.Y > camera.Y - 16f && base.X < camera.X + 320f + 16f)
			{
				return base.Y < camera.Y + 180f + 16f;
			}
			return false;
		}

		private void CreateSprites()
		{
			if (expanded)
			{
				return;
			}
			Calc.PushRandom(randomSeed);
			List<MTexture> imgs = GFX.Game.GetAtlasSubtextures(fgTextureLookup[color]);
			MTexture img = Calc.Random.Choose(imgs);
			Color hue = Color.White;
			if (color == CrystalColor.Rainbow)
			{
				hue = GetHue(Position);
			}
			if (!SolidCheck(new Vector2(base.X - 4f, base.Y - 4f)))
			{
				Add(new Image(img.GetSubtexture(0, 0, 14, 14)).SetOrigin(12f, 12f).SetColor(hue));
			}
			if (!SolidCheck(new Vector2(base.X + 4f, base.Y - 4f)))
			{
				Add(new Image(img.GetSubtexture(10, 0, 14, 14)).SetOrigin(2f, 12f).SetColor(hue));
			}
			if (!SolidCheck(new Vector2(base.X + 4f, base.Y + 4f)))
			{
				Add(new Image(img.GetSubtexture(10, 10, 14, 14)).SetOrigin(2f, 2f).SetColor(hue));
			}
			if (!SolidCheck(new Vector2(base.X - 4f, base.Y + 4f)))
			{
				Add(new Image(img.GetSubtexture(0, 10, 14, 14)).SetOrigin(12f, 2f).SetColor(hue));
			}
			foreach (CrystalStaticSpinner crystal in base.Scene.Tracker.GetEntities<CrystalStaticSpinner>())
			{
				if (crystal != this && crystal.AttachToSolid == AttachToSolid && crystal.X >= base.X && (crystal.Position - Position).Length() < 24f)
				{
					AddSprite((Position + crystal.Position) / 2f - Position);
				}
			}
			base.Scene.Add(border = new Border(this, filler));
			expanded = true;
			Calc.PopRandom();
		}

		private void AddSprite(Vector2 offset)
		{
			if (filler == null)
			{
				base.Scene.Add(filler = new Entity(Position));
				filler.Depth = base.Depth + 1;
			}
			List<MTexture> imgs = GFX.Game.GetAtlasSubtextures(bgTextureLookup[color]);
			Image spr = new Image(Calc.Random.Choose(imgs));
			spr.Position = offset;
			spr.Rotation = (float)Calc.Random.Choose(0, 1, 2, 3) * ((float)Math.PI / 2f);
			spr.CenterOrigin();
			if (color == CrystalColor.Rainbow)
			{
				spr.Color = GetHue(Position + offset);
			}
			filler.Add(spr);
		}

		private bool SolidCheck(Vector2 position)
		{
			if (AttachToSolid)
			{
				return false;
			}
			foreach (Solid item in base.Scene.CollideAll<Solid>(position))
			{
				if (item is SolidTiles)
				{
					return true;
				}
			}
			return false;
		}

		private void ClearSprites()
		{
			if (filler != null)
			{
				filler.RemoveSelf();
			}
			filler = null;
			if (border != null)
			{
				border.RemoveSelf();
			}
			border = null;
			foreach (Image item in base.Components.GetAll<Image>())
			{
				item.RemoveSelf();
			}
			expanded = false;
		}

		private void OnShake(Vector2 pos)
		{
			foreach (Component img in base.Components)
			{
				if (img is Image)
				{
					(img as Image).Position = pos;
				}
			}
		}

		private bool IsRiding(Solid solid)
		{
			return CollideCheck(solid);
		}

		private void OnPlayer(Player player)
		{
			player.Die((player.Position - Position).SafeNormalize());
		}

		private void OnHoldable(Holdable h)
		{
			h.HitSpinner(this);
		}

		public override void Removed(Scene scene)
		{
			if (filler != null && filler.Scene == scene)
			{
				filler.RemoveSelf();
			}
			if (border != null && border.Scene == scene)
			{
				border.RemoveSelf();
			}
			base.Removed(scene);
		}

		public void Destroy(bool boss = false)
		{
			if (InView())
			{
				Audio.Play("event:/game/06_reflection/fall_spike_smash", Position);
				Color col = Color.White;
				if (color == CrystalColor.Red)
				{
					col = Calc.HexToColor("ff4f4f");
				}
				else if (color == CrystalColor.Blue)
				{
					col = Calc.HexToColor("639bff");
				}
				else if (color == CrystalColor.Purple)
				{
					col = Calc.HexToColor("ff4fef");
				}
				CrystalDebris.Burst(Position, col, boss, 8);
			}
			RemoveSelf();
		}

		private Color GetHue(Vector2 position)
		{
			float dist = 280f;
			float percent = (position.Length() + base.Scene.TimeActive * 50f) % dist / dist;
			return Calc.HsvToColor(0.4f + Calc.YoYo(percent) * 0.4f, 0.4f, 0.9f);
		}
	}
}
