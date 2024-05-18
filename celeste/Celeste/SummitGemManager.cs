using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class SummitGemManager : Entity
	{
		private class Gem : Entity
		{
			public Vector2 Shake;

			public Sprite Sprite;

			public Image Bg;

			public BloomPoint Bloom;

			public Gem(int index, Vector2 position)
				: base(position)
			{
				base.Depth = -10010;
				Add(Bg = new Image(GFX.Game["collectables/summitgems/" + index + "/bg"]));
				Add(Sprite = new Sprite(GFX.Game, "collectables/summitgems/" + index + "/gem"));
				Add(Bloom = new BloomPoint(0f, 20f));
				Sprite.AddLoop("idle", "", 0.05f, default(int));
				Sprite.Add("spin", "", 0.05f, "idle");
				Sprite.Play("idle");
				Sprite.CenterOrigin();
				Bg.CenterOrigin();
			}

			public override void Update()
			{
				Bloom.Position = Sprite.Position;
				base.Update();
			}

			public override void Render()
			{
				Vector2 was = Sprite.Position;
				Sprite.Position += Shake;
				base.Render();
				Sprite.Position = was;
			}
		}

		private List<Gem> gems = new List<Gem>();

		public SummitGemManager(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			base.Depth = -10010;
			int index = 0;
			Vector2[] array = data.NodesOffset(offset);
			foreach (Vector2 node in array)
			{
				Gem gem = new Gem(index, node);
				gems.Add(gem);
				index++;
			}
			Add(new Coroutine(Routine()));
		}

		public override void Awake(Scene scene)
		{
			foreach (Gem gem in gems)
			{
				scene.Add(gem);
			}
			base.Awake(scene);
		}

		private IEnumerator Routine()
		{
			Level level = base.Scene as Level;
			if (level.Session.HeartGem)
			{
				foreach (Gem gem2 in gems)
				{
					gem2.Sprite.RemoveSelf();
				}
				gems.Clear();
				yield break;
			}
			while (true)
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null && (player.Position - Position).Length() < 64f)
				{
					break;
				}
				yield return null;
			}
			yield return 0.5f;
			bool alreadyHasHeart = level.Session.OldStats.Modes[0].HeartGem;
			int broken = 0;
			int index = 0;
			foreach (Gem gem in gems)
			{
				bool breakGem = level.Session.SummitGems[index];
				if (!alreadyHasHeart)
				{
					breakGem |= SaveData.Instance.SummitGems != null && SaveData.Instance.SummitGems[index];
				}
				if (breakGem)
				{
					switch (index)
					{
					case 0:
						Audio.Play("event:/game/07_summit/gem_unlock_1", gem.Position);
						break;
					case 1:
						Audio.Play("event:/game/07_summit/gem_unlock_2", gem.Position);
						break;
					case 2:
						Audio.Play("event:/game/07_summit/gem_unlock_3", gem.Position);
						break;
					case 3:
						Audio.Play("event:/game/07_summit/gem_unlock_4", gem.Position);
						break;
					case 4:
						Audio.Play("event:/game/07_summit/gem_unlock_5", gem.Position);
						break;
					case 5:
						Audio.Play("event:/game/07_summit/gem_unlock_6", gem.Position);
						break;
					}
					gem.Sprite.Play("spin");
					while (gem.Sprite.CurrentAnimationID == "spin")
					{
						gem.Bloom.Alpha = Calc.Approach(gem.Bloom.Alpha, 1f, Engine.DeltaTime * 3f);
						if (gem.Bloom.Alpha > 0.5f)
						{
							gem.Shake = Calc.Random.ShakeVector();
						}
						gem.Sprite.Y -= Engine.DeltaTime * 8f;
						gem.Sprite.Scale = Vector2.One * (1f + gem.Bloom.Alpha * 0.1f);
						yield return null;
					}
					yield return 0.2f;
					level.Shake();
					Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
					for (int i = 0; i < 20; i++)
					{
						level.ParticlesFG.Emit(SummitGem.P_Shatter, gem.Position + new Vector2(Calc.Random.Range(-8, 8), Calc.Random.Range(-8, 8)), SummitGem.GemColors[index], Calc.Random.NextFloat((float)Math.PI * 2f));
					}
					broken++;
					gem.Bloom.RemoveSelf();
					gem.Sprite.RemoveSelf();
					yield return 0.25f;
				}
				index++;
			}
			if (broken < 6)
			{
				yield break;
			}
			HeartGem heart = base.Scene.Entities.FindFirst<HeartGem>();
			if (heart == null)
			{
				yield break;
			}
			Audio.Play("event:/game/07_summit/gem_unlock_complete", heart.Position);
			yield return 0.1f;
			Vector2 from = heart.Position;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime)
			{
				if (heart.Scene == null)
				{
					break;
				}
				heart.Position = Vector2.Lerp(from, Position + new Vector2(0f, -16f), Ease.CubeOut(p));
				yield return null;
			}
		}
	}
}
