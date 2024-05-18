using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class OuiCredits : Oui
	{
		private readonly Vector2 onScreen = new Vector2(960f, 0f);

		private readonly Vector2 offScreen = new Vector2(3840f, 0f);

		private Credits credits;

		private float vignetteAlpha;

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Position = offScreen;
			Visible = false;
		}

		public override IEnumerator Enter(Oui from)
		{
			Audio.SetMusic("event:/music/menu/credits");
			base.Overworld.ShowConfirmUI = false;
			Credits.BorderColor = Color.Black;
			credits = new Credits();
			credits.Enabled = false;
			Visible = true;
			vignetteAlpha = 0f;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime * 4f)
			{
				Position = offScreen + (onScreen - offScreen) * Ease.CubeOut(p);
				yield return null;
			}
		}

		public override IEnumerator Leave(Oui next)
		{
			Audio.Play("event:/ui/main/whoosh_large_out");
			base.Overworld.SetNormalMusic();
			base.Overworld.ShowConfirmUI = true;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime * 4f)
			{
				Position = onScreen + (offScreen - onScreen) * Ease.CubeIn(p);
				yield return null;
			}
			Visible = false;
		}

		public override void Update()
		{
			if (Focused && (Input.MenuCancel.Pressed || credits.BottomTimer > 3f))
			{
				base.Overworld.Goto<OuiMainMenu>();
			}
			if (credits != null)
			{
				credits.Update();
				credits.Enabled = Focused && base.Selected;
			}
			vignetteAlpha = Calc.Approach(vignetteAlpha, base.Selected ? 1 : 0, Engine.DeltaTime * (base.Selected ? 1f : 4f));
			base.Update();
		}

		public override void Render()
		{
			if (vignetteAlpha > 0f)
			{
				Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * vignetteAlpha * 0.4f);
				OVR.Atlas["vignette"].Draw(Vector2.Zero, Vector2.Zero, Color.White * Ease.CubeInOut(vignetteAlpha), 1f);
			}
			if (credits != null)
			{
				credits.Render(Position);
			}
		}
	}
}
