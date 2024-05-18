using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using FMOD;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
// using SDL2;

namespace Celeste
{
	public static class Audio
	{
		public static class Banks
		{
			public static Bank Master;

			public static Bank Music;

			public static Bank Sfxs;

			public static Bank UI;

			public static Bank DlcMusic;

			public static Bank DlcSfxs;

			public static Bank Load(string name, bool loadStrings)
			{
				string path = Path.Combine(Engine.ContentDirectory, "FMOD", "Desktop", name);
				CheckFmod(system.Value.loadBankFile(path + ".bank", LOAD_BANK_FLAGS.NORMAL, out var bank));
				bank.loadSampleData();
				if (loadStrings)
				{
					CheckFmod(system.Value.loadBankFile(path + ".strings.bank", LOAD_BANK_FLAGS.NORMAL, out var _));
				}
				return bank;
			}
		}

		private static FMOD.Studio.System? system;

		private static FMOD.ATTRIBUTES_3D attributes3d = default(FMOD.ATTRIBUTES_3D);

		public static Dictionary<string, EventDescription> cachedEventDescriptions = new Dictionary<string, EventDescription>();

		private static Camera currentCamera;

		private static bool ready;

		private static EventInstance? currentMusicEvent = null;

		private static EventInstance? currentAltMusicEvent = null;

		private static EventInstance? currentAmbientEvent = null;

		private static EventInstance? mainDownSnapshot = null;

		public static string CurrentMusic = "";

		private static bool musicUnderwater;

		private static EventInstance? musicUnderwaterSnapshot;

		public static EventInstance? CurrentMusicEventInstance => currentMusicEvent;

		public static EventInstance? CurrentAmbienceEventInstance => currentAmbientEvent;

		public static float MusicVolume
		{
			get
			{
				return VCAVolume("vca:/music");
			}
			set
			{
				VCAVolume("vca:/music", value);
			}
		}

		public static float SfxVolume
		{
			get
			{
				return VCAVolume("vca:/gameplay_sfx");
			}
			set
			{
				VCAVolume("vca:/gameplay_sfx", value);
				VCAVolume("vca:/ui_sfx", value);
			}
		}

		public static bool PauseMusic
		{
			get
			{
				return BusPaused("bus:/music");
			}
			set
			{
				BusPaused("bus:/music", value);
			}
		}

		public static bool PauseGameplaySfx
		{
			get
			{
				return BusPaused("bus:/gameplay_sfx");
			}
			set
			{
				BusPaused("bus:/gameplay_sfx", value);
				BusPaused("bus:/music/stings", value);
			}
		}

		public static bool PauseUISfx
		{
			get
			{
				return BusPaused("bus:/ui_sfx");
			}
			set
			{
				BusPaused("bus:/ui_sfx", value);
			}
		}

		public static bool MusicUnderwater
		{
			get
			{
				return musicUnderwater;
			}
			set
			{
				if (musicUnderwater == value)
				{
					return;
				}
				musicUnderwater = value;
				if (musicUnderwater)
				{
					if (musicUnderwaterSnapshot == null)
					{
						musicUnderwaterSnapshot = CreateSnapshot("snapshot:/underwater");
					}
					else
					{
						ResumeSnapshot(musicUnderwaterSnapshot);
					}
				}
				else
				{
					EndSnapshot(musicUnderwaterSnapshot);
				}
			}
		}

		[DllImport("fmod_SDL", CallingConvention = CallingConvention.Cdecl)]
		private static extern void FMOD_SDL_Register(IntPtr system);

