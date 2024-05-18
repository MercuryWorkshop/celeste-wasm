using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class NPC09_Granny_Inside : NPC
	{
		public const string DoorConversationAvailable = "granny_door";

		private const string DoorConversationDone = "granny_door_done";

		private const string CounterFlag = "granny";

		private int conversation;

		private const int MaxConversation = 4;

		public Hahaha Hahaha;

		public GrannyLaughSfx LaughSfx;

		private Player player;

		private TalkComponent talker;

		private bool talking;

		private Coroutine talkRoutine;

		private bool HasDoorConversation
		{
			get
			{
				if (Level.Session.GetFlag("granny_door"))
				{
					return !Level.Session.GetFlag("granny_door_done");
				}
				return false;
			}
		}

		private bool talkerEnabled
		{
			get
			{
				if (conversation <= 0 || conversation >= 4)
				{
					return HasDoorConversation;
				}
				return true;
			}
		}

		public NPC09_Granny_Inside(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			Add(Sprite = GFX.SpriteBank.Create("granny"));
			Sprite.Play("idle");
			Add(LaughSfx = new GrannyLaughSfx(Sprite));
			MoveAnim = "walk";
			Maxspeed = 40f;
			Add(talker = new TalkComponent(new Rectangle(-20, -8, 40, 8), new Vector2(0f, -24f), OnTalk));
			talker.Enabled = false;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			conversation = Level.Session.GetCounter("granny");
			scene.Add(Hahaha = new Hahaha(Position + new Vector2(8f, -4f)));
			Hahaha.Enabled = false;
		}

		public override void Update()
		{
			if (!talking && conversation == 0)
			{
				player = Level.Tracker.GetEntity<Player>();
				if (player != null && Math.Abs(player.X - base.X) < 48f)
				{
					OnTalk(player);
				}
			}
			talker.Enabled = talkerEnabled;
			Hahaha.Enabled = Sprite.CurrentAnimationID == "laugh";
			base.Update();
		}

		private void OnTalk(Player player)
		{
			this.player = player;
			(base.Scene as Level).StartCutscene(EndTalking);
			Add(talkRoutine = new Coroutine(TalkRoutine(player)));
			talking = true;
		}

		private IEnumerator TalkRoutine(Player player)
		{
			player.StateMachine.State = 11;
			player.Dashes = 1;
			player.ForceCameraUpdate = true;
			while (!player.OnGround())
			{
				yield return null;
			}
			yield return player.DummyWalkToExact((int)base.X - 16);
			player.Facing = Facings.Right;
			player.ForceCameraUpdate = false;
			Vector2 zoomPoint = new Vector2(base.X - 8f - Level.Camera.X, 110f);
			if (HasDoorConversation)
			{
				Sprite.Scale.X = -1f;
				yield return Level.ZoomTo(zoomPoint, 2f, 0.5f);
				yield return Textbox.Say("APP_OLDLADY_LOCKED");
			}
			else if (conversation == 0)
			{
				yield return 0.5f;
				Sprite.Scale.X = -1f;
				yield return 0.25f;
				yield return Level.ZoomTo(zoomPoint, 2f, 0.5f);
				yield return Textbox.Say("APP_OLDLADY_B", StartLaughing, StopLaughing);
			}
			else if (conversation == 1)
			{
				Sprite.Scale.X = -1f;
				yield return Level.ZoomTo(zoomPoint, 2f, 0.5f);
				yield return Textbox.Say("APP_OLDLADY_C", StartLaughing, StopLaughing);
			}
			else if (conversation == 2)
			{
				Sprite.Scale.X = -1f;
				yield return Level.ZoomTo(zoomPoint, 2f, 0.5f);
				yield return Textbox.Say("APP_OLDLADY_D", StartLaughing, StopLaughing);
			}
			else if (conversation == 3)
			{
				Sprite.Scale.X = -1f;
				yield return Level.ZoomTo(zoomPoint, 2f, 0.5f);
				yield return Textbox.Say("APP_OLDLADY_E", StartLaughing, StopLaughing);
			}
			talker.Enabled = talkerEnabled;
			yield return Level.ZoomBack(0.5f);
			Level.EndCutscene();
			EndTalking(Level);
		}

		private IEnumerator StartLaughing()
		{
			Sprite.Play("laugh");
			yield return null;
		}

		private IEnumerator StopLaughing()
		{
			Sprite.Play("idle");
			yield return null;
		}

		private void EndTalking(Level level)
		{
			if (player != null)
			{
				player.StateMachine.State = 0;
				player.ForceCameraUpdate = false;
			}
			if (HasDoorConversation)
			{
				Level.Session.SetFlag("granny_door_done");
			}
			else
			{
				Level.Session.IncrementCounter("granny");
				conversation++;
			}
			if (talkRoutine != null)
			{
				talkRoutine.RemoveSelf();
				talkRoutine = null;
			}
			Sprite.Play("idle");
			talking = false;
		}
	}
}
