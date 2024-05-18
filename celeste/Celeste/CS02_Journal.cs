using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class CS02_Journal : CutsceneEntity
	{
		private class PoemPage : Entity
		{
			private const float TextScale = 0.7f;

			private MTexture paper;

			private VirtualRenderTarget target;

			private FancyText.Text text;

			private float alpha = 1f;

			private float scale = 1f;

			private float rotation;

			private float timer;

			private bool easingOut;

			public PoemPage()
			{
				base.Tag = Tags.HUD;
				paper = GFX.Gui["poempage"];
				text = FancyText.Parse(Dialog.Get("CH2_POEM"), (int)((float)(paper.Width - 120) / 0.7f), -1, 1f, Color.Black * 0.6f);
				Add(new BeforeRenderHook(BeforeRender));
			}

			public IEnumerator EaseIn()
			{
				Audio.Play("event:/game/03_resort/memo_in");
				Vector2 center = new Vector2(Engine.Width, Engine.Height) / 2f;
				Vector2 from = center + new Vector2(0f, 200f);
				Vector2 to = center;
				float rFrom = -0.1f;
				float rTo = 0.05f;
				for (float p = 0f; p < 1f; p += Engine.DeltaTime)
				{
					Position = from + (to - from) * Ease.CubeOut(p);
					alpha = Ease.CubeOut(p);
					rotation = rFrom + (rTo - rFrom) * Ease.CubeOut(p);
					yield return null;
				}
			}

			public IEnumerator EaseOut()
			{
				Audio.Play("event:/game/03_resort/memo_out");
				easingOut = true;
				Vector2 from = Position;
				Vector2 to = new Vector2(Engine.Width, Engine.Height) / 2f + new Vector2(0f, -200f);
				float rFrom = rotation;
				float rTo = rotation + 0.1f;
				for (float p = 0f; p < 1f; p += Engine.DeltaTime * 1.5f)
				{
					Position = from + (to - from) * Ease.CubeIn(p);
					alpha = 1f - Ease.CubeIn(p);
					rotation = rFrom + (rTo - rFrom) * Ease.CubeIn(p);
					yield return null;
				}
				RemoveSelf();
			}

			public void BeforeRender()
			{
				if (target == null)
				{
					target = VirtualContent.CreateRenderTarget("journal-poem", paper.Width, paper.Height);
				}
				Engine.Graphics.GraphicsDevice.SetRenderTarget(target);
				Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
				Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
				paper.Draw(Vector2.Zero);
				text.DrawJustifyPerLine(new Vector2(paper.Width, paper.Height) / 2f, new Vector2(0.5f, 0.5f), Vector2.One * 0.7f, 1f);
				Draw.SpriteBatch.End();
			}

			public override void Removed(Scene scene)
			{
				if (target != null)
				{
					target.Dispose();
				}
				target = null;
				base.Removed(scene);
			}

			public override void SceneEnd(Scene scene)
			{
				if (target != null)
				{
					target.Dispose();
				}
				target = null;
				base.SceneEnd(scene);
			}

			public override void Update()
			{
				timer += Engine.DeltaTime;
				base.Update();
			}

			public override void Render()
			{
				if ((!(base.Scene is Level level) || (!level.FrozenOrPaused && level.RetryPlayerCorpse == null && !level.SkippingCutscene)) && target != null)
				{
					Draw.SpriteBatch.Draw((RenderTarget2D)target, Position, target.Bounds, Color.White * alpha, rotation, new Vector2(target.Width, target.Height) / 2f, scale, SpriteEffects.None, 0f);
					if (!easingOut)
					{
						GFX.Gui["textboxbutton"].DrawCentered(Position + new Vector2(target.Width / 2 + 40, target.Height / 2 + ((timer % 1f < 0.25f) ? 6 : 0)));
					}
				}
			}
		}

		private const string ReadOnceFlag = "poem_read";

		private Player player;

		private PoemPage poem;

		public CS02_Journal(Player player)
		{
			this.player = player;
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Routine()));
		}

		private IEnumerator Routine()
		{
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			if (!Level.Session.GetFlag("poem_read"))
			{
				yield return Textbox.Say("ch2_journal");
				yield return 0.1f;
			}
			poem = new PoemPage();
			base.Scene.Add(poem);
			yield return poem.EaseIn();
			while (!Input.MenuConfirm.Pressed)
			{
				yield return null;
			}
			Audio.Play("event:/ui/main/button_lowkey");
			yield return poem.EaseOut();
			poem = null;
			EndCutscene(Level);
		}

		public override void OnEnd(Level level)
		{
			player.StateMachine.Locked = false;
			player.StateMachine.State = 0;
			level.Session.SetFlag("poem_read");
			if (poem != null)
			{
				poem.RemoveSelf();
			}
		}
	}
}
