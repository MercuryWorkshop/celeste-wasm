using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class DisplacementRenderer : Renderer
	{
		public class Burst
		{
			public MTexture Texture;

			public Entity Follow;

			public Vector2 Position;

			public Vector2 Origin;

			public float Duration;

			public float Percent;

			public float ScaleFrom;

			public float ScaleTo = 1f;

			public Ease.Easer ScaleEaser;

			public float AlphaFrom = 1f;

			public float AlphaTo;

			public Ease.Easer AlphaEaser;

			public Rectangle? WorldClipRect;

			public Collider WorldClipCollider;

			public int WorldClipPadding;

			public Burst(MTexture texture, Vector2 position, Vector2 origin, float duration)
			{
				Texture = texture;
				Position = position;
				Origin = origin;
				Duration = duration;
			}

			public void Update()
			{
				Percent += Engine.DeltaTime / Duration;
			}

			public void Render()
			{
				Vector2 target = Position;
				if (Follow != null)
				{
					target += Follow.Position;
				}
				float alpha = ((AlphaEaser == null) ? (AlphaFrom + (AlphaTo - AlphaFrom) * Percent) : (AlphaFrom + (AlphaTo - AlphaFrom) * AlphaEaser(Percent)));
				float scale = ((ScaleEaser == null) ? (ScaleFrom + (ScaleTo - ScaleFrom) * Percent) : (ScaleFrom + (ScaleTo - ScaleFrom) * ScaleEaser(Percent)));
				Vector2 origin = Origin;
				Rectangle clip = new Rectangle(0, 0, Texture.Width, Texture.Height);
				if (WorldClipCollider != null)
				{
					WorldClipRect = WorldClipCollider.Bounds;
				}
				if (WorldClipRect.HasValue)
				{
					Rectangle worldClip = WorldClipRect.Value;
					worldClip.X -= 1 + WorldClipPadding;
					worldClip.Y -= 1 + WorldClipPadding;
					worldClip.Width += 1 + WorldClipPadding * 2;
					worldClip.Height += 1 + WorldClipPadding * 2;
					float left = target.X - origin.X * scale;
					if (left < (float)worldClip.Left)
					{
						int amount4 = (int)(((float)worldClip.Left - left) / scale);
						origin.X -= amount4;
						clip.X = amount4;
						clip.Width -= amount4;
					}
					float top = target.Y - origin.Y * scale;
					if (top < (float)worldClip.Top)
					{
						int amount3 = (int)(((float)worldClip.Top - top) / scale);
						origin.Y -= amount3;
						clip.Y = amount3;
						clip.Height -= amount3;
					}
					float right = target.X + ((float)Texture.Width - origin.X) * scale;
					if (right > (float)worldClip.Right)
					{
						int amount2 = (int)((right - (float)worldClip.Right) / scale);
						clip.Width -= amount2;
					}
					float bottom = target.Y + ((float)Texture.Height - origin.Y) * scale;
					if (bottom > (float)worldClip.Bottom)
					{
						int amount = (int)((bottom - (float)worldClip.Bottom) / scale);
						clip.Height -= amount;
					}
				}
				Texture.Draw(target, origin, Color.White * alpha, Vector2.One * scale, 0f, clip);
			}
		}

		public bool Enabled = true;

		private float timer;

		private List<Burst> points = new List<Burst>();

		public bool HasDisplacement(Scene scene)
		{
			if (points.Count <= 0 && scene.Tracker.GetComponent<DisplacementRenderHook>() == null)
			{
				return (scene as Level).Foreground.Get<HeatWave>() != null;
			}
			return true;
		}

		public Burst Add(Burst point)
		{
			points.Add(point);
			return point;
		}

		public Burst Remove(Burst point)
		{
			points.Remove(point);
			return point;
		}

		public Burst AddBurst(Vector2 position, float duration, float radiusFrom, float radiusTo, float alpha = 1f, Ease.Easer alphaEaser = null, Ease.Easer radiusEaser = null)
		{
			MTexture tex = GFX.Game["util/displacementcircle"];
			Burst point = new Burst(tex, position, tex.Center, duration);
			point.ScaleFrom = radiusFrom / (float)(tex.Width / 2);
			point.ScaleTo = radiusTo / (float)(tex.Width / 2);
			point.AlphaFrom = alpha;
			point.AlphaTo = 0f;
			point.AlphaEaser = alphaEaser;
			return Add(point);
		}

		public override void Update(Scene scene)
		{
			timer += Engine.DeltaTime;
			for (int i = points.Count - 1; i >= 0; i--)
			{
				if (points[i].Percent >= 1f)
				{
					points.RemoveAt(i);
				}
				else
				{
					points[i].Update();
				}
			}
		}

		public void Clear()
		{
			points.Clear();
		}

		public override void BeforeRender(Scene scene)
		{
			Distort.WaterSine = timer * 16f;
			Distort.WaterCameraY = (int)Math.Floor((scene as Level).Camera.Y);
			Camera camera = (scene as Level).Camera;
			Color baseColor = new Color(0.5f, 0.5f, 0f, 1f);
			Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Displacement.Target);
			Engine.Graphics.GraphicsDevice.Clear(baseColor);
			if (!Enabled)
			{
				return;
			}
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, camera.Matrix);
			(scene as Level).Foreground.Get<HeatWave>()?.RenderDisplacement(scene as Level);
			foreach (DisplacementRenderHook hook in scene.Tracker.GetComponents<DisplacementRenderHook>())
			{
				if (hook.Visible && hook.RenderDisplacement != null)
				{
					hook.RenderDisplacement();
				}
			}
			foreach (Burst point in points)
			{
				point.Render();
			}
			foreach (Entity wall in scene.Tracker.GetEntities<FakeWall>())
			{
				Draw.Rect(wall.X, wall.Y, wall.Width, wall.Height, baseColor);
			}
			Draw.SpriteBatch.End();
		}
	}
}
