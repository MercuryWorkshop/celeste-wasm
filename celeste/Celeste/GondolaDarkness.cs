using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class GondolaDarkness : Entity
	{
		private class Blackness : Entity
		{
			public float Fade;

			public Blackness()
			{
				base.Depth = 9001;
			}

			public override void Render()
			{
				base.Render();
				Camera cam = (base.Scene as Level).Camera;
				Draw.Rect(cam.Left - 1f, cam.Top - 1f, 322f, 182f, Color.Black * Fade);
			}
		}

		private Sprite sprite;

		private Sprite hands;

		private Blackness blackness;

		private float anxiety;

		private float anxietyStutter;

		private WindSnowFG windSnowFG;

		public GondolaDarkness()
		{
			Add(sprite = GFX.SpriteBank.Create("gondolaDarkness"));
			sprite.Play("appear");
			Add(hands = GFX.SpriteBank.Create("gondolaHands"));
			hands.Visible = false;
			Visible = false;
			base.Depth = -999900;
		}

		public IEnumerator Appear(WindSnowFG windSnowFG = null)
		{
			this.windSnowFG = windSnowFG;
			Visible = true;
			base.Scene.Add(blackness = new Blackness());
			for (float t = 0f; t < 1f; t += Engine.DeltaTime / 2f)
			{
				yield return null;
				blackness.Fade = t;
				anxiety = t;
				if (windSnowFG != null)
				{
					windSnowFG.Alpha = 1f - t;
				}
			}
			yield return null;
		}

		public IEnumerator Expand()
		{
			hands.Visible = true;
			hands.Play("appear");
			yield return 1f;
		}

		public IEnumerator Reach(Gondola gondola)
		{
			hands.Play("grab");
			yield return 0.4f;
			hands.Play("pull");
			gondola.PullSides();
		}

		public override void Update()
		{
			base.Update();
			if (base.Scene.OnInterval(0.05f))
			{
				anxietyStutter = Calc.Random.NextFloat(0.1f);
			}
			Distort.AnxietyOrigin = new Vector2(0.5f, 0.5f);
			Distort.Anxiety = anxiety * 0.2f + anxietyStutter * anxiety;
		}

		public override void Render()
		{
			Position = (base.Scene as Level).Camera.Position + (base.Scene as Level).ZoomFocusPoint;
			base.Render();
		}

		public override void Removed(Scene scene)
		{
			anxiety = 0f;
			Distort.Anxiety = 0f;
			if (blackness != null)
			{
				blackness.RemoveSelf();
			}
			if (windSnowFG != null)
			{
				windSnowFG.Alpha = 1f;
			}
			base.Removed(scene);
		}
	}
}
