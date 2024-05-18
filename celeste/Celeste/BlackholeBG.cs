using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class BlackholeBG : Backdrop
	{
		public enum Strengths
		{
			Mild,
			Medium,
			High,
			Wild
		}

		private struct StreamParticle
		{
			public int Color;

			public MTexture Texture;

			public float Percent;

			public float Speed;

			public Vector2 Normal;
		}

		private struct Particle
		{
			public int Color;

			public Vector2 Normal;

			public float Percent;
		}

		private struct SpiralDebris
		{
			public int Color;

			public float Percent;

			public float Offset;
		}

		private const string STRENGTH_FLAG = "blackhole_strength";

		private const int BG_STEPS = 20;

		private const int STREAM_MIN_COUNT = 30;

		private const int STREAM_MAX_COUNT = 50;

		private const int PARTICLE_MIN_COUNT = 150;

		private const int PARTICLE_MAX_COUNT = 220;

		private const int SPIRAL_MIN_COUNT = 0;

		private const int SPIRAL_MAX_COUNT = 10;

		private const int SPIRAL_SEGMENTS = 12;

		private Color[] colorsMild = new Color[3]
		{
			Calc.HexToColor("6e3199") * 0.8f,
			Calc.HexToColor("851f91") * 0.8f,
			Calc.HexToColor("3026b0") * 0.8f
		};

		private Color[] colorsWild = new Color[3]
		{
			Calc.HexToColor("ca4ca7"),
			Calc.HexToColor("b14cca"),
			Calc.HexToColor("ca4ca7")
		};

		private Color[] colorsLerp;

		private Color[,] colorsLerpBlack;

		private Color[,] colorsLerpTransparent;

		private const int colorSteps = 20;

		public float Alpha = 1f;

		public float Scale = 1f;

		public float Direction = 1f;

		public float StrengthMultiplier = 1f;

		public Vector2 CenterOffset;

		public Vector2 OffsetOffset;

		private Strengths strength;

		private readonly Color bgColorInner = Calc.HexToColor("000000");

		private readonly Color bgColorOuterMild = Calc.HexToColor("512a8b");

		private readonly Color bgColorOuterWild = Calc.HexToColor("bd2192");

		private readonly MTexture bgTexture;

		private StreamParticle[] streams = new StreamParticle[50];

		private VertexPositionColorTexture[] streamVerts = new VertexPositionColorTexture[300];

		private Particle[] particles = new Particle[220];

		private SpiralDebris[] spirals = new SpiralDebris[10];

		private VertexPositionColorTexture[] spiralVerts = new VertexPositionColorTexture[720];

		private VirtualRenderTarget buffer;

		private Vector2 center;

		private Vector2 offset;

		private Vector2 shake;

		private float spinTime;

		private bool checkedFlag;

		public int StreamCount => (int)MathHelper.Lerp(30f, 50f, (StrengthMultiplier - 1f) / 3f);

		public int ParticleCount => (int)MathHelper.Lerp(150f, 220f, (StrengthMultiplier - 1f) / 3f);

		public int SpiralCount => (int)MathHelper.Lerp(0f, 10f, (StrengthMultiplier - 1f) / 3f);

		public BlackholeBG()
		{
			bgTexture = GFX.Game["objects/temple/portal/portal"];
			List<MTexture> stars = GFX.Game.GetAtlasSubtextures("bgs/10/blackhole/particle");
			int v2 = 0;
			for (int p3 = 0; p3 < 50; p3++)
			{
				MTexture tex = (streams[p3].Texture = Calc.Random.Choose(stars));
				streams[p3].Percent = Calc.Random.NextFloat();
				streams[p3].Speed = Calc.Random.Range(0.2f, 0.4f);
				streams[p3].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * ((float)Math.PI * 2f), 1f);
				streams[p3].Color = Calc.Random.Next(colorsMild.Length);
				streamVerts[v2].TextureCoordinate = new Vector2(tex.LeftUV, tex.TopUV);
				streamVerts[v2 + 1].TextureCoordinate = new Vector2(tex.RightUV, tex.TopUV);
				streamVerts[v2 + 2].TextureCoordinate = new Vector2(tex.RightUV, tex.BottomUV);
				streamVerts[v2 + 3].TextureCoordinate = new Vector2(tex.LeftUV, tex.TopUV);
				streamVerts[v2 + 4].TextureCoordinate = new Vector2(tex.RightUV, tex.BottomUV);
				streamVerts[v2 + 5].TextureCoordinate = new Vector2(tex.LeftUV, tex.BottomUV);
				v2 += 6;
			}
			int v = 0;
			for (int p2 = 0; p2 < 10; p2++)
			{
				MTexture tex2 = (streams[p2].Texture = Calc.Random.Choose(stars));
				spirals[p2].Percent = Calc.Random.NextFloat();
				spirals[p2].Offset = Calc.Random.NextFloat((float)Math.PI * 2f);
				spirals[p2].Color = Calc.Random.Next(colorsMild.Length);
				for (int t = 0; t < 12; t++)
				{
					float left = MathHelper.Lerp(tex2.LeftUV, tex2.RightUV, (float)t / 12f);
					float right = MathHelper.Lerp(tex2.LeftUV, tex2.RightUV, (float)(t + 1) / 12f);
					spiralVerts[v].TextureCoordinate = new Vector2(left, tex2.TopUV);
					spiralVerts[v + 1].TextureCoordinate = new Vector2(right, tex2.TopUV);
					spiralVerts[v + 2].TextureCoordinate = new Vector2(right, tex2.BottomUV);
					spiralVerts[v + 3].TextureCoordinate = new Vector2(left, tex2.TopUV);
					spiralVerts[v + 4].TextureCoordinate = new Vector2(right, tex2.BottomUV);
					spiralVerts[v + 5].TextureCoordinate = new Vector2(left, tex2.BottomUV);
					v += 6;
				}
			}
			for (int p = 0; p < 220; p++)
			{
				particles[p].Percent = Calc.Random.NextFloat();
				particles[p].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * ((float)Math.PI * 2f), 1f);
				particles[p].Color = Calc.Random.Next(colorsMild.Length);
			}
			center = new Vector2(320f, 180f) / 2f;
			offset = Vector2.Zero;
			colorsLerp = new Color[colorsMild.Length];
			colorsLerpBlack = new Color[colorsMild.Length, 20];
			colorsLerpTransparent = new Color[colorsMild.Length, 20];
		}

		public void SnapStrength(Level level, Strengths strength)
		{
			this.strength = strength;
			StrengthMultiplier = 1f + (float)strength;
			level.Session.SetCounter("blackhole_strength", (int)strength);
		}

		public void NextStrength(Level level, Strengths strength)
		{
			this.strength = strength;
			level.Session.SetCounter("blackhole_strength", (int)strength);
		}

		public override void Update(Scene scene)
		{
			base.Update(scene);
			if (!checkedFlag)
			{
				int strength = (scene as Level).Session.GetCounter("blackhole_strength");
				if (strength >= 0)
				{
					SnapStrength(scene as Level, (Strengths)strength);
				}
				checkedFlag = true;
			}
			if (!Visible)
			{
				return;
			}
			StrengthMultiplier = Calc.Approach(StrengthMultiplier, 1f + (float)this.strength, Engine.DeltaTime * 0.1f);
			if (scene.OnInterval(0.05f))
			{
				for (int i = 0; i < colorsMild.Length; i++)
				{
					colorsLerp[i] = Color.Lerp(colorsMild[i], colorsWild[i], (StrengthMultiplier - 1f) / 3f);
					for (int j = 0; j < 20; j++)
					{
						colorsLerpBlack[i, j] = Color.Lerp(colorsLerp[i], Color.Black, (float)j / 19f) * FadeAlphaMultiplier;
						colorsLerpTransparent[i, j] = Color.Lerp(colorsLerp[i], Color.Transparent, (float)j / 19f) * FadeAlphaMultiplier;
					}
				}
			}
			float streamMultiplier = 1f + (StrengthMultiplier - 1f) * 0.7f;
			int streamCount = StreamCount;
			int v2 = 0;
			for (int p3 = 0; p3 < streamCount; p3++)
			{
				streams[p3].Percent += streams[p3].Speed * Engine.DeltaTime * streamMultiplier * Direction;
				if (streams[p3].Percent >= 1f && Direction > 0f)
				{
					streams[p3].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * ((float)Math.PI * 2f), 1f);
					streams[p3].Percent -= 1f;
				}
				else if (streams[p3].Percent < 0f && Direction < 0f)
				{
					streams[p3].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * ((float)Math.PI * 2f), 1f);
					streams[p3].Percent += 1f;
				}
				float percent = streams[p3].Percent;
				float innerPercent = Ease.CubeIn(Calc.ClampedMap(percent, 0f, 0.8f));
				float outerPercent = Ease.CubeIn(Calc.ClampedMap(percent, 0.2f, 1f));
				Vector2 normal = streams[p3].Normal;
				Vector2 perp = normal.Perpendicular();
				Vector2 inner = normal * 16f + normal * (1f - innerPercent) * 200f;
				float innerSize = (1f - innerPercent) * 8f;
				Color innerColor = colorsLerpBlack[streams[p3].Color, (int)(innerPercent * 0.6f * 19f)];
				Vector2 vector = normal * 16f + normal * (1f - outerPercent) * 280f;
				float outerSize = (1f - outerPercent) * 8f;
				Color outerColor = colorsLerpBlack[streams[p3].Color, (int)(outerPercent * 0.6f * 19f)];
				Vector2 a = inner - perp * innerSize;
				Vector2 b = inner + perp * innerSize;
				Vector2 c = vector + perp * outerSize;
				Vector2 d = vector - perp * outerSize;
				AssignVertColors(streamVerts, v2, ref innerColor, ref innerColor, ref outerColor, ref outerColor);
				AssignVertPosition(streamVerts, v2, ref a, ref b, ref c, ref d);
				v2 += 6;
			}
			float particleMultiplier = StrengthMultiplier * 0.25f;
			int particleCount = ParticleCount;
			for (int p2 = 0; p2 < particleCount; p2++)
			{
				particles[p2].Percent += Engine.DeltaTime * particleMultiplier * Direction;
				if (particles[p2].Percent >= 1f && Direction > 0f)
				{
					particles[p2].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * ((float)Math.PI * 2f), 1f);
					particles[p2].Percent -= 1f;
				}
				else if (particles[p2].Percent < 0f && Direction < 0f)
				{
					particles[p2].Normal = Calc.AngleToVector(Calc.Random.NextFloat() * ((float)Math.PI * 2f), 1f);
					particles[p2].Percent += 1f;
				}
			}
			float spiralMultiplier = 0.2f + (StrengthMultiplier - 1f) * 0.1f;
			int spiralCount = SpiralCount;
			Color color = Color.Lerp(Color.Lerp(bgColorOuterMild, bgColorOuterWild, (StrengthMultiplier - 1f) / 3f), Color.White, 0.1f) * 0.8f;
			int v = 0;
			for (int p = 0; p < spiralCount; p++)
			{
				spirals[p].Percent += streams[p].Speed * Engine.DeltaTime * spiralMultiplier * Direction;
				if (spirals[p].Percent >= 1f && Direction > 0f)
				{
					spirals[p].Offset = Calc.Random.NextFloat((float)Math.PI * 2f);
					spirals[p].Percent -= 1f;
				}
				else if (spirals[p].Percent < 0f && Direction < 0f)
				{
					spirals[p].Offset = Calc.Random.NextFloat((float)Math.PI * 2f);
					spirals[p].Percent += 1f;
				}
				float percent2 = spirals[p].Percent;
				float offset = spirals[p].Offset;
				float lead = Calc.ClampedMap(percent2, 0f, 0.8f);
				float tail = Calc.ClampedMap(percent2, 0f, 1f);
				for (int t = 0; t < 12; t++)
				{
					float per0 = 1f - MathHelper.Lerp(lead, tail, (float)t / 12f);
					float per1 = 1f - MathHelper.Lerp(lead, tail, (float)(t + 1) / 12f);
					Vector2 rot0 = Calc.AngleToVector(per0 * (20f + (float)t * 0.2f) + offset, 1f);
					Vector2 vector2 = rot0 * per0 * 200f;
					float siz0 = per0 * (4f + StrengthMultiplier * 4f);
					Vector2 rot1 = Calc.AngleToVector(per1 * (20f + (float)(t + 1) * 0.2f) + offset, 1f);
					Vector2 dis1 = rot1 * per1 * 200f;
					float siz1 = per1 * (4f + StrengthMultiplier * 4f);
					Color col0 = Color.Lerp(color, Color.Black, (1f - per0) * 0.5f);
					Color col1 = Color.Lerp(color, Color.Black, (1f - per1) * 0.5f);
					Vector2 a2 = vector2 + rot0 * siz0;
					Vector2 b2 = dis1 + rot1 * siz1;
					Vector2 c2 = dis1 - rot1 * siz1;
					Vector2 d2 = vector2 - rot0 * siz0;
					AssignVertColors(spiralVerts, v, ref col0, ref col1, ref col1, ref col0);
					AssignVertPosition(spiralVerts, v, ref a2, ref b2, ref c2, ref d2);
					v += 6;
				}
			}
			Vector2 wind = (scene as Level).Wind;
			Vector2 centerTarget = new Vector2(320f, 180f) / 2f + wind * 0.15f + CenterOffset;
			center += (centerTarget - center) * (1f - (float)Math.Pow(0.009999999776482582, Engine.DeltaTime));
			Vector2 offsetTarget = -wind * 0.25f + OffsetOffset;
			this.offset += (offsetTarget - this.offset) * (1f - (float)Math.Pow(0.009999999776482582, Engine.DeltaTime));
			if (scene.OnInterval(0.025f))
			{
				shake = Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 2f * (StrengthMultiplier - 1f));
			}
			spinTime += (2f + StrengthMultiplier) * Engine.DeltaTime;
		}

		private void AssignVertColors(VertexPositionColorTexture[] verts, int v, ref Color a, ref Color b, ref Color c, ref Color d)
		{
			verts[v].Color = a;
			verts[v + 1].Color = b;
			verts[v + 2].Color = c;
			verts[v + 3].Color = a;
			verts[v + 4].Color = c;
			verts[v + 5].Color = d;
		}

		private void AssignVertPosition(VertexPositionColorTexture[] verts, int v, ref Vector2 a, ref Vector2 b, ref Vector2 c, ref Vector2 d)
		{
			verts[v].Position = new Vector3(a, 0f);
			verts[v + 1].Position = new Vector3(b, 0f);
			verts[v + 2].Position = new Vector3(c, 0f);
			verts[v + 3].Position = new Vector3(a, 0f);
			verts[v + 4].Position = new Vector3(c, 0f);
			verts[v + 5].Position = new Vector3(d, 0f);
		}

		public override void BeforeRender(Scene scene)
		{
			if (buffer == null || buffer.IsDisposed)
			{
				buffer = VirtualContent.CreateRenderTarget("Black Hole", 320, 180);
			}
			Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
			Engine.Graphics.GraphicsDevice.Clear(bgColorInner);
			Draw.SpriteBatch.Begin();
			Color bgColorOuter = Color.Lerp(bgColorOuterMild, bgColorOuterWild, (StrengthMultiplier - 1f) / 3f);
			for (int j = 0; j < 20; j++)
			{
				float percent = (1f - spinTime % 1f) * 0.05f + (float)j / 20f;
				Color color = Color.Lerp(bgColorInner, bgColorOuter, Ease.SineOut(percent));
				float scale = Calc.ClampedMap(percent, 0f, 1f, 0.1f, 4f);
				float rotation = (float)Math.PI * 2f * percent;
				bgTexture.DrawCentered(center + offset * percent + shake * (1f - percent), color, scale, rotation);
			}
			Draw.SpriteBatch.End();
			if (SpiralCount > 0)
			{
				Engine.Instance.GraphicsDevice.Textures[0] = GFX.Game.Sources[0].Texture;
				GFX.DrawVertices(Matrix.CreateTranslation(center.X, center.Y, 0f), spiralVerts, SpiralCount * 12 * 6, GFX.FxTexture);
			}
			if (StreamCount > 0)
			{
				Engine.Instance.GraphicsDevice.Textures[0] = GFX.Game.Sources[0].Texture;
				GFX.DrawVertices(Matrix.CreateTranslation(center.X, center.Y, 0f), streamVerts, StreamCount * 6, GFX.FxTexture);
			}
			Draw.SpriteBatch.Begin();
			int particleCount = ParticleCount;
			for (int i = 0; i < particleCount; i++)
			{
				float ease = Ease.CubeIn(Calc.Clamp(particles[i].Percent, 0f, 1f));
				Vector2 vector = center + particles[i].Normal * Calc.ClampedMap(ease, 1f, 0f, 8f, 220f);
				Color col = colorsLerpTransparent[particles[i].Color, (int)(ease * 19f)];
				float size = 1f + (1f - ease) * 1.5f;
				Draw.Rect(vector - new Vector2(size, size) / 2f, size, size, col);
			}
			Draw.SpriteBatch.End();
		}

		public override void Ended(Scene scene)
		{
			if (buffer != null)
			{
				buffer.Dispose();
				buffer = null;
			}
		}

		public override void Render(Scene scene)
		{
			if (buffer != null && !buffer.IsDisposed)
			{
				Vector2 center = new Vector2(buffer.Width, buffer.Height) / 2f;
				Draw.SpriteBatch.Draw((RenderTarget2D)buffer, center, buffer.Bounds, Color.White * FadeAlphaMultiplier * Alpha, 0f, center, Scale, SpriteEffects.None, 0f);
			}
		}
	}
}
