using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class AutoSavingNotice : Renderer
	{
		private const string title = "autosaving_title_PC";

		private const string desc = "autosaving_desc_PC";

		private const float duration = 3f;

		public static readonly Color TextColor = Color.White;

		public bool Display = true;

		public bool StillVisible;

		public bool ForceClose;

		private float ease;

		private float timer;

		private Sprite icon = GFX.GuiSpriteBank.Create("save");

		private float startTimer = 0.5f;

		private Wiggler wiggler;

		public AutoSavingNotice()
		{
			icon.Visible = false;
			wiggler = Wiggler.Create(0.4f, 4f, delegate(float f)
			{
				icon.Rotation = f * 0.1f;
			});
		}

		public override void Update(Scene scene)
		{
			base.Update(scene);
			if (startTimer > 0f)
			{
				startTimer -= Engine.DeltaTime;
				if (startTimer <= 0f)
				{
					icon.Play("start");
					icon.Visible = true;
				}
			}
			if (scene.OnInterval(1f))
			{
				wiggler.Start();
			}
			bool closing = ForceClose || (!Display && timer >= 1f);
			ease = Calc.Approach(ease, (!closing) ? 1 : 0, Engine.DeltaTime);
			timer += Engine.DeltaTime / 3f;
			StillVisible = Display || ease > 0f;
			wiggler.Update();
			icon.Update();
			if (closing && !string.IsNullOrEmpty(icon.CurrentAnimationID) && icon.CurrentAnimationID.Equals("idle"))
			{
				icon.Play("end");
			}
		}

		public override void Render(Scene scene)
		{
			float eased = Ease.CubeInOut(ease);
			Color color = TextColor * eased;
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);
			ActiveFont.Draw(Dialog.Clean("autosaving_title_PC"), new Vector2(960f, 480f - 30f * eased), new Vector2(0.5f, 1f), Vector2.One, color);
			if (icon.Visible)
			{
				icon.RenderPosition = new Vector2(1920f, 1080f) / 2f;
				icon.Render();
			}
			ActiveFont.Draw(Dialog.Clean("autosaving_desc_PC"), new Vector2(960f, 600f + 30f * eased), new Vector2(0.5f, 0f), Vector2.One, color);
			Draw.SpriteBatch.End();
		}
	}
}
