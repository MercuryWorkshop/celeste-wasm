using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Celeste
{
	[Serializable]
	public class AudioTrackState
	{
		[XmlIgnore]
		private string ev;

		public List<MEP> Parameters = new List<MEP>();

		[XmlAttribute]
		public string Event
		{
			get
			{
				return ev;
			}
			set
			{
				if (ev != value)
				{
					ev = value;
					Parameters.Clear();
				}
			}
		}

		[XmlIgnore]
		public int Progress
		{
			get
			{
				return (int)GetParam("progress");
			}
			set
			{
				Param("progress", value);
			}
		}

		public AudioTrackState()
		{
		}

		public AudioTrackState(string ev)
		{
			Event = ev;
		}

		public AudioTrackState Layer(int index, float value)
		{
			return Param(AudioState.LayerParameters[index], value);
		}

		public AudioTrackState Layer(int index, bool value)
		{
			return Param(AudioState.LayerParameters[index], value);
		}

		public AudioTrackState SetProgress(int value)
		{
			Progress = value;
			return this;
		}

		public AudioTrackState Param(string key, float value)
		{
			foreach (MEP param in Parameters)
			{
				if (param.Key != null && param.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
				{
					param.Value = value;
					return this;
				}
			}
			Parameters.Add(new MEP(key, value));
			return this;
		}

		public AudioTrackState Param(string key, bool value)
		{
			return Param(key, value ? 1 : 0);
		}

		public float GetParam(string key)
		{
			foreach (MEP param in Parameters)
			{
				if (param.Key != null && param.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
				{
					return param.Value;
				}
			}
			return 0f;
		}

		public AudioTrackState Clone()
		{
			AudioTrackState copy = new AudioTrackState();
			copy.Event = Event;
			foreach (MEP param in Parameters)
			{
				copy.Parameters.Add(new MEP(param.Key, param.Value));
			}
			return copy;
		}
	}
}