		public static void Init()
		{
			FMOD.Studio.INITFLAGS flags = FMOD.Studio.INITFLAGS.NORMAL;
			if (Settings.Instance.LaunchWithFMODLiveUpdate)
			{
				flags = FMOD.Studio.INITFLAGS.LIVEUPDATE;
			}
            // .NET 8 Nullable + FMOD changing everything to structs...
            FMOD.Studio.System systemNotNull;
			CheckFmod(FMOD.Studio.System.create(out systemNotNull));
            system = systemNotNull;
            // Not needed on WASM, doesn't work anyway
            /*
			system.getLowLevelSystem(out var lowLevelSystem);
			if (SDL.SDL_GetPlatform().Equals("Linux"))
			{
				FMOD_SDL_Register(lowLevelSystem.getRaw());
			}
            */
			CheckFmod(system.Value.initialize(1024, flags, FMOD.INITFLAGS.NORMAL, IntPtr.Zero));
			attributes3d.forward = new VECTOR
			{
				x = 0f,
				y = 0f,
				z = 1f
			};
			attributes3d.up = new VECTOR
			{
				x = 0f,
				y = 1f,
				z = 0f
			};
			SetListenerPosition(new Vector3(0f, 0f, 1f), new Vector3(0f, 1f, 0f), new Vector3(0f, 0f, -345f));
			ready = true;
		}

		public static void Update()
		{
			if (system.HasValue && ready)
			{
				CheckFmod(system.Value.update());
			}
		}

		public static void Unload()
		{
			if (system.HasValue)
			{
				CheckFmod(system.Value.unloadAll());
				CheckFmod(system.Value.release());
				system = null;
			}
		}

		public static void SetListenerPosition(Vector3 forward, Vector3 up, Vector3 position)
		{
			FMOD.ATTRIBUTES_3D attr = default(FMOD.ATTRIBUTES_3D);
			attr.forward.x = forward.X;
			attr.forward.z = forward.Y;
			attr.forward.z = forward.Z;
			attr.up.x = up.X;
			attr.up.y = up.Y;
			attr.up.z = up.Z;
			attr.position.x = position.X;
			attr.position.y = position.Y;
			attr.position.z = position.Z;
			system.Value.setListenerAttributes(0, attr);
		}

		public static void SetCamera(Camera camera)
		{
			currentCamera = camera;
		}

		internal static void CheckFmod(RESULT result)
		{
			if (result != 0)
			{
				throw new Exception("FMOD Failed: " + result);
			}
		}

		public static EventInstance? Play(string path)
		{
			EventInstance? instance = CreateInstance(path);
			if (instance != null)
			{
				instance.Value.start();
				instance.Value.release();
			}
			return instance;
		}

		public static EventInstance? Play(string path, string param, float value)
		{
			EventInstance? instance = CreateInstance(path);
			if (instance != null)
			{
				SetParameter(instance, param, value);
				instance.Value.start();
				instance.Value.release();
			}
			return instance;
		}

		public static EventInstance? Play(string path, Vector2 position)
		{
			EventInstance? instance = CreateInstance(path, position);
			if (instance != null)
			{
				instance.Value.start();
				instance.Value.release();
			}
			return instance;
		}

		public static EventInstance? Play(string path, Vector2 position, string param, float value)
		{
			EventInstance? instance = CreateInstance(path, position);
			if (instance != null)
			{
				if (param != null)
				{
					instance.Value.setParameterByName(param, value);
				}
				instance.Value.start();
				instance.Value.release();
			}
			return instance;
		}

		public static EventInstance? Play(string path, Vector2 position, string param, float value, string param2, float value2)
		{
			EventInstance? instance = CreateInstance(path, position);
			if (instance != null)
			{
				if (param != null)
				{
					instance.Value.setParameterByName(param, value);
				}
				if (param2 != null)
				{
					instance.Value.setParameterByName(param2, value2);
				}
				instance.Value.start();
			    instance.Value.release();
			}
			return instance;
		}

		public static EventInstance? Loop(string path)
		{
			EventInstance? instance = CreateInstance(path);
			if (instance != null)
			{
				instance.Value.start();
			}
			return instance;
		}

		public static EventInstance? Loop(string path, string param, float value)
		{
			EventInstance? instance = CreateInstance(path);
			if (instance != null)
			{
				instance.Value.setParameterByName(param, value);
				instance.Value.start();
			}
			return instance;
		}

		public static EventInstance? Loop(string path, Vector2 position)
		{
			EventInstance? instance = CreateInstance(path, position);
			if (instance != null)
			{
				instance.Value.start();
			}
			return instance;
		}

