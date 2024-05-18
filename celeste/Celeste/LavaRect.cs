using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class LavaRect : Component
	{
		public enum OnlyModes
		{
			None,
			OnlyTop,
			OnlyBottom
		}

		private struct Bubble
		{
			public Vector2 Position;

			public float Speed;

			public float Alpha;
		}

		private struct SurfaceBubble
		{
			public float X;

			public float Frame;

			public byte Animation;
		}

		public Vector2 Position;

		public float Fade = 16f;

		public float Spikey;

		public OnlyModes OnlyMode;

		public float SmallWaveAmplitude = 1f;

		public float BigWaveAmplitude = 4f;

		public float CurveAmplitude = 12f;

		public float UpdateMultiplier = 1f;

		public Color SurfaceColor = Color.White;

		public Color EdgeColor = Color.LightGray;

		public Color CenterColor = Color.DarkGray;

		private float timer = Calc.Random.NextFloat(100f);

		private VertexPositionColor[] verts;

		private bool dirty;

		private int vertCount;

		private Bubble[] bubbles;

		private SurfaceBubble[] surfaceBubbles;

		private int surfaceBubbleIndex;

		private List<List<MTexture>> surfaceBubbleAnimations;

		public int SurfaceStep { get; private set; }

		public float Width { get; private set; }

		public float Height { get; private set; }

		public LavaRect(float width, float height, int step)
			: base(active: true, visible: true)
		{
			Resize(width, height, step);
		}

		public void Resize(float width, float height, int step)
		{
			Width = width;
			Height = height;
			SurfaceStep = step;
			dirty = true;
			int steps = (int)(width / (float)SurfaceStep * 2f + height / (float)SurfaceStep * 2f + 4f);
			verts = new VertexPositionColor[steps * 3 * 6 + 6];
			bubbles = new Bubble[(int)(width * height * 0.005f)];
			surfaceBubbles = new SurfaceBubble[(int)Math.Max(4f, (float)bubbles.Length * 0.25f)];
			for (int j = 0; j < bubbles.Length; j++)
			{
				bubbles[j].Position = new Vector2(1f + Calc.Random.NextFloat(Width - 2f), Calc.Random.NextFloat(Height));
				bubbles[j].Speed = Calc.Random.Range(4, 12);
				bubbles[j].Alpha = Calc.Random.Range(0.4f, 0.8f);
			}
			for (int i = 0; i < surfaceBubbles.Length; i++)
			{
				surfaceBubbles[i].X = -1f;
			}
			surfaceBubbleAnimations = new List<List<MTexture>>();
			surfaceBubbleAnimations.Add(GFX.Game.GetAtlasSubtextures("danger/lava/bubble_a"));
		}

		public override void Update()
		{
			timer += UpdateMultiplier * Engine.DeltaTime;
			if (UpdateMultiplier != 0f)
			{
				dirty = true;
			}
			for (int j = 0; j < bubbles.Length; j++)
			{
				bubbles[j].Position.Y -= UpdateMultiplier * bubbles[j].Speed * Engine.DeltaTime;
				if (bubbles[j].Position.Y < 2f - Wave((int)(bubbles[j].Position.X / (float)SurfaceStep), Width))
				{
					bubbles[j].Position.Y = Height - 1f;
					if (Calc.Random.Chance(0.75f))
					{
						surfaceBubbles[surfaceBubbleIndex].X = bubbles[j].Position.X;
						surfaceBubbles[surfaceBubbleIndex].Frame = 0f;
						surfaceBubbles[surfaceBubbleIndex].Animation = (byte)Calc.Random.Next(surfaceBubbleAnimations.Count);
						surfaceBubbleIndex = (surfaceBubbleIndex + 1) % surfaceBubbles.Length;
					}
				}
			}
			for (int i = 0; i < surfaceBubbles.Length; i++)
			{
				if (surfaceBubbles[i].X >= 0f)
				{
					surfaceBubbles[i].Frame += Engine.DeltaTime * 6f;
					if (surfaceBubbles[i].Frame >= (float)surfaceBubbleAnimations[surfaceBubbles[i].Animation].Count)
					{
						surfaceBubbles[i].X = -1f;
					}
				}
			}
			base.Update();
		}

		private float Sin(float value)
		{
			return (1f + (float)Math.Sin(value)) / 2f;
		}

		private float Wave(int step, float length)
		{
			int pos = step * SurfaceStep;
			float easeOut = ((OnlyMode != 0) ? 1f : (Calc.ClampedMap(pos, 0f, length * 0.1f) * Calc.ClampedMap(pos, length * 0.9f, length, 1f, 0f)));
			float wave = Sin((float)pos * 0.25f + timer * 4f) * SmallWaveAmplitude;
			wave += Sin((float)pos * 0.05f + timer * 0.5f) * BigWaveAmplitude;
			if (step % 2 == 0)
			{
				wave += Spikey;
			}
			if (OnlyMode != 0)
			{
				wave += (1f - Calc.YoYo((float)pos / length)) * CurveAmplitude;
			}
			return wave * easeOut;
		}

		private void Quad(ref int vert, Vector2 va, Vector2 vb, Vector2 vc, Vector2 vd, Color color)
		{
			Quad(ref vert, va, color, vb, color, vc, color, vd, color);
		}

		private void Quad(ref int vert, Vector2 va, Color ca, Vector2 vb, Color cb, Vector2 vc, Color cc, Vector2 vd, Color cd)
		{
			verts[vert].Position.X = va.X;
			verts[vert].Position.Y = va.Y;
			verts[vert++].Color = ca;
			verts[vert].Position.X = vb.X;
			verts[vert].Position.Y = vb.Y;
			verts[vert++].Color = cb;
			verts[vert].Position.X = vc.X;
			verts[vert].Position.Y = vc.Y;
			verts[vert++].Color = cc;
			verts[vert].Position.X = va.X;
			verts[vert].Position.Y = va.Y;
			verts[vert++].Color = ca;
			verts[vert].Position.X = vc.X;
			verts[vert].Position.Y = vc.Y;
			verts[vert++].Color = cc;
			verts[vert].Position.X = vd.X;
			verts[vert].Position.Y = vd.Y;
			verts[vert++].Color = cd;
		}

		private void Edge(ref int vert, Vector2 a, Vector2 b, float fade, float insetFade)
		{
			float length = (a - b).Length();
			float insetPercent = ((OnlyMode == OnlyModes.None) ? (insetFade / length) : 0f);
			float steps = length / (float)SurfaceStep;
			Vector2 perp = (b - a).SafeNormalize().Perpendicular();
			for (int i = 1; (float)i <= steps; i++)
			{
				Vector2 vector = Vector2.Lerp(a, b, (float)(i - 1) / steps);
				float lastWaveAmount = Wave(i - 1, length);
				Vector2 lastWave = vector - perp * lastWaveAmount;
				Vector2 vector2 = Vector2.Lerp(a, b, (float)i / steps);
				float nextWaveAmount = Wave(i, length);
				Vector2 nextWave = vector2 - perp * nextWaveAmount;
				Vector2 lastInset = Vector2.Lerp(a, b, Calc.ClampedMap((float)(i - 1) / steps, 0f, 1f, insetPercent, 1f - insetPercent));
				Vector2 nextInset = Vector2.Lerp(a, b, Calc.ClampedMap((float)i / steps, 0f, 1f, insetPercent, 1f - insetPercent));
				Quad(ref vert, lastWave + perp, EdgeColor, nextWave + perp, EdgeColor, nextInset + perp * (fade - nextWaveAmount), CenterColor, lastInset + perp * (fade - lastWaveAmount), CenterColor);
				Quad(ref vert, lastInset + perp * (fade - lastWaveAmount), nextInset + perp * (fade - nextWaveAmount), nextInset + perp * fade, lastInset + perp * fade, CenterColor);
				Quad(ref vert, lastWave, nextWave, nextWave + perp * 1f, lastWave + perp * 1f, SurfaceColor);
			}
		}

		public override void Render()
		{
			GameplayRenderer.End();
			if (dirty)
			{
				Vector2 pos = Vector2.Zero;
				Vector2 topleft = pos;
				Vector2 topright = new Vector2(pos.X + Width, pos.Y);
				Vector2 botleft = new Vector2(pos.X, pos.Y + Height);
				Vector2 botright = pos + new Vector2(Width, Height);
				Vector2 fade = new Vector2(Math.Min(Fade, Width / 2f), Math.Min(Fade, Height / 2f));
				vertCount = 0;
				if (OnlyMode == OnlyModes.None)
				{
					Edge(ref vertCount, topleft, topright, fade.Y, fade.X);
					Edge(ref vertCount, topright, botright, fade.X, fade.Y);
					Edge(ref vertCount, botright, botleft, fade.Y, fade.X);
					Edge(ref vertCount, botleft, topleft, fade.X, fade.Y);
					Quad(ref vertCount, topleft + fade, topright + new Vector2(0f - fade.X, fade.Y), botright - fade, botleft + new Vector2(fade.X, 0f - fade.Y), CenterColor);
				}
				else if (OnlyMode == OnlyModes.OnlyTop)
				{
					Edge(ref vertCount, topleft, topright, fade.Y, 0f);
					Quad(ref vertCount, topleft + new Vector2(0f, fade.Y), topright + new Vector2(0f, fade.Y), botright, botleft, CenterColor);
				}
				else if (OnlyMode == OnlyModes.OnlyBottom)
				{
					Edge(ref vertCount, botright, botleft, fade.Y, 0f);
					Quad(ref vertCount, topleft, topright, botright + new Vector2(0f, 0f - fade.Y), botleft + new Vector2(0f, 0f - fade.Y), CenterColor);
				}
				dirty = false;
			}
			Camera cam = (base.Scene as Level).Camera;
			GFX.DrawVertices(Matrix.CreateTranslation(new Vector3(base.Entity.Position + Position, 0f)) * cam.Matrix, verts, vertCount);
			GameplayRenderer.Begin();
			Vector2 offset = base.Entity.Position + Position;
			MTexture tex = GFX.Game["particles/bubble"];
			for (int j = 0; j < bubbles.Length; j++)
			{
				tex.DrawCentered(offset + bubbles[j].Position, SurfaceColor * bubbles[j].Alpha);
			}
			for (int i = 0; i < surfaceBubbles.Length; i++)
			{
				if (surfaceBubbles[i].X >= 0f)
				{
					MTexture mTexture = surfaceBubbleAnimations[surfaceBubbles[i].Animation][(int)surfaceBubbles[i].Frame];
					int x = (int)(surfaceBubbles[i].X / (float)SurfaceStep);
					mTexture.DrawJustified(offset + new Vector2(y: 1f - Wave(x, Width), x: x * SurfaceStep), new Vector2(0.5f, 1f), SurfaceColor);
				}
			}
		}
	}
}
