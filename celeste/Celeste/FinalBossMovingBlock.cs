using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class FinalBossMovingBlock : Solid
	{
		public static ParticleType P_Stop;

		public static ParticleType P_Break;

		public int BossNodeIndex;

		private float startDelay;

		private int nodeIndex;

		private Vector2[] nodes;

		private TileGrid sprite;

		private TileGrid highlight;

		private Coroutine moveCoroutine;

		private bool isHighlighted;

		public FinalBossMovingBlock(Vector2[] nodes, float width, float height, int bossNodeIndex)
			: base(nodes[0], width, height, safe: false)
		{
			BossNodeIndex = bossNodeIndex;
			this.nodes = nodes;
			int newSeed = Calc.Random.Next();
			Calc.PushRandom(newSeed);
			sprite = GFX.FGAutotiler.GenerateBox('g', (int)base.Width / 8, (int)base.Height / 8).TileGrid;
			Add(sprite);
			Calc.PopRandom();
			Calc.PushRandom(newSeed);
			highlight = GFX.FGAutotiler.GenerateBox('G', (int)(base.Width / 8f), (int)base.Height / 8).TileGrid;
			highlight.Alpha = 0f;
			Add(highlight);
			Calc.PopRandom();
			Add(new TileInterceptor(sprite, highPriority: false));
			Add(new LightOcclude());
		}

		public FinalBossMovingBlock(EntityData data, Vector2 offset)
			: this(data.NodesWithPosition(offset), data.Width, data.Height, data.Int("nodeIndex"))
		{
		}

		public override void OnShake(Vector2 amount)
		{
			base.OnShake(amount);
			sprite.Position = amount;
		}

		public void StartMoving(float delay)
		{
			startDelay = delay;
			Add(moveCoroutine = new Coroutine(MoveSequence()));
		}

		private IEnumerator MoveSequence()
		{
			while (true)
			{
				StartShaking(0.2f + startDelay);
				if (!isHighlighted)
				{
					for (float p = 0f; p < 1f; p += Engine.DeltaTime / (0.2f + startDelay + 0.2f))
					{
						highlight.Alpha = Ease.CubeIn(p);
						sprite.Alpha = 1f - highlight.Alpha;
						yield return null;
					}
					highlight.Alpha = 1f;
					sprite.Alpha = 0f;
					isHighlighted = true;
				}
				else
				{
					yield return 0.2f + startDelay + 0.2f;
				}
				startDelay = 0f;
				nodeIndex++;
				nodeIndex %= nodes.Length;
				Vector2 from = Position;
				Vector2 to = nodes[nodeIndex];
				Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 0.8f, start: true);
				tween.OnUpdate = delegate(Tween t)
				{
					MoveTo(Vector2.Lerp(from, to, t.Eased));
				};
				tween.OnComplete = delegate
				{
					if (CollideCheck<SolidTiles>(Position + (to - from).SafeNormalize() * 2f))
					{
						Audio.Play("event:/game/06_reflection/fallblock_boss_impact", base.Center);
						ImpactParticles(to - from);
					}
					else
					{
						StopParticles(to - from);
					}
				};
				Add(tween);
				yield return 0.8f;
			}
		}

		private void StopParticles(Vector2 moved)
		{
			Level level = SceneAs<Level>();
			float dir = moved.Angle();
			if (moved.X > 0f)
			{
				Vector2 at4 = new Vector2(base.Right - 1f, base.Top);
				for (int l = 0; (float)l < base.Height; l += 4)
				{
					level.Particles.Emit(P_Stop, at4 + Vector2.UnitY * (2 + l + Calc.Random.Range(-1, 1)), dir);
				}
			}
			else if (moved.X < 0f)
			{
				Vector2 at3 = new Vector2(base.Left, base.Top);
				for (int k = 0; (float)k < base.Height; k += 4)
				{
					level.Particles.Emit(P_Stop, at3 + Vector2.UnitY * (2 + k + Calc.Random.Range(-1, 1)), dir);
				}
			}
			if (moved.Y > 0f)
			{
				Vector2 at2 = new Vector2(base.Left, base.Bottom - 1f);
				for (int j = 0; (float)j < base.Width; j += 4)
				{
					level.Particles.Emit(P_Stop, at2 + Vector2.UnitX * (2 + j + Calc.Random.Range(-1, 1)), dir);
				}
			}
			else if (moved.Y < 0f)
			{
				Vector2 at = new Vector2(base.Left, base.Top);
				for (int i = 0; (float)i < base.Width; i += 4)
				{
					level.Particles.Emit(P_Stop, at + Vector2.UnitX * (2 + i + Calc.Random.Range(-1, 1)), dir);
				}
			}
		}

		private void BreakParticles()
		{
			Vector2 from = base.Center;
			for (int x = 0; (float)x < base.Width; x += 4)
			{
				for (int y = 0; (float)y < base.Height; y += 4)
				{
					Vector2 at = Position + new Vector2(2 + x, 2 + y);
					SceneAs<Level>().Particles.Emit(P_Break, 1, at, Vector2.One * 2f, (at - from).Angle());
				}
			}
		}

		private void ImpactParticles(Vector2 moved)
		{
			if (moved.X < 0f)
			{
				Vector2 add4 = new Vector2(0f, 2f);
				for (int l = 0; (float)l < base.Height / 8f; l++)
				{
					Vector2 at4 = new Vector2(base.Left - 1f, base.Top + 4f + (float)(l * 8));
					if (!base.Scene.CollideCheck<Water>(at4) && base.Scene.CollideCheck<Solid>(at4))
					{
						SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at4 + add4, 0f);
						SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at4 - add4, 0f);
					}
				}
			}
			else if (moved.X > 0f)
			{
				Vector2 add3 = new Vector2(0f, 2f);
				for (int k = 0; (float)k < base.Height / 8f; k++)
				{
					Vector2 at3 = new Vector2(base.Right + 1f, base.Top + 4f + (float)(k * 8));
					if (!base.Scene.CollideCheck<Water>(at3) && base.Scene.CollideCheck<Solid>(at3))
					{
						SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at3 + add3, (float)Math.PI);
						SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at3 - add3, (float)Math.PI);
					}
				}
			}
			if (moved.Y < 0f)
			{
				Vector2 add2 = new Vector2(2f, 0f);
				for (int j = 0; (float)j < base.Width / 8f; j++)
				{
					Vector2 at2 = new Vector2(base.Left + 4f + (float)(j * 8), base.Top - 1f);
					if (!base.Scene.CollideCheck<Water>(at2) && base.Scene.CollideCheck<Solid>(at2))
					{
						SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at2 + add2, (float)Math.PI / 2f);
						SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at2 - add2, (float)Math.PI / 2f);
					}
				}
			}
			else
			{
				if (!(moved.Y > 0f))
				{
					return;
				}
				Vector2 add = new Vector2(2f, 0f);
				for (int i = 0; (float)i < base.Width / 8f; i++)
				{
					Vector2 at = new Vector2(base.Left + 4f + (float)(i * 8), base.Bottom + 1f);
					if (!base.Scene.CollideCheck<Water>(at) && base.Scene.CollideCheck<Solid>(at))
					{
						SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at + add, -(float)Math.PI / 2f);
						SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, at - add, -(float)Math.PI / 2f);
					}
				}
			}
		}

		public override void Render()
		{
			Vector2 was = Position;
			Position += base.Shake;
			base.Render();
			if (highlight.Alpha > 0f && highlight.Alpha < 1f)
			{
				int inflate = (int)((1f - highlight.Alpha) * 16f);
				Rectangle rect = new Rectangle((int)base.X, (int)base.Y, (int)base.Width, (int)base.Height);
				rect.Inflate(inflate, inflate);
				Draw.HollowRect(rect, Color.Lerp(Color.Purple, Color.Pink, 0.7f));
			}
			Position = was;
		}

		private void Finish()
		{
			Vector2 blastFrom = base.CenterRight + Vector2.UnitX * 10f;
			for (int x = 0; (float)x < base.Width / 8f; x++)
			{
				for (int y = 0; (float)y < base.Height / 8f; y++)
				{
					base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + x * 8, 4 + y * 8), 'f').BlastFrom(blastFrom));
				}
			}
			BreakParticles();
			DestroyStaticMovers();
			RemoveSelf();
		}

		public void Destroy(float delay)
		{
			if (base.Scene != null)
			{
				if (moveCoroutine != null)
				{
					Remove(moveCoroutine);
				}
				if (delay <= 0f)
				{
					Finish();
					return;
				}
				StartShaking(delay);
				Alarm.Set(this, delay, Finish);
			}
		}
	}
}
