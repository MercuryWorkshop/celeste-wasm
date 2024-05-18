using System;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class SwapBlock : Solid
	{
		public enum Themes
		{
			Normal,
			Moon
		}

		private class PathRenderer : Entity
		{
			private SwapBlock block;

			private MTexture pathTexture;

			private MTexture clipTexture = new MTexture();

			private float timer;

			public PathRenderer(SwapBlock block)
				: base(block.Position)
			{
				this.block = block;
				base.Depth = 8999;
				pathTexture = GFX.Game["objects/swapblock/path" + ((block.start.X == block.end.X) ? "V" : "H")];
				timer = Calc.Random.NextFloat();
			}

			public override void Update()
			{
				base.Update();
				timer += Engine.DeltaTime * 4f;
			}

			public override void Render()
			{
				if (block.Theme != Themes.Moon)
				{
					for (int tx = block.moveRect.Left; tx < block.moveRect.Right; tx += pathTexture.Width)
					{
						for (int ty = block.moveRect.Top; ty < block.moveRect.Bottom; ty += pathTexture.Height)
						{
							pathTexture.GetSubtexture(0, 0, Math.Min(pathTexture.Width, block.moveRect.Right - tx), Math.Min(pathTexture.Height, block.moveRect.Bottom - ty), clipTexture);
							clipTexture.DrawCentered(new Vector2(tx + clipTexture.Width / 2, ty + clipTexture.Height / 2), Color.White);
						}
					}
				}
				float glow = 0.5f * (0.5f + ((float)Math.Sin(timer) + 1f) * 0.25f);
				block.DrawBlockStyle(new Vector2(block.moveRect.X, block.moveRect.Y), block.moveRect.Width, block.moveRect.Height, block.nineSliceTarget, null, Color.White * glow);
			}
		}

		public static ParticleType P_Move;

		private const float ReturnTime = 0.8f;

		public Vector2 Direction;

		public bool Swapping;

		public Themes Theme;

		private Vector2 start;

		private Vector2 end;

		private float lerp;

		private int target;

		private Rectangle moveRect;

		private float speed;

		private float maxForwardSpeed;

		private float maxBackwardSpeed;

		private float returnTimer;

		private float redAlpha = 1f;

		private MTexture[,] nineSliceGreen;

		private MTexture[,] nineSliceRed;

		private MTexture[,] nineSliceTarget;

		private Sprite middleGreen;

		private Sprite middleRed;

		private PathRenderer path;

		private EventInstance moveSfx;

		private EventInstance returnSfx;

		private DisplacementRenderer.Burst burst;

		private float particlesRemainder;

		public SwapBlock(Vector2 position, float width, float height, Vector2 node, Themes theme)
			: base(position, width, height, safe: false)
		{
			Theme = theme;
			start = Position;
			end = node;
			maxForwardSpeed = 360f / Vector2.Distance(start, end);
			maxBackwardSpeed = maxForwardSpeed * 0.4f;
			Direction.X = Math.Sign(end.X - start.X);
			Direction.Y = Math.Sign(end.Y - start.Y);
			Add(new DashListener
			{
				OnDash = OnDash
			});
			int x = (int)MathHelper.Min(base.X, node.X);
			int y2 = (int)MathHelper.Min(base.Y, node.Y);
			int r = (int)MathHelper.Max(base.X + base.Width, node.X + base.Width);
			int b = (int)MathHelper.Max(base.Y + base.Height, node.Y + base.Height);
			moveRect = new Rectangle(x, y2, r - x, b - y2);
			MTexture tex;
			MTexture texRed;
			MTexture texTarget;
			if (Theme == Themes.Moon)
			{
				tex = GFX.Game["objects/swapblock/moon/block"];
				texRed = GFX.Game["objects/swapblock/moon/blockRed"];
				texTarget = GFX.Game["objects/swapblock/moon/target"];
			}
			else
			{
				tex = GFX.Game["objects/swapblock/block"];
				texRed = GFX.Game["objects/swapblock/blockRed"];
				texTarget = GFX.Game["objects/swapblock/target"];
			}
			nineSliceGreen = new MTexture[3, 3];
			nineSliceRed = new MTexture[3, 3];
			nineSliceTarget = new MTexture[3, 3];
			for (int x2 = 0; x2 < 3; x2++)
			{
				for (int y = 0; y < 3; y++)
				{
					nineSliceGreen[x2, y] = tex.GetSubtexture(new Rectangle(x2 * 8, y * 8, 8, 8));
					nineSliceRed[x2, y] = texRed.GetSubtexture(new Rectangle(x2 * 8, y * 8, 8, 8));
					nineSliceTarget[x2, y] = texTarget.GetSubtexture(new Rectangle(x2 * 8, y * 8, 8, 8));
				}
			}
			if (Theme == Themes.Normal)
			{
				Add(middleGreen = GFX.SpriteBank.Create("swapBlockLight"));
				Add(middleRed = GFX.SpriteBank.Create("swapBlockLightRed"));
			}
			else if (Theme == Themes.Moon)
			{
				Add(middleGreen = GFX.SpriteBank.Create("swapBlockLightMoon"));
				Add(middleRed = GFX.SpriteBank.Create("swapBlockLightRedMoon"));
			}
			Add(new LightOcclude(0.2f));
			base.Depth = -9999;
		}

		public SwapBlock(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset, data.Enum("theme", Themes.Normal))
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			scene.Add(path = new PathRenderer(this));
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);
			Audio.Stop(moveSfx);
			Audio.Stop(returnSfx);
		}

		public override void SceneEnd(Scene scene)
		{
			base.SceneEnd(scene);
			Audio.Stop(moveSfx);
			Audio.Stop(returnSfx);
		}

		private void OnDash(Vector2 direction)
		{
			Swapping = lerp < 1f;
			target = 1;
			returnTimer = 0.8f;
			burst = (base.Scene as Level).Displacement.AddBurst(base.Center, 0.2f, 0f, 16f);
			if (lerp >= 0.2f)
			{
				speed = maxForwardSpeed;
			}
			else
			{
				speed = MathHelper.Lerp(maxForwardSpeed * 0.333f, maxForwardSpeed, lerp / 0.2f);
			}
			Audio.Stop(returnSfx);
			Audio.Stop(moveSfx);
			if (!Swapping)
			{
				Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", base.Center);
			}
			else
			{
				moveSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_move", base.Center);
			}
		}

		public override void Update()
		{
			base.Update();
			if (returnTimer > 0f)
			{
				returnTimer -= Engine.DeltaTime;
				if (returnTimer <= 0f)
				{
					target = 0;
					speed = 0f;
					returnSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_return", base.Center);
				}
			}
			if (burst != null)
			{
				burst.Position = base.Center;
			}
			redAlpha = Calc.Approach(redAlpha, (target != 1) ? 1 : 0, Engine.DeltaTime * 32f);
			if (target == 0 && lerp == 0f)
			{
				middleRed.SetAnimationFrame(0);
				middleGreen.SetAnimationFrame(0);
			}
			if (target == 1)
			{
				speed = Calc.Approach(speed, maxForwardSpeed, maxForwardSpeed / 0.2f * Engine.DeltaTime);
			}
			else
			{
				speed = Calc.Approach(speed, maxBackwardSpeed, maxBackwardSpeed / 1.5f * Engine.DeltaTime);
			}
			float old = lerp;
			lerp = Calc.Approach(lerp, target, speed * Engine.DeltaTime);
			if (lerp != old)
			{
				Vector2 liftSpeed = (end - start) * speed;
				Vector2 position = Position;
				if (target == 1)
				{
					liftSpeed = (end - start) * maxForwardSpeed;
				}
				if (lerp < old)
				{
					liftSpeed *= -1f;
				}
				if (target == 1 && base.Scene.OnInterval(0.02f))
				{
					MoveParticles(end - start);
				}
				MoveTo(Vector2.Lerp(start, end, lerp), liftSpeed);
				if (position != Position)
				{
					Audio.Position(moveSfx, base.Center);
					Audio.Position(returnSfx, base.Center);
					if (Position == start && target == 0)
					{
						Audio.SetParameter(returnSfx, "end", 1f);
						Audio.Play("event:/game/05_mirror_temple/swapblock_return_end", base.Center);
					}
					else if (Position == end && target == 1)
					{
						Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", base.Center);
					}
				}
			}
			if (Swapping && lerp >= 1f)
			{
				Swapping = false;
			}
			StopPlayerRunIntoAnimation = lerp <= 0f || lerp >= 1f;
		}

		private void MoveParticles(Vector2 normal)
		{
			Vector2 at;
			Vector2 range;
			float dir;
			float add;
			if (normal.X > 0f)
			{
				at = base.CenterLeft;
				range = Vector2.UnitY * (base.Height - 6f);
				dir = (float)Math.PI;
				add = Math.Max(2f, base.Height / 14f);
			}
			else if (normal.X < 0f)
			{
				at = base.CenterRight;
				range = Vector2.UnitY * (base.Height - 6f);
				dir = 0f;
				add = Math.Max(2f, base.Height / 14f);
			}
			else if (normal.Y > 0f)
			{
				at = base.TopCenter;
				range = Vector2.UnitX * (base.Width - 6f);
				dir = -(float)Math.PI / 2f;
				add = Math.Max(2f, base.Width / 14f);
			}
			else
			{
				at = base.BottomCenter;
				range = Vector2.UnitX * (base.Width - 6f);
				dir = (float)Math.PI / 2f;
				add = Math.Max(2f, base.Width / 14f);
			}
			particlesRemainder += add;
			int amount = (int)particlesRemainder;
			particlesRemainder -= amount;
			range *= 0.5f;
			SceneAs<Level>().Particles.Emit(P_Move, amount, at, range, dir);
		}

		public override void Render()
		{
			Vector2 pos = Position + base.Shake;
			if (lerp != (float)target && speed > 0f)
			{
				Vector2 d = (end - start).SafeNormalize();
				if (target == 1)
				{
					d *= -1f;
				}
				float blur = speed / maxForwardSpeed;
				float len = 16f * blur;
				for (int i = 2; (float)i < len; i += 2)
				{
					DrawBlockStyle(pos + d * i, base.Width, base.Height, nineSliceGreen, middleGreen, Color.White * (1f - (float)i / len));
				}
			}
			if (redAlpha < 1f)
			{
				DrawBlockStyle(pos, base.Width, base.Height, nineSliceGreen, middleGreen, Color.White);
			}
			if (redAlpha > 0f)
			{
				DrawBlockStyle(pos, base.Width, base.Height, nineSliceRed, middleRed, Color.White * redAlpha);
			}
		}

		private void DrawBlockStyle(Vector2 pos, float width, float height, MTexture[,] ninSlice, Sprite middle, Color color)
		{
			int columns = (int)(width / 8f);
			int rows = (int)(height / 8f);
			ninSlice[0, 0].Draw(pos + new Vector2(0f, 0f), Vector2.Zero, color);
			ninSlice[2, 0].Draw(pos + new Vector2(width - 8f, 0f), Vector2.Zero, color);
			ninSlice[0, 2].Draw(pos + new Vector2(0f, height - 8f), Vector2.Zero, color);
			ninSlice[2, 2].Draw(pos + new Vector2(width - 8f, height - 8f), Vector2.Zero, color);
			for (int x2 = 1; x2 < columns - 1; x2++)
			{
				ninSlice[1, 0].Draw(pos + new Vector2(x2 * 8, 0f), Vector2.Zero, color);
				ninSlice[1, 2].Draw(pos + new Vector2(x2 * 8, height - 8f), Vector2.Zero, color);
			}
			for (int y2 = 1; y2 < rows - 1; y2++)
			{
				ninSlice[0, 1].Draw(pos + new Vector2(0f, y2 * 8), Vector2.Zero, color);
				ninSlice[2, 1].Draw(pos + new Vector2(width - 8f, y2 * 8), Vector2.Zero, color);
			}
			for (int x = 1; x < columns - 1; x++)
			{
				for (int y = 1; y < rows - 1; y++)
				{
					ninSlice[1, 1].Draw(pos + new Vector2(x, y) * 8f, Vector2.Zero, color);
				}
			}
			if (middle != null)
			{
				middle.Color = color;
				middle.RenderPosition = pos + new Vector2(width / 2f, height / 2f);
				middle.Render();
			}
		}
	}
}