		public static EventInstance? Loop(string path, Vector2 position, string param, float value)
		{
			EventInstance? instance = CreateInstance(path, position);
			if (instance != null)
			{
				instance.Value.setParameterByName(param, value);
				instance.Value.start();
			}
			return instance;
		}

		public static void Pause(EventInstance? instance)
		{
			if (instance != null)
			{
				instance.Value.setPaused(paused: true);
			}
		}

		public static void Resume(EventInstance? instance)
		{
			if (instance != null)
			{
				instance.Value.setPaused(paused: false);
			}
		}

		public static void Position(EventInstance? instance, Vector2 position)
		{
			if (instance != null)
			{
				Vector2 cam = Vector2.Zero;
				if (currentCamera != null)
				{
					cam = currentCamera.Position + new Vector2(320f, 180f) / 2f;
				}
				float px = position.X - cam.X;
				if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
				{
					px = 0f - px;
				}
				attributes3d.position.x = px;
				attributes3d.position.y = position.Y - cam.Y;
				attributes3d.position.z = 0f;
				instance.Value.set3DAttributes(attributes3d);
			}
		}

		public static void SetParameter(EventInstance? instance, string param, float value)
		{
			if (instance != null)
			{
				instance.Value.setParameterByName(param, value);
			}
		}

		public static void Stop(EventInstance? instance, bool allowFadeOut = true)
		{
			if (instance != null)
			{
				instance.Value.stop((!allowFadeOut) ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT);
				instance.Value.release();
			}
		}

		public static EventInstance? CreateInstance(string path, Vector2? position = null)
		{
			EventDescription? desc = GetEventDescription(path);
			if (desc != null)
			{
				desc.Value.createInstance(out var instance);
				desc.Value.is3D(out var is3D);
				if (is3D && position.HasValue)
				{
					Position(instance, position.Value);
				}
				return instance;
			}
			return null;
		}

		public static EventDescription? GetEventDescription(string path)
		{
			EventDescription desc;
			if (path != null && !cachedEventDescriptions.TryGetValue(path, out desc))
			{
				RESULT result = system.Value.getEvent(path, out desc);
				switch (result)
				{
				case RESULT.OK:
					desc.loadSampleData();
					cachedEventDescriptions.Add(path, desc);
					break;
				default:
					throw new Exception("FMOD getEvent failed: " + result);
				case RESULT.ERR_EVENT_NOTFOUND:
					break;
				}
                return desc;
			}
            return null;
		}

		public static void ReleaseUnusedDescriptions()
		{
			List<string> releasing = new List<string>();
			foreach (KeyValuePair<string, EventDescription> kv in cachedEventDescriptions)
			{
				kv.Value.getInstanceCount(out var count);
				if (count <= 0)
				{
					kv.Value.unloadSampleData();
					releasing.Add(kv.Key);
				}
			}
			foreach (string key in releasing)
			{
				cachedEventDescriptions.Remove(key);
			}
		}

		public static string GetEventName(EventInstance? instance)
		{
			if (instance != null)
			{
				instance.Value.getDescription(out var desc);
                string path = "";
                desc.getPath(out path);
                return path;
			}
			return "";
		}

		public static bool IsPlaying(EventInstance? instance)
		{
			if (instance != null)
			{
				instance.Value.getPlaybackState(out var state);
				if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING)
				{
					return true;
				}
			}
			return false;
		}

		public static bool BusPaused(string path, bool? pause = null)
		{
			bool isPaused = false;
			if (system != null && system.Value.getBus(path, out var bus) == RESULT.OK)
			{
				if (pause.HasValue)
				{
					bus.setPaused(pause.Value);
				}
				bus.getPaused(out isPaused);
			}
			return isPaused;
		}

		public static bool BusMuted(string path, bool? mute)
		{
			bool isMuted = false;
			if (system.Value.getBus(path, out var bus) == RESULT.OK)
			{
				if (mute.HasValue)
				{
					bus.setMute(mute.Value);
				}
				bus.getPaused(out isMuted);
			}
			return isMuted;
		}

		public static void BusStopAll(string path, bool immediate = false)
		{
			if (system != null && system.Value.getBus(path, out var bus) == RESULT.OK)
			{
				bus.stopAllEvents(immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT);
			}
		}

