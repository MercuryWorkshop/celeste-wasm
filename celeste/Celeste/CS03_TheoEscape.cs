using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class CS03_TheoEscape : CutsceneEntity
	{
		public const string Flag = "resort_theo";

		private NPC03_Theo_Escaping theo;

		private Player player;

		private Vector2 theoStart;

		public CS03_TheoEscape(NPC03_Theo_Escaping theo, Player player)
		{
			this.theo = theo;
			theoStart = theo.Position;
			this.player = player;
		}

		public override void OnBegin(Level level)
		{
			Add(new Coroutine(Cutscene(level)));
		}

		private IEnumerator Cutscene(Level level)
		{
			player.StateMachine.State = 11;
			player.StateMachine.Locked = true;
			yield return player.DummyWalkTo(theo.X - 64f);
			player.Facing = Facings.Right;
			yield return Level.ZoomTo(new Vector2(240f, 135f), 2f, 0.5f);
			Func<IEnumerator>[] triggers = new Func<IEnumerator>[4] { StopRemovingVent, StartRemoveVent, RemoveVent, GivePhone };
			string dialog = "CH3_THEO_INTRO";
			if (!SaveData.Instance.HasFlag("MetTheo"))
			{
				dialog = "CH3_THEO_NEVER_MET";
			}
			else if (!SaveData.Instance.HasFlag("TheoKnowsName"))
			{
				dialog = "CH3_THEO_NEVER_INTRODUCED";
			}
			yield return Textbox.Say(dialog, triggers);
			theo.Sprite.Scale.X = 1f;
			yield return 0.2f;
			theo.Sprite.Play("walk");
			while (!theo.CollideCheck<Solid>(theo.Position + new Vector2(2f, 0f)))
			{
				yield return null;
				theo.X += 48f * Engine.DeltaTime;
			}
			theo.Sprite.Play("idle");
			yield return 0.2f;
			Audio.Play("event:/char/theo/resort_standtocrawl", theo.Position);
			theo.Sprite.Play("duck");
			yield return 0.5f;
			if (theo.Talker != null)
			{
				theo.Talker.Active = false;
			}
			level.Session.SetFlag("resort_theo");
			player.StateMachine.Locked = false;
			player.StateMachine.State = 0;
			theo.CrawlUntilOut();
			yield return level.ZoomBack(0.5f);
			EndCutscene(level);
		}

		private IEnumerator StartRemoveVent()
		{
			theo.Sprite.Scale.X = 1f;
			yield return 0.1f;
			Audio.Play("event:/char/theo/resort_vent_grab", theo.Position);
			theo.Sprite.Play("goToVent");
			yield return 0.25f;
		}

		private IEnumerator StopRemovingVent()
		{
			theo.Sprite.Play("idle");
			yield return 0.1f;
			theo.Sprite.Scale.X = -1f;
		}

		private IEnumerator RemoveVent()
		{
			yield return 0.8f;
			Audio.Play("event:/char/theo/resort_vent_rip", theo.Position);
			theo.Sprite.Play("fallVent");
			yield return 0.8f;
			theo.grate.Fall();
			yield return 0.8f;
			theo.Sprite.Scale.X = -1f;
			yield return 0.25f;
		}

		private IEnumerator GivePhone()
		{
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				theo.Sprite.Play("walk");
				theo.Sprite.Scale.X = -1f;
				while (theo.X > player.X + 24f)
				{
					theo.X -= 48f * Engine.DeltaTime;
					yield return null;
				}
			}
			theo.Sprite.Play("idle");
			yield return 1f;
		}

		public override void OnEnd(Level level)
		{
			player.StateMachine.Locked = false;
			player.StateMachine.State = 0;
			level.Session.SetFlag("resort_theo");
			SaveData.Instance.SetFlag("MetTheo");
			SaveData.Instance.SetFlag("TheoKnowsName");
			if (theo != null && WasSkipped)
			{
				theo.Position = theoStart;
				theo.CrawlUntilOut();
				if (theo.grate != null)
				{
					theo.grate.RemoveSelf();
				}
			}
		}
	}
}
