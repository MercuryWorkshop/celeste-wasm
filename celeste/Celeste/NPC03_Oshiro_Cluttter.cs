using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class NPC03_Oshiro_Cluttter : NPC
	{
		public const string TalkFlagsA = "oshiro_clutter_";

		public const string TalkFlagsB = "oshiro_clutter_optional_";

		public const string ClearedFlags = "oshiro_clutter_cleared_";

		public const string FinishedFlag = "oshiro_clutter_finished";

		public const string DoorOpenFlag = "oshiro_clutter_door_open";

		public Vector2 HomePosition;

		private int sectionsComplete;

		private bool talked;

		private bool inRoutine;

		private List<Vector2> nodes = new List<Vector2>();

		private Coroutine paceRoutine;

		private Coroutine talkRoutine;

		private SoundSource paceSfx;

		private float paceTimer;

		public Vector2 ZoomPoint
		{
			get
			{
				if (sectionsComplete < 2)
				{
					return Position + new Vector2(0f, -30f) - Level.Camera.Position;
				}
				return Position + new Vector2(0f, -15f) - Level.Camera.Position;
			}
		}

		public NPC03_Oshiro_Cluttter(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			Add(Sprite = new OshiroSprite(-1));
			Add(Talker = new TalkComponent(new Rectangle(-24, -8, 48, 8), new Vector2(0f, -24f), OnTalk));
			Add(Light = new VertexLight(-Vector2.UnitY * 16f, Color.White, 1f, 32, 64));
			MoveAnim = "move";
			IdleAnim = "idle";
			Vector2[] array = data.Nodes;
			foreach (Vector2 node in array)
			{
				nodes.Add(node + offset);
			}
			Add(paceSfx = new SoundSource());
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			if (base.Session.GetFlag("oshiro_clutter_finished"))
			{
				RemoveSelf();
			}
			else
			{
				if (base.Session.GetFlag("oshiro_clutter_cleared_0"))
				{
					sectionsComplete++;
				}
				if (base.Session.GetFlag("oshiro_clutter_cleared_1"))
				{
					sectionsComplete++;
				}
				if (base.Session.GetFlag("oshiro_clutter_cleared_2"))
				{
					sectionsComplete++;
				}
				if (sectionsComplete == 0 || sectionsComplete == 3)
				{
					Sprite.Scale.X = 1f;
				}
				if (sectionsComplete > 0)
				{
					Position = nodes[sectionsComplete - 1];
				}
				else if (!base.Session.GetFlag("oshiro_clutter_0"))
				{
					Add(paceRoutine = new Coroutine(Pace()));
				}
				if (sectionsComplete == 0 && base.Session.GetFlag("oshiro_clutter_0") && !base.Session.GetFlag("oshiro_clutter_optional_0"))
				{
					Sprite.Play("idle_ground");
				}
				if (sectionsComplete == 3 || base.Session.GetFlag("oshiro_clutter_optional_" + sectionsComplete))
				{
					Remove(Talker);
				}
			}
			HomePosition = Position;
		}

		private void OnTalk(Player player)
		{
			talked = true;
			if (paceRoutine != null)
			{
				paceRoutine.RemoveSelf();
			}
			paceRoutine = null;
			if (!base.Session.GetFlag("oshiro_clutter_" + sectionsComplete))
			{
				base.Scene.Add(new CS03_OshiroClutter(player, this, sectionsComplete));
				return;
			}
			Level.StartCutscene(EndTalkRoutine);
			base.Session.SetFlag("oshiro_clutter_optional_" + sectionsComplete);
			Add(talkRoutine = new Coroutine(TalkRoutine(player)));
			if (Talker != null)
			{
				Talker.Enabled = false;
			}
		}

		private IEnumerator TalkRoutine(Player player)
		{
			yield return PlayerApproach(player, turnToFace: true, 24f, (sectionsComplete != 1 && sectionsComplete != 2) ? 1 : (-1));
			yield return Level.ZoomTo(ZoomPoint, 2f, 0.5f);
			yield return Textbox.Say("CH3_OSHIRO_CLUTTER" + sectionsComplete + "_B", StandUp);
			yield return Level.ZoomBack(0.5f);
			Level.EndCutscene();
			EndTalkRoutine(Level);
		}

		private void EndTalkRoutine(Level level)
		{
			if (talkRoutine != null)
			{
				talkRoutine.RemoveSelf();
			}
			talkRoutine = null;
			(Sprite as OshiroSprite).Pop("idle", flip: false);
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (player != null)
			{
				player.StateMachine.Locked = false;
				player.StateMachine.State = 0;
			}
		}

		private IEnumerator StandUp()
		{
			Audio.Play("event:/char/oshiro/chat_get_up", Position);
			(Sprite as OshiroSprite).Pop("idle", flip: false);
			yield return 0.25f;
		}

		private IEnumerator Pace()
		{
			while (true)
			{
				(Sprite as OshiroSprite).Wiggle();
				yield return PaceLeft();
				while (paceTimer < 2.266f)
				{
					yield return null;
				}
				paceTimer = 0f;
				(Sprite as OshiroSprite).Wiggle();
				yield return PaceRight();
				while (paceTimer < 2.266f)
				{
					yield return null;
				}
				paceTimer = 0f;
			}
		}

		public IEnumerator PaceRight()
		{
			Vector2 target = HomePosition;
			if ((Position - target).Length() > 8f)
			{
				paceSfx.Play("event:/char/oshiro/move_04_pace_right");
			}
			yield return MoveTo(target);
		}

		public IEnumerator PaceLeft()
		{
			Vector2 target = HomePosition + new Vector2(-20f, 0f);
			if ((Position - target).Length() > 8f)
			{
				paceSfx.Play("event:/char/oshiro/move_04_pace_left");
			}
			yield return MoveTo(target);
		}

		public override void Update()
		{
			base.Update();
			paceTimer += Engine.DeltaTime;
			Player player = base.Scene.Tracker.GetEntity<Player>();
			if (sectionsComplete == 3 && !inRoutine && player != null && player.X < base.X + 32f && player.Y <= base.Y)
			{
				OnTalk(player);
				inRoutine = true;
			}
			if (sectionsComplete == 0 && !talked)
			{
				Level level = base.Scene as Level;
				if (player != null && !player.Dead)
				{
					float vol = Calc.ClampedMap(Vector2.Distance(base.Center, player.Center), 40f, 128f);
					level.Session.Audio.Music.Layer(1, vol);
					level.Session.Audio.Music.Layer(2, 1f - vol);
					level.Session.Audio.Apply();
				}
				else
				{
					level.Session.Audio.Music.Layer(1, value: true);
					level.Session.Audio.Music.Layer(2, value: false);
					level.Session.Audio.Apply();
				}
			}
		}
	}
}
