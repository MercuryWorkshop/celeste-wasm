using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	public class OshiroLobbyBell : Entity
	{
		private TalkComponent talker;

		public OshiroLobbyBell(Vector2 position)
			: base(position)
		{
			Add(talker = new TalkComponent(new Rectangle(-8, -8, 16, 16), new Vector2(0f, -24f), OnTalk));
			talker.Enabled = false;
		}

		private void OnTalk(Player player)
		{
			Audio.Play("event:/game/03_resort/deskbell_again", Position);
		}

		public override void Update()
		{
			if (!talker.Enabled && base.Scene.Entities.FindFirst<NPC03_Oshiro_Lobby>() == null)
			{
				talker.Enabled = true;
			}
			base.Update();
		}
	}
}
