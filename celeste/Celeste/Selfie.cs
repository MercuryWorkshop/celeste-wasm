using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class Selfie : Entity
	{
		private Level level;

		private Image image;

		private Image overImage;

		private bool waitForKeyPress;

		private float timer;

		private Tween tween;

		public Selfie(Level level)
		{
			base.Tag = Tags.HUD;
			this.level = level;
		}

		public IEnumerator PictureRoutine(string photo = "selfie")
		{
			level.Flash(Color.White);
			yield return 0.5f;
			yield return OpenRoutine(photo);
			yield return WaitForInput();
			yield return EndRoutine();
		}

		public IEnumerator FilterRoutine()
		{
			yield return OpenRoutine();
			yield return 0.5f;
			MTexture tex = GFX.Portraits["selfieFilter"];
			overImage = new Image(tex);
			overImage.Visible = false;
			overImage.CenterOrigin();
			int atWidth = 0;
			tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 0.4f, start: true);
			tween.OnUpdate = delegate(Tween t)
			{
				int num = (int)Math.Round(MathHelper.Lerp(0f, tex.Width, t.Eased));
				if (num != atWidth)
				{
					atWidth = num;
					overImage.Texture = tex.GetSubtexture(tex.Width - atWidth, 0, atWidth, tex.Height);
					overImage.Visible = true;
					overImage.Origin.X = atWidth - tex.Width / 2;
				}
			};
			Audio.Play("event:/game/02_old_site/theoselfie_photo_filter");
			yield return tween.Wait();
			yield return WaitForInput();
			yield return EndRoutine();
		}

		public IEnumerator OpenRoutine(string selfie = "selfie")
		{
			Audio.Play("event:/game/02_old_site/theoselfie_photo_in");
			image = new Image(GFX.Portraits[selfie]);
			image.CenterOrigin();
			float percent = 0f;
			while (percent < 1f)
			{
				percent += Engine.DeltaTime;
				image.Position = Vector2.Lerp(new Vector2(992f, 1080f + image.Height / 2f), new Vector2(960f, 540f), Ease.CubeOut(percent));
				image.Rotation = MathHelper.Lerp(0.5f, 0f, Ease.BackOut(percent));
				yield return null;
			}
		}

		public IEnumerator WaitForInput()
		{
			waitForKeyPress = true;
			while (!Input.MenuCancel.Pressed && !Input.MenuConfirm.Pressed)
			{
				yield return null;
			}
			Audio.Play("event:/ui/main/button_lowkey");
			waitForKeyPress = false;
		}

		public IEnumerator EndRoutine()
		{
			Audio.Play("event:/game/02_old_site/theoselfie_photo_out");
			float percent = 0f;
			while (percent < 1f)
			{
				percent += Engine.DeltaTime * 2f;
				image.Position = Vector2.Lerp(new Vector2(960f, 540f), new Vector2(928f, (0f - image.Height) / 2f), Ease.BackIn(percent));
				image.Rotation = MathHelper.Lerp(0f, -0.15f, Ease.BackIn(percent));
				yield return null;
			}
			yield return null;
			level.Remove(this);
		}

		public override void Update()
		{
			if (tween != null && tween.Active)
			{
				tween.Update();
			}
			if (waitForKeyPress)
			{
				timer += Engine.DeltaTime;
			}
		}

		public override void Render()
		{
			if (base.Scene is Level level && (level.FrozenOrPaused || level.RetryPlayerCorpse != null || level.SkippingCutscene))
			{
				return;
			}
			if (image != null && image.Visible)
			{
				image.Render();
				if (overImage != null && overImage.Visible)
				{
					overImage.Position = image.Position;
					overImage.Rotation = image.Rotation;
					overImage.Scale = image.Scale;
					overImage.Render();
				}
			}
			if (waitForKeyPress)
			{
				GFX.Gui["textboxbutton"].DrawCentered(image.Position + new Vector2(image.Width / 2f + 40f, image.Height / 2f + (float)((timer % 1f < 0.25f) ? 6 : 0)));
			}
		}
	}
}
