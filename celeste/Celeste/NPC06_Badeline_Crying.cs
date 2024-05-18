using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class NPC06_Badeline_Crying : NPC
	{
		private class Orb : Entity
		{
			public Image Sprite;

			public BloomPoint Bloom;

			private float ease;

			public Vector2 Target;

			public Coroutine Routine;

			public float Ease
			{
				get
				{
					return ease;
				}
				set
				{
					ease = value;
					Sprite.Scale = Vector2.One * ease;
					Bloom.Alpha = ease;
				}
			}

			public Orb(Vector2 position)
				: base(position)
			{
				Add(Sprite = new Image(GFX.Game["characters/badeline/orb"]));
				Add(Bloom = new BloomPoint(0f, 32f));
				Add(Routine = new Coroutine(FloatRoutine()));
				Sprite.CenterOrigin();
				base.Depth = -10001;
			}

			public IEnumerator FloatRoutine()
			{
				Vector2 speed = Vector2.Zero;
				Ease = 0.2f;
				while (true)
				{
					Vector2 target = Target + Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 16f + Calc.Random.NextFloat(40f));
					float reset = 0f;
					while (reset < 1f && (target - Position).Length() > 8f)
					{
						Vector2 dir = (target - Position).SafeNormalize();
						speed += dir * 420f * Engine.DeltaTime;
						if (speed.Length() > 90f)
						{
							speed = speed.SafeNormalize(90f);
						}
						Position += speed * Engine.DeltaTime;
						reset += Engine.DeltaTime;
						Ease = Calc.Approach(Ease, 1f, Engine.DeltaTime * 4f);
						yield return null;
					}
				}
			}

			public IEnumerator CircleRoutine(float offset)
			{
				Vector2 from = Position;
				float ease = 0f;
				Player player = base.Scene.Tracker.GetEntity<Player>();
				while (player != null)
				{
					float rotation = base.Scene.TimeActive * 2f + offset;
					Vector2 target = player.Center + Calc.AngleToVector(rotation, 24f);
					ease = Calc.Approach(ease, 1f, Engine.DeltaTime * 2f);
					Position = from + (target - from) * Monocle.Ease.CubeInOut(ease);
					yield return null;
				}
			}

			public IEnumerator AbsorbRoutine()
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null)
				{
					Vector2 from = Position;
					Vector2 to = player.Center;
					for (float p = 0f; p < 1f; p += Engine.DeltaTime)
					{
						float e = Monocle.Ease.BigBackIn(p);
						Position = from + (to - from) * e;
						Ease = 0.2f + (1f - e) * 0.8f;
						yield return null;
					}
				}
			}
		}

		private bool started;

		private Image white;

		private BloomPoint bloom;

		private VertexLight light;

		public SoundSource LoopingSfx;

		private List<Orb> orbs = new List<Orb>();

		public NPC06_Badeline_Crying(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			Add(Sprite = GFX.SpriteBank.Create("badeline_boss"));
			Sprite.Play("scaredIdle");
			Add(white = new Image(GFX.Game["characters/badelineBoss/calm_white"]));
			white.Color = Color.White * 0f;
			white.Origin = Sprite.Origin;
			white.Position = Sprite.Position;
			Add(bloom = new BloomPoint(new Vector2(0f, -6f), 0f, 16f));
			Add(light = new VertexLight(new Vector2(0f, -6f), Color.White, 1f, 24, 64));
			Add(LoopingSfx = new SoundSource("event:/char/badeline/boss_idle_ground"));
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (!base.Session.GetFlag("badeline_connection"))
			{
				return;
			}
			FinalBossStarfield bossBg = (scene as Level).Background.Get<FinalBossStarfield>();
			if (bossBg != null)
			{
				bossBg.Alpha = 0f;
			}
			foreach (Entity entity in base.Scene.Tracker.GetEntities<ReflectionTentacles>())
			{
				entity.RemoveSelf();
			}
			RemoveSelf();
		}

		public override void Update()
		{
			base.Update();
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (!started && player != null && player.X > base.X - 32f)
			{
				base.Scene.Add(new CS06_BossEnd(player, this));
				started = true;
			}
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);
			foreach (Orb orb in orbs)
			{
				orb.RemoveSelf();
			}
		}

		public IEnumerator TurnWhite(float duration)
		{
			float alpha = 0f;
			while (alpha < 1f)
			{
				alpha += Engine.DeltaTime / duration;
				white.Color = Color.White * alpha;
				bloom.Alpha = alpha;
				yield return null;
			}
			Sprite.Visible = false;
		}

		public IEnumerator Disperse()
		{
			Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
			float size = 1f;
			while (orbs.Count < 8)
			{
				float to = size - 0.125f;
				while (size > to)
				{
					white.Scale = Vector2.One * size;
					light.Alpha = size;
					bloom.Alpha = size;
					size -= Engine.DeltaTime;
					yield return null;
				}
				Orb orb = new Orb(Position);
				orb.Target = Position + new Vector2(-16f, -40f);
				base.Scene.Add(orb);
				orbs.Add(orb);
			}
			yield return 3.25f;
			int i = 0;
			foreach (Orb orb3 in orbs)
			{
				orb3.Routine.Replace(orb3.CircleRoutine((float)i / 8f * ((float)Math.PI * 2f)));
				i++;
				yield return 0.2f;
			}
			yield return 2f;
			foreach (Orb orb2 in orbs)
			{
				orb2.Routine.Replace(orb2.AbsorbRoutine());
			}
			yield return 1f;
		}
	}
}
