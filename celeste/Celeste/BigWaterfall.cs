using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class BigWaterfall : Entity
	{
		private enum Layers
		{
			FG,
			BG
		}

		private Layers layer;

		private float width;

		private float height;

		private float parallax;

		private List<float> lines = new List<float>();

		private Color surfaceColor;

		private Color fillColor;

		private float sine;

		private SoundSource loopingSfx;

		private float fade;

		private Vector2 RenderPosition => RenderPositionAtCamera((base.Scene as Level).Camera.Position + new Vector2(160f, 90f));

		public BigWaterfall(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			base.Tag = Tags.TransitionUpdate;
			layer = data.Enum("layer", Layers.BG);
			width = data.Width;
			height = data.Height;
			if (layer == Layers.FG)
			{
				base.Depth = -49900;
				parallax = 0.1f + Calc.Random.NextFloat() * 0.2f;
				surfaceColor = Water.SurfaceColor;
				fillColor = Water.FillColor;
				Add(new DisplacementRenderHook(RenderDisplacement));
				lines.Add(3f);
				lines.Add(width - 4f);
				Add(loopingSfx = new SoundSource());
				loopingSfx.Play("event:/env/local/waterfall_big_main");
			}
			else
			{
				base.Depth = 10010;
				parallax = 0f - (0.7f + Calc.Random.NextFloat() * 0.2f);
				surfaceColor = Calc.HexToColor("89dbf0") * 0.5f;
				fillColor = Calc.HexToColor("29a7ea") * 0.3f;
				lines.Add(6f);
				lines.Add(width - 7f);
			}
			fade = 1f;
			Add(new TransitionListener
			{
				OnIn = delegate(float f)
				{
					fade = f;
				},
				OnOut = delegate(float f)
				{
					fade = 1f - f;
				}
			});
			if (width > 16f)
			{
				int lineCount = Calc.Random.Next((int)(width / 16f));
				for (int i = 0; i < lineCount; i++)
				{
					lines.Add(8f + Calc.Random.NextFloat(width - 16f));
				}
			}
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if ((base.Scene as Level).Transitioning)
			{
				fade = 0f;
			}
		}

		public Vector2 RenderPositionAtCamera(Vector2 camera)
		{
			Vector2 difference = Position + new Vector2(width, height) / 2f - camera;
			Vector2 offset = Vector2.Zero;
			if (layer == Layers.BG)
			{
				offset -= difference * 0.6f;
			}
			else if (layer == Layers.FG)
			{
				offset += difference * 0.2f;
			}
			return Position + offset;
		}

		public void RenderDisplacement()
		{
			Draw.Rect(RenderPosition.X, base.Y, width, height, new Color(0.5f, 0.5f, 1f, 1f));
		}

		public override void Update()
		{
			sine += Engine.DeltaTime;
			if (loopingSfx != null)
			{
				Vector2 cam = (base.Scene as Level).Camera.Position;
				loopingSfx.Position = new Vector2(RenderPosition.X - base.X, Calc.Clamp(cam.Y + 90f, base.Y, height) - base.Y);
			}
			base.Update();
		}

		public override void Render()
		{
			float x = RenderPosition.X;
			Color fill = fillColor * fade;
			Color surface = surfaceColor * fade;
			Draw.Rect(x, base.Y, width, height, fill);
			if (layer == Layers.FG)
			{
				Draw.Rect(x - 1f, base.Y, 3f, height, surface);
				Draw.Rect(x + width - 2f, base.Y, 3f, height, surface);
				{
					foreach (float line2 in lines)
					{
						Draw.Rect(x + line2, base.Y, 1f, height, surface);
					}
					return;
				}
			}
			Vector2 cam = (base.Scene as Level).Camera.Position;
			int steps = 3;
			float num = Math.Max(base.Y, (float)Math.Floor(cam.Y / (float)steps) * (float)steps);
			float bot = Math.Min(base.Y + height, cam.Y + 180f);
			for (float y = num; y < bot; y += (float)steps)
			{
				int wave = (int)(Math.Sin(y / 6f - sine * 8f) * 2.0);
				Draw.Rect(x, y, 4 + wave, steps, surface);
				Draw.Rect(x + width - 4f + (float)wave, y, 4 - wave, steps, surface);
				foreach (float line in lines)
				{
					Draw.Rect(x + (float)wave + line, y, 1f, steps, surface);
				}
			}
		}
	}
}
