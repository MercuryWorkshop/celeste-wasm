using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class HeartGemDoor : Entity
	{
		private struct Particle
		{
			public Vector2 Position;

			public float Speed;

			public Color Color;
		}

		private class WhiteLine : Entity
		{
			private float fade = 1f;

			private int blockSize;

			public WhiteLine(Vector2 origin, int blockSize)
				: base(origin)
			{
				base.Depth = -1000000;
				this.blockSize = blockSize;
			}

			public override void Update()
			{
				base.Update();
				fade = Calc.Approach(fade, 0f, Engine.DeltaTime);
				if (!(fade <= 0f))
				{
					return;
				}
				RemoveSelf();
				Level level = SceneAs<Level>();
				for (float i = (int)level.Camera.Left; i < level.Camera.Right; i += 1f)
				{
					if (i < base.X || i >= base.X + (float)blockSize)
					{
						level.Particles.Emit(P_Slice, new Vector2(i, base.Y));
					}
				}
			}

			public override void Render()
			{
				Vector2 position = (base.Scene as Level).Camera.Position;
				float size = Math.Max(1f, 4f * fade);
				Draw.Rect(position.X - 10f, base.Y - size / 2f, 340f, size, Color.White);
			}
		}

		private const string OpenedFlag = "opened_heartgem_door_";

		public static ParticleType P_Shimmer;

		public static ParticleType P_Slice;

		public readonly int Requires;

		public int Size;

		private readonly float openDistance;

		private float openPercent;

		private Solid TopSolid;

		private Solid BotSolid;

		private float offset;

		private Vector2 mist;

		private MTexture temp = new MTexture();

		private List<MTexture> icon;

		private Particle[] particles = new Particle[50];

		private bool startHidden;

		private float heartAlpha = 1f;

		public int HeartGems
		{
			get
			{
				if (SaveData.Instance.CheatMode)
				{
					return Requires;
				}
				return SaveData.Instance.TotalHeartGems;
			}
		}

		public float Counter { get; private set; }

		public bool Opened { get; private set; }

		private float openAmount => openPercent * openDistance;

		public HeartGemDoor(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			Requires = data.Int("requires");
			Add(new CustomBloom(RenderBloom));
			Size = data.Width;
			openDistance = 32f;
			Vector2? node = data.FirstNodeNullable(offset);
			if (node.HasValue)
			{
				openDistance = Math.Abs(node.Value.Y - base.Y);
			}
			icon = GFX.Game.GetAtlasSubtextures("objects/heartdoor/icon");
			startHidden = data.Bool("startHidden");
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Level level = scene as Level;
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Position = new Vector2(Calc.Random.NextFloat(Size), Calc.Random.NextFloat(level.Bounds.Height));
				particles[i].Speed = Calc.Random.Range(4, 12);
				particles[i].Color = Color.White * Calc.Random.Range(0.2f, 0.6f);
			}
			level.Add(TopSolid = new Solid(new Vector2(base.X, level.Bounds.Top - 32), Size, base.Y - (float)level.Bounds.Top + 32f, safe: true));
			TopSolid.SurfaceSoundIndex = 32;
			TopSolid.SquishEvenInAssistMode = true;
			TopSolid.EnableAssistModeChecks = false;
			level.Add(BotSolid = new Solid(new Vector2(base.X, base.Y), Size, (float)level.Bounds.Bottom - base.Y + 32f, safe: true));
			BotSolid.SurfaceSoundIndex = 32;
			BotSolid.SquishEvenInAssistMode = true;
			BotSolid.EnableAssistModeChecks = false;
			if ((base.Scene as Level).Session.GetFlag("opened_heartgem_door_" + Requires))
			{
				Opened = true;
				Visible = true;
				openPercent = 1f;
				Counter = Requires;
				TopSolid.Y -= openDistance;
				BotSolid.Y += openDistance;
			}
			else
			{
				Add(new Coroutine(Routine()));
			}
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (Opened)
			{
				base.Scene.CollideFirst<DashBlock>(BotSolid.Collider.Bounds)?.RemoveSelf();
			}
			else if (startHidden)
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null && player.X > base.X)
				{
					startHidden = false;
					base.Scene.CollideFirst<DashBlock>(BotSolid.Collider.Bounds)?.RemoveSelf();
				}
				else
				{
					Visible = false;
				}
			}
		}

		private IEnumerator Routine()
		{
			Level level = base.Scene as Level;
			float botFrom2;
			float topFrom2;
			float botTo2;
			float topTo2;
			if (startHidden)
			{
				Player player;
				do
				{
					yield return null;
					player = base.Scene.Tracker.GetEntity<Player>();
				}
				while (player == null || !(Math.Abs(player.X - base.Center.X) < 100f));
				Audio.Play("event:/new_content/game/10_farewell/heart_door", Position);
				Visible = true;
				heartAlpha = 0f;
				topTo2 = TopSolid.Y;
				botTo2 = BotSolid.Y;
				topFrom2 = (TopSolid.Y -= 240f);
				botFrom2 = (BotSolid.Y -= 240f);
				for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime * 1.2f)
				{
					float ease = Ease.CubeIn(p2);
					TopSolid.MoveToY(topFrom2 + (topTo2 - topFrom2) * ease);
					BotSolid.MoveToY(botFrom2 + (botTo2 - botFrom2) * ease);
					DashBlock block = base.Scene.CollideFirst<DashBlock>(BotSolid.Collider.Bounds);
					if (block != null)
					{
						level.Shake(0.5f);
						Celeste.Freeze(0.1f);
						Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
						block.Break(BotSolid.BottomCenter, new Vector2(0f, 1f), playSound: true, playDebrisSound: false);
						Player player3 = base.Scene.Tracker.GetEntity<Player>();
						if (player3 != null && Math.Abs(player3.X - base.Center.X) < 40f)
						{
							player3.PointBounce(player3.Position + Vector2.UnitX * 8f);
						}
					}
					yield return null;
				}
				level.Shake(0.5f);
				Celeste.Freeze(0.1f);
				TopSolid.Y = topTo2;
				BotSolid.Y = botTo2;
				while (heartAlpha < 1f)
				{
					heartAlpha = Calc.Approach(heartAlpha, 1f, Engine.DeltaTime * 2f);
					yield return null;
				}
				yield return 0.6f;
			}
			while (!Opened && Counter < (float)Requires)
			{
				Player player2 = base.Scene.Tracker.GetEntity<Player>();
				if (player2 != null && Math.Abs(player2.X - base.Center.X) < 80f && player2.X < base.X)
				{
					if (Counter == 0f && HeartGems > 0)
					{
						Audio.Play("event:/game/09_core/frontdoor_heartfill", Position);
					}
					if (HeartGems < Requires)
					{
						level.Session.SetFlag("granny_door");
					}
					int num = (int)Counter;
					int target = Math.Min(HeartGems, Requires);
					Counter = Calc.Approach(Counter, target, Engine.DeltaTime * (float)Requires * 0.8f);
					if (num != (int)Counter)
					{
						yield return 0.1f;
						if (Counter < (float)target)
						{
							Audio.Play("event:/game/09_core/frontdoor_heartfill", Position);
						}
					}
				}
				else
				{
					Counter = Calc.Approach(Counter, 0f, Engine.DeltaTime * (float)Requires * 4f);
				}
				yield return null;
			}
			yield return 0.5f;
			base.Scene.Add(new WhiteLine(Position, Size));
			level.Shake();
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
			level.Flash(Color.White * 0.5f);
			Audio.Play("event:/game/09_core/frontdoor_unlock", Position);
			Opened = true;
			level.Session.SetFlag("opened_heartgem_door_" + Requires);
			offset = 0f;
			yield return 0.6f;
			botFrom2 = TopSolid.Y;
			topFrom2 = TopSolid.Y - openDistance;
			botTo2 = BotSolid.Y;
			topTo2 = BotSolid.Y + openDistance;
			for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime)
			{
				level.Shake();
				openPercent = Ease.CubeIn(p2);
				TopSolid.MoveToY(MathHelper.Lerp(botFrom2, topFrom2, openPercent));
				BotSolid.MoveToY(MathHelper.Lerp(botTo2, topTo2, openPercent));
				if (p2 >= 0.4f && level.OnInterval(0.1f))
				{
					for (int i = 4; i < Size; i += 4)
					{
						level.ParticlesBG.Emit(P_Shimmer, 1, new Vector2(TopSolid.Left + (float)i + 1f, TopSolid.Bottom - 2f), new Vector2(2f, 2f), -(float)Math.PI / 2f);
						level.ParticlesBG.Emit(P_Shimmer, 1, new Vector2(BotSolid.Left + (float)i + 1f, BotSolid.Top + 2f), new Vector2(2f, 2f), (float)Math.PI / 2f);
					}
				}
				yield return null;
			}
			TopSolid.MoveToY(topFrom2);
			BotSolid.MoveToY(topTo2);
			openPercent = 1f;
		}

		public override void Update()
		{
			base.Update();
			if (!Opened)
			{
				offset += 12f * Engine.DeltaTime;
				mist.X -= 4f * Engine.DeltaTime;
				mist.Y -= 24f * Engine.DeltaTime;
				for (int i = 0; i < particles.Length; i++)
				{
					particles[i].Position.Y += particles[i].Speed * Engine.DeltaTime;
				}
			}
		}

		public void RenderBloom()
		{
			if (!Opened && Visible)
			{
				DrawBloom(new Rectangle((int)TopSolid.X, (int)TopSolid.Y, Size, (int)(TopSolid.Height + BotSolid.Height)));
			}
		}

		private void DrawBloom(Rectangle bounds)
		{
			Draw.Rect(bounds.Left - 4, bounds.Top, 2f, bounds.Height, Color.White * 0.25f);
			Draw.Rect(bounds.Left - 2, bounds.Top, 2f, bounds.Height, Color.White * 0.5f);
			Draw.Rect(bounds, Color.White * 0.75f);
			Draw.Rect(bounds.Right, bounds.Top, 2f, bounds.Height, Color.White * 0.5f);
			Draw.Rect(bounds.Right + 2, bounds.Top, 2f, bounds.Height, Color.White * 0.25f);
		}

		private void DrawMist(Rectangle bounds, Vector2 mist)
		{
			Color color = Color.White * 0.6f;
			MTexture tex = GFX.Game["objects/heartdoor/mist"];
			int hw = tex.Width / 2;
			int hh = tex.Height / 2;
			for (int tx = 0; tx < bounds.Width; tx += hw)
			{
				for (int ty = 0; ty < bounds.Height; ty += hh)
				{
					tex.GetSubtexture((int)Mod(mist.X, hw), (int)Mod(mist.Y, hh), Math.Min(hw, bounds.Width - tx), Math.Min(hh, bounds.Height - ty), temp);
					temp.Draw(new Vector2(bounds.X + tx, bounds.Y + ty), Vector2.Zero, color);
				}
			}
		}

		private void DrawInterior(Rectangle bounds)
		{
			Draw.Rect(bounds, Calc.HexToColor("18668f"));
			DrawMist(bounds, mist);
			DrawMist(bounds, new Vector2(mist.Y, mist.X) * 1.5f);
			Vector2 cam = (base.Scene as Level).Camera.Position;
			if (Opened)
			{
				cam = Vector2.Zero;
			}
			for (int i = 0; i < particles.Length; i++)
			{
				Vector2 pos = particles[i].Position + cam * 0.2f;
				pos.X = Mod(pos.X, bounds.Width);
				pos.Y = Mod(pos.Y, bounds.Height);
				Draw.Pixel.Draw(new Vector2(bounds.X, bounds.Y) + pos, Vector2.Zero, particles[i].Color);
			}
		}

		private void DrawEdges(Rectangle bounds, Color color)
		{
			MTexture edge = GFX.Game["objects/heartdoor/edge"];
			MTexture top = GFX.Game["objects/heartdoor/top"];
			int slide = (int)(offset % 8f);
			if (slide > 0)
			{
				edge.GetSubtexture(0, 8 - slide, 7, slide, temp);
				temp.DrawJustified(new Vector2(bounds.Left + 4, bounds.Top), new Vector2(0.5f, 0f), color, new Vector2(-1f, 1f));
				temp.DrawJustified(new Vector2(bounds.Right - 4, bounds.Top), new Vector2(0.5f, 0f), color, new Vector2(1f, 1f));
			}
			for (int y = slide; y < bounds.Height; y += 8)
			{
				edge.GetSubtexture(0, 0, 8, Math.Min(8, bounds.Height - y), temp);
				temp.DrawJustified(new Vector2(bounds.Left + 4, bounds.Top + y), new Vector2(0.5f, 0f), color, new Vector2(-1f, 1f));
				temp.DrawJustified(new Vector2(bounds.Right - 4, bounds.Top + y), new Vector2(0.5f, 0f), color, new Vector2(1f, 1f));
			}
			for (int x = 0; x < bounds.Width; x += 8)
			{
				top.DrawCentered(new Vector2(bounds.Left + 4 + x, bounds.Top + 4), color);
				top.DrawCentered(new Vector2(bounds.Left + 4 + x, bounds.Bottom - 4), color, new Vector2(1f, -1f));
			}
		}

		public override void Render()
		{
			Color color = (Opened ? (Color.White * 0.25f) : Color.White);
			if (!Opened && TopSolid.Visible && BotSolid.Visible)
			{
				Rectangle closedRect = new Rectangle((int)TopSolid.X, (int)TopSolid.Y, Size, (int)(TopSolid.Height + BotSolid.Height));
				DrawInterior(closedRect);
				DrawEdges(closedRect, color);
			}
			else
			{
				if (TopSolid.Visible)
				{
					Rectangle topRect = new Rectangle((int)TopSolid.X, (int)TopSolid.Y, Size, (int)TopSolid.Height);
					DrawInterior(topRect);
					DrawEdges(topRect, color);
				}
				if (BotSolid.Visible)
				{
					Rectangle botRect = new Rectangle((int)BotSolid.X, (int)BotSolid.Y, Size, (int)BotSolid.Height);
					DrawInterior(botRect);
					DrawEdges(botRect, color);
				}
			}
			if (!(heartAlpha > 0f))
			{
				return;
			}
			float size = 12f;
			int columns = (int)((float)(Size - 8) / size);
			int rows = (int)Math.Ceiling((float)Requires / (float)columns);
			Color iconColor = color * heartAlpha;
			for (int y = 0; y < rows; y++)
			{
				int inRow = (((y + 1) * columns < Requires) ? columns : (Requires - y * columns));
				Vector2 pos = new Vector2(base.X + (float)Size * 0.5f, base.Y) + new Vector2((float)(-inRow) / 2f + 0.5f, (float)(-rows) / 2f + (float)y + 0.5f) * size;
				if (Opened)
				{
					if (y < rows / 2)
					{
						pos.Y -= openAmount + 8f;
					}
					else
					{
						pos.Y += openAmount + 8f;
					}
				}
				for (int x = 0; x < inRow; x++)
				{
					int index = y * columns + x;
					float ease = Ease.CubeIn(Calc.ClampedMap(Counter, index, (float)index + 1f));
					icon[(int)(ease * (float)(icon.Count - 1))].DrawCentered(pos + new Vector2((float)x * size, 0f), iconColor);
				}
			}
		}

		private float Mod(float x, float m)
		{
			return (x % m + m) % m;
		}
	}
}
