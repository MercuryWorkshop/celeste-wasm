using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class BackgroundFadeIn : Entity
	{
		public Color Color;

		public float Duration;

		public float Delay;

		public float Percent;

		public BackgroundFadeIn(Color color, float delay, float duration)
		{
			base.Tag = (int)Tags.Persistent | (int)Tags.TransitionUpdate;
			base.Depth = 10100;
			Color = color;
			Delay = delay;
			Duration = duration;
			Percent = 0f;
		}

		public override void Update()
		{
			if (Delay <= 0f)
			{
				if (Percent >= 1f)
				{
					RemoveSelf();
				}
				Percent += Engine.DeltaTime / Duration;
			}
			else
			{
				Delay -= Engine.DeltaTime;
			}
			base.Update();
		}

		public override void Render()
		{
			Vector2 cam = (base.Scene as Level).Camera.Position;
			Draw.Rect(cam.X - 10f, cam.Y - 10f, 340f, 200f, Color * (1f - Percent));
		}
	}
}
