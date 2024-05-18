using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	[Pooled]
	[Tracked(false)]
	public class SpeedRing : Entity
	{
		private int index;

		private float angle;

		private float lerp;

		private Color color;

		private Vector2 normal;

		public SpeedRing Init(Vector2 position, float angle, Color color)
		{
			Position = position;
			this.angle = angle;
			this.color = color;
			lerp = 0f;
			normal = Calc.AngleToVector(angle, 1f);
			return this;
		}

		public override void Update()
		{
			lerp += 3f * Engine.DeltaTime;
			Position += normal * 10f * Engine.DeltaTime;
			if (lerp >= 1f)
			{
				RemoveSelf();
			}
		}

		public override void Render()
		{
			Color c = color * MathHelper.Lerp(0.6f, 0f, lerp);
			if (c.A > 0)
			{
				Draw.SpriteBatch.Draw((RenderTarget2D)GameplayBuffers.SpeedRings, Position + new Vector2(-32f, -32f), new Rectangle(index % 4 * 64, index / 4 * 64, 64, 64), c);
			}
		}

		private void DrawRing(Vector2 position)
		{
			float radius = MathHelper.Lerp(4f, 14f, lerp);
			Vector2 last = GetVectorAtAngle(0f, radius);
			for (int i = 1; i <= 8; i++)
			{
				float angle = (float)i * ((float)Math.PI / 8f);
				Vector2 at = GetVectorAtAngle(angle, radius);
				Draw.Line(position + last, position + at, Color.White);
				Draw.Line(position - last, position - at, Color.White);
				last = at;
			}
		}

		private Vector2 GetVectorAtAngle(float radians, float maxRadius)
		{
			Vector2 vec = Calc.AngleToVector(radians, 1f);
			float length = MathHelper.Lerp(maxRadius, maxRadius * 0.5f, Math.Abs(Vector2.Dot(vec, normal)));
			return vec * length;
		}

		public static void DrawToBuffer(Level level)
		{
			List<Entity> rings = level.Tracker.GetEntities<SpeedRing>();
			int index = 0;
			if (rings.Count <= 0)
			{
				return;
			}
			Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.SpeedRings);
			Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
			foreach (SpeedRing item in rings)
			{
				item.index = index;
				item.DrawRing(new Vector2(index % 4 * 64 + 32, index / 4 * 64 + 32));
				index++;
			}
			Draw.SpriteBatch.End();
		}
	}
}
