using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class MiniTextbox : Entity
	{
		public const float TextScale = 0.75f;

		public const float BoxWidth = 1688f;

		public const float BoxHeight = 144f;

		public const float HudElementHeight = 180f;

		private int index;

		private FancyText.Text text;

		private MTexture box;

		private float ease;

		private bool closing;

		private Coroutine routine;

		private Sprite portrait;

		private FancyText.Portrait portraitData;

		private float portraitSize;

		private float portraitScale;

		private SoundSource talkerSfx;

		public static bool Displayed
		{
			get
			{
				foreach (MiniTextbox box in Engine.Scene.Tracker.GetEntities<MiniTextbox>())
				{
					if (!box.closing && box.ease > 0.25f)
					{
						return true;
					}
				}
				return false;
			}
		}

		public MiniTextbox(string dialogId)
		{
			base.Tag = (int)Tags.HUD | (int)Tags.TransitionUpdate;
			portraitSize = 112f;
			box = GFX.Portraits["textbox/default_mini"];
			text = FancyText.Parse(Dialog.Get(dialogId.Trim()), (int)(1688f - portraitSize - 32f), 2);
			foreach (FancyText.Node node in text.Nodes)
			{
				if (node is FancyText.Portrait)
				{
					FancyText.Portrait p = (portraitData = node as FancyText.Portrait);
					portrait = GFX.PortraitsSpriteBank.Create("portrait_" + p.Sprite);
					XmlElement xml = GFX.PortraitsSpriteBank.SpriteData["portrait_" + p.Sprite].Sources[0].XML;
					portraitScale = portraitSize / xml.AttrFloat("size", 160f);
					string textbox = "textbox/" + xml.Attr("textbox", "default") + "_mini";
					if (GFX.Portraits.Has(textbox))
					{
						box = GFX.Portraits[textbox];
					}
					Add(portrait);
				}
			}
			Add(routine = new Coroutine(Routine()));
			routine.UseRawDeltaTime = true;
			Add(new TransitionListener
			{
				OnOutBegin = delegate
				{
					if (!closing)
					{
						routine.Replace(Close());
					}
				}
			});
			if (Level.DialogSnapshot == null)
			{
				Level.DialogSnapshot = Audio.CreateSnapshot("snapshot:/dialogue_in_progress", start: false);
			}
			Audio.ResumeSnapshot(Level.DialogSnapshot);
		}

		private IEnumerator Routine()
		{
			List<Entity> others = base.Scene.Tracker.GetEntities<MiniTextbox>();
			foreach (MiniTextbox other in others)
			{
				if (other != this)
				{
					other.Add(new Coroutine(other.Close()));
				}
			}
			if (others.Count > 0)
			{
				yield return 0.3f;
			}
			while ((ease += Engine.DeltaTime * 4f) < 1f)
			{
				yield return null;
			}
			ease = 1f;
			if (portrait != null)
			{
				string beginAnim = "begin_" + portraitData.Animation;
				if (portrait.Has(beginAnim))
				{
					portrait.Play(beginAnim);
					while (portrait.CurrentAnimationID == beginAnim && portrait.Animating)
					{
						yield return null;
					}
				}
				portrait.Play("talk_" + portraitData.Animation);
				talkerSfx = new SoundSource().Play(portraitData.SfxEvent);
				talkerSfx.Param("dialogue_portrait", portraitData.SfxExpression);
				talkerSfx.Param("dialogue_end", 0f);
				Add(talkerSfx);
			}
			float delay = 0f;
			while (index < text.Nodes.Count)
			{
				if (text.Nodes[index] is FancyText.Char)
				{
					delay += (text.Nodes[index] as FancyText.Char).Delay;
				}
				index++;
				if (delay > 0.016f)
				{
					yield return delay;
					delay = 0f;
				}
			}
			if (portrait != null)
			{
				portrait.Play("idle_" + portraitData.Animation);
			}
			if (talkerSfx != null)
			{
				talkerSfx.Param("dialogue_portrait", 0f);
				talkerSfx.Param("dialogue_end", 1f);
			}
			Audio.EndSnapshot(Level.DialogSnapshot);
			yield return 3f;
			yield return Close();
		}

		private IEnumerator Close()
		{
			if (!closing)
			{
				closing = true;
				while ((ease -= Engine.DeltaTime * 4f) > 0f)
				{
					yield return null;
				}
				ease = 0f;
				RemoveSelf();
			}
		}

		public override void Update()
		{
			if ((base.Scene as Level).RetryPlayerCorpse != null && !closing)
			{
				routine.Replace(Close());
			}
			base.Update();
		}

		public override void Render()
		{
			if (ease <= 0f)
			{
				return;
			}
			Level level = base.Scene as Level;
			if (!level.FrozenOrPaused && level.RetryPlayerCorpse == null && !level.SkippingCutscene)
			{
				Vector2 center = new Vector2(Engine.Width / 2, 72f + ((float)Engine.Width - 1688f) / 4f);
				Vector2 topleft = center + new Vector2(-828f, -56f);
				box.DrawCentered(center, Color.White, new Vector2(1f, ease));
				if (portrait != null)
				{
					portrait.Scale = new Vector2(1f, ease) * portraitScale;
					portrait.RenderPosition = topleft + new Vector2(portraitSize / 2f, portraitSize / 2f);
					portrait.Render();
				}
				text.Draw(new Vector2(topleft.X + portraitSize + 32f, center.Y), new Vector2(0f, 0.5f), new Vector2(1f, ease) * 0.75f, 1f, 0, index);
			}
		}

		public override void Removed(Scene scene)
		{
			Audio.EndSnapshot(Level.DialogSnapshot);
			base.Removed(scene);
		}

		public override void SceneEnd(Scene scene)
		{
			Audio.EndSnapshot(Level.DialogSnapshot);
			base.SceneEnd(scene);
		}
	}
}
