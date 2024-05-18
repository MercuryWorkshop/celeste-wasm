using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class SwitchGate : Solid
	{
		public static ParticleType P_Behind;

		public static ParticleType P_Dust;

		private MTexture[,] nineSlice;

		private Sprite icon;

		private Vector2 iconOffset;

		private Wiggler wiggler;

		private Vector2 node;

		private SoundSource openSfx;

		private bool persistent;

		private Color inactiveColor = Calc.HexToColor("5fcde4");

		private Color activeColor = Color.White;

		private Color finishColor = Calc.HexToColor("f141df");

		public SwitchGate(Vector2 position, float width, float height, Vector2 node, bool persistent, string spriteName)
			: base(position, width, height, safe: false)
		{
			this.node = node;
			this.persistent = persistent;
			Add(icon = new Sprite(GFX.Game, "objects/switchgate/icon"));
			icon.Add("spin", "", 0.1f, "spin");
			icon.Play("spin");
			icon.Rate = 0f;
			icon.Color = inactiveColor;
			icon.Position = (iconOffset = new Vector2(width / 2f, height / 2f));
			icon.CenterOrigin();
			Add(wiggler = Wiggler.Create(0.5f, 4f, delegate(float f)
			{
				icon.Scale = Vector2.One * (1f + f);
			}));
			MTexture tex = GFX.Game["objects/switchgate/" + spriteName];
			nineSlice = new MTexture[3, 3];
			for (int x = 0; x < 3; x++)
			{
				for (int y = 0; y < 3; y++)
				{
					nineSlice[x, y] = tex.GetSubtexture(new Rectangle(x * 8, y * 8, 8, 8));
				}
			}
			Add(openSfx = new SoundSource());
			Add(new LightOcclude(0.5f));
		}

		public SwitchGate(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset, data.Bool("persistent"), data.Attr("sprite", "block"))
		{
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (Switch.CheckLevelFlag(SceneAs<Level>()))
			{
				MoveTo(node);
				icon.Rate = 0f;
				icon.SetAnimationFrame(0);
				icon.Color = finishColor;
			}
			else
			{
				Add(new Coroutine(Sequence(node)));
			}
		}

		public override void Render()
		{
			float columns = base.Collider.Width / 8f - 1f;
			float rows = base.Collider.Height / 8f - 1f;
			for (int x = 0; (float)x <= columns; x++)
			{
				for (int y = 0; (float)y <= rows; y++)
				{
					int tx = (((float)x < columns) ? Math.Min(x, 1) : 2);
					int ty = (((float)y < rows) ? Math.Min(y, 1) : 2);
					nineSlice[tx, ty].Draw(Position + base.Shake + new Vector2(x * 8, y * 8));
				}
			}
			icon.Position = iconOffset + base.Shake;
			icon.DrawOutline();
			base.Render();
		}

		private IEnumerator Sequence(Vector2 node)
		{
			Vector2 start = Position;
			while (!Switch.Check(base.Scene))
			{
				yield return null;
			}
			if (persistent)
			{
				Switch.SetLevelFlag(SceneAs<Level>());
			}
			yield return 0.1f;
			openSfx.Play("event:/game/general/touchswitch_gate_open");
			StartShaking(0.5f);
			while (icon.Rate < 1f)
			{
				icon.Color = Color.Lerp(inactiveColor, activeColor, icon.Rate);
				icon.Rate += Engine.DeltaTime * 2f;
				yield return null;
			}
			yield return 0.1f;
			int particleAt = 0;
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 2f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				MoveTo(Vector2.Lerp(start, node, t.Eased));
				if (base.Scene.OnInterval(0.1f))
				{
					particleAt++;
					particleAt %= 2;
					for (int n = 0; (float)n < base.Width / 8f; n++)
					{
						for (int num = 0; (float)num < base.Height / 8f; num++)
						{
							if ((n + num) % 2 == particleAt)
							{
								SceneAs<Level>().ParticlesBG.Emit(P_Behind, Position + new Vector2(n * 8, num * 8) + Calc.Random.Range(Vector2.One * 2f, Vector2.One * 6f));
							}
						}
					}
				}
			};
			Add(tween);
			yield return 1.8f;
			bool was2 = Collidable;
			Collidable = false;
			if (node.X <= start.X)
			{
				Vector2 add4 = new Vector2(0f, 2f);
				for (int m = 0; (float)m < base.Height / 8f; m++)
				{
					Vector2 at4 = new Vector2(base.Left - 1f, base.Top + 4f + (float)(m * 8));
					Vector2 not4 = at4 + Vector2.UnitX;
					if (base.Scene.CollideCheck<Solid>(at4) && !base.Scene.CollideCheck<Solid>(not4))
					{
						SceneAs<Level>().ParticlesFG.Emit(P_Dust, at4 + add4, (float)Math.PI);
						SceneAs<Level>().ParticlesFG.Emit(P_Dust, at4 - add4, (float)Math.PI);
					}
				}
			}
			if (node.X >= start.X)
			{
				Vector2 add3 = new Vector2(0f, 2f);
				for (int l = 0; (float)l < base.Height / 8f; l++)
				{
					Vector2 at3 = new Vector2(base.Right + 1f, base.Top + 4f + (float)(l * 8));
					Vector2 not3 = at3 - Vector2.UnitX * 2f;
					if (base.Scene.CollideCheck<Solid>(at3) && !base.Scene.CollideCheck<Solid>(not3))
					{
						SceneAs<Level>().ParticlesFG.Emit(P_Dust, at3 + add3, 0f);
						SceneAs<Level>().ParticlesFG.Emit(P_Dust, at3 - add3, 0f);
					}
				}
			}
			if (node.Y <= start.Y)
			{
				Vector2 add2 = new Vector2(2f, 0f);
				for (int k = 0; (float)k < base.Width / 8f; k++)
				{
					Vector2 at2 = new Vector2(base.Left + 4f + (float)(k * 8), base.Top - 1f);
					Vector2 not2 = at2 + Vector2.UnitY;
					if (base.Scene.CollideCheck<Solid>(at2) && !base.Scene.CollideCheck<Solid>(not2))
					{
						SceneAs<Level>().ParticlesFG.Emit(P_Dust, at2 + add2, -(float)Math.PI / 2f);
						SceneAs<Level>().ParticlesFG.Emit(P_Dust, at2 - add2, -(float)Math.PI / 2f);
					}
				}
			}
			if (node.Y >= start.Y)
			{
				Vector2 add = new Vector2(2f, 0f);
				for (int j = 0; (float)j < base.Width / 8f; j++)
				{
					Vector2 at = new Vector2(base.Left + 4f + (float)(j * 8), base.Bottom + 1f);
					Vector2 not = at - Vector2.UnitY * 2f;
					if (base.Scene.CollideCheck<Solid>(at) && !base.Scene.CollideCheck<Solid>(not))
					{
						SceneAs<Level>().ParticlesFG.Emit(P_Dust, at + add, (float)Math.PI / 2f);
						SceneAs<Level>().ParticlesFG.Emit(P_Dust, at - add, (float)Math.PI / 2f);
					}
				}
			}
			Collidable = was2;
			Audio.Play("event:/game/general/touchswitch_gate_finish", Position);
			StartShaking(0.2f);
			while (icon.Rate > 0f)
			{
				icon.Color = Color.Lerp(activeColor, finishColor, 1f - icon.Rate);
				icon.Rate -= Engine.DeltaTime * 4f;
				yield return null;
			}
			icon.Rate = 0f;
			icon.SetAnimationFrame(0);
			wiggler.Start();
			bool was = Collidable;
			Collidable = false;
			if (!base.Scene.CollideCheck<Solid>(base.Center))
			{
				for (int i = 0; i < 32; i++)
				{
					float angle = Calc.Random.NextFloat((float)Math.PI * 2f);
					SceneAs<Level>().ParticlesFG.Emit(TouchSwitch.P_Fire, Position + iconOffset + Calc.AngleToVector(angle, 4f), angle);
				}
			}
			Collidable = was;
		}
	}
}
