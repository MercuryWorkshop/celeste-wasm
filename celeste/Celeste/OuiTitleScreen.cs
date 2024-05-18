using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class OuiTitleScreen : Oui
	{
		public static readonly MountainCamera MountainTarget = new MountainCamera(new Vector3(0f, 12f, 24f), MountainRenderer.RotateLookAt);

		private const float TextY = 1000f;

		private const float TextOutY = 1200f;

		private const int ReflectionSliceSize = 4;

		private float alpha;

		private float fade;

		private string version = "v." + Celeste.Instance.Version;

		private bool hideConfirmButton;

		private Image logo;

		private MTexture title;

		private List<MTexture> reflections;

		private float textY;

		public OuiTitleScreen()
		{
			logo = new Image(GFX.Gui["logo"]);
			logo.CenterOrigin();
			logo.Position = new Vector2(1920f, 1080f) / 2f;
			title = GFX.Gui["title"];
			reflections = new List<MTexture>();
			for (int i = title.Height - 4; i > 0; i -= 4)
			{
				reflections.Add(title.GetSubtexture(0, i, title.Width, 4));
			}
			if (Celeste.PlayMode != 0)
			{
				if ("".Length > 0)
				{
					version += "\n";
				}
				version = version + "\n" + Celeste.PlayMode.ToString() + " Build";
			}
			if (Settings.Instance.LaunchWithFMODLiveUpdate)
			{
				version += "\nFMOD Live Update Enabled";
			}
		}

		public override bool IsStart(Overworld overworld, Overworld.StartMode start)
		{
			if (start == Overworld.StartMode.Titlescreen)
			{
				overworld.ShowInputUI = false;
				overworld.Mountain.SnapCamera(-1, MountainTarget);
				textY = 1000f;
				alpha = 1f;
				fade = 1f;
				return true;
			}
			textY = 1200f;
			return false;
		}

		public override IEnumerator Enter(Oui from)
		{
			yield return null;
			base.Overworld.ShowInputUI = false;
			MountainCamera camera = base.Overworld.Mountain.Camera;
			Vector3 center = MountainRenderer.RotateLookAt;
			Vector3 normal = (camera.Position - new Vector3(center.X, camera.Position.Y - 2f, center.Z)).SafeNormalize();
			MountainCamera away = new MountainCamera(MountainRenderer.RotateLookAt + normal * 20f, camera.Target);
			Add(new Coroutine(FadeBgTo(1f)));
			hideConfirmButton = false;
			Visible = true;
			base.Overworld.Mountain.EaseCamera(-1, away, 2f, nearTarget: false);
			float start = textY;
			yield return 0.4f;
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, 0.6f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				alpha = t.Percent;
				textY = MathHelper.Lerp(start, 1000f, t.Eased);
			};
			Add(tween);
			yield return tween.Wait();
			base.Overworld.Mountain.SnapCamera(-1, MountainTarget);
		}

		public override IEnumerator Leave(Oui next)
		{
			base.Overworld.ShowInputUI = true;
			base.Overworld.Mountain.GotoRotationMode();
			float start = textY;
			Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeInOut, 0.6f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				alpha = 1f - t.Percent;
				textY = MathHelper.Lerp(start, 1200f, t.Eased);
			};
			Add(tween);
			yield return tween.Wait();
			yield return FadeBgTo(0f);
			Visible = false;
		}

		private IEnumerator FadeBgTo(float to)
		{
			while (fade != to)
			{
				yield return null;
				fade = Calc.Approach(fade, to, Engine.DeltaTime * 2f);
			}
		}

		public override void Update()
		{
			int gamepadIndex = -1;
			if (base.Selected && Input.AnyGamepadConfirmPressed(out gamepadIndex) && !hideConfirmButton)
			{
				Input.Gamepad = gamepadIndex;
				Audio.Play("event:/ui/main/title_firstinput");
				base.Overworld.Goto<OuiMainMenu>();
			}
			base.Update();
		}

		public override void Render()
		{
			Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * fade);
			if (!hideConfirmButton)
			{
				Input.GuiButton(Input.MenuConfirm).DrawJustified(new Vector2(1840f, textY), new Vector2(1f, 1f), Color.White * alpha, 1f);
			}
			ActiveFont.Draw(version, new Vector2(80f, textY), new Vector2(0f, 1f), Vector2.One * 0.5f, Color.DarkSlateBlue);
			if (alpha > 0f)
			{
				float scale = MathHelper.Lerp(0.5f, 1f, Ease.SineOut(alpha));
				logo.Color = Color.White * alpha;
				logo.Scale = Vector2.One * scale;
				logo.Render();
				float at = base.Scene.TimeActive * 3f;
				float add = 1f / (float)reflections.Count * ((float)Math.PI * 2f) * 2f;
				float titleScale = (float)title.Width / logo.Width * scale;
				for (int i = 0; i < reflections.Count; i++)
				{
					float downEase = (float)i / (float)reflections.Count;
					float wave = (float)Math.Sin(at) * 32f * downEase;
					Vector2 position = new Vector2(1920f, 1080f) / 2f + new Vector2(wave, logo.Height * 0.5f + (float)(i * 4)) * titleScale;
					float a = Ease.CubeIn(1f - downEase) * alpha * 0.9f;
					reflections[i].DrawJustified(position, new Vector2(0.5f, 0.5f), Color.White * a, new Vector2(1f, -1f) * titleScale);
					at += add * ((float)Math.Sin(base.Scene.TimeActive + (float)i * ((float)Math.PI * 2f) * 0.04f) + 1f);
				}
			}
		}
	}
}
