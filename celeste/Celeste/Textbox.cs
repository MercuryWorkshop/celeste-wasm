using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class Textbox : Entity
	{
		private MTexture textbox = GFX.Portraits["textbox/default"];

		private MTexture textboxOverlay;

		private const int textboxInnerWidth = 1688;

		private const int textboxInnerHeight = 272;

		private const float portraitPadding = 16f;

		private const float tweenDuration = 0.4f;

		private const float switchToIdleAnimationDelay = 0.5f;

		private readonly float innerTextPadding;

		private readonly float maxLineWidthNoPortrait;

		private readonly float maxLineWidth;

		private readonly int linesPerPage;

		private const int stopVoiceCharactersEarly = 4;

		private float ease;

		private FancyText.Text text;

		private Func<IEnumerator>[] events;

		private Coroutine runRoutine;

		private Coroutine skipRoutine;

		private PixelFont font;

		private float lineHeight;

		private FancyText.Anchors anchor;

		private FancyText.Portrait portrait;

		private int index;

		private bool waitingForInput;

		private bool disableInput;

		private int shakeSeed;

		private float timer;

		private float gradientFade;

		private bool isInTrigger;

		private bool canSkip = true;

		private bool easingClose;

		private bool easingOpen;

		public Vector2 RenderOffset;

		private bool autoPressContinue;

		private char lastChar;

		private Sprite portraitSprite = new Sprite(null, null);

		private bool portraitExists;

		private bool portraitIdling;

		private float portraitScale = 1.5f;

		private Wiggler portraitWiggle;

		private Sprite portraitGlitchy;

		private bool isPortraitGlitchy;

		private Dictionary<string, SoundSource> talkers = new Dictionary<string, SoundSource>();

		private SoundSource activeTalker;

		private SoundSource phonestatic;

		public bool Opened { get; private set; }

		public int Page { get; private set; }

		public List<FancyText.Node> Nodes => text.Nodes;

		public bool UseRawDeltaTime
		{
			set
			{
				runRoutine.UseRawDeltaTime = value;
			}
		}

		public int Start { get; private set; }

		public string PortraitName
		{
			get
			{
				if (portrait == null || portrait.Sprite == null)
				{
					return "";
				}
				return portrait.Sprite;
			}
		}

		public string PortraitAnimation
		{
			get
			{
				if (portrait == null || portrait.Sprite == null)
				{
					return "";
				}
				return portrait.Animation;
			}
		}

		public Textbox(string dialog, params Func<IEnumerator>[] events)
			: this(dialog, null, events)
		{
		}

		public Textbox(string dialog, Language language, params Func<IEnumerator>[] events)
		{
			base.Tag = (int)Tags.PauseUpdate | (int)Tags.HUD;
			Opened = true;
			font = Dialog.Language.Font;
			lineHeight = Dialog.Language.FontSize.LineHeight - 1;
			portraitSprite.UseRawDeltaTime = true;
			Add(portraitWiggle = Wiggler.Create(0.4f, 4f));
			this.events = events;
			linesPerPage = (int)(240f / lineHeight);
			innerTextPadding = (272f - lineHeight * (float)linesPerPage) / 2f;
			maxLineWidthNoPortrait = 1688f - innerTextPadding * 2f;
			maxLineWidth = maxLineWidthNoPortrait - 240f - 32f;
			text = FancyText.Parse(Dialog.Get(dialog, language), (int)maxLineWidth, linesPerPage, 0f, null, language);
			index = 0;
			Start = 0;
			skipRoutine = new Coroutine(SkipDialog());
			runRoutine = new Coroutine(RunRoutine());
			runRoutine.UseRawDeltaTime = true;
			if (Level.DialogSnapshot == null)
			{
				Level.DialogSnapshot = Audio.CreateSnapshot("snapshot:/dialogue_in_progress", start: false);
			}
			Audio.ResumeSnapshot(Level.DialogSnapshot);
			Add(phonestatic = new SoundSource());
		}

		public void SetStart(int value)
		{
			int num2 = (index = (Start = value));
		}

		private IEnumerator RunRoutine()
		{
			FancyText.Node last = null;
			float delayBuildup = 0f;
			while (index < Nodes.Count)
			{
				FancyText.Node current = Nodes[index];
				float delay = 0f;
				if (current is FancyText.Anchor)
				{
					if (RenderOffset == Vector2.Zero)
					{
						FancyText.Anchors next2 = (current as FancyText.Anchor).Position;
						if (ease >= 1f && next2 != anchor)
						{
							yield return EaseClose(final: false);
						}
						anchor = next2;
					}
				}
				else if (current is FancyText.Portrait)
				{
					FancyText.Portrait next = current as FancyText.Portrait;
					phonestatic.Stop();
					if (ease >= 1f && (portrait == null || next.Sprite != portrait.Sprite || next.Side != portrait.Side))
					{
						yield return EaseClose(final: false);
					}
					textbox = GFX.Portraits["textbox/default"];
					textboxOverlay = null;
					portraitExists = false;
					activeTalker = null;
					isPortraitGlitchy = false;
					XmlElement xml = null;
					if (!string.IsNullOrEmpty(next.Sprite))
					{
						if (GFX.PortraitsSpriteBank.Has(next.SpriteId))
						{
							xml = GFX.PortraitsSpriteBank.SpriteData[next.SpriteId].Sources[0].XML;
						}
						portraitExists = xml != null;
						isPortraitGlitchy = next.Glitchy;
						if (isPortraitGlitchy && portraitGlitchy == null)
						{
							portraitGlitchy = new Sprite(GFX.Portraits, "noise/");
							portraitGlitchy.AddLoop("noise", "", 0.1f);
							portraitGlitchy.Play("noise");
						}
					}
					if (portraitExists)
					{
						if (portrait == null || next.Sprite != portrait.Sprite)
						{
							GFX.PortraitsSpriteBank.CreateOn(portraitSprite, next.SpriteId);
							portraitScale = 240f / (float)xml.AttrInt("size", 160);
							if (!talkers.ContainsKey(next.SfxEvent))
							{
								SoundSource sfx = new SoundSource().Play(next.SfxEvent);
								talkers.Add(next.SfxEvent, sfx);
								Add(sfx);
							}
						}
						if (talkers.ContainsKey(next.SfxEvent))
						{
							activeTalker = talkers[next.SfxEvent];
						}
						string tex = "textbox/" + xml.Attr("textbox", "default");
						textbox = GFX.Portraits[tex];
						if (GFX.Portraits.Has(tex + "_overlay"))
						{
							textboxOverlay = GFX.Portraits[tex + "_overlay"];
						}
						string stat = xml.Attr("phonestatic", "");
						if (!string.IsNullOrEmpty(stat))
						{
							if (stat == "ex")
							{
								phonestatic.Play("event:/char/dialogue/sfx_support/phone_static_ex");
							}
							else if (stat == "mom")
							{
								phonestatic.Play("event:/char/dialogue/sfx_support/phone_static_mom");
							}
						}
						canSkip = false;
						FancyText.Portrait was = portrait;
						portrait = next;
						if (next.Pop)
						{
							portraitWiggle.Start();
						}
						if (was == null || was.Sprite != next.Sprite || was.Animation != next.Animation)
						{
							if (portraitSprite.Has(next.BeginAnimation))
							{
								portraitSprite.Play(next.BeginAnimation, restart: true);
								yield return EaseOpen();
								while (portraitSprite.CurrentAnimationID == next.BeginAnimation && portraitSprite.Animating)
								{
									yield return null;
								}
							}
							if (portraitSprite.Has(next.IdleAnimation))
							{
								portraitIdling = true;
								portraitSprite.Play(next.IdleAnimation, restart: true);
							}
						}
						yield return EaseOpen();
						canSkip = true;
					}
					else
					{
						portrait = null;
						yield return EaseOpen();
					}
				}
				else if (current is FancyText.NewPage)
				{
					PlayIdleAnimation();
					if (ease >= 1f)
					{
						waitingForInput = true;
						yield return 0.1f;
						while (!ContinuePressed())
						{
							yield return null;
						}
						waitingForInput = false;
					}
					Start = index + 1;
					Page++;
				}
				else if (current is FancyText.Wait)
				{
					PlayIdleAnimation();
					delay = (current as FancyText.Wait).Duration;
				}
				else if (current is FancyText.Trigger)
				{
					isInTrigger = true;
					PlayIdleAnimation();
					FancyText.Trigger trigger = current as FancyText.Trigger;
					if (!trigger.Silent)
					{
						yield return EaseClose(final: false);
					}
					int triggerIndex = trigger.Index;
					if (events != null && triggerIndex >= 0 && triggerIndex < events.Length)
					{
						yield return events[triggerIndex]();
					}
					isInTrigger = false;
				}
				else if (current is FancyText.Char)
				{
					FancyText.Char ch = current as FancyText.Char;
					lastChar = (char)ch.Character;
					if (ease < 1f)
					{
						yield return EaseOpen();
					}
					bool idling = false;
					if (index - 5 > Start)
					{
						for (int i = index; i < Math.Min(index + 4, Nodes.Count); i++)
						{
							if (Nodes[i] is FancyText.NewPage)
							{
								idling = true;
								PlayIdleAnimation();
							}
						}
					}
					if (!idling && !ch.IsPunctuation)
					{
						PlayTalkAnimation();
					}
					if (last != null && last is FancyText.NewPage)
					{
						index--;
						yield return 0.2f;
						index++;
					}
					delay = ch.Delay + delayBuildup;
				}
				last = current;
				index++;
				if (delay < 0.016f)
				{
					delayBuildup += delay;
					continue;
				}
				delayBuildup = 0f;
				if (delay > 0.5f)
				{
					PlayIdleAnimation();
				}
				yield return delay;
			}
			PlayIdleAnimation();
			if (ease > 0f)
			{
				waitingForInput = true;
				while (!ContinuePressed())
				{
					yield return null;
				}
				waitingForInput = false;
				Start = Nodes.Count;
				yield return EaseClose(final: true);
			}
			Close();
		}

		private void PlayIdleAnimation()
		{
			StopTalker();
			if (!portraitIdling && portraitSprite != null && portrait != null && portraitSprite.Has(portrait.IdleAnimation))
			{
				portraitSprite.Play(portrait.IdleAnimation);
				portraitIdling = true;
			}
		}

		private void StopTalker()
		{
			if (activeTalker != null)
			{
				activeTalker.Param("dialogue_portrait", 0f);
				activeTalker.Param("dialogue_end", 1f);
			}
		}

		private void PlayTalkAnimation()
		{
			StartTalker();
			if (portraitIdling && portraitSprite != null && portrait != null && portraitSprite.Has(portrait.TalkAnimation))
			{
				portraitSprite.Play(portrait.TalkAnimation);
				portraitIdling = false;
			}
		}

		private void StartTalker()
		{
			if (activeTalker != null)
			{
				activeTalker.Param("dialogue_portrait", (portrait == null) ? 1 : portrait.SfxExpression);
				activeTalker.Param("dialogue_end", 0f);
			}
		}

		private IEnumerator EaseOpen()
		{
			if (ease < 1f)
			{
				easingOpen = true;
				if (portrait != null && portrait.Sprite.IndexOf("madeline", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					Audio.Play("event:/ui/game/textbox_madeline_in");
				}
				else
				{
					Audio.Play("event:/ui/game/textbox_other_in");
				}
				while ((ease += (runRoutine.UseRawDeltaTime ? Engine.RawDeltaTime : Engine.DeltaTime) / 0.4f) < 1f)
				{
					gradientFade = Math.Max(gradientFade, ease);
					yield return null;
				}
				ease = (gradientFade = 1f);
				easingOpen = false;
			}
		}

		private IEnumerator EaseClose(bool final)
		{
			easingClose = true;
			if (portrait != null && portrait.Sprite.IndexOf("madeline", StringComparison.InvariantCultureIgnoreCase) >= 0)
			{
				Audio.Play("event:/ui/game/textbox_madeline_out");
			}
			else
			{
				Audio.Play("event:/ui/game/textbox_other_out");
			}
			while ((ease -= (runRoutine.UseRawDeltaTime ? Engine.RawDeltaTime : Engine.DeltaTime) / 0.4f) > 0f)
			{
				if (final)
				{
					gradientFade = ease;
				}
				yield return null;
			}
			ease = 0f;
			easingClose = false;
		}

		private IEnumerator SkipDialog()
		{
			while (true)
			{
				if (!waitingForInput && canSkip && !easingOpen && !easingClose && ContinuePressed())
				{
					StopTalker();
					disableInput = true;
					while (!waitingForInput && canSkip && !easingOpen && !easingClose && !isInTrigger && !runRoutine.Finished)
					{
						runRoutine.Update();
					}
				}
				yield return null;
				disableInput = false;
			}
		}

		public bool SkipToPage(int page)
		{
			autoPressContinue = true;
			while (Page != page && !runRoutine.Finished)
			{
				Update();
			}
			autoPressContinue = false;
			Update();
			while (Opened && ease < 1f)
			{
				Update();
			}
			if (Page == page)
			{
				return Opened;
			}
			return false;
		}

		public void Close()
		{
			Opened = false;
			if (base.Scene != null)
			{
				base.Scene.Remove(this);
			}
		}

		private bool ContinuePressed()
		{
			if (!autoPressContinue)
			{
				if (Input.MenuConfirm.Pressed || Input.MenuCancel.Pressed)
				{
					return !disableInput;
				}
				return false;
			}
			return true;
		}

		public override void Update()
		{
			if (base.Scene is Level level && (level.FrozenOrPaused || level.RetryPlayerCorpse != null))
			{
				return;
			}
			if (!autoPressContinue)
			{
				skipRoutine.Update();
			}
			runRoutine.Update();
			if (base.Scene != null && base.Scene.OnInterval(0.05f))
			{
				shakeSeed = Calc.Random.Next();
			}
			if (portraitSprite != null && ease >= 1f)
			{
				portraitSprite.Update();
			}
			if (portraitGlitchy != null && ease >= 1f)
			{
				portraitGlitchy.Update();
			}
			timer += Engine.DeltaTime;
			portraitWiggle.Update();
			int to = Math.Min(index, Nodes.Count);
			for (int i = Start; i < to; i++)
			{
				if (Nodes[i] is FancyText.Char)
				{
					FancyText.Char node = Nodes[i] as FancyText.Char;
					if (node.Fade < 1f)
					{
						node.Fade = Calc.Clamp(node.Fade + 8f * Engine.DeltaTime, 0f, 1f);
					}
				}
			}
		}

		public override void Render()
		{
			if (base.Scene is Level level && (level.FrozenOrPaused || level.RetryPlayerCorpse != null || level.SkippingCutscene))
			{
				return;
			}
			float tween = Ease.CubeInOut(ease);
			if (tween < 0.05f)
			{
				return;
			}
			float screenPadding = 116f;
			Vector2 pos = new Vector2(screenPadding, screenPadding / 2f) + RenderOffset;
			if (RenderOffset == Vector2.Zero)
			{
				if (anchor == FancyText.Anchors.Bottom)
				{
					pos = new Vector2(screenPadding, 1080f - screenPadding / 2f - 272f);
				}
				else if (anchor == FancyText.Anchors.Middle)
				{
					pos = new Vector2(screenPadding, 404f);
				}
				pos.Y += (int)(136f * (1f - tween));
			}
			textbox.DrawCentered(pos + new Vector2(1688f, 272f * tween) / 2f, Color.White, new Vector2(1f, tween));
			if (waitingForInput)
			{
				float offset = ((portrait == null || PortraitSide(portrait) < 0) ? 1688f : 1432f);
				Vector2 at = new Vector2(pos.X + offset, pos.Y + 272f) + new Vector2(-48f, -40 + ((timer % 1f < 0.25f) ? 6 : 0));
				GFX.Gui["textboxbutton"].DrawCentered(at);
			}
			if (portraitExists)
			{
				if (PortraitSide(portrait) > 0)
				{
					portraitSprite.Position = new Vector2(pos.X + 1688f - 240f - 16f, pos.Y);
					portraitSprite.Scale.X = 0f - portraitScale;
				}
				else
				{
					portraitSprite.Position = new Vector2(pos.X + 16f, pos.Y);
					portraitSprite.Scale.X = portraitScale;
				}
				portraitSprite.Scale.X *= ((!portrait.Flipped) ? 1 : (-1));
				portraitSprite.Scale.Y = portraitScale * ((272f * tween - 32f) / 240f) * (float)((!portrait.UpsideDown) ? 1 : (-1));
				portraitSprite.Scale *= 0.9f + portraitWiggle.Value * 0.1f;
				portraitSprite.Position += new Vector2(120f, 272f * tween * 0.5f);
				portraitSprite.Color = Color.White * tween;
				if (Math.Abs(portraitSprite.Scale.Y) > 0.05f)
				{
					portraitSprite.Render();
					if (isPortraitGlitchy && portraitGlitchy != null)
					{
						portraitGlitchy.Position = portraitSprite.Position;
						portraitGlitchy.Origin = portraitSprite.Origin;
						portraitGlitchy.Scale = portraitSprite.Scale;
						portraitGlitchy.Color = Color.White * 0.2f * tween;
						portraitGlitchy.Render();
					}
				}
			}
			if (textboxOverlay != null)
			{
				int sx = 1;
				if (portrait != null && PortraitSide(portrait) > 0)
				{
					sx = -1;
				}
				textboxOverlay.DrawCentered(pos + new Vector2(1688f, 272f * tween) / 2f, Color.White, new Vector2(sx, tween));
			}
			Calc.PushRandom(shakeSeed);
			int lines = 1;
			for (int i = Start; i < text.Nodes.Count; i++)
			{
				if (text.Nodes[i] is FancyText.NewLine)
				{
					lines++;
				}
				else if (text.Nodes[i] is FancyText.NewPage)
				{
					break;
				}
			}
			Vector2 textPadding = new Vector2(innerTextPadding + ((portrait != null && PortraitSide(portrait) < 0) ? 256f : 0f), innerTextPadding);
			Vector2 center = new Vector2((portrait == null) ? maxLineWidthNoPortrait : maxLineWidth, (float)linesPerPage * lineHeight * tween) / 2f;
			float s = ((lines >= 4) ? 0.75f : 1f);
			text.Draw(pos + textPadding + center, new Vector2(0.5f, 0.5f), new Vector2(1f, tween) * s, tween, Start);
			Calc.PopRandom();
		}

		public int PortraitSide(FancyText.Portrait portrait)
		{
			if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
			{
				return -portrait.Side;
			}
			return portrait.Side;
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

		public static IEnumerator Say(string dialog, params Func<IEnumerator>[] events)
		{
			Textbox textbox = new Textbox(dialog, events);
			Engine.Scene.Add(textbox);
			while (textbox.Opened)
			{
				yield return null;
			}
		}

		public static IEnumerator SayWhileFrozen(string dialog, params Func<IEnumerator>[] events)
		{
			Textbox textbox = new Textbox(dialog, events);
			textbox.Tag |= Tags.FrozenUpdate;
			Engine.Scene.Add(textbox);
			while (textbox.Opened)
			{
				yield return null;
			}
		}
	}
}
