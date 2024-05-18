using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class UnlockedRemixDisplay : Entity
	{
		private const float DisplayDuration = 4f;

		private const float LerpInSpeed = 1.2f;

		private const float LerpOutSpeed = 2f;

		private const float IconSize = 128f;

		private const float Spacing = 20f;

		private string text;

		private float drawLerp;

		private MTexture bg;

		private MTexture icon;

		private float rotation;

		private bool unlockedRemix;

		private TotalStrawberriesDisplay strawberries;

		private Wiggler wiggler;

		private bool hasCassetteAlready;

		public UnlockedRemixDisplay()
		{
			base.Tag = (int)Tags.HUD | (int)Tags.Global | (int)Tags.PauseUpdate | (int)Tags.TransitionUpdate;
			bg = GFX.Gui["strawberryCountBG"];
			icon = GFX.Gui["collectables/cassette"];
			text = Dialog.Clean("ui_remix_unlocked");
			Visible = false;
			base.Y = 96f;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			hasCassetteAlready = SaveData.Instance.Areas[AreaData.Get(base.Scene).ID].Cassette;
			unlockedRemix = (scene as Level).Session.Cassette;
		}

		public override void Update()
		{
			base.Update();
			if (!unlockedRemix && (base.Scene as Level).Session.Cassette)
			{
				unlockedRemix = true;
				Add(new Coroutine(DisplayRoutine()));
			}
			if (Visible)
			{
				float y = 96f;
				if (Settings.Instance.SpeedrunClock == SpeedrunType.Chapter)
				{
					y += 58f;
				}
				else if (Settings.Instance.SpeedrunClock == SpeedrunType.File)
				{
					y += 78f;
				}
				if (strawberries.Visible)
				{
					y += 96f;
				}
				base.Y = Calc.Approach(base.Y, y, Engine.DeltaTime * 800f);
			}
		}

		private IEnumerator DisplayRoutine()
		{
			strawberries = base.Scene.Entities.FindFirst<TotalStrawberriesDisplay>();
			Visible = true;
			while ((drawLerp += Engine.DeltaTime * 1.2f) < 1f)
			{
				yield return null;
			}
			Add(wiggler = Wiggler.Create(0.8f, 4f, delegate(float f)
			{
				rotation = f * 0.1f;
			}, start: true));
			drawLerp = 1f;
			yield return 4f;
			while ((drawLerp -= Engine.DeltaTime * 2f) > 0f)
			{
				yield return null;
			}
			Visible = false;
			RemoveSelf();
		}

		public override void Render()
		{
			float width = 0f;
			width = ((!hasCassetteAlready) ? (ActiveFont.Measure(text).X + 128f + 80f) : 188f);
			Vector2 at = Vector2.Lerp(new Vector2(0f - width, base.Y), new Vector2(0f, base.Y), Ease.CubeOut(drawLerp));
			bg.DrawJustified(at + new Vector2(width, 0f), new Vector2(1f, 0.5f));
			Draw.Rect(at.X, at.Y - (float)(bg.Height / 2), width - (float)bg.Width + 1f, bg.Height, Color.Black);
			float scale = 128f / (float)icon.Width;
			icon.DrawJustified(at + new Vector2(20f + (float)icon.Width * scale * 0.5f, 0f), new Vector2(0.5f, 0.5f), Color.White, scale, rotation);
			if (!hasCassetteAlready)
			{
				ActiveFont.DrawOutline(text, at + new Vector2(168f, 0f), new Vector2(0f, 0.6f), Vector2.One, Color.White, 2f, Color.Black);
			}
		}
	}
}
