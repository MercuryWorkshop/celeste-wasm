using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS06_Campfire : CutsceneEntity
	{
		private class Option
		{
			public Question Question;

			public string Goto;

			public List<Question> OnlyAppearIfAsked;

			public List<Question> DoNotAppearIfAsked;

			public bool CanRepeat;

			public float Highlight;

			public const float Width = 1400f;

			public const float Height = 140f;

			public const float Padding = 20f;

			public const float TextScale = 0.7f;

			public Option(Question question, string go)
			{
				Question = question;
				Goto = go;
			}

			public Option Require(params Question[] onlyAppearIfAsked)
			{
				OnlyAppearIfAsked = new List<Question>(onlyAppearIfAsked);
				return this;
			}

			public Option ExcludedBy(params Question[] doNotAppearIfAsked)
			{
				DoNotAppearIfAsked = new List<Question>(doNotAppearIfAsked);
				return this;
			}

			public Option Repeatable()
			{
				CanRepeat = true;
				return this;
			}

			public bool CanAsk(HashSet<Question> asked)
			{
				if (!CanRepeat && asked.Contains(Question))
				{
					return false;
				}
				if (OnlyAppearIfAsked != null)
				{
					foreach (Question other2 in OnlyAppearIfAsked)
					{
						if (!asked.Contains(other2))
						{
							return false;
						}
					}
				}
				if (DoNotAppearIfAsked != null)
				{
					bool allExclusionsMet = true;
					foreach (Question other in DoNotAppearIfAsked)
					{
						if (!asked.Contains(other))
						{
							allExclusionsMet = false;
							break;
						}
					}
					if (allExclusionsMet)
					{
						return false;
					}
				}
				return true;
			}

			public void Update()
			{
				Question.Portrait.Update();
			}

			public void Render(Vector2 position, float ease)
			{
				float tween = Ease.CubeOut(ease);
				float highlight = Ease.CubeInOut(Highlight);
				position.Y += -32f * (1f - tween);
				position.X += highlight * 32f;
				Color bgColor = Color.Lerp(Color.Gray, Color.White, highlight) * tween;
				float txtAlpha = MathHelper.Lerp(0.6f, 1f, highlight) * tween;
				Color portraitColor = Color.White * (0.5f + highlight * 0.5f);
				GFX.Portraits[Question.Textbox].Draw(position, Vector2.Zero, bgColor);
				Facings side = Question.PortraitSide;
				if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
				{
					side = (Facings)(0 - side);
				}
				float portraitSize = 100f;
				Question.Portrait.Scale = Vector2.One * (portraitSize / Question.PortraitSize);
				if (side == Facings.Right)
				{
					Question.Portrait.Position = position + new Vector2(1380f - portraitSize * 0.5f, 70f);
					Question.Portrait.Scale.X *= -1f;
				}
				else
				{
					Question.Portrait.Position = position + new Vector2(20f + portraitSize * 0.5f, 70f);
				}
				Question.Portrait.Color = portraitColor * tween;
				Question.Portrait.Render();
				float textOffset = (140f - ActiveFont.LineHeight * 0.7f) / 2f;
				Vector2 pos = new Vector2(0f, position.Y + 70f);
				Vector2 justify = new Vector2(0f, 0.5f);
				if (side == Facings.Right)
				{
					justify.X = 1f;
					pos.X = position.X + 1400f - 20f - textOffset - portraitSize;
				}
				else
				{
					pos.X = position.X + 20f + textOffset + portraitSize;
				}
				Question.AskText.Draw(pos, justify, Vector2.One * 0.7f, txtAlpha);
			}
		}

		private class Question
		{
			public string Ask;

			public string Answer;

			public string Textbox;

			public FancyText.Text AskText;

			public Sprite Portrait;

			public Facings PortraitSide;

			public float PortraitSize;

			public Question(string id)
			{
				int maxLineWidth = 1828;
				Ask = "ch6_theo_ask_" + id;
				Answer = "ch6_theo_say_" + id;
				AskText = FancyText.Parse(Dialog.Get(Ask), maxLineWidth, -1);
				foreach (FancyText.Node node in AskText.Nodes)
				{
					if (node is FancyText.Portrait)
					{
						FancyText.Portrait data = node as FancyText.Portrait;
						Portrait = GFX.PortraitsSpriteBank.Create(data.SpriteId);
						Portrait.Play(data.IdleAnimation);
						PortraitSide = (Facings)data.Side;
						Textbox = "textbox/" + data.Sprite + "_ask";
						XmlElement xml = GFX.PortraitsSpriteBank.SpriteData[data.SpriteId].Sources[0].XML;
						if (xml != null)
						{
							PortraitSize = xml.AttrInt("size", 160);
						}
						break;
					}
				}
			}
		}

		public const string Flag = "campfire_chat";

		public const string DuskBackgroundFlag = "duskbg";

		public const string StarsBackgrundFlag = "starsbg";

		private NPC theo;

		private Player player;

		private Bonfire bonfire;

		private Plateau plateau;

		private Vector2 cameraStart;

		private Vector2 playerCampfirePosition;

		private Vector2 theoCampfirePosition;

		private Selfie selfie;

		private float optionEase;

		private Dictionary<string, Option[]> nodes = new Dictionary<string, Option[]>();

		private HashSet<Question> asked = new HashSet<Question>();

		private List<Option> currentOptions = new List<Option>();

		private int currentOptionIndex;

		public CS06_Campfire(NPC theo, Player player)
		{
			base.Tag = Tags.HUD;
			this.theo = theo;
			this.player = player;
			Question outfor = new Question("outfor");
			Question temple = new Question("temple");
			Question explain = new Question("explain");
			Question thankyou = new Question("thankyou");
			Question why = new Question("why");
			Question depression = new Question("depression");
			Question defense = new Question("defense");
			Question vacation = new Question("vacation");
			Question trust = new Question("trust");
			Question family = new Question("family");
			Question grandpa = new Question("grandpa");
			Question tips = new Question("tips");
			Question selfie = new Question("selfie");
			Question sleep = new Question("sleep");
			Question sleepConfirm = new Question("sleep_confirm");
			Question sleepCancel = new Question("sleep_cancel");
			nodes.Add("start", new Option[14]
			{
				new Option(outfor, "start").ExcludedBy(why),
				new Option(temple, "start").Require(trust),
				new Option(trust, "start").Require(explain),
				new Option(family, "start").Require(trust, why),
				new Option(grandpa, "start").Require(family, defense),
				new Option(tips, "start").Require(grandpa),
				new Option(explain, "start"),
				new Option(thankyou, "start").Require(explain),
				new Option(why, "start").Require(explain, trust),
				new Option(depression, "start").Require(why),
				new Option(defense, "start").Require(depression),
				new Option(vacation, "start").Require(depression),
				new Option(selfie, "").Require(defense, grandpa),
				new Option(sleep, "sleep").Require(why).ExcludedBy(defense, grandpa).Repeatable()
			});
			nodes.Add("sleep", new Option[2]
			{
				new Option(sleepCancel, "start").Repeatable(),
				new Option(sleepConfirm, "")
			});
		}

		public override void OnBegin(Level level)
		{
			Audio.SetMusic(null, startPlaying: false, allowFadeOut: false);
			level.SnapColorGrade(null);
			level.Bloom.Base = 0f;
			level.Session.SetFlag("duskbg");
			plateau = base.Scene.Entities.FindFirst<Plateau>();
			bonfire = base.Scene.Tracker.GetEntity<Bonfire>();
			level.Camera.Position = new Vector2(level.Bounds.Left, bonfire.Y - 144f);
			level.ZoomSnap(new Vector2(80f, 120f), 2f);
			cameraStart = level.Camera.Position;
			theo.X = level.Camera.X - 48f;
			theoCampfirePosition = new Vector2(bonfire.X - 16f, bonfire.Y);
			player.Light.Alpha = 0f;
			player.X = level.Bounds.Left - 40;
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			playerCampfirePosition = new Vector2(bonfire.X + 20f, bonfire.Y);
			if (level.Session.GetFlag("campfire_chat"))
			{
				WasSkipped = true;
				level.ResetZoom();
				level.EndCutscene();
				EndCutscene(level);
			}
			else
			{
				Add(new Coroutine(Cutscene(level)));
			}
		}

		private IEnumerator PlayerLightApproach()
		{
			while (player.Light.Alpha < 1f)
			{
				player.Light.Alpha = Calc.Approach(player.Light.Alpha, 1f, Engine.DeltaTime * 2f);
				yield return null;
			}
		}

		private IEnumerator Cutscene(Level level)
		{
			yield return 0.1f;
			Add(new Coroutine(PlayerLightApproach()));
			CS06_Campfire cS06_Campfire = this;
			Coroutine component;
			Coroutine camTo = (component = new Coroutine(CutsceneEntity.CameraTo(new Vector2(level.Camera.X + 90f, level.Camera.Y), 6f, Ease.CubeIn)));
			cS06_Campfire.Add(component);
			player.DummyAutoAnimate = false;
			player.Sprite.Play("carryTheoWalk");
			for (float p = 0f; p < 3.5f; p += Engine.DeltaTime)
			{
				SpotlightWipe.FocusPoint = new Vector2(40f, 120f);
				player.NaiveMove(new Vector2(32f * Engine.DeltaTime, 0f));
				yield return null;
			}
			player.Sprite.Play("carryTheoCollapse");
			Audio.Play("event:/char/madeline/theo_collapse", player.Position);
			yield return 0.3f;
			Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			Vector2 at = player.Position + new Vector2(16f, 1f);
			Level.ParticlesFG.Emit(Payphone.P_Snow, 2, at, Vector2.UnitX * 4f);
			Level.ParticlesFG.Emit(Payphone.P_SnowB, 12, at, Vector2.UnitX * 10f);
			yield return 0.7f;
			FadeWipe fade = new FadeWipe(level, wipeIn: false)
			{
				Duration = 1.5f,
				EndTimer = 2.5f
			};
			yield return fade.Wait();
			bonfire.SetMode(Bonfire.Mode.Lit);
			yield return 2.45f;
			camTo.Cancel();
			theo.Position = theoCampfirePosition;
			theo.Sprite.Play("sleep");
			theo.Sprite.SetAnimationFrame(theo.Sprite.CurrentAnimationTotalFrames - 1);
			player.Position = playerCampfirePosition;
			player.Facing = Facings.Left;
			player.Sprite.Play("asleep");
			level.Session.SetFlag("starsbg");
			level.Session.SetFlag("duskbg", setTo: false);
			fade.EndTimer = 0f;
			new FadeWipe(level, wipeIn: true);
			yield return null;
			level.ResetZoom();
			level.Camera.Position = new Vector2(bonfire.X - 160f, bonfire.Y - 140f);
			yield return 3f;
			Audio.SetMusic("event:/music/lvl6/madeline_and_theo");
			yield return 1.5f;
			Add(Wiggler.Create(0.6f, 3f, delegate(float v)
			{
				theo.Sprite.Scale = Vector2.One * (1f + 0.1f * v);
			}, start: true, removeSelfOnFinish: true));
			Level.Particles.Emit(NPC01_Theo.P_YOLO, 4, theo.Position + new Vector2(-4f, -14f), Vector2.One * 3f);
			yield return 0.5f;
			theo.Sprite.Play("wakeup");
			yield return 1f;
			player.Sprite.Play("halfWakeUp");
			yield return 0.25f;
			yield return Textbox.Say("ch6_theo_intro");
			string node = "start";
			while (!string.IsNullOrEmpty(node) && nodes.ContainsKey(node))
			{
				currentOptionIndex = 0;
				currentOptions = new List<Option>();
				Option[] array = nodes[node];
				foreach (Option option in array)
				{
					if (option.CanAsk(asked))
					{
						currentOptions.Add(option);
					}
				}
				if (currentOptions.Count <= 0)
				{
					break;
				}
				Audio.Play("event:/ui/game/chatoptions_appear");
				while ((optionEase += Engine.DeltaTime * 4f) < 1f)
				{
					yield return null;
				}
				optionEase = 1f;
				yield return 0.25f;
				while (!Input.MenuConfirm.Pressed)
				{
					if (Input.MenuUp.Pressed && currentOptionIndex > 0)
					{
						Audio.Play("event:/ui/game/chatoptions_roll_up");
						currentOptionIndex--;
					}
					else if (Input.MenuDown.Pressed && currentOptionIndex < currentOptions.Count - 1)
					{
						Audio.Play("event:/ui/game/chatoptions_roll_down");
						currentOptionIndex++;
					}
					yield return null;
				}
				Audio.Play("event:/ui/game/chatoptions_select");
				while ((optionEase -= Engine.DeltaTime * 4f) > 0f)
				{
					yield return null;
				}
				Option selected = currentOptions[currentOptionIndex];
				asked.Add(selected.Question);
				currentOptions = null;
				yield return Textbox.Say(selected.Question.Answer, WaitABit, SelfieSequence, BeerSequence);
				node = selected.Goto;
				if (string.IsNullOrEmpty(node))
				{
					break;
				}
			}
			FadeWipe wipe = new FadeWipe(level, wipeIn: false);
			wipe.Duration = 3f;
			yield return wipe.Wait();
			EndCutscene(level);
		}

		private IEnumerator WaitABit()
		{
			yield return 0.8f;
		}

		private IEnumerator SelfieSequence()
		{
			Add(new Coroutine(Level.ZoomTo(new Vector2(160f, 105f), 2f, 0.5f)));
			yield return 0.1f;
			theo.Sprite.Play("idle");
			Add(Alarm.Create(Alarm.AlarmMode.Oneshot, delegate
			{
				theo.Sprite.Scale.X = -1f;
			}, 0.25f, start: true));
			player.DummyAutoAnimate = true;
			yield return player.DummyWalkToExact((int)(theo.X + 5f), walkBackwards: false, 0.7f);
			yield return 0.2f;
			Audio.Play("event:/game/02_old_site/theoselfie_foley", theo.Position);
			theo.Sprite.Play("takeSelfie");
			yield return 1f;
			selfie = new Selfie(SceneAs<Level>());
			base.Scene.Add(selfie);
			yield return selfie.PictureRoutine("selfieCampfire");
			selfie = null;
			yield return 0.5f;
			yield return Level.ZoomBack(0.5f);
			yield return 0.2f;
			theo.Sprite.Scale.X = 1f;
			yield return player.DummyWalkToExact((int)playerCampfirePosition.X, walkBackwards: false, 0.7f);
			theo.Sprite.Play("wakeup");
			yield return 0.1;
			player.DummyAutoAnimate = false;
			player.Facing = Facings.Left;
			player.Sprite.Play("sleep");
			yield return 2f;
			player.Sprite.Play("halfWakeUp");
		}

		private IEnumerator BeerSequence()
		{
			yield return 0.5f;
		}

		public override void OnEnd(Level level)
		{
			if (!WasSkipped)
			{
				level.ZoomSnap(new Vector2(160f, 120f), 2f);
				FadeWipe wipe = new FadeWipe(level, wipeIn: true);
				wipe.Duration = 3f;
				Coroutine zoom = new Coroutine(level.ZoomBack(wipe.Duration));
				wipe.OnUpdate = delegate
				{
					zoom.Update();
				};
			}
			if (selfie != null)
			{
				selfie.RemoveSelf();
			}
			level.Session.SetFlag("campfire_chat");
			level.Session.SetFlag("starsbg", setTo: false);
			level.Session.SetFlag("duskbg", setTo: false);
			level.Session.Dreaming = true;
			level.Add(new StarJumpController());
			level.Add(new CS06_StarJumpEnd(theo, player, playerCampfirePosition, cameraStart));
			level.Add(new FlyFeather(level.LevelOffset + new Vector2(272f, 2616f), shielded: false, singleUse: false));
			SetBloom(1f);
			bonfire.Activated = false;
			bonfire.SetMode(Bonfire.Mode.Lit);
			theo.Sprite.Play("sleep");
			theo.Sprite.SetAnimationFrame(theo.Sprite.CurrentAnimationTotalFrames - 1);
			theo.Sprite.Scale.X = 1f;
			theo.Position = theoCampfirePosition;
			player.Sprite.Play("asleep");
			player.Position = playerCampfirePosition;
			player.StateMachine.Locked = false;
			player.StateMachine.State = 15;
			player.Speed = Vector2.Zero;
			player.Facing = Facings.Left;
			level.Camera.Position = player.CameraTarget;
			if (WasSkipped)
			{
				player.StateMachine.State = 0;
			}
			RemoveSelf();
		}

		private void SetBloom(float add)
		{
			Level.Session.BloomBaseAdd = add;
			Level.Bloom.Base = AreaData.Get(Level).BloomBase + add;
		}

		public override void Update()
		{
			if (currentOptions != null)
			{
				for (int i = 0; i < currentOptions.Count; i++)
				{
					currentOptions[i].Update();
					currentOptions[i].Highlight = Calc.Approach(currentOptions[i].Highlight, (currentOptionIndex == i) ? 1 : 0, Engine.DeltaTime * 4f);
				}
			}
			base.Update();
		}

		public override void Render()
		{
			if (Level.Paused || currentOptions == null)
			{
				return;
			}
			int index = 0;
			foreach (Option currentOption in currentOptions)
			{
				Vector2 pos = new Vector2(260f, 120f + 160f * (float)index);
				currentOption.Render(pos, optionEase);
				index++;
			}
		}
	}
}
