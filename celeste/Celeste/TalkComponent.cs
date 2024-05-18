using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class TalkComponent : Component
	{
		public class HoverDisplay
		{
			public MTexture Texture;

			public Vector2 InputPosition;

			public string SfxIn = "event:/ui/game/hotspot_main_in";

			public string SfxOut = "event:/ui/game/hotspot_main_out";
		}

		public class TalkComponentUI : Entity
		{
			public TalkComponent Handler;

			private bool highlighted;

			private float slide;

			private float timer;

			private Wiggler wiggler;

			private float alpha = 1f;

			private Color lineColor = new Color(1f, 1f, 1f);

			public bool Highlighted
			{
				get
				{
					return highlighted;
				}
				set
				{
					if ((highlighted != value) & Display)
					{
						highlighted = value;
						if (highlighted)
						{
							Audio.Play(Handler.HoverUI.SfxIn);
						}
						else
						{
							Audio.Play(Handler.HoverUI.SfxOut);
						}
						wiggler.Start();
					}
				}
			}

			public bool Display
			{
				get
				{
					if (!Handler.Enabled)
					{
						return false;
					}
					if (base.Scene == null)
					{
						return false;
					}
					if (base.Scene.Tracker.GetEntity<Textbox>() != null)
					{
						return false;
					}
					Player player = base.Scene.Tracker.GetEntity<Player>();
					if (player == null || player.StateMachine.State == 11)
					{
						return false;
					}
					Level level = base.Scene as Level;
					if (!level.FrozenOrPaused)
					{
						return level.RetryPlayerCorpse == null;
					}
					return false;
				}
			}

			public TalkComponentUI(TalkComponent handler)
			{
				Handler = handler;
				AddTag((int)Tags.HUD | (int)Tags.Persistent);
				Add(wiggler = Wiggler.Create(0.25f, 4f));
			}

			public override void Awake(Scene scene)
			{
				base.Awake(scene);
				if (Handler.Entity == null || base.Scene.CollideCheck<FakeWall>(Handler.Entity.Position))
				{
					alpha = 0f;
				}
			}

			public override void Update()
			{
				timer += Engine.DeltaTime;
				slide = Calc.Approach(slide, Display ? 1 : 0, Engine.DeltaTime * 4f);
				if (alpha < 1f && Handler.Entity != null && !base.Scene.CollideCheck<FakeWall>(Handler.Entity.Position))
				{
					alpha = Calc.Approach(alpha, 1f, 2f * Engine.DeltaTime);
				}
				base.Update();
			}

			public override void Render()
			{
				Level level = base.Scene as Level;
				if (level.FrozenOrPaused || !(slide > 0f) || Handler.Entity == null)
				{
					return;
				}
				Vector2 camera = level.Camera.Position.Floor();
				Vector2 p = Handler.Entity.Position + Handler.DrawAt - camera;
				if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
				{
					p.X = 320f - p.X;
				}
				p.X *= 6f;
				p.Y *= 6f;
				p.Y += (float)Math.Sin(timer * 4f) * 12f + 64f * (1f - Ease.CubeOut(slide));
				float scale = ((!Highlighted) ? (1f + wiggler.Value * 0.5f) : (1f - wiggler.Value * 0.5f));
				float alpha = Ease.CubeInOut(slide) * this.alpha;
				Color color = lineColor * alpha;
				if (Highlighted)
				{
					Handler.HoverUI.Texture.DrawJustified(p, new Vector2(0.5f, 1f), color * this.alpha, scale);
				}
				else
				{
					GFX.Gui["hover/idle"].DrawJustified(p, new Vector2(0.5f, 1f), color * this.alpha, scale);
				}
				if (Highlighted)
				{
					Vector2 at = p + Handler.HoverUI.InputPosition * scale;
					if (Input.GuiInputController())
					{
						Input.GuiButton(Input.Talk).DrawJustified(at, new Vector2(0.5f), Color.White * alpha, scale);
					}
					else
					{
						ActiveFont.DrawOutline(Input.FirstKey(Input.Talk).ToString().ToUpper(), at, new Vector2(0.5f), new Vector2(scale), Color.White * alpha, 2f, Color.Black);
					}
				}
			}
		}

		public static TalkComponent PlayerOver;

		public bool Enabled = true;

		public Rectangle Bounds;

		public Vector2 DrawAt;

		public Action<Player> OnTalk;

		public bool PlayerMustBeFacing = true;

		public TalkComponentUI UI;

		public HoverDisplay HoverUI;

		private float cooldown;

		private float hoverTimer;

		private float disableDelay;

		public TalkComponent(Rectangle bounds, Vector2 drawAt, Action<Player> onTalk, HoverDisplay hoverDisplay = null)
			: base(active: true, visible: true)
		{
			Bounds = bounds;
			DrawAt = drawAt;
			OnTalk = onTalk;
			if (hoverDisplay == null)
			{
				HoverUI = new HoverDisplay
				{
					Texture = GFX.Gui["hover/highlight"],
					InputPosition = new Vector2(0f, -75f)
				};
			}
			else
			{
				HoverUI = hoverDisplay;
			}
		}

		public override void Update()
		{
			if (UI == null)
			{
				base.Entity.Scene.Add(UI = new TalkComponentUI(this));
			}
			Player player = base.Scene.Tracker.GetEntity<Player>();
			bool hovering = disableDelay < 0.05f && player != null && player.CollideRect(new Rectangle((int)(base.Entity.X + (float)Bounds.X), (int)(base.Entity.Y + (float)Bounds.Y), Bounds.Width, Bounds.Height)) && player.OnGround() && player.Bottom < base.Entity.Y + (float)Bounds.Bottom + 4f && player.StateMachine.State == 0 && (!PlayerMustBeFacing || Math.Abs(player.X - base.Entity.X) <= 16f || player.Facing == (Facings)Math.Sign(base.Entity.X - player.X)) && (PlayerOver == null || PlayerOver == this);
			if (hovering)
			{
				hoverTimer += Engine.DeltaTime;
			}
			else if (UI.Display)
			{
				hoverTimer = 0f;
			}
			if (PlayerOver == this && !hovering)
			{
				PlayerOver = null;
			}
			else if (hovering)
			{
				PlayerOver = this;
			}
			if (hovering && cooldown <= 0f && player != null && (int)player.StateMachine == 0 && Input.Talk.Pressed && Enabled && !base.Scene.Paused)
			{
				cooldown = 0.1f;
				if (OnTalk != null)
				{
					OnTalk(player);
				}
			}
			if (hovering && (int)player.StateMachine == 0)
			{
				cooldown -= Engine.DeltaTime;
			}
			if (!Enabled)
			{
				disableDelay += Engine.DeltaTime;
			}
			else
			{
				disableDelay = 0f;
			}
			UI.Highlighted = hovering && hoverTimer > 0.1f;
			base.Update();
		}

		public override void Removed(Entity entity)
		{
			Dispose();
			base.Removed(entity);
		}

		public override void EntityRemoved(Scene scene)
		{
			Dispose();
			base.EntityRemoved(scene);
		}

		public override void SceneEnd(Scene scene)
		{
			Dispose();
			base.SceneEnd(scene);
		}

		private void Dispose()
		{
			if (PlayerOver == this)
			{
				PlayerOver = null;
			}
			base.Scene.Remove(UI);
			UI = null;
		}

		public override void DebugRender(Camera camera)
		{
			base.DebugRender(camera);
			Draw.HollowRect(base.Entity.X + (float)Bounds.X, base.Entity.Y + (float)Bounds.Y, Bounds.Width, Bounds.Height, Color.Green);
		}
	}
}
