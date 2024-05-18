using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class StarJumpBlock : Solid
	{
		private Level level;

		private bool sinks;

		private float startY;

		private float yLerp;

		private float sinkTimer;

		public StarJumpBlock(Vector2 position, float width, float height, bool sinks)
			: base(position, width, height, safe: false)
		{
			base.Depth = -10000;
			this.sinks = sinks;
			Add(new LightOcclude());
			startY = base.Y;
			SurfaceSoundIndex = 32;
		}

		public StarJumpBlock(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.Bool("sinks"))
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			level = SceneAs<Level>();
			List<MTexture> railingLeft = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/leftrailing");
			List<MTexture> railing = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/railing");
			List<MTexture> railingRight = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/rightrailing");
			List<MTexture> edgeH = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/edgeH");
			for (int x = 8; (float)x < base.Width - 8f; x += 8)
			{
				if (Open(x, -8f))
				{
					Image img2 = new Image(Calc.Random.Choose(edgeH));
					img2.CenterOrigin();
					img2.Position = new Vector2(x + 4, 4f);
					Add(img2);
					Image r = new Image(railing[mod((int)(base.X + (float)x) / 8, railing.Count)]);
					r.Position = new Vector2(x, -8f);
					Add(r);
				}
				if (Open(x, base.Height))
				{
					Image img = new Image(Calc.Random.Choose(edgeH));
					img.CenterOrigin();
					img.Scale.Y = -1f;
					img.Position = new Vector2(x + 4, base.Height - 4f);
					Add(img);
				}
			}
			List<MTexture> edgeV = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/edgeV");
			for (int y = 8; (float)y < base.Height - 8f; y += 8)
			{
				if (Open(-8f, y))
				{
					Image img4 = new Image(Calc.Random.Choose(edgeV));
					img4.CenterOrigin();
					img4.Scale.X = -1f;
					img4.Position = new Vector2(4f, y + 4);
					Add(img4);
				}
				if (Open(base.Width, y))
				{
					Image img3 = new Image(Calc.Random.Choose(edgeV));
					img3.CenterOrigin();
					img3.Position = new Vector2(base.Width - 4f, y + 4);
					Add(img3);
				}
			}
			List<MTexture> corners = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/corner");
			Image topleft = null;
			if (Open(-8f, 0f) && Open(0f, -8f))
			{
				topleft = new Image(Calc.Random.Choose(corners));
				topleft.Scale.X = -1f;
				Image r5 = new Image(railingLeft[mod((int)base.X / 8, railingLeft.Count)]);
				r5.Position = new Vector2(0f, -8f);
				Add(r5);
			}
			else if (Open(-8f, 0f))
			{
				topleft = new Image(Calc.Random.Choose(edgeV));
				topleft.Scale.X = -1f;
			}
			else if (Open(0f, -8f))
			{
				topleft = new Image(Calc.Random.Choose(edgeH));
				Image r4 = new Image(railing[mod((int)base.X / 8, railing.Count)]);
				r4.Position = new Vector2(0f, -8f);
				Add(r4);
			}
			topleft.CenterOrigin();
			topleft.Position = new Vector2(4f, 4f);
			Add(topleft);
			Image topright = null;
			if (Open(base.Width, 0f) && Open(base.Width - 8f, -8f))
			{
				topright = new Image(Calc.Random.Choose(corners));
				Image r3 = new Image(railingRight[mod((int)(base.X + base.Width) / 8 - 1, railingRight.Count)]);
				r3.Position = new Vector2(base.Width - 8f, -8f);
				Add(r3);
			}
			else if (Open(base.Width, 0f))
			{
				topright = new Image(Calc.Random.Choose(edgeV));
			}
			else if (Open(base.Width - 8f, -8f))
			{
				topright = new Image(Calc.Random.Choose(edgeH));
				Image r2 = new Image(railing[mod((int)(base.X + base.Width) / 8 - 1, railing.Count)]);
				r2.Position = new Vector2(base.Width - 8f, -8f);
				Add(r2);
			}
			topright.CenterOrigin();
			topright.Position = new Vector2(base.Width - 4f, 4f);
			Add(topright);
			Image botleft = null;
			if (Open(-8f, base.Height - 8f) && Open(0f, base.Height))
			{
				botleft = new Image(Calc.Random.Choose(corners));
				botleft.Scale.X = -1f;
			}
			else if (Open(-8f, base.Height - 8f))
			{
				botleft = new Image(Calc.Random.Choose(edgeV));
				botleft.Scale.X = -1f;
			}
			else if (Open(0f, base.Height))
			{
				botleft = new Image(Calc.Random.Choose(edgeH));
			}
			botleft.Scale.Y = -1f;
			botleft.CenterOrigin();
			botleft.Position = new Vector2(4f, base.Height - 4f);
			Add(botleft);
			Image botright = null;
			if (Open(base.Width, base.Height - 8f) && Open(base.Width - 8f, base.Height))
			{
				botright = new Image(Calc.Random.Choose(corners));
			}
			else if (Open(base.Width, base.Height - 8f))
			{
				botright = new Image(Calc.Random.Choose(edgeV));
			}
			else if (Open(base.Width - 8f, base.Height))
			{
				botright = new Image(Calc.Random.Choose(edgeH));
			}
			botright.Scale.Y = -1f;
			botright.CenterOrigin();
			botright.Position = new Vector2(base.Width - 4f, base.Height - 4f);
			Add(botright);
		}

		private int mod(int x, int m)
		{
			return (x % m + m) % m;
		}

		private bool Open(float x, float y)
		{
			return !base.Scene.CollideCheck<StarJumpBlock>(new Vector2(base.X + x + 4f, base.Y + y + 4f));
		}

		public override void Update()
		{
			base.Update();
			if (sinks)
			{
				if (HasPlayerRider())
				{
					sinkTimer = 0.1f;
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
				float targetY = MathHelper.Lerp(startY, startY + 12f, Ease.SineInOut(yLerp));
				MoveToY(targetY);
			}
		}

		public override void Render()
		{
			StarJumpController controller = base.Scene.Tracker.GetEntity<StarJumpController>();
			if (controller != null)
			{
				Vector2 camera = level.Camera.Position.Floor();
				VirtualRenderTarget fill = controller.BlockFill;
				Draw.SpriteBatch.Draw((RenderTarget2D)fill, Position, new Rectangle((int)(base.X - camera.X), (int)(base.Y - camera.Y), (int)base.Width, (int)base.Height), Color.White);
			}
			base.Render();
		}
	}
}
