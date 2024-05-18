using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class LanguageSelectUI : TextMenu
	{
		private class LanguageOption : Item
		{
			public Language Language;

			private float selectedEase;

			public bool Selected => Container.Current == this;

			public LanguageOption(Language language)
			{
				Selectable = true;
				Language = language;
			}

			public override void Added()
			{
				Container.InnerContent = InnerContentMode.OneColumn;
				if (Dialog.Language == Language)
				{
					Container.Current = this;
				}
			}

			public override float LeftWidth()
			{
				return Language.Icon.Width;
			}

			public override float Height()
			{
				return Language.Icon.Height;
			}

			public override void Update()
			{
				selectedEase = Calc.Approach(selectedEase, Selected ? 1f : 0f, Engine.DeltaTime * 5f);
			}

			public override void Render(Vector2 position, bool highlighted)
			{
				Color color = (Disabled ? Color.DarkSlateGray : ((highlighted ? Container.HighlightColor : Color.White) * Container.Alpha));
				position += (1f - Ease.CubeOut(Container.Alpha)) * Vector2.UnitY * 32f;
				if (Selected)
				{
					GFX.Gui["dotarrow_outline"].DrawCentered(position, color);
				}
				position += Vector2.UnitX * Ease.CubeInOut(selectedEase) * 32f;
				Language.Icon.DrawJustified(position, new Vector2(0f, 0.5f), Color.White * Container.Alpha, 1f);
			}
		}

		private bool open = true;

		public LanguageSelectUI()
		{
			base.Tag = (int)Tags.HUD | (int)Tags.PauseUpdate;
			Alpha = 0f;
			foreach (Language language in Dialog.OrderedLanguages)
			{
				Add(new LanguageOption(language).Pressed(delegate
				{
					open = false;
					SetNextLanguage(language);
				}));
			}
			OnESC = (OnPause = (OnCancel = delegate
			{
				open = false;
				Focused = false;
			}));
		}

		private void SetNextLanguage(Language next)
		{
			if (Settings.Instance.Language != next.Id)
			{
				Language prev = Dialog.Languages[Settings.Instance.Language];
				Language english = Dialog.Languages["english"];
				if (prev.FontFace != english.FontFace)
				{
					Fonts.Unload(prev.FontFace);
				}
				Fonts.Load(next.FontFace);
				Settings.Instance.Language = next.Id;
				Settings.Instance.ApplyLanguage();
			}
		}

		public override void Update()
		{
			if (Alpha > 0f)
			{
				base.Update();
			}
			if (!open && Alpha <= 0f)
			{
				Close();
			}
			Alpha = Calc.Approach(Alpha, open ? 1 : 0, Engine.DeltaTime * 8f);
		}

		public override void Render()
		{
			if (Alpha > 0f)
			{
				Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * Ease.CubeOut(Alpha));
				base.Render();
			}
		}
	}
}
