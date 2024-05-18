using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class InteractTrigger : Entity
	{
		public const string FlagPrefix = "it_";

		public TalkComponent Talker;

		public List<string> Events;

		private int eventIndex;

		private float timeout;

		private bool used;

		public InteractTrigger(EntityData data, Vector2 offset)
			: base(data.Position + offset)
		{
			Events = new List<string>();
			Events.Add(data.Attr("event"));
			base.Collider = new Hitbox(data.Width, data.Height);
			for (int i = 2; i < 100 && data.Has("event_" + i) && !string.IsNullOrEmpty(data.Attr("event_" + i)); i++)
			{
				Events.Add(data.Attr("event_" + i));
			}
			Vector2 talkOffset = new Vector2(data.Width / 2, 0f);
			if (data.Nodes.Length != 0)
			{
				talkOffset = data.Nodes[0] - data.Position;
			}
			Add(Talker = new TalkComponent(new Rectangle(0, 0, data.Width, data.Height), talkOffset, OnTalk));
			Talker.PlayerMustBeFacing = false;
		}

		public override void Added(Scene scene)
		{
			base.Added(scene);
			Session session = (scene as Level).Session;
			for (int i = 0; i < Events.Count; i++)
			{
				if (session.GetFlag("it_" + Events[i]))
				{
					eventIndex++;
				}
			}
			if (eventIndex >= Events.Count)
			{
				RemoveSelf();
			}
			else if (Events[eventIndex] == "ch5_theo_phone")
			{
				scene.Add(new TheoPhone(Position + new Vector2(base.Width / 2f - 8f, base.Height - 1f)));
			}
		}

		public void OnTalk(Player player)
		{
			if (used)
			{
				return;
			}
			bool setFlag = true;
			switch (Events[eventIndex])
			{
			case "ch2_poem":
				base.Scene.Add(new CS02_Journal(player));
				setFlag = false;
				break;
			case "ch3_guestbook":
				base.Scene.Add(new CS03_Guestbook(player));
				setFlag = false;
				break;
			case "ch3_memo":
				base.Scene.Add(new CS03_Memo(player));
				setFlag = false;
				break;
			case "ch3_diary":
				base.Scene.Add(new CS03_Diary(player));
				setFlag = false;
				break;
			case "ch5_theo_phone":
				base.Scene.Add(new CS05_TheoPhone(player, base.Center.X));
				break;
			case "ch5_mirror_reflection":
				base.Scene.Add(new CS05_Reflection1(player));
				break;
			case "ch5_see_theo":
				base.Scene.Add(new CS05_SeeTheo(player, 0));
				break;
			case "ch5_see_theo_b":
				base.Scene.Add(new CS05_SeeTheo(player, 1));
				break;
			}
			if (setFlag)
			{
				(base.Scene as Level).Session.SetFlag("it_" + Events[eventIndex]);
				eventIndex++;
				if (eventIndex >= Events.Count)
				{
					used = true;
					timeout = 0.25f;
				}
			}
		}

		public override void Update()
		{
			if (used)
			{
				timeout -= Engine.DeltaTime;
				if (timeout <= 0f)
				{
					RemoveSelf();
				}
			}
			else
			{
				while (eventIndex < Events.Count && (base.Scene as Level).Session.GetFlag("it_" + Events[eventIndex]))
				{
					eventIndex++;
				}
				if (eventIndex >= Events.Count)
				{
					RemoveSelf();
				}
			}
			base.Update();
		}
	}
}
