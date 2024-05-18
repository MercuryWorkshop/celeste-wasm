using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CrumblePlatform : Solid
	{
		public static ParticleType P_Crumble;

		private List<Image> images;

		private List<Image> outline;

		private List<Coroutine> falls;

		private List<int> fallOrder;

		private ShakerList shaker;

		private LightOcclude occluder;

		private Coroutine outlineFader;

		public CrumblePlatform(Vector2 position, float width)
			: base(position, width, 8f, safe: false)
		{
			EnableAssistModeChecks = false;
		}

		public CrumblePlatform(EntityData data, Vector2 offset)
			: this(data.Position + offset, data.Width)
		{
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			MTexture tex = GFX.Game["objects/crumbleBlock/outline"];
			outline = new List<Image>();
			if (base.Width <= 8f)
			{
				Image img3 = new Image(tex.GetSubtexture(24, 0, 8, 8));
				img3.Color = Color.White * 0f;
				Add(img3);
				outline.Add(img3);
			}
			else
			{
				for (int j = 0; (float)j < base.Width; j += 8)
				{
					int tx = ((j != 0) ? ((j > 0 && (float)j < base.Width - 8f) ? 1 : 2) : 0);
					Image img = new Image(tex.GetSubtexture(tx * 8, 0, 8, 8));
					img.Position = new Vector2(j, 0f);
					img.Color = Color.White * 0f;
					Add(img);
					outline.Add(img);
				}
			}
			Add(outlineFader = new Coroutine());
			outlineFader.RemoveOnComplete = false;
			images = new List<Image>();
			falls = new List<Coroutine>();
			fallOrder = new List<int>();
			MTexture texture = GFX.Game["objects/crumbleBlock/" + AreaData.Get(scene).CrumbleBlock];
			for (int i = 0; (float)i < base.Width; i += 8)
			{
				int index = (int)((Math.Abs(base.X) + (float)i) / 8f) % 4;
				Image img2 = new Image(texture.GetSubtexture(index * 8, 0, 8, 8));
				img2.Position = new Vector2(4 + i, 4f);
				img2.CenterOrigin();
				Add(img2);
				images.Add(img2);
				Coroutine fallRoutine = new Coroutine();
				fallRoutine.RemoveOnComplete = false;
				falls.Add(fallRoutine);
				Add(fallRoutine);
				fallOrder.Add(i / 8);
			}
			fallOrder.Shuffle();
			Add(new Coroutine(Sequence()));
			Add(shaker = new ShakerList(images.Count, on: false, delegate(Vector2[] v)
			{
				for (int k = 0; k < images.Count; k++)
				{
					images[k].Position = new Vector2(4 + k * 8, 4f) + v[k];
				}
			}));
			Add(occluder = new LightOcclude(0.2f));
		}

		private IEnumerator Sequence()
		{
			while (true)
			{
				bool onTop;
				if (GetPlayerOnTop() != null)
				{
					onTop = true;
					Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
				}
				else
				{
					if (GetPlayerClimbing() == null)
					{
						yield return null;
						continue;
					}
					onTop = false;
					Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
				}
				Audio.Play("event:/game/general/platform_disintegrate", base.Center);
				shaker.ShakeFor(onTop ? 0.6f : 1f, removeOnFinish: false);
				foreach (Image img2 in images)
				{
					SceneAs<Level>().Particles.Emit(P_Crumble, 2, Position + img2.Position + new Vector2(0f, 2f), Vector2.One * 3f);
				}
				for (int k = 0; k < (onTop ? 1 : 3); k++)
				{
					yield return 0.2f;
					foreach (Image img in images)
					{
						SceneAs<Level>().Particles.Emit(P_Crumble, 2, Position + img.Position + new Vector2(0f, 2f), Vector2.One * 3f);
					}
				}
				float timer = 0.4f;
				if (onTop)
				{
					while (timer > 0f && GetPlayerOnTop() != null)
					{
						yield return null;
						timer -= Engine.DeltaTime;
					}
				}
				else
				{
					while (timer > 0f)
					{
						yield return null;
						timer -= Engine.DeltaTime;
					}
				}
				outlineFader.Replace(OutlineFade(1f));
				occluder.Visible = false;
				Collidable = false;
				float delay = 0.05f;
				for (int m = 0; m < 4; m++)
				{
					for (int i = 0; i < images.Count; i++)
					{
						if (i % 4 - m == 0)
						{
							falls[i].Replace(TileOut(images[fallOrder[i]], delay * (float)m));
						}
					}
				}
				yield return 2f;
				while (CollideCheck<Actor>() || CollideCheck<Solid>())
				{
					yield return null;
				}
				outlineFader.Replace(OutlineFade(0f));
				occluder.Visible = true;
				Collidable = true;
				for (int l = 0; l < 4; l++)
				{
					for (int j = 0; j < images.Count; j++)
					{
						if (j % 4 - l == 0)
						{
							falls[j].Replace(TileIn(j, images[fallOrder[j]], 0.05f * (float)l));
						}
					}
				}
			}
		}

		private IEnumerator OutlineFade(float to)
		{
			float from = 1f - to;
			for (float t = 0f; t < 1f; t += Engine.DeltaTime * 2f)
			{
				Color color = Color.White * (from + (to - from) * Ease.CubeInOut(t));
				foreach (Image item in outline)
				{
					item.Color = color;
				}
				yield return null;
			}
		}

		private IEnumerator TileOut(Image img, float delay)
		{
			img.Color = Color.Gray;
			yield return delay;
			float distance = (img.X * 7f % 3f + 1f) * 12f;
			Vector2 from = img.Position;
			for (float time = 0f; time < 1f; time += Engine.DeltaTime / 0.4f)
			{
				yield return null;
				img.Position = from + Vector2.UnitY * Ease.CubeIn(time) * distance;
				img.Color = Color.Gray * (1f - time);
				img.Scale = Vector2.One * (1f - time * 0.5f);
			}
			img.Visible = false;
		}

		private IEnumerator TileIn(int index, Image img, float delay)
		{
			yield return delay;
			Audio.Play("event:/game/general/platform_return", base.Center);
			img.Visible = true;
			img.Color = Color.White;
			img.Position = new Vector2(index * 8 + 4, 4f);
			for (float time = 0f; time < 1f; time += Engine.DeltaTime / 0.25f)
			{
				yield return null;
				img.Scale = Vector2.One * (1f + Ease.BounceOut(1f - time) * 0.2f);
			}
			img.Scale = Vector2.One;
		}
	}
}
