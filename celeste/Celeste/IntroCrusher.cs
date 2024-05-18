using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class IntroCrusher : Solid
	{
		private Vector2 shake;

		private Vector2 start;

		private Vector2 end;

		private TileGrid tilegrid;

		private SoundSource shakingSfx;

		public IntroCrusher(Vector2 position, int width, int height, Vector2 node)
			: base(position, width, height, safe: true)
		{
			start = position;
			end = node;
			base.Depth = -10501;
			SurfaceSoundIndex = 4;
			Add(tilegrid = GFX.FGAutotiler.GenerateBox('3', width / 8, height / 8).TileGrid);
			Add(shakingSfx = new SoundSource());
		}

		public IntroCrusher(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset)
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (SceneAs<Level>().Session.GetLevelFlag("1") || SceneAs<Level>().Session.GetLevelFlag("0b"))
			{
				Position = end;
			}
			else
			{
				Add(new Coroutine(Sequence()));
			}
		}

		public override void Update()
		{
			tilegrid.Position = shake;
			base.Update();
		}

		private IEnumerator Sequence()
		{
			Player p2;
			do
			{
				yield return null;
				p2 = base.Scene.Tracker.GetEntity<Player>();
			}
			while (p2 == null || !(p2.X >= base.X + 30f) || !(p2.X <= base.Right + 8f));
			shakingSfx.Play("event:/game/00_prologue/fallblock_first_shake");
			float time2 = 1.2f;
			Shaker shaker = new Shaker(time2, removeOnFinish: true, delegate(Vector2 v)
			{
				shake = v;
			});
			Add(shaker);
			Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
			while (time2 > 0f)
			{
				Player p = base.Scene.Tracker.GetEntity<Player>();
				if (p != null && (p.X >= base.X + base.Width - 8f || p.X < base.X + 28f))
				{
					shaker.RemoveSelf();
					break;
				}
				yield return null;
				time2 -= Engine.DeltaTime;
			}
			for (int j = 2; (float)j < base.Width; j += 4)
			{
				SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustA, 2, new Vector2(base.X + (float)j, base.Y), Vector2.One * 4f, (float)Math.PI / 2f);
				SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(base.X + (float)j, base.Y), Vector2.One * 4f);
			}
			shakingSfx.Param("release", 1f);
			time2 = 0f;
			do
			{
				yield return null;
				time2 = Calc.Approach(time2, 1f, 2f * Engine.DeltaTime);
				MoveTo(Vector2.Lerp(start, end, Ease.CubeIn(time2)));
			}
			while (!(time2 >= 1f));
			for (int i = 0; (float)i <= base.Width; i += 4)
			{
				SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_FallDustA, 1, new Vector2(base.X + (float)i, base.Bottom), Vector2.One * 4f, -(float)Math.PI / 2f);
				float dir = ((!((float)i < base.Width / 2f)) ? 0f : ((float)Math.PI));
				SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_LandDust, 1, new Vector2(base.X + (float)i, base.Bottom), Vector2.One * 4f, dir);
			}
			shakingSfx.Stop();
			Audio.Play("event:/game/00_prologue/fallblock_first_impact", Position);
			SceneAs<Level>().Shake();
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			Add(new Shaker(0.25f, removeOnFinish: true, delegate(Vector2 v)
			{
				shake = v;
			}));
		}
	}
}
