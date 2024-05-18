using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class FormationBackdrop : Entity
	{
		public bool Display;

		public float Alpha = 1f;

		private bool wasDisplayed;

		private float fade;

		public FormationBackdrop()
		{
			base.Tag = (int)Tags.FrozenUpdate | (int)Tags.Global;
			base.Depth = -1999900;
		}

		public override void Update()
		{
			fade = Calc.Approach(fade, Display ? 1 : 0, Engine.RawDeltaTime * 3f);
			if (Display)
			{
				wasDisplayed = true;
			}
			if (wasDisplayed)
			{
				Level obj = base.Scene as Level;
				Snow snow = obj.Foreground.Get<Snow>();
				if (snow != null)
				{
					snow.Alpha = 1f - fade;
				}
				WindSnowFG wind = obj.Foreground.Get<WindSnowFG>();
				if (wind != null)
				{
					wind.Alpha = 1f - fade;
				}
				if (fade <= 0f)
				{
					wasDisplayed = false;
				}
			}
			base.Update();
		}

		public override void Render()
		{
			Level level = base.Scene as Level;
			if (fade > 0f)
			{
				Draw.Rect(level.Camera.Left - 1f, level.Camera.Top - 1f, 322f, 182f, Color.Black * fade * Alpha * 0.85f);
			}
		}
	}
}
