using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class ResortRoofEnding : Solid
	{
		private MTexture[] roofCenters = new MTexture[4]
		{
			GFX.Game["decals/3-resort/roofCenter"],
			GFX.Game["decals/3-resort/roofCenter_b"],
			GFX.Game["decals/3-resort/roofCenter_c"],
			GFX.Game["decals/3-resort/roofCenter_d"]
		};

		private List<Image> images = new List<Image>();

		private List<Coroutine> wobbleRoutines = new List<Coroutine>();

		public bool BeginFalling;

		public ResortRoofEnding(EntityData data, Vector2 offset)
			: base(data.Position + offset, data.Width, 2f, safe: true)
		{
			EnableAssistModeChecks = false;
			Image broken = new Image(GFX.Game["decals/3-resort/roofEdge_d"]);
			broken.CenterOrigin();
			broken.X = 8f;
			broken.Y = 4f;
			Add(broken);
			int x;
			for (x = 0; (float)x < base.Width; x += 16)
			{
				Image img = new Image(Calc.Random.Choose(roofCenters));
				img.CenterOrigin();
				img.X = x + 8;
				img.Y = 4f;
				Add(img);
				images.Add(img);
			}
			Image end = new Image(GFX.Game["decals/3-resort/roofEdge"]);
			end.CenterOrigin();
			end.X = x + 8;
			end.Y = 4f;
			Add(end);
			images.Add(end);
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (!(base.Scene as Level).Session.GetFlag("oshiroEnding"))
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null)
				{
					base.Scene.Add(new CS03_Ending(this, player));
				}
			}
		}

		public override void Render()
		{
			Position += base.Shake;
			base.Render();
			Position -= base.Shake;
		}

		public void Wobble(AngryOshiro ghost, bool fall = false)
		{
			foreach (Coroutine wobbleRoutine in wobbleRoutines)
			{
				wobbleRoutine.RemoveSelf();
			}
			wobbleRoutines.Clear();
			Player player = base.Scene.Tracker.GetEntity<Player>();
			foreach (Image img in images)
			{
				Coroutine routine = new Coroutine(WobbleImage(img, Math.Abs(base.X + img.X - ghost.X) * 0.001f, player, fall));
				Add(routine);
				wobbleRoutines.Add(routine);
			}
		}

		private IEnumerator WobbleImage(Image img, float delay, Player player, bool fall)
		{
			float orig = img.Y;
			yield return delay;
			for (int i = 0; i < 2; i++)
			{
				base.Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + img.Position + new Vector2(-4 + i * 8, Calc.Random.Range(0, 8)), '9'));
			}
			if (!fall)
			{
				float p3 = 0f;
				float amount2 = 5f;
				while (true)
				{
					p3 += Engine.DeltaTime * 16f;
					amount2 = Calc.Approach(amount2, 1f, Engine.DeltaTime * 5f);
					float wobble2 = (float)Math.Sin(p3) * amount2;
					img.Y = orig + wobble2;
					if (player != null && Math.Abs(base.X + img.X - player.X) < 16f)
					{
						player.Sprite.Y = wobble2;
					}
					yield return null;
				}
			}
			if (fall)
			{
				while (!BeginFalling)
				{
					int wobble = Calc.Random.Range(0, 2);
					img.Y = orig + (float)wobble;
					if (player != null && Math.Abs(base.X + img.X - player.X) < 16f)
					{
						player.Sprite.Y = wobble;
					}
					yield return 0.01f;
				}
				img.Texture = GFX.Game["decals/3-resort/roofCenter_snapped_" + Calc.Random.Choose("a", "b", "c")];
				Collidable = false;
				float amount2 = Calc.Random.NextFloat();
				float p3 = -24f + Calc.Random.NextFloat(48f);
				float speedY = 0f - (80f + Calc.Random.NextFloat(80f));
				float up = new Vector2(0f, -1f).Angle();
				float off = Calc.Random.NextFloat();
				for (float p = 0f; p < 4f; p += Engine.DeltaTime)
				{
					img.Position += new Vector2(p3, speedY) * Engine.DeltaTime;
					img.Rotation += amount2 * Ease.CubeIn(p);
					p3 = Calc.Approach(p3, 0f, Engine.DeltaTime * 200f);
					speedY += 600f * Engine.DeltaTime;
					if (base.Scene.OnInterval(0.1f, off))
					{
						Dust.Burst(Position + img.Position, up);
					}
					yield return null;
				}
			}
			player.Sprite.Y = 0f;
		}
	}
}