		public static float VCAVolume(string path, float? volume = null)
		{
			VCA vca;
			RESULT vCA = system.Value.getVCA(path, out vca);
			float output = 1f;
			float finalvolume = 1f;
			if (vCA == RESULT.OK)
			{
				if (volume.HasValue)
				{
					vca.setVolume(volume.Value);
				}
				vca.getVolume(out output, out finalvolume);
			}
			return output;
		}

		public static EventInstance CreateSnapshot(string name, bool start = true)
		{
			CheckFmod(system.Value.getEvent(name, out var ev));
            /*
			if (ev == null)
			{
				throw new Exception("Snapshot " + name + " doesn't exist");
			}
            */
			ev.createInstance(out var snapshot);
			if (start)
			{
				snapshot.start();
			}
			return snapshot;
		}

		public static void ResumeSnapshot(EventInstance? snapshot)
		{
			if (snapshot != null)
			{
				snapshot.Value.start();
			}
		}

		public static bool IsSnapshotRunning(EventInstance? snapshot)
		{
			if (snapshot != null)
			{
				snapshot.Value.getPlaybackState(out var state);
				if (state != 0 && state != PLAYBACK_STATE.STARTING)
				{
					return state == PLAYBACK_STATE.SUSTAINING;
				}
				return true;
			}
			return false;
		}

		public static void EndSnapshot(EventInstance? snapshot)
		{
			if (snapshot != null)
			{
				snapshot.Value.stop(STOP_MODE.ALLOWFADEOUT);
			}
		}

		public static void ReleaseSnapshot(EventInstance? snapshot)
		{
			if (snapshot != null)
			{
				snapshot.Value.stop(STOP_MODE.ALLOWFADEOUT);
				snapshot.Value.release();
			}
		}

		public static bool SetMusic(string path, bool startPlaying = true, bool allowFadeOut = true)
		{
			if (string.IsNullOrEmpty(path) || path == "null")
			{
				Stop(currentMusicEvent, allowFadeOut);
				currentMusicEvent = null;
				CurrentMusic = "";
			}
			else if (!CurrentMusic.Equals(path, StringComparison.OrdinalIgnoreCase))
			{
				Stop(currentMusicEvent, allowFadeOut);
				EventInstance? instance = CreateInstance(path);
				if (instance != null && startPlaying)
				{
					instance.Value.start();
				}
				currentMusicEvent = instance;
				CurrentMusic = GetEventName(instance);
				return true;
			}
			return false;
		}

		public static bool SetAmbience(string path, bool startPlaying = true)
		{
			if (string.IsNullOrEmpty(path) || path == "null")
			{
				Stop(currentAmbientEvent);
				currentAmbientEvent = null;
			}
			else if (!GetEventName(currentAmbientEvent).Equals(path, StringComparison.OrdinalIgnoreCase))
			{
				Stop(currentAmbientEvent);
				EventInstance? instance = CreateInstance(path);
				if (instance != null && startPlaying)
				{
					instance.Value.start();
				}
				currentAmbientEvent = instance;
				return true;
			}
			return false;
		}

		public static void SetMusicParam(string path, float value)
		{
			if (currentMusicEvent != null)
			{
				currentMusicEvent.Value.setParameterByName(path, value);
			}
		}

		public static void SetAltMusic(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				EndSnapshot(mainDownSnapshot);
				Stop(currentAltMusicEvent);
				currentAltMusicEvent = null;
			}
			else if (!GetEventName(currentAltMusicEvent).Equals(path, StringComparison.OrdinalIgnoreCase))
			{
				StartMainDownSnapshot();
				Stop(currentAltMusicEvent);
				currentAltMusicEvent = Loop(path);
			}
		}

		private static void StartMainDownSnapshot()
		{
			if (mainDownSnapshot == null)
			{
				mainDownSnapshot = CreateSnapshot("snapshot:/music_mains_mute");
			}
			else
			{
				ResumeSnapshot(mainDownSnapshot);
			}
		}

		private static void EndMainDownSnapshot()
		{
			EndSnapshot(mainDownSnapshot);
		}
	}
}
