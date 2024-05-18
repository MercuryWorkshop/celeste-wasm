using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class DreamBlock : Solid
	{
		private struct DreamParticle
		{
			public Vector2 Position;

			public int Layer;

			public Color Color;

			public float TimeOffset;
		}

		private static readonly Color activeBackColor = Color.Black;

		private static readonly Color disabledBackColor = Calc.HexToColor("1f2e2d");

		private static readonly Color activeLineColor = Color.White;

		private static readonly Color disabledLineColor = Calc.HexToColor("6a8480");

		private bool playerHasDreamDash;

		private Vector2? node;

		private LightOcclude occlude;

		private MTexture[] particleTextures;

		private DreamParticle[] particles;

		private float whiteFill;

		private float whiteHeight = 1f;

		private Vector2 shake;

		private float animTimer;

		private Shaker shaker;

		private bool fastMoving;

		private bool oneUse;

		private float wobbleFrom = Calc.Random.NextFloat((float)Math.PI * 2f);

		private float wobbleTo = Calc.Random.NextFloat((float)Math.PI * 2f);

		private float wobbleEase;

		public DreamBlock(Vector2 position, float width, float height, Vector2? node, bool fastMoving, bool oneUse, bool below)
			: base(position, width, height, safe: true)
		{
			base.Depth = -11000;
			this.node = node;
			this.fastMoving = fastMoving;
			this.oneUse = oneUse;
			if (below)
			{
				base.Depth = 5000;
			}
			SurfaceSoundIndex = 11;
			particleTextures = new MTexture[4]
			{
				GFX.Game["objects/dreamblock/particles"].GetSubtexture(14, 0, 7, 7),
				GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7),
				GFX.Game["objects/dreamblock/particles"].GetSubtexture(0, 0, 7, 7),
				GFX.Game["objects/dreamblock/particles"].GetSubtexture(7, 0, 7, 7)
			};
		}

		public DreamBlock(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.FirstNodeNullable(offset), data.Bool("fastMoving"), data.Bool("oneUse"), data.Bool("below"))
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			playerHasDreamDash = SceneAs<Level>().Session.Inventory.DreamDash;
			if (playerHasDreamDash && node.HasValue)
			{
				Vector2 start = Position;
				Vector2 end = node.Value;
				float time = Vector2.Distance(start, end) / 12f;
				if (fastMoving)
				{
					time /= 3f;
				}
				Tween tween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, time, start: true);
				tween.OnUpdate = delegate(Tween t)
				{
					if (Collidable)
					{
						MoveTo(Vector2.Lerp(start, end, t.Eased));
					}
					else
					{
						MoveToNaive(Vector2.Lerp(start, end, t.Eased));
					}
				};
				Add(tween);
			}
			if (!playerHasDreamDash)
			{
				Add(occlude = new LightOcclude());
			}
			Setup();
		}

		public void Setup()
		{
			particles = new DreamParticle[(int)(base.Width / 8f * (base.Height / 8f) * 0.7f)];
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Position = new Vector2(Calc.Random.NextFloat(base.Width), Calc.Random.NextFloat(base.Height));
				particles[i].Layer = Calc.Random.Choose(0, 1, 1, 2, 2, 2);
				particles[i].TimeOffset = Calc.Random.NextFloat();
				particles[i].Color = Color.LightGray * (0.5f + (float)particles[i].Layer / 2f * 0.5f);
				if (playerHasDreamDash)
				{
					switch (particles[i].Layer)
					{
					case 0:
						particles[i].Color = Calc.Random.Choose(Calc.HexToColor("FFEF11"), Calc.HexToColor("FF00D0"), Calc.HexToColor("08a310"));
						break;
					case 1:
						particles[i].Color = Calc.Random.Choose(Calc.HexToColor("5fcde4"), Calc.HexToColor("7fb25e"), Calc.HexToColor("E0564C"));
						break;
					case 2:
						particles[i].Color = Calc.Random.Choose(Calc.HexToColor("5b6ee1"), Calc.HexToColor("CC3B3B"), Calc.HexToColor("7daa64"));
						break;
					}
				}
			}
		}

		public void OnPlayerExit(Player player)
		{
			Dust.Burst(player.Position, player.Speed.Angle(), 16);
			Vector2 dir = Vector2.Zero;
			if (CollideCheck(player, Position + Vector2.UnitX * 4f))
			{
				dir = Vector2.UnitX;
			}
			else if (CollideCheck(player, Position - Vector2.UnitX * 4f))
			{
				dir = -Vector2.UnitX;
			}
			else if (CollideCheck(player, Position + Vector2.UnitY * 4f))
			{
				dir = Vector2.UnitY;
			}
			else if (CollideCheck(player, Position - Vector2.UnitY * 4f))
			{
				dir = -Vector2.UnitY;
			}
			_ = dir != Vector2.Zero;
			if (oneUse)
			{
				OneUseDestroy();
			}
		}

		private void OneUseDestroy()
		{
			Collidable = (Visible = false);
			DisableStaticMovers();
			RemoveSelf();
		}

		public override void Update()
		{
			base.Update();
			if (playerHasDreamDash)
			{
				animTimer += 6f * Engine.DeltaTime;
				wobbleEase += Engine.DeltaTime * 2f;
				if (wobbleEase > 1f)
				{
					wobbleEase = 0f;
					wobbleFrom = wobbleTo;
					wobbleTo = Calc.Random.NextFloat((float)Math.PI * 2f);
				}
				SurfaceSoundIndex = 12;
			}
		}

		public bool BlockedCheck()
		{
			TheoCrystal theo = CollideFirst<TheoCrystal>();
			if (theo != null && !TryActorWiggleUp(theo))
			{
				return true;
			}
			Player player = CollideFirst<Player>();
			if (player != null && !TryActorWiggleUp(player))
			{
				return true;
			}
			return false;
		}

		private bool TryActorWiggleUp(Entity actor)
		{
			bool was = Collidable;
			Collidable = true;
			for (int i = 1; i <= 4; i++)
			{
				if (!actor.CollideCheck<Solid>(actor.Position - Vector2.UnitY * i))
				{
					actor.Position -= Vector2.UnitY * i;
					Collidable = was;
					return true;
				}
			}
			Collidable = was;
			return false;
		}

		public override void Render()
		{
			Camera camera = SceneAs<Level>().Camera;
			if (base.Right < camera.Left || base.Left > camera.Right || base.Bottom < camera.Top || base.Top > camera.Bottom)
			{
				return;
			}
			Draw.Rect(shake.X + base.X, shake.Y + base.Y, base.Width, base.Height, playerHasDreamDash ? activeBackColor : disabledBackColor);
			Vector2 cam = SceneAs<Level>().Camera.Position;
			for (int i = 0; i < particles.Length; i++)
			{
				int layer = particles[i].Layer;
				Vector2 pos = particles[i].Position;
				pos += cam * (0.3f + 0.25f * (float)layer);
				pos = PutInside(pos);
				Color color = particles[i].Color;
				MTexture texture;
				switch (layer)
				{
				case 0:
				{
					int at2 = (int)((particles[i].TimeOffset * 4f + animTimer) % 4f);
					texture = particleTextures[3 - at2];
					break;
				}
				case 1:
				{
					int at = (int)((particles[i].TimeOffset * 2f + animTimer) % 2f);
					texture = particleTextures[1 + at];
					break;
				}
				default:
					texture = particleTextures[2];
					break;
				}
				if (pos.X >= base.X + 2f && pos.Y >= base.Y + 2f && pos.X < base.Right - 2f && pos.Y < base.Bottom - 2f)
				{
					texture.DrawCentered(pos + shake, color);
				}
			}
			if (whiteFill > 0f)
			{
				Draw.Rect(base.X + shake.X, base.Y + shake.Y, base.Width, base.Height * whiteHeight, Color.White * whiteFill);
			}
			WobbleLine(shake + new Vector2(base.X, base.Y), shake + new Vector2(base.X + base.Width, base.Y), 0f);
			WobbleLine(shake + new Vector2(base.X + base.Width, base.Y), shake + new Vector2(base.X + base.Width, base.Y + base.Height), 0.7f);
			WobbleLine(shake + new Vector2(base.X + base.Width, base.Y + base.Height), shake + new Vector2(base.X, base.Y + base.Height), 1.5f);
			WobbleLine(shake + new Vector2(base.X, base.Y + base.Height), shake + new Vector2(base.X, base.Y), 2.5f);
			Draw.Rect(shake + new Vector2(base.X, base.Y), 2f, 2f, playerHasDreamDash ? activeLineColor : disabledLineColor);
			Draw.Rect(shake + new Vector2(base.X + base.Width - 2f, base.Y), 2f, 2f, playerHasDreamDash ? activeLineColor : disabledLineColor);
			Draw.Rect(shake + new Vector2(base.X, base.Y + base.Height - 2f), 2f, 2f, playerHasDreamDash ? activeLineColor : disabledLineColor);
			Draw.Rect(shake + new Vector2(base.X + base.Width - 2f, base.Y + base.Height - 2f), 2f, 2f, playerHasDreamDash ? activeLineColor : disabledLineColor);
		}

		private Vector2 PutInside(Vector2 pos)
		{
			while (pos.X < base.X)
			{
				pos.X += base.Width;
			}
			while (pos.X > base.X + base.Width)
			{
				pos.X -= base.Width;
			}
			while (pos.Y < base.Y)
			{
				pos.Y += base.Height;
			}
			while (pos.Y > base.Y + base.Height)
			{
				pos.Y -= base.Height;
			}
			return pos;
		}

		private void WobbleLine(Vector2 from, Vector2 to, float offset)
		{
			float length = (to - from).Length();
			Vector2 normal = Vector2.Normalize(to - from);
			Vector2 perp = new Vector2(normal.Y, 0f - normal.X);
			Color line = (playerHasDreamDash ? activeLineColor : disabledLineColor);
			Color bg = (playerHasDreamDash ? activeBackColor : disabledBackColor);
			if (whiteFill > 0f)
			{
				line = Color.Lerp(line, Color.White, whiteFill);
				bg = Color.Lerp(bg, Color.White, whiteFill);
			}
			float lastAmp = 0f;
			int interval = 16;
			for (int i = 2; (float)i < length - 2f; i += interval)
			{
				float amp = Lerp(LineAmplitude(wobbleFrom + offset, i), LineAmplitude(wobbleTo + offset, i), wobbleEase);
				if ((float)(i + interval) >= length)
				{
					amp = 0f;
				}
				float len = Math.Min(interval, length - 2f - (float)i);
				Vector2 vector = from + normal * i + perp * lastAmp;
				Vector2 end = from + normal * ((float)i + len) + perp * amp;
				Draw.Line(vector - perp, end - perp, bg);
				Draw.Line(vector - perp * 2f, end - perp * 2f, bg);
				Draw.Line(vector, end, line);
				lastAmp = amp;
			}
		}

		private float LineAmplitude(float seed, float index)
		{
			return (float)(Math.Sin((double)(seed + index / 16f) + Math.Sin(seed * 2f + index / 32f) * 6.2831854820251465) + 1.0) * 1.5f;
		}

		private float Lerp(float a, float b, float percent)
		{
			return a + (b - a) * percent;
		}

		public IEnumerator Activate()
		{
			Level level = SceneAs<Level>();
			yield return 1f;
			Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
			Add(shaker = new Shaker(on: true, delegate(Vector2 t)
			{
				shake = t;
			}));
			shaker.Interval = 0.02f;
			shaker.On = true;
			for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime)
			{
				whiteFill = Ease.CubeIn(p2);
				yield return null;
			}
			shaker.On = false;
			yield return 0.5f;
			ActivateNoRoutine();
			whiteHeight = 1f;
			whiteFill = 1f;
			for (float p2 = 1f; p2 > 0f; p2 -= Engine.DeltaTime * 0.5f)
			{
				whiteHeight = p2;
				if (level.OnInterval(0.1f))
				{
					for (int i = 0; (float)i < base.Width; i += 4)
					{
						level.ParticlesFG.Emit(Strawberry.P_WingsBurst, new Vector2(base.X + (float)i, base.Y + base.Height * whiteHeight + 1f));
					}
				}
				if (level.OnInterval(0.1f))
				{
					level.Shake();
				}
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
				yield return null;
			}
			while (whiteFill > 0f)
			{
				whiteFill -= Engine.DeltaTime * 3f;
				yield return null;
			}
		}

		public void ActivateNoRoutine()
		{
			if (!playerHasDreamDash)
			{
				playerHasDreamDash = true;
				Setup();
				Remove(occlude);
			}
			whiteHeight = 0f;
			whiteFill = 0f;
			if (shaker != null)
			{
				shaker.On = false;
			}
		}

		public void FootstepRipple(Vector2 position)
		{
			if (playerHasDreamDash)
			{
				DisplacementRenderer.Burst burst = (base.Scene as Level).Displacement.AddBurst(position, 0.5f, 0f, 40f);
				burst.WorldClipCollider = base.Collider;
				burst.WorldClipPadding = 1;
			}
		}
	}
}
