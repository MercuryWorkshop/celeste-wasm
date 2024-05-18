using System.Collections;
using Microsoft.Xna.Framework;

namespace Celeste
{
	public abstract class WaveDashPage
	{
		public enum Transitions
		{
			ScaleIn,
			FadeIn,
			Rotate3D,
			Blocky,
			Spiral
		}

		public WaveDashPresentation Presentation;

		public Color ClearColor;

		public Transitions Transition;

		public bool AutoProgress;

		public bool WaitingForInput;

		public int Width => Presentation.ScreenWidth;

		public int Height => Presentation.ScreenHeight;

		public abstract IEnumerator Routine();

		public virtual void Added(WaveDashPresentation presentation)
		{
			Presentation = presentation;
		}

		public virtual void Update()
		{
		}

		public virtual void Render()
		{
		}

		protected IEnumerator PressButton()
		{
			WaitingForInput = true;
			while (!Input.MenuConfirm.Pressed)
			{
				yield return null;
			}
			WaitingForInput = false;
			Audio.Play("event:/new_content/game/10_farewell/ppt_mouseclick");
		}
	}
}
