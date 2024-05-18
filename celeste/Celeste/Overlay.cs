using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Overlay : Entity
	{
		public float Fade;

		public bool XboxOverlay;

		public Overlay()
		{
			base.Tag = Tags.HUD;
			base.Depth = -100000;
		}

		public override void Added(Scene scene)
		{
			if (scene is IOverlayHandler handler)
			{
				handler.Overlay = this;
			}
			base.Added(scene);
		}

		public override void Removed(Scene scene)
		{
			if (scene is IOverlayHandler handler && handler.Overlay == this)
			{
				handler.Overlay = null;
			}
			base.Removed(scene);
		}

		public IEnumerator FadeIn()
		{
			while (Fade < 1f)
			{
				yield return null;
				Fade += Engine.DeltaTime * 4f;
			}
			Fade = 1f;
		}

		public IEnumerator FadeOut()
		{
			while (Fade > 0f)
			{
				yield return null;
				Fade -= Engine.DeltaTime * 4f;
			}
		}

		public void RenderFade()
		{
			Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * Ease.CubeInOut(Fade) * 0.95f);
		}
	}
}
