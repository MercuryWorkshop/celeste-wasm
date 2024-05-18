using System;
using System.Collections;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class EventTrigger : Trigger
	{
		public string Event;

		public bool OnSpawnHack;

		private bool triggered;

		private EventInstance snapshot;

		public float Time { get; private set; }

		public EventTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			Event = data.Attr("event");
			OnSpawnHack = data.Bool("onSpawn");
		}

		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			if (OnSpawnHack)
			{
				Player player2 = CollideFirst<Player>();
				if (player2 != null)
				{
					OnEnter(player2);
				}
			}
			if (Event == "ch9_badeline_helps")
			{
				Player player = base.Scene.Tracker.GetEntity<Player>();
				if (player != null && player.Left > base.Right)
				{
					RemoveSelf();
				}
			}
		}

		public override void OnEnter(Player player)
		{
			if (triggered)
			{
				return;
			}
			triggered = true;
			Level level = base.Scene as Level;
			switch (Event)
			{
			case "end_city":
				base.Scene.Add(new CS01_Ending(player));
				break;
			case "end_oldsite_dream":
				base.Scene.Add(new CS02_DreamingPhonecall(player));
				break;
			case "end_oldsite_awake":
				base.Scene.Add(new CS02_Ending(player));
				break;
			case "ch5_see_theo":
				if (!(base.Scene as Level).Session.GetFlag("seeTheoInCrystal"))
				{
					base.Scene.Add(new CS05_SeeTheo(player, 0));
				}
				break;
			case "ch5_found_theo":
				if (!level.Session.GetFlag("foundTheoInCrystal"))
				{
					base.Scene.Add(new CS05_SaveTheo(player));
				}
				break;
			case "ch5_mirror_reflection":
				if (!level.Session.GetFlag("reflection"))
				{
					base.Scene.Add(new CS05_Reflection1(player));
				}
				break;
			case "cancel_ch5_see_theo":
				level.Session.SetFlag("it_ch5_see_theo");
				level.Session.SetFlag("it_ch5_see_theo_b");
				level.Session.SetFlag("ignore_darkness_" + level.Session.Level);
				Add(new Coroutine(Brighten()));
				break;
			case "ch6_boss_intro":
				if (!level.Session.GetFlag("boss_intro"))
				{
					level.Add(new CS06_BossIntro(base.Center.X, player, level.Entities.FindFirst<FinalBoss>()));
				}
				break;
			case "ch6_reflect":
				if (!level.Session.GetFlag("reflection"))
				{
					base.Scene.Add(new CS06_Reflection(player, base.Center.X - 5f));
				}
				break;
			case "ch7_summit":
				base.Scene.Add(new CS07_Ending(player, new Vector2(base.Center.X, base.Bottom)));
				break;
			case "ch8_door":
				base.Scene.Add(new CS08_EnterDoor(player, base.Left));
				break;
			case "ch9_goto_the_future":
			case "ch9_goto_the_past":
				level.OnEndOfFrame += delegate
				{
					new Vector2(level.LevelOffset.X + (float)level.Bounds.Width - player.X, player.Y - level.LevelOffset.Y);
					Vector2 levelOffset = level.LevelOffset;
					Vector2 vector = player.Position - level.LevelOffset;
					Vector2 vector2 = level.Camera.Position - level.LevelOffset;
					Facings facing = player.Facing;
					level.Remove(player);
					level.UnloadLevel();
					level.Session.Dreaming = true;
					level.Session.Level = ((Event == "ch9_goto_the_future") ? "intro-01-future" : "intro-00-past");
					level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));
					level.Session.FirstLevel = false;
					level.LoadLevel(Player.IntroTypes.Transition);
					level.Camera.Position = level.LevelOffset + vector2;
					level.Session.Inventory.Dashes = 1;
					player.Dashes = Math.Min(player.Dashes, 1);
					level.Add(player);
					player.Position = level.LevelOffset + vector;
					player.Facing = facing;
					player.Hair.MoveHairBy(level.LevelOffset - levelOffset);
					if (level.Wipe != null)
					{
						level.Wipe.Cancel();
					}
					level.Flash(Color.White);
					level.Shake();
					level.Add(new LightningStrike(new Vector2(player.X + 60f, level.Bounds.Bottom - 180), 10, 200f));
					level.Add(new LightningStrike(new Vector2(player.X + 220f, level.Bounds.Bottom - 180), 40, 200f, 0.25f));
					Audio.Play("event:/new_content/game/10_farewell/lightning_strike");
				};
				break;
			case "ch9_moon_intro":
				if (!level.Session.GetFlag("moon_intro") && player.StateMachine.State == 13)
				{
					base.Scene.Add(new CS10_MoonIntro(player));
					break;
				}
				level.Entities.FindFirst<BirdNPC>()?.RemoveSelf();
				level.Session.Inventory.Dashes = 1;
				player.Dashes = 1;
				break;
			case "ch9_hub_intro":
				if (!level.Session.GetFlag("hub_intro"))
				{
					base.Scene.Add(new CS10_HubIntro(base.Scene, player));
				}
				break;
			case "ch9_hub_transition_out":
				Add(new Coroutine(Ch9HubTransitionBackgroundToBright(player)));
				break;
			case "ch9_badeline_helps":
				if (!level.Session.GetFlag("badeline_helps"))
				{
					base.Scene.Add(new CS10_BadelineHelps(player));
				}
				break;
			case "ch9_farewell":
				base.Scene.Add(new CS10_Farewell(player));
				break;
			case "ch9_ending":
				base.Scene.Add(new CS10_Ending(player));
				break;
			case "ch9_end_golden":
				ScreenWipe.WipeColor = Color.White;
				new FadeWipe(level, wipeIn: false, delegate
				{
					level.OnEndOfFrame += delegate
					{
						level.TeleportTo(player, "end-granny", Player.IntroTypes.Transition);
						player.Speed = Vector2.Zero;
					};
				}).Duration = 1f;
				break;
			case "ch9_final_room":
			{
				Session session = (base.Scene as Level).Session;
				switch (session.GetCounter("final_room_deaths"))
				{
				case 0:
					base.Scene.Add(new CS10_FinalRoom(player, first: true));
					break;
				case 50:
					base.Scene.Add(new CS10_FinalRoom(player, first: false));
					break;
				}
				session.IncrementCounter("final_room_deaths");
				break;
			}
			case "ch9_ding_ding_ding":
			{
				Audio.Play("event:/new_content/game/10_farewell/pico8_flag", base.Center);
				Decal flag = null;
				foreach (Decal d in base.Scene.Entities.FindAll<Decal>())
				{
					if (d.Name.ToLower() == "decals/10-farewell/finalflag")
					{
						flag = d;
						break;
					}
				}
				flag?.FinalFlagTrigger();
				break;
			}
			case "ch9_golden_snapshot":
				snapshot = Audio.CreateSnapshot("snapshot:/game_10_golden_room_flavour");
				(base.Scene as Level).SnapColorGrade("golden");
				break;
			default:
				throw new Exception("Event '" + Event + "' does not exist!");
			}
		}

		public override void Removed(Scene scene)
		{
			base.Removed(scene);
			Audio.ReleaseSnapshot(snapshot);
		}

		public override void SceneEnd(Scene scene)
		{
			base.SceneEnd(scene);
			Audio.ReleaseSnapshot(snapshot);
		}

		private IEnumerator Brighten()
		{
			Level level = base.Scene as Level;
			float darkness = AreaData.Get(level).DarknessAlpha;
			while (level.Lighting.Alpha != darkness)
			{
				level.Lighting.Alpha = Calc.Approach(level.Lighting.Alpha, darkness, Engine.DeltaTime * 4f);
				yield return null;
			}
		}

		private IEnumerator Ch9HubTransitionBackgroundToBright(Player player)
		{
			Level level = base.Scene as Level;
			float start = base.Bottom;
			float end = base.Top;
			while (true)
			{
				float lerp = Calc.ClampedMap(player.Y, start, end);
				foreach (Backdrop item in level.Background.GetEach<Backdrop>("bright"))
				{
					item.ForceVisible = true;
					item.FadeAlphaMultiplier = lerp;
				}
				yield return null;
			}
		}
	}
}
