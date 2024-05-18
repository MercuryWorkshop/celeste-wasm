using Microsoft.Xna.Framework;

namespace Celeste
{
	public class MusicTrigger : Trigger
	{
		public string Track;

		public bool SetInSession;

		public bool ResetOnLeave;

		public int Progress;

		private string oldTrack;

		public MusicTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			Track = data.Attr("track");
			ResetOnLeave = data.Bool("resetOnLeave", defaultValue: true);
			Progress = data.Int("progress");
		}

		public override void OnEnter(Player player)
		{
			if (ResetOnLeave)
			{
				oldTrack = Audio.CurrentMusic;
			}
			Session session = SceneAs<Level>().Session;
			session.Audio.Music.Event = SFX.EventnameByHandle(Track);
			if (Progress != 0)
			{
				session.Audio.Music.Progress = Progress;
			}
			session.Audio.Apply();
		}

		public override void OnLeave(Player player)
		{
			if (ResetOnLeave)
			{
				Session session = SceneAs<Level>().Session;
				session.Audio.Music.Event = oldTrack;
				session.Audio.Apply();
			}
		}
	}
}
