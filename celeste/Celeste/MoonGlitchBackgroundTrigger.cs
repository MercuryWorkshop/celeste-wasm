using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class MoonGlitchBackgroundTrigger : Trigger
	{
		private enum Duration
		{
			Short,
			Medium,
			Long
		}

		private Duration duration;

		private bool triggered;

		private bool stayOn;

		private bool running;

		private bool doGlitch;

		public MoonGlitchBackgroundTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			duration = data.Enum("duration", Duration.Short);
			stayOn = data.Bool("stay");
			doGlitch = data.Bool("glitch", defaultValue: true);
		}

		public override void OnEnter(Player player)
		{
			Invoke();
		}

		public void Invoke()
		{
			if (!triggered)
			{
				triggered = true;
				if (doGlitch)
				{
					Add(new Coroutine(InternalGlitchRoutine()));
				}
				else if (!stayOn)
				{
					Toggle(on: false);
				}
			}
		}

		private IEnumerator InternalGlitchRoutine()
		{
			running = true;
			base.Tag = Tags.Persistent;
			float time;
			if (duration == Duration.Short)
			{
				time = 0.2f;
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
				Audio.Play("event:/new_content/game/10_farewell/glitch_short");
			}
			else if (duration == Duration.Medium)
			{
				time = 0.5f;
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
				Audio.Play("event:/new_content/game/10_farewell/glitch_medium");
			}
			else
			{
				time = 1.25f;
				Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
				Audio.Play("event:/new_content/game/10_farewell/glitch_long");
			}
			yield return GlitchRoutine(time, stayOn);
			base.Tag = 0;
			running = false;
		}

		private static void Toggle(bool on)
		{
			Level level = Engine.Scene as Level;
			foreach (Backdrop item in level.Background.GetEach<Backdrop>("blackhole"))
			{
				item.ForceVisible = on;
			}
			foreach (Backdrop item2 in level.Foreground.GetEach<Backdrop>("blackhole"))
			{
				item2.ForceVisible = on;
			}
		}

		private static void Fade(float alpha, bool max = false)
		{
			Level level = Engine.Scene as Level;
			foreach (Backdrop bg2 in level.Background.GetEach<Backdrop>("blackhole"))
			{
				bg2.FadeAlphaMultiplier = (max ? Math.Max(bg2.FadeAlphaMultiplier, alpha) : alpha);
			}
			foreach (Backdrop bg in level.Foreground.GetEach<Backdrop>("blackhole"))
			{
				bg.FadeAlphaMultiplier = (max ? Math.Max(bg.FadeAlphaMultiplier, alpha) : alpha);
			}
		}

		public static IEnumerator GlitchRoutine(float duration, bool stayOn)
		{
			Toggle(on: true);
			if (Settings.Instance.DisableFlashes)
			{
				for (float a2 = 0f; a2 < 1f; a2 += Engine.DeltaTime / 0.1f)
				{
					Fade(a2, max: true);
					yield return null;
				}
				Fade(1f);
				yield return duration;
				if (!stayOn)
				{
					for (float a2 = 0f; a2 < 1f; a2 += Engine.DeltaTime / 0.1f)
					{
						Fade(1f - a2);
						yield return null;
					}
					Fade(1f);
				}
			}
			else if (duration > 0.4f)
			{
				Glitch.Value = 0.3f;
				yield return 0.2f;
				Glitch.Value = 0f;
				yield return duration - 0.4f;
				if (!stayOn)
				{
					Glitch.Value = 0.3f;
				}
				yield return 0.2f;
				Glitch.Value = 0f;
			}
			else
			{
				Glitch.Value = 0.3f;
				yield return duration;
				Glitch.Value = 0f;
			}
			if (!stayOn)
			{
				Toggle(on: false);
			}
		}

		public override void Removed(Scene scene)
		{
			if (running)
			{
				Glitch.Value = 0f;
				Fade(1f);
				if (!stayOn)
				{
					Toggle(on: false);
				}
			}
			base.Removed(scene);
		}
	}
}
