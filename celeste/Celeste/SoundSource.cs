using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
	[Tracked(false)]
	public class SoundSource : Component
	{
		public string EventName;

		public Vector2 Position = Vector2.Zero;

		public bool DisposeOnTransition = true;

		public bool RemoveOnOneshotEnd;

		private EventInstance instance;

		private bool is3D;

		private bool isOneshot;

		public bool Playing { get; private set; }

		public bool Is3D => is3D;

		public bool IsOneshot => isOneshot;

		public bool InstancePlaying
		{
			get
			{
				if (instance != null)
				{
					instance.getPlaybackState(out var state);
					if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING || state == PLAYBACK_STATE.SUSTAINING)
					{
						return true;
					}
				}
				return false;
			}
		}

		public SoundSource()
			: base(active: true, visible: false)
		{
		}

		public SoundSource(string path)
			: this()
		{
			Play(path);
		}

		public SoundSource(Vector2 offset, string path)
			: this()
		{
			Position = offset;
			Play(path);
		}

		public override void Added(Entity entity)
		{
			base.Added(entity);
			UpdateSfxPosition();
		}

		public SoundSource Play(string path, string param = null, float value = 0f)
		{
			Stop();
			EventName = path;
			EventDescription? desc = Audio.GetEventDescription(path);
			if (desc.HasValue)
			{
				desc.Value.createInstance(out instance);
				desc.Value.is3D(out is3D);
				desc.Value.isOneshot(out isOneshot);
			}
			if (instance != null)
			{
				if (is3D)
				{
					Vector2 position = Position;
					if (base.Entity != null)
					{
						position += base.Entity.Position;
					}
					Audio.Position(instance, position);
				}
				if (param != null)
				{
					instance.setParameterByName(param, value);
				}
				instance.start();
				Playing = true;
			}
			return this;
		}

		public SoundSource Param(string param, float value)
		{
			if (instance != null)
			{
				instance.setParameterByName(param, value);
			}
			return this;
		}

		public SoundSource Pause()
		{
			if (instance != null)
			{
				instance.setPaused(paused: true);
			}
			Playing = false;
			return this;
		}

		public SoundSource Resume()
		{
			if (instance != null)
			{
				instance.getPaused(out var paused);
				if (paused)
				{
					instance.setPaused(paused: false);
					Playing = true;
				}
			}
			return this;
		}

		public SoundSource Stop(bool allowFadeout = true)
		{
			Audio.Stop(instance, allowFadeout);
			instance = null;
			Playing = false;
			return this;
		}

		public void UpdateSfxPosition()
		{
			if (is3D && instance != null)
			{
				Vector2 position = Position;
				if (base.Entity != null)
				{
					position += base.Entity.Position;
				}
				Audio.Position(instance, position);
			}
		}

		public override void Update()
		{
			UpdateSfxPosition();
			if (!isOneshot || !(instance != null))
			{
				return;
			}
			instance.getPlaybackState(out var state);
			if (state == PLAYBACK_STATE.STOPPED)
			{
				instance.release();
				instance = null;
				Playing = false;
				if (RemoveOnOneshotEnd)
				{
					RemoveSelf();
				}
			}
		}

		public override void EntityRemoved(Scene scene)
		{
			base.EntityRemoved(scene);
			Stop();
		}

		public override void Removed(Entity entity)
		{
			base.Removed(entity);
			Stop();
		}

		public override void SceneEnd(Scene scene)
		{
			base.SceneEnd(scene);
			Stop(allowFadeout: false);
		}

		public override void DebugRender(Camera camera)
		{
			Vector2 position = Position;
			if (base.Entity != null)
			{
				position += base.Entity.Position;
			}
			if (instance != null && Playing)
			{
				Draw.Circle(position, 4f + base.Scene.RawTimeActive * 2f % 1f * 16f, Color.BlueViolet, 16);
			}
			Draw.HollowRect(position.X - 2f, position.Y - 2f, 4f, 4f, Color.BlueViolet);
		}
	}
}
