using System;
using System.Collections;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
	public class CS03_Memo : CutsceneEntity
	{
		private class MemoPage : Entity
		{
			private const float TextScale = 0.75f;

			private const float PaperScale = 1.5f;

			private Atlas atlas;

			private MTexture paper;

			private MTexture title;

			private VirtualRenderTarget target;

			private FancyText.Text text;

			private float textDownscale = 1f;

			private float alpha = 1f;

			private float scale = 1f;

			private float rotation;

			private float timer;

			private bool easingOut;

			public MemoPage()
			{
				base.Tag = Tags.HUD;
				atlas = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Memo"), Atlas.AtlasDataFormat.Packer);
				paper = atlas["memo"];
				if (atlas.Has("title_" + Settings.Instance.Language))
				{
					title = atlas["title_" + Settings.Instance.Language];
				}
				else
				{
					title = atlas["title_english"];
				}
				float maxWidth = (float)paper.Width * 1.5f - 120f;
				text = FancyText.Parse(Dialog.Get("CH3_MEMO"), (int)(maxWidth / 0.75f), -1, 1f, Color.Black * 0.6f);
				float textWidth = text.WidestLine() * 0.75f;
				if (textWidth > maxWidth)
				{
					textDownscale = maxWidth / textWidth;
				}
				Add(new BeforeRenderHook(BeforeRender));
			}

			public IEnumerator EaseIn()
			{
				Audio.Play("event:/game/03_resort/memo_in");
				Vector2 from = new Vector2(Engine.Width / 2, Engine.Height + 100);
				Vector2 to = new Vector2(Engine.Width / 2, Engine.Height / 2 - 150);
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

			public IEnumerator Wait()
			{
				float start = Position.Y;
				int index = 0;
				while (!Input.MenuCancel.Pressed)
				{
					float target = start - (float)(index * 400);
					Position.Y += (target - Position.Y) * (1f - (float)Math.Pow(0.009999999776482582, Engine.DeltaTime));
					if (Input.MenuUp.Pressed && index > 0)
					{
						index--;
					}
					else if (index < 2)
					{
						if ((Input.MenuDown.Pressed && !Input.MenuDown.Repeating) || Input.MenuConfirm.Pressed)
						{
							index++;
						}
					}
					else if (Input.MenuConfirm.Pressed)
					{
						break;
					}
					yield return null;
				}
				Audio.Play("event:/ui/main/button_lowkey");
			}

			public IEnumerator EaseOut()
			{
				Audio.Play("event:/game/03_resort/memo_out");
				easingOut = true;
				Vector2 from = Position;
				Vector2 to = new Vector2(Engine.Width / 2, -target.Height);
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
					target = VirtualContent.CreateRenderTarget("oshiro-memo", (int)((float)paper.Width * 1.5f), (int)((float)paper.Height * 1.5f));
				}
				Engine.Graphics.GraphicsDevice.SetRenderTarget(target);
				Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
				Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
				paper.Draw(Vector2.Zero, Vector2.Zero, Color.White, 1.5f);
				title.Draw(Vector2.Zero, Vector2.Zero, Color.White, 1.5f);
				text.Draw(new Vector2((float)paper.Width * 1.5f / 2f, 210f), new Vector2(0.5f, 0f), Vector2.One * 0.75f * textDownscale, 1f);
				Draw.SpriteBatch.End();
			}

			public override void Removed(Scene scene)
			{
				if (target != null)
				{
					target.Dispose();
				}
				target = null;
				atlas.Dispose();
				base.Removed(scene);
			}

			public override void SceneEnd(Scene scene)
			{
				if (target != null)
				{
					target.Dispose();
				}
				target = null;
				atlas.Dispose();
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
					Draw.SpriteBatch.Draw((RenderTarget2D)target, Position, target.Bounds, Color.White * alpha, rotation, new Vector2(target.Width, 0f) / 2f, scale, SpriteEffects.None, 0f);
					if (!easingOut)
					{
						GFX.Gui["textboxbutton"].DrawCentered(Position + new Vector2(target.Width / 2 + 40, target.Height + ((timer % 1f < 0.25f) ? 6 : 0)));
					}
				}
			}
		}

		private const string ReadOnceFlag = "memo_read";

		private Player player;

		private MemoPage memo;

		public CS03_Memo(Player player)
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
			if (!Level.Session.GetFlag("memo_read"))
			{
				yield return Textbox.Say("ch3_memo_opening");
				yield return 0.1f;
			}
			memo = new MemoPage();
			base.Scene.Add(memo);
			yield return memo.EaseIn();
			yield return memo.Wait();
			yield return memo.EaseOut();
			memo = null;
			EndCutscene(Level);
		}

		public override void OnEnd(Level level)
		{
			player.StateMachine.Locked = false;
			player.StateMachine.State = 0;
			level.Session.SetFlag("memo_read");
			if (memo != null)
			{
				memo.RemoveSelf();
			}
		}
	}
}
