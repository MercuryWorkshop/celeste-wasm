using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS08_Ending : CutsceneEntity
	{
		public class TimeDisplay : Component
		{
			public Vector2 Position;

			public string Time;

			public Vector2 RenderPosition => (((base.Entity != null) ? base.Entity.Position : Vector2.Zero) + Position).Round();

			public TimeDisplay(string time)
				: base(active: true, visible: true)
			{
				Time = time;
			}

			public override void Render()
			{
				SpeedrunTimerDisplay.DrawTime(RenderPosition, Time);
			}
		}

		private Player player;

		private NPC08_Granny granny;

		private NPC08_Theo theo;

		private BadelineDummy badeline;

		private Entity oshiro;

		private Image vignette;

		private Image vignettebg;

		private string endingDialog;

		private float fade;

		private bool showVersion;

		private float versionAlpha;

		private Coroutine cutscene;

		private string version = Celeste.Instance.Version.ToString();

		public CS08_Ending()
			: base(fadeInOnSkip: false, endingChapterAfter: true)
		{
			base.Tag = (int)Tags.HUD | (int)Tags.PauseUpdate;
			RemoveOnSkipped = false;
		}

		public override void OnBegin(Level level)
		{
			level.SaveQuitDisabled = true;
			string img = "";
			int count = SaveData.Instance.TotalStrawberries;
			if (count < 20)
			{
				img = "final1";
				endingDialog = "EP_PIE_DISAPPOINTED";
			}
			else if (count < 50)
			{
				img = "final2";
				endingDialog = "EP_PIE_GROSSED_OUT";
			}
			else if (count < 90)
			{
				img = "final3";
				endingDialog = "EP_PIE_OKAY";
			}
			else if (count < 150)
			{
				img = "final4";
				endingDialog = "EP_PIE_REALLY_GOOD";
			}
			else
			{
				img = "final5";
				endingDialog = "EP_PIE_AMAZING";
			}
			Add(vignettebg = new Image(GFX.Portraits["finalbg"]));
			vignettebg.Visible = false;
			Add(vignette = new Image(GFX.Portraits[img]));
			vignette.Visible = false;
			vignette.CenterOrigin();
			vignette.Position = Celeste.TargetCenter;
			Add(cutscene = new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			level.ZoomSnap(new Vector2(164f, 120f), 2f);
			level.Wipe.Cancel();
			new FadeWipe(level, wipeIn: true);
			while (player == null)
			{
				granny = level.Entities.FindFirst<NPC08_Granny>();
				theo = level.Entities.FindFirst<NPC08_Theo>();
				player = level.Tracker.GetEntity<Player>();
				yield return null;
			}
			player.StateMachine.State = 11;
			yield return 1f;
			yield return player.DummyWalkToExact((int)player.X + 16);
			yield return 0.25f;
			yield return Textbox.Say("EP_CABIN", BadelineEmerges, OshiroEnters, OshiroSettles, MaddyTurns);
			FadeWipe wipe = new FadeWipe(Level, wipeIn: false);
			wipe.Duration = 1.5f;
			yield return wipe.Wait();
			fade = 1f;
			yield return Textbox.Say("EP_PIE_START");
			yield return 0.5f;
			vignettebg.Visible = true;
			vignette.Visible = true;
			vignettebg.Color = Color.Black;
			vignette.Color = Color.White * 0f;
			Add(vignette);
			float p6;
			for (p6 = 0f; p6 < 1f; p6 += Engine.DeltaTime)
			{
				vignette.Color = Color.White * Ease.CubeIn(p6);
				vignette.Scale = Vector2.One * (1f + 0.25f * (1f - p6));
				vignette.Rotation = 0.05f * (1f - p6);
				yield return null;
			}
			vignette.Color = Color.White;
			vignettebg.Color = Color.White;
			yield return 2f;
			p6 = 1f;
			float p4;
			for (p4 = 0f; p4 < 1f; p4 += Engine.DeltaTime / p6)
			{
				float e = Ease.CubeOut(p4);
				vignette.Position = Vector2.Lerp(Celeste.TargetCenter, Celeste.TargetCenter + new Vector2(0f, 140f), e);
				vignette.Scale = Vector2.One * (0.65f + 0.35f * (1f - e));
				vignette.Rotation = -0.025f * e;
				yield return null;
			}
			yield return Textbox.Say(endingDialog);
			yield return 0.25f;
			p6 = 2f;
			Vector2 posFrom = vignette.Position;
			p4 = vignette.Rotation;
			float scaleFrom = vignette.Scale.X;
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / p6)
			{
				float e2 = Ease.CubeOut(p);
				vignette.Position = Vector2.Lerp(posFrom, Celeste.TargetCenter, e2);
				vignette.Scale = Vector2.One * MathHelper.Lerp(scaleFrom, 1f, e2);
				vignette.Rotation = MathHelper.Lerp(p4, 0f, e2);
				yield return null;
			}
			EndCutscene(level, removeSelf: false);
		}

		public override void OnEnd(Level level)
		{
			vignette.Visible = true;
			vignette.Color = Color.White;
			vignette.Position = Celeste.TargetCenter;
			vignette.Scale = Vector2.One;
			vignette.Rotation = 0f;
			if (player != null)
			{
				player.Speed = Vector2.Zero;
			}
			base.Scene.Entities.FindFirst<Textbox>()?.RemoveSelf();
			cutscene.RemoveSelf();
			Add(new Coroutine(EndingRoutine()));
		}

		private IEnumerator EndingRoutine()
		{
			Level.InCutscene = true;
			Level.PauseLock = true;
			yield return 0.5f;
			TimeSpan timespan = TimeSpan.FromTicks(SaveData.Instance.Time);
			string gameTime = (int)timespan.TotalHours + timespan.ToString("\\:mm\\:ss\\.fff");
			StrawberriesCounter strawbs = new StrawberriesCounter(centeredX: true, SaveData.Instance.TotalStrawberries, 175, showOutOf: true);
			DeathsCounter deaths = new DeathsCounter(AreaMode.Normal, centeredX: true, SaveData.Instance.TotalDeaths);
			TimeDisplay time = new TimeDisplay(gameTime);
			float timeWidth = SpeedrunTimerDisplay.GetTimeWidth(gameTime);
			Add(strawbs);
			Add(deaths);
			Add(time);
			Vector2 from = new Vector2(960f, 1180f);
			Vector2 to = new Vector2(960f, 940f);
			for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.5f)
			{
				Vector2 lerp = Vector2.Lerp(from, to, Ease.CubeOut(p));
				strawbs.Position = lerp + new Vector2(-170f, 0f);
				deaths.Position = lerp + new Vector2(170f, 0f);
				time.Position = lerp + new Vector2((0f - timeWidth) / 2f, 100f);
				yield return null;
			}
			showVersion = true;
			yield return 0.25f;
			while (!Input.MenuConfirm.Pressed)
			{
				yield return null;
			}
			showVersion = false;
			yield return 0.25f;
			Level.CompleteArea(spotlightWipe: false);
		}

		private IEnumerator MaddyTurns()
		{
			yield return 0.1f;
			player.Facing = (Facings)(0 - player.Facing);
			yield return 0.1f;
		}

		private IEnumerator BadelineEmerges()
		{
			Level.Displacement.AddBurst(player.Center, 0.5f, 8f, 32f, 0.5f);
			Level.Session.Inventory.Dashes = 1;
			player.Dashes = 1;
			Level.Add(badeline = new BadelineDummy(player.Position));
			Audio.Play("event:/char/badeline/maddy_split", player.Position);
			badeline.Sprite.Scale.X = 1f;
			yield return badeline.FloatTo(player.Position + new Vector2(-12f, -16f), 1, faceDirection: false);
		}

		private IEnumerator OshiroEnters()
		{
			FadeWipe wipe = new FadeWipe(Level, wipeIn: false);
			wipe.Duration = 1.5f;
			yield return wipe.Wait();
			fade = 1f;
			yield return 0.25f;
			float playerWas = player.X;
			player.X = granny.X + 8f;
			badeline.X = player.X + 12f;
			player.Facing = Facings.Left;
			badeline.Sprite.Scale.X = -1f;
			granny.X = playerWas + 8f;
			theo.X += 16f;
			Level.Add(oshiro = new Entity(new Vector2(granny.X - 24f, granny.Y + 4f)));
			OshiroSprite oshiroSprite = new OshiroSprite(1);
			oshiro.Add(oshiroSprite);
			fade = 0f;
			wipe = new FadeWipe(Level, wipeIn: true);
			wipe.Duration = 1f;
			yield return 0.25f;
			while (oshiro.Y > granny.Y - 4f)
			{
				oshiro.Y -= Engine.DeltaTime * 32f;
				yield return null;
			}
		}

		private IEnumerator OshiroSettles()
		{
			Vector2 from = oshiro.Position;
			Vector2 to = oshiro.Position + new Vector2(40f, 8f);
			for (float p = 0f; p < 1f; p += Engine.DeltaTime)
			{
				oshiro.Position = Vector2.Lerp(from, to, p);
				yield return null;
			}
			granny.Sprite.Scale.X = 1f;
			yield return null;
		}

		public override void Update()
		{
			versionAlpha = Calc.Approach(versionAlpha, showVersion ? 1 : 0, Engine.DeltaTime * 5f);
			base.Update();
		}

		public override void Render()
		{
			if (fade > 0f)
			{
				Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * fade);
			}
			base.Render();
			if (Settings.Instance.SpeedrunClock != 0 && versionAlpha > 0f)
			{
				AreaComplete.VersionNumberAndVariants(version, versionAlpha, 1f);
			}
		}
	}
}
