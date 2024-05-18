using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class BreathingMinigame : Entity
	{
		private struct Particle
		{
			public Vector2 Position;

			public float Speed;

			public float Scale;

			public float Sin;

			public void Reset()
			{
				float range = Calc.Random.NextFloat();
				range *= range * range * range;
				Position = new Vector2(Calc.Random.NextFloat() * 1920f, Calc.Random.NextFloat() * 1080f);
				Scale = Calc.Map(range, 0f, 1f, 0.05f, 0.8f);
				Speed = Scale * Calc.Random.Range(2f, 8f);
				Sin = Calc.Random.NextFloat((float)Math.PI * 2f);
			}
		}

		private const float StablizeDuration = 30f;

		private const float StablizeLossRate = 0.5f;

		private const float StablizeIncreaseDelay = 0.2f;

		private const float StablizeLossPenalty = 0.5f;

		private const float Acceleration = 280f;

		private const float Gravity = 280f;

		private const float Maxspeed = 200f;

		private const float Bounds = 450f;

		private const float BGFadeStart = 0.65f;

		private const float featherSpriteOffset = -128f;

		private const float FadeBoxInMargin = 300f;

		private const float TargetSineAmplitude = 300f;

		private const float TargetSineFreq = 0.25f;

		private const float TargetBoundsAtStart = 160f;

		private const float TargetBoundsAtEnd = 100f;

		public const float MaxRumble = 0.5f;

		private const float PercentBeforeStartLosing = 0.4f;

		private const float LoseDuration = 5f;

		public bool Completed;

		public bool Pausing;

		private bool winnable;

		private float boxAlpha;

		private float featherAlpha;

		private float bgAlpha;

		private float feather;

		private float speed;

		private float stablizedTimer;

		private float currentTargetBounds = 160f;

		private float currentTargetCenter;

		private float speedMultiplier = 1f;

		private float insideTargetTimer;

		private bool boxEnabled;

		private float trailSpeed;

		private bool losing;

		private float losingTimer;

		private Sprite featherSprite;

		private Image featherSlice;

		private Image featherHalfLeft;

		private Image featherHalfRight;

		private SineWave sine;

		private SineWave featherWave;

		private BreathingRumbler rumbler;

		private string text;

		private float textAlpha;

		private VirtualRenderTarget featherBuffer;

		private VirtualRenderTarget smokeBuffer;

		private VirtualRenderTarget tempBuffer;

		private float timer;

		private Particle[] particles;

		private MTexture particleTexture = OVR.Atlas["snow"].GetSubtexture(1, 1, 254, 254);

		private float particleSpeed;

		private float particleAlpha;

		private Vector2 screenCenter => new Vector2(1920f, 1080f) / 2f;

		public BreathingMinigame(bool winnable = true, BreathingRumbler rumbler = null)
		{
			this.rumbler = rumbler;
			this.winnable = winnable;
			base.Tag = Tags.HUD;
			base.Depth = 100;
			Add(featherSprite = GFX.GuiSpriteBank.Create("feather"));
			featherSprite.Position = screenCenter + Vector2.UnitY * (feather + -128f);
			Add(new Coroutine(Routine()));
			Add(featherWave = new SineWave(0.25f));
			Add(new BeforeRenderHook(BeforeRender));
			particles = new Particle[50];
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Reset();
			}
			particleSpeed = 120f;
		}

		public IEnumerator Routine()
		{
			insideTargetTimer = 1f;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime)
			{
				yield return null;
				if (p > 1f)
				{
					p = 1f;
				}
				bgAlpha = p * 0.65f;
			}
			if (winnable)
			{
				yield return ShowText(1);
				yield return FadeGameIn();
				yield return ShowText(2);
				yield return ShowText(3);
				yield return ShowText(4);
				yield return ShowText(5);
			}
			else
			{
				yield return FadeGameIn();
			}
			Add(new Coroutine(FadeBoxIn()));
			float activeBounds = 450f;
			while (stablizedTimer < 30f)
			{
				float percent = stablizedTimer / 30f;
				bool pullUp = Input.Jump.Check || Input.Dash.Check || Input.Aim.Value.Y < 0f;
				if (winnable)
				{
					Audio.SetMusicParam("calm", percent);
					Audio.SetMusicParam("gondola_idle", percent);
				}
				else
				{
					Level level = base.Scene as Level;
					if (!losing)
					{
						float percentToLosing = percent / 0.4f;
						level.Session.Audio.Music.Layer(1, percentToLosing);
						level.Session.Audio.Music.Layer(3, 1f - percentToLosing);
						level.Session.Audio.Apply();
					}
					else
					{
						level.Session.Audio.Music.Layer(1, 1f - losingTimer);
						level.Session.Audio.Music.Layer(3, losingTimer);
						level.Session.Audio.Apply();
					}
				}
				if (!winnable && losing)
				{
					if (Calc.BetweenInterval(losingTimer * 10f, 0.5f))
					{
						pullUp = !pullUp;
					}
					activeBounds = 450f - Ease.CubeIn(losingTimer) * 200f;
				}
				if (pullUp)
				{
					if (feather > 0f - activeBounds)
					{
						speed -= 280f * Engine.DeltaTime;
					}
					particleSpeed -= 2800f * Engine.DeltaTime;
				}
				else
				{
					if (feather < activeBounds)
					{
						speed += 280f * Engine.DeltaTime;
					}
					particleSpeed += 2800f * Engine.DeltaTime;
				}
				speed = Calc.Clamp(speed, -200f, 200f);
				if (feather > activeBounds && speedMultiplier == 0f && speed > 0f)
				{
					speed = 0f;
				}
				if (feather < activeBounds && speedMultiplier == 0f && speed < 0f)
				{
					speed = 0f;
				}
				particleSpeed = Calc.Clamp(particleSpeed, -1600f, 120f);
				speedMultiplier = Calc.Approach(speedMultiplier, ((!(feather < 0f - activeBounds) || !(speed < 0f)) && (!(feather > activeBounds) || !(speed > 0f))) ? 1 : 0, Engine.DeltaTime * 4f);
				currentTargetBounds = Calc.Approach(currentTargetBounds, 160f + -60f * percent, Engine.DeltaTime * 16f);
				feather += speed * speedMultiplier * Engine.DeltaTime;
				if (boxEnabled)
				{
					currentTargetCenter = (0f - sine.Value) * 300f * MathHelper.Lerp(1f, 0f, Ease.CubeIn(percent));
					float top = currentTargetCenter - currentTargetBounds;
					float bottom = currentTargetCenter + currentTargetBounds;
					if (feather > top && feather < bottom)
					{
						insideTargetTimer += Engine.DeltaTime;
						if (insideTargetTimer > 0.2f)
						{
							stablizedTimer += Engine.DeltaTime;
						}
						if (rumbler != null)
						{
							rumbler.Strength = 0.3f * (1f - percent);
						}
					}
					else
					{
						if (insideTargetTimer > 0.2f)
						{
							stablizedTimer = Math.Max(0f, stablizedTimer - 0.5f);
						}
						if (stablizedTimer > 0f)
						{
							stablizedTimer -= 0.5f * Engine.DeltaTime;
						}
						insideTargetTimer = 0f;
						if (rumbler != null)
						{
							rumbler.Strength = 0.5f * (1f - percent);
						}
					}
				}
				else if (rumbler != null)
				{
					rumbler.Strength = 0.2f;
				}
				float fadeTarget = 0.65f + Math.Min(1f, percent / 0.8f) * 0.35000002f;
				bgAlpha = Calc.Approach(bgAlpha, fadeTarget, Engine.DeltaTime);
				featherSprite.Position = screenCenter + Vector2.UnitY * (feather + -128f);
				featherSprite.Play((insideTargetTimer > 0f || !boxEnabled) ? "hover" : "flutter");
				particleAlpha = Calc.Approach(particleAlpha, 1f, Engine.DeltaTime);
				if (!winnable && stablizedTimer > 12f)
				{
					losing = true;
				}
				if (losing)
				{
					losingTimer += Engine.DeltaTime / 5f;
					if (losingTimer > 1f)
					{
						break;
					}
				}
				yield return null;
			}
			if (!winnable)
			{
				Pausing = true;
				while (Pausing)
				{
					if (rumbler != null)
					{
						rumbler.Strength = Calc.Approach(rumbler.Strength, 1f, 2f * Engine.DeltaTime);
					}
					featherSprite.Position += (screenCenter - featherSprite.Position) * (1f - (float)Math.Pow(0.009999999776482582, Engine.DeltaTime));
					boxAlpha -= Engine.DeltaTime * 10f;
					particleAlpha = boxAlpha;
					yield return null;
				}
				losing = false;
				losingTimer = 0f;
				yield return PopFeather();
			}
			else
			{
				bgAlpha = 1f;
				if (rumbler != null)
				{
					rumbler.RemoveSelf();
					rumbler = null;
				}
				while (boxAlpha > 0f)
				{
					yield return null;
					boxAlpha -= Engine.DeltaTime;
					particleAlpha = boxAlpha;
				}
				particleAlpha = 0f;
				yield return 2f;
				while (featherAlpha > 0f)
				{
					yield return null;
					featherAlpha -= Engine.DeltaTime;
				}
				yield return 1f;
			}
			for (Completed = true; bgAlpha > 0f; bgAlpha -= Engine.DeltaTime * (winnable ? 1f : 10f))
			{
				yield return null;
			}
			RemoveSelf();
		}

		private IEnumerator ShowText(int num)
		{
			yield return FadeTextTo(0f);
			text = Dialog.Clean("CH4_GONDOLA_FEATHER_" + num);
			yield return 0.1f;
			yield return FadeTextTo(1f);
			while (!Input.MenuConfirm.Pressed)
			{
				yield return null;
			}
			yield return FadeTextTo(0f);
		}

		private IEnumerator FadeGameIn()
		{
			while (featherAlpha < 1f)
			{
				featherAlpha += Engine.DeltaTime;
				yield return null;
			}
			featherAlpha = 1f;
		}

		private IEnumerator FadeBoxIn()
		{
			yield return winnable ? 5f : 2f;
			while (Math.Abs(feather) > 300f)
			{
				yield return null;
			}
			boxEnabled = true;
			Add(sine = new SineWave(0.12f));
			while (boxAlpha < 1f)
			{
				boxAlpha += Engine.DeltaTime;
				yield return null;
			}
			boxAlpha = 1f;
		}

		private IEnumerator FadeTextTo(float v)
		{
			if (textAlpha != v)
			{
				float from = textAlpha;
				for (float p = 0f; p < 1f; p += Engine.DeltaTime * 4f)
				{
					yield return null;
					textAlpha = from + (v - from) * p;
				}
				textAlpha = v;
			}
		}

		private IEnumerator PopFeather()
		{
			Audio.Play("event:/game/06_reflection/badeline_feather_slice");
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			if (rumbler != null)
			{
				rumbler.RemoveSelf();
				rumbler = null;
			}
			featherSprite.Rotation = 0f;
			featherSprite.Play("hover");
			featherSprite.CenterOrigin();
			featherSprite.Y += featherSprite.Height / 2f;
			yield return 0.25f;
			featherSlice = new Image(GFX.Gui["feather/slice"]);
			featherSlice.CenterOrigin();
			featherSlice.Position = featherSprite.Position;
			featherSlice.Rotation = Calc.Angle(new Vector2(96f, 165f), new Vector2(140f, 112f));
			for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime * 8f)
			{
				featherSlice.Scale.X = (0.25f + Calc.YoYo(p2) * 0.75f) * 8f;
				featherSlice.Scale.Y = (0.5f + (1f - Calc.YoYo(p2)) * 0.5f) * 8f;
				featherSlice.Position = featherSprite.Position + Vector2.Lerp(new Vector2(128f, -128f), new Vector2(-128f, 128f), p2);
				yield return null;
			}
			featherSlice.Visible = false;
			(base.Scene as Level).Shake();
			(base.Scene as Level).Flash(Color.White);
			featherSprite.Visible = false;
			featherHalfLeft = new Image(GFX.Gui["feather/feather_half0"]);
			featherHalfLeft.CenterOrigin();
			featherHalfRight = new Image(GFX.Gui["feather/feather_half1"]);
			featherHalfRight.CenterOrigin();
			for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime)
			{
				featherHalfLeft.Position = featherSprite.Position + Vector2.Lerp(Vector2.Zero, new Vector2(-128f, -32f), p2);
				featherHalfRight.Position = featherSprite.Position + Vector2.Lerp(Vector2.Zero, new Vector2(128f, 32f), p2);
				featherAlpha = 1f - p2;
				yield return null;
			}
		}

		public override void Update()
		{
			timer += Engine.DeltaTime;
			trailSpeed = Calc.Approach(trailSpeed, speed, Engine.DeltaTime * 200f * 8f);
			if (featherWave != null)
			{
				featherSprite.Rotation = featherWave.Value * 0.25f + 0.1f;
			}
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Position.Y += particles[i].Speed * particleSpeed * Engine.DeltaTime;
				if (particleSpeed > -400f)
				{
					particles[i].Position.X += (particleSpeed + 400f) * (float)Math.Sin(particles[i].Sin) * 0.1f * Engine.DeltaTime;
				}
				particles[i].Sin += Engine.DeltaTime;
				if (particles[i].Position.Y < -128f || particles[i].Position.Y > 1208f)
				{
					particles[i].Reset();
					if (particleSpeed < 0f)
					{
						particles[i].Position.Y = 1208f;
					}
					else
					{
						particles[i].Position.Y = -128f;
					}
				}
			}
			base.Update();
		}

		public void BeforeRender()
		{
			if (featherBuffer == null)
			{
				int width = Math.Min(1920, Engine.ViewWidth);
				int height = Math.Min(1080, Engine.ViewHeight);
				featherBuffer = VirtualContent.CreateRenderTarget("breathing-minigame-a", width, height);
				smokeBuffer = VirtualContent.CreateRenderTarget("breathing-minigame-b", width / 2, height / 2);
				tempBuffer = VirtualContent.CreateRenderTarget("breathing-minigame-c", width / 2, height / 2);
			}
			Engine.Graphics.GraphicsDevice.SetRenderTarget(featherBuffer.Target);
			Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
			Matrix matrix = Matrix.CreateScale((float)featherBuffer.Width / 1920f);
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, matrix);
			if (losing)
			{
				featherSprite.Position += new Vector2(Calc.Random.Range(-1, 1), Calc.Random.Range(-1, 1)).SafeNormalize() * losingTimer * 10f;
				featherSprite.Rotation += (float)Calc.Random.Range(-1, 1) * losingTimer * 0.1f;
			}
			featherSprite.Color = Color.White * featherAlpha;
			if (featherSprite.Visible)
			{
				featherSprite.Render();
			}
			if (featherSlice != null && featherSlice.Visible)
			{
				featherSlice.Render();
			}
			if (featherHalfLeft != null && featherHalfLeft.Visible)
			{
				featherHalfLeft.Color = Color.White * featherAlpha;
				featherHalfRight.Color = Color.White * featherAlpha;
				featherHalfLeft.Render();
				featherHalfRight.Render();
			}
			Draw.SpriteBatch.End();
			Engine.Graphics.GraphicsDevice.SetRenderTarget(smokeBuffer.Target);
			Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
			MagicGlow.Render(featherBuffer.Target, timer, (0f - trailSpeed) / 200f * 2f, Matrix.CreateScale(0.5f));
			GaussianBlur.Blur(smokeBuffer.Target, tempBuffer, smokeBuffer);
		}

		public override void Render()
		{
			Color targetColor = ((insideTargetTimer > 0.2f) ? Color.White : (Color.White * 0.6f));
			Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * bgAlpha);
			if (!(base.Scene is Level level) || (!level.FrozenOrPaused && level.RetryPlayerCorpse == null && !level.SkippingCutscene))
			{
				MTexture border = GFX.Gui["feather/border"];
				MTexture box = GFX.Gui["feather/box"];
				box.DrawCentered(scale: new Vector2(((float)border.Width * 2f - 32f) / (float)box.Width, (currentTargetBounds * 2f - 32f) / (float)box.Height), position: screenCenter + new Vector2(0f, currentTargetCenter), color: Color.White * boxAlpha * 0.25f);
				border.Draw(screenCenter + new Vector2(-border.Width, currentTargetCenter - currentTargetBounds), Vector2.Zero, targetColor * boxAlpha, Vector2.One);
				border.Draw(screenCenter + new Vector2(border.Width, currentTargetCenter + currentTargetBounds), Vector2.Zero, targetColor * boxAlpha, new Vector2(-1f, -1f));
				if (featherBuffer != null && !featherBuffer.IsDisposed)
				{
					float scale2 = 1920f / (float)featherBuffer.Width;
					Draw.SpriteBatch.Draw(smokeBuffer.Target, Vector2.Zero, smokeBuffer.Bounds, Color.White * 0.3f, 0f, Vector2.Zero, scale2 * 2f, SpriteEffects.None, 0f);
					Draw.SpriteBatch.Draw(featherBuffer.Target, Vector2.Zero, featherBuffer.Bounds, Color.White, 0f, Vector2.Zero, scale2, SpriteEffects.None, 0f);
				}
				Vector2 stretch = new Vector2(1f, 1f);
				if (particleSpeed < 0f)
				{
					stretch = new Vector2(Math.Min(1f, 1f / ((0f - particleSpeed) * 0.004f)), Math.Max(1f, 1f * (0f - particleSpeed) * 0.004f));
				}
				for (int i = 0; i < particles.Length; i++)
				{
					Vector2 pos = particles[i].Position;
					Vector2 scale = particles[i].Scale * stretch;
					particleTexture.DrawCentered(pos, Color.White * (0.5f * particleAlpha), scale);
				}
				if (!string.IsNullOrEmpty(text) && textAlpha > 0f)
				{
					ActiveFont.Draw(text, new Vector2(960f, 920f), new Vector2(0.5f, 0.5f), Vector2.One, Color.White * textAlpha);
				}
				if (!string.IsNullOrEmpty(text) && textAlpha >= 1f)
				{
					Vector2 textSize = ActiveFont.Measure(text);
					Vector2 at = new Vector2((1920f + textSize.X) / 2f + 40f, 920f + textSize.Y / 2f - 16f) + new Vector2(0f, (timer % 1f < 0.25f) ? 6 : 0);
					GFX.Gui["textboxbutton"].DrawCentered(at);
				}
			}
		}

		public override void Removed(Scene scene)
		{
			Dispose();
			base.Removed(scene);
		}

		public override void SceneEnd(Scene scene)
		{
			Dispose();
			base.SceneEnd(scene);
		}

		private void Dispose()
		{
			if (featherBuffer != null && !featherBuffer.IsDisposed)
			{
				featherBuffer.Dispose();
				featherBuffer = null;
				smokeBuffer.Dispose();
				smokeBuffer = null;
				tempBuffer.Dispose();
				tempBuffer = null;
			}
		}
	}
}
