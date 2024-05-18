using Microsoft.Xna.Framework;

namespace Celeste
{
	public class AltMusicTrigger : Trigger
	{
		public string Track;

		public bool ResetOnLeave;

		public AltMusicTrigger(EntityData data, Vector2 offset)
			: base(data, offset)
		{
			Track = data.Attr("track");
			ResetOnLeave = data.Bool("resetOnLeave", defaultValue: true);
		}

		public override void OnEnter(Player player)
		{
			Audio.SetAltMusic(SFX.EventnameByHandle(Track));
		}

		public override void OnLeave(Player player)
		{
			if (ResetOnLeave)
			{
				Audio.SetAltMusic(null);
			}
		}
	}
}
