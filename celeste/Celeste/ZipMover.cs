using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class ZipMover : Solid
	{
		public enum Themes
		{
			Normal,
			Moon
		}

		private class ZipMoverPathRenderer : Entity
		{
			public ZipMover ZipMover;

			private MTexture cog;

			private Vector2 from;

			private Vector2 to;

			private Vector2 sparkAdd;

			private float sparkDirFromA;

			private float sparkDirFromB;

			private float sparkDirToA;

			private float sparkDirToB;

			public ZipMoverPathRenderer(ZipMover zipMover)
			{
				base.Depth = 5000;
				ZipMover = zipMover;
				from = ZipMover.start + new Vector2(ZipMover.Width / 2f, ZipMover.Height / 2f);
				to = ZipMover.target + new Vector2(ZipMover.Width / 2f, ZipMover.Height / 2f);
				sparkAdd = (from - to).SafeNormalize(5f).Perpendicular();
				float dir = (from - to).Angle();
				sparkDirFromA = dir + (float)Math.PI / 8f;
				sparkDirFromB = dir - (float)Math.PI / 8f;
				sparkDirToA = dir + (float)Math.PI - (float)Math.PI / 8f;
				sparkDirToB = dir + (float)Math.PI + (float)Math.PI / 8f;
				if (zipMover.theme == Themes.Moon)
				{
					cog = GFX.Game["objects/zipmover/moon/cog"];
				}
				else
				{
					cog = GFX.Game["objects/zipmover/cog"];
				}
			}

			public void CreateSparks()
			{
				SceneAs<Level>().ParticlesBG.Emit(P_Sparks, from + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromA);
				SceneAs<Level>().ParticlesBG.Emit(P_Sparks, from - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirFromB);
				SceneAs<Level>().ParticlesBG.Emit(P_Sparks, to + sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToA);
				SceneAs<Level>().ParticlesBG.Emit(P_Sparks, to - sparkAdd + Calc.Random.Range(-Vector2.One, Vector2.One), sparkDirToB);
			}

			public override void Render()
			{
				DrawCogs(Vector2.UnitY, Color.Black);
				DrawCogs(Vector2.Zero);
				if (ZipMover.drawBlackBorder)
				{
					Draw.Rect(new Rectangle((int)(ZipMover.X + ZipMover.Shake.X - 1f), (int)(ZipMover.Y + ZipMover.Shake.Y - 1f), (int)ZipMover.Width + 2, (int)ZipMover.Height + 2), Color.Black);
				}
			}

			private void DrawCogs(Vector2 offset, Color? colorOverride = null)
			{
				Vector2 normal = (to - from).SafeNormalize();
				Vector2 perpA = normal.Perpendicular() * 3f;
				Vector2 perpB = -normal.Perpendicular() * 4f;
				float rotation = ZipMover.percent * (float)Math.PI * 2f;
				Draw.Line(from + perpA + offset, to + perpA + offset, colorOverride.HasValue ? colorOverride.Value : ropeColor);
				Draw.Line(from + perpB + offset, to + perpB + offset, colorOverride.HasValue ? colorOverride.Value : ropeColor);
				for (float i = 4f - ZipMover.percent * (float)Math.PI * 8f % 4f; i < (to - from).Length(); i += 4f)
				{
					Vector2 a = from + perpA + normal.Perpendicular() + normal * i;
					Vector2 b = to + perpB - normal * i;
					Draw.Line(a + offset, a + normal * 2f + offset, colorOverride.HasValue ? colorOverride.Value : ropeLightColor);
					Draw.Line(b + offset, b - normal * 2f + offset, colorOverride.HasValue ? colorOverride.Value : ropeLightColor);
				}
				cog.DrawCentered(from + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, 1f, rotation);
				cog.DrawCentered(to + offset, colorOverride.HasValue ? colorOverride.Value : Color.White, 1f, rotation);
			}
		}

		public static ParticleType P_Scrape;

		public static ParticleType P_Sparks;

		private Themes theme;

		private MTexture[,] edges = new MTexture[3, 3];

		private Sprite streetlight;

		private BloomPoint bloom;

		private ZipMoverPathRenderer pathRenderer;

		private List<MTexture> innerCogs;

		private MTexture temp = new MTexture();

		private bool drawBlackBorder;

		private Vector2 start;

		private Vector2 target;

		private float percent;

		private static readonly Color ropeColor = Calc.HexToColor("663931");

		private static readonly Color ropeLightColor = Calc.HexToColor("9b6157");

		private SoundSource sfx = new SoundSource();

		public ZipMover(Vector2 position, int width, int height, Vector2 target, Themes theme)
			: base(position, width, height, safe: false)
		{
			base.Depth = -9999;
			start = Position;
			this.target = target;
			this.theme = theme;
			Add(new Coroutine(Sequence()));
			Add(new LightOcclude());
			string streelightPath;
			string blockPath;
			string cogPath;
			if (theme == Themes.Moon)
			{
				streelightPath = "objects/zipmover/moon/light";
				blockPath = "objects/zipmover/moon/block";
				cogPath = "objects/zipmover/moon/innercog";
				drawBlackBorder = false;
			}
			else
			{
				streelightPath = "objects/zipmover/light";
				blockPath = "objects/zipmover/block";
				cogPath = "objects/zipmover/innercog";
				drawBlackBorder = true;
			}
			innerCogs = GFX.Game.GetAtlasSubtextures(cogPath);
			Add(streetlight = new Sprite(GFX.Game, streelightPath));
			streetlight.Add("frames", "", 1f);
			streetlight.Play("frames");
			streetlight.Active = false;
			streetlight.SetAnimationFrame(1);
			streetlight.Position = new Vector2(base.Width / 2f - streetlight.Width / 2f, 0f);
			Add(bloom = new BloomPoint(1f, 6f));
			bloom.Position = new Vector2(base.Width / 2f, 4f);
			for (int tx = 0; tx < 3; tx++)
			{
				for (int ty = 0; ty < 3; ty++)
				{
					edges[tx, ty] = GFX.Game[blockPath].GetSubtexture(tx * 8, ty * 8, 8, 8);
				}
			}
			SurfaceSoundIndex = 7;
			sfx.Position = new Vector2(base.Width, base.Height) / 2f;
			Add(sfx);
		}

		public ZipMover(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset, data.Enum("theme", Themes.Normal))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			scene.Add(pathRenderer = new ZipMoverPathRenderer(this));
		}

		public override void Removed(Scene scene)
		{
			scene.Remove(pathRenderer);
			pathRenderer = null;
			base.Removed(scene);
		}

		public override void Update()
		{
			base.Update();
			bloom.Y = streetlight.CurrentAnimationFrame * 3;
		}

		public override void Render()
		{
			Vector2 old = Position;
			Position += base.Shake;
			Draw.Rect(base.X + 1f, base.Y + 1f, base.Width - 2f, base.Height - 2f, Color.Black);
			int dir = 1;
			float off = 0f;
			int total = innerCogs.Count;
			for (int y = 4; (float)y <= base.Height - 4f; y += 8)
			{
				int was = dir;
				for (int x = 4; (float)x <= base.Width - 4f; x += 8)
				{
					int frame = (int)(mod((off + (float)dir * percent * (float)Math.PI * 4f) / ((float)Math.PI / 2f), 1f) * (float)total);
					MTexture tex = innerCogs[frame];
					Rectangle subrect = new Rectangle(0, 0, tex.Width, tex.Height);
					Vector2 offset = Vector2.Zero;
					if (x <= 4)
					{
						offset.X = 2f;
						subrect.X = 2;
						subrect.Width -= 2;
					}
					else if ((float)x >= base.Width - 4f)
					{
						offset.X = -2f;
						subrect.Width -= 2;
					}
					if (y <= 4)
					{
						offset.Y = 2f;
						subrect.Y = 2;
						subrect.Height -= 2;
					}
					else if ((float)y >= base.Height - 4f)
					{
						offset.Y = -2f;
						subrect.Height -= 2;
					}
					tex = tex.GetSubtexture(subrect.X, subrect.Y, subrect.Width, subrect.Height, temp);
					tex.DrawCentered(Position + new Vector2(x, y) + offset, Color.White * ((dir < 0) ? 0.5f : 1f));
					dir = -dir;
					off += (float)Math.PI / 3f;
				}
				if (was == dir)
				{
					dir = -dir;
				}
			}
			for (int tx = 0; (float)tx < base.Width / 8f; tx++)
			{
				for (int ty = 0; (float)ty < base.Height / 8f; ty++)
				{
					int sx = ((tx != 0) ? (((float)tx != base.Width / 8f - 1f) ? 1 : 2) : 0);
					int sy = ((ty != 0) ? (((float)ty != base.Height / 8f - 1f) ? 1 : 2) : 0);
					if (sx != 1 || sy != 1)
					{
						edges[sx, sy].Draw(new Vector2(base.X + (float)(tx * 8), base.Y + (float)(ty * 8)));
					}
				}
			}
			base.Render();
			Position = old;
		}

		private void ScrapeParticlesCheck(Vector2 to)
		{
			if (!base.Scene.OnInterval(0.03f))
			{
				return;
			}
			bool v = to.Y != base.ExactPosition.Y;
			bool h = to.X != base.ExactPosition.X;
			if (v && !h)
			{
				int dir2 = Math.Sign(to.Y - base.ExactPosition.Y);
				Vector2 from2 = ((dir2 != 1) ? base.TopLeft : base.BottomLeft);
				int startY = 4;
				if (dir2 == 1)
				{
					startY = Math.Min((int)base.Height - 12, 20);
				}
				int endY = (int)base.Height;
				if (dir2 == -1)
				{
					endY = Math.Max(16, (int)base.Height - 16);
				}
				if (base.Scene.CollideCheck<Solid>(from2 + new Vector2(-2f, dir2 * -2)))
				{
					for (int l = startY; l < endY; l += 8)
					{
						SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopLeft + new Vector2(0f, (float)l + (float)dir2 * 2f), (dir2 == 1) ? (-(float)Math.PI / 4f) : ((float)Math.PI / 4f));
					}
				}
				if (base.Scene.CollideCheck<Solid>(from2 + new Vector2(base.Width + 2f, dir2 * -2)))
				{
					for (int k = startY; k < endY; k += 8)
					{
						SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopRight + new Vector2(-1f, (float)k + (float)dir2 * 2f), (dir2 == 1) ? ((float)Math.PI * -3f / 4f) : ((float)Math.PI * 3f / 4f));
					}
				}
			}
			else
			{
				if (!h || v)
				{
					return;
				}
				int dir = Math.Sign(to.X - base.ExactPosition.X);
				Vector2 from = ((dir != 1) ? base.TopLeft : base.TopRight);
				int startX = 4;
				if (dir == 1)
				{
					startX = Math.Min((int)base.Width - 12, 20);
				}
				int endX = (int)base.Width;
				if (dir == -1)
				{
					endX = Math.Max(16, (int)base.Width - 16);
				}
				if (base.Scene.CollideCheck<Solid>(from + new Vector2(dir * -2, -2f)))
				{
					for (int j = startX; j < endX; j += 8)
					{
						SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.TopLeft + new Vector2((float)j + (float)dir * 2f, -1f), (dir == 1) ? ((float)Math.PI * 3f / 4f) : ((float)Math.PI / 4f));
					}
				}
				if (base.Scene.CollideCheck<Solid>(from + new Vector2(dir * -2, base.Height + 2f)))
				{
					for (int i = startX; i < endX; i += 8)
					{
						SceneAs<Level>().ParticlesFG.Emit(P_Scrape, base.BottomLeft + new Vector2((float)i + (float)dir * 2f, 0f), (dir == 1) ? ((float)Math.PI * -3f / 4f) : (-(float)Math.PI / 4f));
					}
				}
			}
		}

		private IEnumerator Sequence()
		{
			Vector2 start = Position;
			while (true)
			{
				if (!HasPlayerRider())
				{
					yield return null;
					continue;
				}
				sfx.Play((theme == Themes.Normal) ? "event:/game/01_forsaken_city/zip_mover" : "event:/new_content/game/10_farewell/zip_mover");
				Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
				StartShaking(0.1f);
				yield return 0.1f;
				streetlight.SetAnimationFrame(3);
				StopPlayerRunIntoAnimation = false;
				float at2 = 0f;
				while (at2 < 1f)
				{
					yield return null;
					at2 = Calc.Approach(at2, 1f, 2f * Engine.DeltaTime);
					percent = Ease.SineIn(at2);
					Vector2 to = Vector2.Lerp(start, target, percent);
					ScrapeParticlesCheck(to);
					if (base.Scene.OnInterval(0.1f))
					{
						pathRenderer.CreateSparks();
					}
					MoveTo(to);
				}
				StartShaking(0.2f);
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
				SceneAs<Level>().Shake();
				StopPlayerRunIntoAnimation = true;
				yield return 0.5f;
				StopPlayerRunIntoAnimation = false;
				streetlight.SetAnimationFrame(2);
				at2 = 0f;
				while (at2 < 1f)
				{
					yield return null;
					at2 = Calc.Approach(at2, 1f, 0.5f * Engine.DeltaTime);
					percent = 1f - Ease.SineIn(at2);
					Vector2 to2 = Vector2.Lerp(target, start, Ease.SineIn(at2));
					MoveTo(to2);
				}
				StopPlayerRunIntoAnimation = true;
				StartShaking(0.2f);
				streetlight.SetAnimationFrame(1);
				yield return 0.5f;
			}
		}

		private float mod(float x, float m)
		{
			return (x % m + m) % m;
		}
	}
}
