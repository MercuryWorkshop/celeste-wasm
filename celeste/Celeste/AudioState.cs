using System;

namespace Celeste
{
	[Serializable]
	public class AudioState
	{
		public static string[] LayerParameters = new string[10] { "layer0", "layer1", "layer2", "layer3", "layer4", "layer5", "layer6", "layer7", "layer8", "layer9" };

		public AudioTrackState Music = new AudioTrackState();

		public AudioTrackState Ambience = new AudioTrackState();

		public AudioState()
		{
		}

		public AudioState(AudioTrackState music, AudioTrackState ambience)
		{
			if (music != null)
			{
				Music = music.Clone();
			}
			if (ambience != null)
			{
				Ambience = ambience.Clone();
			}
		}

		public AudioState(string music, string ambience)
		{
			Music.Event = music;
			Ambience.Event = ambience;
		}

		public void Apply(bool forceSixteenthNoteHack = false)
		{
			bool changed = Audio.SetMusic(Music.Event, startPlaying: false);
			if (Audio.CurrentMusicEventInstance != null)
			{
				foreach (MEP param2 in Music.Parameters)
				{
					if (!(param2.Key == "sixteenth_note") || forceSixteenthNoteHack)
					{
						Audio.SetParameter(Audio.CurrentMusicEventInstance, param2.Key, param2.Value);
					}
				}
				if (changed)
				{
					Audio.CurrentMusicEventInstance.start();
				}
			}
			changed = Audio.SetAmbience(Ambience.Event, startPlaying: false);
			if (!(Audio.CurrentAmbienceEventInstance != null))
			{
				return;
			}
			foreach (MEP param in Ambience.Parameters)
			{
				Audio.SetParameter(Audio.CurrentAmbienceEventInstance, param.Key, param.Value);
			}
			if (changed)
			{
				Audio.CurrentAmbienceEventInstance.start();
			}
		}

		public void Stop(bool allowFadeOut = true)
		{
			Audio.SetMusic(null, startPlaying: false, allowFadeOut);
			Audio.SetAmbience(null);
		}

		public AudioState Clone()
		{
			return new AudioState
			{
				Music = Music.Clone(),
				Ambience = Ambience.Clone()
			};
		}
	}
}
