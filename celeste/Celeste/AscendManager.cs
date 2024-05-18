using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class AscendManager : Entity
	{
		public class Streaks : Entity
		{
			private class Particle
			{
				public Vector2 Position;

				public float Speed;

				public int Index;

				public int Color;
			}

			private const float MinSpeed = 600f;

			private const float MaxSpeed = 2000f;

			public float Alpha = 1f;

			private Particle[] particles = new Particle[80];

			private List<MTexture> textures;

			private Color[] colors;

			private Color[] alphaColors;

			private AscendManager manager;

			public Streaks(AscendManager manager)
			{
				this.manager = manager;
				if (manager == null || !manager.Dark)
				{
					colors = new Color[2]
					{
						Color.White,
						Calc.HexToColor("e69ecb")
					};
				}
				else
				{
					colors = new Color[2]
					{
						Calc.HexToColor("041b44"),
						Calc.HexToColor("011230")
					};
				}
				base.Depth = 20;
				textures = GFX.Game.GetAtlasSubtextures("scenery/launch/slice");
				alphaColors = new Color[colors.Length];
				for (int i = 0; i < particles.Length; i++)
				{
					float x = 160f + Calc.Random.Range(24f, 144f) * (float)Calc.Random.Choose(-1, 1);
					float y = Calc.Random.NextFloat(436f);
					float spd = Calc.ClampedMap(Math.Abs(x - 160f), 0f, 160f, 0.25f) * Calc.Random.Range(600f, 2000f);
					particles[i] = new Particle
					{
						Position = new Vector2(x, y),
						Speed = spd,
						Index = Calc.Random.Next(textures.Count),
						Color = Calc.Random.Next(colors.Length)
					};
				}
			}

			public override void Update()
			{
				base.Update();
				for (int i = 0; i < particles.Length; i++)
				{
					particles[i].Position.Y += particles[i].Speed * Engine.DeltaTime;
				}
			}

			public override void Render()
			{
				float alpha = Ease.SineInOut(((manager != null) ? manager.fade : 1f) * Alpha);
				Vector2 cam = (base.Scene as Level).Camera.Position;
				for (int j = 0; j < colors.Length; j++)
				{
					alphaColors[j] = colors[j] * alpha;
				}
				for (int i = 0; i < particles.Length; i++)
				{
					Vector2 pos = particles[i].Position;
					pos.X = Mod(pos.X, 320f);
					pos.Y = -128f + Mod(pos.Y, 436f);
					pos += cam;
					Vector2 scale = default(Vector2);
					scale.X = Calc.ClampedMap(particles[i].Speed, 600f, 2000f, 1f, 0.25f);
					scale.Y = Calc.ClampedMap(particles[i].Speed, 600f, 2000f, 1f, 2f);
					scale *= Calc.ClampedMap(particles[i].Speed, 600f, 2000f, 1f, 4f);
					textures[particles[i].Index].DrawCentered(color: alphaColors[particles[i].Color], position: pos, scale: scale);
				}
				Draw.Rect(cam.X - 10f, cam.Y - 10f, 26f, 200f, alphaColors[0]);
				Draw.Rect(cam.X + 320f - 16f, cam.Y - 10f, 26f, 200f, alphaColors[0]);
			}
		}

		public class Clouds : Entity
		{
			private class Particle
			{
				public Vector2 Position;

				public float Speed;

				public int Index;
			}

			public float Alpha;

			private AscendManager manager;

			private List<MTexture> textures;

			private Particle[] particles = new Particle[10];

			private Color color;

			public Clouds(AscendManager manager)
			{
				this.manager = manager;
				if (manager == null || !manager.Dark)
				{
					color = Calc.HexToColor("b64a86");
				}
				else
				{
					color = Calc.HexToColor("082644");
				}
				base.Depth = -1000000;
				textures = GFX.Game.GetAtlasSubtextures("scenery/launch/cloud");
				for (int i = 0; i < particles.Length; i++)
				{
					particles[i] = new Particle
					{
						Position = new Vector2(Calc.Random.NextFloat(320f), Calc.Random.NextFloat(900f)),
						Speed = Calc.Random.Range(400, 800),
						Index = Calc.Random.Next(textures.Count)
					};
				}
			}

			public override void Update()
			{
				base.Update();
				for (int i = 0; i < particles.Length; i++)
				{
					particles[i].Position.Y += particles[i].Speed * Engine.DeltaTime;
				}
			}

			public override void Render()
			{
				float alpha = ((manager != null) ? manager.fade : 1f) * Alpha;
				Color col = color * alpha;
				Vector2 cam = (base.Scene as Level).Camera.Position;
				for (int i = 0; i < particles.Length; i++)
				{
					Vector2 pos = particles[i].Position;
					pos.Y = -360f + Mod(pos.Y, 900f);
					pos += cam;
					textures[particles[i].Index].DrawCentered(pos, col);
				}
			}
		}

		private class Fader : Entity
		{
			public float Fade;

			private AscendManager manager;

			public Fader(AscendManager manager)
			{
				this.manager = manager;
				base.Depth = -1000010;
			}

			public override void Render()
			{
				if (Fade > 0f)
				{
					Vector2 cam = (base.Scene as Level).Camera.Position;
					Draw.Rect(cam.X - 10f, cam.Y - 10f, 340f, 200f, (manager.Dark ? Color.Black : Color.White) * Fade);
				}
			}
		}

		private const string BeginSwapFlag = "beginswap_";

		private const string BgSwapFlag = "bgswap_";

		public readonly bool Dark;

		public readonly bool Ch9Ending;

		private bool introLaunch;

		private int index;

		private string cutscene;

		private Level level;

		private float fade;

		private float scroll;

		private bool outTheTop;

		private Color background;

		private string ambience;

		public AscendManager(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			base.Tag = Tags.TransitionUpdate;
			base.Depth = 8900;
			index = data.Int("index");
			cutscene = data.Attr("cutscene");
			introLaunch = data.Bool("intro_launch");
			Dark = data.Bool("dark");
			Ch9Ending = cutscene.Equals("CH9_FREE_BIRD", StringComparison.InvariantCultureIgnoreCase);
			ambience = data.Attr("ambience");
			background = (Dark ? Color.Black : Calc.HexToColor("75a0ab"));
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			level = base.Scene as Level;
			Add(new Coroutine(Routine()));
		}

		private IEnumerator Routine()
		{
			Player player = base.Scene.Tracker.GetEntity<Player>();
			while (player == null || player.Y > base.Y)
			{
				player = base.Scene.Tracker.GetEntity<Player>();
				yield return null;
			}
			if (index == 9)
			{
				yield return 1.6f;
			}
			Streaks streaks = new Streaks(this);
			base.Scene.Add(streaks);
			if (!Dark)
			{
				Clouds clouds = new Clouds(this);
				base.Scene.Add(clouds);
			}
			level.Session.SetFlag("beginswap_" + index);
			player.Sprite.Play("launch");
			player.Speed = Vector2.Zero;
			player.StateMachine.State = 11;
			player.DummyGravity = false;
			player.DummyAutoAnimate = false;
			if (!string.IsNullOrWhiteSpace(ambience))
			{
				if (ambience.Equals("null", StringComparison.InvariantCultureIgnoreCase))
				{
					Audio.SetAmbience(null);
				}
				else
				{
					Audio.SetAmbience(SFX.EventnameByHandle(ambience));
				}
			}
			if (introLaunch)
			{
				FadeSnapTo(1f);
				level.Camera.Position = player.Center + new Vector2(-160f, -90f);
				yield return 2.3f;
			}
			else
			{
				yield return FadeTo(1f, Dark ? 2f : 0.8f);
				if (Ch9Ending)
				{
					level.Add(new CS10_FreeBird());
					while (true)
					{
						yield return null;
					}
				}
				if (!string.IsNullOrEmpty(cutscene))
				{
					yield return 0.25f;
					CS07_Ascend cs = new CS07_Ascend(index, cutscene, Dark);
					level.Add(cs);
					yield return null;
					while (cs.Running)
					{
						yield return null;
					}
				}
				else
				{
					yield return 0.5f;
				}
			}
			level.CanRetry = false;
			player.Sprite.Play("launch");
			Audio.Play("event:/char/madeline/summit_flytonext", player.Position);
			yield return 0.25f;
			Vector2 from2 = player.Position;
			for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime / 1f)
			{
				player.Position = Vector2.Lerp(from2, from2 + new Vector2(0f, 60f), Ease.CubeInOut(p2)) + Calc.Random.ShakeVector();
				Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
				yield return null;
			}
			Fader fader = new Fader(this);
			base.Scene.Add(fader);
			from2 = player.Position;
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime / 0.5f)
			{
				float was = player.Y;
				player.Position = Vector2.Lerp(from2, from2 + new Vector2(0f, -160f), Ease.SineIn(p2));
				if (p2 == 0f || Calc.OnInterval(player.Y, was, 16f))
				{
					level.Add(Engine.Pooler.Create<SpeedRing>().Init(player.Center, new Vector2(0f, -1f).Angle(), Color.White));
				}
				if (p2 >= 0.5f)
				{
					fader.Fade = (p2 - 0.5f) * 2f;
				}
				else
				{
					fader.Fade = 0f;
				}
				yield return null;
			}
			level.CanRetry = true;
			outTheTop = true;
			player.Y = level.Bounds.Top;
			player.SummitLaunch(player.X);
			player.DummyGravity = true;
			player.DummyAutoAnimate = true;
			level.Session.SetFlag("bgswap_" + index);
			level.NextTransitionDuration = 0.05f;
			if (introLaunch)
			{
				level.Add(new HeightDisplay(-1));
			}
		}

		public override void Update()
		{
			scroll += Engine.DeltaTime * 240f;
			base.Update();
		}

		public override void Render()
		{
			Draw.Rect(level.Camera.X - 10f, level.Camera.Y - 10f, 340f, 200f, background * fade);
		}

		public override void Removed(Scene scene)
		{
			FadeSnapTo(0f);
			level.Session.SetFlag("bgswap_" + index, setTo: false);
			level.Session.SetFlag("beginswap_" + index, setTo: false);
			if (outTheTop)
			{
				ScreenWipe.WipeColor = (Dark ? Color.Black : Color.White);
				if (introLaunch)
				{
					new MountainWipe(base.Scene, wipeIn: true);
				}
				else if (index == 0)
				{
					AreaData.Get(1).DoScreenWipe(base.Scene, wipeIn: true);
				}
				else if (index == 1)
				{
					AreaData.Get(2).DoScreenWipe(base.Scene, wipeIn: true);
				}
				else if (index == 2)
				{
					AreaData.Get(3).DoScreenWipe(base.Scene, wipeIn: true);
				}
				else if (index == 3)
				{
					AreaData.Get(4).DoScreenWipe(base.Scene, wipeIn: true);
				}
				else if (index == 4)
				{
					AreaData.Get(5).DoScreenWipe(base.Scene, wipeIn: true);
				}
				else if (index == 5)
				{
					AreaData.Get(7).DoScreenWipe(base.Scene, wipeIn: true);
				}
				else if (index >= 9)
				{
					AreaData.Get(10).DoScreenWipe(base.Scene, wipeIn: true);
				}
				ScreenWipe.WipeColor = Color.Black;
			}
			base.Removed(scene);
		}

		private IEnumerator FadeTo(float target, float duration = 0.8f)
		{
			while ((fade = Calc.Approach(fade, target, Engine.DeltaTime / duration)) != target)
			{
				FadeSnapTo(fade);
				yield return null;
			}
			FadeSnapTo(target);
		}

		private void FadeSnapTo(float target)
		{
			fade = target;
			SetSnowAlpha(1f - fade);
			SetBloom(fade * 0.1f);
			if (!Dark)
			{
				return;
			}
			foreach (Parallax item in level.Background.GetEach<Parallax>())
			{
				item.CameraOffset.Y -= 25f * target;
			}
			foreach (Parallax item2 in level.Foreground.GetEach<Parallax>())
			{
				item2.Alpha = 1f - fade;
			}
		}

		private void SetBloom(float add)
		{
			level.Bloom.Base = AreaData.Get(level).BloomBase + add;
		}

		private void SetSnowAlpha(float value)
		{
			Snow snow = level.Foreground.Get<Snow>();
			if (snow != null)
			{
				snow.Alpha = value;
			}
			RainFG rain = level.Foreground.Get<RainFG>();
			if (rain != null)
			{
				rain.Alpha = value;
			}
			WindSnowFG wind = level.Foreground.Get<WindSnowFG>();
			if (wind != null)
			{
				wind.Alpha = value;
			}
		}

		private static float Mod(float x, float m)
		{
			return (x % m + m) % m;
		}
	}
}
