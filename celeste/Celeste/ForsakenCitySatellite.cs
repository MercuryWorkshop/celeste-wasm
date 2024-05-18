using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class ForsakenCitySatellite : Entity
	{
		private class CodeBird : Entity
		{
			private Sprite sprite;

			private Coroutine routine;

			private float timer = Calc.Random.NextFloat();

			private Vector2 speed;

			private Image heartImage;

			private readonly string code;

			private readonly Vector2 origin;

			private readonly Vector2 dash;

			public CodeBird(Vector2 origin, string code)
				: base(origin)
			{
				this.code = code;
				this.origin = origin;
				Add(sprite = new Sprite(GFX.Game, "scenery/flutterbird/"));
				sprite.AddLoop("fly", "flap", 0.08f);
				sprite.Play("fly");
				sprite.CenterOrigin();
				sprite.Color = Colors[code];
				Vector2 dashDir = Vector2.Zero;
				dashDir.X = (code.Contains('L') ? (-1) : (code.Contains('R') ? 1 : 0));
				dashDir.Y = (code.Contains('U') ? (-1) : (code.Contains('D') ? 1 : 0));
				dash = dashDir.SafeNormalize();
				Add(routine = new Coroutine(AimlessFlightRoutine()));
			}

			public override void Update()
			{
				timer += Engine.DeltaTime;
				sprite.Y = (float)Math.Sin(timer * 2f);
				base.Update();
			}

			public void Dash()
			{
				routine.Replace(DashRoutine());
			}

			public void Transform(float duration)
			{
				base.Tag = Tags.FrozenUpdate;
				routine.Replace(TransformRoutine(duration));
			}

			private IEnumerator AimlessFlightRoutine()
			{
				speed = Vector2.Zero;
				while (true)
				{
					Vector2 target = origin + Calc.AngleToVector(Calc.Random.NextFloat((float)Math.PI * 2f), 16f + Calc.Random.NextFloat(40f));
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
						if (Math.Sign(dir.X) != 0)
						{
							sprite.Scale.X = Math.Sign(dir.X);
						}
						yield return null;
					}
				}
			}

			private IEnumerator DashRoutine()
			{
				for (float t3 = 0.25f; t3 > 0f; t3 -= Engine.DeltaTime)
				{
					speed = Calc.Approach(speed, Vector2.Zero, 200f * Engine.DeltaTime);
					Position += speed * Engine.DeltaTime;
					yield return null;
				}
				Vector2 from = Position;
				Vector2 to = origin + dash * 8f;
				if (Math.Sign(to.X - from.X) != 0)
				{
					sprite.Scale.X = Math.Sign(to.X - from.X);
				}
				for (float t3 = 0f; t3 < 1f; t3 += Engine.DeltaTime * 1.5f)
				{
					Position = from + (to - from) * Ease.CubeInOut(t3);
					yield return null;
				}
				Position = to;
				yield return 0.2f;
				if (dash.X != 0f)
				{
					sprite.Scale.X = Math.Sign(dash.X);
				}
				(base.Scene as Level).Displacement.AddBurst(Position, 0.25f, 4f, 24f, 0.4f);
				speed = dash * 300f;
				for (float t3 = 0.4f; t3 > 0f; t3 -= Engine.DeltaTime)
				{
					if (t3 > 0.1f && base.Scene.OnInterval(0.02f))
					{
						SceneAs<Level>().ParticlesBG.Emit(Particles[code], 1, Position, Vector2.One * 2f, dash.Angle());
					}
					speed = Calc.Approach(speed, Vector2.Zero, 800f * Engine.DeltaTime);
					Position += speed * Engine.DeltaTime;
					yield return null;
				}
				yield return 0.4f;
				routine.Replace(AimlessFlightRoutine());
			}

			private IEnumerator TransformRoutine(float duration)
			{
				Color colorFrom = sprite.Color;
				Color colorTo = Color.White;
				Vector2 target = origin;
				Add(heartImage = new Image(GFX.Game["collectables/heartGem/shape"]));
				heartImage.CenterOrigin();
				heartImage.Scale = Vector2.Zero;
				for (float t = 0f; t < 1f; t += Engine.DeltaTime / duration)
				{
					Vector2 dir = (target - Position).SafeNormalize();
					speed += 400f * dir * Engine.DeltaTime;
					float maxspeed = Math.Max(20f, (1f - t) * 200f);
					if (speed.Length() > maxspeed)
					{
						speed = speed.SafeNormalize(maxspeed);
					}
					Position += speed * Engine.DeltaTime;
					sprite.Color = Color.Lerp(colorFrom, colorTo, t);
					heartImage.Scale = Vector2.One * Math.Max(0f, (t - 0.75f) * 4f);
					if (dir.X != 0f)
					{
						sprite.Scale.X = Math.Abs(sprite.Scale.X) * (float)Math.Sign(dir.X);
					}
					sprite.Scale.X = (float)Math.Sign(sprite.Scale.X) * (1f - heartImage.Scale.X);
					sprite.Scale.Y = 1f - heartImage.Scale.X;
					yield return null;
				}
			}
		}

		private const string UnlockedFlag = "unlocked_satellite";

		public static readonly Dictionary<string, Color> Colors = new Dictionary<string, Color>
		{
			{
				"U",
				Calc.HexToColor("f0f0f0")
			},
			{
				"L",
				Calc.HexToColor("9171f2")
			},
			{
				"DR",
				Calc.HexToColor("0a44e0")
			},
			{
				"UR",
				Calc.HexToColor("b32d00")
			},
			{
				"UL",
				Calc.HexToColor("ffcd37")
			}
		};

		public static readonly Dictionary<string, string> Sounds = new Dictionary<string, string>
		{
			{ "U", "event:/game/01_forsaken_city/console_white" },
			{ "L", "event:/game/01_forsaken_city/console_purple" },
			{ "DR", "event:/game/01_forsaken_city/console_blue" },
			{ "UR", "event:/game/01_forsaken_city/console_red" },
			{ "UL", "event:/game/01_forsaken_city/console_yellow" }
		};

		public static readonly Dictionary<string, ParticleType> Particles = new Dictionary<string, ParticleType>();

		private static readonly string[] Code = new string[6] { "U", "L", "DR", "UR", "L", "UL" };

		private static List<string> uniqueCodes = new List<string>();

		private bool enabled;

		private List<string> currentInputs = new List<string>();

		private List<CodeBird> birds = new List<CodeBird>();

		private Vector2 gemSpawnPosition;

		private Vector2 birdFlyPosition;

		private Image sprite;

		private Image pulse;

		private Image computer;

		private Image computerScreen;

		private Sprite computerScreenNoise;

		private Image computerScreenShine;

		private BloomPoint pulseBloom;

		private BloomPoint screenBloom;

		private Level level;

		private DashListener dashListener;

		private SoundSource birdFlyingSfx;

		private SoundSource birdThrustSfx;

		private SoundSource birdFinishSfx;

		private SoundSource staticLoopSfx;

		public ForsakenCitySatellite(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			Add(sprite = new Image(GFX.Game["objects/citysatellite/dish"]));
			Add(pulse = new Image(GFX.Game["objects/citysatellite/light"]));
			Add(computer = new Image(GFX.Game["objects/citysatellite/computer"]));
			Add(computerScreen = new Image(GFX.Game["objects/citysatellite/computerscreen"]));
			Add(computerScreenNoise = new Sprite(GFX.Game, "objects/citysatellite/computerScreenNoise"));
			Add(computerScreenShine = new Image(GFX.Game["objects/citysatellite/computerscreenShine"]));
			sprite.JustifyOrigin(0.5f, 1f);
			pulse.JustifyOrigin(0.5f, 1f);
			Add(new Coroutine(PulseRoutine()));
			Add(pulseBloom = new BloomPoint(new Vector2(-12f, -44f), 1f, 8f));
			Add(screenBloom = new BloomPoint(new Vector2(32f, 20f), 1f, 8f));
			computerScreenNoise.AddLoop("static", "", 0.05f);
			computerScreenNoise.Play("static");
			computer.Position = (computerScreen.Position = (computerScreenShine.Position = (computerScreenNoise.Position = new Vector2(8f, 8f))));
			birdFlyPosition = offset + data.Nodes[0];
			gemSpawnPosition = offset + data.Nodes[1];
			Add(dashListener = new DashListener());
			dashListener.OnDash = delegate(Vector2 dir)
			{
				string text = "";
				if (dir.Y < 0f)
				{
					text = "U";
				}
				else if (dir.Y > 0f)
				{
					text = "D";
				}
				if (dir.X < 0f)
				{
					text += "L";
				}
				else if (dir.X > 0f)
				{
					text += "R";
				}
				currentInputs.Add(text);
				if (currentInputs.Count > Code.Length)
				{
					currentInputs.RemoveAt(0);
				}
				if (currentInputs.Count == Code.Length)
				{
					bool flag = true;
					for (int j = 0; j < Code.Length; j++)
					{
						if (!currentInputs[j].Equals(Code[j]))
						{
							flag = false;
						}
					}
					if (flag && level.Camera.Left + 32f < gemSpawnPosition.X && enabled)
					{
						Add(new Coroutine(UnlockGem()));
					}
				}
			};
			string[] code2 = Code;
			foreach (string code in code2)
			{
				if (!uniqueCodes.Contains(code))
				{
					uniqueCodes.Add(code);
				}
			}
			base.Depth = 8999;
			Add(staticLoopSfx = new SoundSource());
			staticLoopSfx.Position = computer.Position;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			level = scene as Level;
			enabled = !level.Session.HeartGem && !level.Session.GetFlag("unlocked_satellite");
			if (enabled)
			{
				foreach (string code in uniqueCodes)
				{
					CodeBird bird = new CodeBird(birdFlyPosition, code);
					birds.Add(bird);
					level.Add(bird);
				}
				Add(birdFlyingSfx = new SoundSource());
				Add(birdFinishSfx = new SoundSource());
				Add(birdThrustSfx = new SoundSource());
				birdFlyingSfx.Position = birdFlyPosition - Position;
				birdFlyingSfx.Play("event:/game/01_forsaken_city/birdbros_fly_loop");
			}
			else
			{
				staticLoopSfx.Play("event:/game/01_forsaken_city/console_static_loop");
			}
			if (!level.Session.HeartGem && level.Session.GetFlag("unlocked_satellite"))
			{
				HeartGem gem = new HeartGem(gemSpawnPosition);
				level.Add(gem);
			}
		}

		public override void Update()
		{
			base.Update();
			computerScreenNoise.Visible = !pulse.Visible;
			computerScreen.Visible = pulse.Visible;
			screenBloom.Visible = pulseBloom.Visible;
		}

		private IEnumerator PulseRoutine()
		{
			pulseBloom.Visible = (pulse.Visible = false);
			while (enabled)
			{
				yield return 2f;
				for (int i = 0; i < Code.Length; i++)
				{
					if (!enabled)
					{
						break;
					}
					pulse.Color = (computerScreen.Color = Colors[Code[i]]);
					pulseBloom.Visible = (pulse.Visible = true);
					Audio.Play(Sounds[Code[i]], Position + computer.Position);
					yield return 0.5f;
					pulseBloom.Visible = (pulse.Visible = false);
					Audio.Play((i < Code.Length - 1) ? "event:/game/01_forsaken_city/console_static_short" : "event:/game/01_forsaken_city/console_static_long", Position + computer.Position);
					yield return 0.2f;
				}
				Add(Alarm.Create(Alarm.AlarmMode.Oneshot, delegate
				{
					if (enabled)
					{
						birdThrustSfx.Position = birdFlyPosition - Position;
						birdThrustSfx.Play("event:/game/01_forsaken_city/birdbros_thrust");
					}
				}, 1.1f, start: true));
				birds.Shuffle();
				foreach (CodeBird bird in birds)
				{
					if (enabled)
					{
						bird.Dash();
						yield return 0.02f;
					}
				}
			}
			pulseBloom.Visible = (pulse.Visible = false);
		}

		private IEnumerator UnlockGem()
		{
			level.Session.SetFlag("unlocked_satellite");
			birdFinishSfx.Position = birdFlyPosition - Position;
			birdFinishSfx.Play("event:/game/01_forsaken_city/birdbros_finish");
			staticLoopSfx.Play("event:/game/01_forsaken_city/console_static_loop");
			enabled = false;
			yield return 0.25f;
			level.Displacement.Clear();
			yield return null;
			birdFlyingSfx.Stop();
			level.Frozen = true;
			base.Tag = Tags.FrozenUpdate;
			BloomPoint bloom = new BloomPoint(birdFlyPosition - Position, 0f, 32f);
			Add(bloom);
			foreach (CodeBird bird in birds)
			{
				bird.Transform(3f);
			}
			while (bloom.Alpha < 1f)
			{
				bloom.Alpha += Engine.DeltaTime / 3f;
				yield return null;
			}
			yield return 0.25f;
			foreach (CodeBird bird2 in birds)
			{
				bird2.RemoveSelf();
			}
			ParticleSystem particles = new ParticleSystem(-10000, 100);
			particles.Tag = Tags.FrozenUpdate;
			particles.Emit(BirdNPC.P_Feather, 24, birdFlyPosition, new Vector2(4f, 4f));
			level.Add(particles);
			HeartGem gem = new HeartGem(birdFlyPosition)
			{
				Tag = Tags.FrozenUpdate
			};
			level.Add(gem);
			yield return null;
			gem.ScaleWiggler.Start();
			yield return 0.85f;
			SimpleCurve curve = new SimpleCurve(gem.Position, gemSpawnPosition, (gem.Position + gemSpawnPosition) / 2f + new Vector2(0f, -64f));
			for (float t = 0f; t < 1f; t += Engine.DeltaTime)
			{
				yield return null;
				gem.Position = curve.GetPoint(Ease.CubeInOut(t));
			}
			yield return 0.5f;
			particles.RemoveSelf();
			Remove(bloom);
			level.Frozen = false;
		}
	}
}
