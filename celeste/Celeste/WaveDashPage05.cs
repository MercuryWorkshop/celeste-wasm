using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class WaveDashPage05 : WaveDashPage
	{
		private class Display
		{
			public Vector2 Position;

			public FancyText.Text Info;

			public WaveDashPlaybackTutorial Tutorial;

			private Coroutine routine;

			private float xEase;

			private float time;

			public Display(Vector2 position, string text, string tutorial, Vector2 tutorialOffset)
			{
				Position = position;
				Info = FancyText.Parse(text, 896, 8, 1f, Color.Black * 0.6f);
				Tutorial = new WaveDashPlaybackTutorial(tutorial, tutorialOffset, new Vector2(1f, 1f), new Vector2(1f, 1f));
				Tutorial.OnRender = delegate
				{
					Draw.Line(-64f, 20f, 64f, 20f, Color.Black);
				};
				routine = new Coroutine(Routine());
			}

			private IEnumerator Routine()
			{
				PlayerPlayback playback = Tutorial.Playback;
				int step = 0;
				while (true)
				{
					int frameIndex = playback.FrameIndex;
					if (step % 2 == 0)
					{
						Tutorial.Update();
					}
					if (frameIndex != playback.FrameIndex && playback.FrameIndex == playback.FrameCount - 1)
					{
						while (time < 3f)
						{
							yield return null;
						}
						yield return 0.1f;
						while (xEase < 1f)
						{
							xEase = Calc.Approach(xEase, 1f, Engine.DeltaTime * 4f);
							yield return null;
						}
						xEase = 1f;
						yield return 0.5f;
						xEase = 0f;
						time = 0f;
					}
					step++;
					yield return null;
				}
			}

			public void Update()
			{
				time += Engine.DeltaTime;
				routine.Update();
			}

			public void Render()
			{
				Tutorial.Render(Position, 4f);
				Info.DrawJustifyPerLine(Position + Vector2.UnitY * 200f, new Vector2(0.5f, 0f), Vector2.One * 0.8f, 1f);
				if (xEase > 0f)
				{
					Vector2 angle = Calc.AngleToVector((1f - xEase) * 0.1f + (float)Math.PI / 4f, 1f);
					Vector2 perp = angle.Perpendicular();
					float scale = 0.5f + (1f - xEase) * 0.5f;
					float stroke = 64f * scale;
					float size = 300f * scale;
					Vector2 center = Position;
					Draw.Line(center - angle * size, center + angle * size, Color.Red, stroke);
					Draw.Line(center - perp * size, center + perp * size, Color.Red, stroke);
				}
			}
		}

		private List<Display> displays = new List<Display>();

		public WaveDashPage05()
		{
			Transition = Transitions.Spiral;
			ClearColor = Calc.HexToColor("fff2cc");
		}

		public override void Added(WaveDashPresentation presentation)
		{
			base.Added(presentation);
			displays.Add(new Display(new Vector2((float)base.Width * 0.28f, base.Height - 600), Dialog.Get("WAVEDASH_PAGE5_INFO1"), "too_close", new Vector2(-50f, 20f)));
			displays.Add(new Display(new Vector2((float)base.Width * 0.72f, base.Height - 600), Dialog.Get("WAVEDASH_PAGE5_INFO2"), "too_far", new Vector2(-50f, -35f)));
		}

		public override IEnumerator Routine()
		{
			yield return 0.5f;
		}

		public override void Update()
		{
			foreach (Display display in displays)
			{
				display.Update();
			}
		}

		public override void Render()
		{
			ActiveFont.DrawOutline(Dialog.Clean("WAVEDASH_PAGE5_TITLE"), new Vector2(128f, 100f), Vector2.Zero, Vector2.One * 1.5f, Color.White, 2f, Color.Black);
			foreach (Display display in displays)
			{
				display.Render();
			}
		}
	}
}
