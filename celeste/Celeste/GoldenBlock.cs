using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class GoldenBlock : Solid
	{
		private MTexture[,] nineSlice;

		private Image berry;

		private float startY;

		private float yLerp;

		private float sinkTimer;

		private float renderLerp;

		public GoldenBlock(Vector2 position, float width, float height)
			: base(position, width, height, safe: false)
		{
			startY = base.Y;
			berry = new Image(GFX.Game["collectables/goldberry/idle00"]);
			berry.CenterOrigin();
			berry.Position = new Vector2(width / 2f, height / 2f);
			MTexture tex = GFX.Game["objects/goldblock"];
			nineSlice = new MTexture[3, 3];
			for (int x = 0; x < 3; x++)
			{
				for (int y = 0; y < 3; y++)
				{
					nineSlice[x, y] = tex.GetSubtexture(new Rectangle(x * 8, y * 8, 8, 8));
				}
			}
			base.Depth = -10000;
			Add(new LightOcclude());
			Add(new MirrorSurface());
			SurfaceSoundIndex = 32;
		}

		public GoldenBlock(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height)
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			Visible = false;
			Collidable = false;
			renderLerp = 1f;
			bool goldenExists = false;
			foreach (Strawberry strawb in scene.Entities.FindAll<Strawberry>())
			{
				if (strawb.Golden && strawb.Follower.Leader != null)
				{
					goldenExists = true;
					break;
				}
			}
			if (!goldenExists)
			{
				RemoveSelf();
			}
		}

		public override void Update()
		{
			base.Update();
			if (!Visible)
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null && player.X > base.X - 80f)
				{
					Visible = true;
					Collidable = true;
					renderLerp = 1f;
				}
			}
			if (Visible)
			{
				renderLerp = Calc.Approach(renderLerp, 0f, Engine.DeltaTime * 3f);
			}
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

		private void DrawBlock(Vector2 offset, Color color)
		{
			float columns = base.Collider.Width / 8f - 1f;
			float rows = base.Collider.Height / 8f - 1f;
			for (int x = 0; (float)x <= columns; x++)
			{
				for (int y = 0; (float)y <= rows; y++)
				{
					int tx = (((float)x < columns) ? Math.Min(x, 1) : 2);
					int ty = (((float)y < rows) ? Math.Min(y, 1) : 2);
					nineSlice[tx, ty].Draw(Position + offset + base.Shake + new Vector2(x * 8, y * 8), Vector2.Zero, color);
				}
			}
		}

		public override void Render()
		{
			Level level = base.Scene as Level;
			Vector2 offset = new Vector2(0f, ((float)level.Bounds.Bottom - startY + 32f) * Ease.CubeIn(renderLerp));
			Vector2 was = Position;
			Position += offset;
			DrawBlock(new Vector2(-1f, 0f), Color.Black);
			DrawBlock(new Vector2(1f, 0f), Color.Black);
			DrawBlock(new Vector2(0f, -1f), Color.Black);
			DrawBlock(new Vector2(0f, 1f), Color.Black);
			DrawBlock(Vector2.Zero, Color.White);
			berry.Color = Color.White;
			berry.RenderPosition = base.Center;
			berry.Render();
			Position = was;
		}
	}
}
